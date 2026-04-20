using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Data;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    public partial class SupplierDetailsForm : Form
    {
        private int _supplierId;
        private string _supplierName;
        
        private Label lblName;
        private Label lblType;
        private Label lblBalance;
        private Label lblBalTitle;
        private DataGridView dgvHistory;
        private Button btnPayment;
        private Button btnAddBill;
        private Button btnClose;

        public SupplierDetailsForm(int id, string name)
        {
            _supplierId = id;
            _supplierName = name;
            InitializeComponent();
            LoadDetails();
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Supplier Details - " + _supplierName;
            this.BackColor = ThemeConfig.BackgroundColor;

            // Main Layout
            TableLayoutPanel tlpMain = new TableLayoutPanel();
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.ColumnCount = 1;
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F)); // Header Area (Increased from 100)
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
            flpLeft.Width = 400;
            flpLeft.Padding = new Padding(10);
            
            lblName = new Label() { Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.PrimaryColor, AutoSize = true, Margin = new Padding(0,0,0,5) };
            lblType = new Label() { AutoSize = true, ForeColor = ThemeConfig.SecondaryColor, Font = ThemeConfig.StandardFont };

            
            flpLeft.Controls.Add(lblName);
            flpLeft.Controls.Add(lblType);
            
            // Header Right (Balance + Button)
            FlowLayoutPanel flpRight = new FlowLayoutPanel();
            flpRight.FlowDirection = FlowDirection.RightToLeft; 
            flpRight.Dock = DockStyle.Right;
            flpRight.Width = 600; // Increased to prevent button hiding
            flpRight.Padding = new Padding(10);
            flpRight.AutoSize = false;
            
            // 1. Balance Panel
            Panel pnlBalance = new Panel();
            pnlBalance.Size = new Size(200, 70);
            pnlBalance.BackColor = ThemeConfig.BackgroundColor;
            pnlBalance.Margin = new Padding(10, 0, 0, 0);
            
            lblBalance = new Label() { Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.WarningColor, Location = new Point(10, 10), AutoSize = true };
            lblBalTitle = new Label() { Text = "Balance Due", Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.SecondaryColor, Location = new Point(12, 45), AutoSize = true };

            pnlBalance.Controls.Add(lblBalance);
            pnlBalance.Controls.Add(lblBalTitle);
            
            // 2. Add Bill (Record Sale) button â€“ adds to balance
            btnAddBill = new Button() { Size = new Size(150, 45) }; btnAddBill.FlatStyle = FlatStyle.Flat; btnAddBill.FlatAppearance.BorderSize = 0; btnAddBill.Cursor = Cursors.Hand; btnAddBill.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAddBill, e.Graphics, "pos", "Sup_AddBill", Color.White, ThemeConfig.WarningColor, false);
            btnAddBill.Click += BtnAddBill_Click;
            btnAddBill.Margin = new Padding(0, 10, 10, 0);

            // 3. Pay Supplier button â€“ reduces balance
            btnPayment = new Button() { Size = new Size(155, 45) }; btnPayment.FlatStyle = FlatStyle.Flat; btnPayment.FlatAppearance.BorderSize = 0; btnPayment.Cursor = Cursors.Hand; btnPayment.Paint += (s, e) => ThemeConfig.DrawIconButton(btnPayment, e.Graphics, "currency", "Sup_PaySupplier", Color.White, ThemeConfig.SuccessColor, false);
            btnPayment.Click += BtnPayment_Click;
            btnPayment.Margin = new Padding(0, 10, 10, 0); 
            
            flpRight.Controls.Add(pnlBalance);
            flpRight.Controls.Add(btnPayment);
            flpRight.Controls.Add(btnAddBill);
            
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
                string sqlInfo = $"SELECT * FROM suppliers WHERE id = {_supplierId}";
                DataTable dtInfo = DatabaseHelper.ExecuteDataTable(sqlInfo);
                if(dtInfo.Rows.Count > 0)
                {
                    DataRow row = dtInfo.Rows[0];
                    lblName.Text = row["supplier_name"].ToString();
                    lblType.Text = (row["type"] != DBNull.Value ? row["type"].ToString() : "Company") + " | " + row["phone"].ToString();
                    decimal bal = Convert.ToDecimal(row["balance_due"]);
                    lblBalance.Text = $"${bal:N2}";
                }

                // Load History
                string sqlHistory = $@"
                    SELECT payment_date as 'Date', 
                           CASE WHEN notes LIKE '[Sale]%' OR notes LIKE '%Bill%' OR notes LIKE '%Sale%' THEN 'Payment Due' 
                                ELSE 'Payment Received' END as 'Action',
                           amount as 'Amount',
                           CASE WHEN notes IS NULL OR notes = '' OR notes = 'None' THEN 'None'
                                ELSE REPLACE(REPLACE(notes, '[Sale] ', ''), '[Payment] ', '') END as 'Details'
                    FROM payments 
                    WHERE entity_type = 'Supplier' AND entity_id = {_supplierId}
                    ORDER BY payment_date DESC";
                
                dgvHistory.DataSource = DatabaseHelper.ExecuteDataTable(sqlHistory);
                ApplyGridLocalizations(); // Translate columns after bind
            }
            catch(Exception ex) { MessageHelper.ShowError(ex.Message); }
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            this.Text = (isArabic ? "تفاصيل " : "Details - ") + _supplierName;
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

        private void BtnPayment_Click(object sender, EventArgs e)
        {
             // Get current balance
             decimal currentBalance = 0;
             string balStr = lblBalance.Text.Replace("$", "").Trim();
             decimal.TryParse(balStr, out currentBalance);

             Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;
             TransactionEntryForm form = new TransactionEntryForm(L("Sup_PaySupplier"), string.Format(L("Prompt_PaySupplier"), _supplierName));
             if(form.ShowDialog() == DialogResult.OK)
             {
                 decimal amount = form.Amount;
                 string userNotes = string.IsNullOrWhiteSpace(form.Notes) ? "None" : form.Notes;
                 string dbNotes = "[Payment] " + userNotes;

                 if (amount > currentBalance)
                 {
                     if(!MessageHelper.ConfirmAction($"Payment (${amount:N2}) exceeds balance (${currentBalance:N2}). Continue anyway?"))
                        return;
                 }

                 // 1. Update Balance â€” use InvariantCulture
                 string amountStr = amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                 string sql1 = $"UPDATE suppliers SET balance_due = balance_due - {amountStr} WHERE id = {_supplierId}";
                 DatabaseHelper.ExecuteNonQuery(sql1);

                 // 2. Record Payment
                 string sql2 = $"INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes) VALUES ('Supplier', {_supplierId}, {amountStr}, GETDATE(), @notes)";
                 DatabaseHelper.ExecuteNonQuery(sql2, new System.Data.SqlClient.SqlParameter("@notes", dbNotes));

                 GlobalEvents.RaiseSuppliersUpdated();
                 LoadDetails(); // Refresh
             }
        }

        private void BtnAddBill_Click(object sender, EventArgs e)
        {
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;
            TransactionEntryForm form = new TransactionEntryForm(
                L("Sup_AddBill") ?? "Record Sale",
                string.Format(L("Prompt_AddBill") ?? "Enter bill amount from {0}:", _supplierName));
            if(form.ShowDialog() == DialogResult.OK)
            {
                decimal amount = form.Amount;
                string userNotes = string.IsNullOrWhiteSpace(form.Notes) ? "None" : form.Notes;
                string dbNotes = "[Sale] " + userNotes;
                string amountStr = amount.ToString(System.Globalization.CultureInfo.InvariantCulture);

                // Add to supplier balance
                string sql1 = $"UPDATE suppliers SET balance_due = balance_due + {amountStr} WHERE id = {_supplierId}";
                DatabaseHelper.ExecuteNonQuery(sql1);

                // Log it
                string sql2 = $"INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes) VALUES ('Supplier', {_supplierId}, {amountStr}, GETDATE(), @notes)";
                DatabaseHelper.ExecuteNonQuery(sql2, new System.Data.SqlClient.SqlParameter("@notes", dbNotes));

                GlobalEvents.RaiseSuppliersUpdated();
                LoadDetails();
            }
        }
    }
}

