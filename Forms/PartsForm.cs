using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GenericInventorySystem.Data;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Services;

namespace GenericInventorySystem.Forms
{
    /// <summary>
    /// Inventory Management Screen.
    /// Handles listing, filtering, adding, and deleting parts.
    /// Features custom-drawn grid cells for status badges and action icons.
    /// </summary>
    public partial class PartsForm : UserControl
    {
        private DataGridView dgvParts;
        private Button btnAdd;
        private Button btnAddCategory;
        private Button btnFilter;
        private Button btnImport;
        private Button btnExport;
        private ModernTextBox txtSearch;
        private InventoryService _inventoryService;

        public PartsForm()
        {
            InitializeComponent();
            _inventoryService = new InventoryService();
            
            GenericInventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization(); // Apply initially
            ApplyPermissions();
            
            LoadData(); 

            // Sync with global currency changes
            GenericInventorySystem.Services.CurrencyService.CurrencyChanged += (s, e) => { dgvParts.Invalidate(); };

            // Auto-refresh every 30 seconds to pick up Web POS changes
            var syncTimer = new System.Windows.Forms.Timer { Interval = 30000 };
            syncTimer.Tick += (s, e) => { if (this.Visible) LoadData(txtSearch.Text == "Search..." ? "" : txtSearch.Text); };
            syncTimer.Start();
            this.Disposed += (s, e) => syncTimer.Dispose();
        }

        private void ApplyLocalization()
        {
            GenericInventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;

            var ctrlTitle = this.Controls.Find("lblInventoryTitle", true);
            if (ctrlTitle.Length > 0) ctrlTitle[0].Text = L("Parts_Title");

            if (txtSearch != null)
            {
                txtSearch.PlaceholderText = L("Parts_Search");
            }

            var ctrlDel = this.Controls.Find("btnDeleteSelected", true);

            if (btnAdd != null) btnAdd.Invalidate(); 
            if (btnAddCategory != null) btnAddCategory.Invalidate();
            if (btnFilter != null) btnFilter.Invalidate();
            if (btnImport != null) btnImport.Invalidate();
            if (btnExport != null) btnExport.Invalidate();
            if (ctrlDel.Length > 0) ctrlDel[0].Invalidate();

            GenericInventorySystem.Helpers.LocalizationManager.TranslateControl(this);

            // Translate DataGridView columns
            if (dgvParts != null && dgvParts.Columns.Count > 0)
            {
                if (dgvParts.Columns.Contains("colImage")) dgvParts.Columns["colImage"].HeaderText = L("Parts_GridImage");
                if (dgvParts.Columns.Contains("colSKU")) dgvParts.Columns["colSKU"].HeaderText = L("Parts_GridSKU");
                if (dgvParts.Columns.Contains("colBarcode")) dgvParts.Columns["colBarcode"].HeaderText = L("Parts_GridBarcode");
                if (dgvParts.Columns.Contains("colName")) dgvParts.Columns["colName"].HeaderText = L("Parts_GridProduct");
                if (dgvParts.Columns.Contains("colCategory")) dgvParts.Columns["colCategory"].HeaderText = L("Parts_GridCategory");
                if (dgvParts.Columns.Contains("colLocation")) dgvParts.Columns["colLocation"].HeaderText = L("Parts_GridLocation");
                if (dgvParts.Columns.Contains("colShelf")) dgvParts.Columns["colShelf"].HeaderText = L("Parts_GridShelf");
                if (dgvParts.Columns.Contains("colStock")) dgvParts.Columns["colStock"].HeaderText = L("Parts_GridStock");
                if (dgvParts.Columns.Contains("minimum_stock_level")) dgvParts.Columns["minimum_stock_level"].HeaderText = L("Parts_GridMinStock");
                if (dgvParts.Columns.Contains("colPrice")) dgvParts.Columns["colPrice"].HeaderText = L("Parts_GridPrice");
                if (dgvParts.Columns.Contains("colStatus")) dgvParts.Columns["colStatus"].HeaderText = L("Parts_GridStatus");
                if (dgvParts.Columns.Contains("colActions")) dgvParts.Columns["colActions"].HeaderText = L("Parts_GridActions");
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if(this.Visible && !this.DesignMode)
            {
                LoadData(); // Auto-refresh inventory
            }
        }
        private void InitializeComponent()
        {
            this.dgvParts = new DataGridView();
            this.btnAdd = new Button();
            this.btnFilter = new Button();
            this.btnImport = new Button();
            this.btnExport = new Button();
            this.txtSearch = new ModernTextBox();

            ((System.ComponentModel.ISupportInitialize)(this.dgvParts)).BeginInit();
            this.SuspendLayout();

            // STANDARD LAYOUT
            TableLayoutPanel tlpMain = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = ThemeConfig.BackgroundColor, Padding = new Padding(20) };
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Header (panelTop)
            Panel panelTop = new Panel { Dock = DockStyle.Fill, BackColor = ThemeConfig.BackgroundColor, Margin = new Padding(0) };
            
            // Title
            Label lblInventoryTitle = ThemeConfig.CreateStandardHeader("Inventory Management");
            lblInventoryTitle.Name = "lblInventoryTitle";
            panelTop.Controls.Add(lblInventoryTitle);

            // Search Bar
            txtSearch.IsSearch = true;
            txtSearch.ShowLabel = false;
            txtSearch.PlaceholderText = "Search Inventory...";
            txtSearch.Size = new Size(320, 40);
            txtSearch.Location = new Point(0, 55);
            txtSearch.TextChanged += (s, e) => { 
                string ph = GenericInventorySystem.Helpers.LocalizationManager.GetString("Parts_Search");
                if (txtSearch.Text != ph && txtSearch.Text != "Search...") 
                    LoadData(txtSearch.Text); 
            };
            panelTop.Controls.Add(txtSearch);


            // Action Buttons Panel
            FlowLayoutPanel panelButtons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(400, 70),
                Height = 40,
                WrapContents = false
            };

