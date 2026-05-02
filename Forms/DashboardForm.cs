using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting; // Standard Charting
using GenericInventorySystem.Services;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;
using System.Collections.Generic;
using System.Data;

namespace GenericInventorySystem.Forms
{
    public partial class DashboardForm : UserControl
    {
        // Service
        private DashboardService _dashboardService;
        
        // Layout
        private TableLayoutPanel _mainLayout;
        private TableLayoutPanel _cardsLayout;
        private TableLayoutPanel _middleLayout;
        
        // Controls
        private StatCard _cardInventory;
        private StatCard _cardRevenue;
        private StatCard _cardOrders;
        private StatCard _cardLowStock;
        
        private Chart _chartWeeklyRevenue;
        private Chart _chartTrends; // Bottom Chart
        
        private Panel _feedPanel; // For Recent Activity or Top Items
        private DataGridView _gridTopItems; // If using grid
        private Label lblDashboardTitle;
        private Label _lblTop;
        private Label _lblTrend;
        private System.ComponentModel.IContainer components = null;

        public DashboardForm()
        {
            _dashboardService = new DashboardService();
            InitializeComponent();
            InitializeDashboardLayout();
            ApplyLocalization(); 
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Size = new System.Drawing.Size(1200, 800);
            this.BackColor = ThemeConfig.BackgroundColor; // Standard background
            
            GenericInventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            GenericInventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;

            if (this.lblDashboardTitle != null) this.lblDashboardTitle.Text = L("Dash_Title");
            
            if (_cardInventory != null) _cardInventory.Title = L("Dash_TotalInventory");
            if (_cardRevenue != null) _cardRevenue.Title = L("Dash_TotalRevenue");
            if (_cardOrders != null) _cardOrders.Title = L("Dash_TotalOrders");
            if (_cardLowStock != null) _cardLowStock.Title = L("Dash_LowStock");

            if (_chartWeeklyRevenue != null && _chartWeeklyRevenue.Titles.Count > 0) 
                _chartWeeklyRevenue.Titles[0].Text = L("Dash_WeeklyRevenue");
            
            if (_chartTrends != null && _chartTrends.Titles.Count > 0) 
                _chartTrends.Titles[0].Text = L("Dash_MonthlyTrends");

            if (_lblTop != null) _lblTop.Text = L("Dash_TopSelling");
            if (_lblTrend != null) _lblTrend.Text = L("Dash_SalesTrends");

            if (_lblTop != null) _lblTop.Text = L("Dash_TopSelling");
            if (_lblTrend != null) _lblTrend.Text = L("Dash_SalesTrends");

            LoadData(); // refresh data strings
        }


        public void RefreshDashboard()
        {
            LoadData();
        }


