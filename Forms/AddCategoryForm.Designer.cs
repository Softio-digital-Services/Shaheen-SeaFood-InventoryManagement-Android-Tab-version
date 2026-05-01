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
            this.txtName = new GenericInventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10), IsRequired = true };
            this.txtDesc = new GenericInventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15) };
            
            this.SuspendLayout();

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(25),
                AutoSize = true
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.txtName.LabelText = "Category Name";
            tlpMain.Controls.Add(this.txtName, 0, 0);

            this.txtDesc.LabelText = "Description";
            tlpMain.Controls.Add(this.txtDesc, 0, 1);

            this.ClientSize = new System.Drawing.Size(450, 320);
            this.ContentPanel.Controls.Add(tlpMain);

            this.Name = "AddCategoryForm";
            this.Text = "Add Category";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private GenericInventorySystem.Controls.ModernTextBox txtName;
        private GenericInventorySystem.Controls.ModernTextBox txtDesc;
    }
}
