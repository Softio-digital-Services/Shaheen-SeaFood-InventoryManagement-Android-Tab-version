using System;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    public partial class LicenseActivationForm : BaseModalForm
    {
        private ModernTextBox txtLicenseKey;
        private Label lblHardwareId;
        private Label lblStatus;
        private Button btnActivate;
        private Button btnStartTrial;
        private Button btnCancel;
        private LinkLabel lnkCopyHardwareId;

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblHwIdTitle;
        public bool LicenseActivated { get; private set; }

        public LicenseActivationForm()
        {
            InitializeComponent();
            this.TitleText = "License Activation";
            this.Size = new Size(700, 480);
            
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Header
            lblTitle = ThemeConfig.CreateStandardHeader("Activate Your License");
            this.ContentPanel.Controls.Add(lblTitle);

            lblSubtitle = new Label();
            lblSubtitle.Text = "Enter your license key to activate the software";
            lblSubtitle.Font = ThemeConfig.StandardFont;
            lblSubtitle.ForeColor = ThemeConfig.SecondaryColor;
            lblSubtitle.Location = new Point(30, 100);
            lblSubtitle.AutoSize = true;

            this.ContentPanel.Controls.Add(lblSubtitle);

            // License Key Input
            txtLicenseKey = new ModernTextBox();
            txtLicenseKey.LabelText = "License Key";
            txtLicenseKey.Location = new Point(30, 160);
            txtLicenseKey.Width = 620;
            txtLicenseKey.TextChanged += TxtLicenseKey_TextChanged;
            this.ContentPanel.Controls.Add(txtLicenseKey);

            // Hardware ID Display
            lblHwIdTitle = new Label();
            lblHwIdTitle.Text = "Machine ID (for support):";
            lblHwIdTitle.Font = ThemeConfig.SmallBoldFont;
            lblHwIdTitle.ForeColor = ThemeConfig.TextColorDark;
            lblHwIdTitle.Location = new Point(30, 240);
            lblHwIdTitle.AutoSize = true;
            this.ContentPanel.Controls.Add(lblHwIdTitle);

            lblHardwareId = new Label();
            lblHardwareId.Text = HardwareInfo.GetShortHardwareId();
            lblHardwareId.Font = ThemeConfig.StandardFont;
            lblHardwareId.ForeColor = ThemeConfig.SecondaryColor;
            lblHardwareId.Location = new Point(30, 265);
            lblHardwareId.AutoSize = true;
            this.ContentPanel.Controls.Add(lblHardwareId);

            lnkCopyHardwareId = new LinkLabel();
            lnkCopyHardwareId.Text = "Copy to Clipboard";
            lnkCopyHardwareId.Font = ThemeConfig.StandardFont;
            lnkCopyHardwareId.Location = new Point(30, 290);
            lnkCopyHardwareId.AutoSize = true;
            lnkCopyHardwareId.LinkClicked += (s, e) =>
            {
                Clipboard.SetText(lblHardwareId.Text);
                MessageHelper.ShowSuccess("Machine ID copied to clipboard!");
            };
            this.ContentPanel.Controls.Add(lnkCopyHardwareId);

            // Status Label
            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Font = ThemeConfig.StandardFont;
            lblStatus.ForeColor = ThemeConfig.DangerColor;
            lblStatus.Location = new Point(30, 325);
            lblStatus.Size = new Size(620, 30);
            this.ContentPanel.Controls.Add(lblStatus);

            // Buttons
            btnCancel = new ModernButton();
            btnCancel.Text = "Exit";
            btnCancel.Size = new Size(120, 40);
            btnCancel.Location = new Point(230, 370);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            ThemeConfig.ApplySecondaryButton(btnCancel);
            this.ContentPanel.Controls.Add(btnCancel);

            btnStartTrial = new ModernButton();
            btnStartTrial.Text = "Start 30-Day Trial";
            btnStartTrial.Size = new Size(150, 40);
            btnStartTrial.Location = new Point(360, 370);
            btnStartTrial.Click += BtnStartTrial_Click;
            btnStartTrial.Visible = !LocalizationManager.IsArabic; // Hide on AR to avoid layout issues unless space is plenty
            ThemeConfig.ApplySecondaryButton(btnStartTrial);
            this.ContentPanel.Controls.Add(btnStartTrial);

            btnActivate = new ModernButton();
            btnActivate.Text = "Activate";
            btnActivate.Size = new Size(150, 40);
            btnActivate.Location = new Point(530, 370);
            btnActivate.Click += BtnActivate_Click;
            btnActivate.Enabled = false;
            ThemeConfig.ApplyPrimaryButton(btnActivate);
            this.ContentPanel.Controls.Add(btnActivate);


            this.ResumeLayout(false);
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            this.TitleText = isArabic ? "تفعيل الترخيص" : "License Activation";
            lblTitle.Text = isArabic ? "تفعيل الترخيص الخاص بك" : "Activate Your License";
            lblSubtitle.Text = isArabic ? "أدخل مفتاح الترخيص لتفعيل التطبيق" : "Enter your license key to activate the software";
            
            txtLicenseKey.LabelText = isArabic ? "مفتاح الترخيص" : "License Key";
            lblHwIdTitle.Text = isArabic ? "معرف الجهاز (للدعم الفني):" : "Machine ID (for support):";
            lnkCopyHardwareId.Text = isArabic ? "نسخ إلى الحافظة" : "Copy to Clipboard";
            
            btnCancel.Text = isArabic ? "خروج" : "Exit";
            btnStartTrial.Text = isArabic ? "بدء فترة تجريبية مجانية 30 يوماً" : "Start 30-Day Trial";
            btnActivate.Text = isArabic ? "تفعيل" : "Activate";
        }

        private void TxtLicenseKey_TextChanged(object sender, EventArgs e)
        {
            // Enable activate button if key is provided (Format: CPIMS-XXXXX-XXXXX-XXXXX-XXXXX - 25+ chars with dashes)
            string key = txtLicenseKey.Text.Replace("-", "").Replace(" ", "");
            btnActivate.Enabled = key.Length >= 20;
            lblStatus.Text = "";
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            string licenseKey = txtLicenseKey.Text.Trim();

            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                lblStatus.Text = LocalizationManager.IsArabic ? "يرجى إدخال مفتاح الترخيص." : "Please enter a license key.";
                lblStatus.ForeColor = Color.Red;
                return;
            }

            // Validate and activate
            // User requested to remove Name field, so we use a default internal name for validation
            string customerName = "Licensed User"; 
            LicenseKey license = LicenseManager.ActivateLicense(licenseKey, customerName);

            if (license == null)
            {
                lblStatus.Text = LocalizationManager.IsArabic ? "مفتاح الترخيص غير صالح. يرجى التحقق والمحاولة مرة أخرى." : "Invalid license key. Please check and try again.";
                lblStatus.ForeColor = Color.Red;
                return;
            }

            // Success
            LicenseActivated = true;
            string successMsg = LocalizationManager.IsArabic 
                ? $"تم تفعيل الترخيص بنجاح!\n\nينتهي في: {license.ExpirationDate:MMMM dd, yyyy}"
                : $"License activated successfully!\n\nExpires: {license.ExpirationDate:MMMM dd, yyyy}";
            MessageHelper.ShowSuccess(successMsg);
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnStartTrial_Click(object sender, EventArgs e)
        {
            // Check if trial already used
            LicenseKey existingLicense = LicenseManager.GetCurrentLicense();
            if (existingLicense != null && existingLicense.IsTrial())
            {
                MessageHelper.ShowWarning("Trial period has already been used on this machine.");
                return;
            }

            if (MessageHelper.ConfirmAction(LocalizationManager.IsArabic ? "بدء التفعيل التجريبي المجاني لمدة 30 يوماً؟" : "Start a 30-day free trial?"))
            {
                LicenseKey trial = LicenseManager.StartTrial();
                if (trial != null)
                {
                    LicenseActivated = true;
                    string msg = LocalizationManager.IsArabic 
                        ? $"تم التفعيل التجريبي! لديك {trial.DaysRemaining()} يوم متبقي."
                        : $"Trial activated! You have {trial.DaysRemaining()} days remaining.";
                    MessageHelper.ShowSuccess(msg);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageHelper.ShowError(LocalizationManager.IsArabic ? "فشل بدء التفعيل التجريبي. يرجى التواصل مع الدعم الفني." : "Failed to start trial. Please contact support.");
                }
            }
        }
    }
}
