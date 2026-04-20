using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Data;

namespace GenericInventorySystem.Forms
{
    public class ProductSelectorForm : Form
    {
        private TextBox txtSearch;
        private DataGridView dgvProducts;
        private Button btnSelect;
        private Button btnCancel;
        
        public int SelectedPartId { get; private set; }
        public string SelectedPartName { get; private set; }
        public decimal SelectedPrice { get; private set; }
        public int SelectedStock { get; private set; }

        public ProductSelectorForm()
        {
            InitializeComponent();
            ApplyTheme();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(600, 500);
            this.Text = GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "اختر منتج" : "Select Product";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            txtSearch = new TextBox();
            txtSearch.Location = new Point(20, 20);
            txtSearch.Size = new Size(545, 30);
            txtSearch.Font = ThemeConfig.SubHeaderFont;
            txtSearch.TextChanged += (s, e) => LoadProducts();

            dgvProducts = new DataGridView();
            dgvProducts.DataError += (s, e) => { e.ThrowException = false; };
            dgvProducts.Location = new Point(20, 60);
            dgvProducts.Size = new Size(545, 330);
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.ReadOnly = true;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProducts.CellDoubleClick += (s, e) => SelectAndClose();

            btnSelect = new Button();
            btnSelect.Text = GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "إضافة المحدد" : "Add Selected";
            btnSelect.Location = new Point(360, 410);
            btnSelect.Size = new Size(120, 35);
            btnSelect.Click += (s, e) => SelectAndClose();

            btnCancel = new Button();
            btnCancel.Text = GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "إلغاء" : "Cancel";
            btnCancel.Location = new Point(490, 410); // Right aligned
            btnCancel.Size = new Size(75, 35);
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(txtSearch);
            this.Controls.Add(dgvProducts);
            this.Controls.Add(btnSelect);
            this.Controls.Add(btnCancel);
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeConfig.BackgroundColor;
            ThemeConfig.ApplyGridTheme(dgvProducts);
            ThemeConfig.ApplyPrimaryButton(btnSelect);
            ThemeConfig.ApplySecondaryButton(btnCancel);
        }

        private void LoadProducts()
        {
            try
            {
                string search = txtSearch.Text.Trim();
                string sql = "SELECT id, part_name as Name, part_number as SKU, selling_price as Price, quantity_in_stock as Stock FROM parts WHERE date_deleted IS NULL AND status = 'Active'";
                if (!string.IsNullOrEmpty(search))
                {
                    sql += $" AND (part_name LIKE '%{search}%' OR part_number LIKE '%{search}%')";
                }
                dgvProducts.DataSource = DatabaseHelper.ExecuteDataTable(sql);
                if (dgvProducts.Columns["id"] != null) dgvProducts.Columns["id"].Visible = false;

                if (GenericInventorySystem.Helpers.LocalizationManager.IsArabic)
                {
                    if (dgvProducts.Columns["Name"] != null) dgvProducts.Columns["Name"].HeaderText = "الاسم";
                    if (dgvProducts.Columns["SKU"] != null) dgvProducts.Columns["SKU"].HeaderText = "رقم القطعة";
                    if (dgvProducts.Columns["Price"] != null) dgvProducts.Columns["Price"].HeaderText = "السعر";
                    if (dgvProducts.Columns["Stock"] != null) dgvProducts.Columns["Stock"].HeaderText = "المخزون";
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(ex.Message);
            }
        }

        private void SelectAndClose()
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                var row = dgvProducts.SelectedRows[0];
                SelectedPartId = Convert.ToInt32(row.Cells["id"].Value);
                SelectedPartName = row.Cells["Name"].Value.ToString();
                SelectedPrice = Convert.ToDecimal(row.Cells["Price"].Value);
                SelectedStock = Convert.ToInt32(row.Cells["Stock"].Value);

                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
