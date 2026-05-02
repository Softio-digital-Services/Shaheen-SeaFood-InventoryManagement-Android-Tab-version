using System;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Data;

namespace GenericInventorySystem.Forms
{
    public partial class AddCategoryForm : BaseModalForm
    {
        public string NewCategoryName { get; private set; }
        private string _currentImagePath = "";

        public AddCategoryForm()
        {
            InitializeComponent();
            this.TitleText = "Add New Category";
            btnUpload.Click += BtnUpload_Click;
            ApplyLocalization();
        }

        private void BtnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string ext = System.IO.Path.GetExtension(ofd.FileName);
                        string fileName = "cat_" + Guid.NewGuid().ToString().Substring(0, 8) + ext;
                        string targetDir = System.IO.Path.Combine(Application.StartupPath, "Assets", "Categories");
                        
                        if (!System.IO.Directory.Exists(targetDir))
                            System.IO.Directory.CreateDirectory(targetDir);

                        string targetPath = System.IO.Path.Combine(targetDir, fileName);
                        System.IO.File.Copy(ofd.FileName, targetPath, true);

                        _currentImagePath = "Assets/Categories/" + fileName;
                        
                        using (var stream = new System.IO.FileStream(targetPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            if (pbImage.Image != null) pbImage.Image.Dispose();
                            pbImage.Image = Image.FromStream(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageHelper.ShowError("Error uploading image: " + ex.Message);
                    }
                }
            }
        }

        private void ApplyLocalization()
        {
            bool ar = LocalizationManager.IsArabic;
            this.RightToLeft = ar ? RightToLeft.Yes : RightToLeft.No;
            
            this.TitleText = LocalizationManager.GetString("AddCat_Title");
            txtName.LabelText = LocalizationManager.GetString("AddCat_Name");
            txtDesc.LabelText = LocalizationManager.GetString("AddCat_Desc");
            btnUpload.Text = LocalizationManager.GetString("AddPart_Upload");
            
            SetFooterButtons(
                LocalizationManager.GetString("AddCat_Save"),
                LocalizationManager.GetString("Tran_Cancel"),
                btnSave_Click,
                (s, e) => this.Close()
            );
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.ValidateRequiredFields(txtName)) return;

            try
            {
                CategoryData.AddCategory(txtName.Text.Trim(), txtDesc.Text.Trim(), _currentImagePath);
                NewCategoryName = txtName.Text.Trim();
                
                // Real-time Sync: Tell all Web POS clients to refresh categories
                _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Category '{NewCategoryName}' added");
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(ex.Message);
            }
        }
    }
}
