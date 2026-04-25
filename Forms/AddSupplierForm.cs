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
        private FlatDateTimePicker dtDueDate;
        private NumericUpDown numReminderDays;
        private CheckBox chkEnableReminder;
        
        public string SupplierName => rdoCompany.Checked ? txtName.Text.Trim() : ContactPerson;
        public string ContactPerson => $"{txtFirstName.Text.Trim()} {txtLastName.Text.Trim()}".Trim();
        public string Phone => txtPhone.Text.Trim();
        public string Email => txtEmail.Text.Trim();
        public string Address => txtAddress.Text.Trim();
        public DateTime? DueDate => chkEnableReminder.Checked ? dtDueDate.Value : (DateTime?)null;
        public int ReminderDays => (int)numReminderDays.Value;
        public string SupplierType => rdoCompany.Checked ? "Company" : "Individual";

        private RadioButton rdoCompany;
        private RadioButton rdoIndividual;

        public AddSupplierForm()
        {
            InitializeComponent();
            SetFooterButtons(
                GenericInventorySystem.Helpers.LocalizationManager.GetString("AddSup_Save"),
                GenericInventorySystem.Helpers.LocalizationManager.GetString("Popup_Cancel"),
                btnSave_Click,
                btnCancel_Click
            );
            
            ApplyTheme();
            GenericInventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (rdoCompany.Checked)
            {
                if (!ValidationHelper.ValidateRequiredFields(txtName, txtPhone)) return;
            }
            else
            {
                if (!ValidationHelper.ValidateRequiredFields(txtFirstName, txtLastName, txtPhone)) return;
            }

            if (!ValidationHelper.ValidatePhoneNumber(txtPhone.Text)) return;
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !ValidationHelper.ValidateEmail(txtEmail.Text)) return;

            DialogResult = DialogResult.OK; 
            Close();
        }

        private void ApplyLocalization()
        {
            GenericInventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;

            bool isEdit = this.TitleText != null && (this.TitleText.Contains("Edit") || this.TitleText.Contains("تعديل"));
            this.TitleText = isEdit ? L("AddSup_TitleEdit") : L("AddSup_TitleNew");
            
            var lblSection = this.Controls.Find("lblSection", true);
            if(lblSection.Length > 0) lblSection[0].Text = L("AddSup_Section");

            if(txtName != null) txtName.LabelText = L("AddSup_CompanyName");
            if(txtFirstName != null) txtFirstName.LabelText = L("Add_FirstName");
            if(txtLastName != null) txtLastName.LabelText = L("Add_LastName");
            if(txtPhone != null) txtPhone.LabelText = L("Popup_Phone");
            if(txtEmail != null) txtEmail.LabelText = L("AddSup_Email");
            if(txtAddress != null) txtAddress.LabelText = L("Popup_Address");
            
            var lblDue = this.Controls.Find("lblDueDate", true);
            if(lblDue.Length > 0) lblDue[0].Text = L("AddSup_DueDate") ?? "Payment Due Date";
            
            var lblRem = this.Controls.Find("lblRemDays", true);
            if(lblRem.Length > 0) lblRem[0].Text = L("AddSup_ReminderDays") ?? "Reminder (Days Before)";
            
            if(chkEnableReminder != null) chkEnableReminder.Text = L("AddSup_EnableReminder") ?? "Enable Reminder";

            var lblType = this.Controls.Find("lblType", true);
            if(lblType.Length > 0) lblType[0].Text = L("AddSup_Type");

            if(rdoCompany != null) rdoCompany.Text = L("Popup_Company");
            if(rdoIndividual != null) rdoIndividual.Text = L("Popup_Individual");

            UpdateValidationUI();

            SetFooterButtons(
                isEdit ? L("AddSup_UpdateBtn") : L("AddSup_Save"),
                L("Popup_Cancel"),
                btnSave_Click,
                btnCancel_Click
            );
        }

        // Edit Mode Constructor
        public AddSupplierForm(int id, string name, string phone, string email, string address, string type, DateTime? dueDate = null, int reminderDays = 0, string contactPerson = "") : this()
        {
            this.TitleText = GenericInventorySystem.Helpers.LocalizationManager.GetString("AddSup_TitleEdit");
            SetFooterButtons(
                GenericInventorySystem.Helpers.LocalizationManager.GetString("AddSup_UpdateBtn"),
                GenericInventorySystem.Helpers.LocalizationManager.GetString("Popup_Cancel"),
                btnSave_Click,
                btnCancel_Click
            );
            
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

            if (dueDate.HasValue)
            {
                chkEnableReminder.Checked = true;
                dtDueDate.Value = dueDate.Value;
            }
            numReminderDays.Value = reminderDays;

            if (type == "Company") rdoCompany.Checked = true;
            else rdoIndividual.Checked = true;
            UpdateValidationUI();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(550, 900);

            TableLayoutPanel tlpMain = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 10, AutoSize = true, Padding = new Padding(20) };
            for(int i=0; i<10; i++) tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Section Title
            Label lblSection = new Label { Name = "lblSection", Text = "Supplier Details", Font = ThemeConfig.SubHeaderFont, AutoSize = true, ForeColor = ThemeConfig.SecondaryColor, Margin = new Padding(0, 0, 0, 15) };
            tlpMain.Controls.Add(lblSection, 0, 0);

            // Type Selection
            TableLayoutPanel pnlType = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Height = 40, Margin = new Padding(0, 0, 0, 15) };
            pnlType.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlType.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            pnlType.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));

            Label lblType = new Label { Name = "lblType", Text = "Supplier Type", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeConfig.TextColorDark, AutoSize = true, Anchor = AnchorStyles.Left };
            rdoCompany = new RadioButton { Text = "Company", Font = ThemeConfig.StandardFont, AutoSize = true, Anchor = AnchorStyles.Left, Checked = true };
            rdoIndividual = new RadioButton { Text = "Individual", Font = ThemeConfig.StandardFont, AutoSize = true, Anchor = AnchorStyles.Left };
            
            pnlType.Controls.Add(lblType, 0, 0);
            pnlType.Controls.Add(rdoCompany, 1, 0);
            pnlType.Controls.Add(rdoIndividual, 2, 0);
            tlpMain.Controls.Add(pnlType, 0, 1);

            // Company Name
            txtName = new ModernTextBox { LabelText = "Company Name", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
            tlpMain.Controls.Add(txtName, 0, 2);

            // Contact Row
            TableLayoutPanel pnlNames = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Height = 80, Margin = new Padding(0) };
            pnlNames.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlNames.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            txtFirstName = new ModernTextBox { LabelText = "Contact First Name", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 5, 0) };
            txtLastName = new ModernTextBox { LabelText = "Contact Last Name", Dock = DockStyle.Fill, Margin = new Padding(5, 0, 0, 0) };
            pnlNames.Controls.Add(txtFirstName, 0, 0);
            pnlNames.Controls.Add(txtLastName, 1, 0);
            tlpMain.Controls.Add(pnlNames, 0, 3);

            // Phone & Email
            txtPhone = new ModernTextBox { LabelText = "Phone Number", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
            tlpMain.Controls.Add(txtPhone, 0, 4);

            txtEmail = new ModernTextBox { LabelText = "Email Address", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
            tlpMain.Controls.Add(txtEmail, 0, 5);

            // Reminder Group
            TableLayoutPanel pnlReminders = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, AutoSize = true, Margin = new Padding(0, 0, 0, 15) };
            pnlReminders.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlReminders.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            chkEnableReminder = new CheckBox { Text = "Enable Payment Reminder", AutoSize = true, Font = ThemeConfig.StandardFont, Margin = new Padding(5, 0, 0, 10) };
            tlpMain.Controls.Add(chkEnableReminder, 0, 7);

            Label lblDueDate = new Label { Name = "lblDueDate", Text = "Payment Due Date", Font = ThemeConfig.SubHeaderFont, ForeColor = ThemeConfig.TextColorDark, AutoSize = true };
            dtDueDate = new FlatDateTimePicker { Dock = DockStyle.Fill, Enabled = false, Height = 40 };
            pnlReminders.Controls.Add(lblDueDate, 0, 0);
            pnlReminders.Controls.Add(dtDueDate, 0, 1);

            Label lblRemDays = new Label { Name = "lblRemDays", Text = "Reminder (Days Before)", Font = ThemeConfig.SubHeaderFont, ForeColor = ThemeConfig.TextColorDark, AutoSize = true };
            numReminderDays = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 365, Enabled = false, Font = ThemeConfig.StandardFont, Height = 40 };
            pnlReminders.Controls.Add(lblRemDays, 1, 0);
            pnlReminders.Controls.Add(numReminderDays, 1, 1);
            tlpMain.Controls.Add(pnlReminders, 0, 8);

            chkEnableReminder.CheckedChanged += (s, e) => { dtDueDate.Enabled = numReminderDays.Enabled = chkEnableReminder.Checked; };

            // Address
            txtAddress = new ModernTextBox { LabelText = "Address", Dock = DockStyle.Fill, Multiline = true, Height = 100, Margin = new Padding(0, 0, 0, 20) };
            tlpMain.Controls.Add(txtAddress, 0, 9);

            rdoCompany.CheckedChanged += (s, e) => UpdateValidationUI();
            rdoIndividual.CheckedChanged += (s, e) => UpdateValidationUI();

            this.ContentPanel.Controls.Add(tlpMain);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void UpdateValidationUI()
        {
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;
            bool isCompany = rdoCompany.Checked;

            txtName.Visible = isCompany;
            txtName.LabelText = L("Add_CompanyName") + " *";
            
            txtFirstName.LabelText = isCompany ? L("Add_ContactFirstName") : L("Add_FirstName") + " *";
            txtLastName.LabelText = isCompany ? L("Add_ContactLastName") : L("Add_LastName") + " *";
            txtPhone.LabelText = L("Popup_Phone") + " *";
        }

        private void ApplyTheme()
        {
            // Background is White (BaseModalForm)
            // Footer buttons are styled automatically by SetFooterButtons
        }
    }
}
