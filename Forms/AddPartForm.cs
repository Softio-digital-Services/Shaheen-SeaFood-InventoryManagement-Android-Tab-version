using System;
using System.Windows.Forms;
using GenericInventorySystem.Data;
using GenericInventorySystem;
using GenericInventorySystem.Services;
using GenericInventorySystem.Helpers; // Also good to have explicit

namespace GenericInventorySystem.Forms
{
    public partial class AddPartForm : BaseModalForm
    {
        public int? EditPartId { get; set; } = null;
        private int _minStock = 5; // Default

        private EventHandler langHandler;

        public AddPartForm()
        {
            InitializeComponent();
            
            langHandler = (s, e) => ApplyLocalization();
            LocalizationManager.LanguageChanged += langHandler;

            SetFooterButtons(
                LocalizationManager.GetString("AddPart_Save"), 
                LocalizationManager.GetString("AddPart_Cancel"), 
                btnSave_Click, 
                btnCancel_Click
            );
            
            this.TitleText = LocalizationManager.GetString("AddPart_TitleNew");
            ApplyTheme();
            ApplyLocalization();
            cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;
            btnAutoSKU.Click += BtnAutoSKU_Click;

            this.Disposed += (s, e) => {
                LocalizationManager.LanguageChanged -= langHandler;
            };
        }

