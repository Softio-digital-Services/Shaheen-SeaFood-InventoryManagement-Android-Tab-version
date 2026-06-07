using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Controls;

namespace InventorySystem.Forms
{
    public class ManageExpenseCategoriesForm : BaseModalForm
    {
        private DataGridView dgvCategories;
        private ModernTextBox txtNewCategory;
        private Button btnAdd;
        private Button btnDelete;

        public ManageExpenseCategoriesForm()
        {
            bool isAr = LocalizationManager.IsArabic;
            string title = LocalizationManager.GetString("Exp_ManageCategories");
            if (title == "Exp_ManageCategories") title = isAr ? "إدارة فئات المصروفات" : "Manage Expense Categories";
            
            this.TitleText = title;
            this.Size = new Size(450, 500);
            InitializeComponent();
            LoadCategories();
        }

        private void InitializeComponent()
        {
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Add Input Row
            TableLayoutPanel pnlInput = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0)
            };
            pnlInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));

            bool isAr = LocalizationManager.IsArabic;

            txtNewCategory = new ModernTextBox
            {
                Dock = DockStyle.Fill,
                LabelText = isAr ? "فئة جديدة" : "New Category",
                Margin = new Padding(0, 0, 10, 0)
            };

            btnAdd = new Button
            {
                Width = 40,
                Height = 36, // Match the typical textbox height
                Margin = new Padding(0, 23, 0, 0) // Shift down to align with input field inside the ModernTextBox
            };
            ThemeConfig.ApplyStandardAddButton(btnAdd, "");
            btnAdd.Click += BtnAdd_Click;

            pnlInput.Controls.Add(txtNewCategory, 0, 0);
            pnlInput.Controls.Add(btnAdd, 1, 0);
            mainLayout.Controls.Add(pnlInput, 0, 0);

            Panel pnlGridBorder = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 15, 0, 15),
                Padding = new Padding(1),
                BackColor = ThemeConfig.BorderColor
            };

            // Grid
            dgvCategories = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                BackgroundColor = ThemeConfig.SurfaceColor,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Margin = new Padding(0)
            };
            ThemeConfig.ApplyGridTheme(dgvCategories);

            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "category_id",
                DataPropertyName = "category_id",
                Visible = false
            });
            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "category_name",
                DataPropertyName = "category_name",
                HeaderText = isAr ? "اسم الفئة" : "Category Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            pnlGridBorder.Controls.Add(dgvCategories);
            mainLayout.Controls.Add(pnlGridBorder, 0, 1);

            // Delete Button
            string delText = LocalizationManager.GetString("Gen_Delete");
            if (delText == "Gen_Delete") delText = isAr ? "حذف المحدد" : "Delete Selected";

            btnDelete = new ModernButton
            {
                Text = delText,
                Width = 150,
                Height = 35,
                Anchor = AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            ThemeConfig.ApplyPaletteButton(btnDelete, ThemeConfig.DangerColor);
            btnDelete.Click += BtnDelete_Click;

            FlowLayoutPanel flpFooter = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0),
                BackColor = Color.Transparent
            };
            flpFooter.Controls.Add(btnDelete);
            this.FooterPanel.Controls.Add(flpFooter);

            this.ContentPanel.Controls.Add(mainLayout);
        }

        private void LoadCategories()
        {
            DataTable dt = DatabaseHelper.ExecuteDataTable("SELECT category_id, category_name FROM expense_categories ORDER BY category_name");
            dgvCategories.DataSource = dt;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string catName = txtNewCategory.Text.Trim();
            if (string.IsNullOrEmpty(catName))
            {
                MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "الرجاء إدخال اسم الفئة" : "Please enter a category name");
                return;
            }

            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar<long>($"SELECT COUNT(*) FROM expense_categories WHERE LOWER(category_name) = '{catName.ToLower()}'"));
            if (count > 0)
            {
                MessageHelper.ShowWarning("Category already exists!");
                return;
            }

            DatabaseHelper.ExecuteNonQuery($"INSERT INTO expense_categories (category_name) VALUES ('{catName}')");
            txtNewCategory.Clear();
            LoadCategories();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCategories.SelectedRows.Count == 0) return;
            
            if (MessageHelper.ShowConfirm("Are you sure you want to delete this category?"))
            {
                int id = Convert.ToInt32(dgvCategories.SelectedRows[0].Cells["category_id"].Value);
                DatabaseHelper.ExecuteNonQuery($"DELETE FROM expense_categories WHERE category_id = {id}");
                LoadCategories();
            }
        }
    }
}
