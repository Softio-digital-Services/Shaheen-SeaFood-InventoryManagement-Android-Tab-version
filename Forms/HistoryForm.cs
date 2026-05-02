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
        
        // Animation
        private System.Windows.Forms.Timer _refreshTimer;
        private float _refreshAngle = 0;
        private bool _isRefreshing = false;
        
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
                if (col.Name == "Status") col.HeaderText = L("Hist_ColStatus");
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

            Panel pnlRefreshWrapper = new Panel { Dock = DockStyle.Right, Width = 140, Padding = new Padding(5, 5, 0, 15) };
            Button btnRefresh = new Button();
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Text = "Refresh";
            btnRefresh.Dock = DockStyle.Fill;
            ThemeConfig.ApplySecondaryButton(btnRefresh);
            
            // Animation Timer
            _refreshTimer = new System.Windows.Forms.Timer { Interval = 30 }; // ~33 FPS
            _refreshTimer.Tick += (s, e) => {
                _refreshAngle = (_refreshAngle + 15) % 360;
                btnRefresh.Invalidate();
            };

            btnRefresh.Paint += (s, e) => {
                // Clear background
                using (var pb = new SolidBrush(ThemeConfig.GetParentColor(btnRefresh)))
                    e.Graphics.FillRectangle(pb, -1, -1, btnRefresh.Width + 2, btnRefresh.Height + 2);
                
                // Draw rotating icon or static icon
                if (_isRefreshing)
                    DrawRotatingRefreshIcon(btnRefresh, e.Graphics);
                else
                    ThemeConfig.DrawIconButton(btnRefresh, e.Graphics, "refresh", "Hist_Refresh", ThemeConfig.SuccessColor, ThemeConfig.SuccessColor, true);
            };

            btnRefresh.Click += async (s, e) => {
                if (_isRefreshing) return;
                StartRefreshAnimation();
                await System.Threading.Tasks.Task.Run(() => LoadHistory());
                StopRefreshAnimation();
            };
            pnlRefreshWrapper.Controls.Add(btnRefresh);
            pnlActions.Controls.Add(pnlRefreshWrapper);

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
            
            cardActions = new StatCard { Title = "Activity Today", Value = "0", IconImage = ThemeConfig.GetNuricon("history_activity"), ThemeColor = ThemeConfig.PrimaryColor, Dock = DockStyle.Fill };
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
            dgvSuppliers.AutoGenerateColumns = false;
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", DataPropertyName = "Date", Width = 150 });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", DataPropertyName = "Type", Width = 120 });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Supplier", DataPropertyName = "Supplier", Width = 200 });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", DataPropertyName = "Amount", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            dgvSuppliers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Details", DataPropertyName = "Details", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

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
                    MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_OnlyCompleted"));
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
            
            // Critical: Handle data errors to prevent "Red X" or dialog crashes
            dgv.DataError += (s, e) => { 
                Console.WriteLine($"Grid Error: {e.Exception?.Message}");
                e.ThrowException = false; 
            };
            
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
             if (this.InvokeRequired)
             {
                 this.Invoke(new Action(LoadHistory));
                 return;
             }

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
                 
                 dgvQuotations.DataSource = _historyService.GetQuotationHistory(); 
                 
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
                 MessageHelper.ShowError(LocalizationManager.GetString("Msg_HistoryLoadError") ?? ("Error loading history: " + ex.Message));
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

        private void StartRefreshAnimation()
        {
            _isRefreshing = true;
            _refreshAngle = 0;
            _refreshTimer.Start();
        }

        private void StopRefreshAnimation()
        {
            // Give it a tiny moment to feel "real" if it was too fast
            System.Threading.Tasks.Task.Delay(500).ContinueWith(_ => {
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke(new Action(() => {
                        _refreshTimer.Stop();
                        _isRefreshing = false;
                        _refreshAngle = 0;
                        var refreshBtns = this.Controls.Find("btnRefresh", true);
                        if (refreshBtns.Length > 0) refreshBtns[0].Invalidate();
                    }));
                }
            });
        }

        private void DrawRotatingRefreshIcon(Button btn, Graphics g)
        {
            bool isArabic = LocalizationManager.IsArabic;
            string text = LocalizationManager.GetString("Hist_Refresh");
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
            using (var path = ThemeConfig.GetRoundedPathPublic(r, 12))
            {
                using (Pen pen = new Pen(ThemeConfig.SuccessColor, 1.5f))
                    g.DrawPath(pen, path);
            }

            Image img = ThemeConfig.GetNuricon("refresh");
            int iconSize = 24;
            int margin = 8;
            int iconX = isArabic ? (btn.Width - iconSize - margin) : margin;
            int iconY = (btn.Height - iconSize) / 2;

            if (img != null)
            {
                using (var tinted = ThemeConfig.TintImage(img, ThemeConfig.SuccessColor))
                {
                    // Rotate specifically the icon
                    GraphicsState state = g.Save();
                    g.TranslateTransform(iconX + iconSize / 2, iconY + iconSize / 2);
                    g.RotateTransform(_refreshAngle);
                    g.DrawImage(tinted, -iconSize / 2, -iconSize / 2, iconSize, iconSize);
                    g.Restore(state);
                }
            }

            int textX = isArabic ? margin : (iconX + iconSize + 4);
            int textW = btn.Width - iconSize - (margin * 2) - 4;
            Rectangle textRect = new Rectangle(textX, 0, textW, btn.Height);
            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding;
            if (isArabic) flags |= TextFormatFlags.RightToLeft;
            TextRenderer.DrawText(g, text, btn.Font, textRect, ThemeConfig.SuccessColor, flags);
        }
        
    }
}

