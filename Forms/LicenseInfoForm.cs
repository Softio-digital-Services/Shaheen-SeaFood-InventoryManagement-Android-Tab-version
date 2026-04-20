using System;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    public partial class LicenseInfoForm : BaseModalForm
    {
        private LicenseKey _license;

        public LicenseInfoForm()
        {
            InitializeComponent();
            this.Size = new Size(500, 450);
            
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Header
            lblTitle = ThemeConfig.CreateStandardHeader("License Details");
            this.ContentPanel.Controls.Add(lblTitle);

            this.ResumeLayout(false);
        }

        private Label lblTitle;
        private Label lblRenewal;
        private Button btnClose;

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            this.TitleText = isArabic ? "Ù…Ø¹Ù„ÙˆÙ…Ø§Øª Ø§Ù„ØªØ±Ø®ÙŠØµ" : "License Information";
            if (lblTitle != null) lblTitle.Text = isArabic ? "ØªÙØ§ØµÙŠÙ„ Ø§Ù„ØªØ±Ø®ÙŠØµ" : "License Details";
            
            if (btnClose != null) btnClose.Text = isArabic ? "Ø¥ØºÙ„Ø§Ù‚" : "Close";
            
            if (lblRenewal != null)
                lblRenewal.Text = isArabic ? "ØªØ±Ø®ÙŠØµÙƒ Ø¹Ù„Ù‰ ÙˆØ´Ùƒ Ø§Ù„Ø§Ù†ØªÙ‡Ø§Ø¡. ÙŠØ±Ø¬Ù‰ Ø§Ù„ØªØ¬Ø¯ÙŠØ¯ Ù„Ù…ØªØ§Ø¨Ø¹Ø© Ø§Ø³ØªØ®Ø¯Ø§Ù… Ø§Ù„Ø¨Ø±Ù†Ø§Ù…Ø¬." : "Your license is expiring soon. Please renew to continue using the software.";

            LoadLicenseInfo(); // Relying on clearing and rebuilding the rows dynamically
        }

        private void LoadLicenseInfo()
        {
            // Clear dynamically generated controls before rebuilding
            for (int i = this.Controls.Count - 1; i >= 0; i--)
            {
                Control c = this.Controls[i];
                if (c == lblTitle) continue;
                this.Controls.RemoveAt(i);
                c.Dispose();
            }

            // Re-add essentials if we cleared them (just in case)
            if (!this.Controls.Contains(lblTitle)) this.ContentPanel.Controls.Add(lblTitle);

            _license = LicenseManager.GetCurrentLicense();
            bool isArabic = LocalizationManager.IsArabic;

            if (_license == null)
            {
                AddInfoRow(isArabic ? "Ø§Ù„Ø­Ø§Ù„Ø©:" : "Status:", isArabic ? "Ù„Ù… ÙŠØªÙ… Ø§Ù„Ø¹Ø«ÙˆØ± Ø¹Ù„Ù‰ ØªØ±Ø®ÙŠØµ" : "No License Found", 110, ThemeConfig.DangerColor);
                return;
            }

            int yPos = 110;
            int rowHeight = 35;

            // License Type
            string typeDisplay = _license.IsTrial() 
                ? (isArabic ? "Ù†Ø³Ø®Ø© ØªØ¬Ø±ÙŠØ¨ÙŠØ©" : "Trial Version") 
                : (isArabic ? "Ù†Ø³Ø®Ø© Ù…Ø±Ø®ØµØ©" : "Licensed Version");
            AddInfoRow(isArabic ? "Ù†ÙˆØ¹ Ø§Ù„ØªØ±Ø®ÙŠØµ:" : "License Type:", typeDisplay, yPos, ThemeConfig.TextColorDark);
            yPos += rowHeight;

            // Customer Name
            if (!string.IsNullOrEmpty(_license.CustomerName))
            {
                AddInfoRow(isArabic ? "Ø§Ø³Ù… Ø§Ù„Ø¹Ù…ÙŠÙ„:" : "Customer Name:", _license.CustomerName, yPos, ThemeConfig.TextColorDark);
                yPos += rowHeight;
            }

            // License Key
            if (!_license.IsTrial())
            {
                AddInfoRow(isArabic ? "Ù…ÙØªØ§Ø­ Ø§Ù„ØªØ±Ø®ÙŠØµ:" : "License Key:", _license.Key, yPos, ThemeConfig.SecondaryColor);
                yPos += rowHeight;
            }

            // Activation Date
            AddInfoRow(isArabic ? "ØªØ§Ø±ÙŠØ® Ø§Ù„ØªÙØ¹ÙŠÙ„:" : "Activated On:", _license.ActivationDate.ToString("MMMM dd, yyyy"), yPos, ThemeConfig.TextColorDark);
            yPos += rowHeight;

            // Expiration Date
            Color expiryColor = _license.IsExpiringSoon() ? Color.Orange : ThemeConfig.TextColorDark;
            AddInfoRow(isArabic ? "ØªØ§Ø±ÙŠØ® Ø§Ù„Ø§Ù†ØªÙ‡Ø§Ø¡:" : "Expires On:", _license.ExpirationDate.ToString("MMMM dd, yyyy"), yPos, expiryColor);
            yPos += rowHeight;

            // Days Remaining
            int daysLeft = _license.DaysRemaining();
            Color daysColor = daysLeft <= 30 ? ThemeConfig.DangerColor : ThemeConfig.SuccessColor;
            string daysText = isArabic ? $"{daysLeft} ÙŠÙˆÙ…" : $"{daysLeft} days";
            AddInfoRow(isArabic ? "Ø§Ù„Ø£ÙŠØ§Ù… Ø§Ù„Ù…ØªØ¨Ù‚ÙŠØ©:" : "Days Remaining:", daysText, yPos, daysColor);

            yPos += rowHeight;

            // Status
            bool isValid = _license.IsValid();
            string statusText = isValid 
                ? (isArabic ? "Ù†Ø´Ø·" : "Active") 
                : (isArabic ? "Ù…Ù†ØªÙ‡ÙŠ" : "Expired");
            Color statusColor = isValid ? ThemeConfig.SuccessColor : ThemeConfig.DangerColor;
            AddInfoRow(isArabic ? "Ø§Ù„Ø­Ø§Ù„Ø©:" : "Status:", statusText, yPos, statusColor);

            yPos += rowHeight;

            // Machine Name
            AddInfoRow(isArabic ? "Ø§Ù„Ø¬Ù‡Ø§Ø²:" : "Machine:", _license.MachineName, yPos, ThemeConfig.SecondaryColor);
            yPos += rowHeight;

            // Renewal Notice
            if (_license.IsExpiringSoon() && !_license.IsTrial())
            {
                lblRenewal = new Label();
                lblRenewal.Text = isArabic 
                    ? "âš  ØªØ±Ø®ÙŠØµÙƒ Ø¹Ù„Ù‰ ÙˆØ´Ùƒ Ø§Ù„Ø§Ù†ØªÙ‡Ø§Ø¡. ÙŠØ±Ø¬Ù‰ Ø§Ù„ØªØ¬Ø¯ÙŠØ¯ Ù„Ù…ØªØ§Ø¨Ø¹Ø© Ø§Ø³ØªØ®Ø¯Ø§Ù… Ø§Ù„Ø¨Ø±Ù†Ø§Ù…Ø¬." 
                    : "âš  Your license is expiring soon. Please renew to continue using the software.";
                lblRenewal.Font = ThemeConfig.StandardFont;
                lblRenewal.ForeColor = ThemeConfig.WarningColor;
                lblRenewal.Location = new Point(30, yPos + 20);
                lblRenewal.Size = new Size(430, 40);

                this.ContentPanel.Controls.Add(lblRenewal);
            }

            // Close Button
            if (btnClose == null)
            {
                btnClose = new ModernButton();
                btnClose.Click += (s, e) => this.Close();
                ThemeConfig.ApplyPrimaryButton(btnClose);
            }
            btnClose.Text = isArabic ? "Ø¥ØºÙ„Ø§Ù‚" : "Close";
            btnClose.Size = new Size(120, 40);
            btnClose.Location = new Point(isArabic ? 30 : 350, 380); // Adjust position for RTL
            this.ContentPanel.Controls.Add(btnClose);
        }

        private void AddInfoRow(string label, string value, int yPos, Color valueColor)
        {
            bool isArabic = LocalizationManager.IsArabic;
            int rightAlign = this.Width - 50; 

            Label lblLabel = new Label();
            lblLabel.Text = label;
            lblLabel.Font = ThemeConfig.SmallBoldFont;
            lblLabel.ForeColor = ThemeConfig.SecondaryColor;
            lblLabel.AutoSize = true;

            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.Font = ThemeConfig.StandardFont;
            lblValue.ForeColor = valueColor;
            lblValue.AutoSize = true;
            
            this.ContentPanel.Controls.Add(lblLabel);
            this.ContentPanel.Controls.Add(lblValue);

            // Manual positioning because RightToLeft doesn't perfectly mirror dynamically generated absolute coords
            if (isArabic)
            {
                lblLabel.Location = new Point(rightAlign - lblLabel.PreferredWidth, yPos);
                lblValue.Location = new Point(rightAlign - 170 - lblValue.PreferredWidth, yPos);
            }
            else
            {
                lblLabel.Location = new Point(30, yPos);
                lblValue.Location = new Point(200, yPos);
            }
        }
    }
}

