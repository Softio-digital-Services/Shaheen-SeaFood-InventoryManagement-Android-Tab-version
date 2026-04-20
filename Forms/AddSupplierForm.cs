using System;
using System.Windows.Forms;
using System.Drawing;
using GenericInventorySystem.Data;
using GenericInventorySystem.Controls;

namespace GenericInventorySystem.Forms
{
    public partial class AddSupplierForm : BaseModalForm
    {
        private ModernTextBox txtName;
        private ModernTextBox txtFirstName;
        private ModernTextBox txtLastName;
        private ModernTextBox txtPhone;
        private ModernTextBox txtEmail;
        private ModernTextBox txtAddress;
        private Button btnSave;
        private Button btnCancel;
        
        public string SupplierName => rdoCompany.Checked ? txtName.Text.Trim() : ContactPerson;
        public string ContactPerson => $"{txtFirstName.Text.Trim()} {txtLastName.Text.Trim()}".Trim();
        public string Phone => txtPhone.Text.Trim();
        public string Email => txtEmail.Text.Trim();
        public string Address => txtAddress.Text.Trim();
        public string SupplierType => rdoCompany.Checked ? "Company" : "Individual";

        private RadioButton rdoCompany;
        private RadioButton rdoIndividual;

        public AddSupplierForm()
        {
            InitializeComponent();
            this.TitleText = "Add New Supplier";
            ApplyTheme();
            GenericInventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            GenericInventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;

            bool isEdit = this.TitleText != null && (this.TitleText.Contains("Edit") || this.TitleText.Contains("Ã˜ÂªÃ˜Â¹Ã˜Â¯Ã™Å Ã™â€ž") || this.TitleText.Contains("ØªØ¹Ø¯ÙŠÙ„"));
            this.TitleText = isEdit ? L("AddSup_TitleEdit") : L("AddSup_TitleNew");
            
            var lblSection = this.Controls.Find("lblSection", true);
            if(lblSection.Length > 0) lblSection[0].Text = L("AddSup_Section");

            if(txtName != null) txtName.LabelText = L("AddSup_CompanyName");
            if(txtFirstName != null) txtFirstName.LabelText = L("Add_FirstName");
            if(txtLastName != null) txtLastName.LabelText = L("Add_LastName");
            if(txtPhone != null) txtPhone.LabelText = L("Popup_Phone");
            if(txtEmail != null) txtEmail.LabelText = L("AddSup_Email");
            if(txtAddress != null) txtAddress.LabelText = L("Popup_Address");

            var lblType = this.Controls.Find("lblType", true);
            if(lblType.Length > 0) lblType[0].Text = L("AddSup_Type");

            if(rdoCompany != null) rdoCompany.Text = L("Popup_Company");
            if(rdoIndividual != null) rdoIndividual.Text = L("Popup_Individual");

            UpdateValidationUI();

            if(btnCancel != null) btnCancel.Text = L("Popup_Cancel");
            if(btnSave != null) btnSave.Text = isEdit ? L("AddSup_UpdateBtn") : L("AddSup_Save");
        }