            // Filter Button (Rounded Outline)
            btnFilter.Size = new Size(110, 40);
            btnFilter.FlatStyle = FlatStyle.Flat;
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.BackColor = ThemeConfig.SurfaceColor;
            btnFilter.Cursor = Cursors.Hand;
            btnFilter.Click += BtnFilter_Click;
            btnFilter.Paint += (s, e) => ThemeConfig.DrawIconButton(btnFilter, e.Graphics, "filter", "Parts_Filter", ThemeConfig.WarningColor, ThemeConfig.WarningColor, true);
            panelButtons.Controls.Add(btnFilter);

            // Export Button (Rounded Outline)
            btnExport.Size = new Size(100, 40);
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.BackColor = ThemeConfig.SurfaceColor;
            btnExport.Cursor = Cursors.Hand;
            btnExport.Click += BtnExport_Click;
            btnExport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnExport, e.Graphics, "export", "Parts_Export", ThemeConfig.PrimaryColor, ThemeConfig.PrimaryColor, true);
            panelButtons.Controls.Add(btnExport);

            // Import Button (Rounded Outline)
            btnImport.Size = new Size(100, 40);
            btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.FlatAppearance.BorderSize = 0;
            btnImport.BackColor = ThemeConfig.SurfaceColor;
            btnImport.Cursor = Cursors.Hand;
            btnImport.Click += BtnImport_Click;
            btnImport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnImport, e.Graphics, "import", "Parts_Import", ThemeConfig.SuccessBorder, ThemeConfig.SuccessBorder, true);
            panelButtons.Controls.Add(btnImport);

            // Add Category Button
            btnAddCategory = new Button();
            btnAddCategory.Size = new Size(140, 40);
            btnAddCategory.FlatStyle = FlatStyle.Flat;
            btnAddCategory.FlatAppearance.BorderSize = 0;
            btnAddCategory.BackColor = ThemeConfig.SurfaceColor;
            btnAddCategory.Cursor = Cursors.Hand;
            btnAddCategory.Click += BtnAddCategory_Click;
            btnAddCategory.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAddCategory, e.Graphics, "add", "Parts_AddCategory", ThemeConfig.PrimaryColor, ThemeConfig.PrimaryColor, true);
            panelButtons.Controls.Add(btnAddCategory);

            // Delete Selected Button
            Button btnDeleteSelected = new Button { Size = new Size(130, 40), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Name = "btnDeleteSelected" };
            btnDeleteSelected.FlatAppearance.BorderSize = 0;
            btnDeleteSelected.BackColor = ThemeConfig.SurfaceColor;
            btnDeleteSelected.Click += (s, e) => {
                 var checkedIds = new System.Collections.Generic.List<int>();
                 foreach (DataGridViewRow row in dgvParts.Rows) {
                     var chkCell = row.Cells["colCheck"] as DataGridViewCheckBoxCell;
                     if (chkCell != null && Convert.ToBoolean(chkCell.Value ?? false)) {
                         if (int.TryParse(row.Cells["part_id"].Value?.ToString(), out int pId)) checkedIds.Add(pId);
                     }
                 }
                 if (checkedIds.Count == 0) { MessageHelper.ShowWarning("Please select at least one item to delete."); return; }
                 if (MessageHelper.ConfirmAction($"Are you sure you want to delete {checkedIds.Count} selected items?")) {
                     foreach(int i in checkedIds) _inventoryService.DeletePart(i);
                     MessageHelper.ShowSuccess($"{checkedIds.Count} items deleted successfully.");
                     LoadData(txtSearch.Text == "Search..." ? "" : txtSearch.Text);
                 }
            };
            btnDeleteSelected.Paint += (s, e) => ThemeConfig.DrawIconButton(btnDeleteSelected, e.Graphics, "delete", "Parts_Delete", ThemeConfig.DangerColor, ThemeConfig.DangerColor, true);
            panelButtons.Controls.Add(btnDeleteSelected);

            // Add Product Button
            btnAdd.Size = new Size(160, 40);
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.BackColor = ThemeConfig.SurfaceColor;
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Click += BtnAdd_Click;
            btnAdd.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAdd, e.Graphics, "add", "Parts_AddProduct", Color.White, ThemeConfig.PrimaryColor, false);
            panelButtons.Controls.Add(btnAdd);

            panelTop.Controls.Add(panelButtons);
            
            // RTL Awareness for positioning
            panelTop.Resize += (s, e) => {
                if (LocalizationManager.IsArabic) {
                    txtSearch.Location = new Point(panelTop.Width - txtSearch.Width, 55);
                    panelButtons.Location = new Point(0, 50);
                } else {
                    txtSearch.Location = new Point(0, 55);
                    panelButtons.Location = new Point(panelTop.Width - panelButtons.Width, 50);
                }
            };

            tlpMain.Controls.Add(panelTop, 0, 0);
            // DataGridView Configuration
            dgvParts.AllowUserToAddRows = false;
            dgvParts.ReadOnly = false;
            dgvParts.AutoGenerateColumns = false;
            dgvParts.BorderStyle = BorderStyle.None;
            dgvParts.BackgroundColor = ThemeConfig.SurfaceColor;
            
            dgvParts.CellPainting += DgvParts_CellPainting;
            dgvParts.CellFormatting += DgvParts_CellFormatting;
            dgvParts.CellMouseClick += DgvParts_CellMouseClick;
            dgvParts.CellMouseMove += DgvParts_CellMouseMove;
            dgvParts.CellMouseLeave += DgvParts_CellMouseLeave;
            dgvParts.DataError += (s, e) => e.ThrowException = false;

            // Card Panel (Rounded body)
            Panel pnlCard = ThemeConfig.CreateCardPanel(dgvParts);
            tlpMain.Controls.Add(pnlCard, 0, 1);

            this.Controls.Add(tlpMain);
            this.Dock = DockStyle.Fill;

            // Define Columns
            dgvParts.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colCheck", HeaderText = "", Width = 30, FillWeight = 1, ReadOnly = false }); // Active Checkbox
            
            var colImage = new DataGridViewImageColumn { Name = "colImage", HeaderText = "Image", Width = 50, ImageLayout = DataGridViewImageCellLayout.Zoom, FillWeight = 6, ReadOnly = true };
            dgvParts.Columns.Add(colImage);
            
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSKU", HeaderText = "SKU", DataPropertyName = "part_number", FillWeight = 10, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBarcode", HeaderText = "Barcode", DataPropertyName = "barcode", FillWeight = 10, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "Product", DataPropertyName = "part_name", FillWeight = 18, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCategory", HeaderText = "Category", DataPropertyName = "category_name", FillWeight = 12, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLocation", HeaderText = "Location", DataPropertyName = "location", FillWeight = 10, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colShelf", HeaderText = "Shelf", DataPropertyName = "shelf", FillWeight = 8, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStock", HeaderText = "Stock", DataPropertyName = "quantity_in_stock", FillWeight = 8, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "minimum_stock_level", HeaderText = "Min Stock", DataPropertyName = "minimum_stock_level", FillWeight = 8, ReadOnly = true });
            
            var colPrice = new DataGridViewTextBoxColumn { Name = "colPrice", HeaderText = "Price", DataPropertyName = "selling_price", FillWeight = 10, ReadOnly = true };
            // No C2 format here â€” we handle it in CellFormatting using CurrencyService
            dgvParts.Columns.Add(colPrice);
            
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "Status", DataPropertyName = "status", FillWeight = 9, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewButtonColumn { Name = "colActions", HeaderText = "Actions", FillWeight = 10, ReadOnly = true });
            
            // Hidden columns
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "part_id", DataPropertyName = "part_id", Visible = false });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "part_image", DataPropertyName = "part_image", Visible = false });
            
            // Apply Theme LAST to ensure header styles override defaults
            ThemeConfig.ApplyGridTheme(dgvParts);
            ThemeConfig.ApplyHeaderCheckBox(dgvParts, "colCheck");

            ((System.ComponentModel.ISupportInitialize)(this.dgvParts)).EndInit();
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Loads inventory data from the database with optional filtering.
        /// </summary>
        /// <param name="search">Search term for name/SKU</param>
        /// <param name="lowStockOnly">If true, shows only items below minimum stock</param>
        /// <param name="activeOnly">If true, shows only active items</param>
        /// <param name="category">Category name to filter by</param>
        private void LoadData(string search = "", bool lowStockOnly = false, bool activeOnly = false, string category = null)
        {
            try
            {
                dgvParts.DataSource = null; // Force clear
                
                DataTable dt = _inventoryService.GetAllParts(search, lowStockOnly, activeOnly, category);
                dgvParts.DataSource = dt;

                // Re-apply images
                foreach (DataGridViewRow row in dgvParts.Rows)
                {
                    string imagePath = row.Cells["part_image"].Value?.ToString();
                    row.Cells["colImage"].Value = CreateProductImage(imagePath);
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Error: {ex.Message}");
            }
        }

        private Bitmap CreateProductImage(string imagePath = null)
        {
            // Try loading from file first if path provided
            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    // Image path from DB is relative "Assets/Products/filename.ext"
                    // Or possibly just filename if legacy. We check both.
                    string fullPath = System.IO.Path.Combine(Application.StartupPath, imagePath);
                    
                    if (!System.IO.File.Exists(fullPath))
                    {
                        // Try fallback to Assets/Products if relative path lacks it
                        fullPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "Products", System.IO.Path.GetFileName(imagePath));
                    }

                    if (System.IO.File.Exists(fullPath))
                    {
                        // Use MemoryStream to avoid file locking
                        byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
                        using (var ms = new System.IO.MemoryStream(bytes))
                        using (var original = Image.FromStream(ms))
                        {
                            return new Bitmap(original, new Size(40, 40));
                        }
                    }
                }
                catch { /* Fallback to placeholder */ }
            }

            // Procedural Nuricon Placeholder - Beautiful gradient based box
            Bitmap img = new Bitmap(40, 40);
            using (Graphics g = Graphics.FromImage(img))
            {
                g.Clear(ThemeConfig.BackgroundColor); // Standard Light bg
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // Get the new procedural 'part' icon
                Image icon = ThemeConfig.GetNuricon("part");
                if (icon != null)
                {
                    // Draw it centered with a bit of padding
                    g.DrawImage(icon, new Rectangle(6, 6, 28, 28));
                }
            }
            return img;
        }


        private void DgvParts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvParts.Rows[e.RowIndex];
            var stockCell = row.Cells["colStock"];
            var minStockCell = row.Cells["minimum_stock_level"];

            // Price: format using active currency
            if (dgvParts.Columns[e.ColumnIndex].Name == "colPrice" && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal usdPrice))
                {
                    e.Value = GenericInventorySystem.Services.CurrencyService.Format(usdPrice);
                    e.FormattingApplied = true;
                }
            }

            // Low Stock Logic: Whole Row Pink
            if (stockCell.Value != null && minStockCell.Value != null)
            {
                if (int.TryParse(stockCell.Value.ToString(), out int stock) && int.TryParse(minStockCell.Value.ToString(), out int minStock))
                {
                    if (stock <= minStock)
                    {
                        row.DefaultCellStyle.BackColor = ThemeConfig.DangerBadgeBg;
                        row.DefaultCellStyle.SelectionBackColor = ThemeConfig.DangerLight;
                        row.DefaultCellStyle.SelectionForeColor = ThemeConfig.TextColorDark;
                    }
                    else
                    {
                        // Reset to default theme styles
                        row.DefaultCellStyle.BackColor = ThemeConfig.SurfaceColor;
                        row.DefaultCellStyle.SelectionBackColor = ThemeConfig.SelectionBackColor;
                        row.DefaultCellStyle.SelectionForeColor = ThemeConfig.TextColorDark;
                    }
                }
            }
        }

        private void DgvParts_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 1. Stock Level Pill (Only if low stock)
            if (dgvParts.Columns[e.ColumnIndex].Name == "colStock")
            {
                // Check value logic again to decide whether to paint pill
                var row = dgvParts.Rows[e.RowIndex];
                var minStockCell = row.Cells["minimum_stock_level"];
                if (e.Value != null && minStockCell.Value != null)
                {
                    int stock = 0;
                    int minStock = 0;
                    int.TryParse(e.Value.ToString(), out stock);
                    int.TryParse(minStockCell.Value.ToString(), out minStock);

                    if (stock <= minStock)
                    {
                        e.Handled = true;
                        e.PaintBackground(e.CellBounds, true);
                        
                        // Draw Red Pill around the number
                        // Measure text
                        SizeF textSize = e.Graphics.MeasureString(e.Value.ToString(), e.CellStyle.Font);
                        float pillWidth = Math.Max(textSize.Width + 16, 40);
                        // Draw Red Pill around the number - CENTERED
                        float pillX = e.CellBounds.X + (e.CellBounds.Width - pillWidth) / 2;
                        RectangleF pillRect = new RectangleF(
                            pillX,
                            e.CellBounds.Y + (e.CellBounds.Height - 24) / 2,
                            pillWidth,
                            24
                        );
                        
                        using (GraphicsPath path = GetRoundedRect(Rectangle.Round(pillRect), 8))
                        using (SolidBrush brush = new SolidBrush(ThemeConfig.DangerLight)) // Darker pink pill
                        {
                            e.Graphics.FillPath(brush, path);
                        }
                        
                        // Draw Text
                        TextRenderer.DrawText(e.Graphics, e.Value.ToString(), ThemeConfig.SmallBoldFont, Rectangle.Round(pillRect), ThemeConfig.DangerBadgeText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        return;
                    }
                }
            }

            // 2. Status badge (Active) - Outlined Style
            if (dgvParts.Columns[e.ColumnIndex].Name == "colStatus")
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);
                
                string status = e.Value?.ToString() ?? "Active";
                bool isActive = status.Equals("Active", StringComparison.OrdinalIgnoreCase);
                
                // Target: White/Light Bg, Green Border, Green Text
                Color borderColor = isActive ? ThemeConfig.SuccessBorder : ThemeConfig.DangerBorder;
                Color txtColor = isActive ? ThemeConfig.SuccessBadgeText : ThemeConfig.DangerBadgeText;
                Color fillColor = isActive ? ThemeConfig.SuccessBadgeBg : ThemeConfig.DangerBadgeBg;

                Rectangle badgeRect = new Rectangle(e.CellBounds.X + 5, e.CellBounds.Y + 13, 60, 24);
                
                using (var path = GetRoundedRect(badgeRect, 10))
                using (var pen = new Pen(borderColor, 1))
                using (var brush = new SolidBrush(fillColor))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                    
                    TextRenderer.DrawText(e.Graphics, status, ThemeConfig.MicroBoldFont, badgeRect, txtColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

            }

            // 3. Actions - Nuricon Icons for Edit/Delete
            if (dgvParts.Columns[e.ColumnIndex].Name == "colActions")
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);
                
                // Load Nuricon Icons
                Image imgEdit = ThemeConfig.GetNuricon("edit");
                Image imgDelete = ThemeConfig.GetNuricon("delete");

                // Edit Button
                Rectangle editRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + 14, 32, 32);
                if (imgEdit != null)
                {
                    e.Graphics.DrawImage(imgEdit, editRect);
                }

                // Delete Button
                Rectangle delRect = new Rectangle(e.CellBounds.X + 48, e.CellBounds.Y + 14, 32, 32);
                if (imgDelete != null)
                {
                    e.Graphics.DrawImage(imgDelete, delRect);
                }

                // Adjust Stock Button (Added)
                Rectangle adjRect = new Rectangle(e.CellBounds.X + 88, e.CellBounds.Y + 14, 32, 32);
                Image imgAdjust = ThemeConfig.GetNuricon("history"); // Using history icon for adjustment logs/action
                if (imgAdjust != null)
                {
                    e.Graphics.DrawImage(imgAdjust, adjRect);
                }
            }
        }

        private void DrawOutlinedButton(Graphics g, Rectangle rect, string icon, Color color)
        {
            // Draw White Box with Light Gray Border
            using (var path = GetRoundedRect(rect, 8))
            using (var pen = new Pen(ThemeConfig.BorderColor, 1)) // Light slate border
            using (var brush = new SolidBrush(ThemeConfig.SurfaceColor))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            
            // Draw Icon
            // Using DrawString for GDI+ consistency if needed, assuming Emojis
            // Center glyph
             TextRenderer.DrawText(g, icon, ThemeConfig.EmojiFont, rect, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void DgvParts_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dgvParts.Columns[e.ColumnIndex].Name == "colActions")
            {
                // Hit Test based on e.X, e.Y (relative to cell)
                // Edit Rect (X=10, W=30), Delete Rect (X=50, W=30)
                
                // Check Delete
                if (e.X >= 50 && e.X <= 80 && e.Y >= 10 && e.Y <= 40)
                {
                    string id = dgvParts.Rows[e.RowIndex].Cells["part_id"].Value?.ToString();
                    if (string.IsNullOrEmpty(id)) return;

                    if (!GenericInventorySystem.Helpers.UserSession.IsAdmin)
                    {
                        MessageHelper.ShowWarning(GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "Ù„ÙŠØ³ Ù„Ø¯ÙŠÙƒ ØµÙ„Ø§Ø­ÙŠØ© Ù„Ø­Ø°Ù Ø§Ù„Ø£ØµÙ†Ø§Ù." : "You do not have permission to delete items.");
                        return;
                    }

                    if (MessageHelper.ConfirmAction("Delete this item?"))
                    {
                        _inventoryService.DeletePart(int.Parse(id));
                        LoadData();
                    }
                }
                // Check Edit
                else if (e.X >= 10 && e.X <= 40 && e.Y >= 10 && e.Y <= 40)
                {
                    // Edit Clicked
                    if (!GenericInventorySystem.Helpers.UserSession.IsAdmin)
                    {
                        MessageHelper.ShowWarning(GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "Ù„ÙŠØ³ Ù„Ø¯ÙŠÙƒ ØµÙ„Ø§Ø­ÙŠØ© Ù„ØªØ¹Ø¯ÙŠÙ„ Ø§Ù„Ø£ØµÙ†Ø§Ù." : "You do not have permission to edit items.");
                        return;
                    }

                    var row = dgvParts.Rows[e.RowIndex];
                    string id = row.Cells["part_id"].Value?.ToString();
                    if(string.IsNullOrEmpty(id)) return;

                    string name = row.Cells["colName"].Value?.ToString() ?? "";
                    string sku = row.Cells["colSKU"].Value?.ToString() ?? "";
                    
                    object qtyVal = row.Cells["colStock"].Value;
                    int qty = (qtyVal == null || qtyVal == DBNull.Value) ? 0 : Convert.ToInt32(qtyVal);
                    
                    object priceVal = row.Cells["colPrice"].Value;
                    decimal price = (priceVal == null || priceVal == DBNull.Value) ? 0 : Convert.ToDecimal(priceVal);
                    
                    string status = row.Cells["colStatus"].Value?.ToString() ?? "Active";
                    string barcode = row.Cells["colBarcode"].Value?.ToString() ?? "";
                    string location = row.Cells["colLocation"].Value?.ToString() ?? "";
                    string shelf = row.Cells["colShelf"].Value?.ToString() ?? "";
                    string image = row.Cells["part_image"].Value?.ToString() ?? "";
                    string category = row.Cells["colCategory"].Value?.ToString() ?? "";
                    
                    object minVal = row.Cells["minimum_stock_level"].Value;
                    int minStock = (minVal == null || minVal == DBNull.Value) ? 0 : Convert.ToInt32(minVal);

                    using (AddPartForm form = new AddPartForm())
                    {
                        form.LoadPartData(id, name, sku, qty, price, minStock, status, barcode, location, shelf, image, category);
                        
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                             // Refresh grid AND preserve current search/filter context
                             string currentSearch = txtSearch.Text == "Search..." ? "" : txtSearch.Text;
                             LoadData(currentSearch); 
                             MessageHelper.ShowSuccess("Item updated successfully.");
                        }
                    }
                }
                // Check Stock Adjustment
                else if (e.X >= 90 && e.X <= 120 && e.Y >= 10 && e.Y <= 40)
                {
                    var row = dgvParts.Rows[e.RowIndex];
                    int partId = int.Parse(row.Cells["part_id"].Value?.ToString() ?? "0");
                    string partName = row.Cells["colName"].Value?.ToString() ?? "";
                    
                    ShowAdjustmentDialog(partId, partName);
                }
            }
        }

        private void ShowAdjustmentDialog(int partId, string partName)
        {
            string title = (LocalizationManager.IsArabic ? "تعديل المخزون: " : "Adjust Stock: ") + partName;
            BaseModalForm f = new BaseModalForm { TitleText = title, Size = new Size(450, 280) };
            
            TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 5, AutoSize = true, Padding = new Padding(10) };
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

            string instrText = LocalizationManager.IsArabic ? "أدخل الكمية للإضافة (+) أو الطرح (-):" : "Enter quantity to add (+) or subtract (-):";
            Label lblInstr = new Label { Text = instrText, AutoSize = true, Font = ThemeConfig.StandardFont, Margin = new Padding(0, 10, 0, 5) };
            NumericUpDown numQty = new NumericUpDown { Width = 200, Minimum = -99999, Maximum = 99999, Font = ThemeConfig.SubHeaderFont, Margin = new Padding(0, 0, 0, 20) };
            
            string reasonLabelText = LocalizationManager.IsArabic ? "السبب:" : "Reason:";
            Label lblReason = new Label { Text = reasonLabelText, AutoSize = true, Font = ThemeConfig.StandardFont, Margin = new Padding(0, 0, 0, 5) };
            TextBox txtReasonAdjust = new TextBox { Width = 380, Font = ThemeConfig.StandardFont, Margin = new Padding(0, 0, 0, 20), Multiline = true, Height = 80 };
            
            Button btnSaveAdj = new ModernButton { Text = LocalizationManager.IsArabic ? "تعديل" : "Adjust", Size = new Size(120, 40), Anchor = AnchorStyles.Right };
            ThemeConfig.ApplyPrimaryButton(btnSaveAdj);
            
            btnSaveAdj.Click += (s, e) => {
                if (numQty.Value == 0) 
                { 
                    MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "لا يمكن أن يكون التعديل صفراً." : "Adjustment cannot be zero."); 
                    return; 
                }
                if (string.IsNullOrWhiteSpace(txtReasonAdjust.Text)) 
                { 
                    MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "يرجى تقديم سبب." : "Please provide a reason."); 
                    return; 
                }
                
                try {
                    _inventoryService.AdjustStock(partId, (int)numQty.Value, txtReasonAdjust.Text);
                    MessageHelper.ShowSuccess(LocalizationManager.IsArabic ? "تم تعديل المخزون بنجاح!" : "Stock adjusted successfully.");
                    f.DialogResult = DialogResult.OK;
                    f.Close();
                    LoadData(txtSearch.Text == "Search..." ? "" : txtSearch.Text);
                } catch(Exception ex) { MessageHelper.ShowError("Error: " + ex.Message); }
            };

            tlp.Controls.Add(lblInstr, 0, 0);
            tlp.Controls.Add(numQty, 0, 1);
            tlp.Controls.Add(lblReason, 0, 2);
            tlp.Controls.Add(txtReasonAdjust, 0, 3);
            tlp.Controls.Add(btnSaveAdj, 0, 4);
            
            f.ContentPanel.Controls.Add(tlp);
            LocalizationManager.ApplyRTL(f);
            f.ShowDialog();
        }
        

        private void DgvParts_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
             if (e.RowIndex >= 0 && dgvParts.Columns[e.ColumnIndex].Name == "colActions")
             {
                 dgvParts.Cursor = Cursors.Hand;
             }
             else
             {
                 dgvParts.Cursor = Cursors.Default;
             }
        }

        private void DgvParts_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
             dgvParts.Cursor = Cursors.Default;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (AddPartForm form = new AddPartForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                    LoadData();
            }
        }

        // ... CreateProductImage ... (unchanged, but replace block needs to handle context)
        // Wait, I am replacing LoadData AND BtnFilter_Click. 
        // I will target LoadData first, then BtnFilter_Click separately or use a larger block?
        // Larger block is risky if lines between are changed.
        // I will use replace_file_content for LoadData first.
        
        // Actually, let's do the filter button first, assuming LoadData signature changes.
        // Wait, if I change LoadData signature, I break existing calls!
        // Existing calls: LoadData() (no args), LoadData(txtSearch.Text).
        // C# optional params handles this gracefully IF I append the new optional param at the end.
        // My definition: (string search = "", bool lowStockOnly = false, bool activeOnly = false, string category = null)
        // Previous calls: LoadData() -> search="", low=false, active=false, cat=null. OK.
        // LoadData(str) -> search=str ... OK.
        // So it is safe.
        
        // So I will update LoadData in this step.
        // And BtnFilter_Click in next step to keep chunks manageable.
        
        private void BtnFilter_Click(object sender, EventArgs e)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ThemeConfig.ApplyModernMenuTheme(menu);
            
            // Standard Filters
            menu.Items.Add("All Items", null, (s, args) => LoadData());
            menu.Items.Add("Low Stock Only", null, (s, args) => LoadData("", lowStockOnly: true));
            menu.Items.Add("Active Only", null, (s, args) => LoadData("", activeOnly: true));
            
            menu.Items.Add(new ToolStripSeparator());
            
            // Category Filters
            var categoriesHeader = new ToolStripMenuItem("Categories") { Enabled = false, Font = ThemeConfig.ButtonFont };
            menu.Items.Add(categoriesHeader);
            
            try
            {
                 DataTable cats = _inventoryService.GetCategories(); // Returns id, category_name
                 foreach(DataRow row in cats.Rows)
                 {
                     string catName = row["category_name"].ToString();
                     menu.Items.Add(catName, null, (s, args) => LoadData("", category: catName));
                 }
            }
            catch {}
            
            menu.Show(btnFilter, new Point(0, btnFilter.Height));
        }
        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            using (AddCategoryForm form = new AddCategoryForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                     MessageHelper.ShowSuccess("Category added! It will now appear in the dropdown.");
                     // Could refresh grid but categories are in the add part form mostly
                }
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            ExportToCsv();
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            ImportFromCsv();
        }

        private void ExportToCsv()
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV Files (*.csv)|*.csv";
                saveDialog.FileName = $"Parts_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                saveDialog.Title = "Export Parts to CSV";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = (DataTable)dgvParts.DataSource;
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning("No data to export.");
                        return;
                    }

                    // Create export table with selected columns
                    DataTable exportDt = new DataTable();
                    exportDt.Columns.Add("PartNumber");
                    exportDt.Columns.Add("PartName");
                    exportDt.Columns.Add("Category");
                    exportDt.Columns.Add("Quantity");
                    exportDt.Columns.Add("MinimumStock");
                    exportDt.Columns.Add("UnitPrice");
                    exportDt.Columns.Add("Location");
                    exportDt.Columns.Add("Status");

                    foreach (DataRow row in dt.Rows)
                    {
                        exportDt.Rows.Add(
                            row["part_number"],
                            row["part_name"],
                            row["category_name"],
                            row["quantity_in_stock"],
                            row["minimum_stock_level"],
                            row["selling_price"],
                            row["location"],
                            row["status"]
                        );
                    }

                    if (Helpers.ImportExportHelper.ExportToCsv(exportDt, saveDialog.FileName))
                    {
                        MessageHelper.ShowSuccess($"Exported {exportDt.Rows.Count} parts to CSV successfully!");
                    }
                    else
                    {
                        MessageHelper.ShowError("Failed to export data.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Export error: {ex.Message}");
            }
        }

        private void ExportToExcel()
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                saveDialog.FileName = $"Parts_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                saveDialog.Title = "Export Parts to Excel";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = (DataTable)dgvParts.DataSource;
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning("No data to export.");
                        return;
                    }

                    // Create export table with selected columns
                    DataTable exportDt = new DataTable();
                    exportDt.Columns.Add("PartNumber");
                    exportDt.Columns.Add("PartName");
                    exportDt.Columns.Add("Category");
                    exportDt.Columns.Add("Quantity");
                    exportDt.Columns.Add("MinimumStock");
                    exportDt.Columns.Add("UnitPrice");
                    exportDt.Columns.Add("Location");
                    exportDt.Columns.Add("Status");

                    foreach (DataRow row in dt.Rows)
                    {
                        exportDt.Rows.Add(
                            row["part_number"],
                            row["part_name"],
                            row["category_name"],
                            row["quantity_in_stock"],
                            row["minimum_stock_level"],
                            row["selling_price"],
                            row["location"],
                            row["status"]
                        );
                    }

                    if (Helpers.ImportExportHelper.ExportToExcel(exportDt, saveDialog.FileName, "Parts"))
                    {
                        MessageHelper.ShowSuccess($"Exported {exportDt.Rows.Count} parts to Excel successfully!");
                    }
                    else
                    {
                        MessageHelper.ShowError("Failed to export data.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Export error: {ex.Message}");
            }
        }

        private void ImportFromCsv()
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "CSV Files (*.csv)|*.csv";
                openDialog.Title = "Import Parts from CSV";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = Helpers.ImportExportHelper.ImportFromCsv(openDialog.FileName);
                    
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning("No data found in the file.");
                        return;
                    }

                    // Validate columns
                    if (!dt.Columns.Contains("PartNumber") || !dt.Columns.Contains("PartName"))
                    {
                        MessageHelper.ShowError("Invalid file format. Required columns: PartNumber, PartName, Category, Quantity, MinimumStock, UnitPrice, Location, Status");
                        return;
                    }

                    int imported = 0;
                    int skipped = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        try
                        {
                            string partNumber = row["PartNumber"].ToString();
                            string partName = row["PartName"].ToString();
                            
                            if (string.IsNullOrWhiteSpace(partNumber) || string.IsNullOrWhiteSpace(partName))
                            {
                                skipped++;
                                continue;
                            }

                            // Check if part already exists
                            if (_inventoryService.PartExists(partNumber))
                            {
                                skipped++;
                                continue;
                            }

                            // Import the part
                            _inventoryService.ImportPart(
                                partNumber,
                                partName,
                                row["Category"].ToString(),
                                int.Parse(row["Quantity"].ToString()),
                                int.Parse(row["MinimumStock"].ToString()),
                                decimal.Parse(row["UnitPrice"].ToString()),
                                row["Location"].ToString(),
                                row["Status"].ToString()
                            );
                            imported++;
                        }
                        catch
                        {
                            skipped++;
                        }
                    }

                    LoadData();
                    MessageHelper.ShowSuccess($"Import complete!\nImported: {imported}\nSkipped: {skipped}");
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Import error: {ex.Message}");
            }
        }

        private void ImportFromExcel()
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                openDialog.Title = "Import Parts from Excel";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = Helpers.ImportExportHelper.ImportFromExcel(openDialog.FileName, "Parts");
                    
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning("No data found in the file.");
                        return;
                    }

                    // Validate columns
                    if (!dt.Columns.Contains("PartNumber") || !dt.Columns.Contains("PartName"))
                    {
                        MessageHelper.ShowError("Invalid file format. Required columns: PartNumber, PartName, Category, Quantity, MinimumStock, UnitPrice, Location, Status");
                        return;
                    }

                    int imported = 0;
                    int skipped = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        try
                        {
                            string partNumber = row["PartNumber"].ToString();
                            string partName = row["PartName"].ToString();
                            
                            if (string.IsNullOrWhiteSpace(partNumber) || string.IsNullOrWhiteSpace(partName))
                            {
                                skipped++;
                                continue;
                            }

                            // Check if part already exists
                            if (_inventoryService.PartExists(partNumber))
                            {
                                skipped++;
                                continue;
                            }

                            // Import the part
                            _inventoryService.ImportPart(
                                partNumber,
                                partName,
                                row["Category"].ToString(),
                                int.Parse(row["Quantity"].ToString()),
                                int.Parse(row["MinimumStock"].ToString()),
                                decimal.Parse(row["UnitPrice"].ToString()),
                                row["Location"].ToString(),
                                row["Status"].ToString()
                            );
                            imported++;
                        }
                        catch
                        {
                            skipped++;
                        }
                    }

                    LoadData();
                    MessageHelper.ShowSuccess($"Import complete!\nImported: {imported}\nSkipped: {skipped}");
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Import error: {ex.Message}");
            }
        }
        private void ApplyPermissions()
        {
            if (!GenericInventorySystem.Helpers.UserSession.IsAdmin)
            {
                if (btnAdd != null) btnAdd.Visible = false;
                if (btnAddCategory != null) btnAddCategory.Visible = false;
                if (btnImport != null) btnImport.Visible = false; // Import is like adding

                var ctrlDel = this.Controls.Find("btnDeleteSelected", true);
                if (ctrlDel.Length > 0) ctrlDel[0].Visible = false;
            }
        }
    }
}

