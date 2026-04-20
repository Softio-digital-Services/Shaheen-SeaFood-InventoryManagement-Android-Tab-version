using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using GenericInventorySystem.Data;

namespace GenericInventorySystem.Forms
{
    public partial class ReportsForm : UserControl
    {
        private Chart chartValuation;
        private Chart chartPie;
        private Chart chartBar;
        private Panel pnlKPIContainer;
        private Label lblKPI1Value;
        private Label lblKPI2Value;
        private GenericInventorySystem.Services.DashboardService _dashboardService;

        public ReportsForm()
        {
            _dashboardService = new GenericInventorySystem.Services.DashboardService();
            InitializeComponent();
            ApplyTheme();
            GenericInventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            GenericInventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;

            var title = this.Controls.Find("lblMainTitle", true);
            if (title.Length > 0) title[0].Text = L("Rep_Title");

            var valTitle = this.Controls.Find("lblValTitle", true);
            if (valTitle.Length > 0) valTitle[0].Text = L("Rep_ValuationTitle");

            var pieTitle = this.Controls.Find("lblPieTitle", true);
            if (pieTitle.Length > 0) pieTitle[0].Text = L("Rep_CategoryTitle");

            var kpi1 = this.Controls.Find("kpi1Title", true);
            if (kpi1.Length > 0) kpi1[0].Text = L("Rep_TotalSales");

            var kpi2 = this.Controls.Find("kpi2Title", true);
            if (kpi2.Length > 0) kpi2[0].Text = L("Rep_AvgOrder");

            var barTitle = this.Controls.Find("lblBarTitle", true);
            if (barTitle.Length > 0) barTitle[0].Text = L("Rep_TopProductsTitle");

            LoadCharts(); // Rebind charts with translated series labels
        }

        public void RefreshData()
        {
            LoadCharts();
        }

