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
        private LinkLabel lnkCopyHardwareId;

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblHwIdTitle;
        public bool LicenseActivated { get; private set; }

        public LicenseActivationForm()
        {
            InitializeComponent();
            // Adaptive sizing handled by BaseModalForm.OnLoad
            
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(30),
                AutoSize = true
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Header title removed as it's already in the Modal Header

            lblSubtitle = new Label
            {
                Text = "Enter your license key to activate the software",
                Font = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.SecondaryColor,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 20)
            };
            tlpMain.Controls.Add(lblSubtitle, 0, 1);

            // License Key Input
            txtLicenseKey = new ModernTextBox
            {
                LabelText = "License Key",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 20)
            };
            txtLicenseKey.TextChanged += TxtLicenseKey_TextChanged;
            tlpMain.Controls.Add(txtLicenseKey, 0, 2);

            // Hardware ID Section
            TableLayoutPanel tlpMachineId = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, AutoSize = true };
            lblHwIdTitle = new Label { Text = "Machine ID (for support):", Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.TextColorDark, AutoSize = true };
            lblHardwareId = new Label { Text = HardwareInfo.GetShortHardwareId(), Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.SecondaryColor, AutoSize = true };
            lnkCopyHardwareId = new LinkLabel { Text = "Copy to Clipboard", Font = ThemeConfig.StandardFont, AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
            lnkCopyHardwareId.LinkClicked += (s, e) => { 
                Clipboard.SetText(lblHardwareId.Text); 
                MessageHelper.ShowSuccess(LocalizationManager.IsArabic ? "ØªÙ… Ù†Ø³Ø® Ù…Ø¹Ø±Ù Ø§Ù„Ø¬Ù‡Ø§Ø² Ø¥Ù„Ù‰ Ø§Ù„Ø­Ø§ÙØ¸Ø©!" : "Machine ID copied to clipboard!"); 
            };
            
            tlpMachineId.Controls.Add(lblHwIdTitle, 0, 0);
            tlpMachineId.Controls.Add(lblHardwareId, 0, 1);
            tlpMachineId.Controls.Add(lnkCopyHardwareId, 0, 2);
            tlpMain.Controls.Add(tlpMachineId, 0, 3);

            // Status Label
            lblStatus = new Label
            {
                Text = "",
                Font = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.DangerColor,
                AutoSize = true,
                Margin = new Padding(0, 20, 0, 0)
            };
            tlpMain.Controls.Add(lblStatus, 0, 4);

            this.ContentPanel.Controls.Add(tlpMain);

            ApplyLocalization(); // To set initial button text

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            this.TitleText = isArabic ? "ØªÙØ¹ÙŠÙ„ Ø§Ù„ØªØ±Ø®ÙŠØµ" : "License Activation";
            lblSubtitle.Text = isArabic ? "Ø£Ø¯Ø®Ù„ Ù…ÙØªØ§Ø­ Ø§Ù„ØªØ±Ø®ÙŠØµ Ù„ØªÙØ¹ÙŠÙ„ Ø§Ù„Ø¨Ø±Ù†Ø§Ù…Ø¬" : "Enter your license key to activate the software";
            
            txtLicenseKey.LabelText = isArabic ? "Ù…ÙØªØ§Ø­ Ø§Ù„ØªØ±Ø®ÙŠØµ" : "License Key";
            lblHwIdTitle.Text = isArabic ? "Ù…Ø¹Ø±Ù Ø§Ù„Ø¬Ù‡Ø§Ø² (Ù„Ù„Ø¯Ø¹Ù… Ø§Ù„ÙÙ†ÙŠ):" : "Machine ID (for support):";
            lnkCopyHardwareId.Text = isArabic ? "Ù†Ø³Ø® Ø¥Ù„Ù‰ Ø§Ù„Ø­Ø§ÙØ¸Ø©" : "Copy to Clipboard";
            
            SetFooterButtons(
                isArabic ? "ØªÙØ¹ÙŠÙ„" : "Activate",
                isArabic ? "Ø®Ø±ÙˆØ¬" : "Exit",
                BtnActivate_Click,
                (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); },
                isArabic ? "Ø¨Ø¯Ø¡ ÙØªØ±Ø© ØªØ¬Ø±ÙŠØ¨ÙŠØ© Ù…Ø¬Ø§Ù†ÙŠØ© 30 ÙŠÙˆÙ…Ø§Ù‹" : "Start 30-Day Trial",
                BtnStartTrial_Click
            );
        }

        private void TxtLicenseKey_TextChanged(object sender, EventArgs e)
        {
            // Enable activate button if key is provided (Format: CPIMS-XXXXX-XXXXX-XXXXX-XXXXX - 25+ chars with dashes)
            string key = txtLicenseKey.Text.Replace("-", "").Replace(" ", "");
            if (PrimaryButton != null) PrimaryButton.Enabled = key.Length >= 20;
            lblStatus.Text = "";
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            string licenseKey = txtLicenseKey.Text.Trim();

            if (!ValidationHelper.ValidateRequiredFields(this, new Control[] { txtLicenseKey }, new string[] { LocalizationManager.IsArabic ? "Ù…ÙØªØ§Ø­ Ø§Ù„ØªØ±Ø®ÙŠØµ" : "License Key" }))
            {
                return;
            }

            // Validate and activate
            // User requested to remove Name field, so we use a default internal name for validation
            string customerName = "Licensed User"; 
            LicenseKey license = LicenseManager.ActivateLicense(licenseKey, customerName);

            if (license == null)
            {
                lblStatus.Text = LocalizationManager.IsArabic ? "Ù…ÙØªØ§Ø­ Ø§Ù„ØªØ±Ø®ÙŠØµ ØºÙŠØ± ØµØ§Ù„Ø­. ÙŠØ±Ø¬Ù‰ Ø§Ù„ØªØ­Ù‚Ù‚ ÙˆØ§Ù„Ù…Ø­Ø§ÙˆÙ„Ø© Ù…Ø±Ø© Ø£Ø®Ø±Ù‰." : "Invalid license key. Please check and try again.";
                lblStatus.ForeColor = Color.Red;
                return;
            }

            // Success
            LicenseActivated = true;
            string successMsg = LocalizationManager.IsArabic 
                ? $"ØªÙ… ØªÙØ¹ÙŠÙ„ Ø§Ù„ØªØ±Ø®ÙŠØµ Ø¨Ù†Ø¬Ø§Ø­!\n\nÙŠÙ†ØªÙ‡ÙŠ ÙÙŠ: {license.ExpirationDate:MMMM dd, yyyy}"
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
                MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "Ù„Ù‚Ø¯ ØªÙ… Ø§Ø³ØªØ®Ø¯Ø§Ù… Ø§Ù„ÙØªØ±Ø© Ø§Ù„ØªØ¬Ø±ÙŠØ¨ÙŠØ© Ø¨Ø§Ù„ÙØ¹Ù„ Ø¹Ù„Ù‰ Ù‡Ø°Ø§ Ø§Ù„Ø¬Ù‡Ø§Ø²." : "Trial period has already been used on this machine.");
                return;
            }

            if (MessageHelper.ConfirmAction(LocalizationManager.IsArabic ? "Ø¨Ø¯Ø¡ Ø§Ù„ØªÙØ¹ÙŠÙ„ Ø§Ù„ØªØ¬Ø±ÙŠØ¨ÙŠ Ø§Ù„Ù…Ø¬Ø§Ù†ÙŠ Ù„Ù…Ø¯Ø© 30 ÙŠÙˆÙ…Ø§Ù‹ØŸ" : "Start a 30-day free trial?"))
            {
                LicenseKey trial = LicenseManager.StartTrial();
                if (trial != null)
                {
                    LicenseActivated = true;
                    string msg = LocalizationManager.IsArabic 
                        ? $"ØªÙ… Ø§Ù„ØªÙØ¹ÙŠÙ„ Ø§Ù„ØªØ¬Ø±ÙŠØ¨ÙŠ! Ù„Ø¯ÙŠÙƒ {trial.DaysRemaining()} ÙŠÙˆÙ… Ù…ØªØ¨Ù‚ÙŠ."
                        : $"Trial activated! You have {trial.DaysRemaining()} days remaining.";
                    MessageHelper.ShowSuccess(msg);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageHelper.ShowError(LocalizationManager.IsArabic ? "ÙØ´Ù„ Ø¨Ø¯Ø¡ Ø§Ù„ØªÙØ¹ÙŠÙ„ Ø§Ù„ØªØ¬Ø±ÙŠØ¨ÙŠ. ÙŠØ±Ø¬Ù‰ Ø§Ù„ØªÙˆØ§ØµÙ„ Ù…Ø¹ Ø§Ù„Ø¯Ø¹Ù… Ø§Ù„ÙÙ†ÙŠ." : "Failed to start trial. Please contact support.");
                }
            }
        }
    }
}
