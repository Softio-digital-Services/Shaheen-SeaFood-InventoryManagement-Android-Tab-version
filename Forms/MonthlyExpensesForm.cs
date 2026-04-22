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
        private ModernTextBox txtAmount;
        private ModernTextBox txtDescription;
        private ComboBox cmbCategory;
        private Button btnAdd;
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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.Padding = new Padding(20);
            this.Controls.Add(mainLayout);

            // Header
            Panel pnlHeader = new Panel { Dock = DockStyle.Fill };
            lblExpensesTitle = ThemeConfig.CreateStandardHeader("Monthly Expenses Alignment");
            lblExpensesTitle.Name = "lblExpensesTitle";
            pnlHeader.Controls.Add(lblExpensesTitle);
            
            lblTotal = new Label { Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.DangerColor, AutoSize = true, Location = new Point(700, 0) };
            pnlHeader.Controls.Add(lblTotal);
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // Entry Panel
            Panel pnlEntry = new Panel { Dock = DockStyle.Fill, BackColor = ThemeConfig.SurfaceColor };
            
            Label l1 = new Label { Text = "Category", Location = new Point(20, 20), AutoSize = true, Font = ThemeConfig.StandardFont };
            cmbCategory = new ComboBox { Location = new Point(20, 45), Width = 200 };
            cmbCategory.Items.AddRange(new object[] { "Rent", "Utilities", "Wages", "Supplies", "Maintenance", "Other" });
            ThemeConfig.ApplyComboBoxStyle(cmbCategory);
            
            txtAmount = new ModernTextBox { LabelText = "Amount ($)", Location = new Point(240, 20), Width = 150 };
            txtDescription = new ModernTextBox { LabelText = "Description", Location = new Point(410, 20), Width = 300 };
            
            btnAdd = new Button { Text = "+ Log Expense", Location = new Point(730, 40), Size = new Size(150, 40) };
            ThemeConfig.ApplyPrimaryButton(btnAdd);
            btnAdd.Click += BtnAdd_Click;

            pnlEntry.Controls.Add(l1);
            pnlEntry.Controls.Add(cmbCategory);
            pnlEntry.Controls.Add(txtAmount);
            pnlEntry.Controls.Add(txtDescription);
            pnlEntry.Controls.Add(btnAdd);
            mainLayout.Controls.Add(pnlEntry, 0, 1);

            // Grid
            dgvExpenses = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, AutoGenerateColumns = false };
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "expense_id", Width = 80 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date", DataPropertyName = "expense_date", Width = 150 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category", DataPropertyName = "category", Width = 150 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Amount", DataPropertyName = "amount", Width = 150 });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Description", DataPropertyName = "description", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Recorded By", DataPropertyName = "recorded_by", Width = 150 });
            
            mainLayout.Controls.Add(dgvExpenses, 0, 2);
        }

        private void ApplyTheme() { ThemeConfig.ApplyGridTheme(dgvExpenses); }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            bool isAr = LocalizationManager.IsArabic;
            lblExpensesTitle.Text = isAr ? "إدارة المصاريف الشهرية" : "Monthly Expenses Alignment";
            btnAdd.Text = isAr ? "+ تسجيل مصاريف" : "+ Log Expense";
        }

        public void LoadData()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteDataTable("SELECT * FROM expenses ORDER BY expense_date DESC");
                dgvExpenses.DataSource = dt;
                
                decimal total = 0;
                foreach (DataRow r in dt.Rows) total += Convert.ToDecimal(r["amount"]);
                lblTotal.Text = $"Total Month Overhead: {CurrencyService.Format(total, "USD")}";
            }
            catch (Exception ex) { MessageHelper.ShowError("Error loading expenses: " + ex.Message); }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbCategory.SelectedItem == null || !decimal.TryParse(txtAmount.Text, out decimal amt)) {
                MessageHelper.ShowWarning("Please select a category and enter a valid numeric amount."); return;
            }
            
            string sql = "INSERT INTO expenses (category, amount, description, recorded_by) VALUES (@cat, @amt, @desc, @usr)";
            DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@cat", cmbCategory.SelectedItem.ToString()),
                new SqlParameter("@amt", amt),
                new SqlParameter("@desc", txtDescription.Text),
                new SqlParameter("@usr", UserSession.Username));
                
            txtAmount.Text = ""; txtDescription.Text = ""; cmbCategory.SelectedIndex = -1;
            LoadData();
        }
    }
}