        private void InitializeComponent()
        {
            this.Controls.Clear();
            this.Size = new Size(1100, 750);
            this.BackColor = ThemeConfig.BackgroundColor;

            // Root Layout
            TableLayoutPanel tlpRoot = new TableLayoutPanel();
            tlpRoot.Dock = DockStyle.Fill;
            tlpRoot.ColumnCount = 1;
            tlpRoot.RowCount = 4;
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Header Compact
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));  // Top Row (Valuation + Pie)
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));  // Bottom Row (KPIs + Bar)
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F)); // Spacer
            tlpRoot.Padding = new Padding(20);
            this.Controls.Add(tlpRoot);

            // 1. Header
            Label lblTitle = ThemeConfig.CreateStandardHeader("Analytics & Reports");
            lblTitle.Name = "lblMainTitle";
            lblTitle.Location = new Point(0, 0); // Override ThemeConfig default to avoid double padding
            tlpRoot.Controls.Add(lblTitle, 0, 0);

            // 2. Top Row Layout
            TableLayoutPanel tlpTop = new TableLayoutPanel();
            tlpTop.Dock = DockStyle.Fill;
            tlpTop.ColumnCount = 2;
            tlpTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F)); // Valuation (Wide)
            tlpTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F)); // Pie (Narrow)
            tlpTop.Margin = new Padding(0, 0, 0, 20);
            tlpRoot.Controls.Add(tlpTop, 0, 1);

            // Valuation Chart Card
            Panel pnlValuation = CreateCardPanel();
            pnlValuation.Dock = DockStyle.Fill;
            pnlValuation.Margin = new Padding(0, 0, 10, 0);
            pnlValuation.Padding = new Padding(20);
            
            Label lblValTitle = GetTitleLabel("Inventory Valuation Over Time");
            lblValTitle.Name = "lblValTitle";
            pnlValuation.Controls.Add(lblValTitle);

            chartValuation = new Chart();
            chartValuation.Dock = DockStyle.Fill;
            pnlValuation.Controls.Add(chartValuation);
            lblValTitle.SendToBack(); // Maintain docking order hack or just add chart second (done)
            tlpTop.Controls.Add(pnlValuation, 0, 0);

            // Pie Chart Card
            Panel pnlPie = CreateCardPanel();
            pnlPie.Dock = DockStyle.Fill;
            pnlPie.Margin = new Padding(10, 0, 0, 0);
            pnlPie.Padding = new Padding(20);
            
            Label lblPieTitle = GetTitleLabel("Sales by Category");
            lblPieTitle.Name = "lblPieTitle";
            pnlPie.Controls.Add(lblPieTitle);

            chartPie = new Chart();
            chartPie.Dock = DockStyle.Fill;
            pnlPie.Controls.Add(chartPie);
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
            Panel kpi1 = CreateCardPanel();
            kpi1.Dock = DockStyle.Fill;
            kpi1.Margin = new Padding(0, 0, 0, 10);
            Label kpi1Title = new Label { Name = "kpi1Title", Text = "Total Sales (YTD):", Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.SecondaryColor, Location = new Point(15, 15), AutoSize = true };
            lblKPI1Value = new Label { Text = "$0", Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.TextColorDark, Location = new Point(15, 45), AutoSize = true };

            kpi1.Controls.Add(kpi1Title);
            kpi1.Controls.Add(lblKPI1Value);
            tlpKPIs.Controls.Add(kpi1, 0, 0);
            
            // KPI 2
            Panel kpi2 = CreateCardPanel();
            kpi2.Dock = DockStyle.Fill;
            kpi2.Margin = new Padding(0, 10, 0, 0);
            Label kpi2Title = new Label { Name = "kpi2Title", Text = "Average Order Value:", Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.SecondaryColor, Location = new Point(15, 15), AutoSize = true };
            lblKPI2Value = new Label { Text = "$0", Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.TextColorDark, Location = new Point(15, 45), AutoSize = true };

            kpi2.Controls.Add(kpi2Title);
            kpi2.Controls.Add(lblKPI2Value);
            tlpKPIs.Controls.Add(kpi2, 0, 1);

            tlpBottom.Controls.Add(pnlKPIContainer, 0, 0);

            // Bar Chart Card
            Panel pnlBar = CreateCardPanel();
            pnlBar.Dock = DockStyle.Fill;
            pnlBar.Margin = new Padding(10, 0, 0, 0);
            pnlBar.Padding = new Padding(20);
            
            Label lblBarTitle = GetTitleLabel("Top Selling Products (This Month)");
            lblBarTitle.Name = "lblBarTitle";
            pnlBar.Controls.Add(lblBarTitle);

            chartBar = new Chart();
            chartBar.Dock = DockStyle.Fill;
            pnlBar.Controls.Add(chartBar);
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
                Height = 30
            };

        }

        private void ApplyTheme()
        {
            // Adding Titles before applying theme
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;
            
            chartValuation.Titles.Clear();
            ThemeConfig.ApplyChartTheme(chartValuation);
            
            chartPie.Titles.Clear();
            ThemeConfig.ApplyChartTheme(chartPie);
            
            chartBar.Titles.Clear();
            ThemeConfig.ApplyChartTheme(chartBar);
            
            // Tweak specific chart details
            if(chartPie.ChartAreas.Count > 0) 
            {
                chartPie.ChartAreas[0].Area3DStyle.Enable3D = false;
                chartPie.ChartAreas[0].InnerPlotPosition = new ElementPosition(5, 5, 90, 80);
            }
        }
        

        private void LoadCharts()
        {
            try 
            {
                LoadValuationChart();
                LoadCategoryChart();
                LoadTopProductsChart();
            }
            catch (Exception ex)
            {
                // Silent fail or log
                Console.WriteLine(ex.Message);
            }
        }

        private void LoadValuationChart()
        {
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;
            chartValuation.Series.Clear();
            var s = chartValuation.Series.Add(L("Rep_ChartValuation"));
            s.ChartType = SeriesChartType.SplineArea; // Filled area looks premium
            s.Color = Color.FromArgb(40, ThemeConfig.PrimaryColor); // Subtle Blue Fill
            s.BorderColor = ThemeConfig.PrimaryColor; // Solid Blue Line
            s.BorderWidth = 4;

            s.MarkerStyle = MarkerStyle.Circle;
            s.MarkerSize = 8;
            s.MarkerColor = Color.White;
            s.MarkerBorderColor = s.BorderColor;
            s.MarkerBorderWidth = 2;

            // Database Data
            var monthlyRevenue = _dashboardService.GetMonthlyRevenue();
            foreach (var kvp in monthlyRevenue)
            {
                s.Points.AddXY(kvp.Key, kvp.Value);
            }

            // Update KPIs
            lblKPI1Value.Text = _dashboardService.GetTotalSalesYTD().ToString("C");
            lblKPI2Value.Text = _dashboardService.GetAverageOrderValue().ToString("C");
        }

        private void LoadCategoryChart()
        {
            chartPie.Series.Clear();
            var s = chartPie.Series.Add("Series1");
            s.ChartType = SeriesChartType.Doughnut;
            
            // Database Data
            DataTable dt = _dashboardService.GetSalesByCategory();
            foreach (DataRow row in dt.Rows)
            {
                s.Points.AddXY(row["category_name"].ToString(), Convert.ToDecimal(row["total_sales"]));
            }
            
            // Custom Colors from Palette
            for(int i=0; i<s.Points.Count; i++) s.Points[i].Color = ThemeConfig.ChartPalette[i % ThemeConfig.ChartPalette.Length];

            
            // Fix Clipping at top
            if(chartPie.ChartAreas.Count > 0)
            {
                 // Give it breathing room inside the chart area
                 chartPie.ChartAreas[0].InnerPlotPosition = new ElementPosition(5, 5, 90, 80); // X, Y, W, H (Percentages)
            }
        }

        private void LoadTopProductsChart()
        {
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;
            chartBar.Series.Clear();
            var s = chartBar.Series.Add(L("Rep_ChartSales"));
            s.ChartType = SeriesChartType.Column; // Vertical Bar
            s.Color = ThemeConfig.PrimaryColor;
            s.BackGradientStyle = GradientStyle.TopBottom;
            s.BackSecondaryColor = Color.FromArgb(150, ThemeConfig.PrimaryColor);

            // Database Data
            DataTable dt = _dashboardService.GetTopSellingItems(5);
            foreach (DataRow row in dt.Rows)
            {
                s.Points.AddXY(row["part_name"].ToString(), Convert.ToInt32(row["total_sold"]));
            }
        }

        private Panel CreateCardPanel()
        {
            Panel p = new Panel();
            p.BackColor = Color.White;
            p.Padding = new Padding(1); // Border width equivalent
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                using (var path = GetRoundedRect(r, 12))
                using (var pen = new Pen(ThemeConfig.BorderColor, 1))
                {
                    using (var brush = new SolidBrush(ThemeConfig.SurfaceColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                    e.Graphics.DrawPath(pen, path);
                }
            };

            return p;
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