        private void InitializeDashboardLayout()
        {
            // Main Container
            _mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20),
                BackColor = ThemeConfig.BackgroundColor // Horizon Light Gray
            };

            
            // Row Styles
            // Row Styles
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // Title (Standardized)
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F)); // Cards
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));   // Middle (Bar Chart + List)
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));   // Bottom (Line Chart)
            
            this.Controls.Add(_mainLayout);

            // 0. Title row -- holds the header label + Scan-to-Connect button
            Panel titleRow = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            lblDashboardTitle = ThemeConfig.CreateStandardHeader("Performance Dashboard");
            lblDashboardTitle.Name = "lblDashboardTitle";
            lblDashboardTitle.Margin = new Padding(0);
            lblDashboardTitle.Dock = DockStyle.Left;
            lblDashboardTitle.AutoSize = true;

            // Live server URL label
            string serverUrl = ScanToConnectForm.GetServerUrl();
            var lblServerUrl = new Label
            {
                Text = "📶 " + serverUrl,
                Font = new Font("Segoe UI", 10f),
                ForeColor = ThemeConfig.MutedTextColor,
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 10, 8, 0),
                Cursor = Cursors.Hand
            };
            
            lblServerUrl.Click += (s, e) => {
                Clipboard.SetText(serverUrl);
                string originalText = lblServerUrl.Text;
                lblServerUrl.Text = "✅ " + LocalizationManager.GetString("Msg_Copied");
                lblServerUrl.ForeColor = ThemeConfig.AccentColor;
                
                Timer t = new Timer { Interval = 1500 };
                t.Tick += (ts, te) => {
                    lblServerUrl.Text = originalText;
                    lblServerUrl.ForeColor = ThemeConfig.MutedTextColor;
                    t.Stop();
                    t.Dispose();
                };
                t.Start();
            };

            // Scan-to-Connect button
            var btnScan = new Button
            {
                Text = "Scan to Connect",
                Dock = DockStyle.Right,
                Width = 145,
                Margin = new Padding(10, 6, 10, 6)
            };
            ThemeConfig.ApplyPrimaryButton(btnScan);
            btnScan.Click += (s, e) => new ScanToConnectForm().ShowDialog();

            titleRow.Controls.Add(lblDashboardTitle);
            titleRow.Controls.Add(lblServerUrl);
            titleRow.Controls.Add(btnScan);
            _mainLayout.Controls.Add(titleRow, 0, 0);

            // 1. Cards Layout (Top)
            _cardsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 15), // Spacing below cards
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 4; i++) _cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            
            // Create Cards
            _cardInventory = CreateStatCard("Total Inventory", "inventory", ThemeConfig.PrimaryColor); 
            _cardRevenue = CreateStatCard("Total Revenue", "revenue", ThemeConfig.SuccessColor); 
            _cardOrders = CreateStatCard("Total Orders", "orders", ThemeConfig.WarningColor); 
            _cardLowStock = CreateStatCard("Low Stock", "bell", ThemeConfig.DangerColor); 


            _cardsLayout.Controls.Add(_cardInventory, 0, 0);
            _cardsLayout.Controls.Add(_cardRevenue, 1, 0);
            _cardsLayout.Controls.Add(_cardOrders, 2, 0);
            _cardsLayout.Controls.Add(_cardLowStock, 3, 0);
            
            _mainLayout.Controls.Add(_cardsLayout, 0, 1);

            // 2. Middle Section (Charts + Feed)
            _middleLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 15),
                BackColor = Color.Transparent
            };
            _middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F)); 
            _middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F)); 
            
            // Bar Chart Card
            _chartWeeklyRevenue = CreateModernChart();
            Panel pnlWeeklyCard = ThemeConfig.CreateCardPanel(_chartWeeklyRevenue);
            pnlWeeklyCard.Margin = new Padding(0, 0, 10, 0);
            _middleLayout.Controls.Add(pnlWeeklyCard, 0, 0);
            
            // Top Items Grid Card
            Panel rightContent = new Panel { Dock = DockStyle.Fill };
            _lblTop = new Label 
            { 
                Name = "lblTop",
                Text = "Top Selling Items", 
                Font = ThemeConfig.SubHeaderFont, 
                Dock = DockStyle.Top, 
                Height = 30,
                ForeColor = ThemeConfig.TextColorDark 
            };
            rightContent.Controls.Add(_lblTop);
            
            _gridTopItems = new DataGridView();
            _gridTopItems.DataError += (s, e) => { e.ThrowException = false; };
            ThemeConfig.ApplyGridTheme(_gridTopItems);
            _gridTopItems.Dock = DockStyle.Fill;
            _gridTopItems.ColumnHeadersVisible = true; 
            _gridTopItems.ScrollBars = ScrollBars.Vertical; 
            rightContent.Controls.Add(_gridTopItems);
            _lblTop.BringToFront();

            Panel pnlTopItemsCard = ThemeConfig.CreateCardPanel(rightContent);
            pnlTopItemsCard.Margin = new Padding(10, 0, 0, 0);
            _middleLayout.Controls.Add(pnlTopItemsCard, 1, 0);
            
            _mainLayout.Controls.Add(_middleLayout, 0, 2);

            // 3. Bottom Section (Line Chart)
            Panel bottomContent = new Panel { Dock = DockStyle.Fill };
            _lblTrend = new Label { Name = "lblTrend", Text = "Sales Trends", Font = ThemeConfig.SubHeaderFont, Dock = DockStyle.Top, Height = 30, ForeColor = ThemeConfig.TextColorDark };
            bottomContent.Controls.Add(_lblTrend);

            _chartTrends = CreateModernChart();
            _chartTrends.Series.Clear(); 
            _chartTrends.Dock = DockStyle.Fill;
            bottomContent.Controls.Add(_chartTrends);
            _lblTrend.BringToFront();

            Panel pnlTrendsCard = ThemeConfig.CreateCardPanel(bottomContent);
            _mainLayout.Controls.Add(pnlTrendsCard, 0, 3);
        }

        private StatCard CreateStatCard(string title, string iconName, Color color)
        {
            StatCard card = new StatCard
            {
                Title = title,
                IconImage = ThemeConfig.GetNuricon(iconName),
                ThemeColor = color,
                Value = "0",
                Subtitle = "Loading...",
                Dock = DockStyle.Fill,
                Margin = new Padding(5) // Spacing between cards
            };
            return card;
        }

        private Chart CreateModernChart()
        {
            Chart chart = new Chart { Dock = DockStyle.Fill, BackColor = Color.White };
            ChartArea area = new ChartArea("Default");
            chart.ChartAreas.Add(area);
            
            ThemeConfig.ApplyChartTheme(chart);
            
            return chart;
        }

        public void LoadData()
        {
            if (_dashboardService == null || _chartWeeklyRevenue == null) return;
            try
            {
                // 1. Cards
                decimal totalValue = _dashboardService.GetTotalInventoryValue();
                int totalItems = _dashboardService.GetTotalItems();
                int lowStock = _dashboardService.GetLowStockCount();
                int orders = _dashboardService.GetOrdersCount(); // Default today

                Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;

                if (_cardInventory != null)
                {
                    _cardInventory.Value = totalItems.ToString("N0") + L("Dash_ItemsSuffix");
                    _cardInventory.Subtitle = string.Format(L("Dash_ValuedAt"), $"{totalValue:C}");
                    _cardInventory.FinalizeLayout();
                }
                
                if (_cardRevenue != null)
                {
                    _cardRevenue.Value = _dashboardService.GetSales("Today").ToString("C");
                    _cardRevenue.Subtitle = L("Dash_TodayRevenue");
                    _cardRevenue.FinalizeLayout();
                }
                
                if (_cardOrders != null)
                {
                    _cardOrders.Value = orders.ToString("N0");
                    _cardOrders.Subtitle = L("Dash_NewOrders");
                    _cardOrders.FinalizeLayout();
                }
                
                if (_cardLowStock != null)
                {
                    _cardLowStock.Value = lowStock.ToString("N0");
                    _cardLowStock.Subtitle = L("Dash_NeedsReordering");
                    _cardLowStock.FinalizeLayout();
                    if(lowStock > 0) _cardLowStock.ThemeColor = ThemeConfig.DangerColorBright; // Alert color
                }

                
                // 2. Bar Chart (Weekly Revenue)
                try
                {
                    _chartWeeklyRevenue.Series.Clear();
                    Series seriesBar = new Series(L("Rep_ChartSales"));
                    seriesBar.ChartType = SeriesChartType.Column;
                    seriesBar.ChartArea = "Default"; // Explicit link to the area we forced in ThemeConfig
                    
                    // Add series first, then points (safer for some Chart versions)
                    _chartWeeklyRevenue.Series.Add(seriesBar);

                    var weeklyData = _dashboardService.GetWeeklyRevenue();
                    foreach(var kvp in weeklyData)
                    {
                        seriesBar.Points.AddXY(kvp.Key, kvp.Value);
                    }

                    ThemeConfig.ApplyChartTheme(_chartWeeklyRevenue);
                }
                catch { /* Fail gracefully */ }

                // 3. Top Items Grid
                _gridTopItems.ScrollBars = ScrollBars.Both; // Ensure horizontal scroll if needed too
                _gridTopItems.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None; // Enforce fixed height
                
                var topItems = _dashboardService.GetTopSellingItems(50); // Fetch MORE to enable scrolling
                if(topItems != null)
                {
                    _gridTopItems.DataSource = topItems;
                    if(_gridTopItems.Columns.Contains("part_name")) _gridTopItems.Columns["part_name"].HeaderText = L("Parts_GridProduct");
                    if(_gridTopItems.Columns.Contains("total_sold")) _gridTopItems.Columns["total_sold"].HeaderText = L("Dash_Sold");
                }

                // 4. Line Chart (Trends)
                try
                {
                    _chartTrends.Series.Clear();
                    Series seriesSpline = new Series(L("Dash_SalesTrends"));
                    seriesSpline.ChartArea = "Default";
                    seriesSpline.ChartType = SeriesChartType.SplineArea;
                    seriesSpline.Color = Color.FromArgb(40, ThemeConfig.SuccessColor); 
                    seriesSpline.BorderWidth = 4;
                    seriesSpline.BorderColor = ThemeConfig.SuccessColor;
                    
                    _chartTrends.Series.Add(seriesSpline);

                    var trendData = _dashboardService.GetMonthlySalesTrend();
                    foreach (var kvp in trendData)
                    {
                        seriesSpline.Points.AddXY(kvp.Key, kvp.Value);
                    }

                    ThemeConfig.ApplyChartTheme(_chartTrends);
                }
                catch { /* Fail gracefully */ }
            }
            catch (Exception ex)
            {
                // Surface the real error so we can debug it - previously this was silent
                System.Diagnostics.Debug.WriteLine("Dashboard LoadData error: " + ex.Message);
                // Show user-visible error only if controls are ready
                if (_cardRevenue != null)
                    _cardRevenue.Subtitle = "DB Error: " + ex.Message.Substring(0, Math.Min(ex.Message.Length, 60));
            }
        }
    }
}

