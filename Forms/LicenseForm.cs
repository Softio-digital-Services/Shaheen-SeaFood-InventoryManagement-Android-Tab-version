using System;
using System.Drawing;
using System.Windows.Forms;
using Shaheen_InventoryManagement_Android.Helpers;
using Shaheen_InventoryManagement_Android.Controls;

namespace Shaheen_InventoryManagement_Android.Forms
{
    public class LicenseForm : BaseModalForm
    {
        private ModernTextBox txtCustomerName;
        private ModernTextBox txtLicenseKey;
        private ModernTextBox txtHardwareId;

        public LicenseForm()
        {
            this.TitleText = LocalizationManager.GetString("License_Title", "Software Activation");
            this.Size = new Size(500, 440);
            LocalizationManager.ApplyRTL(this);

            TableLayoutPanel tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = Color.Transparent
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // Description
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));   // Hardware ID
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));   // Customer Name
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));   // License Key

            Label lblDesc = new Label
            {
                Text = LocalizationManager.GetString("License_Msg_Activate", "This product is unregistered. Please enter your customer name and license key to activate, or start a 30-day trial."),
                AutoSize = false,
                Height = 55,
                Font = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.SecondaryColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10),
                TextAlign = LocalizationManager.IsArabic ? ContentAlignment.TopRight : ContentAlignment.TopLeft
            };

            // Hardware ID section: TableLayoutPanel to put textbox and Copy button side by side
            TableLayoutPanel tlpRepoHw = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = Color.Transparent
            };
            tlpRepoHw.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRepoHw.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85F));

            txtHardwareId = new ModernTextBox
            {
                LabelText = LocalizationManager.GetString("License_HardwareId", "Hardware ID"),
                Text = HardwareInfo.GetShortHardwareId(),
                ReadOnly = true,
                Dock = DockStyle.Fill
            };

            ModernButton btnCopy = new ModernButton
            {
                Text = LocalizationManager.GetString("License_Copy", "Copy"),
                Size = new Size(75, 35),
                Margin = new Padding(10, 25, 0, 0), // Align with input field
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnCopy.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(txtHardwareId.Text);
                    MessageHelper.ShowSuccess(LocalizationManager.GetString("License_Copied", "Hardware ID copied to clipboard!"));
                }
                catch (Exception ex)
                {
                    MessageHelper.ShowError(LocalizationManager.GetString("License_CopyFailed", "Failed to copy to clipboard: ") + ex.Message);
                }
            };

            if (LocalizationManager.IsArabic)
            {
                tlpRepoHw.Controls.Add(btnCopy, 0, 0);
                tlpRepoHw.Controls.Add(txtHardwareId, 1, 0);
            }
            else
            {
                tlpRepoHw.Controls.Add(txtHardwareId, 0, 0);
                tlpRepoHw.Controls.Add(btnCopy, 1, 0);
            }

            // Customer Name
            txtCustomerName = new ModernTextBox
            {
                LabelText = LocalizationManager.GetString("License_CustomerName", "Customer / Company Name"),
                Dock = DockStyle.Fill,
                IsRequired = true
            };

            // License Key
            txtLicenseKey = new ModernTextBox
            {
                LabelText = LocalizationManager.GetString("License_Key", "License Key"),
                Dock = DockStyle.Fill,
                IsRequired = true
            };

            tlp.Controls.Add(lblDesc, 0, 0);
            tlp.Controls.Add(tlpRepoHw, 0, 1);
            tlp.Controls.Add(txtCustomerName, 0, 2);
            tlp.Controls.Add(txtLicenseKey, 0, 3);

            this.ContentPanel.Controls.Add(tlp);

            SetFooterButtons(
                LocalizationManager.GetString("License_Activate", "Activate"),
                LocalizationManager.GetString("License_StartTrial", "Start Trial"),
                (s, e) => ActivateLicense(),
                (s, e) => StartTrial(),
                LocalizationManager.GetString("License_Exit", "Exit"),
                (s, e) => { DialogResult = DialogResult.Cancel; Close(); }
            );

            this.Shown += (s, e) => {
                txtCustomerName.Focus();
            };
        }

        private void ActivateLicense()
        {
            string key = txtLicenseKey.Text.Trim();
            string customer = txtCustomerName.Text.Trim();

            if (string.IsNullOrWhiteSpace(customer))
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("License_CustomerRequired", "Customer Name is required."));
                txtCustomerName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("License_KeyRequired", "License Key is required."));
                txtLicenseKey.Focus();
                return;
            }

            var activated = LicenseManager.ActivateLicense(key, customer);
            if (activated != null)
            {
                MessageHelper.ShowSuccess(LocalizationManager.GetString("License_ActivatedSuccessfully", "License activated successfully!"));
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageHelper.ShowError(LocalizationManager.GetString("License_ActivationFailed", "Invalid License Key or Customer Name for this machine."));
            }
        }

        private void StartTrial()
        {
            LicenseKey existing = LicenseManager.GetCurrentLicense();
            if (existing != null && existing.IsTrial() && DateTime.Now > existing.ExpirationDate)
            {
                MessageHelper.ShowError(LocalizationManager.GetString("License_TrialExpired", "Your trial period has already expired. Please activate a full license key."));
                return;
            }

            var trialLicense = LicenseManager.StartTrial();
            if (trialLicense != null)
            {
                MessageHelper.ShowSuccess(LocalizationManager.GetString("License_TrialStarted", "30-day trial period started successfully!"));
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageHelper.ShowError(LocalizationManager.GetString("License_TrialFailed", "Failed to start trial. Please contact support."));
            }
        }
    }
}
