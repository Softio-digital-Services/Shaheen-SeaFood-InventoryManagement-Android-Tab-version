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

            // Root container with padding
            Panel pnlRoot = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = ThemeConfig.BackgroundColor };
            this.Controls.Add(pnlRoot);

            // Header
            lblPOTitle = ThemeConfig.CreateStandardHeader(LocalizationManager.GetString("PO_Title"));
            lblPOTitle.Name = "lblPOTitle";

            txtSearch = new ModernTextBox();
            txtSearch.IsSearch = true;
            txtSearch.ShowLabel = false;
            txtSearch.PlaceholderText = LocalizationManager.GetString("Msg_SearchPO");
            txtSearch.Size = new Size(320, 40);
            txtSearch.TextChanged += (s, e) => LoadPurchaseOrders(txtSearch.Text);

            Button btnNewPO = new Button { Size = new Size(180, 40), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnNewPO.FlatAppearance.BorderSize = 0;
            btnNewPO.Click += BtnNewPO_Click;
            btnNewPO.Paint += (s, e) => ThemeConfig.DrawIconButton(btnNewPO, e.Graphics, "add", "PO_New", Color.White, ThemeConfig.PrimaryColor, false);

            List<Control> buttons = new List<Control> { btnNewPO };

            if (UserSession.IsAdmin || UserSession.IsAccountant)
            {
                Button btnAutoPO = new Button { Size = new Size(200, 40), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                btnAutoPO.FlatAppearance.BorderSize = 0;
                btnAutoPO.Click += BtnAutoPO_Click;
                btnAutoPO.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAutoPO, e.Graphics, "orders", "PO_Predictive", ThemeConfig.PrimaryColor, ThemeConfig.PrimaryColor, true);
                buttons.Add(btnAutoPO);
            }

            TableLayoutPanel tlpHeader = ThemeConfig.CreateGlobalFormHeader(lblPOTitle, txtSearch, buttons.ToArray());

            // Body — DockStyle.Fill takes all space below the header
            pnlContent = new Panel { Dock = DockStyle.Fill };
            dgvPO = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, AutoGenerateColumns = false, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None };
            ThemeConfig.ApplyGridTheme(dgvPO);

            dgvPO.Columns.Add(new DataGridViewTextBoxColumn { Name = "POID", DataPropertyName = "po_id", HeaderText = LocalizationManager.GetString("PO_GridNumber"), Width = 80 });
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

            // IMPORTANT: WinForms docks in reverse Z-order. Add Fill first, then Top controls bottom-to-top.
            pnlRoot.Controls.Add(pnlContent);   // Fill — added first
            pnlRoot.Controls.Add(tlpHeader);    // Top — docked last = appears at top

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
                        MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_StockUpdated"));
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
            string title = autoPopulateLowStock ? (LocalizationManager.GetString("Msg_PredictivePO")) : LocalizationManager.GetString("PO_New");
            BaseModalForm f = new BaseModalForm { TitleText = title, Size = new Size(1100, 700) }; // Decreased height to remove whitespace
            
            // Root Container
            TableLayoutPanel tlpRoot = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(25, 25, 25, 15)
            };
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 85F));  // Header (Increased to 85 to prevent any clipping)
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F)); // Footer
            f.ContentPanel.Controls.Add(tlpRoot);

            // --- HEADER SECTION ---
            TableLayoutPanel tlpHeader = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0)
            };
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F)); // Supplier
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380F)); // Part Search
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Spacer
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F)); // Add Button

            // 1. Supplier
            ModernComboBox cmbSup = new ModernComboBox { 
                Height = 75, // Increased to ensure no clipping
                Dock = DockStyle.Bottom, 
                LabelText = LocalizationManager.GetString("PO_Supplier") + ":",
                Margin = new Padding(0, 0, 10, 5), // Bottom margin to prevent clipping
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            DataTable dtSup = DatabaseHelper.ExecuteDataTable("SELECT id, supplier_name FROM suppliers WHERE date_deleted IS NULL");
            cmbSup.DataSource = dtSup; cmbSup.DisplayMember = "supplier_name"; cmbSup.ValueMember = "id";
            tlpHeader.Controls.Add(cmbSup, 0, 0);

            // 2. Part Search
            ModernComboBox cmbParts = new ModernComboBox { 
                Height = 75, // Increased to ensure no clipping
                Dock = DockStyle.Bottom, 
                LabelText = LocalizationManager.GetString("PO_QuickAdd"),
                Margin = new Padding(0, 0, 10, 5), // Bottom margin to prevent clipping
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            DataTable dtParts = DatabaseHelper.ExecuteDataTable("SELECT id, part_name, purchase_price FROM parts WHERE date_deleted IS NULL");
            cmbParts.DataSource = dtParts; cmbParts.DisplayMember = "part_name"; cmbParts.ValueMember = "id";
            tlpHeader.Controls.Add(cmbParts, 1, 0);

            // 3. Add Button (Right Aligned & Level)
            Button btnAddRow = new Button { 
                Text = "", 
                Size = new Size(180, 42),
                Dock = DockStyle.Bottom,
                Margin = new Padding(0, 0, 0, 5), // Match dropdown bottom margin
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddRow.FlatAppearance.BorderSize = 0;
            btnAddRow.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAddRow, e.Graphics, "add", "PO_AddItem", Color.White, ThemeConfig.PrimaryColor, false);
            tlpHeader.Controls.Add(btnAddRow, 3, 0); 

            tlpRoot.Controls.Add(tlpHeader, 0, 0);

            // --- GRID SECTION ---
            DataGridView dgvItems = new DataGridView { 
                Dock = DockStyle.Fill, 
                AllowUserToAddRows = false, 
                BackgroundColor = Color.White,
                Margin = new Padding(0, 20, 0, 0) 
            };
            ThemeConfig.ApplyGridTheme(dgvItems);
            dgvItems.Columns.Add("PartID", "ID"); dgvItems.Columns["PartID"].ReadOnly = true; dgvItems.Columns["PartID"].Width = 60;
            dgvItems.Columns.Add("PartName", LocalizationManager.GetString("AddPart_Product")); dgvItems.Columns["PartName"].ReadOnly = true; dgvItems.Columns["PartName"].Width = 350;
            dgvItems.Columns.Add("Qty", LocalizationManager.GetString("POS_GridQty")); dgvItems.Columns["Qty"].Width = 100;
            dgvItems.Columns.Add("Cost", LocalizationManager.GetString("POS_GridUnitCost")); dgvItems.Columns["Cost"].Width = 150;
            dgvItems.Columns.Add("Subtotal", LocalizationManager.GetString("PO_Subtotal")); dgvItems.Columns["Subtotal"].ReadOnly = true; dgvItems.Columns["Subtotal"].Width = 150;
            tlpRoot.Controls.Add(dgvItems, 0, 1); // RESTORED

            // --- FOOTER SECTION ---
            Panel pnlFooter = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            
            Label lblGrandTotal = new Label { 
                Text = LocalizationManager.GetString("PO_GrandTotal") + " " + CurrencyService.Format(0), 
                AutoSize = true, 
                Font = ThemeConfig.HeaderFont, 
                ForeColor = ThemeConfig.TextColorDark,
                TextAlign = ContentAlignment.MiddleRight // Ensure right alignment
            };
            pnlFooter.Controls.Add(lblGrandTotal);

            Button btnSave = new ModernButton { 
                Text = (LocalizationManager.GetString("Msg_FinalizePO")), 
                Size = new Size(280, 45), // Increased width to prevent text clipping
            };
            ThemeConfig.ApplyPrimaryButton(btnSave);
            pnlFooter.Controls.Add(btnSave);
            
            pnlFooter.Resize += (s, e) => {
                lblGrandTotal.Location = new Point(pnlFooter.Width - lblGrandTotal.Width - 5, 10); // Moved down (10) and slight right margin
                btnSave.Location = new Point(pnlFooter.Width - btnSave.Width, 52); // Moved lower (52)
            };
            
            tlpRoot.Controls.Add(pnlFooter, 0, 2);

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

                // Auto-select first supplier with low stock
                if (lowStock.Rows.Count > 0)
                {
                    var firstSupplierId = lowStock.Rows[0]["supplier_id"];
                    if (firstSupplierId != DBNull.Value)
                    {
                        cmbSup.SelectedValue = firstSupplierId;
                    }
                }
            }

            btnSave.Click += (s, e) => {
                if (cmbSup.SelectedValue == null) {
                    MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_SelectSupplier"));
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
                    MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_AddItemsFirst"));
                    return;
                }
                _purchaseService.CreatePurchaseOrder(Convert.ToInt32(cmbSup.SelectedValue), items, "Manual PO Creation");
                f.DialogResult = DialogResult.OK; f.Close(); LoadPurchaseOrders();
            };

            f.Shown += (s, e) => {
                f.ActiveControl = null;
                dgvItems.Focus();
                cmbSup.InnerComboBox.Select(0, 0);
                cmbParts.InnerComboBox.Select(0, 0);
            };

            cmbSup.InnerComboBox.SelectedIndexChanged += (s, e) => {
                if (cmbSup.InnerComboBox.IsHandleCreated) {
                    cmbSup.InnerComboBox.BeginInvoke(new Action(() => {
                        cmbSup.InnerComboBox.Select(0, 0);
                        dgvItems.Focus();
                    }));
                }
            };

            cmbParts.InnerComboBox.SelectedIndexChanged += (s, e) => {
                if (cmbParts.InnerComboBox.IsHandleCreated) {
                    cmbParts.InnerComboBox.BeginInvoke(new Action(() => {
                        cmbParts.InnerComboBox.Select(0, 0);
                        dgvItems.Focus();
                    }));
                }
            };

            f.ShowDialog();
        }

        private void UpdatePOTotal(DataGridView dgv, Label lbl) {
            decimal total = 0;
            foreach (DataGridViewRow row in dgv.Rows) total += Convert.ToDecimal(row.Cells["Subtotal"].Value ?? 0);
            lbl.Text = LocalizationManager.GetString("PO_GrandTotal") + " " + CurrencyService.Format(total);
        }

        private void BtnAutoPO_Click(object sender, EventArgs e)
        {
            ShowNewPODialog(true);
        }
    }
}
