using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Services;

namespace GenericInventorySystem
{
    public partial class MainForm : Form
    {
        private Forms.PartsForm partsForm;
        private Forms.UsersForm usersForm;
        private Forms.QuotationsForm quotationsForm;
        private GenericInventorySystem.Forms.DashboardForm dashboardForm;
        private GenericInventorySystem.Forms.ReportsForm reportsForm;
        private GenericInventorySystem.Forms.HistoryForm historyForm;
        private Forms.CustomersForm customersForm;
        private Forms.SuppliersForm suppliersForm;
        private Forms.POSForm posForm;
        private Forms.PurchaseOrdersForm purchaseOrdersForm;
        private Forms.MonthlyExpensesForm monthlyExpensesForm;

        // Header Controls
        private PictureBox pbNotification;
        private PictureBox pbUserAvatar;
        private ContextMenuStrip menuNotifications;
        private ContextMenuStrip menuUserProfile;
        
        private Services.DashboardService _dashboardService;
        private System.Windows.Forms.Timer _notificationTimer;
        private int _lowStockCount = 0;
        private Helpers.Plugins.PluginContext _pluginContext;

        public MainForm()
        {
            InitializeComponent();
            ApplyTheme();
            InitializeNavigation();
            RefineNavigationLayout();
            ApplyPermissions();
            InitializeNotificationSystem();
            
            panel1.MouseDown += Header_MouseDown;
            label2.MouseDown += Header_MouseDown;

            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();

            LoadPlugins();
            this.FormClosed += (s, e) => Helpers.Plugins.PluginManager.ShutdownAll();
        }

        private void LoadPlugins()
        {
            var pnlNav = this.Controls.Find("pnlNav", true).FirstOrDefault() as Panel;
            _pluginContext = new Helpers.Plugins.PluginContext
            {
                ConnectionString = DatabaseConfig.ConnectionString,
                CurrentUser      = UserSession.Username,
                IsAdmin          = UserSession.IsAdmin,
                CheckLicense     = (key) => Helpers.LicenseManager.IsFeatureEnabled(key),
                ShowSuccess      = (msg) => MessageHelper.ShowSuccess(msg),
                ShowError        = (msg) => MessageHelper.ShowError(msg),
                ShowInfo         = (msg) => MessageHelper.ShowInfo(msg),
                AddTab = (tabTitle, iconName, tabOrder, contentFactory) => {
                    // Filter out Calculator and Backup from sidebar as they are now in the header
                    if (tabTitle.Contains("Calculator") || tabTitle.Contains("Backup") || tabTitle.Contains("\u062d\u0627\u0633\u0628\u0629") || tabTitle.Contains("\u0646\u0633\u062e\u0629")) return;

                    if (this.InvokeRequired) this.Invoke((Action)(() => AddPluginTab(tabTitle, iconName, contentFactory, pnlNav)));
                    else AddPluginTab(tabTitle, iconName, contentFactory, pnlNav);
                },
                AddMenuItem = (group, item) => {
                    if (this.InvokeRequired) this.Invoke((Action)(() => AddPluginMenuItem(group, item)));
                    else AddPluginMenuItem(group, item);
                }
            };
            Helpers.Plugins.PluginManager.DiscoverAndLoad(_pluginContext);
        }

        private void AddPluginTab(string tabTitle, string iconName, Func<UserControl> contentFactory, Panel pnlNav)
        {
            UserControl cachedContent = null;
            Button btn = CreateNavigationButton(tabTitle, iconName, (s, e) => {
                if (cachedContent == null) {
                    cachedContent = contentFactory();
                    cachedContent.Dock = DockStyle.Fill;
                    panel3.Controls.Add(cachedContent);
                }
                ShowForm(cachedContent);
            });
            btn.Dock = DockStyle.Top;
            btn.Margin = new Padding(0);
            if (pnlNav != null) { pnlNav.Controls.Add(btn); btn.BringToFront(); }
        }

