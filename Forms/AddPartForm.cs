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

        public AddPartForm()
        {
            InitializeComponent();
            // Sizing is handled automatically by BaseModalForm.OnLoad
            SetFooterButtons(
                LocalizationManager.GetString("AddPart_Save"), 
                LocalizationManager.GetString("AddPart_Cancel"), 
                btnSave_Click, 
                btnCancel_Click
            );
            
            this.TitleText = LocalizationManager.GetString("AddPart_TitleNew");
            ApplyTheme();
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;
            
            // Disable MouseWheel on NumericUpDowns to prevent accidental scrolling
            numQuantity.MouseWheel += PreventNumericScroll;
            numPrice.MouseWheel += PreventNumericScroll;
            numMinStock.MouseWheel += PreventNumericScroll;
        }

        private void PreventNumericScroll(object sender, MouseEventArgs e)
        {
            // Handled = true stops the built-in numeric scrolling
            if (e is HandledMouseEventArgs handledArgs)
            {
                handledArgs.Handled = true;
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeConfig.SurfaceColor;
            
            ThemeConfig.ApplyComboBoxStyle(cmbCategory);
            ThemeConfig.ApplyComboBoxStyle(cmbStatus);

            ThemeConfig.ApplyPrimaryButton(btnScan);
            ThemeConfig.ApplySecondaryButton(btnUpload);
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
            
            if(!string.IsNullOrEmpty(imagePath))
            {
                 try
                 {
                     string fullPath = System.IO.Path.Combine(Application.StartupPath, imagePath); 
                     if(System.IO.File.Exists(fullPath))
                     {
                         // Use MemoryStream to avoid file locking
                         byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
                         using (var ms = new System.IO.MemoryStream(bytes))
                         {
                             if(pbImage.Image != null) pbImage.Image.Dispose();
                             pbImage.Image = System.Drawing.Image.FromStream(ms);
                         }
                     }
                 }
                 catch { /* Silently fail and use placeholder / empty */ }
            }
        }

        private bool _isCategoryLoading = false;
        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isCategoryLoading || cmbCategory.DataSource == null) return;

            if (cmbCategory.SelectedItem is CategoryData cat && cat.Id == -1)
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
                        // Reset to first item
                        if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;
                    }
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
                    CategoryName = LocalizationManager.IsArabic ? "+ إضافة فئة جديدة..." : "+ Add New Category..." 
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

            try
            {
                string name = txtPartName.Text.Trim();
                string number = partNum;
                int qty = (int)numQuantity.Value;
                decimal price = numPrice.Value;
                string status = cmbStatus.SelectedItem?.ToString() ?? "Active";
                string barcode = txtBarcode.Text.Trim();
                string location = txtLocation.Text.Trim();
                string shelf = txtShelf.Text.Trim();
                string category = cmbCategory.Text; // Use Text to allow new categories or typed ones
                
                string image = _currentImagePath; 
                if(string.IsNullOrEmpty(image)) image = null;

                // Instantiate service locally or via property if available (PartsForm has it, but this is a different form)
                InventoryService service = new InventoryService();

                _minStock = (int)numMinStock.Value;
                
                if (EditPartId == null)
                {
                    // ADD
                   service.AddPart(name, number, category, qty, price, _minStock, image, barcode, location, shelf, status);
                   MessageHelper.ShowSuccess(LocalizationManager.IsArabic ? "تم إضافة الصنف بنجاح!" : "Part added successfully!");
                }
                else
                {
                    // UPDATE
                    service.UpdatePart(EditPartId.Value, name, number, category, price, qty, _minStock, image, barcode, location, shelf, status);
                    MessageHelper.ShowSuccess(LocalizationManager.IsArabic ? "تم تحديث الصنف بنجاح!" : "Part updated successfully!");
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
                    // Copy to Assets/Products
                    string assetsDir = System.IO.Path.Combine(Application.StartupPath, "Assets", "Products");
                    if(!System.IO.Directory.Exists(assetsDir)) System.IO.Directory.CreateDirectory(assetsDir);
                    
                    string ext = System.IO.Path.GetExtension(ofd.FileName);
                    string newName = Guid.NewGuid().ToString() + ext;
                    string destPath = System.IO.Path.Combine(assetsDir, newName);
                    
                    System.IO.File.Copy(ofd.FileName, destPath, true);
                    
                    // Show preview using MemoryStream to avoid file locking
                    byte[] bytes = System.IO.File.ReadAllBytes(destPath);
                    using (var ms = new System.IO.MemoryStream(bytes))
                    {
                        if(pbImage.Image != null) pbImage.Image.Dispose();
                        pbImage.Image = System.Drawing.Image.FromStream(ms);
                    }
                    
                    // Store relative path "Assets/Products/filename.ext"
                    _currentImagePath = "Assets/Products/" + newName;
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
            MessageHelper.ShowInfo(LocalizationManager.IsArabic ? "جاهز للمسح! يرجى استخدام ماسح الباركود الآن." : "Ready to scan! Please use your barcode scanner now.");
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

            // Labels
            string currSymbol = GenericInventorySystem.Services.CurrencyService.GetSymbol("USD");
            lblQuantity.Text = LocalizationManager.GetString("AddPart_Stock");
            lblMinStock.Text = LocalizationManager.GetString("AddPart_MinStock");
            lblPrice.Text = string.Format(LocalizationManager.GetString("AddPart_Price"), "USD", currSymbol);
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
        }
    }
}
