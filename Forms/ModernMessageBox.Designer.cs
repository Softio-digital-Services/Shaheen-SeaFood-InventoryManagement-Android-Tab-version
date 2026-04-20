
namespace GenericInventorySystem.Forms
{
    partial class ModernMessageBox
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelTitleBar;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Panel panelBody;
        private System.Windows.Forms.Label labelCaption;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.PictureBox pictureBoxIcon;
        private GenericInventorySystem.Controls.ModernButton button1;
        private GenericInventorySystem.Controls.ModernButton button2;
        private GenericInventorySystem.Controls.ModernButton button3;
        private System.Windows.Forms.Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTitleBar = new System.Windows.Forms.Panel();
            this.labelCaption = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.button1 = new GenericInventorySystem.Controls.ModernButton();
            this.button2 = new GenericInventorySystem.Controls.ModernButton();
            this.button3 = new GenericInventorySystem.Controls.ModernButton();
            this.panelBody = new System.Windows.Forms.Panel();
            this.labelMessage = new System.Windows.Forms.Label();
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();

            // 
            // panelButtons
            // 
            this.panelButtons.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelButtons.Controls.Add(this.button1);
            this.panelButtons.Controls.Add(this.button2);
            this.panelButtons.Controls.Add(this.button3);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Height = 60;

            // Buttons - Initial setup, positioned dynamically in code
            this.button1.Size = new System.Drawing.Size(100, 35);
            this.button1.Visible = false;
            
            this.button2.Size = new System.Drawing.Size(100, 35);
            this.button2.Visible = false;

            this.button3.Size = new System.Drawing.Size(100, 35);
            this.button3.Visible = false;

            // 
            // panelBody
            // 
            this.panelBody.BackColor = System.Drawing.Color.White;
            this.panelBody.Controls.Add(this.labelMessage);
            this.panelBody.Controls.Add(this.pictureBoxIcon);
            this.panelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBody.Padding = new System.Windows.Forms.Padding(10, 50, 0, 0); // Top padding to avoid overlap with Base Header

            // 
            // pictureBoxIcon
            // 
            this.pictureBoxIcon.Location = new System.Drawing.Point(20, 50); // Shift down
            this.pictureBoxIcon.Size = new System.Drawing.Size(40, 40);
            this.pictureBoxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true; 
            this.labelMessage.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.labelMessage.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);
            this.labelMessage.Location = new System.Drawing.Point(70, 50); // Shift down
            this.labelMessage.Text = "Message Text";

            // 
            // ModernMessageBox
            // 
            this.ClientSize = new System.Drawing.Size(400, 250);
            this.ContentPanel.Controls.Add(this.panelBody);
            this.ContentPanel.Controls.Add(this.panelButtons);
            // FormBorderStyle handled in code
        }
    }
}
