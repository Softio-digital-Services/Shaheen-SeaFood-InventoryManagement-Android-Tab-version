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
            
            // Modern Controls
            this.txtBarcode = new GenericInventorySystem.Controls.ModernTextBox();
            this.btnScan = new GenericInventorySystem.Controls.ModernButton(); // Using ModernButton for consistency
            this.txtPartName = new GenericInventorySystem.Controls.ModernTextBox();
            this.txtPartNumber = new GenericInventorySystem.Controls.ModernTextBox();
            this.txtLocation = new GenericInventorySystem.Controls.ModernTextBox();
            this.txtShelf = new GenericInventorySystem.Controls.ModernTextBox();
            
            // Numeric Wrappers (Using Labels for consistent looking headers)
            this.lblQuantity = new System.Windows.Forms.Label();
            this.numQuantity = new System.Windows.Forms.NumericUpDown();
            this.lblMinStock = new System.Windows.Forms.Label();
            this.numMinStock = new System.Windows.Forms.NumericUpDown();
            this.lblPrice = new System.Windows.Forms.Label();
            this.numPrice = new System.Windows.Forms.NumericUpDown();
            
            // Combos
            this.lblCategory = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.cmbStatus = new System.Windows.Forms.ComboBox();
            
            // Buttons
            this.btnSave = new GenericInventorySystem.Controls.ModernButton();
            this.btnCancel = new GenericInventorySystem.Controls.ModernButton();
            
            // Image
            this.pbImage = new System.Windows.Forms.PictureBox();
            this.btnUpload = new GenericInventorySystem.Controls.ModernButton();

            ((System.ComponentModel.ISupportInitialize)(this.numQuantity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinStock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPrice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).BeginInit();
            this.SuspendLayout();

            // 
            // txtBarcode
            // 
            this.txtBarcode.LabelText = "Barcode";
            this.txtBarcode.Location = new System.Drawing.Point(20, 60);
            this.txtBarcode.Name = "txtBarcode";
            this.txtBarcode.Size = new System.Drawing.Size(200, 60); // Width leaves room for Scan button
            this.txtBarcode.TabIndex = 0;
            
            // 
            // btnScan
            // 
            this.btnScan.Location = new System.Drawing.Point(230, 85); // Aligned with input box of ModernTextBox
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(80, 35);
            this.btnScan.Text = "Scan";
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);

            // 
            // txtPartName (Product)
            // 
            this.txtPartName.LabelText = "Product";
            this.txtPartName.Location = new System.Drawing.Point(20, 130);
            this.txtPartName.Name = "txtPartName";
            this.txtPartName.Size = new System.Drawing.Size(350, 60);
            this.txtPartName.TabIndex = 2;

            // 
            // txtPartNumber (SKU)
            // 
            this.txtPartNumber.LabelText = "SKU";
            this.txtPartNumber.Location = new System.Drawing.Point(20, 200);
            this.txtPartNumber.Name = "txtPartNumber";
            this.txtPartNumber.Size = new System.Drawing.Size(350, 60);
            this.txtPartNumber.TabIndex = 3;

            // 
            // Numeric Row - Manual Styling to match Modern Look
            // Stock (Quantity)
            this.lblQuantity.AutoSize = true;
            this.lblQuantity.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblQuantity.ForeColor = ThemeConfig.TextColorDark;
            this.lblQuantity.Location = new System.Drawing.Point(25, 270);
            this.lblQuantity.Name = "lblQuantity";
            this.lblQuantity.Text = "Stock";
            
            this.numQuantity.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numQuantity.Location = new System.Drawing.Point(25, 295);
            this.numQuantity.Size = new System.Drawing.Size(100, 27);
            this.numQuantity.Maximum = 10000;
            this.numQuantity.TabIndex = 4;

            // Min Stock
            this.lblMinStock.AutoSize = true;
            this.lblMinStock.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblMinStock.ForeColor = ThemeConfig.TextColorDark;
            this.lblMinStock.Location = new System.Drawing.Point(135, 270);
            this.lblMinStock.Name = "lblMinStock";
            this.lblMinStock.Text = "Min Stock";

            this.numMinStock.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numMinStock.Location = new System.Drawing.Point(135, 295);
            this.numMinStock.Size = new System.Drawing.Size(100, 27);
            this.numMinStock.Maximum = 10000;
            this.numMinStock.TabIndex = 5;

            // Price
            this.lblPrice.AutoSize = true;
            this.lblPrice.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPrice.ForeColor = ThemeConfig.TextColorDark;
            this.lblPrice.Location = new System.Drawing.Point(245, 270);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Text = "Price ($)";

            this.numPrice.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numPrice.Location = new System.Drawing.Point(245, 295);
            this.numPrice.Size = new System.Drawing.Size(100, 27);
            this.numPrice.DecimalPlaces = 2;
            this.numPrice.Maximum = 10000;
            this.numPrice.TabIndex = 6;

            // 
            // txtLocation (Aisle)
            // 
            this.txtLocation.LabelText = "Location (Aisle)";
            this.txtLocation.Location = new System.Drawing.Point(20, 330);
            this.txtLocation.Name = "txtLocation";
            this.txtLocation.Size = new System.Drawing.Size(160, 60);
            this.txtLocation.TabIndex = 6;

            // 
            // txtShelf
            // 
            this.txtShelf.LabelText = "Shelf / Bin";
            this.txtShelf.Location = new System.Drawing.Point(190, 330);
            this.txtShelf.Name = "txtShelf";
            this.txtShelf.Size = new System.Drawing.Size(160, 60);
            this.txtShelf.TabIndex = 7;

            // 
            // Category
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCategory.ForeColor = ThemeConfig.TextColorDark;
            this.lblCategory.Location = new System.Drawing.Point(25, 400);
            this.lblCategory.Text = "Category";

            this.cmbCategory.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbCategory.Location = new System.Drawing.Point(25, 425);
            this.cmbCategory.Size = new System.Drawing.Size(325, 25);
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cmbCategory.TabIndex = 8;

            // 
            // Status
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = ThemeConfig.TextColorDark;
            this.lblStatus.Location = new System.Drawing.Point(25, 460);
            this.lblStatus.Text = "Status";

            this.cmbStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbStatus.Location = new System.Drawing.Point(25, 485);
            this.cmbStatus.Size = new System.Drawing.Size(325, 25);
            this.cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            this.cmbStatus.SelectedIndex = 0;
            this.cmbStatus.TabIndex = 9;

            // 
            // Image Section (Right Side)
            // 
            this.pbImage.Location = new System.Drawing.Point(390, 85);
            this.pbImage.Name = "pbImage";
            this.pbImage.Size = new System.Drawing.Size(160, 160);
            this.pbImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbImage.BackColor = System.Drawing.Color.WhiteSmoke;

            this.btnUpload.Location = new System.Drawing.Point(390, 255);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(160, 35);
            this.btnUpload.Text = "Upload Image";
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            
            // 
            // Buttons
            // 
            this.btnSave.Location = new System.Drawing.Point(25, 540); // Bottom Left
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(150, 40);
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            this.btnCancel.Location = new System.Drawing.Point(190, 540);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 40);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // 
            // AddPartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 600); // Expanded Size
            this.ContentPanel.Controls.Add(this.btnCancel);
            this.ContentPanel.Controls.Add(this.btnSave);
            this.ContentPanel.Controls.Add(this.btnUpload);
            this.ContentPanel.Controls.Add(this.pbImage);
            this.ContentPanel.Controls.Add(this.cmbStatus);
            this.ContentPanel.Controls.Add(this.lblStatus);
            this.ContentPanel.Controls.Add(this.cmbCategory);
            this.ContentPanel.Controls.Add(this.lblCategory);
            this.ContentPanel.Controls.Add(this.txtShelf);
            this.ContentPanel.Controls.Add(this.txtLocation);
            this.ContentPanel.Controls.Add(this.numPrice);
            this.ContentPanel.Controls.Add(this.lblPrice);
            this.ContentPanel.Controls.Add(this.numMinStock);
            this.ContentPanel.Controls.Add(this.lblMinStock);
            this.ContentPanel.Controls.Add(this.numQuantity);
            this.ContentPanel.Controls.Add(this.lblQuantity);
            this.ContentPanel.Controls.Add(this.txtPartNumber);
            this.ContentPanel.Controls.Add(this.txtPartName);
            this.ContentPanel.Controls.Add(this.btnScan);
            this.ContentPanel.Controls.Add(this.txtBarcode);
            this.Name = "AddPartForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Part";
            ((System.ComponentModel.ISupportInitialize)(this.numQuantity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinStock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPrice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Controls
        private GenericInventorySystem.Controls.ModernTextBox txtBarcode;
        private GenericInventorySystem.Controls.ModernButton btnScan;
        private GenericInventorySystem.Controls.ModernTextBox txtPartName;
        private GenericInventorySystem.Controls.ModernTextBox txtPartNumber;
        private GenericInventorySystem.Controls.ModernTextBox txtLocation;
        private GenericInventorySystem.Controls.ModernTextBox txtShelf;
        
        private System.Windows.Forms.Label lblQuantity;
        private System.Windows.Forms.NumericUpDown numQuantity;
        private System.Windows.Forms.Label lblMinStock;
        private System.Windows.Forms.NumericUpDown numMinStock;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.NumericUpDown numPrice;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ComboBox cmbStatus;
        
        private GenericInventorySystem.Controls.ModernButton btnSave;
        private GenericInventorySystem.Controls.ModernButton btnCancel;
        private System.Windows.Forms.PictureBox pbImage;
        private GenericInventorySystem.Controls.ModernButton btnUpload;
    }
}