        private void BtnAutoSKU_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPartName.Text))
            {
                MessageHelper.ShowWarning("Please enter product name first.");
                return;
            }

            BarcodeService barcodeService = new BarcodeService();
            string suggestedSku = barcodeService.GenerateSKU(cmbCategory.Text, txtPartName.Text);
            txtPartNumber.Text = suggestedSku;
        }



        private void ApplyTheme()
        {
            this.BackColor = ThemeConfig.SurfaceColor;
            
            ThemeConfig.ApplyComboBoxStyle(cmbCategory);
            ThemeConfig.ApplyComboBoxStyle(cmbStatus);

            ThemeConfig.ApplyPrimaryButton(btnScan);
            ThemeConfig.ApplySecondaryButton(btnUpload);
            ThemeConfig.ApplySecondaryButton(btnAutoSKU);
        }


        public void LoadPartData(string id, string name, string number, int qty, decimal price, int minStock, string status, string barcode, string location, string shelf, string imagePath, string category)
        {
            EditPartId = int.Parse(id);
            _minStock = minStock;
            
            this.TitleText = LocalizationManager.GetString("AddPart_TitleEdit");
            SetFooterButtons(
                LocalizationManager.GetString("AddPart_Update"), 
                LocalizationManager.GetString("AddPart_Cancel"), 
                btnSave_Click, 
                btnCancel_Click
            );
            
            txtPartName.Text = name;
            txtPartNumber.Text = number;
            numQuantity.Value = qty;
            numMinStock.Value = minStock;
            numPrice.Value = price;
            cmbStatus.SelectedItem = status;
            txtBarcode.Text = barcode;
            txtLocation.Text = location;
            txtShelf.Text = shelf;
            
            // Handle Category Selection
            int index = cmbCategory.FindStringExact(category);
            if (index >= 0)
            {
                cmbCategory.SelectedIndex = index;
            }
            else
            {
                cmbCategory.SelectedIndex = -1;
                cmbCategory.Text = category; // Allow custom text
            }

            _currentImagePath = imagePath;
            UpdateImagePreview(imagePath);
        }

        private void UpdateImagePreview(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                // If product has no image, try getting it from category
                if (cmbCategory.SelectedItem is CategoryData cat && !string.IsNullOrEmpty(cat.CategoryImage))
                {
                    imagePath = cat.CategoryImage;
                }
                else
                {
                    pbImage.Image = null;
                    return;
                }
            }

            try
            {
                string fullPath = System.IO.Path.Combine(Application.StartupPath, imagePath);
                if (System.IO.File.Exists(fullPath))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
                    using (var ms = new System.IO.MemoryStream(bytes))
                    {
                        var oldImg = pbImage.Image;
                        pbImage.Image = null;
                        if (oldImg != null) oldImg.Dispose();
                        pbImage.Image = System.Drawing.Image.FromStream(ms);
                    }
                }
            }
            catch { pbImage.Image = null; }
        }

        private bool _isCategoryLoading = false;
        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isCategoryLoading || cmbCategory.DataSource == null) return;

            if (cmbCategory.SelectedItem is CategoryData cat)
            {
                // Toggle visibility based on category
                bool isService = cat.CategoryName.Equals("Services", StringComparison.OrdinalIgnoreCase) || 
                                 cat.CategoryName.Equals("Service", StringComparison.OrdinalIgnoreCase);
                
                numQuantity.Visible = !isService;
                numMinStock.Visible = !isService;

                if (cat.Id == -1)
                {
                    using (var f = new AddCategoryForm())
                    {
                        if (f.ShowDialog() == DialogResult.OK)
                        {
                            string newCat = f.NewCategoryName;
                            LoadCategories();
                            
                            // Select the new one
                            int idx = -1;
                            for(int i=0; i<cmbCategory.Items.Count; i++) {
                                if ((cmbCategory.Items[i] as CategoryData)?.CategoryName == newCat) { idx = i; break; }
                            }
                            if (idx >= 0) cmbCategory.SelectedIndex = idx;
                        }
                        else
                        {
                            if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;
                        }
                    }
                }
                else if (string.IsNullOrEmpty(_currentImagePath))
                {
                    UpdateImagePreview(null);
                }
            }
        }
        private void LoadCategories()
        {
            _isCategoryLoading = true;
            try
            {
                var categories = CategoryData.GetAllCategories();
                
                // Add "Add New" item
                var addNew = new CategoryData { 
                    Id = -1, 
                    CategoryName = "+ Add New Category..." 
                };
                categories.Add(addNew);

                cmbCategory.DataSource = null;
                cmbCategory.DisplayMember = "CategoryName";
                cmbCategory.ValueMember = "CategoryName";
                cmbCategory.DataSource = categories;
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError("Error loading categories: " + ex.Message);
            }
            finally
            {
                _isCategoryLoading = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadCategories();
        }

        public void SetBarcode(string barcode)
        {
            txtBarcode.Text = barcode;
            // Focus part name so user can start typing name immediately after scan
            this.ActiveControl = txtPartName;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.ValidateRequiredFields(txtPartName)) return;

            // Validation - SKU is a free-text field (e.g. OIL-001), not a number
            string partNum = txtPartNumber.Text.Trim();
            string barcode = txtBarcode.Text.Trim();

            // Auto-generate SKU if empty
            if (string.IsNullOrWhiteSpace(partNum))
            {
                BarcodeService barcodeService = new BarcodeService();
                partNum = barcodeService.GenerateSKU(cmbCategory.Text, txtPartName.Text);
                txtPartNumber.Text = partNum;
            }

            // Instantiate service locally or via property if available
            InventoryService service = new InventoryService();

            if (!string.IsNullOrWhiteSpace(barcode))
            {
                if (service.BarcodeExists(barcode, EditPartId))
                {
                    MessageHelper.ShowWarning(LocalizationManager.IsArabic 
                        ? "هذا الباركود موجود بالفعل في النظام." 
                        : "This barcode already exists in the system.");
                    return;
                }
            }

            try
            {
                string name = txtPartName.Text.Trim();
                string number = partNum;
                int qty = (int)numQuantity.Value;
                decimal price = numPrice.Value;
                string rawStatus = cmbStatus.SelectedItem?.ToString() ?? "Active";
                string status = (rawStatus == LocalizationManager.GetString("Status_Active") || rawStatus == "Active") ? "Active" : "Inactive";
                
                string location = txtLocation.Text.Trim();
                string shelf = txtShelf.Text.Trim();
                string category = cmbCategory.Text; 
                
                string image = _currentImagePath; 
                if(string.IsNullOrEmpty(image)) image = null;


                _minStock = (int)numMinStock.Value;
                
                if (EditPartId == null)
                {
                    // ADD
                   service.AddPart(name, number, category, qty, price, _minStock, image, barcode, location, shelf, status);
                   _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Item '{name}' added via WinForms");
                   MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_PartAdded"));
                }
                else
                {
                    // UPDATE
                    service.UpdatePart(EditPartId.Value, name, number, category, price, qty, _minStock, image, barcode, location, shelf, status);
                    _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Item '{name}' updated via WinForms");
                    MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_PartUpdated"));
                }
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError("Error saving part: " + ex.Message);
            }
        }

        private string _currentImagePath = "";

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string assetsDir = System.IO.Path.Combine(Application.StartupPath, "Assets", "Products");
                    if(!System.IO.Directory.Exists(assetsDir)) System.IO.Directory.CreateDirectory(assetsDir);
                    
                    string ext = System.IO.Path.GetExtension(ofd.FileName);
                    string newName = Guid.NewGuid().ToString() + ext;
                    string destPath = System.IO.Path.Combine(assetsDir, newName);
                    
                    System.IO.File.Copy(ofd.FileName, destPath, true);
                    
                    _currentImagePath = "Assets/Products/" + newName;
                    UpdateImagePreview(_currentImagePath);
                }
                catch(Exception ex)
                {
                    MessageHelper.ShowError("Error uploading image: " + ex.Message);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void btnScan_Click(object sender, EventArgs e)
        {
            txtBarcode.Text = "";
            txtBarcode.Focus();
            MessageHelper.ShowInfo("Ready to scan! Please use your barcode scanner now.");
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            // Form Title
            this.TitleText = EditPartId == null ? LocalizationManager.GetString("AddPart_TitleNew") : LocalizationManager.GetString("AddPart_TitleEdit");

            // ModernTextBox Labels
            txtBarcode.LabelText = LocalizationManager.GetString("AddPart_Barcode");
            txtPartName.LabelText = LocalizationManager.GetString("AddPart_Product");
            txtPartNumber.LabelText = LocalizationManager.GetString("AddPart_SKU");
            txtLocation.LabelText = LocalizationManager.GetString("AddPart_Location");
            txtShelf.LabelText = LocalizationManager.GetString("AddPart_Shelf");
            btnAutoSKU.Text = isArabic ? "\u2728 \u062A\u0644\u0642\u0627\u0626\u064A" : "\u2728 Auto";

            // Modern Numeric Inputs
            string currSymbol = GenericInventorySystem.Services.CurrencyService.GetSymbol("USD");
            numQuantity.LabelText = LocalizationManager.GetString("AddPart_Stock");
            numMinStock.LabelText = LocalizationManager.GetString("AddPart_MinStock");
            numPrice.LabelText = string.Format(LocalizationManager.GetString("AddPart_Price"), "USD", currSymbol);

            lblCategory.Text = LocalizationManager.GetString("AddPart_Category");
            lblStatus.Text = LocalizationManager.GetString("AddPart_Status");

            // Buttons are handled via SetFooterButtons, but we update text here for lang changes
            SetFooterButtons(
                EditPartId == null ? LocalizationManager.GetString("AddPart_Save") : LocalizationManager.GetString("AddPart_Update"),
                LocalizationManager.GetString("AddPart_Cancel"),
                btnSave_Click,
                btnCancel_Click
            );

            // Dropdown items translation
            string currentStatus = cmbStatus.SelectedItem?.ToString();
            cmbStatus.Items.Clear();
            if (isArabic)
            {
                cmbStatus.Items.AddRange(new object[] { LocalizationManager.GetString("Status_Active"), LocalizationManager.GetString("Status_Inactive") });
                if (currentStatus == "Active" || currentStatus == LocalizationManager.GetString("Status_Active")) cmbStatus.SelectedIndex = 0;
                else if (currentStatus == "Inactive" || currentStatus == LocalizationManager.GetString("Status_Inactive")) cmbStatus.SelectedIndex = 1;
                else cmbStatus.SelectedIndex = 0;
            }
            else
            {
                cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" });
                if (currentStatus == LocalizationManager.GetString("Status_Active") || currentStatus == "Active") cmbStatus.SelectedIndex = 0;
                else if (currentStatus == LocalizationManager.GetString("Status_Inactive") || currentStatus == "Inactive") cmbStatus.SelectedIndex = 1;
                else cmbStatus.SelectedIndex = 0;
            }

            // Ensure visibility is correct for current selection
            if (cmbCategory.SelectedItem is CategoryData selectedCat)
            {
                bool isService = selectedCat.CategoryName.Equals("Services", StringComparison.OrdinalIgnoreCase);
                numQuantity.Visible = !isService;
                numMinStock.Visible = !isService;
            }
        }
    }
}