        private void AddPluginMenuItem(string group, Helpers.Plugins.PluginMenuItem item)
        {
            ContextMenuStrip strip = panel1.Tag as ContextMenuStrip;
            if (strip == null) {
                strip = new ContextMenuStrip();
                ThemeConfig.ApplyModernMenuTheme(strip);
                panel1.Tag = strip;
            }
            ToolStripMenuItem groupMenu = null;
            foreach (ToolStripItem si in strip.Items) {
                if (si is ToolStripMenuItem tsm && tsm.Text == group) { groupMenu = tsm; break; }
            }
            if (groupMenu == null) {
                groupMenu = new ToolStripMenuItem(group);
                groupMenu.Font = ThemeConfig.StandardFont;
                strip.Items.Add(groupMenu);
            }
            if (item.IsSeparator) { groupMenu.DropDownItems.Add(new ToolStripSeparator()); return; }
            var tsItem = new ToolStripMenuItem(item.Label);
            tsItem.Font = ThemeConfig.StandardFont;
            if (!string.IsNullOrEmpty(item.Icon)) tsItem.Image = ThemeConfig.GetNuricon(item.Icon);
            if (item.OnClick != null) tsItem.Click += (s, e) => item.OnClick();
            groupMenu.DropDownItems.Add(tsItem);
        }

        private void ApplyLocalization()
        {
            bool isAr = LocalizationManager.IsArabic;
            LocalizationManager.ApplyRTL(this);
            LocalizationManager.TranslateControl(this);
            Func<string, string> L = LocalizationManager.GetString;
  
            label2.Text = L("Nav_MainTitle");
            Dashboard_btn.Text = "  " + L("Nav_Dashboard");
  
            UpdateNavText("btnInventory", "Nav_Inventory");
            UpdateNavText("btnCustomers", "Nav_Customers");
            UpdateNavText("btnSuppliers", "Nav_Suppliers");
            UpdateNavText("btnPOS", "Nav_POS");
            UpdateNavText("btnReports", "Nav_Reports");
            UpdateNavText("btnHistory", "Nav_History");
            UpdateNavText("btnQuotations", "Nav_Quotations");
            UpdateNavText("btnCurrencies", "Nav_Currencies");
            UpdateNavText("btnPO", "Nav_PurchaseOrders");
            UpdateNavText("btnExpenses", "Nav_Expenses");
            UpdateNavText("btnUsers", "Nav_Users");

            button3.Text = "  " + L("Nav_Logout");
            if(itemAddUser != null) itemAddUser.Text = L("Nav_AddUser");
            if(itemLicenseInfo != null) itemLicenseInfo.Text = L("Nav_LicenseInfo");
            if(itemLogout != null) itemLogout.Text = L("Nav_Logout");
            if(itemLogout != null) itemLogout.Text = L("Nav_Logout");
            if(btnLock != null) btnLock.Text = ""; // Icon is set via btnLock.Image below

            var pnlHeaderIcons = this.Controls.Find("rightPanel", true).FirstOrDefault() as Panel;
            if (pnlHeaderIcons != null) {
                if (isAr) {
                    panel2.Dock = DockStyle.Right;
                    pnlHeaderIcons.Dock = DockStyle.Left;
                } else {
                    panel2.Dock = DockStyle.Left;
                    pnlHeaderIcons.Dock = DockStyle.Right;
                }
            }
            var pbLogo = panel1.Controls.Find("pbLogo", true).FirstOrDefault() as PictureBox;
            if (pbLogo != null)
            {
                pbLogo.Location = isAr ? new Point(panel1.Width - pbLogo.Width - 20, (panel1.Height - pbLogo.Height) / 2) : new Point(20, (panel1.Height - pbLogo.Height) / 2);
            }
            label2.Location = isAr ? new Point(panel1.Width - label2.Width - 20, (panel1.Height - label2.Height) / 2) : new Point(20, (panel1.Height - label2.Height) / 2);
        }

        private void UpdateNavText(string name, string key) {
            var btns = this.Controls.Find(name, true);
            if (btns.Length > 0) btns[0].Text = "  " + LocalizationManager.GetString(key);
        }

