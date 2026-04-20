using System;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Data;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    public partial class AddCategoryForm : BaseModalForm
    {
        public AddCategoryForm()
        {
            InitializeComponent();
            this.TitleText = "Add New Category";
            
            // Match Theme
            btnSave.BackColor = ThemeConfig.PrimaryColor;
            btnCancel.BackColor = ThemeConfig.SecondaryColor;

            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            this.RightToLeft = LocalizationManager.IsArabic ? RightToLeft.Yes : RightToLeft.No;

            this.TitleText = LocalizationManager.GetString("AddCat_Title");
            txtName.LabelText = LocalizationManager.GetString("AddCat_Name");
            txtDesc.LabelText = LocalizationManager.GetString("AddCat_Desc");
            btnSave.Text = LocalizationManager.GetString("AddCat_Save");
            btnCancel.Text = LocalizationManager.GetString("Popup_Cancel");
        }

        
        private void btnSave_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageHelper.ShowWarning("Category Name is required.");
                return;
            }

            try
            {
                CategoryData.AddCategory(txtName.Text.Trim(), txtDesc.Text.Trim());
                MessageHelper.ShowSuccess("Category added successfully!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch(Exception ex)
            {
                MessageHelper.ShowError("Error adding category: " + ex.Message);
            }
        }
    }
}
