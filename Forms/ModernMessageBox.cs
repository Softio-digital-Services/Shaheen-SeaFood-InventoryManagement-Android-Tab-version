using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    public partial class ModernMessageBox : BaseModalForm
    {
        // Fields
        private Color primaryColor = Color.CornflowerBlue;

        // Custom properties
        public Color PrimaryColor
        {
            get { return primaryColor; }
            set
            {
                primaryColor = value;
                // Border/Accent logic if needed, otherwise handled by BaseModalForm styling
            }
        }

        public ModernMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            InitializeComponent();
            InitializeItems();
            
            this.labelMessage.Text = text;
            this.TitleText = caption; // Use BaseModalForm property
            
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            SetIcon(icon);
            SetButtons(buttons, isArabic);
        }

        private void InitializeItems()
        {
            this.labelMessage.MaximumSize = new Size(350, 0); // Word wrap
            this.Size = new Size(450, 250); // Default Message Box Size
        }

        private void SetIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Error:
                    this.pictureBoxIcon.Image = SystemIcons.Error.ToBitmap();
                    PrimaryColor = ThemeConfig.DangerColor; // Red
                    break;
                case MessageBoxIcon.Information:
                    this.pictureBoxIcon.Image = SystemIcons.Information.ToBitmap();
                    PrimaryColor = ThemeConfig.PrimaryColor; // Blue
                    break;
                case MessageBoxIcon.Question:
                    this.pictureBoxIcon.Image = SystemIcons.Question.ToBitmap();
                    PrimaryColor = ThemeConfig.PrimaryColor; 
                    break;
                case MessageBoxIcon.Exclamation:
                    this.pictureBoxIcon.Image = SystemIcons.Warning.ToBitmap();
                    PrimaryColor = ThemeConfig.WarningColor; // Orange
                    break;
                case MessageBoxIcon.None: 
                    this.pictureBoxIcon.Image = null;
                    PrimaryColor = ThemeConfig.PrimaryColor;
                    break;
            }
        }

        private void SetButtons(MessageBoxButtons buttons, bool isArabic)
        {
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    button1.Visible = true;
                    button1.Text = isArabic ? "موافق" : "OK";
                    button1.Location = new Point(this.panelButtons.Width - button1.Width - 20, 10);
                    ThemeConfig.ApplyPrimaryButton(button1);
                    button1.DialogResult = DialogResult.OK; // Set result
                    
                    button2.Visible = false;
                    button3.Visible = false;
                    this.AcceptButton = button1;
                    break;

                case MessageBoxButtons.OKCancel:
                    button1.Visible = true;
                    button1.Text = isArabic ? "موافق" : "OK";
                    button1.Location = new Point(this.panelButtons.Width - button1.Width - button2.Width - 30, 10);
                    ThemeConfig.ApplyPrimaryButton(button1);
                    button1.DialogResult = DialogResult.OK;

                    button2.Visible = true;
                    button2.Text = isArabic ? "إلغاء" : "Cancel";
                    button2.Location = new Point(this.panelButtons.Width - button2.Width - 20, 10);
                    ThemeConfig.ApplySecondaryButton(button2);
                    button2.DialogResult = DialogResult.Cancel;
                    
                    button3.Visible = false;
                    this.AcceptButton = button1;
                    this.CancelButton = button2;
                    break;

                case MessageBoxButtons.YesNo:
                    button1.Visible = true;
                    button1.Text = isArabic ? "نعم" : "Yes";
                    button1.Location = new Point(this.panelButtons.Width - button1.Width - button2.Width - 30, 10);
                    ThemeConfig.ApplyPrimaryButton(button1);
                    button1.DialogResult = DialogResult.Yes;

                    button2.Visible = true;
                    button2.Text = isArabic ? "لا" : "No";
                    button2.Location = new Point(this.panelButtons.Width - button2.Width - 20, 10);
                    ThemeConfig.ApplySecondaryButton(button2);
                    button2.DialogResult = DialogResult.No;

                    button3.Visible = false;
                    this.AcceptButton = button1;
                    break;

                case MessageBoxButtons.YesNoCancel:
                    button1.Visible = true;
                    button1.Text = isArabic ? "نعم" : "Yes";
                    button1.Location = new Point(this.panelButtons.Width - button1.Width - button2.Width - button3.Width - 40, 10);
                    ThemeConfig.ApplyPrimaryButton(button1);
                    button1.DialogResult = DialogResult.Yes;

                    button2.Visible = true;
                    button2.Text = isArabic ? "لا" : "No";
                    button2.Location = new Point(this.panelButtons.Width - button2.Width - button3.Width - 30, 10);
                    ThemeConfig.ApplySecondaryButton(button2);
                    button2.DialogResult = DialogResult.No;

                    button3.Visible = true;
                    button3.Text = isArabic ? "إلغاء" : "Cancel";
                    button3.Location = new Point(this.panelButtons.Width - button3.Width - 20, 10);
                    ThemeConfig.ApplySecondaryButton(button3);
                    button3.DialogResult = DialogResult.Cancel;

                    this.AcceptButton = button1;
                    this.CancelButton = button3;
                    break;

                case MessageBoxButtons.RetryCancel:
                    button1.Visible = true;
                    button1.Text = isArabic ? "إعادة المحاولة" : "Retry";
                    button1.Location = new Point(this.panelButtons.Width - button1.Width - button2.Width - 30, 10);
                    ThemeConfig.ApplyPrimaryButton(button1);
                    button1.DialogResult = DialogResult.Retry;

                    button2.Visible = true;
                    button2.Text = isArabic ? "إلغاء" : "Cancel";
                    button2.Location = new Point(this.panelButtons.Width - button2.Width - 20, 10);
                    ThemeConfig.ApplySecondaryButton(button2);
                    button2.DialogResult = DialogResult.Cancel;

                    button3.Visible = false;
                    this.AcceptButton = button1;
                    this.CancelButton = button2;
                    break;

                case MessageBoxButtons.AbortRetryIgnore:
                    button1.Visible = true;
                    button1.Text = isArabic ? "إيقاف" : "Abort";
                    button1.Location = new Point(this.panelButtons.Width - button1.Width - button2.Width - button3.Width - 40, 10);
                    ThemeConfig.ApplySecondaryButton(button1);
                    button1.DialogResult = DialogResult.Abort;

                    button2.Visible = true;
                    button2.Text = isArabic ? "إعادة" : "Retry";
                    button2.Location = new Point(this.panelButtons.Width - button2.Width - button3.Width - 30, 10);
                    ThemeConfig.ApplyPrimaryButton(button2);
                    button2.DialogResult = DialogResult.Retry;

                    button3.Visible = true;
                    button3.Text = isArabic ? "تجاهل" : "Ignore";
                    button3.Location = new Point(this.panelButtons.Width - button3.Width - 20, 10);
                    ThemeConfig.ApplySecondaryButton(button3);
                    button3.DialogResult = DialogResult.Ignore;

                    this.AcceptButton = button2;
                    break;
            }
        }

        // Static Show Method (The entry point)
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            try { 
                System.IO.File.AppendAllText(System.IO.Path.Combine(Application.StartupPath, "crash.txt"), DateTime.Now.ToString() + "\nMBox [" + caption + "]\n" + text + "\n" + new System.Diagnostics.StackTrace(true).ToString() + "\n\n"); 
            } catch {}

            DialogResult result;
            using (var msgForm = new ModernMessageBox(text, caption, buttons, icon))
            {
                result = msgForm.ShowDialog();
            }
            return result;
        }
    }
}
