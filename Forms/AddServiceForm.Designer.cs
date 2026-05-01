using System.Windows.Forms;
using System.Drawing;

namespace GenericInventorySystem.Forms
{
    partial class AddServiceForm
    {
        private System.ComponentModel.IContainer components = null;

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
            this.txtName = new GenericInventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15), IsRequired = true };
            this.txtCode = new GenericInventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0) };
            this.btnAutoSKU = new GenericInventorySystem.Controls.ModernButton { Text = " Auto", Dock = DockStyle.Bottom, Height = 42, Margin = new Padding(0) };
            this.numPrice = new GenericInventorySystem.Controls.ModernNumericUpDown { DecimalPlaces = 2, Maximum = 10000, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15) };
            this.lblStatus = new System.Windows.Forms.Label { AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeConfig.TextColorDark, Margin = new Padding(0, 0, 0, 5) };
            this.cmbStatus = new System.Windows.Forms.ComboBox { Font = new Font("Segoe UI", 10F), Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            this.pbImage = new System.Windows.Forms.PictureBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke };
            this.btnUpload = new GenericInventorySystem.Controls.ModernButton { Text = "Upload Image", Dock = DockStyle.Top, Height = 35, Margin = new Padding(0) };

            this.SuspendLayout();

            TableLayoutPanel tlpOuter = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 1, AutoSize = true, Padding = new Padding(20) };
            tlpOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tlpOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            TableLayoutPanel tlpFields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, AutoSize = true };
            tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            tlpFields.Controls.Add(txtName, 0, 0);

            // SKU Row
            TableLayoutPanel pnlSKU = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Height = 67, Margin = new Padding(0, 0, 0, 10) };
            pnlSKU.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlSKU.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            pnlSKU.Controls.Add(txtCode, 0, 0);
            pnlSKU.Controls.Add(btnAutoSKU, 1, 0);
            tlpFields.Controls.Add(pnlSKU, 0, 1);

            tlpFields.Controls.Add(numPrice, 0, 2);
            tlpFields.Controls.Add(lblStatus, 0, 3);
            
            Panel pnlStatus = ThemeConfig.WrapInStyledInput(cmbStatus, 42);
            pnlStatus.Dock = DockStyle.Fill;
            tlpFields.Controls.Add(pnlStatus, 0, 4);

            // Image Column
            TableLayoutPanel pnlImage = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 2, Padding = new Padding(15, 0, 0, 0), AutoSize = true };
            pnlImage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlImage.RowStyles.Add(new RowStyle(SizeType.Absolute, 215F));
            pnlImage.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlImage.Controls.Add(pbImage, 0, 0);
            pnlImage.Controls.Add(btnUpload, 0, 1);

            tlpOuter.Controls.Add(tlpFields, 0, 0);
            tlpOuter.Controls.Add(pnlImage, 1, 0);

            this.ClientSize = new System.Drawing.Size(650, 480);
            this.ContentPanel.Controls.Add(tlpOuter);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private GenericInventorySystem.Controls.ModernTextBox txtName;
        private GenericInventorySystem.Controls.ModernTextBox txtCode;
        private GenericInventorySystem.Controls.ModernButton btnAutoSKU;
        private GenericInventorySystem.Controls.ModernNumericUpDown numPrice;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ComboBox cmbStatus;
        private System.Windows.Forms.PictureBox pbImage;
        private GenericInventorySystem.Controls.ModernButton btnUpload;
    }
}
