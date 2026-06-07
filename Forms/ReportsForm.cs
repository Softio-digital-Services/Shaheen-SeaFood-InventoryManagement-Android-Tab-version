using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using InventorySystem.Data;

namespace InventorySystem.Forms
{
    public partial class ReportsForm : UserControl
    {
        private Chart chartValuation;
        private Chart chartPie;
        private Chart chartBar;
        private Panel pnlKPIContainer;
        private Label lblKPI1Value;
        private Label lblKPI2Value;
        private InventorySystem.Services.DashboardService _dashboardService;

        public ReportsForm()
        {
            _dashboardService = new InventorySystem.Services.DashboardService();
            InitializeComponent();
            ApplyTheme();
            InventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyLocalization();
        }


        private void ApplyLocalization()
        {
            InventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;
            bool isRTL = InventorySystem.Helpers.LocalizationManager.IsArabic;

            var titleAlign  = isRTL ? ContentAlignment.BottomRight : ContentAlignment.BottomLeft;
            var valueAlign  = isRTL ? ContentAlignment.TopRight    : ContentAlignment.TopLeft;

            var title = this.Controls.Find("lblMainTitle", true);
            if (title.Length > 0) title[0].Text = L("Rep_Title");

            var valTitle = this.Controls.Find("lblValTitle", true);
            if (valTitle.Length > 0) valTitle[0].Text = L("Rep_ValuationTitle");

            var pieTitle = this.Controls.Find("lblPieTitle", true);
            if (pieTitle.Length > 0) pieTitle[0].Text = L("Rep_CategoryTitle");

            var kpi1 = this.Controls.Find("kpi1Title", true);
            if (kpi1.Length > 0)
            {
                kpi1[0].Text = L("Rep_TotalSales");
                ((Label)kpi1[0]).TextAlign = titleAlign;
            }
            if (lblKPI1Value != null) lblKPI1Value.TextAlign = valueAlign;

            var kpi2 = this.Controls.Find("kpi2Title", true);
            if (kpi2.Length > 0)
            {
                kpi2[0].Text = L("Rep_AvgOrder");
                ((Label)kpi2[0]).TextAlign = titleAlign;
            }
            if (lblKPI2Value != null) lblKPI2Value.TextAlign = valueAlign;

            var barTitle = this.Controls.Find("lblBarTitle", true);
            if (barTitle.Length > 0) barTitle[0].Text = L("Rep_TopProductsTitle");

            LoadCharts();
        }

        public void RefreshData()
        {
            LoadCharts();
        }

