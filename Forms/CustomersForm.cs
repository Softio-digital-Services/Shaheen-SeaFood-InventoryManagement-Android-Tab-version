using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Data;
using GenericInventorySystem.Controls;

namespace GenericInventorySystem.Forms
{
    public partial class CustomersForm : UserControl
    {
        private DataGridView dgvCustomers;
        private Button btnAddNew;
        private Button btnCustomerDetails;
        private Button btnImport;
        private Button btnExport;
        private Button btnDeleteBulk;
        private Label lblCustomersTitle;
        private ModernTextBox txtSearch;
        private DataTable _dtCustomers;
        private System.Windows.Forms.Timer _searchTimer;

        public CustomersForm()
        {
            InitializeComponent();
            SetupSearchTimer();
            ApplyTheme();
            ApplyLocalization();
        }

        private void SetupSearchTimer()
        {
            _searchTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                PerformSearch();
            };
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeConfig.BackgroundColor;
            if (lblCustomersTitle != null) { lblCustomersTitle.Font = ThemeConfig.HeaderFont; lblCustomersTitle.ForeColor = ThemeConfig.PrimaryColor; }

            ThemeConfig.ApplyGridTheme(dgvCustomers);
            
            // Buttons are styled via Paint event in InitializeComponent
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            var L = LocalizationManager.GetString;

            if (lblCustomersTitle != null) lblCustomersTitle.Text = L("Cust_Title") ?? "Customers management";
            if (txtSearch != null) txtSearch.PlaceholderText = L("Cust_Search") ?? "Search customers...";
            
            if (btnAddNew != null) btnAddNew.Invalidate();
            if (btnImport != null) btnImport.Invalidate();
            if (btnExport != null) btnExport.Invalidate();
            
            var ctrlDel = this.Controls.Find("btnDeleteSelected", true);
            if (ctrlDel.Length > 0) ctrlDel[0].Invalidate();

