using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GenericInventorySystem.Data;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Services;

namespace GenericInventorySystem.Forms
{
    public partial class HistoryForm : UserControl
    {
        // UI Controls
        private Label lblHistoryTitle;
        private Panel pnlTabs;
        private Button btnTabInventory;
        private Button btnTabCustomers;
        private Button btnTabSuppliers;
        private Button btnTabOrders; 
        private Button btnTabQuotations; // NEW
        private Panel pnlIndicator; // Slide indicator
        
        private Panel pnlContent;
        private Panel pnlInventoryCard;
        private Panel pnlCustomersCard;
        private Panel pnlSuppliersCard;
        private Panel pnlOrdersCard; 
        private Panel pnlQuotationsCard; // NEW
        
        private DataGridView dgvInventory;
        private DataGridView dgvCustomers;
        private DataGridView dgvOrders; 
        private DataGridView dgvSuppliers;
        private DataGridView dgvQuotations; // NEW

        private ModernTextBox txtSearch; 
        
        // Stats
        private StatCard cardActions;
        private StatCard cardOrders;
        private StatCard cardPayments;
        
        private HistoryService _historyService;

        public HistoryForm()
        {
            InitializeComponent();
            _historyService = new HistoryService(); // Ideally injected
            ApplyTheme();
            
            GenericInventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();

            // Default Tab
            SwitchTab(btnTabInventory);
            LoadHistory();
        }

        private void ApplyLocalization()
        {
            GenericInventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;

            if (lblHistoryTitle != null) lblHistoryTitle.Text = L("Hist_Title");

            var ctrlRefresh = this.Controls.Find("btnRefresh", true);
            if (ctrlRefresh.Length > 0 && ctrlRefresh[0] is Button btnRefresh)
                btnRefresh.Text = L("Hist_Refresh");

            if (btnTabInventory != null) btnTabInventory.Text = L("Hist_TabInventory");
            if (btnTabCustomers != null) btnTabCustomers.Text = L("Hist_TabCustomers");
            if (btnTabSuppliers != null) btnTabSuppliers.Text = L("Hist_TabSuppliers");
            if (btnTabOrders != null) btnTabOrders.Text = L("Hist_TabOrders");
            if (btnTabQuotations != null) btnTabQuotations.Text = L("Hist_TabQuotations");

            if (txtSearch != null) 
            {
                txtSearch.PlaceholderText = L("Hist_Search");
            }

            if (cardActions != null) cardActions.Title = L("Hist_StatActivity");
            if (cardOrders != null) cardOrders.Title = L("Hist_StatOrders");
            if (cardPayments != null) cardPayments.Title = L("Hist_StatPayments");

            ApplyColumnHeaders();
        }

