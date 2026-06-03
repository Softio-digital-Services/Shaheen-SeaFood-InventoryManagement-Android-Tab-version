using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using butcherPOS.Data;
using butcherPOS.Controls;
using butcherPOS.Services;
using butcherPOS.Helpers;

namespace butcherPOS.Forms
{
    public class AddProductServiceForm : BaseModalForm
    {
        private PictureBox pbImage;
        private ModernButton btnUpload;
        
        private ModernTextBox txtName;
        private ModernTextBox txtDescription;
        private ModernTextBox txtSku;
        private ModernTextBox txtBarcode;
        private ModernButton btnAutoSku;
        private ModernButton btnScanBarcode;
        private ModernTextBox txtLocation;
        private ModernTextBox txtShelf;
        private ComboBox cmbCategory;
        private ModernTextBox txtUom;
        private ModernTextBox txtBatch;
        private DateTimePicker dtpExpiry;

        private RadioButton rbProduct;
        private RadioButton rbService;
        private CheckBox chkSales;
        private CheckBox chkPurchase;
        private CheckBox chkInactive;

        private ComboBox cmbTaxRate;
        private CheckBox chkTrackStock;
        private ModernNumericUpDown numStock;
        private ModernNumericUpDown numLowLevel;

        private ComboBox cmbSupplier;
        private ModernNumericUpDown numCost;

        private ModernNumericUpDown[] numPrices = new ModernNumericUpDown[4];
        private TextBox[] txtGrosses = new TextBox[4];
        private TextBox[] txtProfits = new TextBox[4];

        private int? _editPartId = null;
        private string _currentImagePath = null;
        
        public AddProductServiceForm()
        {
            this.ClientSize = new Size(900, 800);
            this.TitleText = LocalizationManager.GetString("AddPart_TitleNew") ?? "Product / Service";
            
            InitializeUI();
            LoadDropdowns();
            
            SetFooterButtons(
                LocalizationManager.GetString("AddPart_Save") ?? "Save", 
                LocalizationManager.GetString("AddPart_Cancel") ?? "Cancel", 
                btnSave_Click, 
                btnCancel_Click
            );

            // Wire calculation events
            numCost.ValueChanged += CalculateMargins;
            for(int i=0; i<4; i++) {
                numPrices[i].ValueChanged += CalculateMargins;
            }

            rbProduct.CheckedChanged += (s, e) => { if (rbProduct.Checked) { chkTrackStock.Enabled = true; chkTrackStock.Checked = true; } };
            rbService.CheckedChanged += (s, e) => { if (rbService.Checked) { chkTrackStock.Checked = false; chkTrackStock.Enabled = false; } };
            chkTrackStock.CheckedChanged += (s, e) => { numStock.Enabled = chkTrackStock.Checked; numLowLevel.Enabled = chkTrackStock.Checked; };
        }

        private void InitializeUI()
        {
            this.ContentPanel.AutoScroll = true;
            TableLayoutPanel tlpMain = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, RowCount = 3, Padding = new Padding(15, 15, 30, 15), AutoSize = true };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // -- Left Pane (Image) --
            Panel pnlLeft = new Panel { Dock = DockStyle.Top, Margin = new Padding(0,0,10,10), Height = 280 };
            pbImage = new PictureBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke };
            btnUpload = new ModernButton { Text = "Upload Image", Dock = DockStyle.Bottom, Height = 35, Margin = new Padding(0,10,0,0) };
            btnUpload.Click += btnUpload_Click;
            pnlLeft.Controls.Add(pbImage);
            pnlLeft.Controls.Add(btnUpload);
            tlpMain.Controls.Add(pnlLeft, 0, 0);

