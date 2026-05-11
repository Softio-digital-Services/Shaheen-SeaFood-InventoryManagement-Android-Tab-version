using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GenericInventorySystem.Data;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Services;
using GenericInventorySystem.Controls;

namespace GenericInventorySystem.Forms
{
    public partial class SuppliersForm : UserControl
    {
        private DataGridView dgvSuppliers;
        private Button btnAdd;
        private Button btnDetails;
        private Button btnImport;
        private Button btnExport;
        private Label lblSuppliersTitle;
        private ModernTextBox txtSearch;

        public SuppliersForm()
        {
            InitializeComponent();
            ApplyTheme();

            GenericInventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
            ApplyPermissions();
        }

        private void ApplyLocalization()
        {
            GenericInventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            GenericInventorySystem.Helpers.LocalizationManager.TranslateControl(this);
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;

            if (lblSuppliersTitle != null) lblSuppliersTitle.Text = L("Sup_Title");

            if (txtSearch != null)
            {
                txtSearch.PlaceholderText = L("Sup_Search");
            }

            if (btnAdd != null) btnAdd.Invalidate();
            if (btnDetails != null) btnDetails.Invalidate();
            if (btnImport != null) btnImport.Invalidate();
            if (btnExport != null) btnExport.Invalidate();

            var ctrlDel = this.Controls.Find("btnDeleteSelected", true);
            if (ctrlDel.Length > 0) ctrlDel[0].Invalidate();

            if (dgvSuppliers != null && dgvSuppliers.Columns.Count > 0)
            {
                if (dgvSuppliers.Columns.Contains("colCompany")) dgvSuppliers.Columns["colCompany"].HeaderText = L("Sup_GridCompany");
                if (dgvSuppliers.Columns.Contains("colContact")) dgvSuppliers.Columns["colContact"].HeaderText = L("Sup_GridContact");
                if (dgvSuppliers.Columns.Contains("colPhone")) dgvSuppliers.Columns["colPhone"].HeaderText = L("Sup_GridPhone");
                if (dgvSuppliers.Columns.Contains("colEmail")) dgvSuppliers.Columns["colEmail"].HeaderText = L("Sup_GridEmail");
                if (dgvSuppliers.Columns.Contains("colAddress")) dgvSuppliers.Columns["colAddress"].HeaderText = L("Cust_GridAddress") ?? "Address";
                if (dgvSuppliers.Columns.Contains("colBalance")) dgvSuppliers.Columns["colBalance"].HeaderText = L("Cust_GridBalance") ?? "Balance Due";
                if (dgvSuppliers.Columns.Contains("colActiveOrders")) dgvSuppliers.Columns["colActiveOrders"].HeaderText = L("Sup_GridActiveOrders");
                if (dgvSuppliers.Columns.Contains("colDueDate")) dgvSuppliers.Columns["colDueDate"].HeaderText = L("AddSup_DueDate") ?? "Due Date";
                if (dgvSuppliers.Columns.Contains("colActions")) dgvSuppliers.Columns["colActions"].HeaderText = L("Sup_GridActions");
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
            this.dgvSuppliers = new System.Windows.Forms.DataGridView();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDetails = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.lblSuppliersTitle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSuppliers)).BeginInit();
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
             this.lblSuppliersTitle = ThemeConfig.CreateStandardHeader("Supplier Management");
            this.lblSuppliersTitle.Name = "lblSuppliersTitle";
            tlpHeader.Controls.Add(this.lblSuppliersTitle, 0, 0);

            // Row 1: Actions (Search + Buttons)
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
            txtSearch.PlaceholderText = "Search Suppliers...";
            txtSearch.Size = new Size(320, 40);
            txtSearch.Anchor = AnchorStyles.Left;
            txtSearch.TextChanged += (s, e) => { 
                string ph = GenericInventorySystem.Helpers.LocalizationManager.GetString("Sup_Search");
                if (txtSearch.Text != ph && txtSearch.Text != "Search...") 
                    LoadData(txtSearch.Text); 
            };
            tlpActions.Controls.Add(txtSearch, 0, 0);

