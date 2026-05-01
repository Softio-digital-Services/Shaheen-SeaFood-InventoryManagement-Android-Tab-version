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
            this.TitleText = LocalizationManager.IsArabic ? "Ø¥Ø¶Ø§ÙØ© ÙØ¦Ø© Ø¬Ø¯ÙŠØ¯Ø©" : "Add New Category";
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            bool ar = LocalizationManager.IsArabic;
            this.RightToLeft = ar ? RightToLeft.Yes : RightToLeft.No;
            
            txtName.LabelText = ar ? "Ø§Ø³Ù… Ø§Ù„ÙØ¦Ø©" : "Category Name";
            txtDesc.LabelText = ar ? "Ø§Ù„ÙˆØµÙ" : "Description";
            
            SetFooterButtons(
                ar ? "Ø­ÙØ¸" : "Save",
                ar ? "Ø¥Ù„ØºØ§Ø¡" : "Cancel",
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
