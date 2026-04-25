using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GenericInventorySystem.Data;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Services;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    public partial class CustomersForm : UserControl
    {
        private DataGridView dgvCustomers;
        private Button btnAdd;
        private Button btnDetails;
        private Button btnImport;
        private Button btnExport;
        private Label lblCustomersTitle;
        private ModernTextBox txtSearch;
        private CustomerService _customerService;

        public CustomersForm()
        {
            InitializeComponent();
            _customerService = new CustomerService();
            ApplyTheme();
            
            GenericInventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            // Auto-refresh grid when any customer balance changes (e.g. from CustomerDetailsForm popup)
            GlobalEvents.OnCustomersUpdated += () => { if (this.IsHandleCreated) this.Invoke((Action)(() => LoadData())); };
            ApplyLocalization();
            ApplyPermissions();
        }

        private void ApplyLocalization()
        {
            GenericInventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            GenericInventorySystem.Helpers.LocalizationManager.TranslateControl(this);
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;

            if (lblCustomersTitle != null) lblCustomersTitle.Text = L("Cust_Title");

            if (txtSearch != null)
            {
                txtSearch.PlaceholderText = L("Cust_Search");
            }

            if (btnAdd != null) btnAdd.Invalidate(); 
            if (btnDetails != null) btnDetails.Invalidate();
            if (btnImport != null) btnImport.Invalidate();
            if (btnExport != null) btnExport.Invalidate();
            
            var ctrlDel = this.Controls.Find("btnDeleteSelected", true);
            if (ctrlDel.Length > 0) ctrlDel[0].Invalidate();

            if (dgvCustomers != null && dgvCustomers.Columns.Count > 0)
            {
                if (dgvCustomers.Columns.Contains("colName")) dgvCustomers.Columns["colName"].HeaderText = L("Cust_GridName");
                if (dgvCustomers.Columns.Contains("colPhone")) dgvCustomers.Columns["colPhone"].HeaderText = L("Cust_GridPhone");
                if (dgvCustomers.Columns.Contains("colEmail")) dgvCustomers.Columns["colEmail"].HeaderText = L("Cust_GridEmail");
                if (dgvCustomers.Columns.Contains("colAddress")) dgvCustomers.Columns["colAddress"].HeaderText = L("Cust_GridAddress");
                if (dgvCustomers.Columns.Contains("colBalance")) dgvCustomers.Columns["colBalance"].HeaderText = L("Cust_GridBalance");
                if (dgvCustomers.Columns.Contains("colCreditLimit")) dgvCustomers.Columns["colCreditLimit"].HeaderText = L("AddCust_CreditLimit") ?? "Credit Limit";
                if (dgvCustomers.Columns.Contains("colDueDate")) dgvCustomers.Columns["colDueDate"].HeaderText = L("AddCust_DueDate") ?? "Due Date";
                if (dgvCustomers.Columns.Contains("colActions")) dgvCustomers.Columns["colActions"].HeaderText = L("Cust_GridActions");
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

        private void InitializeComponent()
        {
            this.dgvCustomers = new System.Windows.Forms.DataGridView();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDetails = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.lblCustomersTitle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCustomers)).BeginInit();
            this.SuspendLayout();

            // Main Layout
            TableLayoutPanel tlpMain = new TableLayoutPanel();
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.RowCount = 2;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F)); // Header height
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.BackColor = ThemeConfig.BackgroundColor;

            // Header Panel
            Panel panelTop = new Panel();
            panelTop.Dock = DockStyle.Fill;
            panelTop.BackColor = ThemeConfig.BackgroundColor;
            panelTop.Padding = new Padding(20);

            // lblCustomersTitle
            this.lblCustomersTitle = ThemeConfig.CreateStandardHeader("Customers Directory");
            this.lblCustomersTitle.Name = "lblCustomersTitle";

            // Search Bar (Upgraded to ModernTextBox)
            txtSearch = new ModernTextBox();
            txtSearch.IsSearch = true;
            txtSearch.ShowLabel = false;
            txtSearch.PlaceholderText = "Search Customers...";
            txtSearch.Size = new Size(320, 40);
            txtSearch.Location = new Point(20, 75);
            txtSearch.TextChanged += (s, e) => { 
                string ph = GenericInventorySystem.Helpers.LocalizationManager.GetString("Cust_Search");
                if (txtSearch.Text != ph && txtSearch.Text != "Search...") 
                    LoadData(txtSearch.Text); 
            };
            panelTop.Controls.Add(txtSearch);

            // Actions Panel (FlowLayout for Buttons)
            FlowLayoutPanel panelActions = new FlowLayoutPanel();
            panelActions.FlowDirection = FlowDirection.LeftToRight;
            panelActions.AutoSize = true;
            panelActions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panelActions.Location = new Point(360, 70);
            panelActions.Height = 40;
            panelActions.WrapContents = false;
            panelActions.Padding = new Padding(0);
            panelActions.Margin = new Padding(0);
            
            // Import Button (Green Outline)
            this.btnImport.Size = new System.Drawing.Size(100, 40);
            this.btnImport.Text = "";
            this.btnImport.Name = "btnImportCust";
            this.btnImport.FlatStyle = FlatStyle.Flat;
            this.btnImport.FlatAppearance.BorderSize = 0;
            this.btnImport.BackColor = ThemeConfig.SurfaceColor;
            this.btnImport.Cursor = Cursors.Hand;
            this.btnImport.Margin = new Padding(0, 0, 10, 0);
            this.btnImport.Click += BtnImport_Click;
            this.btnImport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnImport, e.Graphics, "import", "Cust_Import", ThemeConfig.SuccessBorder, ThemeConfig.SuccessBorder, true);

            // Export Button (Blue Outline)
            this.btnExport.Size = new System.Drawing.Size(100, 40);
            this.btnExport.Text = "";
            this.btnExport.Name = "btnExportCust";
            this.btnExport.FlatStyle = FlatStyle.Flat;
            this.btnExport.FlatAppearance.BorderSize = 0;
            this.btnExport.BackColor = ThemeConfig.SurfaceColor;
            this.btnExport.Cursor = Cursors.Hand;
            this.btnExport.Margin = new Padding(0, 0, 10, 0);
            this.btnExport.Click += BtnExport_Click;
            this.btnExport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnExport, e.Graphics, "export", "Cust_Export", ThemeConfig.PrimaryColor, ThemeConfig.PrimaryColor, true);

            // Customer Details Button (Blue)
            this.btnDetails = new Button();
            this.btnDetails.Size = new System.Drawing.Size(160, 40);
            this.btnDetails.Text = "";
            this.btnDetails.Name = "btnDetailsCust";
            this.btnDetails.FlatStyle = FlatStyle.Flat;
            this.btnDetails.FlatAppearance.BorderSize = 0;
            this.btnDetails.BackColor = Color.Transparent;
            this.btnDetails.Cursor = Cursors.Hand;
            this.btnDetails.Margin = new Padding(0, 0, 10, 0);
            this.btnDetails.Click += BtnDetails_Click;
            this.btnDetails.Paint += (s, e) => ThemeConfig.DrawIconButton(btnDetails, e.Graphics, "view", "Cust_Details", ThemeConfig.TextColorLight, ThemeConfig.SuccessColor, false);

            // Add Customer (Blue)
            this.btnAdd.Size = new System.Drawing.Size(160, 40);
            this.btnAdd.Text = "";
            this.btnAdd.Name = "btnAddCustomer"; 
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.BackColor = ThemeConfig.SurfaceColor; 
            this.btnAdd.Cursor = Cursors.Hand;
            this.btnAdd.Margin = new Padding(0);
            this.btnAdd.Click += BtnAdd_Click;
            this.btnAdd.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAdd, e.Graphics, "add", "Cust_AddCustomer", ThemeConfig.TextColorLight, ThemeConfig.PrimaryColor, false);

            // Delete Selected Button (Red Outline)
            Button btnDeleteSelected = new Button();
            btnDeleteSelected.Size = new Size(130, 40);
            btnDeleteSelected.Text = ""; 
            btnDeleteSelected.Name = "btnDeleteSelected";
            btnDeleteSelected.FlatStyle = FlatStyle.Flat;
            btnDeleteSelected.FlatAppearance.BorderSize = 0;
            btnDeleteSelected.BackColor = ThemeConfig.SurfaceColor;
            btnDeleteSelected.Cursor = Cursors.Hand;
            btnDeleteSelected.Margin = new Padding(0, 0, 10, 0);
            btnDeleteSelected.Click += (s, e) =>
            {
                var checkedIds = new System.Collections.Generic.List<int>();
                foreach (DataGridViewRow row in dgvCustomers.Rows)
                {
                    var chkCell = row.Cells["colCheck"] as DataGridViewCheckBoxCell;
                    if (chkCell != null && Convert.ToBoolean(chkCell.Value ?? false))
                    {
                        if (int.TryParse(row.Cells["customer_id"].Value?.ToString(), out int cId))
                            checkedIds.Add(cId);
                    }
                }
                if (checkedIds.Count == 0)
                {
                    MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "يرجى تحديد عميل واحد على الأقل لحذفه." : "Please select at least one customer to delete.");
                    return;
                }
                string confirmMsg = LocalizationManager.IsArabic 
                    ? $"هل أنت متأكد من رغبتك في حذف {checkedIds.Count} من العملاء المحددين؟" 
                    : $"Are you sure you want to delete {checkedIds.Count} selected customers?";
                if (MessageHelper.ConfirmAction(confirmMsg))
                {
                    foreach(int i in checkedIds) _customerService.DeleteCustomer(i);
                    string successMsg = LocalizationManager.IsArabic 
                        ? $"تم حذف {checkedIds.Count} من العملاء بنجاح." 
                        : $"{checkedIds.Count} customers deleted successfully.";
                    MessageHelper.ShowSuccess(successMsg);
                    LoadData(txtSearch.Text == "Search..." ? "" : txtSearch.Text);
                }
            };
            btnDeleteSelected.Paint += (s, e) => ThemeConfig.DrawIconButton(btnDeleteSelected, e.Graphics, "delete", "Cust_Delete", ThemeConfig.DangerColor, ThemeConfig.DangerColor, true);

            panelActions.Controls.Add(this.btnImport);
            panelActions.Controls.Add(this.btnExport);
            panelActions.Controls.Add(btnDeleteSelected);
            panelActions.Controls.Add(this.btnDetails);
            panelActions.Controls.Add(this.btnAdd);

            panelTop.Controls.Add(this.lblCustomersTitle);
            panelTop.Controls.Add(txtSearch); 
            panelTop.Controls.Add(panelActions);
            
            panelTop.Resize += (s, e) =>
            {
                if (GenericInventorySystem.Helpers.LocalizationManager.IsArabic)
                {
                    txtSearch.Location = new Point(panelTop.Width - txtSearch.Width - 20, 75);
                    panelActions.Location = new Point(20, 70);
                }
                else
                {
                    txtSearch.Location = new Point(20, 75);
                    panelActions.Location = new Point(panelTop.Width - panelActions.Width - 20, 70);
                }
            };

            // Grid Panel
            Panel panelGrid = new Panel();
            panelGrid.Dock = DockStyle.Fill;
            panelGrid.Padding = new Padding(20, 10, 20, 20);
            panelGrid.BackColor = ThemeConfig.BackgroundColor;

            // Grid Config
            this.dgvCustomers.Dock = DockStyle.Fill;
            this.dgvCustomers.AllowUserToAddRows = false;
            this.dgvCustomers.ReadOnly = false; 
            this.dgvCustomers.RowHeadersVisible = false;
            this.dgvCustomers.BackgroundColor = ThemeConfig.SurfaceColor;
            this.dgvCustomers.BorderStyle = BorderStyle.None;
            this.dgvCustomers.AutoGenerateColumns = false; 
            this.dgvCustomers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvCustomers.MultiSelect = false; 
            
            dgvCustomers.DefaultCellStyle.SelectionForeColor = ThemeConfig.TextColorDark;
            dgvCustomers.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvCustomers.GridColor = ThemeConfig.BorderColor;

            // Events
            dgvCustomers.CellPainting += DgvCustomers_CellPainting;
            dgvCustomers.CellContentClick += DgvCustomers_CellContentClick;
            dgvCustomers.CellMouseMove += DgvCustomers_CellMouseMove;
            dgvCustomers.CellMouseLeave += DgvCustomers_CellMouseLeave;
            dgvCustomers.DataError += (s, e) => {
                Console.WriteLine("DataError: " + (e.Exception != null ? e.Exception.Message : "Unknown"));
                e.ThrowException = false;
            };

            // Columns
            dgvCustomers.Columns.Clear();
            dgvCustomers.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colCheck", HeaderText = "", Width = 30, ReadOnly = false });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "Customer Name", DataPropertyName = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPhone", HeaderText = "Phone", DataPropertyName = "Phone", Width = 120, ReadOnly = true });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmail", HeaderText = "Email", DataPropertyName = "Email", Width = 200, ReadOnly = true });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAddress", HeaderText = "Address", DataPropertyName = "Address", Width = 200, ReadOnly = true });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBalance", HeaderText = "Balance Due", DataPropertyName = "Balance Due", Width = 120, ReadOnly = true });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCreditLimit", HeaderText = "Credit Limit", DataPropertyName = "credit_limit", Width = 100, ReadOnly = true });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDueDate", HeaderText = "Due Date", DataPropertyName = "payment_due_date", Width = 100, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" } });
            dgvCustomers.Columns.Add(new DataGridViewButtonColumn { Name = "colActions", HeaderText = "Actions", Width = 100, ReadOnly = true });

            // Hidden Fields
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "ID", Visible = false });
            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { Name = "reminder_days", DataPropertyName = "reminder_days", Visible = false });

            panelGrid.Controls.Add(this.dgvCustomers);

            tlpMain.Controls.Add(panelTop, 0, 0);
            tlpMain.Controls.Add(panelGrid, 0, 1);

            this.Controls.Add(tlpMain);
            this.Size = new System.Drawing.Size(950, 600); 

            ((System.ComponentModel.ISupportInitialize)(this.dgvCustomers)).EndInit();
            this.ResumeLayout(false);
        }

        private void DgvCustomers_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
             e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

             if (dgvCustomers.Columns[e.ColumnIndex].Name == "colActions")
             {
                 e.Handled = true;
                 e.PaintBackground(e.CellBounds, true);
                 
                 // Edit Icon 
                 Rectangle editRect = new Rectangle(e.CellBounds.X + 5, e.CellBounds.Y + 14, 32, 32);
                 DrawNuriconButton(e.Graphics, editRect, ThemeConfig.GetNuricon("edit"));
 
                 // Delete Icon
                 Rectangle delRect = new Rectangle(e.CellBounds.X + 45, e.CellBounds.Y + 14, 32, 32);
                 DrawNuriconButton(e.Graphics, delRect, ThemeConfig.GetNuricon("delete"));
             }
        }

        private void DrawNuriconButton(Graphics g, Rectangle rect, Image icon)
        {
            using (var path = GetRoundedRect(rect, 8))
            using (var pen = new Pen(ThemeConfig.BorderColor, 1))
            using (var brush = new SolidBrush(ThemeConfig.SurfaceColor))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
            if (icon != null) g.DrawImage(icon, new Rectangle(rect.X + 6, rect.Y + 6, 20, 20));
        }

        private void DgvCustomers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            
            string colName = dgvCustomers.Columns[e.ColumnIndex].Name;
            if (colName != "colActions") return;

            int id = Convert.ToInt32(dgvCustomers.Rows[e.RowIndex].Cells["ID"].Value);
            
            Point cur = dgvCustomers.PointToClient(Cursor.Position);
            Rectangle cellBounds = dgvCustomers.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
            int relX = cur.X - cellBounds.X;

            if (relX >= 5 && relX <= 37) // Edit Rect (5, 14, 32, 32)
            {
                // Edit
                if (!GenericInventorySystem.Helpers.UserSession.IsAdmin)
                {
                    MessageHelper.ShowWarning(GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "\u0644\u064a\u0633 \u0644\u062f\u064a\u0643 \u0635\u0644\u0627\u062d\u064a\u0629 \u0644\u062a\u0639\u062f\u064a\u0644 \u0627\u0644\u0639\u0645\u0644\u0627\u0621." : "You do not have permission to edit customers.");
                    return;
                }

                string name = dgvCustomers.Rows[e.RowIndex].Cells["colName"].Value?.ToString() ?? "";
                string phone = dgvCustomers.Rows[e.RowIndex].Cells["colPhone"].Value?.ToString() ?? "";
                string email = dgvCustomers.Rows[e.RowIndex].Cells["colEmail"].Value?.ToString() ?? "";
                string addr = dgvCustomers.Rows[e.RowIndex].Cells["colAddress"].Value?.ToString() ?? "";
                
                object creditVal = dgvCustomers.Rows[e.RowIndex].Cells["colCreditLimit"].Value;
                decimal credit = (creditVal == null || creditVal == DBNull.Value) ? 1000 : Convert.ToDecimal(creditVal);
                
                DateTime? dueDate = dgvCustomers.Rows[e.RowIndex].Cells["colDueDate"].Value as DateTime?;
                
                object remVal = dgvCustomers.Rows[e.RowIndex].Cells["reminder_days"].Value;
                int reminderDays = (remVal == null || remVal == DBNull.Value) ? 0 : Convert.ToInt32(remVal);
                string type = "Individual";

                AddCustomerForm form = new AddCustomerForm(id, name, phone, email, addr, type, credit, dueDate, reminderDays);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _customerService.UpdateCustomer(id, form.CustomerName, form.Phone, form.Email, form.Address, form.CustomerType, form.CreditLimit, form.DueDate, form.ReminderDays);
                    LoadData();
                }
            }
            else if (relX >= 45 && relX <= 77) // Delete Rect (45, 14, 32, 32)
            {
                // Delete
                if (!GenericInventorySystem.Helpers.UserSession.IsAdmin)
                {
                    MessageHelper.ShowWarning(GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "\u0644\u064a\u0633 \u0644\u062f\u064a\u0643 \u0635\u0644\u0627\u062d\u064a\u0629 \u0644\u062d\u0630\u0641 \u0627\u0644\u0639\u0645\u0644\u0627\u0621." : "You do not have permission to delete customers.");
                    return;
                }

                 if (MessageHelper.ShowConfirm(LocalizationManager.IsArabic ? "هل أنت متأكد من رغبتك في حذف هذا العميل؟" : "Are you sure you want to delete this customer?"))
                {
                    _customerService.DeleteCustomer(id);
                    LoadData();
                }
            }
        }

        private void DgvCustomers_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
             if (e.RowIndex >= 0 && dgvCustomers.Columns[e.ColumnIndex].Name == "colActions")
             {
                 dgvCustomers.Cursor = Cursors.Hand;
             }
             else
             {
                 dgvCustomers.Cursor = Cursors.Default;
             }
        }

        private void DgvCustomers_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
             dgvCustomers.Cursor = Cursors.Default;
        }

        private void UpdateStats()
        {
            // Stats removed from UI as per new design
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeConfig.BackgroundColor;
            lblCustomersTitle.ForeColor = ThemeConfig.PrimaryColor;
            ThemeConfig.ApplyGridTheme(dgvCustomers);
        }

        private void LoadData(string search = "")
        {
            try
            {
                DataTable dt = _customerService.GetAllCustomers(search);
                dgvCustomers.DataSource = dt;
            }
            catch(Exception ex) { MessageHelper.ShowError((LocalizationManager.IsArabic ? "خطأ في تحميل البيانات: " : "Error loading data: ") + ex.Message); }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            AddCustomerForm form = new AddCustomerForm();
            if(form.ShowDialog() == DialogResult.OK)
            {
                _customerService.AddCustomer(form.CustomerName, form.Phone, form.Email, form.Address, form.CustomerType, form.CreditLimit, form.DueDate, form.ReminderDays);
                LoadData();
            }
        }

        private void BtnDetails_Click(object sender, EventArgs e)
        {
            if(dgvCustomers.SelectedRows.Count == 0) { MessageHelper.ShowInfo(LocalizationManager.IsArabic ? "يرجى تحديد عميل." : "Please select a customer."); return; }
            int id = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells["ID"].Value);
            string name = dgvCustomers.SelectedRows[0].Cells["colName"].Value.ToString();
            var form = new CustomerDetailsForm(id, name);
            form.ShowDialog();
            LoadData();
        }

        private void DrawRoundedButton(Graphics g, Rectangle rect, string text, Color bgColor, Color textColor)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, rect.Width - 1, rect.Height - 1);
            
            using (var path = GetRoundedRect(r, 8))
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillPath(brush, path);
            }
            TextRenderer.DrawText(g, text, ThemeConfig.ButtonFont, r, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void DrawOutlinedButton(Graphics g, Rectangle rect, string icon, Color color)
        {
            using (var path = GetRoundedRect(rect, 8))
            using (var pen = new Pen(ThemeConfig.BorderColor, 1))
            using (var brush = new SolidBrush(ThemeConfig.SurfaceColor))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            TextRenderer.DrawText(g, icon, ThemeConfig.EmojiFont, rect, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            ExportToCsv();
        }

        private void BtnImport_Click(object sender, EventArgs e)
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
                    DataTable dt = (DataTable)dgvCustomers.DataSource;
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "لا توجد بيانات للتصدير." : "No data to export.");
                        return;
                    }

                    DataTable exportDt = new DataTable();
                    exportDt.Columns.Add("CustomerName");
                    exportDt.Columns.Add("Email");
                    exportDt.Columns.Add("Phone");
                    exportDt.Columns.Add("Address");
                    exportDt.Columns.Add("City");
                    exportDt.Columns.Add("PostalCode");
                    exportDt.Columns.Add("Notes");

                    foreach (DataRow row in dt.Rows)
                    {
                        exportDt.Rows.Add(
                            row["Name"],
                            row["Email"],
                            row["Phone"],
                            row["Address"],
                            "",
                            "",
                            ""
                        );
                    }

                    if (Helpers.ImportExportHelper.ExportToCsv(exportDt, saveDialog.FileName))
                    {
                        string successMsg = LocalizationManager.IsArabic 
                            ? $"تم تصدير {exportDt.Rows.Count} من العملاء إلى ملف CSV بنجاح!" 
                            : $"Exported {exportDt.Rows.Count} customers to CSV successfully!";
                        MessageHelper.ShowSuccess(successMsg);
                    }
                    else
                    {
                        MessageHelper.ShowError(LocalizationManager.IsArabic ? "فشل تصدير البيانات." : "Failed to export data.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError((LocalizationManager.IsArabic ? "خطأ في التصدير: " : "Export error: ") + ex.Message);
            }
        }

        private void ExportToExcel()
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                saveDialog.FileName = $"Customers_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                saveDialog.Title = "Export Customers to Excel";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = (DataTable)dgvCustomers.DataSource;
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "لا توجد بيانات للتصدير." : "No data to export.");
                        return;
                    }

                    DataTable exportDt = new DataTable();
                    exportDt.Columns.Add("CustomerName");
                    exportDt.Columns.Add("Email");
                    exportDt.Columns.Add("Phone");
                    exportDt.Columns.Add("Address");
                    exportDt.Columns.Add("City");
                    exportDt.Columns.Add("PostalCode");
                    exportDt.Columns.Add("Notes");

                    foreach (DataRow row in dt.Rows)
                    {
                        exportDt.Rows.Add(
                            row["Name"],
                            row["Email"],
                            row["Phone"],
                            row["Address"],
                            "",
                            "",
                            ""
                        );
                    }

                    if (Helpers.ImportExportHelper.ExportToExcel(exportDt, saveDialog.FileName, "Customers"))
                    {
                        string successMsg = LocalizationManager.IsArabic 
                            ? $"تم تصدير {exportDt.Rows.Count} من العملاء إلى ملف Excel بنجاح!" 
                            : $"Exported {exportDt.Rows.Count} customers to Excel successfully!";
                        MessageHelper.ShowSuccess(successMsg);
                    }
                    else
                    {
                        MessageHelper.ShowError(LocalizationManager.IsArabic ? "فشل تصدير البيانات." : "Failed to export data.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError((LocalizationManager.IsArabic ? "خطأ في التصدير: " : "Export error: ") + ex.Message);
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
                        MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "لم يتم العثور على بيانات في الملف." : "No data found in the file.");
                        return;
                    }

                    if (!dt.Columns.Contains("CustomerName") || !dt.Columns.Contains("Email"))
                    {
                        MessageHelper.ShowError(LocalizationManager.IsArabic 
                            ? "تنسيق ملف غير صالح. الأعمدة المطلوبة: CustomerName, Email, Phone, Address, City, PostalCode, Notes" 
                            : "Invalid file format. Required columns: CustomerName, Email, Phone, Address, City, PostalCode, Notes");
                        return;
                    }

                    int imported = 0;
                    int skipped = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        try
                        {
                            string customerName = row["CustomerName"].ToString();
                            string email = row["Email"].ToString();
                            
                            if (string.IsNullOrWhiteSpace(customerName))
                            {
                                skipped++;
                                continue;
                            }

                            if (_customerService.CustomerExists(email))
                            {
                                skipped++;
                                continue;
                            }

                            _customerService.ImportCustomer(
                                customerName,
                                email,
                                row["Phone"].ToString(),
                                row["Address"].ToString(),
                                row["City"].ToString(),
                                row["PostalCode"].ToString(),
                                row["Notes"].ToString()
                            );
                            imported++;
                        }
                        catch
                        {
                            skipped++;
                        }
                    }

                    LoadData();
                    string completeMsg = LocalizationManager.IsArabic 
                        ? $"اكتمل الاستيراد!\nتم استيراد: {imported}\nتم تخطي: {skipped}" 
                        : $"Import complete!\nImported: {imported}\nSkipped: {skipped}";
                    MessageHelper.ShowSuccess(completeMsg);
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError((LocalizationManager.IsArabic ? "خطأ في الاستيراد: " : "Import error: ") + ex.Message);
            }
        }

        private void ImportFromExcel()
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|All Files (*.*)|*.*";
                openDialog.Title = "Import Customers from Excel";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = Helpers.ImportExportHelper.ImportFromExcel(openDialog.FileName);
                    
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "لم يتم العثور على بيانات في الملف." : "No data found in the file.");
                        return;
                    }

                    if (!dt.Columns.Contains("CustomerName") || !dt.Columns.Contains("Email"))
                    {
                        MessageHelper.ShowError(LocalizationManager.IsArabic 
                            ? "تنسيق ملف غير صالح. الأعمدة المطلوبة: CustomerName, Email, Phone, Address, City, PostalCode, Notes" 
                            : "Invalid file format. Required columns: CustomerName, Email, Phone, Address, City, PostalCode, Notes");
                        return;
                    }

                    int imported = 0;
                    int skipped = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        try
                        {
                            string customerName = row["CustomerName"].ToString();
                            string email = row["Email"].ToString();
                            
                            if (string.IsNullOrWhiteSpace(customerName))
                            {
                                skipped++;
                                continue;
                            }

                            if (_customerService.CustomerExists(email))
                            {
                                skipped++;
                                continue;
                            }

                            _customerService.ImportCustomer(
                                customerName,
                                email,
                                row["Phone"].ToString(),
                                row["Address"].ToString(),
                                row["City"].ToString(),
                                row["PostalCode"].ToString(),
                                row["Notes"].ToString()
                            );
                            imported++;
                        }
                        catch
                        {
                            skipped++;
                        }
                    }

                    LoadData();
                    string completeMsg = LocalizationManager.IsArabic 
                        ? $"اكتمل الاستيراد!\nتم استيراد: {imported}\nتم تخطي: {skipped}" 
                        : $"Import complete!\nImported: {imported}\nSkipped: {skipped}";
                    MessageHelper.ShowSuccess(completeMsg);
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError((LocalizationManager.IsArabic ? "خطأ في الاستيراد: " : "Import error: ") + ex.Message);
            }
        }

        private void ApplyPermissions()
        {
            if (!GenericInventorySystem.Helpers.UserSession.IsAdmin)
            {
                var ctrlDel = this.Controls.Find("btnDeleteSelected", true);
                if (ctrlDel.Length > 0) ctrlDel[0].Visible = false;
                
                if (btnImport != null) btnImport.Visible = false;
            }
        }
    }
}
