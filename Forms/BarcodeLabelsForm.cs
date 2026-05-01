using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Data;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Services;

namespace GenericInventorySystem.Forms
{
    public partial class BarcodeLabelsForm : UserControl
    {
        private DataGridView dgvItems;
        private ModernTextBox txtSearch;
        private ModernButton btnGenerate;
        private Label lblTitle;
        private DataTable _dtItems;
        private System.Windows.Forms.Timer _searchTimer;
        private CheckBox _headerCheckBox;

        public BarcodeLabelsForm()
        {
            InitializeComponent();
            SetupSearchTimer();
            ApplyTheme();
            ApplyLocalization();
        }

        private void SetupSearchTimer()
        {
            _searchTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                PerformSearch();
            };
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeConfig.BackgroundColor;
            if (lblTitle != null) { lblTitle.Font = ThemeConfig.HeaderFont; lblTitle.ForeColor = ThemeConfig.PrimaryColor; }
            ThemeConfig.ApplyGridTheme(dgvItems);
            ThemeConfig.ApplyPrimaryButton(btnGenerate);
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            bool isAr = LocalizationManager.IsArabic;
            
            lblTitle.Text = isAr ? "Ø·Ø¨Ø§Ø¹Ø© Ù…Ù„ØµÙ‚Ø§Øª Ø§Ù„Ø¨Ø§Ø±ÙƒÙˆØ¯" : "Barcode Labels Generation";
            txtSearch.PlaceholderText = isAr ? "Ø§Ø¨Ø­Ø« Ø¹Ù† Ø§Ù„Ù…Ù†ØªØ¬Ø§Øª..." : "Search items...";
            btnGenerate.Text = isAr ? "\u2728 \u0645\u0639\u0627\u064A\u0646\u0623 \u0648\u0637\u0628\u0627\u0639\u0629" : "\u2728 Preview and Print";

            if (dgvItems.Columns.Count > 0)
            {
                dgvItems.Columns["colSelect"].HeaderText = ""; // Empty because we have a checkbox
                dgvItems.Columns["colName"].HeaderText = isAr ? "Ø§Ù„Ø§Ø³Ù…" : "Product Name";
                dgvItems.Columns["colSku"].HeaderText = isAr ? "Ø§Ù„Ø±Ù…Ø² (SKU)" : "SKU";
                dgvItems.Columns["colBarcode"].HeaderText = isAr ? "Ø§Ù„Ø¨Ø§Ø±ÙƒÙˆØ¯" : "Barcode";
                dgvItems.Columns["colQty"].HeaderText = isAr ? "Ø§Ù„ÙƒÙ…ÙŠØ©" : "Qty";
                dgvItems.Columns["colMinus"].HeaderText = "";
                dgvItems.Columns["colPlus"].HeaderText = "";
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible && !this.DesignMode) LoadData();
        }

        private void LoadData()
        {
            try
            {
                string sql = "SELECT id, part_name, part_number FROM parts WHERE date_deleted IS NULL ORDER BY part_name";
                _dtItems = DatabaseHelper.ExecuteDataTable(sql);
                DisplayData(_dtItems);
                if (_headerCheckBox != null) _headerCheckBox.Checked = false;
            }
            catch (Exception ex) { ErrorLogger.LogError(ex, "loading barcode items"); }
        }

        private void DisplayData(DataTable dt)
        {
            dgvItems.Rows.Clear();
            if (dt == null) return;

            foreach (DataRow r in dt.Rows)
            {
                int rowIndex = dgvItems.Rows.Add();
                var row = dgvItems.Rows[rowIndex];
                row.Cells["colId"].Value = r["id"];
                row.Cells["colName"].Value = r["part_name"];
                row.Cells["colSku"].Value = r["part_number"];
                row.Cells["colQty"].Value = 1; 
                row.Cells["colSelect"].Value = false;
                row.Cells["colMinus"].Value = "-";
                row.Cells["colPlus"].Value = "+";
                
                // Render barcode preview
                BarcodeService bs = new BarcodeService();
                row.Cells["colBarcode"].Value = bs.RenderCode128(r["part_number"]?.ToString() ?? "", 120, 30);
            }
        }

        private void PerformSearch()
        {
            string term = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(term)) { DisplayData(_dtItems); return; }
            if (_dtItems == null) return;

            DataTable filtered = _dtItems.Clone();
            var rows = _dtItems.AsEnumerable().Where(r => 
                (r["part_name"]?.ToString().ToLower().Contains(term) ?? false) ||
                (r["part_number"]?.ToString().ToLower().Contains(term) ?? false)
            );