        private void InitializeNavigation()
        {
            ThemeConfig.ApplyFormIcon(this);
            Panel pnlNav = new Panel { Name = "pnlNav", Dock = DockStyle.Fill, Padding = new Padding(0), BackColor = Color.Transparent, AutoScroll = true };
            panel2.Controls.Add(pnlNav);
            pnlNav.BringToFront();

            PictureBox pbSidebarLogo = new PictureBox { Name = "pbSidebarLogo", Size = new Size(140, 160), SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Top, Padding = new Padding(0, 20, 0, 20) };
            try { string logoPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "inventory_logo.png"); if(System.IO.File.Exists(logoPath)) pbSidebarLogo.Image = Image.FromFile(logoPath); } catch { }
            pnlNav.Controls.Add(pbSidebarLogo);
            pbSidebarLogo.BringToFront(); // Highest index in Dock=Top is top, index 0 is bottom. No, wait. 
            // In WinForms Dock=Top: The control with the HIGHEST z-order index is at the top.
            // BringToFront sets index to 0. SendToBack sets to last.
            // So for A to be above B: A should have higher index than B.
            // A.SendToBack() makes it top if it's the first one. 
            // Let's just use the simplest logic: Add them and BringToFront each.
            // If I add Logo then BringToFront: Logo is index 0.
            // If I add Dashboard then BringToFront: Dashboard is 0, Logo is 1.
            // Now Logo (1) is ABOVE Dashboard (0).
            // This is exactly what we want.

            // Dashboard - wrapped in try/catch so a DB error never crashes MainForm
            try
            {
                dashboardForm = new GenericInventorySystem.Forms.DashboardForm { Dock = DockStyle.Fill };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Dashboard init error: " + ex.Message);
                dashboardForm = new GenericInventorySystem.Forms.DashboardForm();
                dashboardForm.Dock = DockStyle.Fill;
            }
            panel3.Controls.Add(dashboardForm);
            panel3.Controls.Add(dashboardForm);
            
            // Consolidate Dashboard Button logic to avoid redundant subscriptions and visual lag
            Dashboard_btn.Click -= button1_Click;
            Dashboard_btn.Click += (s, e) => { 
                ShowForm(dashboardForm); 
                HighlightSelectedButton(Dashboard_btn); 
            };
            
            Image dashIcon = ThemeConfig.GetNuricon("dashboard");
            ThemeConfig.ApplySidebarButtonIcon(Dashboard_btn, dashIcon != null ? ResizeImage(dashIcon, 22, 22) : null, false);
            Dashboard_btn.Text = "  " + LocalizationManager.GetString("Nav_Dashboard");

            // Forms Setup
            usersForm = InitializeForm<Forms.UsersForm>();
            partsForm = InitializeForm<Forms.PartsForm>();
            customersForm = InitializeForm<Forms.CustomersForm>();
            suppliersForm = InitializeForm<Forms.SuppliersForm>();
            posForm = InitializeForm<Forms.POSForm>();
            reportsForm = InitializeForm<Forms.ReportsForm>();
            historyForm = InitializeForm<Forms.HistoryForm>();
            quotationsForm = InitializeForm<Forms.QuotationsForm>();
            purchaseOrdersForm = InitializeForm<Forms.PurchaseOrdersForm>();
            monthlyExpensesForm = InitializeForm<Forms.MonthlyExpensesForm>();

            // Navigation Buttons
            Dashboard_btn.Height = 50;
            Dashboard_btn.Width = 225; // Force exact width matching panel2
            Dashboard_btn.Margin = new Padding(0);
            Dashboard_btn.Dock = DockStyle.Top;
            Dashboard_btn.Text = "  Dashboard";
            Dashboard_btn.Image = ResizeImage(ThemeConfig.GetNuricon("dashboard"), 22, 22);
            ThemeConfig.ApplySidebarButtonIcon(Dashboard_btn, Dashboard_btn.Image, false);
            Dashboard_btn.BringToFront(); // Place below logo
            pnlNav.Controls.Add(Dashboard_btn);
            Dashboard_btn.BringToFront(); 
            