            // -- Tax Rates (Left Pane, Row 1) --
            FlowLayoutPanel flpTax = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, Margin = new Padding(0,10,10,0), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            Label lblTax = new Label { Text = "Tax Rates:", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Margin = new Padding(0,0,0,5) };
            cmbTaxRate = new ComboBox { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            Panel pnlTax = ThemeConfig.WrapInStyledInput(cmbTaxRate, 35); pnlTax.Width = 260;
            flpTax.Controls.AddRange(new Control[] { lblTax, pnlTax });
            tlpMain.Controls.Add(flpTax, 0, 1);
            tlpMain.SetRowSpan(flpTax, 2);

            // -- Middle Pane (General Info) --
            FlowLayoutPanel flpMiddle = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = new Padding(10,0,10,0), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            int midW = 420; // Fixed width for forms
            txtName = new ModernTextBox { LabelText = "Name", Width = midW, Margin = new Padding(0,0,0,10) };
            txtDescription = new ModernTextBox { LabelText = "Description", Width = midW, Margin = new Padding(0,0,0,10) };
            
            FlowLayoutPanel flpSku = new FlowLayoutPanel { Width = midW, Height = 67, Margin = new Padding(0,0,0,10), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            txtSku = new ModernTextBox { LabelText = "SKU", Width = midW - 80, Margin = new Padding(0) };
            btnAutoSku = new ModernButton { Text = "Auto", Width = 70, Margin = new Padding(10,25,0,0) };
            flpSku.Controls.AddRange(new Control[] { txtSku, btnAutoSku });
            btnAutoSku.Click += (s, e) => { txtSku.Text = "SKU-" + DateTime.Now.ToString("yyMMddHHmmss"); };

            FlowLayoutPanel flpBarcode = new FlowLayoutPanel { Width = midW, Height = 67, Margin = new Padding(0,0,0,10), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            txtBarcode = new ModernTextBox { LabelText = "Barcode", Width = midW - 80, Margin = new Padding(0) };
            btnScanBarcode = new ModernButton { Text = "Scan", Width = 70, Margin = new Padding(10,25,0,0) };
            flpBarcode.Controls.AddRange(new Control[] { txtBarcode, btnScanBarcode });
            btnScanBarcode.Click += (s, e) => { MessageHelper.ShowInfo("Ready to scan..."); txtBarcode.Focus(); };
            
            Label lblCat = new Label { Text = "Category:", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cmbCategory = new ComboBox { Width = midW, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            Panel pnlCat = ThemeConfig.WrapInStyledInput(cmbCategory, 35); pnlCat.Width = midW; pnlCat.Margin = new Padding(0,0,0,10);
            
            txtUom = new ModernTextBox { LabelText = "Unit of Measure", Width = midW, Margin = new Padding(0,0,0,10) };
            txtBatch = new ModernTextBox { LabelText = "Batch No.", Width = midW, Margin = new Padding(0,0,0,10) };
            
            Label lblExp = new Label { Text = "Expiry Date:", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            dtpExpiry = new DateTimePicker { Width = midW, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10F), Margin = new Padding(0,0,0,10), ShowCheckBox = true, Checked = false };
            
            txtLocation = new ModernTextBox { LabelText = "Location", Width = midW, Margin = new Padding(0,0,0,10) };
            txtShelf = new ModernTextBox { LabelText = "Shelf", Width = midW, Margin = new Padding(0,0,0,10) };
            
            flpMiddle.Controls.AddRange(new Control[] { txtName, txtDescription, flpSku, flpBarcode, lblCat, pnlCat, txtUom, txtBatch, lblExp, dtpExpiry, txtLocation, txtShelf });
            tlpMain.Controls.Add(flpMiddle, 1, 0);

            // -- Right Pane (Flags) --
            FlowLayoutPanel flpRight = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, Margin = new Padding(10,0,0,0), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            GroupBox gbType = new GroupBox { Text = "Type", Width = 150, Height = 90, Margin = new Padding(0,0,0,15) };
            rbProduct = new RadioButton { Text = "Product", Checked = true, Location = new Point(15, 25) };
            rbService = new RadioButton { Text = "Service", Location = new Point(15, 55) };
            gbType.Controls.Add(rbProduct); gbType.Controls.Add(rbService);
            
            chkSales = new CheckBox { Text = "Sales item", Checked = true, Margin = new Padding(5,0,0,10) };
            chkPurchase = new CheckBox { Text = "Purchase item", Margin = new Padding(5,0,0,10) };
            chkInactive = new CheckBox { Text = "Inactive", Margin = new Padding(5,0,0,10) };
            
            flpRight.Controls.AddRange(new Control[] { gbType, chkSales, chkPurchase, chkInactive });
            tlpMain.Controls.Add(flpRight, 2, 0);

            // -- Middle Row 1: Stock & Supplier --
            FlowLayoutPanel flpStockSupp = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Margin = new Padding(10,0,0,0), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            
            GroupBox gbStock = new GroupBox { Text = "Stock control", Width = 310, Height = 120, Margin = new Padding(0,0,15,10) };
            chkTrackStock = new CheckBox { Text = "Control this item", Checked = true, Location = new Point(10, 20), AutoSize = true };
            numStock = new ModernNumericUpDown { LabelText = "Stock", Width = 135, Location = new Point(10, 50) };
            numLowLevel = new ModernNumericUpDown { LabelText = "Low level", Width = 135, Location = new Point(160, 50) };
            gbStock.Controls.AddRange(new Control[] { chkTrackStock, numStock, numLowLevel });
            
            GroupBox gbSupp = new GroupBox { Text = "Supplier", Width = 280, Height = 120, Margin = new Padding(0,0,0,10) };
            cmbSupplier = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            Panel pnlSupp = ThemeConfig.WrapInStyledInput(cmbSupplier, 35); pnlSupp.Location = new Point(10, 75); pnlSupp.Width = 130;
            Label lblSupp = new Label { Text = "Supplier Name", AutoSize = true, Location = new Point(10, 50), Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeConfig.TextColorDark };
            numCost = new ModernNumericUpDown { LabelText = "Cost", Width = 120, Location = new Point(150, 50), DecimalPlaces = 2, Maximum = 1000000 };
            gbSupp.Controls.AddRange(new Control[] { lblSupp, pnlSupp, numCost });

            flpStockSupp.Controls.AddRange(new Control[] { gbStock, gbSupp });
            tlpMain.Controls.Add(flpStockSupp, 1, 1);
            tlpMain.SetColumnSpan(flpStockSupp, 2);

            // -- Prices Grid --
            GroupBox gbPrices = new GroupBox { Text = "Prices", Dock = DockStyle.Top, Margin = new Padding(10,10,0,20), AutoSize = true };
            TableLayoutPanel tlpPrices = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, RowCount = 5, Padding = new Padding(10), AutoSize = true };
            tlpPrices.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpPrices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlpPrices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlpPrices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            
            tlpPrices.Controls.Add(new Label { Text = "Level", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 0, 0);
            tlpPrices.Controls.Add(new Label { Text = "Price", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 1, 0);
            tlpPrices.Controls.Add(new Label { Text = "Gross %", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 2, 0);
            tlpPrices.Controls.Add(new Label { Text = "Profit", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 3, 0);

            for (int i=0; i<4; i++) {
                tlpPrices.Controls.Add(new Label { Text = "Price " + (i+1), Anchor = AnchorStyles.Left }, 0, i+1);
                numPrices[i] = new ModernNumericUpDown { Width = 150, DecimalPlaces = 2, Maximum = 1000000, Margin = new Padding(2), Dock=DockStyle.Top };
                txtGrosses[i] = new TextBox { Width = 150, ReadOnly = true, Margin = new Padding(2,8,2,2), Font = new Font("Segoe UI", 10F), Dock=DockStyle.Top };
                txtProfits[i] = new TextBox { Width = 150, ReadOnly = true, Margin = new Padding(2,8,2,2), Font = new Font("Segoe UI", 10F), Dock=DockStyle.Top };
                
                tlpPrices.Controls.Add(numPrices[i], 1, i+1);
                tlpPrices.Controls.Add(txtGrosses[i], 2, i+1);
                tlpPrices.Controls.Add(txtProfits[i], 3, i+1);
            }
            gbPrices.Controls.Add(tlpPrices);
            tlpMain.Controls.Add(gbPrices, 1, 2);
            tlpMain.SetColumnSpan(gbPrices, 2);

            this.ContentPanel.Controls.Add(tlpMain);
        }

        private void LoadDropdowns()
        {
            try {
                // Categories
                var cats = CategoryData.GetAllCategories();
                cmbCategory.DisplayMember = "CategoryName"; cmbCategory.ValueMember = "CategoryName";
                cmbCategory.DataSource = cats;

                // Suppliers
                var sups = DatabaseHelper.ExecuteDataTable("SELECT id, supplier_name FROM suppliers WHERE date_deleted IS NULL");
                var dtSup = new System.Data.DataTable();
                dtSup.Columns.Add("id", typeof(int)); dtSup.Columns.Add("name", typeof(string));
                dtSup.Rows.Add(-1, "N/A");
                foreach(System.Data.DataRow r in sups.Rows) dtSup.Rows.Add(r["id"], r["supplier_name"]);
                cmbSupplier.DisplayMember = "name"; cmbSupplier.ValueMember = "id";
                cmbSupplier.DataSource = dtSup;

                // Taxes
                cmbTaxRate.Items.Add(new { Text = "Rate 1 N/A (0%)", Value = 0m });
                cmbTaxRate.Items.Add(new { Text = "Standard (15%)", Value = 15m });
                cmbTaxRate.Items.Add(new { Text = "Reduced (5%)", Value = 5m });
                cmbTaxRate.DisplayMember = "Text"; cmbTaxRate.ValueMember = "Value";
                cmbTaxRate.SelectedIndex = 0;
            } catch (Exception ex) { MessageHelper.ShowError("Error loading data: " + ex.Message); }
        }

        public void LoadPartData(PartData part)
        {
            if (part == null) return;
            _editPartId = part.Id;
            this.TitleText = LocalizationManager.GetString("AddPart_TitleEdit") ?? "Edit Product / Service";
            
            txtName.Text = part.PartName;
            txtDescription.Text = part.Description;
            txtSku.Text = part.PartNumber;
            txtBarcode.Text = part.Barcode;
            cmbCategory.Text = part.CategoryName;
            txtUom.Text = part.UnitOfMeasure;
            txtBatch.Text = part.BatchNumber;
            txtLocation.Text = part.Location;
            txtShelf.Text = part.Shelf;
            if (!string.IsNullOrEmpty(part.ExpiryDate) && DateTime.TryParse(part.ExpiryDate, out DateTime exp)) {
                dtpExpiry.Checked = true;
                dtpExpiry.Value = exp;
            } else { dtpExpiry.Checked = false; }

            if (part.ItemType == "Service") rbService.Checked = true; else rbProduct.Checked = true;
            chkSales.Checked = part.IsSalesItem;
            chkPurchase.Checked = part.IsPurchaseItem;
            chkInactive.Checked = part.IsInactive;

            for(int i=0; i<cmbTaxRate.Items.Count; i++) {
                dynamic item = cmbTaxRate.Items[i];
                if (item.Value == part.TaxRate) { cmbTaxRate.SelectedIndex = i; break; }
            }

            chkTrackStock.Checked = part.IsStockTracked;
            numStock.Value = part.QuantityInStock;
            numLowLevel.Value = part.MinimumStockLevel;

            if (part.SupplierId.HasValue) cmbSupplier.SelectedValue = part.SupplierId.Value;
            numCost.Value = part.PurchasePrice;

            numPrices[0].Value = part.SellingPrice;
            numPrices[1].Value = part.Price2;
            numPrices[2].Value = part.Price3;
            numPrices[3].Value = part.Price4;

            _currentImagePath = part.PartImage;
            UpdateImagePreview();
            CalculateMargins(null, null);
        }

        private void CalculateMargins(object sender, EventArgs e)
        {
            decimal cost = numCost.Value;
            for(int i=0; i<4; i++) {
                decimal price = numPrices[i].Value;
                decimal profit = price - cost;
                decimal gross = price > 0 ? (profit / price) * 100 : 0;
                
                txtProfits[i].Text = profit.ToString("N2");
                txtGrosses[i].Text = gross.ToString("N1") + "%";
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" };
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                try {
                    string assetsDir = System.IO.Path.Combine(Application.StartupPath, "Assets", "Products");
                    if(!System.IO.Directory.Exists(assetsDir)) System.IO.Directory.CreateDirectory(assetsDir);
                    string newName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(ofd.FileName);
                    string destPath = System.IO.Path.Combine(assetsDir, newName);
                    System.IO.File.Copy(ofd.FileName, destPath, true);
                    _currentImagePath = "Assets/Products/" + newName;
                    UpdateImagePreview();
                } catch(Exception ex) { MessageHelper.ShowError("Error uploading: " + ex.Message); }
            }
        }

        private void UpdateImagePreview()
        {
            if (string.IsNullOrEmpty(_currentImagePath)) { pbImage.Image = null; return; }
            try {
                string fullPath = System.IO.Path.Combine(Application.StartupPath, _currentImagePath);
                if (System.IO.File.Exists(fullPath)) {
                    using (var ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(fullPath))) {
                        var old = pbImage.Image; pbImage.Image = null; if (old != null) old.Dispose();
                        pbImage.Image = System.Drawing.Image.FromStream(ms);
                    }
                }
            } catch { pbImage.Image = null; }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageHelper.ShowWarning("Please enter a name."); return; }

            PartData p = new PartData {
                Id = _editPartId ?? 0,
                PartName = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Barcode = txtBarcode.Text.Trim(),
                PartNumber = txtSku.Text.Trim(),
                CategoryName = cmbCategory.Text,
                UnitOfMeasure = txtUom.Text.Trim(),
                BatchNumber = txtBatch.Text.Trim(),
                Location = txtLocation.Text.Trim(),
                Shelf = txtShelf.Text.Trim(),
                ExpiryDate = dtpExpiry.Checked ? dtpExpiry.Value.ToString("yyyy-MM-dd") : "",
                ItemType = rbService.Checked ? "Service" : "Product",
                IsSalesItem = chkSales.Checked,
                IsPurchaseItem = chkPurchase.Checked,
                IsInactive = chkInactive.Checked,
                TaxRate = (decimal)((dynamic)cmbTaxRate.SelectedItem).Value,
                IsStockTracked = chkTrackStock.Checked,
                QuantityInStock = (int)numStock.Value,
                MinimumStockLevel = (int)numLowLevel.Value,
                PurchasePrice = numCost.Value,
                SellingPrice = numPrices[0].Value,
                Price2 = numPrices[1].Value,
                Price3 = numPrices[2].Value,
                Price4 = numPrices[3].Value,
                PartImage = _currentImagePath,
                Status = chkInactive.Checked ? "Inactive" : "Active"
            };

            if (cmbSupplier.SelectedValue != null && (int)cmbSupplier.SelectedValue != -1)
                p.SupplierId = (int)cmbSupplier.SelectedValue;

            try {
                new InventoryService().SaveProductService(p);
                this.DialogResult = DialogResult.OK;
                this.Close();
            } catch (Exception ex) { MessageHelper.ShowError(ex.Message); }
        }

        private void btnCancel_Click(object sender, EventArgs e) { this.DialogResult = DialogResult.Cancel; this.Close(); }
    }
}
