using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
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
        private Label lblAmount;
        private Label lblDescription;
        private ModernTextBox txtAmount;
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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.Padding = new Padding(20);
            this.Controls.Add(mainLayout);

            // Header
            Panel pnlHeader = new Panel { Dock = DockStyle.Fill };
            lblExpensesTitle = ThemeConfig.CreateStandardHeader("Monthly Expenses");
            lblExpensesTitle.Name = "lblExpensesTitle";
            pnlHeader.Controls.Add(lblExpensesTitle);
            
            lblTotal = new Label { Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.DangerColor, AutoSize = true, Location = new Point(700, 0) };
            pnlHeader.Controls.Add(lblTotal);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // Entry Panel
            Panel pnlEntry = new Panel { Dock = DockStyle.Fill, BackColor = ThemeConfig.SurfaceColor, Padding = new Padding(15) };
            
            // Main Grid for Entry
            TableLayoutPanel grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3 };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F)); // Category
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F)); // Date
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F)); // Amount
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));   // Description (Takes rest)
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F)); // Spacing
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F)); // Inputs (Increased for built-in labels)
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Buttons

            cmbCategory = new ModernComboBox { Dock = DockStyle.Fill };
            cmbCategory.PlaceholderText = "Category";
            cmbCategory.Items.AddRange(new object[] { "Rent", "Utilities", "Wages", "Supplies", "Maintenance", "Other" });
            
            dtpDate = new FlatDateTimePicker { Dock = DockStyle.Fill };
            
            txtAmount = new ModernTextBox { Dock = DockStyle.Fill };
            txtAmount.PlaceholderText = "Amount";
            
            txtDescription = new ModernTextBox { Dock = DockStyle.Fill };
            txtDescription.PlaceholderText = "Description";
            
            grid.Controls.Add(cmbCategory, 0, 1);
            grid.Controls.Add(dtpDate, 1, 1);
            grid.Controls.Add(txtAmount, 2, 1);
            grid.Controls.Add(txtDescription, 3, 1);

            FlowLayoutPanel buttonGroup = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 10, 0, 0) };
            btnAdd = new Button { Text = "+ Log Expense", Size = new Size(150, 40) };
            ThemeConfig.ApplyPrimaryButton(btnAdd);
            btnAdd.Click += BtnAdd_Click;

            btnDelete = new Button { Text = "Delete Selected", Size = new Size(150, 40) };
            ThemeConfig.ApplyDangerButton(btnDelete);
            btnDelete.Click += BtnDelete_Click;

            chkRecurring = new CheckBox { Text = "Recurring (Auto-Add)", Font = ThemeConfig.SmallBoldFont, AutoSize = true, Margin = new Padding(10, 10, 0, 0) };
            
            buttonGroup.Controls.Add(btnAdd);
            buttonGroup.Controls.Add(btnDelete);
            buttonGroup.Controls.Add(chkRecurring);
            grid.Controls.Add(buttonGroup, 0, 2);
            grid.SetColumnSpan(buttonGroup, 4);

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
            
            DataGridViewButtonColumn btnPaid = new DataGridViewButtonColumn { 
                Name = "Action", 
                HeaderText = "Action", 
                Text = "Pay Now", 
                UseColumnTextForButtonValue = true, 
                Width = 120 
            };
            dgvExpenses.Columns.Add(btnPaid);
            dgvExpenses.CellContentClick += DgvExpenses_CellContentClick;

            // Card for Grid
            Panel pnlGridCard = ThemeConfig.CreateCardPanel(dgvExpenses);
            mainLayout.Controls.Add(pnlGridCard, 0, 2);
        }

        private void ApplyLocalization()
        {
            lblExpensesTitle.Text = LocalizationManager.GetString("Exp_Title");
            btnAdd.Text = LocalizationManager.GetString("Exp_Add");
            btnDelete.Text = LocalizationManager.GetString("Exp_Delete");
            
            if (txtAmount != null) txtAmount.PlaceholderText = LocalizationManager.GetString("Exp_Amount");
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
            ThemeConfig.ApplyPrimaryButton(btnAdd);
            ThemeConfig.ApplyDangerButton(btnDelete);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbCategory.SelectedIndex == -1)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("Exp_ReqCategory"));
                return;
            }

            if (!ValidationHelper.ValidateRequired(txtAmount, LocalizationManager.GetString("Exp_Amount"))) return;
            if (!ValidationHelper.ValidateNumeric(txtAmount.Text, LocalizationManager.GetString("Exp_Amount"), out decimal amount)) return;

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
            txtAmount.Clear();
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
