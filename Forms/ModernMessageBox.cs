using System;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    public partial class ModernMessageBox : BaseModalForm
    {
        private Label lblMessage;
        private PictureBox picIcon;

        public ModernMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            InitializeModernUI();
            
            this.TitleText = caption;
            this.lblMessage.Text = text;
            
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            SetIcon(icon);
            SetButtons(buttons, isArabic);
            
            // Adjust size based on message length
            AdjustSize(text);
        }

        private void InitializeModernUI()
        {
            this.Size = new Size(400, 200);
            this.EnforceMinWidth = false;

            // Content Area
            TableLayoutPanel tlpContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(20),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            picIcon = new PictureBox
            {
                Size = new Size(48, 48),
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(0, 5, 10, 0)
            };

            lblMessage = new Label
            {
                Text = "Message Text",
                Font = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.TextColorDark,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true
            };

            tlpContent.Controls.Add(picIcon, 0, 0);
            tlpContent.Controls.Add(lblMessage, 1, 0);

            this.ContentPanel.AddControl(tlpContent);
        }

        private void SetIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Error:
                    picIcon.Image = SystemIcons.Error.ToBitmap();
                    this.BorderColor = ThemeConfig.DangerColor;
                    break;
                case MessageBoxIcon.Information:
                    picIcon.Image = SystemIcons.Information.ToBitmap();
                    this.BorderColor = ThemeConfig.PrimaryColor;
                    break;
                case MessageBoxIcon.Question:
                    picIcon.Image = SystemIcons.Question.ToBitmap();
                    this.BorderColor = ThemeConfig.PrimaryColor;
                    break;
                case MessageBoxIcon.Exclamation:
                    picIcon.Image = SystemIcons.Warning.ToBitmap();
                    this.BorderColor = ThemeConfig.WarningColor;
                    break;
                default:
                    picIcon.Visible = false;
                    this.BorderColor = ThemeConfig.PrimaryColor;
                    break;
            }
        }

        private void SetButtons(MessageBoxButtons buttons, bool isArabic)
        {
            string ok = isArabic ? "موافق" : "OK";
            string cancel = isArabic ? "إلغاء" : "Cancel";
            string yes = isArabic ? "نعم" : "Yes";
            string no = isArabic ? "لا" : "No";

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    SetFooterButtons(ok, "", (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); }, null);
                    break;

                case MessageBoxButtons.OKCancel:
                    SetFooterButtons(ok, cancel, 
                        (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); }, 
                        (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); });
                    break;

                case MessageBoxButtons.YesNo:
                    SetFooterButtons(yes, no, 
                        (s, e) => { this.DialogResult = DialogResult.Yes; this.Close(); }, 
                        (s, e) => { this.DialogResult = DialogResult.No; this.Close(); });
                    break;

                case MessageBoxButtons.YesNoCancel:
                    SetFooterButtons(yes, no, 
                        (s, e) => { this.DialogResult = DialogResult.Yes; this.Close(); }, 
                        (s, e) => { this.DialogResult = DialogResult.No; this.Close(); },
                        cancel, (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); });
                    break;
            }
        }

        private void AdjustSize(string text)
        {
            // Initial size estimate
            this.Width = 400;
            
            // Allow BaseModalForm.OnLoad to handle the final FitToContent
            // But we can trigger it early if we want immediate results
            FitToContent();
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            using (var msgBox = new ModernMessageBox(text, caption, buttons, icon))
            {
                return msgBox.ShowDialog();
            }
        }
    }
}
