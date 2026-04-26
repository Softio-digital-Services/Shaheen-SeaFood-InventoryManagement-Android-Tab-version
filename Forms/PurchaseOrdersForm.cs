using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Services;

namespace GenericInventorySystem.Forms
{
    public partial class PurchaseOrdersForm : UserControl
    {
        private DataGridView dgvPO;
        private Panel pnlContent;
        private Panel pnlCard;
        private PurchaseService _purchaseService;
        private Label lblPOTitle;
        private ModernTextBox txtSearch;

        public PurchaseOrdersForm()
        {
            InitializeComponent();
            _purchaseService = new PurchaseService();
            ApplyLocalization();
            LoadPurchaseOrders();
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            if (lblPOTitle != null) lblPOTitle.Text = LocalizationManager.GetString("PO_Title");
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Size = new Size(1100, 750);
            this.BackColor = ThemeConfig.BackgroundColor;

            // Main Layout
            TableLayoutPanel tlpMain = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(20), BackColor = ThemeConfig.BackgroundColor };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // Title
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // Search/Actions
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Content
            this.Controls.Add(tlpMain);
 
            // 0. Title
            lblPOTitle = ThemeConfig.CreateStandardHeader(LocalizationManager.GetString("PO_Title"));
            lblPOTitle.Name = "lblPOTitle";
            lblPOTitle.Margin = new Padding(0);
            tlpMain.Controls.Add(lblPOTitle, 0, 0);

            // 1. Actions Row (Search + Buttons)
            Panel pnlActions = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            
            // Search Bar
            txtSearch = new ModernTextBox();
            txtSearch.IsSearch = true;
            txtSearch.ShowLabel = false;
            txtSearch.PlaceholderText = LocalizationManager.IsArabic ? "البحث في طلبات الشراء..." : "Search purchase orders...";
            txtSearch.Size = new Size(320, 40);
            txtSearch.Location = new Point(0, 5); // Align with buttons vertically
            txtSearch.TextChanged += (s, e) => LoadPurchaseOrders(txtSearch.Text);
            pnlActions.Controls.Add(txtSearch);

            // Buttons Panel
            FlowLayoutPanel panelButtons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Dock = DockStyle.Right,
                WrapContents = false
            };

            // New PO Button
            Button btnNewPO = new Button
            {
                Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeConfig.SurfaceColor,
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            btnNewPO.FlatAppearance.BorderSize = 0;
            btnNewPO.Click += BtnNewPO_Click;
            btnNewPO.Paint += (s, e) => ThemeConfig.DrawIconButton(btnNewPO, e.Graphics, "add", "PO_New", Color.White, ThemeConfig.PrimaryColor, false);
            panelButtons.Controls.Add(btnNewPO);

            // Predictive Buy-List
            if (UserSession.IsAdmin || UserSession.IsAccountant)
            {
                Button btnAutoPO = new Button
                {
                    Size = new Size(200, 40),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ThemeConfig.SurfaceColor,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(10, 0, 0, 0)
                };
                btnAutoPO.FlatAppearance.BorderSize = 0;
                btnAutoPO.Click += BtnAutoPO_Click;
                btnAutoPO.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAutoPO, e.Graphics, "orders", "PO_Predictive", ThemeConfig.PrimaryColor, ThemeConfig.PrimaryColor, true);
                panelButtons.Controls.Add(btnAutoPO);
            }

            pnlActions.Controls.Add(panelButtons);
            tlpMain.Controls.Add(pnlActions, 0, 1);

