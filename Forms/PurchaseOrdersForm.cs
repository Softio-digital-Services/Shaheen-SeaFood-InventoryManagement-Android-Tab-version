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

            Button btnNewPO = new ModernButton { Text = "+ " + LocalizationManager.GetString("PO_New"), Size = new Size(200, 40), Location = new Point(800, 10), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            ThemeConfig.ApplyPrimaryButton(btnNewPO);
            btnNewPO.Click += BtnNewPO_Click;
            pnlHeader.Controls.Add(btnNewPO);

            if (UserSession.IsAdmin || UserSession.IsAccountant)
            {
                Button btnAutoPO = new ModernButton { Text = "Predictive Buy-List", Size = new Size(200, 40), Location = new Point(580, 10), Anchor = AnchorStyles.Top | AnchorStyles.Right };
                ThemeConfig.ApplySecondaryButton(btnAutoPO);
                btnAutoPO.Click += BtnAutoPO_Click;
                pnlHeader.Controls.Add(btnAutoPO);
            }


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

        private void LoadPurchaseOrders()
        {
            dgvPO.DataSource = _purchaseService.GetPurchaseOrders();
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
            // Simplified New PO logic for now
            BaseModalForm f = new BaseModalForm { TitleText = LocalizationManager.GetString("PO_New"), Size = new Size(800, 600) };
            
            // Add Supplier Selector, Item Grid, etc.
            // For brevity, I'll implement a basic one here
            Panel p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            f.ContentPanel.Controls.Add(p);

            Label lblSup = new Label { Text = LocalizationManager.GetString("PO_Supplier") + ":", Location = new Point(0, 0), AutoSize = true };
            ComboBox cmbSup = new ComboBox { Location = new Point(0, 25), Width = 300 };
            ThemeConfig.ApplyComboBoxStyle(cmbSup);
            DataTable dtSup = DatabaseHelper.ExecuteDataTable("SELECT id, supplier_name FROM suppliers WHERE date_deleted IS NULL");
            cmbSup.DataSource = dtSup; cmbSup.DisplayMember = "supplier_name"; cmbSup.ValueMember = "id";
            p.Controls.Add(lblSup); p.Controls.Add(cmbSup);

            DataGridView dgvItems = new DataGridView { Location = new Point(0, 70), Size = new Size(740, 350), AllowUserToAddRows = true, BackgroundColor = Color.White };
            ThemeConfig.ApplyGridTheme(dgvItems);
            dgvItems.Columns.Add("PartID", "Part ID");
            dgvItems.Columns.Add("PartName", LocalizationManager.GetString("AddPart_Product"));
            dgvItems.Columns.Add("Qty", LocalizationManager.GetString("POS_GridQty"));
            dgvItems.Columns.Add("Cost", LocalizationManager.GetString("POS_GridUnitCost"));
            p.Controls.Add(dgvItems);

            if (autoPopulateLowStock)
            {
                DataTable lowStock = DatabaseHelper.ExecuteDataTable("SELECT id, part_name, (minimum_stock_level - quantity_in_stock + reorder_quantity) as req_qty, purchase_price, supplier_id FROM parts WHERE quantity_in_stock <= minimum_stock_level AND status = 'Active'");
                cmbSup.SelectedIndexChanged += (s, e) => {
                    dgvItems.Rows.Clear();
                    if (cmbSup.SelectedValue != null) {
                        int supId = Convert.ToInt32(cmbSup.SelectedValue);
                        foreach (DataRow r in lowStock.Rows) {
                            if (r["supplier_id"] != DBNull.Value && (int)r["supplier_id"] == supId) {
                                dgvItems.Rows.Add(r["id"], r["part_name"], r["req_qty"], r["purchase_price"]);
                            }
                        }
                    }
                };
            }

            Button btnSave = new ModernButton { Text = "Create PO", Location = new Point(620, 440), Size = new Size(120, 40) };
            ThemeConfig.ApplyPrimaryButton(btnSave);
            btnSave.Click += (s, e) => {
                if (cmbSup.SelectedValue == null) return;
                List<PurchaseItemInfo> items = new List<PurchaseItemInfo>();
                foreach (DataGridViewRow row in dgvItems.Rows) {
                    if (row.IsNewRow) continue;
                    if (row.Cells["PartID"].Value != null && row.Cells["Qty"].Value != null)
                        items.Add(new PurchaseItemInfo { PartId = int.Parse(row.Cells["PartID"].Value.ToString()), Quantity = int.Parse(row.Cells["Qty"].Value.ToString()), CostPrice = decimal.Parse(row.Cells["Cost"].Value?.ToString() ?? "0") });
                }
                if (items.Count == 0) return;
                _purchaseService.CreatePurchaseOrder(Convert.ToInt32(cmbSup.SelectedValue), items, "");
                f.DialogResult = DialogResult.OK; f.Close(); LoadPurchaseOrders();
            };
            p.Controls.Add(btnSave);
            
            f.ShowDialog();
        }

        private void BtnAutoPO_Click(object sender, EventArgs e)
        {
            ShowNewPODialog(true);
        }
    }
}