            bool isAdmin = UserSession.IsAdmin;
            bool isAccountant = UserSession.IsAccountant;
            bool isWorker = UserSession.IsStaff;

            // Worker can see POS, Inventory, Customers
            if (isAdmin || isWorker || isAccountant) AddNavButton(pnlNav, "Inventory", "inventory", "btnInventory", () => ShowForm(partsForm));
            if (isAdmin || isWorker) AddNavButton(pnlNav, "POS / Checkout", "pos", "btnPOS", () => ShowForm(posForm));
            if (isAdmin || isWorker || isAccountant) AddNavButton(pnlNav, "Customers", "customers", "btnCustomers", () => ShowForm(customersForm));
            
            // Accountants & Admins
            if (isAdmin || isAccountant)
            {
                AddNavButton(pnlNav, "Suppliers", "suppliers", "btnSuppliers", () => ShowForm(suppliersForm));
                AddNavButton(pnlNav, "Purchase Orders", "inventory", "btnPO", () => ShowForm(purchaseOrdersForm));
                AddNavButton(pnlNav, "Monthly Expenses", "expenses", "btnExpenses", () => { monthlyExpensesForm.LoadData(); ShowForm(monthlyExpensesForm); });
                AddNavButton(pnlNav, "Reports", "reports", "btnReports", () => { reportsForm.RefreshData(); ShowForm(reportsForm); });
                AddNavButton(pnlNav, "History", "history", "btnHistory", () => { historyForm.LoadHistory(); ShowForm(historyForm); });
            }

            // Admin Only
            if (isAdmin)
            {
                AddNavButton(pnlNav, "Quotations", "quotations", "btnQuotations", () => { quotationsForm.LoadQuotations(); ShowForm(quotationsForm); });
                AddNavButton(pnlNav, "Users Management", "user", "btnUsers", () => ShowForm(usersForm));
            }

            ShowForm(dashboardForm);
            HighlightSelectedButton(Dashboard_btn);
        }

        private T InitializeForm<T>() where T : UserControl, new() {
            T f = new T { Dock = DockStyle.Fill, Visible = false };
            panel3.Controls.Add(f);
            return f;
        }

        private void AddNavButton(Panel pnl, string text, string icon, string name, Action clickAction) {
            Button btn = CreateNavigationButton(text, icon, (s, e) => clickAction());
            btn.Name = name; btn.Dock = DockStyle.Top; btn.Margin = new Padding(0);
            pnl.Controls.Add(btn);
            btn.BringToFront(); // Stack below previous items
        }

