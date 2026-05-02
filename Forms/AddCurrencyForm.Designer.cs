using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    partial class AddCurrencyForm
    {
        private void InitializeComponent()
        {
            this.txtCode = new GenericInventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15), IsRequired = true };
            this.txtName = new GenericInventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15), IsRequired = true };
            this.txtSymbol = new GenericInventorySystem.Controls.ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 15) };
            this.numRate = new GenericInventorySystem.Controls.ModernNumericUpDown { DecimalPlaces = 4, Maximum = 1000000, Dock = DockStyle.Fill, Margin = new Padding(0) };
            this.btnFetch = new GenericInventorySystem.Controls.ModernButton { Text = "Fetch", Width = 110, Height = 42, Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Margin = new Padding(0, 0, 0, 0) };
            ThemeConfig.ApplyPrimaryButton(btnFetch);
            
            this.SuspendLayout();

            TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 4, AutoSize = true, Padding = new Padding(25) };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            tlp.Controls.Add(txtCode, 0, 0);
            tlp.Controls.Add(txtName, 0, 1);
            tlp.Controls.Add(txtSymbol, 0, 2);
            
            TableLayoutPanel tlpRate = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 1, Height = 75, Margin = new Padding(0, 0, 0, 15) };
            tlpRate.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRate.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tlpRate.Controls.Add(numRate, 0, 0);
            tlpRate.Controls.Add(btnFetch, 1, 0);
            tlp.Controls.Add(tlpRate, 0, 3);

            this.ClientSize = new System.Drawing.Size(420, 480);
            this.ContentPanel.Controls.Add(tlp);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private GenericInventorySystem.Controls.ModernTextBox txtCode;
        private GenericInventorySystem.Controls.ModernTextBox txtName;
        private GenericInventorySystem.Controls.ModernTextBox txtSymbol;
        private GenericInventorySystem.Controls.ModernNumericUpDown numRate;
        private GenericInventorySystem.Controls.ModernButton btnFetch;
    }
}