        private void ApplyColumnHeaders()
        {
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;

            foreach (DataGridViewColumn col in dgvInventory.Columns) {
                if (col.Name == "Date") col.HeaderText = L("Hist_ColDate");
                if (col.Name == "Action") col.HeaderText = L("Hist_ColAction");
                if (col.Name == "Item") col.HeaderText = L("Hist_ColItem");
                if (col.Name == "Details") col.HeaderText = L("Hist_ColDetails");
                if (col.Name == "User") col.HeaderText = L("Hist_ColUser");
            }
            foreach (DataGridViewColumn col in dgvCustomers.Columns) {
                if (col.Name == "Date") col.HeaderText = L("Hist_ColDate");
                if (col.Name == "Type") col.HeaderText = L("Hist_ColType");
                if (col.Name == "Customer") col.HeaderText = L("Hist_ColCustomer");
                if (col.Name == "Amount") col.HeaderText = L("Hist_ColAmount");
                if (col.Name == "Details") col.HeaderText = L("Hist_ColDetails");
            }
            foreach (DataGridViewColumn col in dgvQuotations.Columns) {
                if (col.Name == "ID") col.HeaderText = L("Hist_ColOrderID");
                if (col.Name == "Date") col.HeaderText = L("Hist_ColDate");
                if (col.Name == "Customer") col.HeaderText = L("Hist_ColCustomer");
                if (col.Name == "Total") col.HeaderText = L("Hist_ColTotal");
                if (col.Name == "Items") col.HeaderText = L("Hist_ColItems");
            }
            foreach (DataGridViewColumn col in dgvOrders.Columns) {
                if (col.Name == "Order ID") col.HeaderText = L("Hist_ColOrderID");
                if (col.Name == "Date") col.HeaderText = L("Hist_ColDate");
                if (col.Name == "Customer") col.HeaderText = L("Hist_ColCustomer");
                if (col.Name == "Total") col.HeaderText = L("Hist_ColTotal");
                if (col.Name == "Status") col.HeaderText = L("Hist_ColStatus") == "Hist_ColStatus" && GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "\u0627\u0644\u062D\u0627\u0644\u0629" : L("Hist_ColStatus") != "Hist_ColStatus" ? L("Hist_ColStatus") : "Status";
                if (col.Name == "Items") col.HeaderText = L("Hist_ColItems");
                if (col.Name == "colReturn") col.HeaderText = "";
            }
            foreach (DataGridViewColumn col in dgvSuppliers.Columns) {
                if (col.Name == "Date") col.HeaderText = L("Hist_ColDate");
                if (col.Name == "Type") col.HeaderText = L("Hist_ColType");
                if (col.Name == "Supplier") col.HeaderText = L("Hist_ColSupplier");
                if (col.Name == "Amount") col.HeaderText = L("Hist_ColAmount");
                if (col.Name == "Details") col.HeaderText = L("Hist_ColDetails");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Size = new Size(1100, 750);
            this.BackColor = ThemeConfig.BackgroundColor;

            // Main Layout
            // Container
            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(20), BackColor = ThemeConfig.BackgroundColor };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // 0. Title
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // 1. Actions (Search/Refresh)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F)); // 2. Stats
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // 3. Tabs
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // 4. Content
            this.Controls.Add(mainLayout);

            // 0. Title
            lblHistoryTitle = ThemeConfig.CreateStandardHeader("System History Logs");
            lblHistoryTitle.Name = "lblHistoryTitle";
            lblHistoryTitle.Margin = new Padding(0);
            mainLayout.Controls.Add(lblHistoryTitle, 0, 0);

            // 1. Actions Row (Search + Refresh)
            Panel pnlActions = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            
            txtSearch = new ModernTextBox();
            txtSearch.IsSearch = true;
            txtSearch.ShowLabel = false;
            txtSearch.PlaceholderText = "Search history...";
            txtSearch.Size = new Size(320, 40);
            txtSearch.Location = new Point(0, 5); 
            txtSearch.TextChanged += (s, e) => ApplyFilter();
            pnlActions.Controls.Add(txtSearch);

            Button btnRefresh = new Button();
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Text = "Refresh";
            btnRefresh.Size = new Size(120, 40);
            btnRefresh.Dock = DockStyle.Right;
            ThemeConfig.ApplySecondaryButton(btnRefresh);
            btnRefresh.Click += (s, e) => LoadHistory();
            pnlActions.Controls.Add(btnRefresh);

            mainLayout.Controls.Add(pnlActions, 0, 1);

            // 2. Stats Panel
            TableLayoutPanel tlpStats = new TableLayoutPanel();
            tlpStats.Dock = DockStyle.Fill;
            tlpStats.ColumnCount = 3;
            tlpStats.RowCount = 1;
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlpStats.Margin = new Padding(0, 0, 0, 10);
            
            cardActions = new StatCard { Title = "Activity Today", Value = "0", IconImage = ThemeConfig.GetNuricon("dashboard"), ThemeColor = ThemeConfig.PrimaryColor, Dock = DockStyle.Fill };
            cardOrders = new StatCard { Title = "Orders Today", Value = "0", IconImage = ThemeConfig.GetNuricon("orders"), ThemeColor = ThemeConfig.SuccessColor, Dock = DockStyle.Fill };
            cardPayments = new StatCard { Title = "Payments Today", Value = "0", IconImage = ThemeConfig.GetNuricon("revenue"), ThemeColor = ThemeConfig.WarningColor, Dock = DockStyle.Fill };
            
            tlpStats.Controls.Add(cardActions, 0, 0);
            tlpStats.Controls.Add(cardOrders, 1, 0);
            tlpStats.Controls.Add(cardPayments, 2, 0);
            
            mainLayout.Controls.Add(tlpStats, 0, 2);

            // 3. Custom Tabs
            pnlTabs = new Panel();
            pnlTabs.Dock = DockStyle.Fill;
            pnlTabs.Height = 50;
            pnlTabs.Margin = new Padding(0);
            
            pnlIndicator = new Panel { Height = 3, BackColor = ThemeConfig.PrimaryColor, Top = 40, Visible = false };
            pnlTabs.Controls.Add(pnlIndicator);

            btnTabInventory = CreateTabButton("Inventory Logs", 0);
            btnTabCustomers = CreateTabButton("Customer History", 150);
            btnTabSuppliers = CreateTabButton("Supplier History", 300);
            btnTabOrders = CreateTabButton("Orders History", 450); 
            btnTabQuotations = CreateTabButton("Quotation History", 600); // NEW

            pnlTabs.Controls.Add(btnTabInventory);
            pnlTabs.Controls.Add(btnTabCustomers);
            pnlTabs.Controls.Add(btnTabSuppliers);
            pnlTabs.Controls.Add(btnTabOrders);
            pnlTabs.Controls.Add(btnTabQuotations);
            
            mainLayout.Controls.Add(pnlTabs, 0, 3);

            // 4. Content Area
            pnlContent = new Panel();
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Margin = new Padding(0, 5, 0, 0);
            mainLayout.Controls.Add(pnlContent, 0, 4);
            
            // Grids
            dgvInventory = CreateGrid();
            dgvCustomers = CreateGrid();
            dgvSuppliers = CreateGrid();
            dgvOrders = CreateGrid(); 
            dgvOrders.CellFormatting += DgvOrders_CellFormatting;
            dgvOrders.CellContentClick += DgvOrders_CellContentClick;
            dgvOrders.CellPainting += DgvOrders_CellPainting;

            dgvQuotations = CreateGrid(); // NEW
            
            // Card Wrappers
            pnlInventoryCard = ThemeConfig.CreateCardPanel(dgvInventory);
            pnlCustomersCard = ThemeConfig.CreateCardPanel(dgvCustomers);
            pnlSuppliersCard = ThemeConfig.CreateCardPanel(dgvSuppliers);
            pnlOrdersCard = ThemeConfig.CreateCardPanel(dgvOrders); 
            pnlQuotationsCard = ThemeConfig.CreateCardPanel(dgvQuotations); // NEW
            
            pnlInventoryCard.Visible = false;
            pnlCustomersCard.Visible = false;
            pnlSuppliersCard.Visible = false;
            pnlOrdersCard.Visible = false;
            pnlQuotationsCard.Visible = false;
            
            pnlContent.Controls.Add(pnlInventoryCard);
            pnlContent.Controls.Add(pnlCustomersCard);
            pnlContent.Controls.Add(pnlSuppliersCard);
            pnlContent.Controls.Add(pnlOrdersCard);
            pnlContent.Controls.Add(pnlQuotationsCard);
            
            mainLayout.Controls.Add(pnlContent, 0, 4);

            this.ResumeLayout(false);
        }

