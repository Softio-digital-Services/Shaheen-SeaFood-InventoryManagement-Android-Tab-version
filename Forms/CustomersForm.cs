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

                if (dgvCustomers.Columns.Contains("colCreditLimit")) dgvCustomers.Columns["colCreditLimit"].HeaderText = L("AddCust_CreditLimit");
                if (dgvCustomers.Columns.Contains("colDueDate")) dgvCustomers.Columns["colDueDate"].HeaderText = L("AddCust_DueDate");
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

            tlpMain.Padding = new Padding(20);

            // Header Panel (TableLayoutPanel for robust RTL)
            TableLayoutPanel tlpHeader = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                ColumnCount = 1,
                RowCount = 2
            };
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Title Row
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Actions Row
            
            // Row 0: Title
            this.lblCustomersTitle = ThemeConfig.CreateStandardHeader("Customers management");
            this.lblCustomersTitle.Name = "lblCustomersTitle";
            tlpHeader.Controls.Add(lblCustomersTitle, 0, 0);

            // Row 1: Search + Buttons
            TableLayoutPanel tlpActions = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                ColumnCount = 2,
                RowCount = 1
            };
            tlpActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350F));
            tlpActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Search Bar
            this.txtSearch = new ModernTextBox();
            txtSearch.IsSearch = true;
            txtSearch.ShowLabel = false;
            txtSearch.PlaceholderText = "Search Customers...";
            txtSearch.Size = new Size(320, 40);
            txtSearch.Anchor = AnchorStyles.Left;
            txtSearch.TextChanged += txtSearch_TextChanged;
            tlpActions.Controls.Add(txtSearch, 0, 0);

            // Actions Panel (FlowLayout for Buttons)
            FlowLayoutPanel panelButtons = new FlowLayoutPanel();
            panelButtons.FlowDirection = FlowDirection.LeftToRight;
            panelButtons.AutoSize = true;
            panelButtons.Anchor = AnchorStyles.Right;
            panelButtons.WrapContents = false;
            panelButtons.Padding = new Padding(0);
            panelButtons.Margin = new Padding(0);

            // Add Customer Button (Primary Solid) - Added first to FLP (RTL)
            this.btnAddNew.Size = new System.Drawing.Size(160, 40);
            this.btnAddNew.Text = "";
            this.btnAddNew.Name = "btnAddCust";
            this.btnAddNew.FlatStyle = FlatStyle.Flat;
            btnAddNew.FlatAppearance.BorderSize = 0;
            btnAddNew.Cursor = Cursors.Hand;
            btnAddNew.Margin = new Padding(0, 0, 10, 0);
            btnAddNew.Click += btnAddNew_Click;
            btnAddNew.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAddNew, e.Graphics, "add", "Cust_AddCustomer", Color.White, ThemeConfig.PrimaryColor, false);
            panelButtons.Controls.Add(btnAddNew);

            // Details Button
            this.btnCustomerDetails.Size = new System.Drawing.Size(160, 40);
            this.btnCustomerDetails.Text = "";
            this.btnCustomerDetails.Name = "btnDetailsCust";
            this.btnCustomerDetails.FlatStyle = FlatStyle.Flat;
            btnCustomerDetails.FlatAppearance.BorderSize = 0;
            btnCustomerDetails.Cursor = Cursors.Hand;
            btnCustomerDetails.Margin = new Padding(0, 0, 10, 0);
            btnCustomerDetails.Click += btnCustomerDetails_Click;
            btnCustomerDetails.Paint += (s, e) => ThemeConfig.DrawIconButton(btnCustomerDetails, e.Graphics, "view", "Cust_Details", ThemeConfig.TextColorLight, ThemeConfig.WarningColor, false);
            panelButtons.Controls.Add(btnCustomerDetails);

            // Delete Selected Button (Red Outline)
            this.btnDeleteBulk.Size = new System.Drawing.Size(130, 40);
            this.btnDeleteBulk.Text = "";
            this.btnDeleteBulk.Name = "btnDeleteSelected";
            this.btnDeleteBulk.FlatStyle = FlatStyle.Flat;
            btnDeleteBulk.FlatAppearance.BorderSize = 0;
            btnDeleteBulk.Cursor = Cursors.Hand;
            btnDeleteBulk.Margin = new Padding(0, 0, 10, 0);
            btnDeleteBulk.Click += btnDeleteBulk_Click;
            btnDeleteBulk.Paint += (s, e) => ThemeConfig.DrawIconButton(btnDeleteBulk, e.Graphics, "delete", "Cust_Delete", ThemeConfig.DangerBorder, ThemeConfig.DangerBorder, true);
            panelButtons.Controls.Add(btnDeleteBulk);

            // Export Button (Blue Outline)
            this.btnExport.Size = new System.Drawing.Size(100, 40);
            this.btnExport.Text = "";
            this.btnExport.Name = "btnExportCust";
            this.btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Cursor = Cursors.Hand;
            btnExport.Margin = new Padding(0, 0, 10, 0);
            btnExport.Click += btnExport_Click;
            btnExport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnExport, e.Graphics, "export", "Cust_Export", ThemeConfig.PrimaryColor, ThemeConfig.PrimaryColor, true);
            panelButtons.Controls.Add(btnExport);

            // Import Button (Green Outline)
            this.btnImport.Size = new System.Drawing.Size(100, 40);
            this.btnImport.Text = "";
            this.btnImport.Name = "btnImportCust";
            this.btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.FlatAppearance.BorderSize = 0;
            btnImport.Cursor = Cursors.Hand;
            btnImport.Margin = new Padding(0, 0, 10, 0);
            btnImport.Click += btnImport_Click;
            btnImport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnImport, e.Graphics, "import", "Cust_Import", ThemeConfig.SuccessBorder, ThemeConfig.SuccessBorder, true);
            panelButtons.Controls.Add(btnImport);

            tlpActions.Controls.Add(panelButtons, 1, 0);
            tlpHeader.Controls.Add(tlpActions, 0, 1);

            tlpMain.Controls.Add(tlpHeader, 0, 0);

            // DataGridView
            dgvCustomers = new DataGridView
            {
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
                Rectangle editRect = new Rectangle(e.CellBounds.X + 12, e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, 24, 24);
                Image imgEdit = ThemeConfig.GetNuricon("edit");
                if (imgEdit != null) e.Graphics.DrawImage(imgEdit, editRect);

                // Delete Icon
                Rectangle delRect = new Rectangle(e.CellBounds.X + 48, e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, 24, 24);
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
                        MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoDataExport"));
                        return;
                    }

                    if (Helpers.ImportExportHelper.ExportToCsv(dt, saveDialog.FileName))
                    {
                        string successMsg = LocalizationManager.IsArabic
                            ? $"تم تصدير {dt.Rows.Count} عملاء إلى CSV بنجاح!"
                            : $"Exported {dt.Rows.Count} customers to CSV successfully!";
                        MessageHelper.ShowSuccess(successMsg);
                    }
                    else
                    {
                        MessageHelper.ShowError(LocalizationManager.GetString("Msg_ExportFailed"));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError((LocalizationManager.GetString("Msg_ExportError")) + ex.Message);
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
                        MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoDataFile"));
                        return;
                    }

                    if (!dt.Columns.Contains("CustomerName"))
                    {
                        MessageHelper.ShowError(LocalizationManager.IsArabic
                            ? "تنسيق الملف غير صالح. الأعمدة المطلوبة: CustomerName, Phone, Email, Address, CustomerType"
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
                        ? $"اكتمل الاستيراد!\nتم الاستيراد: {imported}\nتم التخطي: {skipped}"
                        : $"Import complete!\nImported: {imported}\nSkipped: {skipped}";
                    MessageHelper.ShowSuccess(completeMsg);
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError((LocalizationManager.GetString("Msg_ImportError")) + ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }
    }
}