            foreach (var row in rows) filtered.ImportRow(row);
            DisplayData(filtered);
            if (_headerCheckBox != null) _headerCheckBox.Checked = false;
        }

        private void InitializeComponent()
        {
            this.dgvItems = new DataGridView();
            this.txtSearch = new ModernTextBox();
            this.btnGenerate = new ModernButton();
            this.lblTitle = new Label();
            
            TableLayoutPanel tlpMain = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = ThemeConfig.BackgroundColor, Padding = new Padding(20) };
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Panel pnlTop = new Panel { Dock = DockStyle.Fill };
            lblTitle = ThemeConfig.CreateStandardHeader("Barcode Labels");
            pnlTop.Controls.Add(lblTitle);

            txtSearch.IsSearch = true;
            txtSearch.ShowLabel = false;
            txtSearch.Size = new Size(350, 40);
            txtSearch.Location = new Point(0, 50);
            txtSearch.TextChanged += (s, e) => { _searchTimer.Stop(); _searchTimer.Start(); };
            pnlTop.Controls.Add(txtSearch);

            btnGenerate.Size = new Size(180, 40);
            btnGenerate.Location = new Point(360, 50);
            btnGenerate.Click += btnGenerate_Click;
            pnlTop.Controls.Add(btnGenerate);

            tlpMain.Controls.Add(pnlTop, 0, 0);

            dgvItems = new DataGridView { 
                Dock = DockStyle.Fill, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.CellSelect, 
                BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None,
                RowTemplate = { Height = 40 }, RowHeadersVisible = false
            };
            
            dgvItems.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colSelect", Width = 50 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", Width = 200, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSku", Width = 120, ReadOnly = false });
            dgvItems.Columns.Add(new DataGridViewImageColumn { Name = "colBarcode", Width = 140, ImageLayout = DataGridViewImageCellLayout.Zoom, ReadOnly = true });
            
            dgvItems.Columns.Add(new DataGridViewButtonColumn { Name = "colMinus", Width = 25, FlatStyle = FlatStyle.Flat });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQty", Width = 40, ReadOnly = false });
            dgvItems.Columns.Add(new DataGridViewButtonColumn { Name = "colPlus", Width = 25, FlatStyle = FlatStyle.Flat });

            dgvItems.CellContentClick += DgvItems_CellContentClick;
            dgvItems.CellPainting += DgvItems_CellPainting;

            ThemeConfig.ApplyHeaderCheckBox(dgvItems, "colSelect");

            Panel pnlGrid = ThemeConfig.CreateCardPanel(dgvItems);
            tlpMain.Controls.Add(pnlGrid, 0, 1);

            this.Controls.Add(tlpMain);
        }

        private void DgvItems_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {

            if (e.RowIndex < 0) return;

            // Paint Minus/Plus buttons
            if (dgvItems.Columns[e.ColumnIndex].Name == "colMinus" || dgvItems.Columns[e.ColumnIndex].Name == "colPlus")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                e.Handled = true;
                
                using (var pen = new Pen(ThemeConfig.PrimaryColor, 1.5f))
                {
                    int midX = e.CellBounds.X + e.CellBounds.Width / 2;
                    int midY = e.CellBounds.Y + e.CellBounds.Height / 2;
                    int size = 5;
                    
                    if (dgvItems.Columns[e.ColumnIndex].Name == "colMinus")
                    {
                        e.Graphics.DrawLine(pen, midX - size, midY, midX + size, midY);
                    }
                    else
                    {
                        e.Graphics.DrawLine(pen, midX - size, midY, midX + size, midY);
                        e.Graphics.DrawLine(pen, midX, midY - size, midX, midY + size);
                    }
                }
            }
        }

        private void DgvItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            
            var row = dgvItems.Rows[e.RowIndex];
            int currentQty = Convert.ToInt32(row.Cells["colQty"].Value ?? 1);

            if (dgvItems.Columns[e.ColumnIndex].Name == "colMinus")
            {
                if (currentQty > 1) row.Cells["colQty"].Value = currentQty - 1;
            }
            else if (dgvItems.Columns[e.ColumnIndex].Name == "colPlus")
            {
                row.Cells["colQty"].Value = currentQty + 1;
            }
            else if (dgvItems.Columns[e.ColumnIndex].Name == "colSelect")
            {
                dgvItems.EndEdit();
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            dgvItems.EndEdit();
            var selectedItems = new List<LabelPrintItem>();
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (Convert.ToBoolean(row.Cells["colSelect"].Value))
                {
                    selectedItems.Add(new LabelPrintItem {
                        Name = row.Cells["colName"].Value.ToString(),
                        SKU = row.Cells["colSku"].Value?.ToString() ?? "",
                        Quantity = Convert.ToInt32(row.Cells["colQty"].Value ?? 1)
                    });
                }
            }

            if (selectedItems.Count == 0)
            {
                MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "ÙŠØ±Ø¬Ù‰ Ø§Ø®ØªÙŠØ§Ø± ØµÙ†Ù ÙˆØ§Ø­Ø¯ Ø¹Ù„Ù‰ Ø§Ù„Ø£Ù‚Ù„." : "Please select at least one item.");
                return;
            }

            ThermalLabelHelper.GenerateLabelPDF(selectedItems);
        }
    }

    public class LabelPrintItem
    {
        public string Name { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }
    }
}