        private void DgvOrders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvOrders.Columns[e.ColumnIndex].Name == "colReturn")
            {
                int orderId = Convert.ToInt32(dgvOrders.Rows[e.RowIndex].Cells["Order ID"].Value);
                string status = dgvOrders.Rows[e.RowIndex].Cells["Status"].Value.ToString();
                
                if (status == "Quotation" || status == "Draft")
                {
                    MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "يمكن إرجاع الطلبات المكتملة فقط." : "Only completed orders can be returned.");
                    return;
                }

                ReturnEntryForm form = new ReturnEntryForm(orderId);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadHistory();
                }
            }
        }

        private void DgvOrders_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvOrders.Columns[e.ColumnIndex].Name == "colReturn")
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);
                
                // Draw return icon (reusing revenue or history icon or similar)
                Image img = ThemeConfig.GetNuricon("history"); 
                if (img != null)
                {
                    int size = 20;
                    Rectangle rect = new Rectangle(e.CellBounds.X + (e.CellBounds.Width - size) / 2, e.CellBounds.Y + (e.CellBounds.Height - size) / 2, size, size);
                    e.Graphics.DrawImage(img, rect);
                }
            }
        }

        private void DgvOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                var col = dgvOrders.Columns[e.ColumnIndex];
                if (col.Name == "Status" && e.Value != null)
                {
                    string statusStr = e.Value.ToString();
                    if (statusStr == "Completed" && GenericInventorySystem.Helpers.LocalizationManager.IsArabic)
                    {
                        e.Value = "\u0645\u0643\u062A\u0645\u0644";
                        e.FormattingApplied = true;
                    }
                }
            }
        }

        private Button CreateTabButton(string text, int x)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = ThemeConfig.SubHeaderFont;
            btn.ForeColor = ThemeConfig.SecondaryColor;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = Color.Transparent;

            btn.SetBounds(x, 0, 150, 40);
            btn.Cursor = Cursors.Hand;
            btn.Click += (s, e) => SwitchTab(btn);
            return btn;
        }

        private DataGridView CreateGrid()
        {
            DataGridView dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = ThemeConfig.SurfaceColor;

            dgv.BorderStyle = BorderStyle.None;
            dgv.RowHeadersVisible = false;
            ThemeConfig.ApplyGridTheme(dgv);
            return dgv;
        }


        private void SwitchTab(Button clickedBtn)
        {
            // Reset Styles
            btnTabInventory.ForeColor = ThemeConfig.SecondaryColor;
            btnTabCustomers.ForeColor = ThemeConfig.SecondaryColor;
            btnTabSuppliers.ForeColor = ThemeConfig.SecondaryColor;
            btnTabOrders.ForeColor = ThemeConfig.SecondaryColor; 
            btnTabQuotations.ForeColor = ThemeConfig.SecondaryColor; // NEW


            // Set Active
            clickedBtn.ForeColor = ThemeConfig.PrimaryColor;
            
            // Move Indicator
            pnlIndicator.Visible = true;
            pnlIndicator.Width = clickedBtn.Width - 40;
            pnlIndicator.Left = clickedBtn.Left + 20;
            pnlIndicator.Top = clickedBtn.Bottom - 3;
            pnlIndicator.BringToFront();

            // Show Content
            pnlInventoryCard.Visible = false;
            pnlCustomersCard.Visible = false;
            pnlSuppliersCard.Visible = false;
            pnlOrdersCard.Visible = false;
            pnlQuotationsCard.Visible = false; // NEW

            if (clickedBtn == btnTabInventory) pnlInventoryCard.Visible = true;
            else if (clickedBtn == btnTabCustomers) pnlCustomersCard.Visible = true;
            else if (clickedBtn == btnTabSuppliers) pnlSuppliersCard.Visible = true;
            else if (clickedBtn == btnTabOrders) pnlOrdersCard.Visible = true;
            else if (clickedBtn == btnTabQuotations) pnlQuotationsCard.Visible = true; // NEW

            txtSearch.Text = ""; // Clear filter on tab change
            ApplyFilter();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeConfig.BackgroundColor;
        }

        public void LoadHistory()
        {
             try
             {
                 // Load Grids
                 dgvInventory.DataSource = _historyService.GetInventoryLogs();
                 dgvCustomers.DataSource = _historyService.GetCustomerHistory();
                 dgvSuppliers.DataSource = _historyService.GetSupplierHistory();
                 dgvOrders.DataSource = _historyService.GetOrderHistory(); 
                 if (!dgvOrders.Columns.Contains("colReturn"))
                 {
                     DataGridViewButtonColumn btnReturn = new DataGridViewButtonColumn
                     {
                         Name = "colReturn",
                         HeaderText = "",
                         Width = 50,
                         FlatStyle = FlatStyle.Flat
                     };
                     dgvOrders.Columns.Add(btnReturn);
                 }
                 
                 dgvQuotations.DataSource = _historyService.GetQuotationHistory(); // NEW
                 
                 // Load Stats
                 var stats = _historyService.GetTodayStats();
                 cardActions.Value = stats.actions.ToString();
                 cardOrders.Value = stats.orders.ToString();
                 cardPayments.Value = stats.payments.ToString();
                 
                 ApplyColumnHeaders();
                 ApplyFilter(); // Ensure filter applies if data reloads
             }
             catch (Exception ex)
             {
                 MessageHelper.ShowError((LocalizationManager.IsArabic ? "خطأ في تحميل السجل: " : "Error loading history: ") + ex.Message);
             }
        }

        private void ApplyFilter()
        {
             if (txtSearch == null) return;
             string ph = LocalizationManager.GetString("Hist_Search");
             string filterText = txtSearch.Text.Trim();
             
             if (filterText == ph || filterText == "Search..." || filterText == LocalizationManager.GetString("Hist_Search")) 
                 filterText = "";

             filterText = filterText.Replace("'", "''");
             
             DataGridView activeDgv = null;

             if (pnlInventoryCard != null && pnlInventoryCard.Visible) activeDgv = dgvInventory;
             else if (pnlCustomersCard != null && pnlCustomersCard.Visible) activeDgv = dgvCustomers;
             else if (pnlSuppliersCard != null && pnlSuppliersCard.Visible) activeDgv = dgvSuppliers;
             else if (pnlOrdersCard != null && pnlOrdersCard.Visible) activeDgv = dgvOrders;
             else if (pnlQuotationsCard != null && pnlQuotationsCard.Visible) activeDgv = dgvQuotations;

             if (activeDgv == null || activeDgv.DataSource == null) return;

             DataTable dt = activeDgv.DataSource as DataTable;
             if (dt == null) return;

             if (string.IsNullOrWhiteSpace(filterText))
             {
                 dt.DefaultView.RowFilter = "";
                 return;
             }

             // Build a generic RowFilter across all columns
             System.Text.StringBuilder filterBuilder = new System.Text.StringBuilder();
             bool first = true;
             foreach (DataColumn col in dt.Columns)
             {
                 if (col.DataType == typeof(string))
                 {
                     if (!first) filterBuilder.Append(" OR ");
                     filterBuilder.AppendFormat("[{0}] LIKE '%{1}%'", col.ColumnName, filterText);
                     first = false;
                 }
                 else if (col.DataType == typeof(int) || col.DataType == typeof(decimal))
                 {
                     if (!first) filterBuilder.Append(" OR ");
                     filterBuilder.AppendFormat("Convert([{0}], 'System.String') LIKE '%{1}%'", col.ColumnName, filterText);
                     first = false;
                 }
             }

             try
             {
                 dt.DefaultView.RowFilter = filterBuilder.ToString();
             }
             catch { /* Ignore invalid filter strings */ }
        }
        
    }
}

