using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using Shaheen_InventoryManagement_Android.Controls;
using Shaheen_InventoryManagement_Android.Helpers;
using Shaheen_InventoryManagement_Android.Services;

namespace Shaheen_InventoryManagement_Android
{
    public partial class MainForm : Form
    {
        // NOTE: WS_EX_COMPOSITED removed — it causes see-through rendering artifacts
        // on borderless maximized WinForms windows (other apps show behind the form).
        private Forms.PartsForm partsForm;
        private Shaheen_InventoryManagement_Android.Forms.DashboardForm dashboardForm;
        private Shaheen_InventoryManagement_Android.Forms.HistoryForm historyForm;
        private Forms.POSForm posForm;
        private Forms.UserProfileControl userProfileControl;

        private UserControl activeForm = null;
        private UserControl previousForm = null;

        // Header Controls
        private PictureBox pbNotification;
        private PictureBox pbUserAvatar;
        private ContextMenuStrip menuNotifications;


        private Services.DashboardService _dashboardService;
        private System.Windows.Forms.Timer _notificationTimer;
        private int _alertCount = 0;
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
                CurrentUser = UserSession.Username,
                UserRole = UserSession.Role,
                IsAdmin = UserSession.IsAdmin,
                CheckLicense = (key) => Helpers.LicenseManager.IsFeatureEnabled(key),
                ShowSuccess = (msg) => MessageHelper.ShowSuccess(msg),
                ShowError = (msg) => MessageHelper.ShowError(msg),
                ShowInfo = (msg) => MessageHelper.ShowInfo(msg),
                AddTab = (tabTitle, iconName, tabOrder, contentFactory, tabId) =>
                {
                    // Filter out Calculator and Backup from sidebar as they are now in the header
                    if (tabTitle.Contains("Calculator") || tabTitle.Contains("Backup") || tabTitle.Contains("\u062d\u0627\u0633\u0628\u0629") || tabTitle.Contains("\u0646\u0633\u062e\u0629")) return;

                    if (this.InvokeRequired) this.Invoke((Action)(() => AddPluginTab(tabTitle, iconName, contentFactory, pnlNav, tabId)));
                    else AddPluginTab(tabTitle, iconName, contentFactory, pnlNav, tabId);
                },
                AddMenuItem = (group, item) =>
                {
                    if (this.InvokeRequired) this.Invoke((Action)(() => AddPluginMenuItem(group, item)));
                    else AddPluginMenuItem(group, item);
                }
            };
            Helpers.Plugins.PluginManager.DiscoverAndLoad(_pluginContext);
        }

        private void AddPluginTab(string tabTitle, string iconName, Func<UserControl> contentFactory, Panel pnlNav, string tabId)
        {
            UserControl cachedContent = null;
            Button btn = CreateNavigationButton(tabTitle, iconName, (s, e) =>
            {
                if (cachedContent == null)
                {
                    cachedContent = contentFactory();
                    ThemeConfig.ApplyGlobalTheme(cachedContent);
                    cachedContent.Dock = DockStyle.Fill;
                    panel3.Controls.Add(cachedContent);
                }
                ShowForm(cachedContent);
            });
            btn.Dock = DockStyle.Top;
            btn.Margin = new Padding(0);
            if (!string.IsNullOrEmpty(tabId)) btn.Name = tabId;
            if (pnlNav != null) { pnlNav.Controls.Add(btn); btn.BringToFront(); }
        }

        private void AddPluginMenuItem(string group, Helpers.Plugins.PluginMenuItem item)
        {
            ContextMenuStrip strip = panel1.Tag as ContextMenuStrip;
            if (strip == null)
            {
                strip = new ContextMenuStrip();
                ThemeConfig.ApplyModernMenuTheme(strip);
                panel1.Tag = strip;
            }
            ToolStripMenuItem groupMenu = null;
            foreach (ToolStripItem si in strip.Items)
            {
                if (si is ToolStripMenuItem tsm && tsm.Text == group) { groupMenu = tsm; break; }
            }
            if (groupMenu == null)
            {
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
            UpdateNavText("btnPOS", "Nav_POS");
            UpdateNavText("btnHistory", "Nav_History");

            button3.Text = "  " + L("Nav_Logout");
            if (itemLogout != null) itemLogout.Text = L("Nav_Logout");

            var pnlHeaderIcons = this.Controls.Find("rightPanel", true).FirstOrDefault() as Panel;
            if (pnlHeaderIcons != null)
            {
                if (isAr)
                {
                    panel2.Dock = DockStyle.Right;
                    pnlHeaderIcons.Dock = DockStyle.Left;
                }
                else
                {
                    panel2.Dock = DockStyle.Left;
                    pnlHeaderIcons.Dock = DockStyle.Right;
                }
            }
            label2.Visible = false; // Forced hide to prevent clipping
            label2.Location = isAr ? new Point(panel1.Width - label2.Width - 10, (panel1.Height - label2.Height) / 2) : new Point(10, (panel1.Height - label2.Height) / 2);

            var lblFooterVersion = this.Controls.Find("lblFooterVersion", true).FirstOrDefault() as Label;
            if (lblFooterVersion != null)
                lblFooterVersion.Text = L("Nav_MainTitle") + " | Version 1.0.2 | (c) 2026 Softio Services";
            var lblFooterDeveloper = this.Controls.Find("lblFooterDeveloper", true).FirstOrDefault() as Label;
            if (lblFooterDeveloper != null)
                lblFooterDeveloper.Text = isAr ? "تطوير سوفتيو" : "Developed by Softio";
        }

        private void UpdateNavText(string name, string key)
        {
            var btns = this.Controls.Find(name, true);
            if (btns.Length > 0) btns[0].Text = "  " + LocalizationManager.GetString(key);
        }

        private void InitializeNavigation()
        {
            ThemeConfig.ApplyFormIcon(this);
            Panel pnlNav = new Panel { Name = "pnlNav", Dock = DockStyle.Fill, Padding = new Padding(0), BackColor = Color.Transparent, AutoScroll = true };
            panel2.Controls.Add(pnlNav);
            pnlNav.BringToFront();

            // Dedicated Logo Container to prevent layout overrides and ensure margins
            Panel pnlLogoContainer = new Panel
            {
                Name = "pnlLogoContainer",
                Dock = DockStyle.Top,
                Height = 160, // Increased height for more gaps
                BackColor = Color.Transparent,
                Padding = new Padding(0, 35, 0, 25) // 35px top margin from header, 25px bottom margin from menu
            };
            pnlNav.Controls.Add(pnlLogoContainer);
            pnlLogoContainer.BringToFront();

            PictureBox pbSidebarLogo = new PictureBox
            {
                Name = "pbSidebarLogo",
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            try
            {
                string logoPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "logo.png");
                if (System.IO.File.Exists(logoPath)) pbSidebarLogo.Image = Image.FromFile(logoPath);
            }
            catch { }
            pnlLogoContainer.Controls.Add(pbSidebarLogo);
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
                dashboardForm = new Shaheen_InventoryManagement_Android.Forms.DashboardForm { Dock = DockStyle.Fill };
                ThemeConfig.ApplyGlobalTheme(dashboardForm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Dashboard init error: " + ex.Message);
                dashboardForm = new Shaheen_InventoryManagement_Android.Forms.DashboardForm();
                dashboardForm.Dock = DockStyle.Fill;
            }
            panel3.Controls.Add(dashboardForm);
            panel3.Controls.Add(dashboardForm);

            // Consolidate Dashboard Button logic to avoid redundant subscriptions and visual lag
            Dashboard_btn.Click -= button1_Click;
            Dashboard_btn.Click += (s, e) =>
            {
                ShowForm(dashboardForm);
                HighlightSelectedButton(Dashboard_btn);
            };

            Image dashIcon = ThemeConfig.GetNuricon("dashboard");
            ThemeConfig.ApplySidebarButtonIcon(Dashboard_btn, dashIcon != null ? ResizeImage(dashIcon, 18, 18) : null, false);
            Dashboard_btn.Text = "  " + LocalizationManager.GetString("Nav_Dashboard");

            // Forms Setup
            partsForm = InitializeForm<Forms.PartsForm>();
            posForm = InitializeForm<Forms.POSForm>();
            historyForm = InitializeForm<Forms.HistoryForm>();
            userProfileControl = InitializeForm<Forms.UserProfileControl>();

            // Navigation Buttons
            Dashboard_btn.Height = 50;
            Dashboard_btn.Width = 225; // Force exact width matching panel2
            Dashboard_btn.Margin = new Padding(0);
            Dashboard_btn.Dock = DockStyle.Top;
            Dashboard_btn.Text = "  " + LocalizationManager.GetString("Nav_Dashboard");
            Dashboard_btn.Image = ResizeImage(ThemeConfig.GetNuricon("dashboard"), 18, 18);
            ThemeConfig.ApplySidebarButtonIcon(Dashboard_btn, Dashboard_btn.Image, false);
            Dashboard_btn.BringToFront(); // Place below logo
            pnlNav.Controls.Add(Dashboard_btn);
            Dashboard_btn.BringToFront();

            bool isAdmin = UserSession.IsAdmin;
            bool isAccountant = UserSession.IsAccountant;
            bool isWorker = UserSession.IsStaff;

            // All roles can access core modules
            if (isAdmin || isWorker || isAccountant) AddNavButton(pnlNav, "Inventory", "inventory", "btnInventory", () => ShowForm(partsForm));
            if (isAdmin || isWorker || isAccountant) AddNavButton(pnlNav, "POS / Checkout", "pos", "btnPOS", () => ShowForm(posForm));
            if (isAdmin || isWorker || isAccountant) AddNavButton(pnlNav, "History", "history", "btnHistory", () => { historyForm.LoadHistory(); ShowForm(historyForm); });

            ShowForm(dashboardForm);
            HighlightSelectedButton(Dashboard_btn);
        }

        private T InitializeForm<T>() where T : UserControl, new()
        {
            T f = new T { Dock = DockStyle.Fill, Visible = false };
            ThemeConfig.ApplyGlobalTheme(f);
            panel3.Controls.Add(f);
            return f;
        }

        private void AddNavButton(Panel pnl, string text, string icon, string name, Action clickAction)
        {
            Button btn = CreateNavigationButton(text, icon, (s, e) => clickAction());
            btn.Name = name; btn.Dock = DockStyle.Top; btn.Margin = new Padding(0);
            pnl.Controls.Add(btn);
            btn.BringToFront(); // Stack below previous items
        }

        private void RefineNavigationLayout()
        {
            panel2.Controls.Remove(label4); label4.Visible = false;
            // Re-order panel2 to ensure pnlNav fills the remaining space
            Panel pnlNav = panel2.Controls.Find("pnlNav", true).FirstOrDefault() as Panel;
            if (pnlNav != null) panel2.Controls.Remove(pnlNav);

            // Now re-add pnlNav to fill the REMAINING space
            if (pnlNav != null)
            {
                panel2.Controls.Add(pnlNav);
                pnlNav.Dock = DockStyle.Fill;
                pnlNav.BringToFront();
            }

            // Logout Button - Moved to the extreme bottom of the sidebar
            button3.Parent = panel2;
            button3.Dock = DockStyle.Bottom;
            button3.SendToBack(); // Puts it at the absolute bottom edge
            button3.Height = 50;
            button3.Text = "  " + LocalizationManager.GetString("Nav_Logout");
            button3.ForeColor = ThemeConfig.DangerColor;
            Image logoutIcon = ThemeConfig.GetNuricon("logout");
            if (logoutIcon != null) { button3.Image = ResizeImage(logoutIcon, 22, 22); button3.ImageAlign = ContentAlignment.MiddleLeft; button3.TextImageRelation = TextImageRelation.ImageBeforeText; }
            button3.TextAlign = ContentAlignment.MiddleLeft; button3.Padding = new Padding(LocalizationManager.IsArabic ? 0 : 15, 0, LocalizationManager.IsArabic ? 15 : 0, 0);
            button3.Font = Dashboard_btn.Font;
            button3.FlatAppearance.MouseOverBackColor = ThemeConfig.DangerLight;

            // Logout button is docked to the bottom.

            SetupHeaderIcons();
            SetupFooter();
        }

        private void SetupFooter()
        {
            Panel pnlFooter = new Panel
            {
                Name = "pnlFooter",
                Dock = DockStyle.Bottom,
                Height = 20,
                BackColor = ThemeConfig.SurfaceColor,
                Padding = new Padding(8, 0, 8, 0)
            };

            TableLayoutPanel tlpFooter = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = ThemeConfig.SurfaceColor
            };
            tlpFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tlpFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpFooter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Label lblVersion = new Label
            {
                Name = "lblFooterVersion",
                Text = LocalizationManager.GetString("Nav_MainTitle") + " | Version 1.0.2 | (c) 2026 Softio Services",
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 7f),
                ForeColor = ThemeConfig.TextColorDark,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty,
                AutoEllipsis = true
            };

            Label lblDeveloper = new Label
            {
                Name = "lblFooterDeveloper",
                Text = LocalizationManager.IsArabic ? "تطوير سوفتيو" : "Developed by Softio",
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 7f, FontStyle.Italic),
                ForeColor = ThemeConfig.PrimaryColor,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = Padding.Empty,
                AutoEllipsis = true
            };

            tlpFooter.Controls.Add(lblVersion, 0, 0);
            tlpFooter.Controls.Add(lblDeveloper, 1, 0);
            pnlFooter.Controls.Add(tlpFooter);

            // In WinForms with Dock, controls are laid out in reverse z-order.
            // Add footer BEFORE panel3 so Dock=Bottom is claimed before Dock=Fill.
            this.Controls.Add(pnlFooter);
            this.Controls.Add(panel3);
            this.Controls.Add(panel2);
            this.Controls.Add(panel1);
        }

        private void SetupHeaderIcons()
        {
            // Use Dock=Right so the panel always stretches correctly at any screen width.
            // We position icons from the right edge using fixed offsets.
            const int iconAreaWidth = 250;
            Panel rightPanel = new Panel { Name = "rightPanel", Width = iconAreaWidth, BackColor = Color.Transparent, Dock = DockStyle.Right };
            int w = iconAreaWidth;
            AddHeaderButton(rightPanel, w - 45, "Close", "btnWinClose", () => Application.Exit());
            AddHeaderButton(rightPanel, w - 90, "Maximize", "btnWinMax", () => { this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized; });
            AddHeaderButton(rightPanel, w - 135, "Minimize", "btnWinMin", () => this.WindowState = FormWindowState.Minimized);

            pbUserAvatar = new PictureBox { Size = new Size(42, 42), Location = new Point(w - 185, 4), SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("user"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbUserAvatar);
            pbUserAvatar.Click += (s, e) =>
            {
                ShowForm(userProfileControl);
                HighlightSelectedButton(null);
            };
            rightPanel.Controls.Add(pbUserAvatar);

            pbNotification = new PictureBox { Size = new Size(42, 42), Location = new Point(w - 235, 4), SizeMode = PictureBoxSizeMode.Zoom, Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("bell"), Color.White) };
            ThemeConfig.ApplyHeaderIconStyle(pbNotification);
            pbNotification.Paint += (s, e) => { if (_alertCount > 0) { e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; using (SolidBrush b = new SolidBrush(ThemeConfig.DangerColorBright)) e.Graphics.FillEllipse(b, 24, 6, 8, 8); } };
            pbNotification.Click += (s, e) => ShowNotifications(s, e);
            rightPanel.Controls.Add(pbNotification);

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

        private void AddHeaderButton(Panel p, int x, string type, string name, Action click)
        {
            Button b = new Button { Name = name, Size = new Size(45, 38), Location = new Point(x, 6) };
            ThemeConfig.ApplyWindowControl(b, type); b.Click += (s, e) => click();
            p.Controls.Add(b);
        }



        private void ShowNotifications(object sender, EventArgs e)
        {
            if (menuNotifications == null) { menuNotifications = new ContextMenuStrip(); ThemeConfig.ApplyModernMenuTheme(menuNotifications); }
            menuNotifications.Items.Clear();
            var notifications = _dashboardService.GetNotifications();
            bool isAr = LocalizationManager.IsArabic;
            if (notifications.Count == 0) menuNotifications.Items.Add(LocalizationManager.GetString("Main_NoNotifications")).Enabled = false;
            else
            {
                foreach (var n in notifications)
                {
                    var item = new ToolStripMenuItem($"{n.Title}: {n.Message}") { Tag = n, Font = ThemeConfig.StandardFont, Image = ThemeConfig.GetNuricon(n.Type == "LowStock" ? "warning" : "check") };
                    item.Click += (s, ev) =>
                    {
                        if (n.Target == "btnInventory") ShowForm(partsForm);
                        else if (n.Target == "btnCustomers") ClickNavButton("btnCustomers");
                        else if (n.Target == "btnSuppliers") ClickNavButton("btnSuppliers");
                        else ShowForm(dashboardForm);
                    };
                    menuNotifications.Items.Add(item);
                }
            }
            menuNotifications.Show(sender as Control, new Point(0, (sender as Control).Height));
        }

        private void InitializeNotificationSystem()
        {
            _dashboardService = new Services.DashboardService();

            _notificationTimer = new System.Windows.Forms.Timer { Interval = 30000 };
            _notificationTimer.Tick += (s, e) => RefreshNotificationBadge(); _notificationTimer.Start(); RefreshNotificationBadge();
        }

        private void RefreshNotificationBadge()
        {
            int oldCount = _alertCount;
            _alertCount = _dashboardService.GetLowStockCount();
            if (oldCount != _alertCount && pbNotification != null) pbNotification.Invalidate();
        }

        private void ApplyTheme()
        {
            this.Text = ThemeConfig.AppTitle;

            label2.Text = ThemeConfig.AppTitle;
            label2.Font = ThemeConfig.HeaderFont;
            label2.ForeColor = Color.White; // New: White on Blue
            label2.Visible = false; // Explicitly hide the title
            panel1.Height = 60; // Decreased height
            panel1.Padding = Padding.Empty;
            panel1.Margin = Padding.Empty;
            panel1.BorderStyle = BorderStyle.None; // Remove border that adds padding
            label1.Text = string.Format(LocalizationManager.GetString("WelcomeUser", "Welcome, {0}"), UserSession.FullName, UserSession.Role);
            label1.Font = ThemeConfig.SmallBoldFont;
            label1.ForeColor = Color.FromArgb(180, 255, 255, 255); // Subtle white

            panel1.BackColor = ThemeConfig.HeaderColor; // Theme Red Header
            panel1.Paint += (s, e) =>
            {
                // No border needed for deep blue header
            };

            panel2.BackColor = Color.FromArgb(248, 250, 252); // Light Gray Sidebar
            panel3.BackColor = ThemeConfig.BackgroundColor;

            itemLogout.Click += ItemLogout_Click;
        }

        private Button CreateNavigationButton(string text, string iconName, EventHandler clickHandler)
        {
            SidebarButton btn = new SidebarButton
            {
                Height = 50,
                Dock = DockStyle.Top,
                Text = "  " + text,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Image icon = ThemeConfig.GetNuricon(iconName);
            if (icon != null) btn.Image = ResizeImage(icon, 18, 18);
            ThemeConfig.ApplySidebarButtonIcon(btn, btn.Image, false);
            btn.Click += clickHandler; btn.Click += (s, e) => HighlightSelectedButton(btn);
            return btn;
        }

        private Button selectedButton = null;
        private void HighlightSelectedButton(Button btn)
        {
            if (selectedButton == btn) return;
            if (selectedButton != null)
            {
                ThemeConfig.ApplySidebarButtonIcon(selectedButton, selectedButton.Image, false);
                selectedButton.Tag = false;
                selectedButton.Paint -= DrawSelectionBorder;
                selectedButton.Invalidate();
            }
            selectedButton = btn;
            ThemeConfig.ApplySidebarButtonIcon(selectedButton, selectedButton.Image, true);
            selectedButton.Tag = true;
            selectedButton.Paint += DrawSelectionBorder;
            selectedButton.Invalidate();
            selectedButton.Update();
        }

        private void DrawSelectionBorder(object sender, PaintEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && (bool)btn.Tag)
                using (Pen pen = new Pen(ThemeConfig.PrimaryColor, 5)) e.Graphics.DrawLine(pen, 0, 0, 0, btn.Height);
        }



        private void ClickNavButton(string id)
        {
            var btns = this.Controls.Find(id, true);
            if (btns.Length > 0 && btns[0] is Button btn) { btn.PerformClick(); }
        }

        private void ShowForm(UserControl form)
        {
            if (activeForm != form)
            {
                previousForm = activeForm;
                activeForm = form;
            }
            foreach (Control c in panel3.Controls) if (c is UserControl) c.Visible = false;
            form.Visible = true; form.BringToFront();
            panel3.Focus(); // Focus the main panel to prevent auto-selecting the first control (like search bar) in the UserControl
            if (form is Shaheen_InventoryManagement_Android.Forms.DashboardForm dash) dash.RefreshDashboard();
            if (form is Forms.UserProfileControl profile) profile.LoadData();
        }

        public void NavigateBack()
        {
            if (previousForm != null)
            {
                ShowForm(previousForm);
                
                Button btnToHighlight = null;
                if (previousForm == dashboardForm) btnToHighlight = Dashboard_btn;
                else if (previousForm == partsForm) btnToHighlight = this.Controls.Find("btnInventory", true).FirstOrDefault() as Button;
                else if (previousForm == posForm) btnToHighlight = this.Controls.Find("btnPOS", true).FirstOrDefault() as Button;
                else if (previousForm == historyForm) btnToHighlight = this.Controls.Find("btnHistory", true).FirstOrDefault() as Button;
                
                if (btnToHighlight != null) HighlightSelectedButton(btnToHighlight);
            }
            else
            {
                ShowForm(dashboardForm);
                HighlightSelectedButton(Dashboard_btn);
            }
        }

        public void PerformLogout(bool isSwitchAccount = false)
        {
            DatabaseHelper.LogUserAction(UserSession.Username, UserSession.FullName, "Logged out");
            UserSession.Clear();
            new LoginForm().Show();
            this.Hide();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (posForm != null && posForm.Visible)
            {
                if (posForm.HandleKeyPress(keyData))
                {
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ItemLogout_Click(object sender, EventArgs e) { button3_Click_1(sender, e); }
        private void button1_Click(object sender, EventArgs e) { ShowForm(dashboardForm); }
        private void ApplyPermissions()
        {
            bool isAdmin = UserSession.IsAdmin;
            bool isStaff = UserSession.Role == "Staff";
            bool isAccountant = UserSession.Role == "Accountant";

            if (isAccountant)
            {
                // Auto-show POS
                var btnPOS = this.Controls.Find("btnPOS", true);
                if (btnPOS.Length > 0) { ShowForm(posForm); HighlightSelectedButton((Button)btnPOS[0]); }
            }

            if (!isAdmin && pbUserAvatar != null) pbUserAvatar.Visible = false;
            label1.Text = string.Format(LocalizationManager.GetString("WelcomeUser", "Welcome, {0} ({1})"), UserSession.FullName, UserSession.Role);
        }

        private void SetNavVisibility(string name, bool visible)
        {
            var ctrls = this.Controls.Find(name, true);
            if (ctrls.Length > 0) ctrls[0].Visible = visible;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (MessageHelper.ConfirmAction(LocalizationManager.GetString("Msg_ConfirmLogout", "Logout?"))) { new LoginForm().Show(); this.Hide(); }
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


