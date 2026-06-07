using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using InventorySystem.Data;
using InventorySystem.Controls;
using InventorySystem.Services;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
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
        private ModernComboBox cmbCategory;
        private ModernTextBox txtUom;
        private ModernTextBox txtBatch;
        private FlatDateTimePicker dtpExpiry;

        private RadioButton rbProduct;
        private RadioButton rbService;
        private CheckBox chkSales;
        private CheckBox chkPurchase;
        private CheckBox chkInactive;

        private ModernComboBox cmbTaxRate;
        private CheckBox chkTrackStock;
        private ModernNumericUpDown numStock;
        private ModernNumericUpDown numLowLevel;

        private ModernComboBox cmbSupplier;
        private ModernNumericUpDown numCost;

        private ModernNumericUpDown[] numPrices = new ModernNumericUpDown[4];
        private ModernTextBox[] txtGrosses = new ModernTextBox[4];
        private ModernTextBox[] txtProfits = new ModernTextBox[4];

        private int? _editPartId = null;
        private string _currentImagePath = null;
        
        public AddProductServiceForm()
        {
            this.ClientSize = new Size(1050, 800);
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
            TableLayoutPanel tlpMain = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 3, Padding = new Padding(15, 15, 15, 15), AutoSize = true };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // -- Left Pane --
            FlowLayoutPanel flpLeft = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = new Padding(0,0,10,0), AutoSize = true };
            
            // Image
            Panel pnlLeft = new Panel { Width = 280, Height = 280, Margin = new Padding(0,0,0,15) };
            pbImage = new PictureBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = ThemeConfig.BackgroundColor };
            btnUpload = new ModernButton { Text = "Upload Image", Dock = DockStyle.Bottom, Height = 35, Margin = new Padding(0,10,0,0) };
            btnUpload.Click += btnUpload_Click;
            ThemeConfig.ApplyPrimaryButton(btnUpload);
            pnlLeft.Controls.Add(pbImage);
            pnlLeft.Controls.Add(btnUpload);
            flpLeft.Controls.Add(pnlLeft);

            // Type
            GroupBox gbType = new GroupBox { Text = "Type", Width = 280, Height = 60, Margin = new Padding(0,0,0,15) };
            rbProduct = new RadioButton { Text = "Product", Checked = true, Location = new Point(15, 25), AutoSize = true };
            rbService = new RadioButton { Text = "Service", Location = new Point(120, 25), AutoSize = true };
            gbType.Controls.Add(rbProduct); gbType.Controls.Add(rbService);
            flpLeft.Controls.Add(gbType);

            // Settings
            GroupBox gbSettings = new GroupBox { Text = "Settings", Width = 280, Height = 100, Margin = new Padding(0,0,0,15) };
            chkSales = new CheckBox { Text = "Sales item", Checked = true, Location = new Point(15, 25), AutoSize = true };
            chkPurchase = new CheckBox { Text = "Purchase item", Location = new Point(15, 50), AutoSize = true };
            chkInactive = new CheckBox { Text = "Inactive", Location = new Point(15, 75), AutoSize = true };
            gbSettings.Controls.AddRange(new Control[] { chkSales, chkPurchase, chkInactive });
            flpLeft.Controls.Add(gbSettings);

            // Tax
            cmbTaxRate = new ModernComboBox { LabelText = "Tax Rates:", Width = 280, Margin = new Padding(0,0,0,15), DropDownStyle = ComboBoxStyle.DropDownList };
            flpLeft.Controls.Add(cmbTaxRate);

            // Category
            cmbCategory = new ModernComboBox { LabelText = "Category", Width = 280, Margin = new Padding(0,0,0,10), DropDownStyle = ComboBoxStyle.DropDownList };
            flpLeft.Controls.Add(cmbCategory);

            // Expiry Date
            FlowLayoutPanel flpExp = new FlowLayoutPanel { Width = 280, Height = 67, Margin = new Padding(0,0,0,10), FlowDirection = FlowDirection.TopDown };
            Label lblExp = new Label { Text = "Expiry Date", AutoSize = true, Font = ThemeConfig.SmallBoldFont ?? new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeConfig.TextColorDark };
            dtpExpiry = new FlatDateTimePicker { Width = 270, Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
            flpExp.Controls.Add(lblExp); flpExp.Controls.Add(dtpExpiry);
            flpLeft.Controls.Add(flpExp);

            tlpMain.Controls.Add(flpLeft, 0, 0);
            tlpMain.SetRowSpan(flpLeft, 3);

            // -- Right Pane (General Info) --
            FlowLayoutPanel flpMiddle = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Margin = new Padding(10,0,0,0), AutoSize = true };
            int fullW = 690; // 690 fits Name/Desc perfectly inside 720
            int halfW = 340; // 340 * 2 + 10 margin = 690
            
            txtName = new ModernTextBox { LabelText = "Name", Width = fullW, Margin = new Padding(0,0,20,10) };
            txtDescription = new ModernTextBox { LabelText = "Description", Width = fullW, Margin = new Padding(0,0,20,10) };
            
            FlowLayoutPanel flpSku = new FlowLayoutPanel { Width = halfW, Height = 67, Margin = new Padding(0,0,10,10), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            txtSku = new ModernTextBox { LabelText = "SKU", Width = halfW - 75, Margin = new Padding(0) };
            btnAutoSku = new ModernButton { Text = "Auto", Width = 65, Height = 35, Margin = new Padding(10,25,0,0) };
            ThemeConfig.ApplyPrimaryButton(btnAutoSku);
            flpSku.Controls.AddRange(new Control[] { txtSku, btnAutoSku });
            btnAutoSku.Click += (s, e) => 
            { 
                string cat = cmbCategory.Text.Trim();
                if (string.IsNullOrEmpty(cat)) cat = "GEN";
                string name = txtName.Text.Trim();
                if (string.IsNullOrEmpty(name)) name = "PRD";
                
                string catPrefix = cat.Length >= 3 ? cat.Substring(0, 3).ToUpper() : cat.ToUpper().PadRight(3, 'X');
                string namePrefix = name.Length >= 3 ? name.Substring(0, 3).ToUpper() : name.ToUpper().PadRight(3, 'X');
                
                txtSku.Text = $"{catPrefix}-{namePrefix}-{DateTime.Now.ToString("yyMMddHHmm")}"; 
            };

            FlowLayoutPanel flpBarcode = new FlowLayoutPanel { Width = halfW, Height = 67, Margin = new Padding(0,0,10,10), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            txtBarcode = new ModernTextBox { LabelText = "Barcode", Width = halfW - 75, Margin = new Padding(0) };
            btnScanBarcode = new ModernButton { Text = "Scan", Width = 65, Height = 35, Margin = new Padding(10,25,0,0) };
            ThemeConfig.ApplyPrimaryButton(btnScanBarcode);
            flpBarcode.Controls.AddRange(new Control[] { txtBarcode, btnScanBarcode });
            btnScanBarcode.Click += (s, e) => { MessageHelper.ShowInfo("Ready to scan..."); txtBarcode.Focus(); };
            
            txtBatch = new ModernTextBox { LabelText = "Batch No.", Width = halfW, Margin = new Padding(0,0,10,10) };
            txtLocation = new ModernTextBox { LabelText = "Location", Width = halfW, Margin = new Padding(0,0,10,10) };
            txtShelf = new ModernTextBox { LabelText = "Shelf", Width = halfW, Margin = new Padding(0,0,10,10) };
            txtUom = new ModernTextBox { LabelText = "Unit of Measure", Width = halfW, Margin = new Padding(0,0,10,10) };
            
            flpMiddle.Controls.AddRange(new Control[] { txtName, txtDescription, flpSku, flpBarcode, txtBatch, txtLocation, txtShelf, txtUom });
            tlpMain.Controls.Add(flpMiddle, 1, 0);

            // -- Right Pane Row 1: Stock & Supplier --
            FlowLayoutPanel flpStockSupp = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Margin = new Padding(10,0,0,0), AutoSize = true };
            
            GroupBox gbStock = new GroupBox { Text = "Stock control", Width = halfW, Height = 120, Margin = new Padding(0,0,10,10) };
            chkTrackStock = new CheckBox { Text = "Control this item", Checked = true, Location = new Point(10, 20), AutoSize = true };
            numStock = new ModernNumericUpDown { LabelText = "Stock", Width = (halfW / 2) - 15, Location = new Point(10, 45) };
            numLowLevel = new ModernNumericUpDown { LabelText = "Low level", Width = (halfW / 2) - 15, Location = new Point((halfW / 2) + 5, 45) };
            gbStock.Controls.AddRange(new Control[] { chkTrackStock, numStock, numLowLevel });
            
            GroupBox gbSupp = new GroupBox { Text = "Supplier Cost", Width = halfW, Height = 120, Margin = new Padding(0,0,10,10) };
            cmbSupplier = new ModernComboBox { LabelText = "Supplier", Width = (halfW / 2) - 15, Location = new Point(10, 45), DropDownStyle = ComboBoxStyle.DropDownList };
            numCost = new ModernNumericUpDown { LabelText = "Cost", Width = (halfW / 2) - 15, Location = new Point((halfW / 2) + 5, 45), DecimalPlaces = 2, Maximum = 1000000 };
            gbSupp.Controls.AddRange(new Control[] { cmbSupplier, numCost });

            flpStockSupp.Controls.AddRange(new Control[] { gbStock, gbSupp });
            tlpMain.Controls.Add(flpStockSupp, 1, 1);

            // -- Prices Grid --
            GroupBox gbPrices = new GroupBox { Text = "Prices", Dock = DockStyle.Top, Margin = new Padding(10,0,20,20), AutoSize = true };
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
                numPrices[i] = new ModernNumericUpDown { Width = 150, DecimalPlaces = 2, Maximum = 1000000, Margin = new Padding(2), Dock=DockStyle.Fill, ShowLabel=false };
                txtGrosses[i] = new ModernTextBox { Width = 150, ReadOnly = true, ShowLabel = false, Margin = new Padding(2), Dock=DockStyle.Fill };
                txtProfits[i] = new ModernTextBox { Width = 150, ReadOnly = true, ShowLabel = false, Margin = new Padding(2), Dock=DockStyle.Fill };
                
                tlpPrices.Controls.Add(numPrices[i], 1, i+1);
                tlpPrices.Controls.Add(txtGrosses[i], 2, i+1);
                tlpPrices.Controls.Add(txtProfits[i], 3, i+1);
            }
            gbPrices.Controls.Add(tlpPrices);
            tlpMain.Controls.Add(gbPrices, 1, 2);

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
                ExpiryDate = dtpExpiry.Checked ? dtpExpiry.Value.Value.ToString("yyyy-MM-dd") : "",
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
