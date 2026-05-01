using System;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Controls;

namespace GenericInventorySystem.Forms
{
    public class OrderIdPromptForm : BaseModalForm
    {
        private ModernTextBox txtOrderId;
        public int OrderId { get; private set; }

        public OrderIdPromptForm()
        {
            this.TitleText = LocalizationManager.IsArabic ? "Ø¥Ø±Ø¬Ø§Ø¹ Ø·Ù„Ø¨" : "Return Order";
            this.Size = new Size(400, 280);

            Panel pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            Label lblDesc = new Label
            {
                Text = LocalizationManager.IsArabic ? "Ø£Ø¯Ø®Ù„ Ø±Ù‚Ù… Ø§Ù„Ø·Ù„Ø¨ Ù„Ù…Ø¹Ø§Ù„Ø¬Ø© Ø§Ù„Ù…Ø±ØªØ¬Ø¹:" : "Enter the Order ID to process the return:",
                AutoSize = true,
                Font = ThemeConfig.StandardFont,
                Location = new Point(20, 10),
                ForeColor = ThemeConfig.SecondaryColor
            };
            
            txtOrderId = new ModernTextBox
            {
                LabelText = LocalizationManager.IsArabic ? "Ø±Ù‚Ù… Ø§Ù„Ø·Ù„Ø¨:" : "Order ID:",
                Location = new Point(20, 40),
                Width = 360
            };
            txtOrderId.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Submit(); };
            
            pnl.Controls.Add(lblDesc);
            pnl.Controls.Add(txtOrderId);

            this.ContentPanel.Controls.Add(pnl);

            SetFooterButtons(
                LocalizationManager.IsArabic ? "Ø§Ø³ØªÙ…Ø±Ø§Ø±" : "Continue",
                LocalizationManager.IsArabic ? "Ø¥Ù„ØºØ§Ø¡" : "Cancel",
                (s, e) => Submit(),
                (s, e) => { DialogResult = DialogResult.Cancel; Close(); }
            );
            
            this.Shown += (s, e) => {
                txtOrderId.Focus();
                this.ActiveControl = txtOrderId;
            };
        }

        private void Submit()
        {
            if (int.TryParse(txtOrderId.Text, out int id) && id > 0)
            {
                OrderId = id;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "ÙŠØ±Ø¬Ù‰ Ø¥Ø¯Ø®Ø§Ù„ Ø±Ù‚Ù… Ø·Ù„Ø¨ ØµØ­ÙŠØ­." : "Please enter a valid numeric Order ID.");
                txtOrderId.Focus();
            }
        }
    }
}
