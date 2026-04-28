using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Data;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Services;

namespace GenericInventorySystem.Forms
{
    public class MonthlyExpensesForm : UserControl
    {
        private DataGridView dgvExpenses;
        private Label lblExpensesTitle;
        private Label lblCategory;
        private Label lblDate;
        private Label lblDescription;
        private ModernNumericUpDown numAmount;
        private ModernTextBox txtDescription;
        private ModernComboBox cmbCategory;
        private FlatDateTimePicker dtpDate;
        private Button btnAdd;
        private Button btnDelete;
        private Label lblTotal;
        private CheckBox chkRecurring;
        private ExpenseService _expenseService = new ExpenseService();

        public MonthlyExpensesForm()
        {
            InitializeComponent();
            ApplyTheme();
            LoadData();
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1100, 750);
            this.BackColor = ThemeConfig.BackgroundColor;

            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));  // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F)); // Entry (Increased for labels)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid
            mainLayout.Padding = new Padding(20);
            this.Controls.Add(mainLayout);

            // Header
            Panel pnlHeader = new Panel { Dock = DockStyle.Fill };
            lblExpensesTitle = ThemeConfig.CreateStandardHeader("Monthly Expenses");
            lblExpensesTitle.Name = "lblExpensesTitle";
            pnlHeader.Controls.Add(lblExpensesTitle);
            
            lblTotal = new Label { 
                Font = new Font("Segoe UI", 16F, FontStyle.Bold), 
                ForeColor = ThemeConfig.DangerColor, 
                AutoSize = true, 
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlHeader.Controls.Add(lblTotal);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // --- ENTRY SECTION ---
            TableLayoutPanel grid = new TableLayoutPanel { 
                Dock = DockStyle.Fill, 
                ColumnCount = 6, 
                RowCount = 2, 
                Padding = new Padding(10),
                BackColor = ThemeConfig.SurfaceColor 
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F)); // Category
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F)); // Date (Increased from 180)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F)); // Amount (Increased from 120)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350F)); // Description
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Spacer
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F)); // Actions
            
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 85F)); // Increased to 85 to prevent clipping
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F)); // Sub Row (Recurring)

            // 1. Category with Add Button
            Panel pnlCatContainer = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5, 5, 5, 10) };
            cmbCategory = new ModernComboBox { 
                Width = 135,
                Location = new Point(0, 0),
                LabelText = LocalizationManager.IsArabic ? "الفئة" : "Expense Category"
            };
            cmbCategory.Items.AddRange(new object[] { "Rent", "Utilities", "Wages", "Supplies", "Maintenance", "Other" });
            
            Button btnQuickAddCat = new Button { 
                Size = new Size(32, 32), 
                Location = new Point(138, 30), 
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnQuickAddCat.FlatAppearance.BorderSize = 0;
            btnQuickAddCat.Paint += (s, e) => ThemeConfig.DrawIconButton(btnQuickAddCat, e.Graphics, "add", "", ThemeConfig.PrimaryColor, Color.Transparent, false);
            btnQuickAddCat.Click += (s, e) => {
                using (var f = new AddCategoryForm()) {
                    if (f.ShowDialog() == DialogResult.OK) {
                        // Refresh categories if needed, for now just a placeholder action
                        MessageHelper.ShowInfo("Category added. Please refresh.");
                    }
                }
            };
            pnlCatContainer.Controls.Add(cmbCategory);
            pnlCatContainer.Controls.Add(btnQuickAddCat);
            
            Panel pnlDate = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5, 5, 5, 10) };
            Label lblDateRef = new Label { Text = LocalizationManager.IsArabic ? "التاريخ" : "Expense Date", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = ThemeConfig.TextColorDark, Location = new Point(0, 0), AutoSize = true };
            dtpDate = new FlatDateTimePicker { Width = 170, Height = 42, Location = new Point(0, 25) };
            pnlDate.Controls.Add(dtpDate); 
            pnlDate.Controls.Add(lblDateRef);
            
            numAmount = new ModernNumericUpDown { 
                LabelText = LocalizationManager.IsArabic ? "المبلغ" : "Amount",
                DecimalPlaces = 2, 
                Maximum = 1000000, 
                Width = 120 
            };

            
            txtDescription = new ModernTextBox { 
                Dock = DockStyle.Fill, 
                LabelText = LocalizationManager.IsArabic ? "الوصف" : "Description",
                PlaceholderText = "Expense details...",
                Margin = new Padding(5, 5, 5, 10),
                Multiline = true
            };

            // Actions Container (Right Aligned)
            FlowLayoutPanel pnlActions = new FlowLayoutPanel { 
                Dock = DockStyle.Fill, 
                FlowDirection = FlowDirection.LeftToRight, 
                Padding = new Padding(0, 30, 0, 0),
                WrapContents = false
            };

            // Increased width to 140
            btnAdd = new Button { Text = "", Size = new Size(140, 42), Margin = new Padding(5, 0, 5, 0), FlatStyle = FlatStyle.Flat };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAdd, e.Graphics, "add", "Exp_Add", Color.White, ThemeConfig.PrimaryColor, false);
            btnAdd.Click += BtnAdd_Click;

            btnDelete = new Button { Text = "", Size = new Size(140, 42), Margin = new Padding(5, 0, 5, 0), FlatStyle = FlatStyle.Flat };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Paint += (s, e) => ThemeConfig.DrawIconButton(btnDelete, e.Graphics, "remove", "Exp_Delete", Color.White, ThemeConfig.DangerColor, false);
            btnDelete.Click += BtnDelete_Click;

            chkRecurring = new CheckBox { 
                Text = LocalizationManager.IsArabic ? "تكرار" : "Recurring", 
                Font = ThemeConfig.StandardFont, 
                AutoSize = true, 
                Margin = new Padding(5, 5, 0, 0),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.System
            };
            
            pnlActions.Controls.Add(btnAdd);
            pnlActions.Controls.Add(btnDelete);
            
            grid.Controls.Add(pnlCatContainer, 0, 0);
            grid.Controls.Add(pnlDate, 1, 0);
            grid.Controls.Add(numAmount, 2, 0);
            grid.Controls.Add(txtDescription, 3, 0);
            grid.Controls.Add(pnlActions, 5, 0);
            grid.Controls.Add(chkRecurring, 0, 1);

            // Card for Entry Panel
            Panel pnlEntryCard = ThemeConfig.CreateCardPanel(grid);
            pnlEntryCard.Margin = new Padding(0, 0, 0, 15);
            mainLayout.Controls.Add(pnlEntryCard, 0, 1);


            // Grid
            dgvExpenses = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, AutoGenerateColumns = false };
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "expense_id", Width = 60 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date", DataPropertyName = "expense_date", Width = 130 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category", DataPropertyName = "category", Width = 120 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Amount", DataPropertyName = "amount", Width = 100 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", DataPropertyName = "description", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            
            // Hidden data columns
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "is_paid", DataPropertyName = "is_paid", Visible = false });
            
            // New Status Columns
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 100 });
            dgvExpenses.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Recurring", HeaderText = "Auto", DataPropertyName = "is_recurring", Width = 60 });
            
            mainLayout.Controls.Add(dgvExpenses, 0, 2);
            ThemeConfig.ApplyGridTheme(dgvExpenses);

            ApplyLocalization();

            DataGridViewButtonColumn btnPaid = new DataGridViewButtonColumn { 
                Name = "Action", 
                HeaderText = "Action", 
                Text = "Pay Now", 
                UseColumnTextForButtonValue = true, 
                Width = 120,
                FlatStyle = FlatStyle.Flat
            };
            dgvExpenses.Columns.Add(btnPaid);
            dgvExpenses.CellPainting += (s, e) => {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvExpenses.Columns[e.ColumnIndex].Name == "Action") {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                    var drv = dgvExpenses.Rows[e.RowIndex].DataBoundItem as DataRowView;
                    bool isPaid = drv != null && drv["is_paid"] != DBNull.Value ? Convert.ToBoolean(drv["is_paid"]) : true;
                    
                    if (!isPaid) {
                        Rectangle r = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + 8, e.CellBounds.Width - 16, e.CellBounds.Height - 16);
                        using (var path = ThemeConfig.GetRoundedPathPublic(r, 8))
                        using (var brush = new SolidBrush(ThemeConfig.PrimaryColor)) {
                            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            e.Graphics.FillPath(brush, path);
                            TextRenderer.DrawText(e.Graphics, "Pay Now", ThemeConfig.SmallBoldFont, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                    e.Handled = true;
                }
            };
            dgvExpenses.CellContentClick += DgvExpenses_CellContentClick;

            // Card for Grid
            Panel pnlGridCard = ThemeConfig.CreateCardPanel(dgvExpenses);
            mainLayout.Controls.Add(pnlGridCard, 0, 2);
        }

        private void ApplyLocalization()
        {
            lblExpensesTitle.Text = LocalizationManager.GetString("Exp_Title");
            
            // Text removed to preserve DrawIconButton icons
            btnAdd.Text = ""; 
            btnDelete.Text = "";
            
            if (txtDescription != null) txtDescription.PlaceholderText = LocalizationManager.GetString("Exp_Description");
            if (cmbCategory != null) cmbCategory.PlaceholderText = LocalizationManager.GetString("Exp_Category");
            
            if (dgvExpenses.Columns.Contains("Category")) dgvExpenses.Columns["Category"].HeaderText = LocalizationManager.GetString("Exp_Category");
            if (dgvExpenses.Columns.Contains("Date")) dgvExpenses.Columns["Date"].HeaderText = LocalizationManager.GetString("Exp_Date");
            if (dgvExpenses.Columns.Contains("Amount")) dgvExpenses.Columns["Amount"].HeaderText = LocalizationManager.GetString("Exp_Amount");
            if (dgvExpenses.Columns.Contains("Description")) dgvExpenses.Columns["Description"].HeaderText = LocalizationManager.GetString("Exp_Description");
        }

        private void ApplyTheme() 
        { 
            ThemeConfig.ApplyGridTheme(dgvExpenses); 
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbCategory.SelectedIndex == -1)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("Exp_ReqCategory"));
                return;
            }

            decimal amount = numAmount.Value;
            if (amount <= 0) {
                MessageHelper.ShowWarning(LocalizationManager.GetString("Exp_Amount") + " must be greater than 0");
                return;
            }

            DatabaseHelper.ExecuteNonQuery("INSERT INTO expenses (category, expense_date, amount, description, recorded_by, is_recurring, is_paid) VALUES (@cat, @date, @amt, @desc, @usr, @rec, 1)",
                new SqlParameter("@cat", cmbCategory.SelectedItem.ToString()),
                new SqlParameter("@date", dtpDate.Value),
                new SqlParameter("@amt", amount),
                new SqlParameter("@desc", txtDescription.Text),
                new SqlParameter("@usr", UserSession.Username),
                new SqlParameter("@rec", chkRecurring.Checked));
            
            ClearForm();
            LoadData();
        }

        private void DgvExpenses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvExpenses.Columns[e.ColumnIndex].Name == "Action")
            {
                var drv = dgvExpenses.Rows[e.RowIndex].DataBoundItem as DataRowView;
                if (drv == null) return;

                int id = Convert.ToInt32(drv["expense_id"]);
                bool isPaid = drv["is_paid"] != DBNull.Value ? Convert.ToBoolean(drv["is_paid"]) : true;
                
                if (!isPaid) {
                    _expenseService.MarkAsPaid(id);
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvExpenses.SelectedRows.Count == 0) return;
            if (MessageHelper.ShowConfirmation(LocalizationManager.GetString("Exp_ConfirmDelete")))
            {
                int id = Convert.ToInt32(dgvExpenses.SelectedRows[0].Cells["Id"].Value);
                DatabaseHelper.ExecuteNonQuery("DELETE FROM expenses WHERE expense_id = @id", new SqlParameter("@id", id));
                LoadData();
            }
        }

        private void ClearForm()
        {
            cmbCategory.SelectedIndex = -1;
            numAmount.Value = 0;
            txtDescription.Clear();
            dtpDate.Value = DateTime.Now;
        }

        public void LoadData()
        {
            DataTable dt = DatabaseHelper.ExecuteDataTable("SELECT * FROM expenses ORDER BY expense_date DESC");
            dgvExpenses.DataSource = dt;
            
            decimal total = 0;
            foreach (DataGridViewRow row in dgvExpenses.Rows) {
                var drv = row.DataBoundItem as DataRowView;
                if (drv == null) continue;

                bool isPaid = drv["is_paid"] != DBNull.Value ? Convert.ToBoolean(drv["is_paid"]) : true;
                row.Cells["Status"].Value = isPaid ? "Paid" : "UNPAID";
                row.DefaultCellStyle.ForeColor = isPaid ? Color.Black : Color.Red;
                
                total += drv["amount"] != DBNull.Value ? Convert.ToDecimal(drv["amount"]) : 0;
            }

            lblTotal.Text = $"{LocalizationManager.GetString("Exp_Total")}: {CurrencyService.Format(total, "USD")}";
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.AddRange(new object[] { "Rent", "Utilities", "Wages", "Supplies", "Maintenance", "Other" });
        }
    }
}
