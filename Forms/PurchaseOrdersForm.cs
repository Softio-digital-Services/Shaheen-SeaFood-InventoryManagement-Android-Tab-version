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

            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(20) };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainLayout);

            Panel pnlHeader = new Panel { Dock = DockStyle.Fill };
            lblPOTitle = ThemeConfig.CreateStandardHeader(LocalizationManager.GetString("PO_Title"));
            lblPOTitle.Name = "lblPOTitle";
            pnlHeader.Controls.Add(lblPOTitle);

            txtSearch = new ModernTextBox();
            txtSearch.IsSearch = true;
            txtSearch.ShowLabel = false;
            txtSearch.PlaceholderText = "Search purchase orders...";
            txtSearch.Size = new Size(320, 40);
            txtSearch.Location = new Point(0, 45);
            txtSearch.TextChanged += (s, e) => LoadPurchaseOrders(txtSearch.Text);
            pnlHeader.Controls.Add(txtSearch);

            FlowLayoutPanel pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Right, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 10, 10, 0) };
            Button btnNewPO = new ModernButton { Text = "+ " + LocalizationManager.GetString("PO_New"), Size = new Size(180, 40) };
            ThemeConfig.ApplyPrimaryButton(btnNewPO);
            btnNewPO.Click += BtnNewPO_Click;
            pnlButtons.Controls.Add(btnNewPO);

            if (UserSession.IsAdmin || UserSession.IsAccountant)
            {
                Button btnAutoPO = new ModernButton { Text = "Predictive Buy-List", Size = new Size(200, 40) };
                ThemeConfig.ApplySecondaryButton(btnAutoPO);
                btnAutoPO.Click += BtnAutoPO_Click;
            pnlButtons.Controls.Add(btnAutoPO);
            }
            pnlHeader.Controls.Add(pnlButtons);

            pnlHeader.Resize += (s, e) =>
            {
                if (LocalizationManager.IsArabic)
                {
                    txtSearch.Location = new Point(pnlHeader.Width - txtSearch.Width, 45);
                    pnlButtons.Location = new Point(0, 10);
                }
                else
                {
                    txtSearch.Location = new Point(0, 45);
                    pnlButtons.Location = new Point(pnlHeader.Width - pnlButtons.Width, 10);
                }
            };


            mainLayout.Controls.Add(pnlHeader, 0, 0);

            pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            dgvPO = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, AutoGenerateColumns = false, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None };
            ThemeConfig.ApplyGridTheme(dgvPO);

            dgvPO.Columns.Add(new DataGridViewTextBoxColumn { Name = "POID", DataPropertyName = "po_id", HeaderText = "PO #", Width = 80 });
            dgvPO.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", DataPropertyName = "order_date", HeaderText = LocalizationManager.GetString("PO_Date"), Width = 150 });
            dgvPO.Columns.Add(new DataGridViewTextBoxColumn { Name = "Supplier", DataPropertyName = "supplier_name", HeaderText = LocalizationManager.GetString("PO_Supplier"), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvPO.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", DataPropertyName = "total_amount", HeaderText = LocalizationManager.GetString("PO_Amount"), Width = 120 });
            dgvPO.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", DataPropertyName = "status", HeaderText = LocalizationManager.GetString("PO_Status"), Width = 100 });
            
            DataGridViewButtonColumn btnAction = new DataGridViewButtonColumn { Name = "colAction", HeaderText = LocalizationManager.GetString("Parts_GridActions"), Text = LocalizationManager.GetString("Return_Action"), UseColumnTextForButtonValue = true, Width = 100, FlatStyle = FlatStyle.Flat };
            dgvPO.Columns.Add(btnAction);

            dgvPO.CellContentClick += DgvPO_CellContentClick;
            dgvPO.CellFormatting += DgvPO_CellFormatting;

            pnlCard = CreateCardPanel(dgvPO);
            pnlContent.Controls.Add(pnlCard);
            mainLayout.Controls.Add(pnlContent, 0, 1);

            this.ResumeLayout(false);
        }

        private Panel CreateCardPanel(Control inner)
        {
            Panel p = new Panel { Dock = DockStyle.Fill, BackColor = ThemeConfig.SurfaceColor, Padding = new Padding(20) };
            p.Controls.Add(inner);
            p.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                using (var path = ThemeConfig.GetRoundedPathPublic(r, 12))
                using (var pen = new Pen(ThemeConfig.BorderColor, 1)) e.Graphics.DrawPath(pen, path);
            };
            return p;
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
            BaseModalForm f = new BaseModalForm { TitleText = LocalizationManager.GetString("PO_New"), Size = new Size(950, 700) };
            Panel p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            f.ContentPanel.Controls.Add(p);

            // 1. Supplier Selection
            Label lblSup = new Label { Text = LocalizationManager.GetString("PO_Supplier") + ":", Location = new Point(0, 0), AutoSize = true, Font = ThemeConfig.SmallBoldFont };
            ComboBox cmbSup = new ComboBox { Location = new Point(0, 25), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            ThemeConfig.ApplyComboBoxStyle(cmbSup);
            DataTable dtSup = DatabaseHelper.ExecuteDataTable("SELECT id, supplier_name FROM suppliers WHERE date_deleted IS NULL");
            cmbSup.DataSource = dtSup; cmbSup.DisplayMember = "supplier_name"; cmbSup.ValueMember = "id";
            p.Controls.Add(lblSup); p.Controls.Add(cmbSup);

            // 2. Part Search Section
            Label lblPart = new Label { Text = "Quick Add Part (Search):", Location = new Point(330, 0), AutoSize = true, Font = ThemeConfig.SmallBoldFont };
            ModernComboBox cmbParts = new ModernComboBox { Location = new Point(330, 25), Width = 400 };
            DataTable dtParts = DatabaseHelper.ExecuteDataTable("SELECT id, part_name, purchase_price FROM parts WHERE date_deleted IS NULL");
            cmbParts.DataSource = dtParts; cmbParts.DisplayMember = "part_name"; cmbParts.ValueMember = "id";
            p.Controls.Add(lblPart); p.Controls.Add(cmbParts);

            Button btnAddRow = new Button { Text = "+ Add to Order", Location = new Point(740, 25), Size = new Size(130, 32) };
            ThemeConfig.ApplyPrimaryButton(btnAddRow);
            p.Controls.Add(btnAddRow);

            // 3. Items Grid
            DataGridView dgvItems = new DataGridView { Location = new Point(0, 80), Size = new Size(900, 420), AllowUserToAddRows = false, BackgroundColor = Color.White };
            ThemeConfig.ApplyGridTheme(dgvItems);
            dgvItems.Columns.Add("PartID", "ID"); dgvItems.Columns["PartID"].ReadOnly = true; dgvItems.Columns["PartID"].Width = 60;
            dgvItems.Columns.Add("PartName", LocalizationManager.GetString("AddPart_Product")); dgvItems.Columns["PartName"].ReadOnly = true; dgvItems.Columns["PartName"].Width = 350;
            dgvItems.Columns.Add("Qty", LocalizationManager.GetString("POS_GridQty")); dgvItems.Columns["Qty"].Width = 100;
            dgvItems.Columns.Add("Cost", LocalizationManager.GetString("POS_GridUnitCost")); dgvItems.Columns["Cost"].Width = 150;
            dgvItems.Columns.Add("Subtotal", "Subtotal"); dgvItems.Columns["Subtotal"].ReadOnly = true; dgvItems.Columns["Subtotal"].Width = 150;
            p.Controls.Add(dgvItems);

            Label lblGrandTotal = new Label { Text = "Grand Total: 0.00", Location = new Point(0, 510), Size = new Size(900, 30), Font = ThemeConfig.HeaderFont, TextAlign = ContentAlignment.TopRight };
            p.Controls.Add(lblGrandTotal);

            // Event: Add Item from search
            btnAddRow.Click += (s, e) => {
                if (cmbParts.SelectedValue == null) return;
                DataRowView drv = cmbParts.SelectedItem as DataRowView;
                decimal cost = drv["purchase_price"] != DBNull.Value ? Convert.ToDecimal(drv["purchase_price"]) : 0;
                dgvItems.Rows.Add(drv["id"], drv["part_name"], 1, cost, cost);
                UpdatePOTotal(dgvItems, lblGrandTotal);
            };

            // Event: Auto-calculate subtotals
            dgvItems.CellValueChanged += (s, e) => {
                if (e.RowIndex < 0) return;
                if (dgvItems.Columns[e.ColumnIndex].Name == "Qty" || dgvItems.Columns[e.ColumnIndex].Name == "Cost") {
                    decimal qty = Convert.ToDecimal(dgvItems.Rows[e.RowIndex].Cells["Qty"].Value ?? 0);
                    decimal cost = Convert.ToDecimal(dgvItems.Rows[e.RowIndex].Cells["Cost"].Value ?? 0);
                    dgvItems.Rows[e.RowIndex].Cells["Subtotal"].Value = qty * cost;
                    UpdatePOTotal(dgvItems, lblGrandTotal);
                }
            };

            // Event: Predictive Link
            if (autoPopulateLowStock) {
                DataTable lowStock = DatabaseHelper.ExecuteDataTable("SELECT id, part_name, (minimum_stock_level - quantity_in_stock + reorder_quantity) as req_qty, purchase_price, supplier_id FROM parts WHERE quantity_in_stock <= minimum_stock_level AND status = 'Active'");
                cmbSup.SelectedIndexChanged += (s, e) => {
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

            Button btnSave = new ModernButton { Text = "Finalize Purchase Order", Location = new Point(650, 560), Size = new Size(250, 45) };
            ThemeConfig.ApplyPrimaryButton(btnSave);
            btnSave.Click += (s, e) => {
                if (cmbSup.SelectedValue == null) return;
                List<PurchaseItemInfo> items = new List<PurchaseItemInfo>();
                foreach (DataGridViewRow row in dgvItems.Rows) {
                    if (row.Cells["PartID"].Value != null)
                        items.Add(new PurchaseItemInfo { PartId = int.Parse(row.Cells["PartID"].Value.ToString()), Quantity = int.Parse(row.Cells["Qty"].Value.ToString()), CostPrice = decimal.Parse(row.Cells["Cost"].Value?.ToString() ?? "0") });
                }
                if (items.Count == 0) return;
                _purchaseService.CreatePurchaseOrder(Convert.ToInt32(cmbSup.SelectedValue), items, "Generated via softio order manager");
                f.DialogResult = DialogResult.OK; f.Close(); LoadPurchaseOrders();
            };
            p.Controls.Add(btnSave);
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
