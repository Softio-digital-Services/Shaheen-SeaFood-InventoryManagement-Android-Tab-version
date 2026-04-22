using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GenericInventorySystem.Data;

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

            var ctrlSearch = this.Controls.Find("txtSearch", true);
            if (ctrlSearch.Length > 0 && ctrlSearch[0] is TextBox txtSearch)
            {
                if (txtSearch.Text == "Search..." || txtSearch.Text == "\u0628\u062d\u062b...")
                {
                    txtSearch.Text = L("Sup_Search");
                }
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
                if (dgvSuppliers.Columns.Contains("colActiveOrders")) dgvSuppliers.Columns["colActiveOrders"].HeaderText = L("Sup_GridActiveOrders");
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
            tlpMain.BackColor = ThemeConfig.BackgroundColor;

            // Header Panel
            Panel panelTop = new Panel();
            panelTop.Dock = DockStyle.Fill;
            panelTop.BackColor = ThemeConfig.BackgroundColor;
            panelTop.Padding = new Padding(20);

            // lblSuppliersTitle
            this.lblSuppliersTitle = ThemeConfig.CreateStandardHeader("Supplier Management");
            this.lblSuppliersTitle.Name = "lblSuppliersTitle";

            // Search Bar
            Panel searchPanel = new Panel();
            searchPanel.Location = new Point(20, 75);
            searchPanel.Size = new Size(320, 35); 
            searchPanel.BackColor = Color.White;
            searchPanel.Padding = new Padding(0);

            TextBox txtSearch = new TextBox();
            txtSearch.Name = "txtSearch";
            txtSearch.Location = new Point(40, 8);
            txtSearch.Size = new Size(260, 20);
            txtSearch.Font = ThemeConfig.StandardFont;
            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.Text = "Search...";
            txtSearch.ForeColor = ThemeConfig.SecondaryColor;

            searchPanel.Paint += (s, e) => 
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, searchPanel.Width - 1, searchPanel.Height - 1);
                using (GraphicsPath path = GetRoundedRect(r, 8)) 
                using (Pen pen = new Pen(ThemeConfig.BorderColor, 1.5f)) 
                {
                    e.Graphics.DrawPath(pen, path);
                }
                Image imgSearch = ThemeConfig.GetNuricon("search");
                if (imgSearch != null) e.Graphics.DrawImage(imgSearch, new Rectangle(10, 8, 20, 20));
            };

            txtSearch.Enter += (s, e) => { string ph = Properties.Resources.Sup_Search; if (txtSearch.Text == ph) { txtSearch.Text = ""; txtSearch.ForeColor = ThemeConfig.TextColorDark; } };
            txtSearch.Leave += (s, e) => { string ph = Properties.Resources.Sup_Search; if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = ph; txtSearch.ForeColor = ThemeConfig.SecondaryColor; } };
            txtSearch.TextChanged += (s, e) => { string ph = Properties.Resources.Sup_Search; if (txtSearch.Text != ph && txtSearch.Text != "Search..." && txtSearch.Text != "\u0628\u062d\u062b...") LoadData(txtSearch.Text); };
            searchPanel.Controls.Add(txtSearch);

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
            this.btnImport.Name = "btnImportSup";
            this.btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.FlatAppearance.BorderSize = 0;
            btnImport.BackColor = ThemeConfig.SurfaceColor;
            btnImport.Cursor = Cursors.Hand;
            this.btnImport.Margin = new Padding(0, 0, 10, 0);
            this.btnImport.Click += BtnImport_Click;
            this.btnImport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnImport, e.Graphics, "import", "Sup_Import", ThemeConfig.SuccessBorder, ThemeConfig.SuccessBorder, true);

            // Export Button (Blue Outline)
            this.btnExport.Size = new System.Drawing.Size(100, 40);
            this.btnExport.Text = "";
            this.btnExport.Name = "btnExportSup";
            this.btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.BackColor = ThemeConfig.SurfaceColor;
            btnExport.Cursor = Cursors.Hand;
            this.btnExport.Margin = new Padding(0, 0, 10, 0);
            this.btnExport.Click += BtnExport_Click;
            this.btnExport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnExport, e.Graphics, "export", "Sup_Export", ThemeConfig.PrimaryColor, ThemeConfig.PrimaryColor, true);

            // Supplier Details Button
            this.btnDetails = new Button();
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

            // Add New Supplier
            this.btnAdd.Size = new System.Drawing.Size(160, 40);
            this.btnAdd.Text = "";
            this.btnAdd.Name = "btnAddSupplier";
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.BackColor = ThemeConfig.SurfaceColor;
            btnAdd.Cursor = Cursors.Hand;
            this.btnAdd.Margin = new Padding(0);
            this.btnAdd.Click += BtnAdd_Click;
            this.btnAdd.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAdd, e.Graphics, "add", "Sup_AddSupplier", ThemeConfig.TextColorLight, ThemeConfig.PrimaryColor, false);

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
                foreach (DataGridViewRow row in dgvSuppliers.Rows)
                {
                    var chkCell = row.Cells["colCheck"] as DataGridViewCheckBoxCell;
                    if (chkCell != null && Convert.ToBoolean(chkCell.Value ?? false))
                    {
                        if (int.TryParse(row.Cells["supplier_id"].Value?.ToString(), out int sId))
                            checkedIds.Add(sId);
                    }
                }
                if (checkedIds.Count == 0)
                {
                    MessageHelper.ShowWarning("Please select at least one supplier to delete.");
                    return;
                }
                if (MessageHelper.ConfirmAction($"Are you sure you want to delete {checkedIds.Count} selected suppliers?"))
                {
                    Services.SupplierService supplierService = new Services.SupplierService();
                    foreach(int i in checkedIds) supplierService.DeleteSupplier(i);
                    MessageHelper.ShowSuccess($"{checkedIds.Count} suppliers deleted successfully.");
                    LoadData();
                }
            };
            btnDeleteSelected.Paint += (s, e) => ThemeConfig.DrawIconButton(btnDeleteSelected, e.Graphics, "delete", "Sup_Delete", ThemeConfig.DangerColor, ThemeConfig.DangerColor, true);

            panelButtons.Controls.Add(this.btnImport);
            panelButtons.Controls.Add(this.btnExport);
            panelButtons.Controls.Add(btnDeleteSelected);
            panelButtons.Controls.Add(this.btnDetails);
            panelButtons.Controls.Add(this.btnAdd);

            panelTop.Controls.Add(this.lblSuppliersTitle);
            panelTop.Controls.Add(searchPanel);
            panelTop.Controls.Add(panelButtons);

            panelTop.Resize += (s, e) =>
            {
                if (GenericInventorySystem.Helpers.LocalizationManager.IsArabic)
                {
                    searchPanel.Location = new Point(panelTop.Width - searchPanel.Width - 20, 75);
                    panelButtons.Location = new Point(20, 70);
                }
                else
                {
                    searchPanel.Location = new Point(20, 75);
                    panelButtons.Location = new Point(panelTop.Width - panelButtons.Width - 20, 70);
                }
            };

            // Grid Panel
            Panel panelGrid = new Panel();
            panelGrid.Dock = DockStyle.Fill;
            panelGrid.Padding = new Padding(20, 10, 20, 20);
            panelGrid.BackColor = ThemeConfig.BackgroundColor;

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

            // Events
            dgvSuppliers.CellPainting += DgvSuppliers_CellPainting;
            dgvSuppliers.CellContentClick += DgvSuppliers_CellContentClick;
            dgvSuppliers.CellMouseMove += DgvSuppliers_CellMouseMove;
            dgvSuppliers.CellMouseLeave += DgvSuppliers_CellMouseLeave;
            dgvSuppliers.DataError += (s, e) => {
                Console.WriteLine("DataError: " + (e.Exception != null ? e.Exception.Message : "Unknown"));
                e.ThrowException = false;
            };

            // Columns
            dgvSuppliers.Columns.Clear();
            dgvSuppliers.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colCheck", HeaderText = "", Width = 30, ReadOnly = false });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCompany", HeaderText = "Company Name", DataPropertyName = "supplier_name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colContact", HeaderText = "Contact Person", DataPropertyName = "contact_person", Width = 150, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPhone", HeaderText = "Phone", DataPropertyName = "phone", Width = 120, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmail", HeaderText = "Email", DataPropertyName = "email", Width = 200, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colActiveOrders", HeaderText = "Active Orders", DataPropertyName = "active_orders", Width = 100, ReadOnly = true });
            dgvSuppliers.Columns.Add(new DataGridViewButtonColumn { Name = "colActions", HeaderText = "Actions", Width = 100, ReadOnly = true });

            // Hidden ID
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "id", Visible = false });

            panelGrid.Controls.Add(this.dgvSuppliers);

            tlpMain.Controls.Add(panelTop, 0, 0);
            tlpMain.Controls.Add(panelGrid, 0, 1);

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

        private void DgvSuppliers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
             if (e.RowIndex < 0) return;
            
            string colName = dgvSuppliers.Columns[e.ColumnIndex].Name;
            if (colName != "colActions") return;

            int id = Convert.ToInt32(dgvSuppliers.Rows[e.RowIndex].Cells["ID"].Value);
            
            Point cur = dgvSuppliers.PointToClient(Cursor.Position);
            Rectangle cellBounds = dgvSuppliers.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
            int relX = cur.X - cellBounds.X;

            if (relX >= 5 && relX <= 37) // Edit Rect (5, 14, 32, 32)
            {
                // Edit
                if (!GenericInventorySystem.Helpers.UserSession.IsAdmin)
                {
                    MessageHelper.ShowWarning(GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "\u0644\u064a\u0633 \u0644\u062f\u064a\u0643 \u0635\u0644\u0627\u062d\u064a\u0629 \u0644\u062a\u0639\u062f\u064a\u0644 \u0627\u0644\u0645\u0648\u0631\u062f\u064a\u0646." : "You do not have permission to edit suppliers.");
                    return;
                }

                string name = dgvSuppliers.Rows[e.RowIndex].Cells["colCompany"].Value.ToString();
                string phone = dgvSuppliers.Rows[e.RowIndex].Cells["colPhone"].Value.ToString();
                string email = dgvSuppliers.Rows[e.RowIndex].Cells["colEmail"].Value.ToString();
                string contact = dgvSuppliers.Rows[e.RowIndex].Cells["colContact"].Value?.ToString() ?? "";
                AddSupplierForm form = new AddSupplierForm(id, name, phone, email, "", "Company", contact);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    SupplierData.UpdateSupplier(id, form.SupplierName, form.Phone, form.Email, form.Address, form.SupplierType, form.ContactPerson);
                    LoadData();
                }
            }
            else if (relX >= 45 && relX <= 77) // Delete Rect (45, 14, 32, 32)
            {
                // Delete
                if (!GenericInventorySystem.Helpers.UserSession.IsAdmin)
                {
                    MessageHelper.ShowWarning(GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "\u0644\u064a\u0633 \u0644\u062f\u064a\u0643 \u0635\u0644\u0627\u062d\u064a\u0629 \u0644\u062d\u0630\u0641 \u0627\u0644\u0645\u0648\u0631\u062f\u064a\u0646." : "You do not have permission to delete suppliers.");
                    return;
                }

                if (MessageHelper.ShowConfirm("Are you sure you want to delete this supplier?"))
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
            this.BackColor = ThemeConfig.BackgroundColor;
        }

        private void LoadData(string search = "")
        {
            try
            {
                string sql = "SELECT id as ID, supplier_name, contact_person, phone, email, 0 as active_orders FROM suppliers WHERE date_deleted IS NULL";
                
                if (!string.IsNullOrEmpty(search))
                {
                    sql += $" AND (supplier_name LIKE '%{search}%' OR phone LIKE '%{search}%' OR email LIKE '%{search}%' OR contact_person LIKE '%{search}%')";
                }
                
                sql += " ORDER BY supplier_name";
                
                DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
                dgvSuppliers.DataSource = dt;
            }
            catch(Exception ex) 
            { 
                 MessageHelper.ShowError("Error loading data: " + ex.Message); 
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            AddSupplierForm form = new AddSupplierForm();
            if(form.ShowDialog() == DialogResult.OK)
            {
                SupplierData.AddSupplier(form.SupplierName, form.Phone, form.Email, form.Address, form.SupplierType, form.ContactPerson);
                LoadData();
            }
        }

        private void BtnDetails_Click(object sender, EventArgs e)
        {
            if(dgvSuppliers.SelectedRows.Count == 0) { MessageHelper.ShowInfo("Select a supplier."); return; }
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
                        MessageHelper.ShowWarning("No data to export.");
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
                        MessageHelper.ShowSuccess($"Exported {exportDt.Rows.Count} suppliers to CSV successfully!");
                    }
                    else
                    {
                        MessageHelper.ShowError("Failed to export data.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Export error: {ex.Message}");
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
                        MessageHelper.ShowWarning("No data to export.");
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
                        MessageHelper.ShowSuccess($"Exported {exportDt.Rows.Count} suppliers to Excel successfully!");
                    }
                    else
                    {
                        MessageHelper.ShowError("Failed to export data.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Export error: {ex.Message}");
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
                        MessageHelper.ShowWarning("No data found in the file.");
                        return;
                    }

                    if (!dt.Columns.Contains("SupplierName"))
                    {
                        MessageHelper.ShowError("Invalid file format. Required columns: SupplierName, ContactPerson, Email, Phone, Address, City, PostalCode, Website, Notes");
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
                    MessageHelper.ShowSuccess($"Import complete!\nImported: {imported}\nSkipped: {skipped}");
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Import error: {ex.Message}");
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
                        MessageHelper.ShowWarning("No data found in the file.");
                        return;
                    }

                    if (!dt.Columns.Contains("SupplierName"))
                    {
                        MessageHelper.ShowError("Invalid file format. Required columns: SupplierName, ContactPerson, Email, Phone, Address, City, PostalCode, Website, Notes");
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
                    MessageHelper.ShowSuccess($"Import complete!\nImported: {imported}\nSkipped: {skipped}");
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
