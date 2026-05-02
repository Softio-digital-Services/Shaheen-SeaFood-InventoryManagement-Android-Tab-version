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
            this.TitleText = "Return Order";
            this.Size = new Size(400, 280);

            Panel pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            Label lblDesc = new Label
            {
                Text = LocalizationManager.GetString("Msg_EnterOrderId"),
                AutoSize = true,
                Font = ThemeConfig.StandardFont,
                Location = new Point(20, 10),
                ForeColor = ThemeConfig.SecondaryColor
            };
            
            txtOrderId = new ModernTextBox
            {
                LabelText = LocalizationManager.GetString("Msg_OrderId") ?? "Order ID:",
                Location = new Point(20, 40),
                Width = 360,
                Height = 67 // 25 label + 42 input
            };
            txtOrderId.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Submit(); };
            
            pnl.Controls.Add(lblDesc);
            pnl.Controls.Add(txtOrderId);

            this.ContentPanel.Controls.Add(pnl);

            SetFooterButtons(
                LocalizationManager.GetString("Tran_Continue") ?? "Continue",
                LocalizationManager.GetString("Popup_Cancel"),
                (s, e) => Submit(),
                (s, e) => { DialogResult = DialogResult.Cancel; Close(); }
            );
            
            this.Shown += (s, e) => {
                txtOrderId.Focus();
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
                MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_InvalidOrderId"));
                txtOrderId.Focus();
            }
        }
    }
}
