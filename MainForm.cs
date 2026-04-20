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
            var pnlNav = this.Controls.Find("pnlNav", true).FirstOrDefault() as FlowLayoutPanel;
            var context = new Helpers.Plugins.PluginContext
            {
                ConnectionString = DatabaseConfig.ConnectionString,
                CurrentUser      = UserSession.Username,
                IsAdmin          = UserSession.IsAdmin,
                CheckLicense     = (key) => Helpers.LicenseManager.IsFeatureEnabled(key),
                ShowSuccess      = (msg) => MessageHelper.ShowSuccess(msg),
                ShowError        = (msg) => MessageHelper.ShowError(msg),
                ShowInfo         = (msg) => MessageHelper.ShowInfo(msg),
                AddTab = (tabTitle, iconName, tabOrder, contentFactory) => {
                    if (this.InvokeRequired) this.Invoke((Action)(() => AddPluginTab(tabTitle, iconName, contentFactory, pnlNav)));
                    else AddPluginTab(tabTitle, iconName, contentFactory, pnlNav);
                },
                AddMenuItem = (group, item) => {
                    if (this.InvokeRequired) this.Invoke((Action)(() => AddPluginMenuItem(group, item)));
                    else AddPluginMenuItem(group, item);
                }
            };
            Helpers.Plugins.PluginManager.DiscoverAndLoad(context);
        }

        private void AddPluginTab(string tabTitle, string iconName, Func<UserControl> contentFactory, FlowLayoutPanel pnlNav)
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
            if (pnlNav != null) pnlNav.Controls.Add(btn);
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

            button3.Text = "  " + L("Nav_Logout");
            if(itemAddUser != null) itemAddUser.Text = L("Nav_AddUser");
            if(itemLicenseInfo != null) itemLicenseInfo.Text = L("Nav_LicenseInfo");
            if(itemLogout != null) itemLogout.Text = L("Nav_Logout");
            if(btnUserProfile != null) btnUserProfile.Text = "  " + L("Nav_UserProfile");
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
            label2.Location = isAr ? new Point(panel1.Width - label2.Width - 20, (panel1.Height - label2.Height) / 2) : new Point(20, (panel1.Height - label2.Height) / 2);
        }

        private void UpdateNavText(string name, string key) {
            var btns = this.Controls.Find(name, true);
            if (btns.Length > 0) btns[0].Text = "  " + LocalizationManager.GetString(key);
        }

        private void InitializeNavigation()
        {
            ThemeConfig.ApplyFormIcon(this);
            FlowLayoutPanel pnlNav = new FlowLayoutPanel { Name = "pnlNav", Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0), BackColor = Color.Transparent };
            panel2.Controls.Add(pnlNav);
            pnlNav.BringToFront();

            PictureBox pbSidebarLogo = new PictureBox { Size = new Size(140, 140), SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding((panel2.Width - 140) / 2, 20, 0, 20) };
            try { string logoPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "inventory_logo.png"); if(System.IO.File.Exists(logoPath)) pbSidebarLogo.Image = Image.FromFile(logoPath); } catch { }
            pnlNav.Controls.Add(pbSidebarLogo);

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
            Dashboard_btn.Click -= button1_Click;
            Dashboard_btn.Click += (s, e) => { ShowForm(dashboardForm); HighlightSelectedButton(Dashboard_btn); };
            // Style Dashboard_btn exactly like every other nav button via AddNavButton
            Image dashIcon = ThemeConfig.GetNuricon("dashboard");
            ThemeConfig.ApplySidebarButtonIcon(Dashboard_btn, dashIcon != null ? ResizeImage(dashIcon, 22, 22) : null, false);
            Dashboard_btn.Text = "  " + LocalizationManager.GetString("Nav_Dashboard");
            Dashboard_btn.Click += (s, e) => HighlightSelectedButton(Dashboard_btn);

            // Forms Setup
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
            pnlNav.Controls.Add(Dashboard_btn);
            
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
                AddNavButton(pnlNav, "Monthly Expenses", "history", "btnExpenses", () => { monthlyExpensesForm.LoadData(); ShowForm(monthlyExpensesForm); });
                AddNavButton(pnlNav, "Reports", "reports", "btnReports", () => { reportsForm.RefreshData(); ShowForm(reportsForm); });
                AddNavButton(pnlNav, "History", "history", "btnHistory", () => { historyForm.LoadHistory(); ShowForm(historyForm); });
            }

            // Admin Only
            if (isAdmin)
            {
                AddNavButton(pnlNav, "Quotations", "quotations", "btnQuotations", () => { quotationsForm.LoadQuotations(); ShowForm(quotationsForm); });
                AddNavButton(pnlNav, "Currencies", "currencies", "btnCurrencies", () => { using (var f = new Forms.CurrencySettingsForm()) f.ShowDialog(this); });
            }

            ShowForm(dashboardForm);
            HighlightSelectedButton(Dashboard_btn);
        }

        private T InitializeForm<T>() where T : UserControl, new() {
            T f = new T { Dock = DockStyle.Fill, Visible = false };
            panel3.Controls.Add(f);
            return f;
        }

        private void AddNavButton(FlowLayoutPanel pnl, string text, string icon, string name, Action clickAction) {
            Button btn = CreateNavigationButton(text, icon, (s, e) => clickAction());
            btn.Name = name; btn.Dock = DockStyle.Top; btn.Margin = new Padding(0);
            pnl.Controls.Add(btn);
        }

        private void RefineNavigationLayout()
        {
            panel2.Controls.Remove(label4); label4.Visible = false;
            panel2.Controls.Remove(btnUserProfile); btnUserProfile.Visible = false;
            button3.Dock = DockStyle.Bottom; button3.Height = 45; button3.Text = "  Logout"; button3.ForeColor = ThemeConfig.DangerColor;
            Image logoutIcon = ThemeConfig.GetNuricon("logout");
            if (logoutIcon != null) { button3.Image = ResizeImage(logoutIcon, 22, 22); button3.ImageAlign = ContentAlignment.MiddleLeft; button3.TextImageRelation = TextImageRelation.ImageBeforeText; }
            button3.TextAlign = ContentAlignment.MiddleLeft; button3.Padding = new Padding(15, 0, 0, 0); button3.Font = ThemeConfig.ButtonFont;
            button3.FlatAppearance.MouseOverBackColor = ThemeConfig.DangerLight;
            button3.BringToFront();
            SetupHeaderIcons();
            label3.Text = "x"; label3.Font = ThemeConfig.SubHeaderFont;
            label3.Location = new Point(panel1.Width - 40, (panel1.Height - 25) / 2); label3.BringToFront();
        }

        private void SetupHeaderIcons()
        {
            Panel rightPanel = new Panel { Name = "rightPanel", Size = new Size(350, 60), BackColor = Color.Transparent, Dock = DockStyle.Right };
            int w = rightPanel.Width;
            AddHeaderButton(rightPanel, w - 45, "Close", "btnWinClose", () => Application.Exit());
            AddHeaderButton(rightPanel, w - 90, "Maximize", "btnWinMax", () => { this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized; });
            AddHeaderButton(rightPanel, w - 135, "Minimize", "btnWinMin", () => this.WindowState = FormWindowState.Minimized);
            
            pbUserAvatar = new PictureBox { Size = new Size(38, 38), Location = new Point(w - 185, 11), Cursor = Cursors.Hand, SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.GetNuricon("user") };
            pbUserAvatar.Click += BtnUserProfile_Click;
            rightPanel.Controls.Add(pbUserAvatar);

            pbNotification = new PictureBox { Size = new Size(38, 38), Location = new Point(w - 235, 11), Cursor = Cursors.Hand, SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.GetNuricon("bell") };
            pbNotification.Paint += (s, e) => { if (_lowStockCount > 0) { e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; using (SolidBrush b = new SolidBrush(ThemeConfig.DangerColorBright)) e.Graphics.FillEllipse(b, 24, 6, 8, 8); } };
            pbNotification.Click += (s, e) => ShowNotifications(s, e);
            rightPanel.Controls.Add(pbNotification);

            btnLock.Location = new Point(w - 285, 11);
            btnLock.Size = new Size(38, 38);
            btnLock.Image = ResizeImage(ThemeConfig.GetNuricon("lock"), 22, 22);
            btnLock.FlatStyle = FlatStyle.Flat; btnLock.FlatAppearance.BorderSize = 0;
            rightPanel.Controls.Add(btnLock);

            panel1.Controls.Add(rightPanel);
        }

        private void AddHeaderButton(Panel p, int x, string type, string name, Action click) {
            Button b = new Button { Name = name, Size = new Size(45, 38), Location = new Point(x, 11) };
            ThemeConfig.ApplyWindowControl(b, type); b.Click += (s, e) => click();
            p.Controls.Add(b);
        }

        private void BtnLock_Click(object sender, EventArgs e) { using (var lockScreen = new Forms.LockScreenForm()) lockScreen.ShowDialog(); }

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
                    item.Click += (s, ev) => { if (n.Target == "btnInventory") ShowForm(partsForm); else ShowForm(dashboardForm); };
                    menuNotifications.Items.Add(item);
                }
            }
            menuNotifications.Show(sender as Control, new Point(0, (sender as Control).Height));
        }

        private void InitializeNotificationSystem() {
            _dashboardService = new Services.DashboardService(); _notificationTimer = new System.Windows.Forms.Timer { Interval = 30000 };
            _notificationTimer.Tick += (s, e) => RefreshNotificationBadge(); _notificationTimer.Start(); RefreshNotificationBadge();
        }

        private void RefreshNotificationBadge() {
            int oldCount = _lowStockCount; _lowStockCount = _dashboardService.GetLowStockCount();
            if (oldCount != _lowStockCount && pbNotification != null) pbNotification.Invalidate();
        }

        private void ApplyTheme() {
            this.Text = ThemeConfig.AppTitle; label2.Text = ThemeConfig.AppTitle; label1.Text = "Welcome, " + UserSession.FullName;
            panel1.BackColor = ThemeConfig.SurfaceColor; panel2.BackColor = Color.White; panel3.BackColor = ThemeConfig.BackgroundColor;
            itemAddUser.Click += ItemAddUser_Click; itemLicenseInfo.Click += ItemLicenseInfo_Click; itemLogout.Click += ItemLogout_Click; btnLock.Click += BtnLock_Click;
        }

        private Button CreateNavigationButton(string text, string iconName, EventHandler clickHandler) {
            SidebarButton btn = new SidebarButton { Height = 50, Width = 200, Text = "  " + text, FlatStyle = FlatStyle.Flat };
            Image icon = ThemeConfig.GetNuricon(iconName);
            if (icon != null) btn.Image = ResizeImage(icon, 22, 22);
            ThemeConfig.ApplySidebarButtonIcon(btn, btn.Image, false);
            btn.Click += clickHandler; btn.Click += (s, e) => HighlightSelectedButton(btn);
            return btn;
        }

        private Button selectedButton = null;
        private void HighlightSelectedButton(Button btn) {
            if (selectedButton != null) { ThemeConfig.ApplySidebarButton(selectedButton, false); selectedButton.Tag = false; selectedButton.Paint -= DrawSelectionBorder; }
            selectedButton = btn; ThemeConfig.ApplySidebarButton(selectedButton, true); selectedButton.Tag = true;
            selectedButton.Paint += DrawSelectionBorder; selectedButton.Invalidate();
        }

        private void DrawSelectionBorder(object sender, PaintEventArgs e) {
            if (sender is Button btn && btn.Tag != null && (bool)btn.Tag)
                using (Pen pen = new Pen(ThemeConfig.PrimaryColor, 4)) e.Graphics.DrawLine(pen, 0, 0, 0, btn.Height);
        }

        private void BtnUserProfile_Click(object sender, EventArgs e) {
            if (usersForm == null) { usersForm = InitializeForm<Forms.UsersForm>(); }
            ShowForm(usersForm); HighlightSelectedButton(btnUserProfile);
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

        private void label3_Click(object sender, EventArgs e) { Application.Exit(); }

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