        // Edit Mode Constructor
        public AddSupplierForm(int id, string name, string phone, string email, string address, string type, string contactPerson = "") : this()
        {
            this.TitleText = "Edit Supplier";
            btnSave.Text = "Update Supplier";
            
            txtName.Text = name;
            
            // Split contact person name
            if (!string.IsNullOrEmpty(contactPerson))
            {
                string[] parts = contactPerson.Split(new char[] { ' ' }, 2);
                if (parts.Length > 0) txtFirstName.Text = parts[0];
                if (parts.Length > 1) txtLastName.Text = parts[1];
            }

            txtPhone.Text = phone;
            txtEmail.Text = email;
            txtAddress.Text = address;

            if (type == "Company") rdoCompany.Checked = true;
            else rdoIndividual.Checked = true;
            UpdateValidationUI();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(500, 720);

            Label lblSection = new Label();
            this.SuspendLayout();

            // 
            // Section Title
            // 
            lblSection.Name = "lblSection";
            lblSection.Text = "Supplier Details";
            lblSection.Font = ThemeConfig.SubHeaderFont;
            lblSection.Location = new System.Drawing.Point(30, 70);
            lblSection.AutoSize = true;
            lblSection.ForeColor = ThemeConfig.SecondaryColor;
            this.ContentPanel.Controls.Add(lblSection);

            // 
            // Fields Configuration
            // 
            int startY = 110;
            int gap = 85;
            int w = 420;

            txtName = new ModernTextBox();
            txtName.LabelText = "Company Name";
            txtName.Location = new Point(30, startY);
            txtName.Width = w;
            this.ContentPanel.Controls.Add(txtName);

            // Contact Person Names
            txtFirstName = new ModernTextBox();
            txtFirstName.LabelText = "Contact First Name";
            txtFirstName.Location = new Point(30, startY + gap);
            txtFirstName.Width = (w / 2) - 5;
            this.ContentPanel.Controls.Add(txtFirstName);
            
            txtLastName = new ModernTextBox();
            txtLastName.LabelText = "Contact Last Name";
            txtLastName.Location = new Point(30 + (w / 2) + 5, startY + gap);
            txtLastName.Width = (w / 2) - 5;
            this.ContentPanel.Controls.Add(txtLastName);

            txtPhone = new ModernTextBox();
            txtPhone.LabelText = "Phone Number";
            txtPhone.Location = new Point(30, startY + gap * 2);
            txtPhone.Width = w;
            this.ContentPanel.Controls.Add(txtPhone);

            txtEmail = new ModernTextBox();
            txtEmail.LabelText = "Email / Contact Person";
            txtEmail.Location = new Point(30, startY + gap * 3);
            txtEmail.Width = w;
            this.ContentPanel.Controls.Add(txtEmail);

            txtAddress = new ModernTextBox();
            txtAddress.LabelText = "Address";
            txtAddress.Location = new Point(30, startY + gap * 4);
            txtAddress.Width = w;
            txtAddress.Multiline = true;
            txtAddress.Height = 110;
            this.ContentPanel.Controls.Add(txtAddress);

            // Type Radio Buttons
            int radioY = startY + gap * 4 + 130;
            Label lblType = new Label();
            lblType.Name = "lblType";
            lblType.Text = "Supplier Type";
            lblType.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblType.ForeColor = ThemeConfig.TextColorDark;
            lblType.Location = new System.Drawing.Point(35, radioY);
            lblType.AutoSize = true;
            this.ContentPanel.Controls.Add(lblType);

            rdoCompany = new RadioButton();
            rdoCompany.Text = "Company";
            rdoCompany.Location = new System.Drawing.Point(40, radioY + 25);
            rdoCompany.Font = ThemeConfig.StandardFont;
            rdoCompany.AutoSize = true;
            rdoCompany.Checked = true;
            this.ContentPanel.Controls.Add(rdoCompany);

            rdoIndividual = new RadioButton();
            rdoIndividual.Text = "Individual";
            rdoIndividual.Location = new System.Drawing.Point(150, radioY + 25);
            rdoIndividual.Font = ThemeConfig.StandardFont;
            rdoIndividual.AutoSize = true;
            this.ContentPanel.Controls.Add(rdoIndividual);

            rdoCompany.CheckedChanged += (s, e) => UpdateValidationUI();
            rdoIndividual.CheckedChanged += (s, e) => UpdateValidationUI();

            // 
            // Buttons
            // 
            btnCancel = new ModernButton();
            btnCancel.Text = "Cancel";
            btnCancel.Size = new System.Drawing.Size(120, 40);
            btnCancel.Location = new System.Drawing.Point(210, 640); 
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            btnSave = new ModernButton();
            btnSave.Text = "Save Supplier";
            btnSave.Size = new System.Drawing.Size(140, 40);
            btnSave.Location = new System.Drawing.Point(340, 640);
            btnSave.Click += (s, e) => 
            { 
                 Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;

                 if (rdoCompany.Checked)
                 {
                     if (string.IsNullOrWhiteSpace(txtName.Text))
                     {
                         MessageHelper.ShowWarning(L("AddSup_ReqName"));
                         return;
                     }
                 }
                 else
                 {
                     if (string.IsNullOrWhiteSpace(txtFirstName.Text))
                     {
                         MessageHelper.ShowWarning(L("Add_ReqFirstName"));
                         return;
                     }
                     if (string.IsNullOrWhiteSpace(txtLastName.Text))
                     {
                         MessageHelper.ShowWarning(L("Add_ReqLastName"));
                         return;
                     }
                 }

                 if(string.IsNullOrWhiteSpace(txtPhone.Text) || !ValidationHelper.ValidatePhoneNumber(txtPhone.Text))
                 {
                     MessageHelper.ShowWarning(L("Popup_ReqPhone"));
                     return;
                 }

                 DialogResult = DialogResult.OK; 
                 Close(); 
            };

            this.ContentPanel.Controls.Add(btnSave); 
            this.ContentPanel.Controls.Add(btnCancel);
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void UpdateValidationUI()
        {
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;
            bool isCompany = rdoCompany.Checked;

            txtName.Visible = isCompany;
            txtName.LabelText = L("AddSup_CompanyName"); // It already has the asterisk effectively, or I should be consistent
            // Actually I want to be consistent with the asterisk logic
            txtName.LabelText = L("Add_CompanyName") + " *";
            
            txtFirstName.LabelText = isCompany ? L("Add_ContactFirstName") : L("Add_FirstName") + " *";
            txtLastName.LabelText = isCompany ? L("Add_ContactLastName") : L("Add_LastName") + " *";
            txtPhone.LabelText = L("Popup_Phone") + " *";
        }

        private void ApplyTheme()
        {
            // Background is White (BaseModalForm)
            ThemeConfig.ApplyPrimaryButton(btnSave);
            ThemeConfig.ApplySecondaryButton(btnCancel);
        }
    }
}



