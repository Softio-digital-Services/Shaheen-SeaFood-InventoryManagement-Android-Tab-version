namespace GenericInventorySystem.Forms
{
    partial class AddPartForm
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
            this.components = new System.ComponentModel.Container();
            
            this.txtBarcode = new GenericInventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0) };
            this.btnScan = new GenericInventorySystem.Controls.ModernButton { Text = "Scan", Dock = DockStyle.Bottom, Height = 42, Margin = new Padding(0) };
            this.txtPartName = new GenericInventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10), IsRequired = true };
            this.txtPartNumber = new GenericInventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0) };
            this.txtLocation = new GenericInventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0) };
            this.txtShelf = new GenericInventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(5, 0, 0, 0) };
            
            this.numQuantity = new GenericInventorySystem.Controls.ModernNumericUpDown { Maximum = 10000, Dock = DockStyle.Fill };
            this.numMinStock = new GenericInventorySystem.Controls.ModernNumericUpDown { Maximum = 10000, Dock = DockStyle.Fill };
            this.numPrice = new GenericInventorySystem.Controls.ModernNumericUpDown { DecimalPlaces = 2, Maximum = 10000, Dock = DockStyle.Fill };
            
            this.lblCategory = new System.Windows.Forms.Label { AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeConfig.TextColorDark };
            this.cmbCategory = new System.Windows.Forms.ComboBox { Font = new Font("Segoe UI", 10F), Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems };
            this.lblStatus = new System.Windows.Forms.Label { AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeConfig.TextColorDark };
            this.cmbStatus = new System.Windows.Forms.ComboBox { Font = new Font("Segoe UI", 10F), Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            
            this.pbImage = new System.Windows.Forms.PictureBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.WhiteSmoke };
            this.btnUpload = new GenericInventorySystem.Controls.ModernButton { Text = "Upload Image", Dock = DockStyle.Top, Height = 35, Margin = new Padding(0) };

            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).BeginInit();
            this.SuspendLayout();

            TableLayoutPanel tlpOuter = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 1, AutoSize = true, Padding = new Padding(10) };
            tlpOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tlpOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            TableLayoutPanel tlpFields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 9, AutoSize = true };
            tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for(int i=0; i<9; i++) tlpFields.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Barcode Row
            TableLayoutPanel pnlBarcode = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Height = 67, Margin = new Padding(0, 0, 0, 10) };
            pnlBarcode.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlBarcode.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            pnlBarcode.Controls.Add(txtBarcode, 0, 0);
            pnlBarcode.Controls.Add(btnScan, 1, 0);
            tlpFields.Controls.Add(pnlBarcode, 0, 0);

            tlpFields.Controls.Add(txtPartName, 0, 1);
            
            // SKU Row with Auto-Generate
            TableLayoutPanel pnlSKU = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Height = 67, Margin = new Padding(0, 0, 0, 10) };
            pnlSKU.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlSKU.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            pnlSKU.Controls.Add(txtPartNumber, 0, 0);
            this.btnAutoSKU = new GenericInventorySystem.Controls.ModernButton { Text = "âœ¨ Auto", Dock = DockStyle.Bottom, Height = 42, Margin = new Padding(0) };
            pnlSKU.Controls.Add(btnAutoSKU, 1, 0);
            tlpFields.Controls.Add(pnlSKU, 0, 2);

            // Stock/Price Row
            TableLayoutPanel pnlStock = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, Height = 67, Margin = new Padding(0, 0, 0, 20) };
            pnlStock.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            pnlStock.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            pnlStock.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            pnlStock.Controls.Add(numQuantity, 0, 0);
            pnlStock.SetRowSpan(numQuantity, 2);
            pnlStock.Controls.Add(numMinStock, 1, 0);
            pnlStock.SetRowSpan(numMinStock, 2);
            pnlStock.Controls.Add(numPrice, 2, 0);
            pnlStock.SetRowSpan(numPrice, 2);
            tlpFields.Controls.Add(pnlStock, 0, 3);
            
            // Location Row
            TableLayoutPanel pnlLoc = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Height = 67, Margin = new Padding(0, 0, 0, 10) };
            pnlLoc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlLoc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlLoc.Controls.Add(txtLocation, 0, 0);
            pnlLoc.Controls.Add(txtShelf, 1, 0);
            tlpFields.Controls.Add(pnlLoc, 0, 4);

            // Category
            tlpFields.Controls.Add(lblCategory, 0, 5);
            Panel pnlCat = ThemeConfig.WrapInStyledInput(cmbCategory, 42);
            pnlCat.Dock = DockStyle.Fill;
            pnlCat.Margin = new Padding(0, 0, 0, 10);
            tlpFields.Controls.Add(pnlCat, 0, 6);

            // Status
            tlpFields.Controls.Add(lblStatus, 0, 7);
            Panel pnlStatus = ThemeConfig.WrapInStyledInput(cmbStatus, 42);
            pnlStatus.Dock = DockStyle.Fill;
            tlpFields.Controls.Add(pnlStatus, 0, 8);

            // Image Column
            TableLayoutPanel pnlImage = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 2, Padding = new Padding(15, 0, 0, 0), AutoSize = true };
            pnlImage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlImage.RowStyles.Add(new RowStyle(SizeType.Absolute, 215F));
            pnlImage.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlImage.Controls.Add(pbImage, 0, 0);
            pnlImage.Controls.Add(btnUpload, 0, 1);

            tlpOuter.Controls.Add(tlpFields, 0, 0);
            tlpOuter.Controls.Add(pnlImage, 1, 0);

            this.ClientSize = new System.Drawing.Size(650, 700);
            this.ContentPanel.Controls.Add(tlpOuter);

            btnScan.Click += new System.EventHandler(this.btnScan_Click);
            btnUpload.Click += new System.EventHandler(this.btnUpload_Click);

            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Controls
        private GenericInventorySystem.Controls.ModernTextBox txtBarcode;
        private GenericInventorySystem.Controls.ModernButton btnScan;
        private GenericInventorySystem.Controls.ModernTextBox txtPartName;
        private GenericInventorySystem.Controls.ModernTextBox txtPartNumber;
        private GenericInventorySystem.Controls.ModernButton btnAutoSKU;
        private GenericInventorySystem.Controls.ModernTextBox txtLocation;
        private GenericInventorySystem.Controls.ModernTextBox txtShelf;
        
        private GenericInventorySystem.Controls.ModernNumericUpDown numQuantity;
        private GenericInventorySystem.Controls.ModernNumericUpDown numMinStock;
        private GenericInventorySystem.Controls.ModernNumericUpDown numPrice;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ComboBox cmbStatus;
        
        private System.Windows.Forms.PictureBox pbImage;
        private GenericInventorySystem.Controls.ModernButton btnUpload;
    }
}
