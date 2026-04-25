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
            Panel pnlEntry = new Panel { Dock = DockStyle.Fill, BackColor = ThemeConfig.SurfaceColor };
            
            lblCategory = new Label { Text = "Category", Location = new Point(20, 10), AutoSize = true, Font = ThemeConfig.SmallBoldFont };
            cmbCategory = new ModernComboBox { Location = new Point(20, 30), Width = 200 };
            cmbCategory.Items.AddRange(new object[] { "Rent", "Utilities", "Wages", "Supplies", "Maintenance", "Other" });
            
            lblDate = new Label { Text = "Date", Location = new Point(240, 10), AutoSize = true, Font = ThemeConfig.SmallBoldFont };
            dtpDate = new FlatDateTimePicker { Location = new Point(240, 30), Width = 150 };
            
            lblAmount = new Label { Text = "Amount", Location = new Point(410, 10), AutoSize = true, Font = ThemeConfig.SmallBoldFont };
            txtAmount = new ModernTextBox { Location = new Point(410, 30), Width = 150 };
            
            lblDescription = new Label { Text = "Description", Location = new Point(580, 10), AutoSize = true, Font = ThemeConfig.SmallBoldFont };
            txtDescription = new ModernTextBox { Location = new Point(580, 30), Width = 300 };
            
            btnAdd = new Button { Text = "+ Log Expense", Location = new Point(20, 100), Size = new Size(150, 40) };
            ThemeConfig.ApplyPrimaryButton(btnAdd);
            btnAdd.Click += BtnAdd_Click;

            btnDelete = new Button { Text = "Delete Selected", Location = new Point(180, 100), Size = new Size(150, 40) };
            ThemeConfig.ApplyDangerButton(btnDelete);
            btnDelete.Click += BtnDelete_Click;

            pnlEntry.Controls.Add(lblCategory);
            pnlEntry.Controls.Add(cmbCategory);
            pnlEntry.Controls.Add(lblDate);
            pnlEntry.Controls.Add(dtpDate);
            pnlEntry.Controls.Add(lblAmount);
            pnlEntry.Controls.Add(txtAmount);
            pnlEntry.Controls.Add(lblDescription);
            pnlEntry.Controls.Add(txtDescription);
            pnlEntry.Controls.Add(btnAdd);
            pnlEntry.Controls.Add(btnDelete);
            mainLayout.Controls.Add(pnlEntry, 0, 1);

            // Grid
            dgvExpenses = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, AutoGenerateColumns = false };
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "expense_id", Width = 80 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date", DataPropertyName = "expense_date", Width = 150 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category", DataPropertyName = "category", Width = 150 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Amount", DataPropertyName = "amount", Width = 150 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", DataPropertyName = "description", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { Name = "RecordedBy", HeaderText = "Recorded By", DataPropertyName = "recorded_by", Width = 150 });
            
            mainLayout.Controls.Add(dgvExpenses, 0, 2);
        }

        private void ApplyLocalization()
        {
            lblExpensesTitle.Text = LocalizationManager.GetString("Exp_Title");
            lblCategory.Text = LocalizationManager.GetString("Exp_Category");
            lblDate.Text = LocalizationManager.GetString("Exp_Date");
            lblAmount.Text = LocalizationManager.GetString("Exp_Amount");
            lblDescription.Text = LocalizationManager.GetString("Exp_Description");
            btnAdd.Text = LocalizationManager.GetString("Exp_Add");
            btnDelete.Text = LocalizationManager.GetString("Exp_Delete");
            
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

            DatabaseHelper.ExecuteNonQuery("INSERT INTO expenses (category, expense_date, amount, description, recorded_by) VALUES (@cat, @date, @amt, @desc, @usr)",
                new SqlParameter("@cat", cmbCategory.SelectedItem.ToString()),
                new SqlParameter("@date", dtpDate.Value),
                new SqlParameter("@amt", amount),
                new SqlParameter("@desc", txtDescription.Text),
                new SqlParameter("@usr", UserSession.Username));
            
            ClearForm();
            LoadData();
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
            foreach (DataRow r in dt.Rows) total += Convert.ToDecimal(r["amount"]);
            lblTotal.Text = $"{LocalizationManager.GetString("Exp_Total")}: {CurrencyService.Format(total, "USD")}";
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.AddRange(new object[] { "Rent", "Utilities", "Wages", "Supplies", "Maintenance", "Other" });
        }
    }
}
