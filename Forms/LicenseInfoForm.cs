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
        private TableLayoutPanel tlpInfo;
        private Label lblTitle;
        private Label lblRenewal;

        public LicenseInfoForm()
        {
            this.Width = 600;
            InitializeComponent();
            // Adaptive sizing handled by BaseModalForm.OnLoad
            
            ApplyLocalization();
            LoadLicenseInfo();
            LocalizationManager.LanguageChanged += (s, e) => { ApplyLocalization(); LoadLicenseInfo(); };
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Header removed as it's already in the Modal Header

            tlpInfo = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 0,
                Padding = new Padding(25, 10, 25, 10),
                AutoSize = true,
                AutoScroll = false
            };
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            tlpInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.ContentPanel.Controls.Add(tlpInfo);

            ApplyLocalization(); // To set initial button text

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            this.TitleText = isArabic ? "معلومات الترخيص" : "License Information";
            
            SetFooterButtons(
                isArabic ? "إغلاق" : "Close",
                "",
                (s, e) => this.Close(),
                null
            );
        }

        private void LoadLicenseInfo()
        {
            tlpInfo.Controls.Clear();
            tlpInfo.RowCount = 0;
            tlpInfo.RowStyles.Clear();

            _license = LicenseManager.GetCurrentLicense();
            bool isArabic = LocalizationManager.IsArabic;

            if (_license == null)
            {
                AddInfoRow(isArabic ? "الحالة:" : "Status:", isArabic ? "لم يتم العثور على ترخيص" : "No License Found", ThemeConfig.DangerColor);
                return;
            }

            // License Type
            string typeDisplay = _license.IsTrial() 
                ? (isArabic ? "نسخة تجريبية" : "Trial Version") 
                : (isArabic ? "نسخة مرخصة" : "Licensed Version");
            AddInfoRow(isArabic ? "نوع الترخيص:" : "License Type:", typeDisplay, ThemeConfig.TextColorDark);

            // Customer Name
            if (!string.IsNullOrEmpty(_license.CustomerName))
            {
                AddInfoRow(isArabic ? "اسم العميل:" : "Customer Name:", _license.CustomerName, ThemeConfig.TextColorDark);
            }

            // License Key
            if (!_license.IsTrial())
            {
                AddInfoRow(isArabic ? "مفتاح الترخيص:" : "License Key:", _license.Key, ThemeConfig.SecondaryColor);
            }

            // Activation Date
            AddInfoRow(isArabic ? "تاريخ التفعيل:" : "Activated On:", _license.ActivationDate.ToString("MMMM dd, yyyy"), ThemeConfig.TextColorDark);

            // Expiration Date
            Color expiryColor = _license.IsExpiringSoon() ? Color.Orange : ThemeConfig.TextColorDark;
            AddInfoRow(isArabic ? "تاريخ الانتهاء:" : "Expires On:", _license.ExpirationDate.ToString("MMMM dd, yyyy"), expiryColor);

            // Days Remaining
            int daysLeft = _license.DaysRemaining();
            Color daysColor = daysLeft <= 30 ? ThemeConfig.DangerColor : ThemeConfig.SuccessColor;
            string daysText = isArabic ? $"{daysLeft} يوم" : $"{daysLeft} days";
            AddInfoRow(isArabic ? "الأيام المتبقية:" : "Days Remaining:", daysText, daysColor);

            // Status
            bool isValid = _license.IsValid();
            string statusText = isValid ? (isArabic ? "نشط" : "Active") : (isArabic ? "منتهي" : "Expired");
            Color statusColor = isValid ? ThemeConfig.SuccessColor : ThemeConfig.DangerColor;
            AddInfoRow(isArabic ? "الحالة:" : "Status:", statusText, statusColor);

            // Machine Name
            AddInfoRow(isArabic ? "الجهاز:" : "Machine:", _license.MachineName, ThemeConfig.SecondaryColor);

            // Renewal Notice
            if (_license.IsExpiringSoon() && !_license.IsTrial())
            {
                int row = tlpInfo.RowCount++;
                tlpInfo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                lblRenewal = new Label
                {
                    Text = isArabic 
                        ? "⚠ ترخيصك على وشك الانتهاء. يرجى التجديد لمتابعة استخدام البرنامج." 
                        : "⚠ Your license is expiring soon. Please renew to continue using the software.",
                    Font = ThemeConfig.StandardFont,
                    ForeColor = ThemeConfig.WarningColor,
                    AutoSize = true,
                    Margin = new Padding(0, 20, 0, 0)
                };
                tlpInfo.Controls.Add(lblRenewal, 0, row);
                tlpInfo.SetColumnSpan(lblRenewal, 2);
            }
        }

        private void AddInfoRow(string label, string value, Color valueColor)
        {
            int row = tlpInfo.RowCount++;
            tlpInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));

            Label lblLabel = new Label { Text = label, Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.SecondaryColor, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            Label lblValue = new Label { Text = value, Font = ThemeConfig.StandardFont, ForeColor = valueColor, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            
            tlpInfo.Controls.Add(lblLabel, 0, row);
            tlpInfo.Controls.Add(lblValue, 1, row);
        }
    }
}
