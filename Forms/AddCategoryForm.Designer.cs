namespace GenericInventorySystem.Forms
{
    partial class AddCategoryForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtName = new GenericInventorySystem.Controls.ModernTextBox();
            this.txtDesc = new GenericInventorySystem.Controls.ModernTextBox();
            this.btnSave = new GenericInventorySystem.Controls.ModernButton();
            this.btnCancel = new GenericInventorySystem.Controls.ModernButton();
            this.SuspendLayout();

            // 
            // txtName
            // 
            this.txtName.LabelText = "Category Name";
            this.txtName.Location = new System.Drawing.Point(25, 70);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(350, 60);
            this.txtName.TabIndex = 0;

            // 
            // txtDesc
            // 
            this.txtDesc.LabelText = "Description";
            this.txtDesc.Location = new System.Drawing.Point(25, 140);
            this.txtDesc.Name = "txtDesc";
            this.txtDesc.Size = new System.Drawing.Size(350, 60);
            this.txtDesc.TabIndex = 1;

            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(25, 220);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(160, 40);
            this.btnSave.Text = "Save Category";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(215, 220);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(160, 40);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += (s, e) => { this.DialogResult = System.Windows.Forms.DialogResult.Cancel; this.Close(); };

            // 
            // AddCategoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.ContentPanel.Controls.Add(this.btnCancel);
            this.ContentPanel.Controls.Add(this.btnSave);
            this.ContentPanel.Controls.Add(this.txtDesc);
            this.ContentPanel.Controls.Add(this.txtName);
            this.Name = "AddCategoryForm";
            this.Text = "Add Category";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
        }

        private GenericInventorySystem.Controls.ModernTextBox txtName;
        private GenericInventorySystem.Controls.ModernTextBox txtDesc;
        private GenericInventorySystem.Controls.ModernButton btnSave;
        private GenericInventorySystem.Controls.ModernButton btnCancel;
    }
}