        private void InitializeComponent()
        {
            this.Controls.Clear();
            this.Size = new Size(1100, 750);


            // Root Layout
            TableLayoutPanel tlpRoot = new TableLayoutPanel();
            tlpRoot.Dock = DockStyle.Fill;
            tlpRoot.ColumnCount = 1;
            tlpRoot.RowCount = 3;
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));  // Top Row (Valuation + Pie)
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));  // Bottom Row (KPIs + Bar)
            tlpRoot.Padding = new Padding(20);
            this.Controls.Add(tlpRoot);

            // 1. Header
            Label lblTitle = ThemeConfig.CreateStandardHeader("Analytics & Reports");
            lblTitle.Name = "lblMainTitle";

            TableLayoutPanel tlpHeader = ThemeConfig.CreateGlobalFormHeader(lblTitle, null, null);
            tlpRoot.Controls.Add(tlpHeader, 0, 0);

            // 2. Top Row Layout
            TableLayoutPanel tlpTop = new TableLayoutPanel();
            tlpTop.Dock = DockStyle.Fill;
            tlpTop.ColumnCount = 2;
            tlpTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F)); // Valuation (Wide)
            tlpTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F)); // Pie (Narrow)
            tlpTop.Margin = new Padding(0, 0, 0, 20);
            tlpRoot.Controls.Add(tlpTop, 0, 1);

            // Valuation Chart Card
            chartValuation = new Chart { Dock = DockStyle.Fill };
            Panel pnlValuation = ThemeConfig.CreateCardPanel(chartValuation);
            pnlValuation.Dock = DockStyle.Fill;
            pnlValuation.Margin = new Padding(0, 0, 10, 0);
            
            Label lblValTitle = GetTitleLabel("Inventory Valuation Over Time");
            lblValTitle.Name = "lblValTitle";
            pnlValuation.Controls[0].Controls.Add(lblValTitle);
            lblValTitle.BringToFront(); // Ensure title is visible above chart in card

            tlpTop.Controls.Add(pnlValuation, 0, 0);

            // Pie Chart Card
            chartPie = new Chart { Dock = DockStyle.Fill };
            Panel pnlPie = ThemeConfig.CreateCardPanel(chartPie);
            pnlPie.Dock = DockStyle.Fill;
            pnlPie.Margin = new Padding(10, 0, 0, 0);
            
            Label lblPieTitle = GetTitleLabel("Sales by Category");
            lblPieTitle.Name = "lblPieTitle";
            pnlPie.Controls[0].Controls.Add(lblPieTitle);
            lblPieTitle.BringToFront();

            tlpTop.Controls.Add(pnlPie, 1, 0);


            // 3. Bottom Row Layout
            TableLayoutPanel tlpBottom = new TableLayoutPanel();
            tlpBottom.Dock = DockStyle.Fill;
            tlpBottom.ColumnCount = 2;
            tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // KPIs (Narrow)
            tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F)); // Bar (Wide)
            tlpRoot.Controls.Add(tlpBottom, 0, 2);

            // KPI Container
            pnlKPIContainer = new Panel();
            pnlKPIContainer.Dock = DockStyle.Fill;
            pnlKPIContainer.Margin = new Padding(0, 0, 10, 0);
            
            // Note: We'll add KPI cards dynamically or just place 2 here
            TableLayoutPanel tlpKPIs = new TableLayoutPanel();
            tlpKPIs.Dock = DockStyle.Fill;
            tlpKPIs.ColumnCount = 1;
            tlpKPIs.RowCount = 2;
            tlpKPIs.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpKPIs.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pnlKPIContainer.Controls.Add(tlpKPIs);

            // KPI 1
            TableLayoutPanel tlpKPI1 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
                Padding = new Padding(14, 10, 14, 10)
            };
            tlpKPI1.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            tlpKPI1.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));

            Label kpi1Title = new Label
            {
                Name = "kpi1Title", Text = "Total Sales (YTD):",
                Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.SecondaryColor,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft,
                AutoSize = false
            };
            lblKPI1Value = new Label
            {
                Text = "$0", Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.TextColorDark,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft,
                AutoSize = false
            };
            tlpKPI1.Controls.Add(kpi1Title, 0, 0);
            tlpKPI1.Controls.Add(lblKPI1Value, 0, 1);

            Panel kpi1 = ThemeConfig.CreateCardPanel(tlpKPI1);
            kpi1.Dock = DockStyle.Fill;
            kpi1.Margin = new Padding(0, 0, 0, 10);
            tlpKPIs.Controls.Add(kpi1, 0, 0);

            // KPI 2
            TableLayoutPanel tlpKPI2 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
                Padding = new Padding(14, 10, 14, 10)
            };
            tlpKPI2.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            tlpKPI2.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));

            Label kpi2Title = new Label
            {
                Name = "kpi2Title", Text = "Average Order Value:",
                Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.SecondaryColor,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft,
                AutoSize = false
            };
            lblKPI2Value = new Label
            {
                Text = "$0", Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.TextColorDark,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft,
                AutoSize = false
            };
            tlpKPI2.Controls.Add(kpi2Title, 0, 0);
            tlpKPI2.Controls.Add(lblKPI2Value, 0, 1);

            Panel kpi2 = ThemeConfig.CreateCardPanel(tlpKPI2);
            kpi2.Dock = DockStyle.Fill;
            kpi2.Margin = new Padding(0, 10, 0, 0);
            tlpKPIs.Controls.Add(kpi2, 0, 1);

            tlpBottom.Controls.Add(pnlKPIContainer, 0, 0);

            // Bar Chart Card
            chartBar = new Chart { Dock = DockStyle.Fill };
            Panel pnlBar = ThemeConfig.CreateCardPanel(chartBar);
            pnlBar.Dock = DockStyle.Fill;
            pnlBar.Margin = new Padding(10, 0, 0, 0);
            
            Label lblBarTitle = GetTitleLabel("Top Selling Products (This Month)");
            lblBarTitle.Name = "lblBarTitle";
            pnlBar.Controls[0].Controls.Add(lblBarTitle);
            lblBarTitle.BringToFront();

            tlpBottom.Controls.Add(pnlBar, 1, 0);
        }

        private Label GetTitleLabel(string text)
        {
            return new Label 
            { 
                Text = text, 
                Dock = DockStyle.Top, 
                Font = ThemeConfig.SubHeaderFont, 
                ForeColor = ThemeConfig.TextColorDark,
                Height = 35,
                Padding = new Padding(15, 10, 0, 0)
            };
        }

        private void ApplyTheme()
        {
            // Adding Titles before applying theme
            Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;
            
            chartValuation.Titles.Clear();
            ThemeConfig.ApplyChartTheme(chartValuation);
            
            chartPie.Titles.Clear();
            ThemeConfig.ApplyChartTheme(chartPie);
            
            chartBar.Titles.Clear();
            ThemeConfig.ApplyChartTheme(chartBar);
        }
        

        private void LoadCharts()
        {
            try 
            {
                // Ensure chart areas are initialized with the expected name
                if (chartValuation.ChartAreas.Count == 0) chartValuation.ChartAreas.Add("Default");
                chartValuation.ChartAreas[0].Name = "Default";
                
                if (chartPie.ChartAreas.Count == 0) chartPie.ChartAreas.Add("Default");
                chartPie.ChartAreas[0].Name = "Default";

                if (chartBar.ChartAreas.Count == 0) chartBar.ChartAreas.Add("Default");
                chartBar.ChartAreas[0].Name = "Default";

                LoadValuationChart();
                LoadCategoryChart();
                LoadTopProductsChart();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Chart Load Error: " + ex.Message);
            }
        }

        private void LoadValuationChart()
        {
            Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;
            chartValuation.Series.Clear();
            var s = new Series(L("Rep_ChartValuation"));
            s.ChartArea = "Default";
            s.ChartType = SeriesChartType.SplineArea; 
            s.Color = Color.FromArgb(40, ThemeConfig.PrimaryColor); 
            s.BorderColor = ThemeConfig.PrimaryColor; 
            s.BorderWidth = 4;
            
            // Add to chart before points
            chartValuation.Series.Add(s);

            // Database Data
            var monthlyRevenue = _dashboardService.GetMonthlyRevenue();
            foreach (var kvp in monthlyRevenue)
            {
                s.Points.AddXY(kvp.Key, kvp.Value);
            }

            ThemeConfig.ApplyChartTheme(chartValuation);

            // Update KPIs
            lblKPI1Value.Text = _dashboardService.GetTotalSalesYTD().ToString("C");
            lblKPI2Value.Text = _dashboardService.GetAverageOrderValue().ToString("C");
        }

        private void LoadCategoryChart()
        {
            chartPie.Series.Clear();
            var s = new Series("Series1");
            s.ChartArea = "Default";
            s.ChartType = SeriesChartType.Doughnut;
            chartPie.Series.Add(s);
            
            // Database Data
            DataTable dt = _dashboardService.GetSalesByCategory();
            foreach (DataRow row in dt.Rows)
            {
                s.Points.AddXY(row["category_name"].ToString(), Convert.ToDecimal(row["total_sales"]));
            }
            
            ThemeConfig.ApplyChartTheme(chartPie);
            
            // Colors from Palette
            for(int i=0; i<s.Points.Count; i++) s.Points[i].Color = ThemeConfig.ChartPalette[i % ThemeConfig.ChartPalette.Length];
        }

        private void LoadTopProductsChart()
        {
            Func<string, string> L = InventorySystem.Helpers.LocalizationManager.GetString;
            chartBar.Series.Clear();
            var s = new Series(L("Rep_ChartSales"));
            s.ChartArea = "Default";
            s.ChartType = SeriesChartType.Column; 
            s.Color = ThemeConfig.PrimaryColor;
            chartBar.Series.Add(s);

            // Database Data
            DataTable dt = _dashboardService.GetTopSellingItems(5);
            foreach (DataRow row in dt.Rows)
            {
                s.Points.AddXY(row["part_name"].ToString(), Convert.ToInt32(row["total_sold"]));
            }

            ThemeConfig.ApplyChartTheme(chartBar);
        }

    }
}