            // Actions Panel (FlowLayout for Buttons)
            FlowLayoutPanel panelButtons = new FlowLayoutPanel();
            panelButtons.FlowDirection = FlowDirection.LeftToRight;
            panelButtons.AutoSize = true;
            panelButtons.Anchor = AnchorStyles.Right; // Pins to far edge (mirrored in RTL)
            panelButtons.WrapContents = false;
            panelButtons.Padding = new Padding(0);
            panelButtons.Margin = new Padding(0);

            // Add New Supplier Button - Added first (RTL pin)
            this.btnAdd.Size = new System.Drawing.Size(160, 40);
            this.btnAdd.Text = "";
            this.btnAdd.Name = "btnAddSupplier";
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Cursor = Cursors.Hand;
            this.btnAdd.Margin = new Padding(0, 0, 10, 0);
            this.btnAdd.Click += BtnAdd_Click;
            this.btnAdd.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAdd, e.Graphics, "add", "Sup_AddSupplier", ThemeConfig.TextColorLight, ThemeConfig.PrimaryColor, false);
            panelButtons.Controls.Add(this.btnAdd);

            // Supplier Details Button
            this.btnDetails.Size = new System.Drawing.Size(160, 40);
            this.btnDetails.Text = "";
            this.btnDetails.Name = "btnDetailsSup";
            this.btnDetails.FlatStyle = FlatStyle.Flat;
            this.btnDetails.FlatAppearance.BorderSize = 0;
            this.btnDetails.BackColor = Color.Transparent;
            this.btnDetails.Cursor = Cursors.Hand;
            this.btnDetails.Margin = new Padding(0, 0, 10, 0);
            this.btnDetails.Click += BtnDetails_Click;
            this.btnDetails.Paint += (s, e) => ThemeConfig.DrawIconButton(btnDetails, e.Graphics, "view", "Sup_Details", ThemeConfig.TextColorLight, ThemeConfig.WarningColor, false);
            panelButtons.Controls.Add(this.btnDetails);

            // Delete Selected Button (Red Outline)
            Button btnDeleteSelected = new Button();
            btnDeleteSelected.Size = new Size(130, 40);
            btnDeleteSelected.Text = ""; 
            btnDeleteSelected.Name = "btnDeleteSelected";
            btnDeleteSelected.FlatStyle = FlatStyle.Flat;
            btnDeleteSelected.FlatAppearance.BorderSize = 0;
            btnDeleteSelected.Cursor = Cursors.Hand;
            btnDeleteSelected.Margin = new Padding(0, 0, 10, 0);
            btnDeleteSelected.Click += (s, e) =>
            {
                var checkedIds = new System.Collections.Generic.List<int>();
                foreach (DataGridViewRow row in dgvSuppliers.Rows)
                {
                    var chkCell = row.Cells["colCheck"] as DataGridViewCheckBoxCell;
                    if (chkCell != null && Convert.ToBoolean(chkCell.Value ?? false))
                    {
                        if (int.TryParse(row.Cells["ID"].Value?.ToString(), out int sId))
                            checkedIds.Add(sId);
                    }
                }
                if (checkedIds.Count == 0)
                {
                    MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_SelectOneSupplier"));
                    return;
                }
                string confirmMsg = LocalizationManager.IsArabic 
                    ? $"هل أنت متأكد من رغبتك في حذف {checkedIds.Count} من الموردين المحددين؟" 
                    : $"Are you sure you want to delete {checkedIds.Count} selected suppliers?";
                if (MessageHelper.ConfirmAction(confirmMsg))
                {
                    Services.SupplierService supplierService = new Services.SupplierService();
                    foreach(int i in checkedIds) supplierService.DeleteSupplier(i);
                    LoadData();
                }
            };
            btnDeleteSelected.Paint += (s, e) => ThemeConfig.DrawIconButton(btnDeleteSelected, e.Graphics, "delete", "Sup_Delete", ThemeConfig.DangerColor, ThemeConfig.DangerColor, true);
            panelButtons.Controls.Add(btnDeleteSelected);

