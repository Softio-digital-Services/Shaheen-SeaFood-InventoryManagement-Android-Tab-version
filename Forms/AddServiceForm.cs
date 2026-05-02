using System;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Services;

namespace GenericInventorySystem.Forms
{
    public partial class AddServiceForm : BaseModalForm
    {
        private InventoryService _inventoryService;
        private EventHandler langHandler;
        private string _editingPartId = null;
        private string _currentImagePath = "";

        public AddServiceForm()
        {
            InitializeComponent();
            _inventoryService = new InventoryService();

            langHandler = (s, e) => ApplyLocalization();
            LocalizationManager.LanguageChanged += langHandler;

            SetFooterButtons(
                LocalizationManager.GetString("AddPart_Save"), 
                LocalizationManager.GetString("AddPart_Cancel"), 
                btnSave_Click, 
                btnCancel_Click
            );

            this.TitleText = LocalizationManager.GetString("Parts_AddService");
            ApplyTheme();
            ApplyLocalization();

            btnAutoSKU.Click += BtnAutoSKU_Click;
            btnUpload.Click += btnUpload_Click;

            this.Disposed += (s, e) => {
                LocalizationManager.LanguageChanged -= langHandler;
            };
        }

        private void BtnAutoSKU_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageHelper.ShowWarning("Please enter service name first.");
                return;
            }

            BarcodeService barcodeService = new BarcodeService();
            string suggestedSku = barcodeService.GenerateSKU("Services", txtName.Text);
            txtCode.Text = suggestedSku;
        }

        public void LoadServiceData(string id, string name, string sku, decimal price, string status, string image)
        {
            _editingPartId = id;
            this.TitleText = "Edit Service";
            
            txtName.Text = name;
            txtCode.Text = sku;
            numPrice.Value = price;
            
            _currentImagePath = image;
            UpdateImagePreview(image);

            // Set status
            bool isArabic = LocalizationManager.IsArabic;
            if (status == "Active") cmbStatus.SelectedIndex = 0;
            else cmbStatus.SelectedIndex = 1;

            SetFooterButtons(
                "Update Service",
                LocalizationManager.GetString("AddPart_Cancel"),
                btnSave_Click,
                btnCancel_Click
            );
        }

        private void UpdateImagePreview(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                // Try getting image from "Services" category
                try {
                    object catImg = DatabaseHelper.ExecuteScalar<object>("SELECT category_image FROM categories WHERE category_name = 'Services'");
                    if (catImg != null && catImg != DBNull.Value && !string.IsNullOrEmpty(catImg.ToString()))
                    {
                        imagePath = catImg.ToString();
                    }
                } catch { }
            }

            if (string.IsNullOrEmpty(imagePath))
            {
                pbImage.Image = null;
                return;
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

        private void ApplyTheme()
        {
            this.BackColor = ThemeConfig.SurfaceColor;
            ThemeConfig.ApplyComboBoxStyle(cmbStatus);
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            this.TitleText = _editingPartId == null ? LocalizationManager.GetString("Parts_AddService") : ("Edit Service");
            txtName.LabelText = LocalizationManager.GetString("AddPart_Product") + " / " + (LocalizationManager.GetString("Msg_ServiceName"));
            txtCode.LabelText = LocalizationManager.GetString("AddPart_SKU");
            btnAutoSKU.Text = isArabic ? "\u2728 \u062A\u0644\u0642\u0627\u0626\u064A" : "\u2728 Auto";
            btnUpload.Text = LocalizationManager.GetString("AddPart_Upload");

            string currSymbol = GenericInventorySystem.Services.CurrencyService.GetSymbol("USD");
            numPrice.LabelText = string.Format(LocalizationManager.GetString("AddPart_Price"), "USD", currSymbol);
            lblStatus.Text = LocalizationManager.GetString("AddPart_Status");

            string saveText = _editingPartId == null ? LocalizationManager.GetString("AddPart_Save") : ("Update Service");

            SetFooterButtons(
                saveText,
                LocalizationManager.GetString("AddPart_Cancel"),
                btnSave_Click,
                btnCancel_Click
            );

            // Status dropdown
            string currentStatus = cmbStatus.SelectedItem?.ToString();
            cmbStatus.Items.Clear();
            if (isArabic)
            {
                cmbStatus.Items.AddRange(new object[] { LocalizationManager.GetString("Status_Active"), LocalizationManager.GetString("Status_Inactive") });
                cmbStatus.SelectedIndex = (currentStatus == "Inactive" || currentStatus == LocalizationManager.GetString("Status_Inactive")) ? 1 : 0;
            }
            else
            {
                cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" });
                cmbStatus.SelectedIndex = (currentStatus == LocalizationManager.GetString("Status_Inactive") || currentStatus == "Inactive") ? 1 : 0;
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string ext = System.IO.Path.GetExtension(ofd.FileName);
                        string fileName = "service_" + Guid.NewGuid().ToString().Substring(0, 8) + ext;
                        string targetDir = System.IO.Path.Combine(Application.StartupPath, "Assets", "Products");
                        
                        if (!System.IO.Directory.Exists(targetDir))
                            System.IO.Directory.CreateDirectory(targetDir);

                        string targetPath = System.IO.Path.Combine(targetDir, fileName);
                        System.IO.File.Copy(ofd.FileName, targetPath, true);

                        _currentImagePath = "Assets/Products/" + fileName;
                        UpdateImagePreview(_currentImagePath);
                    }
                    catch (Exception ex)
                    {
                        MessageHelper.ShowError("Error uploading image: " + ex.Message);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.ValidateRequiredFields(txtName)) return;

            string name = txtName.Text.Trim();
            string code = txtCode.Text.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                BarcodeService barcodeService = new BarcodeService();
                code = barcodeService.GenerateSKU("Services", name);
            }

            decimal price = numPrice.Value;
            string rawStatus = cmbStatus.SelectedItem?.ToString() ?? "Active";
            string status = (rawStatus == LocalizationManager.GetString("Status_Active") || rawStatus == "Active") ? "Active" : "Inactive";

            try
            {
                if (_editingPartId == null)
                {
                    // Services always have 999 stock and belong to 'Services' category
                    _inventoryService.AddPart(name, code, "Services", 999, price, 0, _currentImagePath, "", "Service Area", "N/A", status);
                    _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Service '{name}' added via WinForms");
                    MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_ServiceAdded"));
                }
                else
                {
                    _inventoryService.UpdatePart(int.Parse(_editingPartId), name, code, "Services", price, 999, 0, _currentImagePath, "", "Service Area", "N/A", status);
                    _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Service '{name}' updated via WinForms");
                }
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError("Error saving service: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
