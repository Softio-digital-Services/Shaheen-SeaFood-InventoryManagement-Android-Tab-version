using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using GenericInventorySystem.Data;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    public partial class CustomerDetailsForm : Form
    {
        private int _customerId;
        private string _customerName;
        
        private Label lblName;
        private Label lblType;
        private Label lblBalance;
        private Label lblBalTitle;
        private DataGridView dgvHistory;
        private Button btnRecordSale;
        private Button btnReceivePayment;
        private Button btnClose;

        public CustomerDetailsForm(int id, string name)
        {
            _customerId = id;
            _customerName = name;
            InitializeComponent();
            LoadDetails();
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(950, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Customer Details - " + _customerName;
            this.BackColor = ThemeConfig.BackgroundColor;

            // Main Layout
            TableLayoutPanel tlpMain = new TableLayoutPanel();
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.ColumnCount = 1;
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F)); // Header Area
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Grid (Fill)
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // Footer
            tlpMain.Padding = new Padding(20);
            
            // --- HEADER ---
            Panel pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Fill;
            pnlHeader.BackColor = ThemeConfig.SurfaceColor;
            
            // Header Left (Title)
            FlowLayoutPanel flpLeft = new FlowLayoutPanel();
            flpLeft.FlowDirection = FlowDirection.TopDown;
            flpLeft.Dock = DockStyle.Left;
            flpLeft.Width = 350;
            flpLeft.Padding = new Padding(10);
            
            lblName = new Label() { Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.PrimaryColor, AutoSize = true, Margin = new Padding(0,0,0,5) };
            lblType = new Label() { AutoSize = true, ForeColor = ThemeConfig.SecondaryColor, Font = ThemeConfig.StandardFont };

            flpLeft.Controls.Add(lblName);
            flpLeft.Controls.Add(lblType);
            
            // Header Right (Balance + Buttons)
            FlowLayoutPanel flpRight = new FlowLayoutPanel();
            flpRight.FlowDirection = FlowDirection.RightToLeft;
            flpRight.Dock = DockStyle.Right;
            flpRight.Width = 580;
            flpRight.Padding = new Padding(10);
            flpRight.AutoSize = false;
            
            // 1. Balance Panel
            Panel pnlBalance = new Panel();
            pnlBalance.Size = new Size(180, 70);
            pnlBalance.BackColor = ThemeConfig.BackgroundColor;
            pnlBalance.Margin = new Padding(10, 0, 0, 0);
            
            lblBalance = new Label() { Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.WarningColor, Location = new Point(10, 10), AutoSize = true };
            lblBalTitle = new Label() { Text = "Balance Due", Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.SecondaryColor, Location = new Point(12, 45), AutoSize = true };

            pnlBalance.Controls.Add(lblBalance);
            pnlBalance.Controls.Add(lblBalTitle);
            
            // 2. Receive Payment Button (Green)
            btnReceivePayment = new Button() { Size = new Size(160, 45) }; btnReceivePayment.FlatStyle = FlatStyle.Flat; btnReceivePayment.FlatAppearance.BorderSize = 0; btnReceivePayment.Cursor = Cursors.Hand; btnReceivePayment.Paint += (s, e) => ThemeConfig.DrawIconButton(btnReceivePayment, e.Graphics, "currency", "Cust_ReceivePayment", Color.White, ThemeConfig.SuccessColor, false);
            btnReceivePayment.Click += BtnReceivePayment_Click;
            
            // 3. Record Sale Button (Blue)
            btnRecordSale = new Button() { Size = new Size(150, 45) }; btnRecordSale.FlatStyle = FlatStyle.Flat; btnRecordSale.FlatAppearance.BorderSize = 0; btnRecordSale.Cursor = Cursors.Hand; btnRecordSale.Paint += (s, e) => ThemeConfig.DrawIconButton(btnRecordSale, e.Graphics, "pos", "Cust_RecordSale", Color.White, ThemeConfig.PrimaryColor, false);
            btnRecordSale.Click += BtnRecordSale_Click;
            
            flpRight.Controls.Add(pnlBalance);
            flpRight.Controls.Add(btnReceivePayment);
            flpRight.Controls.Add(btnRecordSale);
            
            pnlHeader.Controls.Add(flpRight);
            pnlHeader.Controls.Add(flpLeft);

            // --- GRID ---
            dgvHistory = new DataGridView();
            dgvHistory.DataError += (s, e) => { e.ThrowException = false; };
            dgvHistory.Dock = DockStyle.Fill;
            dgvHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHistory.CellFormatting += DgvHistory_CellFormatting;
            ThemeConfig.ApplyGridTheme(dgvHistory);

            // --- FOOTER ---
            Panel pnlFooter = new Panel();
            pnlFooter.Dock = DockStyle.Fill;
            
            btnClose = new Button() { Text = "Close", Size = new Size(120, 40) };
            ThemeConfig.ApplySecondaryButton(btnClose);
            btnClose.Click += (s,e) => Close();
            
            FlowLayoutPanel flpFooter = new FlowLayoutPanel();
            flpFooter.Dock = DockStyle.Right;
            flpFooter.FlowDirection = FlowDirection.RightToLeft;
            flpFooter.Padding = new Padding(0, 10, 0, 0);
            flpFooter.Controls.Add(btnClose);
            
            pnlFooter.Controls.Add(flpFooter);

            tlpMain.Controls.Add(pnlHeader, 0, 0);
            tlpMain.Controls.Add(dgvHistory, 0, 1);
            tlpMain.Controls.Add(pnlFooter, 0, 2);

            this.Controls.Add(tlpMain);
        }

        private void LoadDetails()
        {
            try 
            {
                // Load Info
                string sqlInfo = $"SELECT * FROM customers WHERE customer_id = {_customerId}";
                DataTable dtInfo = DatabaseHelper.ExecuteDataTable(sqlInfo);
                if(dtInfo.Rows.Count > 0)
                {
                    DataRow row = dtInfo.Rows[0];
                    lblName.Text = row["full_name"].ToString();
                    string typeVal = row.Table.Columns.Contains("type") && row["type"] != DBNull.Value ? row["type"].ToString() : "Individual";
                    string phone = row.Table.Columns.Contains("phone") && row["phone"] != DBNull.Value ? row["phone"].ToString() : "-";
                    lblType.Text = typeVal + " | " + phone;
                    
                    decimal bal = row.Table.Columns.Contains("current_balance") && row["current_balance"] != DBNull.Value 
                        ? Convert.ToDecimal(row["current_balance"]) : 0m;
                    lblBalance.Text = $"${bal:N2}";
                }

                // Load History â€” combines sales (balance additions) and payments received
                string sqlHistory = $@"
                    SELECT payment_date as 'Date', 
                           CASE WHEN notes LIKE '[Sale]%' OR notes LIKE '%Sale%' OR notes LIKE '%Order%' THEN 'Payment Due'
                                ELSE 'Payment Received' END as 'Action',
                           amount as 'Amount',
                           CASE WHEN notes IS NULL OR notes = '' OR notes = 'None' THEN 'None'
                                ELSE REPLACE(REPLACE(notes, '[Sale] ', ''), '[Payment] ', '') END as 'Details'
                    FROM payments 
                    WHERE entity_type = 'Customer' AND entity_id = {_customerId}
                    ORDER BY payment_date DESC";
                
                dgvHistory.DataSource = DatabaseHelper.ExecuteDataTable(sqlHistory);
                ApplyGridLocalizations();
            }
            catch(Exception ex) { MessageHelper.ShowError(ex.Message); }
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            this.Text = (isArabic ? "تفاصيل " : "Details - ") + _customerName;
            lblBalTitle.Text = isArabic ? "الرصيد المستحق" : "Balance Due";
            btnClose.Text = LocalizationManager.GetString("Popup_Cancel");

            ApplyGridLocalizations();
        }

        private void ApplyGridLocalizations()
        {
            if (dgvHistory == null || dgvHistory.Columns.Count == 0) return;

            if (dgvHistory.Columns["Date"] != null) dgvHistory.Columns["Date"].HeaderText = LocalizationManager.GetString("Hist_ColDate");
            if (dgvHistory.Columns["Action"] != null) dgvHistory.Columns["Action"].HeaderText = LocalizationManager.GetString("Hist_ColAction");
            if (dgvHistory.Columns["Amount"] != null) dgvHistory.Columns["Amount"].HeaderText = LocalizationManager.GetString("Hist_ColAmount");
            if (dgvHistory.Columns["Details"] != null) dgvHistory.Columns["Details"].HeaderText = LocalizationManager.GetString("Hist_ColDetails");
        }
        
        private void DgvHistory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null) return;

            string colName = dgvHistory.Columns[e.ColumnIndex].Name;

            if (colName == "Action")
            {
                string actionKey = e.Value.ToString();
                e.Value = LocalizationManager.GetString("Action_" + actionKey);
            }
            else if (colName == "Details")
            {
                string val = e.Value.ToString();
                if (val == "None") e.Value = LocalizationManager.GetString("Tran_None") ?? val;
            }
        }

        private void BtnRecordSale_Click(object sender, EventArgs e)
        {
            decimal currentBalance = ParseBalance();

            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;
            TransactionEntryForm form = new TransactionEntryForm(
                L("Cust_RecordSale"),
                string.Format(L("Prompt_RecordSale"), _customerName));
            
            if(form.ShowDialog() == DialogResult.OK)
            {
                decimal amount = form.Amount;
                string userNotes = string.IsNullOrWhiteSpace(form.Notes) ? "None" : form.Notes;
                string dbNotes = "[Sale] " + userNotes;
                string amountStr = amount.ToString(CultureInfo.InvariantCulture);

                // 1. Update Balance â€” customers table uses current_balance
                string sql1 = $"UPDATE customers SET current_balance = current_balance + {amountStr} WHERE customer_id = {_customerId}";
                DatabaseHelper.ExecuteNonQuery(sql1);

                // 2. Record transaction log
                string sql2 = $"INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes) VALUES ('Customer', {_customerId}, {amountStr}, GETDATE(), @notes)";
                DatabaseHelper.ExecuteNonQuery(sql2, new System.Data.SqlClient.SqlParameter("@notes", dbNotes));

                GlobalEvents.RaiseCustomersUpdated(); // Sync main grid
                LoadDetails();
            }
        }

        private void BtnReceivePayment_Click(object sender, EventArgs e)
        {
            decimal currentBalance = ParseBalance();

            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;
            TransactionEntryForm form = new TransactionEntryForm(
                L("Cust_ReceivePayment"),
                string.Format(L("Prompt_ReceivePayment"), _customerName));
            
            if(form.ShowDialog() == DialogResult.OK)
            {
                decimal amount = form.Amount;
                string userNotes = string.IsNullOrWhiteSpace(form.Notes) ? "None" : form.Notes;
                string dbNotes = "[Payment] " + userNotes;

                if(amount > currentBalance)
                {
                    if(!MessageHelper.ConfirmAction($"Payment (${amount:N2}) exceeds balance (${currentBalance:N2}). Continue?"))
                        return;
                }

                string amountStr = amount.ToString(CultureInfo.InvariantCulture);

                // 1. Update Balance â€” customers table uses current_balance
                string sql1 = $"UPDATE customers SET current_balance = current_balance - {amountStr} WHERE customer_id = {_customerId}";
                DatabaseHelper.ExecuteNonQuery(sql1);
 
                // 2. Record Payment
                string sql2 = $"INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes) VALUES ('Customer', {_customerId}, {amountStr}, GETDATE(), @notes)";
                DatabaseHelper.ExecuteNonQuery(sql2, new System.Data.SqlClient.SqlParameter("@notes", dbNotes));
 
                GlobalEvents.RaiseCustomersUpdated(); // Sync main grid
                LoadDetails();
            }
        }

        private decimal ParseBalance()
        {
            string balStr = lblBalance.Text.Replace("$", "").Replace(",", "").Trim();
            decimal.TryParse(balStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal bal);
            return bal;
        }
    }
}