            // Export Button
            this.btnExport.Size = new System.Drawing.Size(100, 40);
            this.btnExport.Text = "";
            this.btnExport.Name = "btnExportSup";
            this.btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Cursor = Cursors.Hand;
            this.btnExport.Margin = new Padding(0, 0, 10, 0);
            this.btnExport.Click += BtnExport_Click;
            this.btnExport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnExport, e.Graphics, "export", "Sup_Export", ThemeConfig.PrimaryColor, ThemeConfig.PrimaryColor, true);
            panelButtons.Controls.Add(this.btnExport);

            // Import Button
            this.btnImport.Size = new System.Drawing.Size(100, 40);
            this.btnImport.Text = "";
            this.btnImport.Name = "btnImportSup";
            this.btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.FlatAppearance.BorderSize = 0;
            btnImport.Cursor = Cursors.Hand;
            this.btnImport.Margin = new Padding(0, 0, 10, 0);
            this.btnImport.Click += BtnImport_Click;
            this.btnImport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnImport, e.Graphics, "import", "Sup_Import", ThemeConfig.SuccessBorder, ThemeConfig.SuccessBorder, true);
            panelButtons.Controls.Add(this.btnImport);

            tlpActions.Controls.Add(panelButtons, 1, 0);
            tlpHeader.Controls.Add(tlpActions, 0, 1);

            tlpMain.Controls.Add(tlpHeader, 0, 0);

            // Grid Config
            this.dgvSuppliers.Dock = DockStyle.Fill;
            this.dgvSuppliers.AllowUserToAddRows = false;
            this.dgvSuppliers.ReadOnly = false;
            this.dgvSuppliers.RowHeadersVisible = false;
            this.dgvSuppliers.BackgroundColor = ThemeConfig.SurfaceColor;
            this.dgvSuppliers.BorderStyle = BorderStyle.None;
            this.dgvSuppliers.AutoGenerateColumns = false;
            this.dgvSuppliers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvSuppliers.MultiSelect = false;

            dgvSuppliers.RowTemplate.Height = 60;
            dgvSuppliers.DefaultCellStyle.BackColor = ThemeConfig.SurfaceColor;
            dgvSuppliers.DefaultCellStyle.ForeColor = ThemeConfig.TextColorDark;
            dgvSuppliers.DefaultCellStyle.SelectionBackColor = ThemeConfig.ActiveBackColor;
            dgvSuppliers.DefaultCellStyle.SelectionForeColor = ThemeConfig.TextColorDark;
            dgvSuppliers.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvSuppliers.GridColor = ThemeConfig.BorderColor;

            ThemeConfig.ApplyGridTheme(dgvSuppliers);
            ThemeConfig.ApplyHeaderCheckBox(dgvSuppliers, "colCheck");

            dgvSuppliers.CellPainting += DgvSuppliers_CellPainting;
            dgvSuppliers.CellMouseDown += DgvSuppliers_CellMouseDown;
            dgvSuppliers.CellMouseMove += DgvSuppliers_CellMouseMove;
            dgvSuppliers.CellMouseLeave += DgvSuppliers_CellMouseLeave;
            dgvSuppliers.DataError += (s, e) =>
            {
                Console.WriteLine("DataError: " + (e.Exception != null ? e.Exception.Message : "Unknown"));
                e.ThrowException = false;
            };

            // Columns
            dgvSuppliers.Columns.Clear();
            dgvSuppliers.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colCheck", HeaderText = "", Width = 30, ReadOnly = false });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCompany", HeaderText = "Company Name", DataPropertyName = "supplier_name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPhone", HeaderText = "Phone", DataPropertyName = "phone", Width = 120, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmail", HeaderText = "Email", DataPropertyName = "email", Width = 200, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAddress", HeaderText = "Address", DataPropertyName = "address", Width = 200, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBalance", HeaderText = "Balance Due", DataPropertyName = "balance_due", Width = 120, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDueDate", HeaderText = "Due Date", DataPropertyName = "payment_due_date", Width = 100, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" } });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colActions", HeaderText = "Actions", Width = 100, ReadOnly = true });

            // Hidden Fields
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "id", Visible = false });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "reminder_days", DataPropertyName = "reminder_days", Visible = false });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colContact", DataPropertyName = "contact_person", Visible = false });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "type", DataPropertyName = "type", Visible = false });

            // Card Panel (Rounded body)
            Panel pnlCard = ThemeConfig.CreateCardPanel(dgvSuppliers);
            tlpMain.Controls.Add(tlpHeader, 0, 0);
            tlpMain.Controls.Add(pnlCard, 0, 1);

            this.Controls.Add(tlpMain);
            this.Size = new System.Drawing.Size(950, 600);

            ((System.ComponentModel.ISupportInitialize)(this.dgvSuppliers)).EndInit();
            this.ResumeLayout(false);
        }

        private void DgvSuppliers_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (dgvSuppliers.Columns[e.ColumnIndex].Name == "colActions")
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                // Edit Icon
                Rectangle editRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + 14, 32, 32);
                Image imgEdit = ThemeConfig.GetNuricon("edit");
                if (imgEdit != null) e.Graphics.DrawImage(imgEdit, editRect);

                // Delete Icon
                Rectangle delRect = new Rectangle(e.CellBounds.X + 48, e.CellBounds.Y + 14, 32, 32);
                Image imgDelete = ThemeConfig.GetNuricon("delete");
                if (imgDelete != null) e.Graphics.DrawImage(imgDelete, delRect);
            }
        }

        private void DgvSuppliers_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.Button != MouseButtons.Left) return;

            string colName = dgvSuppliers.Columns[e.ColumnIndex].Name;
            if (colName != "colActions") return;

            int id = Convert.ToInt32(dgvSuppliers.Rows[e.RowIndex].Cells["ID"].Value);

            if (e.X >= 5 && e.X <= 42) // Edit Rect (8, 14, 32, 32) + tolerance
            {
                // Edit
                if (!GenericInventorySystem.Helpers.UserSession.IsAdmin)
                {
                    MessageHelper.ShowWarning(GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "\u0644\u064a\u0633 \u0644\u062f\u064a\u0643 \u0635\u0644\u0627\u062d\u064a\u0629 \u0644\u062a\u0639\u062f\u064a\u0644 \u0627\u0644\u0645\u0648\u0631\u062f\u064a\u0646." : "You do not have permission to edit suppliers.");
                    return;
                }

                string name = dgvSuppliers.Rows[e.RowIndex].Cells["colCompany"].Value?.ToString() ?? "";
                string phone = dgvSuppliers.Rows[e.RowIndex].Cells["colPhone"].Value?.ToString() ?? "";
                string email = dgvSuppliers.Rows[e.RowIndex].Cells["colEmail"].Value?.ToString() ?? "";
                string contact = dgvSuppliers.Rows[e.RowIndex].Cells["colContact"].Value?.ToString() ?? "";
                string address = dgvSuppliers.Rows[e.RowIndex].Cells["colAddress"].Value?.ToString() ?? "";
                string type = dgvSuppliers.Rows[e.RowIndex].Cells["type"].Value?.ToString() ?? "Company";
                DateTime? dueDate = dgvSuppliers.Rows[e.RowIndex].Cells["colDueDate"].Value as DateTime?;

                object remVal = dgvSuppliers.Rows[e.RowIndex].Cells["reminder_days"].Value;
                int reminderDays = (remVal == null || remVal == DBNull.Value) ? 0 : Convert.ToInt32(remVal);

                AddSupplierForm form = new AddSupplierForm(id, name, phone, email, address, type, dueDate, reminderDays, contact);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    SupplierData.UpdateSupplier(id, form.SupplierName, form.Phone, form.Email, form.Address, form.SupplierType, form.ContactPerson, form.DueDate, form.ReminderDays);
                    LoadData();
                }
            }
            else if (e.X >= 45 && e.X <= 82) // Delete Rect (48, 14, 32, 32) + tolerance
            {
                // Delete
                if (!GenericInventorySystem.Helpers.UserSession.IsAdmin)
                {
                    MessageHelper.ShowWarning(GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "\u0644\u064a\u0633 \u0644\u062f\u064a\u0643 \u0635\u0644\u0627\u062d\u064a\u0629 \u0644\u062d\u0630\u0641 \u0627\u0644\u0645\u0648\u0631\u062f\u064a\u0646." : "You do not have permission to delete suppliers.");
                    return;
                }

                string deleteConfirm = LocalizationManager.GetString("Msg_ConfirmDelete");
                if (MessageHelper.ConfirmAction(deleteConfirm))
                {
                    SupplierData.DeleteSupplier(id);
                    LoadData();
                }
            }
        }

        private void DgvSuppliers_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvSuppliers.Columns[e.ColumnIndex].Name == "colActions")
            {
                dgvSuppliers.Cursor = Cursors.Hand;
            }
            else
            {
                dgvSuppliers.Cursor = Cursors.Default;
            }
        }

        private void DgvSuppliers_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dgvSuppliers.Cursor = Cursors.Default;
        }

        private void DrawRoundedButton(Graphics g, Rectangle rect, string text, Color bgColor, Color textColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, rect.Width - 1, rect.Height - 1);
            using (var path = GetRoundedRect(r, 8))
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillPath(brush, path);
            }
            TextRenderer.DrawText(g, text, ThemeConfig.ButtonFont, r, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void ApplyTheme()
        {

        }

        private void LoadData(string search = "")
        {
            try
            {
                string sql = "SELECT id, supplier_name, contact_person, phone, email, address, type, 0 as active_orders, payment_due_date, reminder_days FROM suppliers WHERE date_deleted IS NULL";

                if (!string.IsNullOrEmpty(search))
                {
                    sql += $" AND (supplier_name LIKE '%{search}%' OR phone LIKE '%{search}%' OR email LIKE '%{search}%' OR contact_person LIKE '%{search}%')";
                }

                sql += " ORDER BY supplier_name";

                DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
                dgvSuppliers.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(("Error loading data: ") + ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            AddSupplierForm form = new AddSupplierForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                SupplierData.AddSupplier(form.SupplierName, form.Phone, form.Email, form.Address, form.SupplierType, form.ContactPerson, form.DueDate, form.ReminderDays);
                LoadData();
            }
        }

        private void BtnDetails_Click(object sender, EventArgs e)
        {
            if (dgvSuppliers.SelectedRows.Count == 0) { MessageHelper.ShowInfo("Select a supplier."); return; }
            int id = Convert.ToInt32(dgvSuppliers.SelectedRows[0].Cells["ID"].Value);
            string name = dgvSuppliers.SelectedRows[0].Cells["colCompany"].Value.ToString();
            var form = new SupplierDetailsForm(id, name);
            form.ShowDialog();
            LoadData();
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
                saveDialog.FileName = $"Suppliers_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                saveDialog.Title = "Export Suppliers to CSV";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string sql = "SELECT supplier_name, 'Unknown' as contact_person, phone, email, address, '' as city, '' as postal_code, '' as website, '' as notes FROM suppliers WHERE date_deleted IS NULL ORDER BY supplier_name";
                    DataTable dt = DatabaseHelper.ExecuteDataTable(sql);

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoDataExport"));
                        return;
                    }

                    DataTable exportDt = new DataTable();
                    exportDt.Columns.Add("SupplierName");
                    exportDt.Columns.Add("ContactPerson");
                    exportDt.Columns.Add("Email");
                    exportDt.Columns.Add("Phone");
                    exportDt.Columns.Add("Address");
                    exportDt.Columns.Add("City");
                    exportDt.Columns.Add("PostalCode");
                    exportDt.Columns.Add("Website");
                    exportDt.Columns.Add("Notes");

                    foreach (DataRow row in dt.Rows)
                    {
                        exportDt.Rows.Add(
                            row["supplier_name"],
                            row["contact_person"],
                            row["email"],
                            row["phone"],
                            row["address"],
                            row["city"],
                            row["postal_code"],
                            row["website"],
                            row["notes"]
                        );
                    }

                    if (Helpers.ImportExportHelper.ExportToCsv(exportDt, saveDialog.FileName))
                    {
                        string successMsg = LocalizationManager.IsArabic
                            ? $"تم تصدير {exportDt.Rows.Count} موردين إلى CSV بنجاح!"
                            : $"Exported {exportDt.Rows.Count} suppliers to CSV successfully!";
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

        private void ExportToExcel()
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                saveDialog.FileName = $"Suppliers_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                saveDialog.Title = "Export Suppliers to Excel";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string sql = "SELECT supplier_name, 'Unknown' as contact_person, phone, email, address, '' as city, '' as postal_code, '' as website, '' as notes FROM suppliers WHERE date_deleted IS NULL ORDER BY supplier_name";
                    DataTable dt = DatabaseHelper.ExecuteDataTable(sql);

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoDataExport"));
                        return;
                    }

                    DataTable exportDt = new DataTable();
                    exportDt.Columns.Add("SupplierName");
                    exportDt.Columns.Add("ContactPerson");
                    exportDt.Columns.Add("Email");
                    exportDt.Columns.Add("Phone");
                    exportDt.Columns.Add("Address");
                    exportDt.Columns.Add("City");
                    exportDt.Columns.Add("PostalCode");
                    exportDt.Columns.Add("Website");
                    exportDt.Columns.Add("Notes");

                    foreach (DataRow row in dt.Rows)
                    {
                        exportDt.Rows.Add(
                            row["supplier_name"],
                            row["contact_person"],
                            row["email"],
                            row["phone"],
                            row["address"],
                            row["city"],
                            row["postal_code"],
                            row["website"],
                            row["notes"]
                        );
                    }

                    if (Helpers.ImportExportHelper.ExportToExcel(exportDt, saveDialog.FileName, "Suppliers"))
                    {
                        string successMsg = LocalizationManager.IsArabic
                            ? $"تم تصدير {exportDt.Rows.Count} موردين إلى Excel بنجاح!"
                            : $"Exported {exportDt.Rows.Count} suppliers to Excel successfully!";
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
                openDialog.Title = "Import Suppliers from CSV";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = Helpers.ImportExportHelper.ImportFromCsv(openDialog.FileName);

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoDataFile"));
                        return;
                    }

                    if (!dt.Columns.Contains("SupplierName"))
                    {
                        MessageHelper.ShowError(LocalizationManager.IsArabic
                            ? "تنسيق الملف غير صالح. الأعمدة المطلوبة: SupplierName, ContactPerson, Email, Phone, Address, City, PostalCode, Website, Notes"
                            : "Invalid file format. Required columns: SupplierName, ContactPerson, Email, Phone, Address, City, PostalCode, Website, Notes");
                        return;
                    }

                    int imported = 0;
                    int skipped = 0;
                    Services.SupplierService supplierService = new Services.SupplierService();

                    foreach (DataRow row in dt.Rows)
                    {
                        try
                        {
                            string supplierName = row["SupplierName"].ToString();

                            if (string.IsNullOrWhiteSpace(supplierName))
                            {
                                skipped++;
                                continue;
                            }

                            if (supplierService.SupplierExists(supplierName))
                            {
                                skipped++;
                                continue;
                            }

                            supplierService.ImportSupplier(
                                supplierName,
                                row["ContactPerson"].ToString(),
                                row["Email"].ToString(),
                                row["Phone"].ToString(),
                                row["Address"].ToString(),
                                row["City"].ToString(),
                                row["PostalCode"].ToString(),
                                row["Website"].ToString(),
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

        private void ImportFromExcel()
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|All Files (*.*)|*.*";
                openDialog.Title = "Import Suppliers from Excel";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = Helpers.ImportExportHelper.ImportFromExcel(openDialog.FileName);

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoDataFile"));
                        return;
                    }

                    if (!dt.Columns.Contains("SupplierName"))
                    {
                        MessageHelper.ShowError(LocalizationManager.IsArabic
                            ? "تنسيق الملف غير صالح. الأعمدة المطلوبة: SupplierName, ContactPerson, Email, Phone, Address, City, PostalCode, Website, Notes"
                            : "Invalid file format. Required columns: SupplierName, ContactPerson, Email, Phone, Address, City, PostalCode, Website, Notes");
                        return;
                    }

                    int imported = 0;
                    int skipped = 0;
                    Services.SupplierService supplierService = new Services.SupplierService();

                    foreach (DataRow row in dt.Rows)
                    {
                        try
                        {
                            string supplierName = row["SupplierName"].ToString();

                            if (string.IsNullOrWhiteSpace(supplierName))
                            {
                                skipped++;
                                continue;
                            }

                            if (supplierService.SupplierExists(supplierName))
                            {
                                skipped++;
                                continue;
                            }

                            supplierService.ImportSupplier(
                                supplierName,
                                row["ContactPerson"].ToString(),
                                row["Email"].ToString(),
                                row["Phone"].ToString(),
                                row["Address"].ToString(),
                                row["City"].ToString(),
                                row["PostalCode"].ToString(),
                                row["Website"].ToString(),
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
                        ? $"اكتمل الاستيراد!\nتم الاستيراد: {imported}\nتم التخطي: {skipped}"
                        : $"Import complete!\nImported: {imported}\nSkipped: {skipped}";
                    MessageHelper.ShowSuccess(completeMsg);
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Import error: {ex.Message}");
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