        private void RefineNavigationLayout()
        {
            panel2.Controls.Remove(label4); label4.Visible = false;
            
            // Re-order panel2 to ensure pnlBranding is NOT covered by pnlNav
            Panel pnlNav = panel2.Controls.Find("pnlNav", true).FirstOrDefault() as Panel;
            if(pnlNav != null) panel2.Controls.Remove(pnlNav);

            // Create a dedicated branding panel at the bottom
            Panel pnlBranding = new Panel { 
                Name = "pnlBranding",
                Dock = DockStyle.Bottom, 
                Height = 60, 
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            panel2.Controls.Add(pnlBranding);
            
            // Now re-add pnlNav to fill the REMAINING space
            if(pnlNav != null) {
                panel2.Controls.Add(pnlNav);
                pnlNav.Dock = DockStyle.Fill;
                pnlNav.BringToFront();
            }

            // Logout Button - Moved into the branding panel
            button3.Parent = pnlBranding;
            button3.Dock = DockStyle.Top; 
            button3.Height = 50; 
            button3.Text = "  Logout"; 
            button3.ForeColor = ThemeConfig.DangerColor;
            Image logoutIcon = ThemeConfig.GetNuricon("logout");
            if (logoutIcon != null) { button3.Image = ResizeImage(logoutIcon, 22, 22); button3.ImageAlign = ContentAlignment.MiddleLeft; button3.TextImageRelation = TextImageRelation.ImageBeforeText; }
            button3.TextAlign = ContentAlignment.MiddleLeft; button3.Padding = new Padding(15, 0, 0, 0); button3.Font = ThemeConfig.ButtonFont;
            button3.FlatAppearance.MouseOverBackColor = ThemeConfig.DangerLight;

            SetupHeaderIcons();
            SetupFooter();
        }

        private void SetupFooter()
        {
            Panel pnlFooter = new Panel {
                Name = "pnlFooter",
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = ThemeConfig.SurfaceColor,
                Padding = new Padding(15, 0, 15, 0)
            };
            this.Controls.Add(pnlFooter);
            pnlFooter.BringToFront(); // Ensure it stays on top of the fill panel

            Label lblVersion = new Label {
                Text = "Generic Inventory System | Version 1.0.2 | © 2026 Softio Services",
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeConfig.TextColorDark,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Left
            };
            pnlFooter.Controls.Add(lblVersion);

            Label lblDeveloper = new Label {
                Text = "Developed by Softio",
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = ThemeConfig.PrimaryColor,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Right
            };
            pnlFooter.Controls.Add(lblDeveloper);
            
            // Adjust panel3 to not be covered (it's Dock=Fill)
            panel3.BringToFront(); // No, panel3 should be BEHIND the footer if it's Dock=Fill?
            // Actually, in WinForms, the LAST control added with Dock=Fill takes the remaining space.
            // But Dock=Bottom takes space from the container. 
            // Let's ensure the order is correct.
            pnlFooter.SendToBack(); 
            panel2.SendToBack();
            panel1.SendToBack();
            panel3.BringToFront();
        }

        private void SetupHeaderIcons()
        {
            Panel rightPanel = new Panel { Name = "rightPanel", Size = new Size(500, 50), BackColor = Color.Transparent, Dock = DockStyle.Right };
            int w = rightPanel.Width;
            AddHeaderButton(rightPanel, w - 45, "Close", "btnWinClose", () => Application.Exit());
            AddHeaderButton(rightPanel, w - 90, "Maximize", "btnWinMax", () => { this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized; });
            AddHeaderButton(rightPanel, w - 135, "Minimize", "btnWinMin", () => this.WindowState = FormWindowState.Minimized);
            
            pbUserAvatar = new PictureBox { Size = new Size(42, 42), Location = new Point(w - 185, 4), SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("user"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbUserAvatar);
            pbUserAvatar.Click += (s, e) => menuUser.Show(pbUserAvatar, new Point(0, pbUserAvatar.Height));
            rightPanel.Controls.Add(pbUserAvatar);

            pbNotification = new PictureBox { Size = new Size(42, 42), Location = new Point(w - 235, 4), SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("bell"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbNotification);
            pbNotification.Paint += (s, e) => { if (_lowStockCount > 0) { e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; using (SolidBrush b = new SolidBrush(ThemeConfig.DangerColorBright)) e.Graphics.FillEllipse(b, 24, 6, 8, 8); } };
            pbNotification.Click += (s, e) => ShowNotifications(s, e);
            rightPanel.Controls.Add(pbNotification);

            btnLock.Location = new Point(w - 285, 4);
            btnLock.Size = new Size(42, 42);
            btnLock.Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("lock"), Color.White);
            btnLock.SizeMode = PictureBoxSizeMode.Zoom;
            ThemeConfig.ApplyHeaderIconStyle(btnLock);
            btnLock.Click += (s, e) => BtnLock_Click(s, e);
            rightPanel.Controls.Add(btnLock);

            // Calculator
            PictureBox pbCalc = new PictureBox { Size = new Size(42, 42), Location = new Point(w - 335, 4), SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("calculator"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbCalc);
            pbCalc.Click += (s, e) => ShowInPopup(new Plugins.CalculatorPanel(), LocalizationManager.IsArabic ? "\u062d\u0627\u0633\u0628\u0629" : "Calculator", 380, 580);
            rightPanel.Controls.Add(pbCalc);

            // Backup
            PictureBox pbBackup = new PictureBox { Size = new Size(42, 42), Location = new Point(w - 385, 4), SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("backup"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbBackup);
            pbBackup.Click += (s, e) => ShowInPopup(new Plugins.BackupPanel(_pluginContext), LocalizationManager.IsArabic ? "\u0646\u0633\u062e\u0629 \u0627\u062d\u062a\u064a\u0627\u0637\u064a\u0629" : "Backup & Restore", 520, 500);
            rightPanel.Controls.Add(pbBackup);

            // Currencies
            PictureBox pbCurrencies = new PictureBox { Size = new Size(42, 42), Location = new Point(w - 435, 4), SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("currencies"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbCurrencies);
            pbCurrencies.Click += (s, e) => { using (var f = new Forms.CurrencySettingsForm()) f.ShowDialog(this); };
            rightPanel.Controls.Add(pbCurrencies);

            // Contact Us
            PictureBox pbContact = new PictureBox { Size = new Size(42, 42), Location = new Point(w - 485, 4), SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("contact_us"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbContact);
            ToolTip tt = new ToolTip(); tt.SetToolTip(pbContact, "Contact Us: softioservices@gmail.com");
            pbContact.Click += (s, e) => {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("mailto:softioservices@gmail.com") { UseShellExecute = true }); }
                catch { MessageHelper.ShowInfo("Contact us at: softioservices@gmail.com"); }
            };
            rightPanel.Controls.Add(pbContact);

            panel1.Controls.Add(rightPanel);
        }

        private void ShowInPopup(UserControl control, string title, int width, int height)
        {
            using (var f = new Forms.BaseModalForm())
            {
                f.TitleText = title;
                f.Width = width;
                f.Height = height + 70; // Header offset
                
                control.Dock = DockStyle.Fill;
                f.ContentPanel.Controls.Add(control);
                
                // BaseModalForm.OnLoad will call FitToContent() which will expand if needed
                f.ShowDialog(this);
            }
        }

        private void AddHeaderButton(Panel p, int x, string type, string name, Action click) {
            Button b = new Button { Name = name, Size = new Size(45, 38), Location = new Point(x, 6) };
            ThemeConfig.ApplyWindowControl(b, type); b.Click += (s, e) => click();
            p.Controls.Add(b);
        }

        private LockOverlay _lockOverlay;
        private void BtnLock_Click(object sender, EventArgs e) 
        { 
            if (_lockOverlay == null)
            {
                _lockOverlay = new LockOverlay();
                _lockOverlay.Unlocked += (s, ev) => {
                    _lockOverlay.Visible = false;
                    this.Controls.Remove(_lockOverlay);
                    _lockOverlay.Dispose();
                    _lockOverlay = null;
                };
            }
            
            if (!this.Controls.Contains(_lockOverlay))
            {
                this.Controls.Add(_lockOverlay);
                _lockOverlay.BringToFront();
            }
            _lockOverlay.Visible = true;
            _lockOverlay.Focus();
        }

        private void ShowNotifications(object sender, EventArgs e)
        {
            if(menuNotifications == null) { menuNotifications = new ContextMenuStrip(); ThemeConfig.ApplyModernMenuTheme(menuNotifications); }
            menuNotifications.Items.Clear();
            var notifications = _dashboardService.GetNotifications();
            bool isAr = LocalizationManager.IsArabic;
            if (notifications.Count == 0) menuNotifications.Items.Add(LocalizationManager.GetString("Main_NoNotifications")).Enabled = false;
            else {
                foreach (var n in notifications) {
                    var item = new ToolStripMenuItem($"{n.Title}: {n.Message}") { Tag = n, Font = ThemeConfig.StandardFont, Image = ThemeConfig.GetNuricon(n.Type == "LowStock" ? "warning" : "check") };
                    item.Click += (s, ev) => { 
                        if (n.Target == "btnInventory") ShowForm(partsForm); 
                        else if (n.Target == "btnCustomers") ShowForm(customersForm);
                        else if (n.Target == "btnSuppliers") ShowForm(suppliersForm);
                        else ShowForm(dashboardForm); 
                    };
                    menuNotifications.Items.Add(item);
                }
            }
            menuNotifications.Show(sender as Control, new Point(0, (sender as Control).Height));
        }

        private void InitializeNotificationSystem() {
            _dashboardService = new Services.DashboardService(); 
            var expenseService = new Services.ExpenseService();
            expenseService.ProcessRecurringExpenses(); // Check for month-end expenses

            _notificationTimer = new System.Windows.Forms.Timer { Interval = 30000 };
            _notificationTimer.Tick += (s, e) => RefreshNotificationBadge(); _notificationTimer.Start(); RefreshNotificationBadge();
        }

        private void RefreshNotificationBadge() {
            int oldCount = _lowStockCount; 
            _lowStockCount = _dashboardService.GetLowStockCount() + _dashboardService.GetPaymentRemindersCount();
            if (oldCount != _lowStockCount && pbNotification != null) pbNotification.Invalidate();
        }

        private void ApplyTheme() {
            this.Text = ThemeConfig.AppTitle; 
            
            label2.Text = ThemeConfig.AppTitle; 
            label2.Font = ThemeConfig.HeaderFont;
            label2.ForeColor = Color.White; // New: White on Blue
            label2.Padding = new Padding(15, 0, 0, 0);
            label2.Visible = false; // Hide text title, use logo instead

            panel1.Height = 70; // Increased height for larger logo
            // Header Logo
            if (panel1.Controls.Find("pbLogo", true).Length == 0)
            {
                PictureBox pbLogo = new PictureBox {
                    Name = "pbLogo",
                    Size = new Size(220, 60), 
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent
                };
                try { 
                    string logoPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "softio_logo.png");
                    if(System.IO.File.Exists(logoPath)) pbLogo.Image = Image.FromFile(logoPath); 
                } catch { }
                panel1.Controls.Add(pbLogo);
                pbLogo.BringToFront();
                pbLogo.Location = new Point(0, (panel1.Height - pbLogo.Height) / 2); // Far left
                pbLogo.MouseDown += Header_MouseDown;
            }

            var existingLogo = panel1.Controls.Find("pbLogo", true).FirstOrDefault() as PictureBox;
            if (existingLogo != null) {
                existingLogo.Location = new Point(0, (panel1.Height - existingLogo.Height) / 2); // Far left
            }

            label1.Text = "Welcome, " + UserSession.FullName;
            label1.Font = ThemeConfig.SmallBoldFont;
            label1.ForeColor = Color.FromArgb(180, 255, 255, 255); // Subtle white

            panel1.BackColor = Color.FromArgb(25, 118, 210); // Deep Blue Header
            panel1.Paint += (s, e) => {
                // No border needed for deep blue header
            };

            panel2.BackColor = Color.FromArgb(248, 250, 252); // Light Gray Sidebar
            panel3.BackColor = ThemeConfig.BackgroundColor;
            
            itemAddUser.Click += ItemAddUser_Click; 
            itemLicenseInfo.Click += ItemLicenseInfo_Click; 
            itemLogout.Click += ItemLogout_Click; 
            btnLock.Click += BtnLock_Click;
        }

        private Button CreateNavigationButton(string text, string iconName, EventHandler clickHandler) {
            SidebarButton btn = new SidebarButton { Height = 50, Dock = DockStyle.Top, Text = "  " + text, FlatStyle = FlatStyle.Flat };
            Image icon = ThemeConfig.GetNuricon(iconName);
            if (icon != null) btn.Image = ResizeImage(icon, 22, 22);
            ThemeConfig.ApplySidebarButtonIcon(btn, btn.Image, false);
            btn.Click += clickHandler; btn.Click += (s, e) => HighlightSelectedButton(btn);
            return btn;
        }

        private Button selectedButton = null;
        private void HighlightSelectedButton(Button btn) {
            if (selectedButton == btn) return;
            if (selectedButton != null) { 
                ThemeConfig.ApplySidebarButton(selectedButton, false); 
                selectedButton.Tag = false; 
                selectedButton.Paint -= DrawSelectionBorder; 
                selectedButton.Invalidate(); // Immediately clear old highlight
            }
            selectedButton = btn; 
            ThemeConfig.ApplySidebarButton(selectedButton, true); 
            selectedButton.Tag = true;
            selectedButton.Paint += DrawSelectionBorder; 
            selectedButton.Invalidate(); // Show new highlight
            selectedButton.Update(); // Force immediate repaint for responsiveness
        }

        private void DrawSelectionBorder(object sender, PaintEventArgs e) {
            if (sender is Button btn && btn.Tag != null && (bool)btn.Tag)
                using (Pen pen = new Pen(ThemeConfig.PrimaryColor, 5)) e.Graphics.DrawLine(pen, 0, 0, 0, btn.Height);
        }



        private void ShowForm(UserControl form) {
            foreach(Control c in panel3.Controls) if(c is UserControl) c.Visible = false;
            form.Visible = true; form.BringToFront();
            if (form is GenericInventorySystem.Forms.DashboardForm dash) dash.RefreshDashboard();
        }

        private void ItemAddUser_Click(object sender, EventArgs e) { new Forms.AddUserForm().ShowDialog(this); }
        private void ItemLicenseInfo_Click(object sender, EventArgs e) { new Forms.LicenseInfoForm().ShowDialog(this); }
        private void ItemLogout_Click(object sender, EventArgs e) { button3_Click_1(sender, e); }
        private void button1_Click(object sender, EventArgs e) { ShowForm(dashboardForm); }
        private void ApplyPermissions()
        {
            bool isAdmin = UserSession.IsAdmin;
            bool isStaff = UserSession.Role == "Staff";
            bool isAccountant = UserSession.Role == "Accountant";
            
            // Sidebar Buttons Hide Logic
            SetNavVisibility("btnReports", isAdmin);
            SetNavVisibility("btnCurrencies", isAdmin);
            SetNavVisibility("btnHistory", isAdmin || isAccountant);
            SetNavVisibility("btnPO", isAdmin || isAccountant);

            if (isStaff)
            {
                SetNavVisibility("btnCustomers", false);
                SetNavVisibility("btnSuppliers", false);
            }

            if (isAccountant)
            {
                // Hide most except POS and History/PO maybe? 
                // Based on previous logic:
                string[] toHide = { "Dashboard_btn", "btnInventory", "btnCustomers", "btnSuppliers", "btnReports", "btnQuotations", "btnCurrencies" };
                foreach (string name in toHide) SetNavVisibility(name, false);

                // Auto-show POS
                var btnPOS = this.Controls.Find("btnPOS", true);
                if (btnPOS.Length > 0) { ShowForm(posForm); HighlightSelectedButton((Button)btnPOS[0]); }
            }

            if (!isAdmin && pbUserAvatar != null) pbUserAvatar.Visible = false;
            label1.Text = string.Format(LocalizationManager.GetString("WelcomeUser") ?? "Welcome, {0} ({1})", UserSession.FullName, UserSession.Role);
        }

        private void SetNavVisibility(string name, bool visible) {
            var ctrls = this.Controls.Find(name, true);
            if (ctrls.Length > 0) ctrls[0].Visible = visible;
        }

        private void button3_Click_1(object sender, EventArgs e) {
            if(MessageHelper.ConfirmAction(LocalizationManager.GetString("Msg_ConfirmLogout") ?? "Logout?")) { new LoginForm().Show(); this.Hide(); }
        }

        [System.Runtime.InteropServices.DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void Header_MouseDown(object sender, MouseEventArgs e) { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(this.Handle, 0x112, 0xf012, 0); } }

        private Image ResizeImage(Image img, int width, int height)
        {
            Bitmap b = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, width, height);
            }
            return b;
        }
    }
}