            // Content
            pnlContent = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 10, 0, 0) };
            dgvPO = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, AutoGenerateColumns = false, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None };
            ThemeConfig.ApplyGridTheme(dgvPO);

            dgvPO.Columns.Add(new DataGridViewTextBoxColumn { Name = "POID", DataPropertyName = "po_id", HeaderText = "PO #", Width = 80 });
            dgvPO.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", DataPropertyName = "order_date", HeaderText = LocalizationManager.GetString("PO_Date"), Width = 150 });
            dgvPO.Columns.Add(new DataGridViewTextBoxColumn { Name = "Supplier", DataPropertyName = "supplier_name", HeaderText = LocalizationManager.GetString("PO_Supplier"), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvPO.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", DataPropertyName = "total_amount", HeaderText = LocalizationManager.GetString("PO_Amount"), Width = 120 });
            dgvPO.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", DataPropertyName = "status", HeaderText = LocalizationManager.GetString("PO_Status"), Width = 120 });
            
            DataGridViewImageColumn colAction = new DataGridViewImageColumn
            {
                Name = "colAction",
                HeaderText = LocalizationManager.GetString("Parts_GridActions"),
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                Width = 140
            };
            dgvPO.Columns.Add(colAction);

            dgvPO.CellContentClick += DgvPO_CellContentClick;
            dgvPO.CellFormatting += DgvPO_CellFormatting;
            dgvPO.CellPainting += DgvPO_CellPainting;

            pnlCard = ThemeConfig.CreateCardPanel(dgvPO);
            pnlContent.Controls.Add(pnlCard);
            tlpMain.Controls.Add(pnlContent, 0, 2);

            this.ResumeLayout(false);
        }

        private void DgvPO_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvPO.Columns[e.ColumnIndex].Name == "colAction")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

                string status = dgvPO.Rows[e.RowIndex].Cells["Status"].Value?.ToString();
                bool isReceived = status == "Received";

                // We only show the "Receive" icon if it's NOT received
                if (!isReceived)
                {
                    Image receiveIcon = ThemeConfig.GetNuricon("delivery");
                    if (receiveIcon != null)
                    {
                        int iconSize = 24;
                        int x = e.CellBounds.X + (e.CellBounds.Width - iconSize) / 2;
                        int y = e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2;

                        using (Image tinted = ThemeConfig.TintImage(receiveIcon, ThemeConfig.PrimaryColor))
                        {
                            e.Graphics.DrawImage(tinted, new Rectangle(x, y, iconSize, iconSize));
                        }
                    }
                }
                else
                {
                    // Draw a checkmark or nothing
                    Image checkIcon = ThemeConfig.GetNuricon("check");
                    if (checkIcon != null)
                    {
                        int iconSize = 24;
                        int x = e.CellBounds.X + (e.CellBounds.Width - iconSize) / 2;
                        int y = e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2;
                        using (Image tinted = ThemeConfig.TintImage(checkIcon, ThemeConfig.SuccessColor))
                        {
                            e.Graphics.DrawImage(tinted, new Rectangle(x, y, iconSize, iconSize));
                        }
                    }
                }

                e.Handled = true;
            }
        }


        private void LoadPurchaseOrders(string search = "")
        {
            DataTable dt = _purchaseService.GetPurchaseOrders();
            if (!string.IsNullOrEmpty(search))
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = string.Format("supplier_name LIKE '%{0}%' OR po_id = {1}", search, int.TryParse(search, out int id) ? id : -1);
                dt = dv.ToTable();
            }
            dgvPO.DataSource = dt;
        }

        private void DgvPO_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvPO.Columns[e.ColumnIndex].Name == "Status")
            {
                if (e.Value?.ToString() == "Received") e.CellStyle.ForeColor = ThemeConfig.SuccessColor;
                else e.CellStyle.ForeColor = ThemeConfig.WarningColor;
            }
            if (e.RowIndex >= 0 && dgvPO.Columns[e.ColumnIndex].Name == "Total" && e.Value != null)
            {
                e.Value = CurrencyService.Format(Convert.ToDecimal(e.Value));
                e.FormattingApplied = true;
            }
        }

        private void DgvPO_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvPO.Columns[e.ColumnIndex].Name == "colAction")
            {
                int poId = (int)dgvPO.Rows[e.RowIndex].Cells["POID"].Value;
                string status = dgvPO.Rows[e.RowIndex].Cells["Status"].Value.ToString();

                if (status == "Received")
                {
                    MessageHelper.ShowInfo(LocalizationManager.GetString("PO_AlreadyReceived") ?? "This order has already been received.");
                    return;
                }

                if (MessageHelper.ConfirmAction(LocalizationManager.GetString("PO_ConfirmReceive")))
                {
                    try {
                        _purchaseService.MarkAsReceived(poId);
                        MessageHelper.ShowSuccess("Stock updated successfully.");
                        LoadPurchaseOrders();
                    } catch(Exception ex) { MessageHelper.ShowError(ex.Message); }
                }
            }
        }

        private void BtnNewPO_Click(object sender, EventArgs e)
        {
            ShowNewPODialog();
        }

        private void ShowNewPODialog(bool autoPopulateLowStock = false)
        {
            BaseModalForm f = new BaseModalForm { TitleText = LocalizationManager.GetString("PO_New"), Size = new Size(1050, 750) };
            
            // Root Container
            TableLayoutPanel tlpRoot = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20)
            };
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));  // Header
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F)); // Footer
            f.ContentPanel.Controls.Add(tlpRoot);

            // --- HEADER SECTION ---
            TableLayoutPanel tlpHeader = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0)
            };
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            // 1. Supplier
            ModernComboBox cmbSup = new ModernComboBox { 
                Dock = DockStyle.Fill, 
                LabelText = LocalizationManager.GetString("PO_Supplier") + ":",
                Margin = new Padding(0, 0, 10, 0)
            };
            DataTable dtSup = DatabaseHelper.ExecuteDataTable("SELECT id, supplier_name FROM suppliers WHERE date_deleted IS NULL");
            cmbSup.DataSource = dtSup; cmbSup.DisplayMember = "supplier_name"; cmbSup.ValueMember = "id";
            tlpHeader.Controls.Add(cmbSup, 0, 0);

            // 2. Part Search
            ModernComboBox cmbParts = new ModernComboBox { 
                Dock = DockStyle.Fill, 
                LabelText = (LocalizationManager.IsArabic ? "إضافة سريعة (بحث):" : "Quick Add Part (Search):"),
                Margin = new Padding(0, 0, 10, 0)
            };
            DataTable dtParts = DatabaseHelper.ExecuteDataTable("SELECT id, part_name, purchase_price FROM parts WHERE date_deleted IS NULL");
            cmbParts.DataSource = dtParts; cmbParts.DisplayMember = "part_name"; cmbParts.ValueMember = "id";
            tlpHeader.Controls.Add(cmbParts, 1, 0);

            // 3. Add Button
            Button btnAddRow = new Button { 
                Text = "+ " + (LocalizationManager.IsArabic ? "أضف للطلب" : "Add to Order"), 
                Height = 42,
                Dock = DockStyle.Bottom,
                Margin = new Padding(0, 0, 0, 3) // Align with input bottoms
            };
            ThemeConfig.ApplyPrimaryButton(btnAddRow);
            tlpHeader.Controls.Add(btnAddRow, 2, 0);

            tlpRoot.Controls.Add(tlpHeader, 0, 0);

            // --- GRID SECTION ---
            DataGridView dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, BackgroundColor = Color.White };
            ThemeConfig.ApplyGridTheme(dgvItems);
            dgvItems.Columns.Add("PartID", "ID"); dgvItems.Columns["PartID"].ReadOnly = true; dgvItems.Columns["PartID"].Width = 60;
            dgvItems.Columns.Add("PartName", LocalizationManager.GetString("AddPart_Product")); dgvItems.Columns["PartName"].ReadOnly = true; dgvItems.Columns["PartName"].Width = 350;
            dgvItems.Columns.Add("Qty", LocalizationManager.GetString("POS_GridQty")); dgvItems.Columns["Qty"].Width = 100;
            dgvItems.Columns.Add("Cost", LocalizationManager.GetString("POS_GridUnitCost")); dgvItems.Columns["Cost"].Width = 150;
            dgvItems.Columns.Add("Subtotal", "Subtotal"); dgvItems.Columns["Subtotal"].ReadOnly = true; dgvItems.Columns["Subtotal"].Width = 150;
            tlpRoot.Controls.Add(dgvItems, 0, 1);

            // --- FOOTER SECTION ---
            TableLayoutPanel tlpFooter = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 10, 0, 0)
            };
            tlpFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            FlowLayoutPanel flpTotals = new FlowLayoutPanel {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                WrapContents = false
            };

            Label lblGrandTotal = new Label { 
                Text = "Grand Total: 0.00", 
                AutoSize = true, 
                Font = ThemeConfig.HeaderFont, 
                ForeColor = ThemeConfig.TextColorDark,
                Margin = new Padding(0, 0, 0, 10),
                Anchor = AnchorStyles.Right
            };

            Button btnSave = new ModernButton { Text = (LocalizationManager.IsArabic ? "إتمام طلب الشراء" : "Finalize Purchase Order"), Size = new Size(250, 45), Anchor = AnchorStyles.Right };
            ThemeConfig.ApplyPrimaryButton(btnSave);
            
            flpTotals.Controls.Add(lblGrandTotal);
            flpTotals.Controls.Add(btnSave);
            tlpFooter.Controls.Add(flpTotals, 1, 0);
            tlpRoot.Controls.Add(tlpFooter, 0, 2);

            // --- EVENTS ---
            btnAddRow.Click += (s, e) => {
                if (cmbParts.SelectedValue == null) return;
                DataRowView drv = cmbParts.SelectedItem as DataRowView;
                decimal cost = drv["purchase_price"] != DBNull.Value ? Convert.ToDecimal(drv["purchase_price"]) : 0;
                
                bool found = false;
                foreach(DataGridViewRow row in dgvItems.Rows) {
                    if (row.Cells["PartID"].Value?.ToString() == drv["id"].ToString()) {
                        row.Cells["Qty"].Value = Convert.ToInt32(row.Cells["Qty"].Value ?? 1) + 1;
                        found = true; break;
                    }
                }
                if (!found) dgvItems.Rows.Add(drv["id"], drv["part_name"], 1, cost, cost);
                UpdatePOTotal(dgvItems, lblGrandTotal);
            };

            dgvItems.CellValueChanged += (s, e) => {
                if (e.RowIndex < 0) return;
                if (dgvItems.Columns[e.ColumnIndex].Name == "Qty" || dgvItems.Columns[e.ColumnIndex].Name == "Cost") {
                    decimal qty = 0; decimal.TryParse(dgvItems.Rows[e.RowIndex].Cells["Qty"].Value?.ToString(), out qty);
                    decimal cost = 0; decimal.TryParse(dgvItems.Rows[e.RowIndex].Cells["Cost"].Value?.ToString(), out cost);
                    dgvItems.Rows[e.RowIndex].Cells["Subtotal"].Value = qty * cost;
                    UpdatePOTotal(dgvItems, lblGrandTotal);
                }
            };

            if (autoPopulateLowStock) {
                DataTable lowStock = DatabaseHelper.ExecuteDataTable("SELECT id, part_name, (minimum_stock_level - quantity_in_stock + reorder_quantity) as req_qty, purchase_price, supplier_id FROM parts WHERE quantity_in_stock <= minimum_stock_level AND status = 'Active'");
                cmbSup.InnerComboBox.SelectedIndexChanged += (s, e) => {
                    dgvItems.Rows.Clear();
                    if (cmbSup.SelectedValue != null && int.TryParse(cmbSup.SelectedValue.ToString(), out int supId)) {
                        foreach (DataRow r in lowStock.Rows) {
                            if (r["supplier_id"] != DBNull.Value && Convert.ToInt32(r["supplier_id"]) == supId) {
                                dgvItems.Rows.Add(r["id"], r["part_name"], r["req_qty"], r["purchase_price"], Convert.ToDecimal(r["req_qty"]) * Convert.ToDecimal(r["purchase_price"]));
                            }
                        }
                        UpdatePOTotal(dgvItems, lblGrandTotal);
                    }
                };
            }

            btnSave.Click += (s, e) => {
                if (cmbSup.SelectedValue == null) {
                    MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "يرجى اختيار مورد." : "Please select a supplier.");
                    return;
                }
                List<PurchaseItemInfo> items = new List<PurchaseItemInfo>();
                foreach (DataGridViewRow row in dgvItems.Rows) {
                    if (row.Cells["PartID"].Value != null)
                        items.Add(new PurchaseItemInfo { 
                            PartId = int.Parse(row.Cells["PartID"].Value.ToString()), 
                            Quantity = int.Parse(row.Cells["Qty"].Value.ToString()), 
                            CostPrice = decimal.Parse(row.Cells["Cost"].Value?.ToString() ?? "0") 
                        });
                }
                if (items.Count == 0) {
                    MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "يرجى إضافة أصناف للطلب." : "Please add items to the order.");
                    return;
                }
                _purchaseService.CreatePurchaseOrder(Convert.ToInt32(cmbSup.SelectedValue), items, "Manual PO Creation");
                f.DialogResult = DialogResult.OK; f.Close(); LoadPurchaseOrders();
            };

            f.ShowDialog();
        }

        private void UpdatePOTotal(DataGridView dgv, Label lbl) {
            decimal total = 0;
            foreach (DataGridViewRow row in dgv.Rows) total += Convert.ToDecimal(row.Cells["Subtotal"].Value ?? 0);
            lbl.Text = $"Grand Total: {CurrencyService.Format(total)}";
        }

        private void BtnAutoPO_Click(object sender, EventArgs e)
        {
            ShowNewPODialog(true);
        }
    }
}