            // Grid Columns
            if (dgvCustomers != null && dgvCustomers.Columns.Count > 0)
            {
                if (dgvCustomers.Columns.Contains("colName")) dgvCustomers.Columns["colName"].HeaderText = L("Cust_GridName");
                if (dgvCustomers.Columns.Contains("colPhone")) dgvCustomers.Columns["colPhone"].HeaderText = L("Cust_GridPhone");
                if (dgvCustomers.Columns.Contains("colEmail")) dgvCustomers.Columns["colEmail"].HeaderText = L("Cust_GridEmail");
                if (dgvCustomers.Columns.Contains("colAddress")) dgvCustomers.Columns["colAddress"].HeaderText = L("Cust_GridAddress");
                if (dgvCustomers.Columns.Contains("colBalance")) dgvCustomers.Columns["colBalance"].HeaderText = L("Cust_GridBalance");
                if (dgvCustomers.Columns.Contains("colActions")) dgvCustomers.Columns["colActions"].HeaderText = L("Cust_GridActions");
                
                if (dgvCustomers.Columns.Contains("colCreditLimit")) dgvCustomers.Columns["colCreditLimit"].HeaderText = "Credit Limit";
                if (dgvCustomers.Columns.Contains("colDueDate")) dgvCustomers.Columns["colDueDate"].HeaderText = "Due Date";
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible && !this.DesignMode)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            try
            {
                string sql = "SELECT customer_id as ID, full_name, phone, email, address, current_balance, type, credit_limit, payment_due_date, reminder_days FROM customers WHERE date_deleted IS NULL ORDER BY full_name";
                _dtCustomers = DatabaseHelper.ExecuteDataTable(sql);
                DisplayData(_dtCustomers);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "loading customers");
            }
        }

        private void DisplayData(DataTable dt)
        {
            dgvCustomers.Rows.Clear();
            if (dt == null) return;

            foreach (DataRow r in dt.Rows)
            {
                int rowIndex = dgvCustomers.Rows.Add();
                var row = dgvCustomers.Rows[rowIndex];

                row.Cells["colSelect"].Value = false;
                row.Cells["colId"].Value = r["ID"];
                row.Cells["colName"].Value = r["full_name"];
                row.Cells["colPhone"].Value = r["phone"];
                row.Cells["colEmail"].Value = r["email"];
                row.Cells["colAddress"].Value = r["address"];
                
                decimal bal = r["current_balance"] != DBNull.Value ? Convert.ToDecimal(r["current_balance"]) : 0;
                row.Cells["colBalance"].Value = bal.ToString("N2");

                decimal limit = r["credit_limit"] != DBNull.Value ? Convert.ToDecimal(r["credit_limit"]) : 0;
                row.Cells["colCreditLimit"].Value = limit > 0 ? limit.ToString("N2") : "";

                if (r["payment_due_date"] != DBNull.Value)
                    row.Cells["colDueDate"].Value = Convert.ToDateTime(r["payment_due_date"]).ToString("yyyy-MM-dd");
                else
                    row.Cells["colDueDate"].Value = "";
            }
        }

        private void PerformSearch()
        {
            string term = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(term))
            {
                DisplayData(_dtCustomers);
                return;
            }

            if (_dtCustomers == null) return;

            DataTable filtered = _dtCustomers.Clone();
            var rows = _dtCustomers.AsEnumerable().Where(r => 
                (r["full_name"]?.ToString().ToLower().Contains(term) ?? false) ||
                (r["phone"]?.ToString().ToLower().Contains(term) ?? false) ||
                (r["email"]?.ToString().ToLower().Contains(term) ?? false)
            );

            foreach (var row in rows) filtered.ImportRow(row);
            DisplayData(filtered);
        }

        private void InitializeComponent()
        {
            this.dgvCustomers = new System.Windows.Forms.DataGridView();
            this.btnAddNew = new System.Windows.Forms.Button();
            this.btnCustomerDetails = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnDeleteBulk = new System.Windows.Forms.Button();
            this.lblCustomersTitle = new System.Windows.Forms.Label();
            
            ((System.ComponentModel.ISupportInitialize)(this.dgvCustomers)).BeginInit();
            this.SuspendLayout();

            // Main Layout
            TableLayoutPanel tlpMain = new TableLayoutPanel();
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.RowCount = 2;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.BackColor = ThemeConfig.BackgroundColor;
            tlpMain.Padding = new Padding(20);

            // Header Panel
            Panel panelTop = new Panel();
            panelTop.Dock = DockStyle.Fill;
            panelTop.BackColor = ThemeConfig.BackgroundColor;
            panelTop.Margin = new Padding(0);

            // lblTitle
            this.lblCustomersTitle = ThemeConfig.CreateStandardHeader("Customers management");
            this.lblCustomersTitle.Name = "lblCustomersTitle";
            panelTop.Controls.Add(lblCustomersTitle);

            // Search Bar
            this.txtSearch = new ModernTextBox();
            txtSearch.IsSearch = true;
            txtSearch.ShowLabel = false;
            txtSearch.PlaceholderText = "Search Customers...";
            txtSearch.Size = new Size(320, 40);
            txtSearch.Location = new Point(0, 55);
            txtSearch.TextChanged += txtSearch_TextChanged;
            panelTop.Controls.Add(txtSearch);

            // Actions Panel (FlowLayout for Buttons)
            FlowLayoutPanel panelButtons = new FlowLayoutPanel();
            panelButtons.FlowDirection = FlowDirection.LeftToRight;
            panelButtons.AutoSize = true;
            panelButtons.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panelButtons.Location = new Point(360, 70); 
            panelButtons.Height = 40;
            panelButtons.WrapContents = false;
            panelButtons.Padding = new Padding(0);
            panelButtons.Margin = new Padding(0);

            // Import Button (Green Outline)
            this.btnImport.Size = new System.Drawing.Size(100, 40);
            this.btnImport.Text = "";
            this.btnImport.Name = "btnImportCust";
            this.btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.FlatAppearance.BorderSize = 0;
            btnImport.BackColor = ThemeConfig.SurfaceColor;
            btnImport.Cursor = Cursors.Hand;
            this.btnImport.Margin = new Padding(0, 0, 10, 0);
            this.btnImport.Click += btnImport_Click;
            this.btnImport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnImport, e.Graphics, "import", "Cust_Import", ThemeConfig.SuccessBorder, ThemeConfig.SuccessBorder, true);
            panelButtons.Controls.Add(btnImport);

            // Export Button (Blue Outline)
            this.btnExport.Size = new System.Drawing.Size(100, 40);
            this.btnExport.Text = "";
            this.btnExport.Name = "btnExportCust";
            this.btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.BackColor = ThemeConfig.SurfaceColor;
            btnExport.Cursor = Cursors.Hand;
            this.btnExport.Margin = new Padding(0, 0, 10, 0);
            this.btnExport.Click += btnExport_Click;
            this.btnExport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnExport, e.Graphics, "export", "Cust_Export", ThemeConfig.PrimaryColor, ThemeConfig.PrimaryColor, true);
            panelButtons.Controls.Add(btnExport);

            // Delete Selected Button (Red Outline)
            this.btnDeleteBulk.Size = new System.Drawing.Size(130, 40);
            this.btnDeleteBulk.Text = "";
            this.btnDeleteBulk.Name = "btnDeleteSelected";
            this.btnDeleteBulk.FlatStyle = FlatStyle.Flat;
            btnDeleteBulk.FlatAppearance.BorderSize = 0;
            btnDeleteBulk.BackColor = ThemeConfig.SurfaceColor;
            btnDeleteBulk.Cursor = Cursors.Hand;
            this.btnDeleteBulk.Margin = new Padding(0, 0, 10, 0);
            this.btnDeleteBulk.Click += btnDeleteBulk_Click;
            this.btnDeleteBulk.Paint += (s, e) => ThemeConfig.DrawIconButton(btnDeleteBulk, e.Graphics, "delete", "Cust_Delete", ThemeConfig.DangerBorder, ThemeConfig.DangerBorder, true);
            panelButtons.Controls.Add(btnDeleteBulk);

            // Details Button
            this.btnCustomerDetails.Size = new System.Drawing.Size(160, 40);
            this.btnCustomerDetails.Text = "";
            this.btnCustomerDetails.Name = "btnDetailsCust";
            this.btnCustomerDetails.FlatStyle = FlatStyle.Flat;
            btnCustomerDetails.FlatAppearance.BorderSize = 0;
            btnCustomerDetails.BackColor = ThemeConfig.SurfaceColor;
            btnCustomerDetails.Cursor = Cursors.Hand;
            this.btnCustomerDetails.Margin = new Padding(0, 0, 10, 0);
            this.btnCustomerDetails.Click += btnCustomerDetails_Click;
            this.btnCustomerDetails.Paint += (s, e) => ThemeConfig.DrawIconButton(btnCustomerDetails, e.Graphics, "view", "Cust_Details", ThemeConfig.TextColorLight, ThemeConfig.WarningColor, false);
            panelButtons.Controls.Add(btnCustomerDetails);

            // Add Customer Button (Primary Solid)
            this.btnAddNew.Size = new System.Drawing.Size(160, 40);
            this.btnAddNew.Text = "";
            this.btnAddNew.Name = "btnAddCust";
            this.btnAddNew.FlatStyle = FlatStyle.Flat;
            btnAddNew.FlatAppearance.BorderSize = 0;
            btnAddNew.BackColor = ThemeConfig.SurfaceColor;
            btnAddNew.Cursor = Cursors.Hand;
            this.btnAddNew.Margin = new Padding(0);
            this.btnAddNew.Click += btnAddNew_Click;
            this.btnAddNew.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAddNew, e.Graphics, "add", "Cust_AddCustomer", Color.White, ThemeConfig.PrimaryColor, false);
            panelButtons.Controls.Add(btnAddNew);

            panelTop.Controls.Add(panelButtons);
            
            // Align buttons panel to right (RTL Aware)
            panelTop.Resize += (s, e) => {
                if (LocalizationManager.IsArabic) {
                    txtSearch.Location = new Point(panelTop.Width - txtSearch.Width, 55);
                    panelButtons.Location = new Point(0, 50);
                } else {
                    txtSearch.Location = new Point(0, 55);
                    panelButtons.Location = new Point(panelTop.Width - panelButtons.Width, 50);
                }
            };

            tlpMain.Controls.Add(panelTop, 0, 0);

            // DataGridView
            dgvCustomers = new DataGridView { 
                Dock = DockStyle.Fill, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                MultiSelect = false,
                BackgroundColor = ThemeConfig.SurfaceColor,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0)
            };
            
            dgvCustomers.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colSelect", Width = 50, HeaderText = "" });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", Visible = false });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", Width = 200, MinimumWidth = 150 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPhone", Width = 130 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmail", Width = 180 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAddress", Width = 220 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBalance", Width = 120 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCreditLimit", Width = 120 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDueDate", Width = 120 });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colActions", Width = 100 });
            
            ThemeConfig.ApplyGridTheme(dgvCustomers);
            ThemeConfig.ApplyHeaderCheckBox(dgvCustomers, "colSelect");
            
            // Register Events
            dgvCustomers.CellPainting += DgvCustomers_CellPainting;
            dgvCustomers.CellMouseDown += DgvCustomers_CellMouseDown;
            
            Panel pnlCard = ThemeConfig.CreateCardPanel(dgvCustomers);
            tlpMain.Controls.Add(pnlCard, 0, 1);

            this.Controls.Add(tlpMain);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCustomers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            using (var form = new AddCustomerForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    SaveCustomer(form);
                    LoadData();
                }
            }
        }

        private void SaveCustomer(AddCustomerForm form)
        {
            try
            {
                string sql = "INSERT INTO customers (full_name, phone, email, address, type, current_balance, credit_limit, payment_due_date, reminder_days, date_added) " +
                             "VALUES (@name, @phone, @email, @addr, @type, 0, @limit, @due, @rem, datetime('now'))";
                
                DatabaseHelper.ExecuteNonQuery(sql,
                    new Microsoft.Data.Sqlite.SqliteParameter("@name", form.CustomerName),
                    new Microsoft.Data.Sqlite.SqliteParameter("@phone", form.Phone),
                    new Microsoft.Data.Sqlite.SqliteParameter("@email", form.Email),
                    new Microsoft.Data.Sqlite.SqliteParameter("@addr", form.Address),
                    new Microsoft.Data.Sqlite.SqliteParameter("@type", form.CustomerType),
                    new Microsoft.Data.Sqlite.SqliteParameter("@limit", form.CreditLimit),
                    new Microsoft.Data.Sqlite.SqliteParameter("@due", (object)form.DueDate ?? DBNull.Value),
                    new Microsoft.Data.Sqlite.SqliteParameter("@rem", form.ReminderDays));
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "adding customer");
            }
        }

        private void btnCustomerDetails_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells["colId"].Value);
            string name = dgvCustomers.SelectedRows[0].Cells["colName"].Value.ToString();
            ShowDetails(id, name);
        }

        private void ShowDetails(int id, string name)
        {
            using (var form = new CustomerDetailsForm(id, name))
            {
                form.ShowDialog();
            }
        }

        private void DgvCustomers_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            
            if (dgvCustomers.Columns[e.ColumnIndex].Name == "colActions")
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Edit Icon 
                Rectangle editRect = new Rectangle(e.CellBounds.X + 12, e.CellBounds.Y + (e.CellBounds.Height - 24)/2, 24, 24);
                Image imgEdit = ThemeConfig.GetNuricon("edit");
                if (imgEdit != null) e.Graphics.DrawImage(imgEdit, editRect);

                // Delete Icon
                Rectangle delRect = new Rectangle(e.CellBounds.X + 48, e.CellBounds.Y + (e.CellBounds.Height - 24)/2, 24, 24);
                Image imgDelete = ThemeConfig.GetNuricon("delete");
                if (imgDelete != null) e.Graphics.DrawImage(imgDelete, delRect);
            }
        }

        private void DgvCustomers_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.Button != MouseButtons.Left) return;
            
            string colName = dgvCustomers.Columns[e.ColumnIndex].Name;
            if (colName != "colActions") return;

            int id = Convert.ToInt32(dgvCustomers.Rows[e.RowIndex].Cells["colId"].Value);
            
            if (e.X >= 8 && e.X <= 40) // Edit Rect (12-36, added tolerance)
            {
                EditCustomer(id);
            }
            else if (e.X >= 44 && e.X <= 76) // Delete Rect (48-72, added tolerance)
            {
                DeleteCustomer(id);
            }
        }

        private void EditCustomer(int id)
        {
            try
            {
                DataRow r = _dtCustomers.AsEnumerable().FirstOrDefault(row => Convert.ToInt32(row["ID"]) == id);
                if (r == null) return;

                string name = r["full_name"].ToString();
                string phone = r["phone"].ToString();
                string email = r["email"].ToString();
                string address = r["address"].ToString();
                string type = r["type"].ToString();
                decimal limit = r["credit_limit"] != DBNull.Value ? Convert.ToDecimal(r["credit_limit"]) : 0;
                DateTime? due = r["payment_due_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["payment_due_date"]) : null;
                int rem = r["reminder_days"] != DBNull.Value ? Convert.ToInt32(r["reminder_days"]) : 0;

                using (var form = new AddCustomerForm(id, name, phone, email, address, type, limit, due, rem))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        UpdateCustomer(id, form);
                        LoadData();
                    }
                }
            }
            catch (Exception ex) { ErrorLogger.LogError(ex, "EditCustomer"); }
        }

        private void UpdateCustomer(int id, AddCustomerForm form)
        {
            try
            {
                string sql = "UPDATE customers SET full_name=@name, phone=@phone, email=@email, address=@addr, type=@type, credit_limit=@limit, payment_due_date=@due, reminder_days=@rem WHERE customer_id=@id";
                DatabaseHelper.ExecuteNonQuery(sql,
                    new Microsoft.Data.Sqlite.SqliteParameter("@name", form.CustomerName),
                    new Microsoft.Data.Sqlite.SqliteParameter("@phone", form.Phone),
                    new Microsoft.Data.Sqlite.SqliteParameter("@email", form.Email),
                    new Microsoft.Data.Sqlite.SqliteParameter("@addr", form.Address),
                    new Microsoft.Data.Sqlite.SqliteParameter("@type", form.CustomerType),
                    new Microsoft.Data.Sqlite.SqliteParameter("@limit", form.CreditLimit),
                    new Microsoft.Data.Sqlite.SqliteParameter("@due", (object)form.DueDate ?? DBNull.Value),
                    new Microsoft.Data.Sqlite.SqliteParameter("@rem", form.ReminderDays),
                    new Microsoft.Data.Sqlite.SqliteParameter("@id", id));
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "updating customer");
            }
        }

        private void DeleteCustomer(int id)
        {
            if (MessageHelper.ConfirmAction(LocalizationManager.GetString("Customers_DeleteConfirm") ?? "Are you sure you want to delete this customer?"))
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery("UPDATE customers SET date_deleted = datetime('now') WHERE customer_id = @id", new Microsoft.Data.Sqlite.SqliteParameter("@id", id));
                    LoadData();
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, "deleting customer");
                }
            }
        }

        private void btnDeleteBulk_Click(object sender, EventArgs e)
        {
            var ids = new List<int>();
            foreach (DataGridViewRow row in dgvCustomers.Rows)
            {
                if (Convert.ToBoolean(row.Cells["colSelect"].Value))
                {
                    ids.Add(Convert.ToInt32(row.Cells["colId"].Value));
                }
            }

            if (ids.Count == 0) return;

            if (MessageHelper.ConfirmAction("Are you sure you want to delete the selected customers?"))
            {
                try
                {
                    foreach (int id in ids)
                    {
                        DatabaseHelper.ExecuteNonQuery("UPDATE customers SET date_deleted = datetime('now') WHERE customer_id = @id", new Microsoft.Data.Sqlite.SqliteParameter("@id", id));
                    }
                    LoadData();
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, "bulk deleting customers");
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportToCsv();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            ImportFromCsv();
        }

        private void ExportToCsv()
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV Files (*.csv)|*.csv";
                saveDialog.FileName = $"Customers_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                saveDialog.Title = "Export Customers to CSV";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string sql = "SELECT full_name as CustomerName, phone as Phone, email as Email, address as Address, type as CustomerType, current_balance as Balance, credit_limit as CreditLimit FROM customers WHERE date_deleted IS NULL ORDER BY full_name";
                    DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
                    
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "Ù„Ø§ ØªÙˆØ¬Ø¯ Ø¨ÙŠØ§Ù†Ø§Øª Ù„Ù„ØªØµØ¯ÙŠØ±." : "No data to export.");
                        return;
                    }

                    if (Helpers.ImportExportHelper.ExportToCsv(dt, saveDialog.FileName))
                    {
                        string successMsg = LocalizationManager.IsArabic 
                            ? $"ØªÙ… ØªØµØ¯ÙŠØ± {dt.Rows.Count} Ù…Ù† Ø§Ù„Ø¹Ù…Ù„Ø§Ø¡ Ø¥Ù„Ù‰ Ù…Ù„Ù CSV Ø¨Ù†Ø¬Ø§Ø­!" 
                            : $"Exported {dt.Rows.Count} customers to CSV successfully!";
                        MessageHelper.ShowSuccess(successMsg);
                    }
                    else
                    {
                        MessageHelper.ShowError(LocalizationManager.IsArabic ? "ÙØ´Ù„ ØªØµØ¯ÙŠØ± Ø§Ù„Ø¨ÙŠØ§Ù†Ø§Øª." : "Failed to export data.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError((LocalizationManager.IsArabic ? "Ø®Ø·Ø£ ÙÙŠ Ø§Ù„ØªØµØ¯ÙŠØ±: " : "Export error: ") + ex.Message);
            }
        }

        private void ImportFromCsv()
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "CSV Files (*.csv)|*.csv";
                openDialog.Title = "Import Customers from CSV";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = Helpers.ImportExportHelper.ImportFromCsv(openDialog.FileName);
                    
                     if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "Ù„Ø§ ØªÙˆØ¬Ø¯ Ø¨ÙŠØ§Ù†Ø§Øª ÙÙŠ Ø§Ù„Ù…Ù„Ù." : "No data found in the file.");
                        return;
                    }

                     if (!dt.Columns.Contains("CustomerName"))
                    {
                        MessageHelper.ShowError(LocalizationManager.IsArabic 
                            ? "ØªÙ†Ø³ÙŠÙ‚ Ù…Ù„Ù ØºÙŠØ± ØµØ§Ù„Ø­. Ø§Ù„Ø£Ø¹Ù…Ø¯Ø© Ø§Ù„Ù…Ø·Ù„ÙˆØ¨Ø©: CustomerName, Phone, Email, Address, CustomerType" 
                            : "Invalid file format. Required columns: CustomerName, Phone, Email, Address, CustomerType");
                        return;
                    }

                    int imported = 0;
                    int skipped = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        try
                        {
                            string custName = row.Table.Columns.Contains("CustomerName") ? row["CustomerName"].ToString() : "";
                            
                            if (string.IsNullOrWhiteSpace(custName))
                            {
                                skipped++;
                                continue;
                            }

                            string checkSql = "SELECT COUNT(*) FROM customers WHERE full_name = @n AND date_deleted IS NULL";
                            int count = DatabaseHelper.ExecuteScalar<int>(checkSql, new Microsoft.Data.Sqlite.SqliteParameter("@n", custName));
                            
                            if (count > 0)
                            {
                                skipped++;
                                continue;
                            }

                            string phone = row.Table.Columns.Contains("Phone") ? row["Phone"].ToString() : "";
                            string email = row.Table.Columns.Contains("Email") ? row["Email"].ToString() : "";
                            string address = row.Table.Columns.Contains("Address") ? row["Address"].ToString() : "";
                            string type = row.Table.Columns.Contains("CustomerType") ? row["CustomerType"].ToString() : "Individual";

                            string sql = "INSERT INTO customers (full_name, phone, email, address, type, current_balance, date_added) " +
                                         "VALUES (@name, @phone, @email, @addr, @type, 0, datetime('now'))";
                            
                            DatabaseHelper.ExecuteNonQuery(sql,
                                new Microsoft.Data.Sqlite.SqliteParameter("@name", custName),
                                new Microsoft.Data.Sqlite.SqliteParameter("@phone", phone),
                                new Microsoft.Data.Sqlite.SqliteParameter("@email", email),
                                new Microsoft.Data.Sqlite.SqliteParameter("@addr", address),
                                new Microsoft.Data.Sqlite.SqliteParameter("@type", type));
                                
                            imported++;
                        }
                        catch
                        {
                            skipped++;
                        }
                    }

                    LoadData();
                    string completeMsg = LocalizationManager.IsArabic 
                        ? $"Ø§ÙƒØªÙ…Ù„ Ø§Ù„Ø§Ø³ØªÙŠØ±Ø§Ø¯!\nØªÙ… Ø§Ø³ØªÙŠØ±Ø§Ø¯: {imported}\nØªÙ… ØªØ®Ø·ÙŠ: {skipped}" 
                        : $"Import complete!\nImported: {imported}\nSkipped: {skipped}";
                    MessageHelper.ShowSuccess(completeMsg);
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError((LocalizationManager.IsArabic ? "Ø®Ø·Ø£ ÙÙŠ Ø§Ù„Ø§Ø³ØªÙŠØ±Ø§Ø¯: " : "Import error: ") + ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }
    }
}
