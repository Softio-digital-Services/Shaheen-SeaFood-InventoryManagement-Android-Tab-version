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

        public AddCategoryForm()
        {
            InitializeComponent();
            this.TitleText = LocalizationManager.IsArabic ? "إضافة فئة جديدة" : "Add New Category";
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            bool ar = LocalizationManager.IsArabic;
            this.RightToLeft = ar ? RightToLeft.Yes : RightToLeft.No;
            
            txtName.LabelText = ar ? "اسم الفئة" : "Category Name";
            txtDesc.LabelText = ar ? "الوصف" : "Description";
            
            SetFooterButtons(
                ar ? "حفظ" : "Save",
                ar ? "إلغاء" : "Cancel",
                btnSave_Click,
                (s, e) => this.Close()
            );
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.ValidateRequiredFields(txtName)) return;

            try
            {
                CategoryData.AddCategory(txtName.Text.Trim(), txtDesc.Text.Trim());
                NewCategoryName = txtName.Text.Trim();
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
