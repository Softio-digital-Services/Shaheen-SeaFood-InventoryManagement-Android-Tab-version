using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Shaheen_InventoryManagement_Android.Controls;
using Shaheen_InventoryManagement_Android.Data;
using Shaheen_InventoryManagement_Android.Helpers;
using Shaheen_InventoryManagement_Android.Services;

namespace Shaheen_InventoryManagement_Android.Forms
{
    public partial class POSForm : UserControl
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }
        // -- LEFT PANEL CONTROLS ----------------------------------------------
        private FlowLayoutPanel pnlProducts;   // product card grid
        private FlowLayoutPanel pnlChips;      // category chip strip
        private Shaheen_InventoryManagement_Android.Controls.ModernTextBox txtProductSearch;

        // -- RIGHT PANEL CONTROLS ---------------------------------------------
        private Panel pnlCartItems;        // scrollable cart rows
        private Shaheen_InventoryManagement_Android.Controls.ModernComboBox cmbCustomers;
        private Label lblOrderNum;
        private Label lblSubtotalVal, lblTaxVal, lblShippingVal, lblTotalVal;
        private CheckBox chkApplyVAT, chkApplyShipping;
        private NumericUpDown numShipping;
        private Button btnCheckout;
        private StatCard cardTodayOrders, cardTodaySales, cardPending;

        // -- STATE -------------------------------------------------------------
        private DataTable cartTable;
        private DashboardService _dashboardService;
        private string _activeCategory = null; // null = "All"
        private int _sessionOrderCount = 0;
        private int _currentPage = 1;
        private int _pageSize = 50;
        private Label lblPageInfo;
        private Shaheen_InventoryManagement_Android.Controls.ModernButton btnPrevPage;
        private Shaheen_InventoryManagement_Android.Controls.ModernButton btnNextPage;
        private DateTime _lastScanTime = DateTime.Now;
        private string _scanBuffer = "";
        private ShippingDetailsForm _shippingDetails = null;

        // ---------------------------------------------------------------------
        // CONSTRUCTOR
        // ---------------------------------------------------------------------
        public POSForm()
        {
            InitializeComponent();
            _dashboardService = new DashboardService();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
            ApplyPermissions();
        }

        // ---------------------------------------------------------------------
        // LIFECYCLE
        // ---------------------------------------------------------------------
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible && !this.DesignMode)
            {
                if (cartTable == null) InitializeCart();
                RefreshStats();
                LoadCustomers();
                LoadProducts();
                this.ActiveControl = null;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Form parent = this.FindForm();
            if (parent != null)
            {
                parent.KeyPreview = true;
                parent.KeyPress -= POSForm_KeyPress;
                parent.KeyPress += POSForm_KeyPress;
            }
        }

        // ---------------------------------------------------------------------
        // INITIALIZE COMPONENT  (layout)
        // ---------------------------------------------------------------------
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Size = new Size(1200, 800);
            this.BackColor = ThemeConfig.BackgroundColor;

            // -- Root split: 70% left | 30% right -------------------------------
            TableLayoutPanel tlpRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0),
                BackColor = ThemeConfig.BackgroundColor
            };
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(tlpRoot);

            // ------------------------------------------------------------------
            // LEFT PANEL ï¿½ product browser
            // ------------------------------------------------------------------
            // LEFT PANEL ï¿½ product browser
            // ------------------------------------------------------------------
            TableLayoutPanel tlpLeft = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = ThemeConfig.BackgroundColor,
                Padding = new Padding(16, 16, 8, 16)
            };
            // Row 0  Header (Title + Search + Actions)
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            // Row 1  Stat cards
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Absolute, 95F));
            // Row 2  Category section (title + cards)
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Absolute, 136F));
            // Row 3  Product grid
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            // Row 4  Pagination
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tlpRoot.Controls.Add(tlpLeft, 0, 0);

            // -- Header -------------------------------------------------------
            string pageTitle = LocalizationManager.GetString("POS_PageTitle");
            Label lblPageTitle = ThemeConfig.CreateStandardHeader(pageTitle == "POS_PageTitle" ? "Checkout" : pageTitle);

            txtProductSearch = new Shaheen_InventoryManagement_Android.Controls.ModernTextBox
            {
                IsSearch = true,
                ShowLabel = false,
                PlaceholderText = LocalizationManager.GetString("POS_SearchProducts", "Search products..."),
                Size = new Size(320, 35)
            };
            txtProductSearch.TextChanged += (s, ev) => LoadProducts(txtProductSearch.Text);

            Button btnManageDrafts = new Shaheen_InventoryManagement_Android.Controls.ModernButton { Text = "Manage Drafts", Cursor = Cursors.Hand, Height = 35, Width = 160 };
            Button btnAddShipping = new Shaheen_InventoryManagement_Android.Controls.ModernButton { Text = "Add Shipping Details", Cursor = Cursors.Hand, Height = 35, Width = 200 };

            ThemeConfig.ApplyPaletteButton(btnManageDrafts, Color.FromArgb(99, 102, 241)); // Indigo
            ThemeConfig.ApplyPaletteButton(btnAddShipping, Color.FromArgb(16, 185, 129)); // Emerald Green

            btnManageDrafts.Click += (s, ev) =>
            {
                string msg = LocalizationManager.GetString("Msg_ManageDraftsInHistory");
                if (msg == "Msg_ManageDraftsInHistory") msg = "Please navigate to the History tab to manage drafts.";
                MessageHelper.ShowInfo(msg);
            };
            btnAddShipping.Click += (s, ev) =>
            {
                if (_shippingDetails == null) _shippingDetails = new ShippingDetailsForm();
                if (_shippingDetails.ShowDialog() == DialogResult.OK)
                {
                    btnAddShipping.Text = "View Shipping Details";
                }
            };

            var actionButtons = new Control[] { btnManageDrafts, btnAddShipping };
            TableLayoutPanel tlpHeader = ThemeConfig.CreateGlobalFormHeader(lblPageTitle, txtProductSearch, actionButtons);
            tlpLeft.Controls.Add(tlpHeader, 0, 0);

            // -- Stat Cards ----------------------------------------------------
            TableLayoutPanel tlpStats = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 6),
                BackColor = Color.Transparent
            };
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            cardTodayOrders = new StatCard { Title = "Orders", Value = "0", IconImage = ThemeConfig.GetNuricon("pos"), ThemeColor = ThemeConfig.PrimaryColor, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0) };
            cardTodaySales = new StatCard { Title = "Sales", Value = "$0", IconImage = ThemeConfig.GetNuricon("revenue"), ThemeColor = ThemeConfig.SuccessColor, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0) };
            cardPending = new StatCard { Title = "Pending", Value = "0", IconImage = ThemeConfig.GetNuricon("orders"), ThemeColor = ThemeConfig.WarningColor, Dock = DockStyle.Fill, Margin = new Padding(0) };

            tlpStats.Controls.Add(cardTodayOrders, 0, 0);
            tlpStats.Controls.Add(cardTodaySales, 1, 0);
            tlpStats.Controls.Add(cardPending, 2, 0);

            tlpLeft.Controls.Add(tlpStats, 0, 1);

            // -- Category section (title bar + scrollable cards) -------------
            Panel pnlCategorySection = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 6)
            };
            tlpLeft.Controls.Add(pnlCategorySection, 0, 2);

            // Title row ï¿½ "Menu" label + prev/next arrows
            Panel pnlCatHeader = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent
            };
            string menuTitleTrans = LocalizationManager.GetString("POS_MenuTitle");
            Label lblCatTitle = new Label
            {
                Text = menuTitleTrans == "POS_MenuTitle" ? "Categories" : menuTitleTrans,
                Font = ThemeConfig.CardTitleFont,
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = true,
                Location = new Point(2, 4),
                BackColor = Color.Transparent
            };
            // Prev / Next scroll nav buttons
            Label btnCatNext = new Label
            {
                Text = ">",
                AutoSize = false,
                Size = new Size(30, 30),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Light", 14F),
                BackColor = Color.Transparent,
                ForeColor = ThemeConfig.SecondaryColor,
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label btnCatPrev = new Label
            {
                Text = "<",
                AutoSize = false,
                Size = new Size(30, 30),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Light", 14F),
                BackColor = Color.Transparent,
                ForeColor = ThemeConfig.SecondaryColor,
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlCatHeader.Resize += (s, ev) =>
            {
                // Align to the right edge with a small margin
                btnCatNext.Location = new Point(pnlCatHeader.Width - 30, 0);
                btnCatPrev.Location = new Point(pnlCatHeader.Width - 60, 0);
            };
            pnlCatHeader.Controls.AddRange(new Control[] { lblCatTitle, btnCatPrev, btnCatNext });
            pnlCategorySection.Controls.Add(pnlCatHeader);

            Panel pnlChipsWrapper = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            pnlCategorySection.Controls.Add(pnlChipsWrapper);
            pnlChipsWrapper.BringToFront();

            pnlChips = new FlowLayoutPanel
            {
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 2, 0, 0),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlChipsWrapper.Controls.Add(pnlChips);

            pnlChipsWrapper.Resize += (s, e) =>
            {
                // Make pnlChips taller than wrapper to push horizontal scrollbar out of view
                pnlChips.SetBounds(0, 0, pnlChipsWrapper.Width, pnlChipsWrapper.Height + SystemInformation.HorizontalScrollBarHeight + 10);
            };

            // Wire nav buttons to scroll the chip panel
            btnCatNext.Click += (s, e) =>
            {
                pnlChips.AutoScrollPosition = new Point(
                    Math.Min(-pnlChips.AutoScrollPosition.X + 180,
                             pnlChips.HorizontalScroll.Maximum), 0);
            };
            btnCatPrev.Click += (s, e) =>
            {
                pnlChips.AutoScrollPosition = new Point(
                    Math.Max(-pnlChips.AutoScrollPosition.X - 180, 0), 0);
            };
            // Enable horizontal-only scroll
            pnlChips.HorizontalScroll.Enabled = true;
            pnlChips.VerticalScroll.Enabled = false;
            pnlChips.AutoScroll = true;

            // -- Product grid ------------------------------------------------
            pnlProducts = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 8, 0, 0),
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            tlpLeft.Controls.Add(pnlProducts, 0, 3);

            Panel pnlPagination = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            btnPrevPage = new Shaheen_InventoryManagement_Android.Controls.ModernButton { Text = "< Prev", Size = new Size(80, 30), Location = new Point(0, 10), Cursor = Cursors.Hand };
            btnPrevPage.Click += (s, e) => { if (_currentPage > 1) { _currentPage--; LoadProducts(txtProductSearch.Text); } };
            ThemeConfig.ApplySecondaryButton(btnPrevPage);

            btnNextPage = new Shaheen_InventoryManagement_Android.Controls.ModernButton { Text = "Next >", Size = new Size(80, 30), Location = new Point(200, 10), Cursor = Cursors.Hand };
            btnNextPage.Click += (s, e) => { _currentPage++; LoadProducts(txtProductSearch.Text); };
            ThemeConfig.ApplySecondaryButton(btnNextPage);

            lblPageInfo = new Label { Text = "Page 1", AutoSize = false, Size = new Size(100, 30), Location = new Point(90, 10), TextAlign = ContentAlignment.MiddleCenter, Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.TextColorDark };

            pnlPagination.Controls.Add(btnPrevPage);
            pnlPagination.Controls.Add(lblPageInfo);
            pnlPagination.Controls.Add(btnNextPage);
            tlpLeft.Controls.Add(pnlPagination, 0, 4);

            // ------------------------------------------------------------------
            // RIGHT PANEL ï¿½ cart & summary
            // ------------------------------------------------------------------
            // RIGHT PANEL - cards layout
            // ------------------------------------------------------------------
            Panel pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent, // matches form background
                Padding = new Padding(12)
            };
            tlpRoot.Controls.Add(pnlRight, 1, 0);

            // Container for 3 cards
            TableLayoutPanel tlpRight = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            tlpRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            // Card 1: Order details and cart
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F)); // Gap
            // Card 2: Actions
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F)); // Gap
            // Card 3: Footer
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            pnlRight.Controls.Add(tlpRight);

            // Paint handler for drawing white rounded cards
            PaintEventHandler cardPaint = (s, pe) =>
            {
                pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pathCard = ThemeConfig.GetRoundedPathPublic(((Control)s).ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.White))
                {
                    pe.Graphics.FillPath(brush, pathCard);
                }
            };

            // -- Card 1: Order Details & Cart ---------------------------------
            Panel pnlCard1 = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0), Tag = "surface" };
            pnlCard1.Paint += cardPaint;
            tlpRight.Controls.Add(pnlCard1, 0, 0);

            TableLayoutPanel tlpCard1 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            tlpCard1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpCard1.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F));  // Header
            tlpCard1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));  // Ordered Items Title
            tlpCard1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Cart items
            tlpCard1.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F)); // Payment Summary
            tlpCard1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // Currency Selector
            pnlCard1.Controls.Add(tlpCard1);

            // -- Order Header
            Panel pnlOrderHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(16, 16, 16, 0) };

            Label lblNewOrder = new Label { Text = "New Order", Font = ThemeConfig.CardTitleFont, ForeColor = ThemeConfig.TextColorDark, AutoSize = true, Location = new Point(16, 16) };
            pnlOrderHeader.Controls.Add(lblNewOrder);

            lblOrderNum = new Label { Text = "#001", Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold), ForeColor = ThemeConfig.SecondaryColor, AutoSize = true, Location = new Point(16, 42) };
            pnlOrderHeader.Controls.Add(lblOrderNum);

            PictureBox btnTrash = new PictureBox { Image = ThemeConfig.GetNuricon("delete"), SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(28, 28), Cursor = Cursors.Hand };
            btnTrash.Click += (s, e) =>
            {
                if (cartTable.Rows.Count > 0 && MessageHelper.ConfirmAction(LocalizationManager.GetString("POS_ClearCartConfirm", "Clear cart?")))
                {
                    cartTable.Rows.Clear();
                    RefreshCartDisplay();
                }
            };
            pnlOrderHeader.Controls.Add(btnTrash);

            cmbCustomers = new Shaheen_InventoryManagement_Android.Controls.ModernComboBox { DropDownStyle = ComboBoxStyle.DropDownList, ShowLabel = false };
            pnlOrderHeader.Controls.Add(cmbCustomers);

            Button btnAddCustomer = new Button
            {
                Size = new Size(26, 26),
                Cursor = Cursors.Hand,
                TabStop = false,
                Margin = new Padding(0)
            };
            ThemeConfig.ApplyStandardAddButton(btnAddCustomer, "");
            btnAddCustomer.Size = new Size(26, 26);
            btnAddCustomer.Click += (s, e) =>
            {
                var f = new AddCustomerForm();
                if (f.ShowDialog() == DialogResult.OK)
                {
                    LoadCustomers();
                }
            };
            pnlOrderHeader.Controls.Add(btnAddCustomer);

            pnlOrderHeader.Resize += (s, ev) =>
            {
                int w = pnlOrderHeader.Width;
                btnTrash.Location = new Point(w - 40, 12);
                lblNewOrder.Location = new Point(16, 16);
                lblOrderNum.Location = new Point(16, 42);

                cmbCustomers.Width = w - 16 - 16 - 26 - 8; // span most of the width
                cmbCustomers.Location = new Point(16, 65);

                btnAddCustomer.Size = new Size(26, 26);
                btnAddCustomer.Location = new Point(cmbCustomers.Right + 8, cmbCustomers.Top + (cmbCustomers.Height - btnAddCustomer.Height) / 2);
            };
            tlpCard1.Controls.Add(pnlOrderHeader, 0, 0);

            // -- Ordered Items Title
            Panel pnlOrderedItemsHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(16, 0, 16, 0) };
            BuildOrderedItemsHeader(pnlOrderedItemsHeader);
            tlpCard1.Controls.Add(pnlOrderedItemsHeader, 0, 1);

            // -- Cart items
            pnlCartItems = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0) };
            tlpCard1.Controls.Add(pnlCartItems, 0, 2);

            // -- Payment Summary
            Panel pnlSummary = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(16, 8, 16, 4) };
            BuildSummaryPanel(pnlSummary);
            tlpCard1.Controls.Add(pnlSummary, 0, 3);

            // -- Currency Selector
            Panel pnlCurrency = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(16, 0, 16, 10) };
            BuildCurrencySelectorPanel(pnlCurrency);
            tlpCard1.Controls.Add(pnlCurrency, 0, 4);

            // -- Card 2: Actions ---------------------------------------
            Panel pnlCard2 = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0), Tag = "surface" };
            pnlCard2.Paint += cardPaint;
            tlpRight.Controls.Add(pnlCard2, 0, 2);

            TableLayoutPanel tlpCard2 = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            tlpCard2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpCard2.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F)); // Header height
            tlpCard2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Buttons height
            Panel pnlActionsHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(16, 8, 16, 0) };
            string actStr = LocalizationManager.GetString("POS_Actions");
            Label lblActionsTitle = new Label { Text = (string.IsNullOrEmpty(actStr) || actStr == "POS_Actions") ? "Actions" : actStr, Font = ThemeConfig.CardTitleFont, ForeColor = ThemeConfig.TextColorDark, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft, BackColor = Color.Transparent };
            pnlActionsHeader.Controls.Add(lblActionsTitle);
            tlpCard2.Controls.Add(pnlActionsHeader, 0, 0);

            Panel pnlActions = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(16, 4, 16, 12) };
            BuildActionsPanel(pnlActions);
            tlpCard2.Controls.Add(pnlActions, 0, 1);
            pnlCard2.Controls.Add(tlpCard2);

            // -- Card 3: Footer Buttons ---------------------------------------
            Panel pnlCard3 = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0), Tag = "surface" };
            pnlCard3.Paint += cardPaint;
            tlpRight.Controls.Add(pnlCard3, 0, 4);

            Panel pnlFooter = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(16, 12, 16, 12) };
            BuildFooterButtons(pnlFooter);
            pnlCard3.Controls.Add(pnlFooter);

            this.ResumeLayout(false);

            // Build chips after layout
            this.Load += (s, ev) => BuildCategoryChips();
        }

        // ---------------------------------------------------------------------
        // "Ordered Items" SECTION HEADER
        // ---------------------------------------------------------------------
        private Label _lblCartCount; // updated by RefreshCartDisplay

        private void BuildOrderedItemsHeader(Panel pnl)
        {
            // "Ordered Items" bold label on the left
            string orderedItemsTrans = LocalizationManager.GetString("POS_OrderedItems");
            Label lblOrderedItems = new Label
            {
                Text = orderedItemsTrans == "POS_OrderedItems" ? "Current Order" : orderedItemsTrans,
                Font = ThemeConfig.CardTitleFont,
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Grey count badge on the right (e.g. "05")
            _lblCartCount = new Label
            {
                Text = "00",
                Font = new Font(ThemeConfig.AppFontFamily, 10F, FontStyle.Regular),
                ForeColor = ThemeConfig.SecondaryColor,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            pnl.Resize += (s, ev) =>
            {
                lblOrderedItems.Location = new Point(16, (pnl.Height - lblOrderedItems.Height) / 2);
                _lblCartCount.Location = new Point(pnl.Width - 16 - _lblCartCount.Width, (pnl.Height - _lblCartCount.Height) / 2);
                _lblCartCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            };
            pnl.Controls.AddRange(new Control[] { lblOrderedItems, _lblCartCount });
        }

        // ---------------------------------------------------------------------
        // ACTIONS PANEL
        // ---------------------------------------------------------------------
        private void BuildActionsPanel(Panel pnl)
        {
            Button btnReturn = new ModernButton { Text = "Return", Cursor = Cursors.Hand };
            Button btnDraft = new ModernButton { Text = "Save Draft", Cursor = Cursors.Hand };
            Button btnQuote = new ModernButton { Text = "Quotation", Cursor = Cursors.Hand };
            Button btnBill = new ModernButton { Text = "Customer Bill", Cursor = Cursors.Hand };

            ThemeConfig.ApplyPaletteButton(btnReturn, Color.FromArgb(239, 68, 68)); // Red for Return
            ThemeConfig.ApplyPaletteButton(btnDraft, Color.FromArgb(139, 92, 246)); // Purple for Draft
            ThemeConfig.ApplyPaletteButton(btnQuote, Color.FromArgb(59, 130, 246)); // Blue for Quote
            ThemeConfig.ApplyPaletteButton(btnBill, Color.FromArgb(16, 185, 129)); // Green for Bill

            Font forceFont = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold);
            btnReturn.Font = forceFont;
            btnDraft.Font = forceFont;
            btnQuote.Font = forceFont;
            btnBill.Font = forceFont;

            // Wire up placeholders
            btnReturn.Click += (s, e) =>
            {
                var frm = new BlindReturnForm();
                frm.ShowDialog();
            };
            btnDraft.Click += (s, e) =>
            {
                if (cartTable.Rows.Count == 0) { MessageHelper.ShowWarning("Cart is empty!"); return; }
                int cid = cmbCustomers.SelectedValue != null ? Convert.ToInt32(cmbCustomers.SelectedValue) : -1;
                decimal totalAmount = 0;
                List<Shaheen_InventoryManagement_Android.Services.OrderItem> items = new List<Shaheen_InventoryManagement_Android.Services.OrderItem>();
                foreach (DataRow r in cartTable.Rows)
                {
                    if (r.RowState != DataRowState.Deleted)
                    {
                        totalAmount += (decimal)r["Total"];
                        items.Add(new Shaheen_InventoryManagement_Android.Services.OrderItem { PartId = (int)r["PartID"], Quantity = (int)r["Quantity"], UnitPrice = (decimal)r["SellingPrice"] });
                    }
                }
                decimal t = chkApplyVAT.Checked ? (totalAmount * 0.11m) : 0;
                decimal ship = chkApplyShipping.Checked ? numShipping.Value : 0;
                totalAmount += (t + ship);
                DateTime? dDate = _shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo) ? _shippingDetails.DeliveryDate : (DateTime?)null;
                DateTime? pDate = _shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo) ? _shippingDetails.PaymentDueDate : (DateTime?)null;
                string sAddr = _shippingDetails?.ShippingTo;
                if (new Shaheen_InventoryManagement_Android.Services.OrderService().PlaceOrder(cid, items, totalAmount, false, "Draft", pDate, sAddr, dDate) > 0)
                {
                    MessageHelper.ShowInfo(LocalizationManager.GetString("Msg_DraftSaved", "Draft saved successfully!"));
                    cartTable.Rows.Clear();
                    RefreshCartDisplay();
                }
            };
            btnQuote.Click += (s, e) =>
            {
                if (cartTable.Rows.Count == 0) { MessageHelper.ShowWarning("Cart is empty!"); return; }
                int cid = cmbCustomers.SelectedValue != null ? Convert.ToInt32(cmbCustomers.SelectedValue) : -1;
                decimal totalAmount = 0;
                List<Shaheen_InventoryManagement_Android.Services.OrderItem> items = new List<Shaheen_InventoryManagement_Android.Services.OrderItem>();
                foreach (DataRow r in cartTable.Rows)
                {
                    if (r.RowState != DataRowState.Deleted)
                    {
                        totalAmount += (decimal)r["Total"];
                        items.Add(new Shaheen_InventoryManagement_Android.Services.OrderItem { PartId = (int)r["PartID"], Quantity = (int)r["Quantity"], UnitPrice = (decimal)r["SellingPrice"] });
                    }
                }
                decimal tQuote = chkApplyVAT.Checked ? (totalAmount * 0.11m) : 0;
                decimal shipQuote = chkApplyShipping.Checked ? numShipping.Value : 0;
                totalAmount += (tQuote + shipQuote);
                DateTime? dDateQ = _shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo) ? _shippingDetails.DeliveryDate : (DateTime?)null;
                DateTime? pDateQ = _shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo) ? _shippingDetails.PaymentDueDate : (DateTime?)null;
                string sAddrQ = _shippingDetails?.ShippingTo;
                if (new Shaheen_InventoryManagement_Android.Services.OrderService().PlaceOrder(cid, items, totalAmount, false, "Quotation", pDateQ, sAddrQ, dDateQ) > 0)
                {
                    MessageHelper.ShowInfo(LocalizationManager.GetString("Msg_QuotationSaved", "Quotation saved successfully!"));
                    cartTable.Rows.Clear();
                    RefreshCartDisplay();
                }
            };
            btnBill.Click += (s, e) =>
            {
                if (cartTable.Rows.Count == 0) { MessageHelper.ShowWarning("Cart is empty!"); return; }
                int cid = cmbCustomers.SelectedValue != null ? Convert.ToInt32(cmbCustomers.SelectedValue) : -1;
                if (cid <= 0)
                {
                    MessageHelper.ShowWarning(LocalizationManager.GetString("POS_SelectCustomerForBill", "Please select a registered customer to add to their bill."));
                    return;
                }
                decimal totalAmount = 0;
                List<Shaheen_InventoryManagement_Android.Services.OrderItem> items = new List<Shaheen_InventoryManagement_Android.Services.OrderItem>();
                foreach (DataRow r in cartTable.Rows)
                {
                    if (r.RowState != DataRowState.Deleted)
                    {
                        totalAmount += (decimal)r["Total"];
                        items.Add(new Shaheen_InventoryManagement_Android.Services.OrderItem { PartId = (int)r["PartID"], Quantity = (int)r["Quantity"], UnitPrice = (decimal)r["SellingPrice"] });
                    }
                }
                decimal tBill = chkApplyVAT.Checked ? (totalAmount * 0.11m) : 0;
                decimal shipBill = chkApplyShipping.Checked ? numShipping.Value : 0;
                totalAmount += (tBill + shipBill);
                DateTime? dDateB = _shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo) ? _shippingDetails.DeliveryDate : (DateTime?)null;
                DateTime? pDateB = _shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo) ? _shippingDetails.PaymentDueDate : (DateTime?)null;
                string sAddrB = _shippingDetails?.ShippingTo;
                if (new Shaheen_InventoryManagement_Android.Services.OrderService().PlaceOrder(cid, items, totalAmount, false, "Completed", pDateB, sAddrB, dDateB) > 0)
                {
                    MessageHelper.ShowInfo(LocalizationManager.GetString("Msg_AddedToBill", "Successfully added to customer bill!"));
                    cartTable.Rows.Clear();
                    RefreshCartDisplay();
                }
            };

            pnl.Resize += (s, ev) =>
            {
                int gap = 8;
                int btnH = 35; // Standard button height
                int totalW = pnl.Width;
                int btnW = (totalW - gap) / 2;

                int y1 = 0;
                int y2 = y1 + btnH + gap;

                btnReturn.SetBounds(0, y1, btnW, btnH);
                btnDraft.SetBounds(btnW + gap, y1, btnW, btnH);
                btnQuote.SetBounds(0, y2, btnW, btnH);
                btnBill.SetBounds(btnW + gap, y2, btnW, btnH);
            };
            pnl.Controls.AddRange(new Control[] { btnReturn, btnDraft, btnQuote, btnBill });
        }

        // ---------------------------------------------------------------------
        // SUMMARY PANEL BUILD
        // ---------------------------------------------------------------------
        private void BuildSummaryPanel(Panel pnl)
        {
            // "Payment Summary" section header
            string paymentSummaryTrans = LocalizationManager.GetString("POS_PaymentSummary");
            Label lblSummaryHeader = new Label
            {
                Text = paymentSummaryTrans == "POS_PaymentSummary" ? "Payment Summary" : paymentSummaryTrans,
                Font = ThemeConfig.CardTitleFont,
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Subtotal row
            Label lblSubtotalTitle = MakeSummaryLabel("Subtotal", false);
            lblSubtotalVal = MakeSummaryValueLabel("$0.00", false);

            // VAT row
            chkApplyVAT = new CheckBox
            {
                Text = "",
                Checked = false,
                AutoSize = true,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            chkApplyVAT.CheckedChanged += (s, e) => UpdateTotal();

            Label lblVATTitle = MakeSummaryLabel("VAT (11%)", false);
            lblTaxVal = MakeSummaryValueLabel("$0.00", false);

            // Shipping row
            chkApplyShipping = new CheckBox
            {
                Text = "",
                Checked = false,
                AutoSize = true,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            chkApplyShipping.CheckedChanged += (s, e) =>
            {
                numShipping.Visible = chkApplyShipping.Checked;
                UpdateTotal();
            };

            Label lblShipTitle = MakeSummaryLabel("Shipping", false);
            lblShippingVal = MakeSummaryValueLabel("$0.00", false);
            numShipping = new NumericUpDown
            {
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 99999,
                Font = ThemeConfig.StandardFont,
                Visible = false,
                Width = 80,
                BorderStyle = BorderStyle.FixedSingle
            };
            numShipping.ValueChanged += (s, e) => UpdateTotal();

            // Total Payable row
            Label lblTotalTitle = MakeSummaryLabel("Total Payable", true);
            lblTotalVal = MakeSummaryValueLabel("$0.00", true);

            // Use Resize to do absolute layout
            pnl.Resize += (s, ev) =>
            {
                int w = pnl.Width - 32;
                int rightX = pnl.Width - 16;

                int y0 = 4;   // header
                int y1 = 28;  // subtotal
                int y2 = 52;  // vat
                int y3 = 76;  // shipping
                int y4 = 108; // total

                lblSummaryHeader.Location = new Point(16, y0);

                lblSubtotalTitle.Location = new Point(16, y1);
                lblSubtotalVal.Location = new Point(rightX - lblSubtotalVal.Width, y1);
                lblSubtotalVal.Anchor = AnchorStyles.Top | AnchorStyles.Right;

                chkApplyVAT.Location = new Point(16, y2);
                lblVATTitle.Location = new Point(36, y2);
                lblTaxVal.Location = new Point(rightX - lblTaxVal.Width, y2);
                lblTaxVal.Anchor = AnchorStyles.Top | AnchorStyles.Right;

                chkApplyShipping.Location = new Point(16, y3);
                lblShipTitle.Location = new Point(36, y3);
                lblShippingVal.Location = new Point(rightX - lblShippingVal.Width, y3);
                lblShippingVal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                numShipping.Location = new Point(rightX - numShipping.Width - 2, y3 - 2);
                numShipping.Anchor = AnchorStyles.Top | AnchorStyles.Right;

                lblTotalTitle.Location = new Point(16, y4);
                lblTotalVal.Location = new Point(rightX - lblTotalVal.Width, y4);
                lblTotalVal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            };

            pnl.Controls.AddRange(new Control[]
            {
                lblSummaryHeader,
                lblSubtotalTitle, lblSubtotalVal,
                chkApplyVAT, lblVATTitle, lblTaxVal,
                chkApplyShipping, lblShipTitle, lblShippingVal, numShipping,
                lblTotalTitle, lblTotalVal
            });

            // Draw separator before Total Payable
            pnl.Paint += (s, pe) =>
            {
                int sepY = 100;
                using (var pen = new Pen(ThemeConfig.POS_SeparatorColor, 1f))
                    pe.Graphics.DrawLine(pen, 16, sepY, pnl.Width - 16, sepY);
            };
        }

        private Label MakeSummaryLabel(string text, bool isTotal)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = isTotal ? new Font(ThemeConfig.AppFontFamily, 12F, FontStyle.Bold) : new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold),
                ForeColor = isTotal ? ThemeConfig.TextColorDark : ThemeConfig.SecondaryColor,
                BackColor = Color.Transparent
            };
        }

        private Label MakeSummaryValueLabel(string text, bool isTotal)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Width = 120,
                Height = 22,
                TextAlign = ContentAlignment.MiddleRight,
                Font = isTotal ? new Font(ThemeConfig.AppFontFamily, 12F, FontStyle.Bold) : new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                BackColor = Color.Transparent
            };
        }

        // ---------------------------------------------------------------------
        // CURRENCY SELECTOR PANEL ï¿½ ComboBox dropdown
        // ---------------------------------------------------------------------
        private void BuildCurrencySelectorPanel(Panel pnl)
        {
            var currencies = Shaheen_InventoryManagement_Android.Services.CurrencyService.SupportedCurrencies;

            string curTrans = LocalizationManager.GetString("POS_Currency");
            Label lblCurrLabel = new Label
            {
                Text = (string.IsNullOrEmpty(curTrans) || curTrans == "POS_Currency") ? "Currency" : curTrans,
                Font = ThemeConfig.SmallBoldFont ?? new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.SecondaryColor,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Shaheen_InventoryManagement_Android.Controls.ModernComboBox cmbCurrency = new Shaheen_InventoryManagement_Android.Controls.ModernComboBox
            {
                Font = ThemeConfig.StandardFont,
                Cursor = Cursors.Hand,
                ShowLabel = false
            };

            foreach (var curr in currencies)
                cmbCurrency.Items.Add(curr.Code);

            // Select current active
            string active = Shaheen_InventoryManagement_Android.Services.CurrencyService.ActiveCurrency;
            int idx = cmbCurrency.Items.IndexOf(active);
            cmbCurrency.SelectedIndex = idx >= 0 ? idx : 0;

            cmbCurrency.InnerComboBox.SelectedIndexChanged += (s, e) =>
            {
                string selected = cmbCurrency.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selected))
                {
                    Shaheen_InventoryManagement_Android.Services.CurrencyService.ActiveCurrency = selected;
                    if (cartTable != null)
                    {
                        LoadProducts(_activeCategory);
                        RefreshCartDisplay();
                        RefreshStats();
                    }
                }
            };

            Shaheen_InventoryManagement_Android.Services.CurrencyService.CurrencyChanged += (s, e) =>
            {
                string cur = Shaheen_InventoryManagement_Android.Services.CurrencyService.ActiveCurrency;
                int i = cmbCurrency.Items.IndexOf(cur);
                if (i >= 0 && cmbCurrency.SelectedIndex != i)
                    cmbCurrency.SelectedIndex = i;
                if (cartTable != null)
                {
                    LoadProducts(_activeCategory);
                    RefreshCartDisplay();
                    RefreshStats();
                }
            };

            pnl.Resize += (s, ev) =>
            {
                int h = pnl.Height - 10;
                int cy = (pnl.Height - h) / 2;
                lblCurrLabel.Location = new Point(16, cy + (h - lblCurrLabel.Height) / 2);
                int cw = 140; // Fixed small width for currency dropdown
                cmbCurrency.SetBounds(pnl.Width - 16 - cw, cy, cw, h);
            };

            pnl.Controls.AddRange(new Control[] { lblCurrLabel, cmbCurrency });
        }

        private Button CreatePayPillButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                Tag = "standard_",
                FlatStyle = FlatStyle.Flat,
                Font = ThemeConfig.SmallBoldFont ?? new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = ThemeConfig.SurfaceColor,
                ForeColor = ThemeConfig.TextColorDark
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (var path = ThemeConfig.GetRoundedPathPublic(r, 12))
                {
                    using (var br = new SolidBrush(btn.BackColor))
                        pe.Graphics.FillPath(br, path);
                    using (var pen = new Pen(btn.FlatAppearance.BorderColor, 1.5f))
                        pe.Graphics.DrawPath(pen, path);

                    TextRenderer.DrawText(pe.Graphics, btn.Text, btn.Font,
                        new Rectangle(0, 0, btn.Width, btn.Height), btn.ForeColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };
            return btn;
        }

        private void SetPayPillActive(Button btn, bool active)
        {
            btn.BackColor = ThemeConfig.SurfaceColor;
            btn.ForeColor = active ? ThemeConfig.PrimaryColor : ThemeConfig.TextColorDark;
            btn.FlatAppearance.BorderColor = active ? ThemeConfig.PrimaryColor : ThemeConfig.BorderColor;
            btn.Invalidate();
        }
        // 
        // FOOTER BUTTONS
        // ---------------------------------------------------------------------
        private void BuildFooterButtons(Panel pnl)
        {
            string prnStr = LocalizationManager.GetString("POS_Print");
            string plOrd = LocalizationManager.GetString("POS_PlaceOrder");

            Button btnPrintReceipt = new ModernButton
            {
                Name = "btnPrintReceipt",
                Text = (string.IsNullOrEmpty(prnStr) || prnStr == "POS_Print") ? "Print" : prnStr,
                Image = ThemeConfig.GetNuricon("print"),
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnPrintReceipt.Click += BtnPrintReceipt_Click;
            ThemeConfig.ApplySecondaryButton(btnPrintReceipt);

            btnCheckout = new ModernButton
            {
                Name = "btnCheckout",
                Text = (string.IsNullOrEmpty(plOrd) || plOrd == "POS_PlaceOrder") ? "Place Order" : plOrd,
                Image = ThemeConfig.GetNuricon("pos"),
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnCheckout.Click += BtnCheckout_Click;
            ThemeConfig.ApplyPrimaryButton(btnCheckout);

            pnl.Resize += (s, ev) =>
            {
                int h = 35; // Force height to 35px to match standard buttons
                int gap = 8;
                int totalW = pnl.Width - 32 - gap;
                int printW = (int)(totalW * 0.3);
                int checkoutW = totalW - printW;

                int yOffset = Math.Max(0, (pnl.Height - h) / 2); // Center vertically

                btnPrintReceipt.SetBounds(16, yOffset, printW, h);
                btnCheckout.SetBounds(16 + printW + gap, yOffset, checkoutW, h);

                btnPrintReceipt.Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold);
                btnCheckout.Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold);
            };

            pnl.Controls.AddRange(new Control[] { btnPrintReceipt, btnCheckout });
        }

        // ---------------------------------------------------------------------
        // CATEGORY CHIPS  ï¿½ reference card style
        // ---------------------------------------------------------------------
        private void BuildCategoryChips()
        {
            pnlChips.SuspendLayout();
            pnlChips.Controls.Clear();

            bool isRTL = LocalizationManager.IsArabic;
            pnlChips.FlowDirection = isRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            string allCatTrans = LocalizationManager.GetString("POS_AllCategories");
            AddCategoryChip(allCatTrans == "POS_AllCategories" ? "All Categories" : allCatTrans, null);

            try
            {
                var categories = CategoryData.GetAllCategories();
                foreach (var cat in categories)
                    AddCategoryChip(cat.CategoryName, cat.CategoryName);

                var allParts = PartData.GetAllParts();
                int othersCount = allParts.FindAll(p => string.IsNullOrEmpty(p.CategoryName)).Count;
                if (othersCount > 0)
                    AddCategoryChip("Others", "Others");
            }
            catch { }

            pnlChips.ResumeLayout();
        }

        private void AddCategoryChip(string label, string categoryKey)
        {
            bool isActive = (_activeCategory == categoryKey) ||
                            (_activeCategory == null && categoryKey == null);

            // Count items in this category
            int itemCount = 0;
            try
            {
                var allParts = PartData.GetAllParts();
                itemCount = categoryKey == null
                    ? allParts.Count
                    : categoryKey == "Others"
                        ? allParts.FindAll(p => string.IsNullOrEmpty(p.CategoryName)).Count
                        : allParts.FindAll(p => string.Equals(p.CategoryName, categoryKey, StringComparison.OrdinalIgnoreCase)).Count;
            }
            catch { }

            string countText = $"{itemCount} items";

            Image chipIcon = categoryKey == null ? ThemeConfig.GetNuricon("dashboard") : ThemeConfig.GetNuricon("category_placeholder");
            if (categoryKey != null)
            {
                try
                {
                    var cats = CategoryData.GetAllCategories();
                    var cat = cats.Find(c => string.Equals(c.CategoryName, categoryKey, StringComparison.OrdinalIgnoreCase));
                    if (cat != null && !string.IsNullOrEmpty(cat.CategoryImage))
                    {
                        string fullPath = System.IO.Path.Combine(Application.StartupPath, cat.CategoryImage);
                        if (System.IO.File.Exists(fullPath))
                        {
                            using (var ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(fullPath)))
                            {
                                chipIcon = Image.FromStream(ms);
                            }
                        }
                    }
                }
                catch { }
            }

            // Card dimensions ï¿½ wider to accommodate icon + text
            var nameFont = ThemeConfig.SmallBoldFont ?? new Font("Segoe UI", 9F, FontStyle.Bold);
            var countFont = new Font("Segoe UI", 7.5F);
            int nameW = TextRenderer.MeasureText(label, nameFont).Width;
            int cntW = TextRenderer.MeasureText(countText, countFont).Width;
            int cardW = Math.Max(nameW, cntW) + 72;  // icon + padding + textWidth + right-pad
            cardW = Math.Max(cardW, 140);
            const int CARD_H = 64;
            // const int ICON_AREA = 32; // width reserved for the emoji circle

            // Active border color ï¿½ teal/primary on top edge (like reference)
            Color activeBorder = ThemeConfig.POS_ChipActiveBorder;
            Color inactiveBg = ThemeConfig.SurfaceColor;

            var chip = new Panel
            {
                AutoSize = false,
                Width = cardW,
                Height = CARD_H,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0),
                BackColor = Color.Transparent,
                ForeColor = ThemeConfig.TextColorDark,
                Tag = categoryKey
            };
            // drawn manually

            chip.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Clear background with parent colour
                using (var parentBrush = new SolidBrush(ThemeConfig.GetParentColor(chip)))
                    g.FillRectangle(parentBrush, -1, -1, chip.Width + 2, chip.Height + 2);

                var r = new Rectangle(0, 0, chip.Width - 1, chip.Height - 1);
                int cardRadius = 8; // Less rounded than before, matching reference

                // Card fill
                Color bgFill = isActive
                    ? Color.FromArgb(12, activeBorder.R, activeBorder.G, activeBorder.B)
                    : inactiveBg;

                using (var path = RoundedPath(r, cardRadius))
                using (var br = new SolidBrush(bgFill))
                    g.FillPath(br, path);

                // Border
                if (isActive)
                {
                    using (var path = RoundedPath(r, cardRadius))
                    using (var pen = new Pen(activeBorder, 1.5f)) // Prominent border for active
                        g.DrawPath(pen, path);
                }
                else
                {
                    using (var path = RoundedPath(r, cardRadius))
                    using (var pen = new Pen(ThemeConfig.BorderColor, 1f)) // Faint border for inactive
                        g.DrawPath(pen, path);
                }

                // -- Icon squircle (left side) ----------------------------------
                int cx = 10;
                int iconSize = 36;
                int cy = (CARD_H - iconSize) / 2;

                // Light grey background for the icon area
                using (var iconBgPath = RoundedPath(new Rectangle(cx, cy, iconSize, iconSize), 6))
                using (var iconBr = new SolidBrush(Color.FromArgb(242, 244, 246))) // Very light grey
                    g.FillPath(iconBr, iconBgPath);

                if (chipIcon != null)
                {
                    float scale = Math.Min(iconSize * 0.7f / chipIcon.Width, iconSize * 0.7f / chipIcon.Height);
                    float sw = chipIcon.Width * scale;
                    float sh = chipIcon.Height * scale;
                    float dx = cx + (iconSize - sw) / 2f;
                    float dy = cy + (iconSize - sh) / 2f;
                    g.DrawImage(chipIcon, new RectangleF(dx, dy, sw, sh));
                }

                // -- Text block (right of icon) --------------------------------
                int textX = cx + iconSize + 10;
                int textW = chip.Width - textX - 8;

                // Title and Subtitle block is vertically centered together
                int textBlockY = cy + 2;

                // Category name ï¿½ always dark, bold
                TextRenderer.DrawText(g, label, nameFont,
                    new Rectangle(textX, textBlockY, textW, 16), ThemeConfig.TextColorDark,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

                // Item count ï¿½ always small grey below name
                using (var cf = new Font("Segoe UI", 7.5F))
                    TextRenderer.DrawText(g, countText, cf,
                        new Rectangle(textX, textBlockY + 18, textW, 16), ThemeConfig.SecondaryColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

                countFont.Dispose();
            };

            chip.Click += (s, e) =>
            {
                _activeCategory = categoryKey;
                BuildCategoryChips();
                LoadProducts(txtProductSearch?.Text);
            };

            pnlChips.Controls.Add(chip);
        }

        // ---------------------------------------------------------------------
        // PRODUCT LOADING
        // ---------------------------------------------------------------------
        public async void LoadProducts(string search = null)
        {
            pnlProducts.SuspendLayout();
            foreach (Control c in pnlProducts.Controls) c.Dispose();
            pnlProducts.Controls.Clear();

            try
            {
                int totalCount = string.IsNullOrWhiteSpace(search)
                    ? await System.Threading.Tasks.Task.Run(() => PartData.GetAllPartsCount(_activeCategory))
                    : await System.Threading.Tasks.Task.Run(() => PartData.SearchPartsCount(search.Trim(), _activeCategory));

                int totalPages = (int)Math.Ceiling(totalCount / (double)_pageSize);
                if (totalPages == 0) totalPages = 1;
                if (_currentPage > totalPages) _currentPage = totalPages;

                if (lblPageInfo != null) lblPageInfo.Text = $"Page {_currentPage} of {totalPages}";
                if (btnPrevPage != null) btnPrevPage.Enabled = _currentPage > 1;
                if (btnNextPage != null) btnNextPage.Enabled = _currentPage < totalPages;

                int offset = (_currentPage - 1) * _pageSize;

                List<PartData> all = string.IsNullOrWhiteSpace(search)
                    ? await System.Threading.Tasks.Task.Run(() => PartData.GetAllParts(_activeCategory, _pageSize, offset))
                    : await System.Threading.Tasks.Task.Run(() => PartData.SearchParts(search.Trim(), _activeCategory, _pageSize, offset));

                if (all.Count == 0)
                {
                    Label noResults = new Label
                    {
                        Text = "No products found.",
                        Font = ThemeConfig.StandardFont,
                        ForeColor = ThemeConfig.SecondaryColor,
                        AutoSize = true,
                        Margin = new Padding(16)
                    };
                    pnlProducts.Controls.Add(noResults);
                }
                else
                {
                    var controlsList = new System.Collections.Generic.List<Control>();
                    foreach (var part in all)
                    {
                        controlsList.Add(CreateProductCard(part));
                    }
                    pnlProducts.Controls.AddRange(controlsList.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Error loading products: {ex.Message}");
            }

            pnlProducts.ResumeLayout();
        }

        // ---------------------------------------------------------------------
        // PRODUCT CARD
        // ---------------------------------------------------------------------
        private Panel CreateProductCard(PartData part)
        {
            bool outOfStock = part.QuantityInStock <= 0;
            // cartQty is read live in Paint/layout so the border updates instantly
            int GetLiveQty() => GetCartQty(part.Id);

            Color cardBgColor = outOfStock ? Color.FromArgb(240, 240, 242) : ThemeConfig.SurfaceColor;

            // -- Card shell --------------------------------------------------
            // Matches the green reference: compact, rounded, white bg, subtle border
            const int CARD_W = 170;
            const int CARD_H = 225;
            const int IMG_SIZE = 110;  // larger image circle like reference
            const int RADIUS = 16;
            const int BTN_SIZE = 26;

            Panel card = new Panel
            {
                Size = new Size(CARD_W, CARD_H),
                BackColor = ThemeConfig.SurfaceColor,
                Margin = new Padding(0, 0, 12, 12),
                Cursor = outOfStock ? Cursors.No : Cursors.Hand,
                Tag = part.Id,
            };
            // Enable double-buffering via reflection (DoubleBuffered is protected on Panel)
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(card, true);


            bool hovered = false;
            card.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                // erase parent color first so rounded corners show correctly
                using (var parentBrush = new SolidBrush(ThemeConfig.GetParentColor(card)))
                    pe.Graphics.FillRectangle(parentBrush, -1, -1, card.Width + 2, card.Height + 2);

                var r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                int liveQty = GetLiveQty();
                Color borderColor;
                if (liveQty > 0) borderColor = ThemeConfig.PrimaryColor;   // in-cart: teal accent
                else if (hovered && !outOfStock) borderColor = ThemeConfig.PrimaryColor;
                else borderColor = ThemeConfig.BorderColor;

                using (var path = RoundedPath(r, RADIUS))
                {
                    using (var br = new SolidBrush(cardBgColor))
                        pe.Graphics.FillPath(br, path);
                    float borderW = (liveQty > 0 || (hovered && !outOfStock)) ? 1.8f : 1f;
                    using (var pen = new Pen(borderColor, borderW))
                        pe.Graphics.DrawPath(pen, path);
                }
            };
            // Hover wiring deferred ï¿½ applied after all children are built (see PropagateHover below)

            // ------------------------------------------------------------------
            // SECTION 1 ï¿½ Image container  (div.card-image)
            // A transparent panel that centres the circular image
            // ------------------------------------------------------------------
            const int IMG_SECTION_H = 128; // height of image zone
            Panel pnlImageSection = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(CARD_W, IMG_SECTION_H),
                BackColor = Color.Transparent
            };
            card.Controls.Add(pnlImageSection);

            // Circular background disc ï¿½ centred in the image section
            int circleDiameter = IMG_SIZE + 6;
            int circleX = (CARD_W - circleDiameter) / 2;
            int circleY = (IMG_SECTION_H - circleDiameter) / 2;

            Panel pnlImgBg = new Panel
            {
                Location = new Point(circleX, circleY),
                Size = new Size(circleDiameter, circleDiameter),
                BackColor = Color.Transparent   // parent handles clearing
            };

            // Load image once ï¿½ drawn directly in Paint (no PictureBox needed)
            var bmp = LoadProductImage(part.PartImage, part.CategoryName, IMG_SIZE);

            pnlImgBg.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                int w = pnlImgBg.Width;
                int h = pnlImgBg.Height;

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                // -- Step 1: fill the background circle ----------------------
                using (var br = new SolidBrush(ThemeConfig.BackgroundColor))
                    g.FillEllipse(br, 0, 0, w - 1, h - 1);

                // -- Step 2: clip to circle, then draw the image perfectly centred -
                if (bmp != null)
                {
                    // Scale to fit inside a PAD-inset square, preserving aspect ratio
                    const int PAD = 6;
                    float avail = Math.Min(w, h) - PAD * 2f;   // usable diameter
                    float scale = Math.Min(avail / bmp.Width, avail / bmp.Height);

                    // Use float throughout ï¿½ integer truncation causes systematic 1px error
                    float scaledW = bmp.Width * scale;
                    float scaledH = bmp.Height * scale;

                    // Centre image exactly at (w/2, h/2) regardless of bitmap dimensions
                    float dx = (w - scaledW) / 2f;
                    float dy = (h - scaledH) / 2f;

                    using (var clipPath = new GraphicsPath())
                    {
                        clipPath.AddEllipse(1, 1, w - 3, h - 3);
                        g.SetClip(clipPath);
                        g.DrawImage(bmp, new RectangleF(dx, dy, scaledW, scaledH));
                        g.ResetClip();
                    }
                }
            };
            pnlImageSection.Controls.Add(pnlImgBg);

            // ------------------------------------------------------------------
            // SECTION 2 ï¿½ Text container  (div.card-body)
            // Category italic label + bold product name, both centred
            // ------------------------------------------------------------------
            const int TEXT_SECTION_H = 56;
            int textSectionY = IMG_SECTION_H;  // sits directly under the image section
            Panel pnlTextSection = new Panel
            {
                Location = new Point(0, textSectionY),
                Size = new Size(CARD_W, TEXT_SECTION_H),
                BackColor = Color.Transparent,
                Padding = new Padding(10, 0, 10, 0)
            };
            card.Controls.Add(pnlTextSection);

            // Category ï¿½ small italic grey (like reference)
            Label lblCat = new Label
            {
                Text = part.CategoryName,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Italic),
                ForeColor = ThemeConfig.SecondaryColor,
                AutoSize = false,
                Width = CARD_W - 20,
                Height = 16,
                Location = new Point(10, 2),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            pnlTextSection.Controls.Add(lblCat);

            // Product name  bold, dark, 2-line wrap
            Label lblName = new Label
            {
                Text = part.PartName,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = false,
                Width = CARD_W - 20,
                Height = 18,
                Location = new Point(10, 18),
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.Transparent
            };
            pnlTextSection.Controls.Add(lblName);

            Label lblDesc = new Label
            {
                Text = part.Description,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.Gray,
                AutoSize = false,
                Width = CARD_W - 20,
                Height = 16,
                Location = new Point(10, 36),
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.Transparent
            };
            pnlTextSection.Controls.Add(lblDesc);

            // ------------------------------------------------------------------
            // SECTION 3  Footer row  (div.card-footer)
            // Price (left) |  qty + (right), all vertically centred
            // ------------------------------------------------------------------
            int footerY = textSectionY + TEXT_SECTION_H;
            const int FOOTER_H = CARD_H - IMG_SECTION_H - TEXT_SECTION_H;
            Panel pnlFooterRow = new Panel
            {
                Location = new Point(0, footerY),
                Size = new Size(CARD_W, FOOTER_H),
                BackColor = Color.Transparent,  // transparent keeps card rounded corners intact
                Padding = new Padding(10, 0, 10, 0)
            };
            card.Controls.Add(pnlFooterRow);

            // -- Price label (left side of footer) --------------------------
            Label lblPrice = new Label
            {
                Text = CurrencyService.Format(part.SellingPrice),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = ThemeConfig.PrimaryColor,
                AutoSize = false,          // fixed size so DoFooterLayout() can centre it immediately
                Width = 80,
                Height = BTN_SIZE,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            // -- - panel button (outlined rounded rectangle) -----------------
            // Using Panel instead of Button: Panels have no ButtonBase clip insets.
            Panel btnMinus = new Panel
            {
                Name = "btnMinus_" + part.Id,
                Size = new Size(BTN_SIZE, BTN_SIZE),
                Cursor = Cursors.Hand,
                BackColor = cardBgColor,
            };
            btnMinus.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(1, 1, btnMinus.Width - 2, btnMinus.Height - 2);
                using (var path = RoundedPath(rect, 7))
                using (var pen = new Pen(btnMinus.Enabled ? ThemeConfig.BorderColor : ThemeConfig.SecondaryColor, 1.5f))
                    pe.Graphics.DrawPath(pen, path);
                Color textCol = btnMinus.Enabled ? ThemeConfig.TextColorDark : ThemeConfig.SecondaryColor;
                TextRenderer.DrawText(pe.Graphics, "-",
                    new Font("Segoe UI", 10F),
                    new Rectangle(0, 0, btnMinus.Width, btnMinus.Height),
                    textCol,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            };

            TextBox txtQty = new TextBox
            {
                Text = "1",
                Size = new Size(30, BTN_SIZE),
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                BackColor = cardBgColor,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0),
                Tag = "qtyLabel_" + part.Id
            };
            txtQty.KeyPress += (sender, ev) =>
            {
                if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar)) ev.Handled = true;
                if (ev.KeyChar == (char)Keys.Enter) { ev.Handled = true; card.Focus(); }
            };
            txtQty.Leave += (sender, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtQty.Text) || txtQty.Text == "0") txtQty.Text = "1";
            };

            int GetInputQty()
            {
                if (int.TryParse(txtQty.Text, out int q) && q > 0) return q;
                return 1;
            }

            // -- + panel button (filled circle, primary color) -----------------
            Panel btnPlus = new Panel
            {
                Size = new Size(BTN_SIZE, BTN_SIZE),
                Cursor = Cursors.Hand,
                BackColor = cardBgColor,
            };
            btnPlus.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                pe.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                Color circleColor = outOfStock ? ThemeConfig.SecondaryColor : ThemeConfig.PrimaryColor;
                // Draw perfect circle: diameter = BTN_SIZE-2, centred 1px from each edge
                int d = BTN_SIZE - 2;
                int cx = 1;
                int cy = 1;
                using (var br = new SolidBrush(circleColor))
                    pe.Graphics.FillEllipse(br, cx, cy, d, d);
                // + glyph centred exactly inside the circle bounding rect
                using (var sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    using (var wb = new SolidBrush(Color.White))
                        pe.Graphics.DrawString("+", new Font("Segoe UI", 11F, FontStyle.Bold), wb,
                            new RectangleF(cx, cy, d, d), sf);
                }
            };

            // Disable qty controls when out of stock
            if (outOfStock)
            {
                btnPlus.Enabled = false;
                btnMinus.Enabled = false;
            }

            // -- Layout helper  called immediately AND on every resize --------
            void DoFooterLayout()
            {
                int fy = (FOOTER_H - BTN_SIZE) / 2;   // vertical centre
                int rEdge = CARD_W - 10;                               // Right ? Left: [?+] [qty] []
                btnPlus.Location = new Point(rEdge - BTN_SIZE, fy);
                txtQty.Location = new Point(rEdge - BTN_SIZE - 30, fy + 4);
                btnMinus.Location = new Point(rEdge - BTN_SIZE - 30 - BTN_SIZE, fy);

                // Price: left-aligned, width capped so it never overlaps btnMinus
                lblPrice.Location = new Point(10, fy);
                lblPrice.Width = btnMinus.Location.X - 10 - 4;  // 4px safety gap
            }

            // CRITICAL z-order: add lblPrice LAST so it becomes Controls[n-1] = BACK.
            // Controls[0] = FRONT (painted last/on top). If lblPrice were FRONT and its right
            // edge overlapped btnMinus, its transparent repaint would erase btnMinus's left border.
            // By being BACK, Windows clips lblPrice's DC to EXCLUDE the area of front controls.
            pnlFooterRow.Controls.AddRange(new Control[] { btnPlus, txtQty, btnMinus, lblPrice });

            // Position controls right now (before Resize ever fires)
            DoFooterLayout();

            // Also re-layout if the panel is ever resized at runtime
            pnlFooterRow.Resize += (s, ev) => DoFooterLayout();

            // ------------------------------------------------------------------
            // BADGES / OVERLAYS
            // ------------------------------------------------------------------
            if (outOfStock)
            {
                Label lblBadge = new Label
                {
                    Text = "Out of Stock",
                    Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = false,
                    Width = CARD_W - 20,
                    Height = 18,
                    Location = new Point(10, textSectionY + 2),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                lblBadge.Paint += (s, pe) =>
                {
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = RoundedPath(new Rectangle(0, 0, lblBadge.Width - 1, lblBadge.Height - 1), 6))
                    using (var br = new SolidBrush(ThemeConfig.DangerColor))
                        pe.Graphics.FillPath(br, path);
                    TextRenderer.DrawText(pe.Graphics, lblBadge.Text, lblBadge.Font,
                        new Rectangle(0, 0, lblBadge.Width, lblBadge.Height), Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                };
                card.Controls.Add(lblBadge);
            }
            else if (part.QuantityInStock <= part.MinimumStockLevel && part.MinimumStockLevel > 0)
            {
                // "Low stock" hint shown as a small tinted label inside footer (above price)
                Label lblStock = new Label
                {
                    Text = $"Low: {part.QuantityInStock}",
                    Font = new Font("Segoe UI", 7F),
                    ForeColor = ThemeConfig.WarningColor,
                    AutoSize = true,
                    Location = new Point(10, footerY - 14),
                    BackColor = Color.Transparent
                };
                card.Controls.Add(lblStock);
            }

            // ------------------------------------------------------------------
            // HOVER PROPAGATION
            // WinForms fires MouseLeave on 'card' the instant the cursor enters
            // any child control, so we must attach Enter/Leave to every
            // descendant as well.  We use a recursive helper called AFTER all
            // children have been added.
            // ------------------------------------------------------------------
            void SetHover(bool value)
            {
                hovered = value;
                card.Invalidate();
            }

            void PropagateHover(Control parent)
            {
                parent.MouseEnter += (s, e) => SetHover(true);
                parent.MouseLeave += (s, e) =>
                {
                    // Only clear hover if the cursor is truly outside the card
                    Point cursor = card.PointToClient(Control.MousePosition);
                    if (!card.ClientRectangle.Contains(cursor))
                        SetHover(false);
                };
                foreach (Control child in parent.Controls)
                    PropagateHover(child);
            }
            PropagateHover(card);

            // ------------------------------------------------------------------
            // CLICK HANDLERS  clicking image/text/card body adds to cart
            // ------------------------------------------------------------------
            Action addToCart = () =>
            {
                if (!outOfStock)
                {
                    AddToCart(part.Id, part.PartName, part.SellingPrice, part.QuantityInStock, GetInputQty());
                    txtQty.Text = "1";
                }
            };

            card.Click += (s, e) => addToCart();
            pnlImageSection.Click += (s, e) => addToCart();
            pnlImgBg.Click += (s, e) => addToCart();

            pnlTextSection.Click += (s, e) => addToCart();
            lblCat.Click += (s, e) => addToCart();
            lblName.Click += (s, e) => addToCart();
            lblPrice.Click += (s, e) => addToCart();

            btnPlus.Click += (s, e) => { addToCart(); };
            btnMinus.Click += (s, e) =>
            {
                if (!outOfStock)
                {
                    RemoveOneFromCart(part.Id, GetInputQty());
                    txtQty.Text = "1";
                }
            };

            return card;
        }




        // ---------------------------------------------------------------------
        // CART DISPLAY ï¿½ simple text rows matching reference design
        // ---------------------------------------------------------------------
        public void RefreshCartDisplay()
        {
            if (cartTable == null) return;
            pnlCartItems.SuspendLayout();
            foreach (Control c in pnlCartItems.Controls)
                c.Dispose();
            pnlCartItems.Controls.Clear();

            // Count total items for header
            int totalItems = 0;
            foreach (DataRow dr in cartTable.Rows)
                if (dr.RowState != DataRowState.Deleted) totalItems += (int)dr["Quantity"];

            if (_lblCartCount != null)
            {
                _lblCartCount.Text = totalItems.ToString("D2");
            }

            // Cart rows
            foreach (DataRow row in cartTable.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;

                int partId = (int)row["PartID"];
                string partName = row["PartName"].ToString();
                int qty = (int)row["Quantity"];
                decimal price = (decimal)row["SellingPrice"];
                decimal total = (decimal)row["Total"];

                Panel rowPanel = new Panel
                {
                    Height = 46,
                    Dock = DockStyle.Top,
                    BackColor = Color.Transparent,
                    Tag = partId
                };

                // Bottom separator line
                rowPanel.Paint += (s, pe) =>
                {
                    using (var pen = new Pen(ThemeConfig.POS_SeparatorColor, 1f))
                        pe.Graphics.DrawLine(pen, 16, rowPanel.Height - 1, rowPanel.Width - 16, rowPanel.Height - 1);
                };

                // Product name - bold
                Label lblName = new Label
                {
                    Text = partName,
                    Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Regular),
                    ForeColor = ThemeConfig.SecondaryColor,
                    AutoSize = false,
                    Height = 18,
                    Location = new Point(16, 4),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                rowPanel.Controls.Add(lblName);

                Label lblQtyTxt = new Label
                {
                    Text = $"{qty} Ã— ",
                    Font = new Font(ThemeConfig.AppFontFamily, 11F, FontStyle.Bold),
                    ForeColor = ThemeConfig.PrimaryColor,
                    AutoSize = true,
                    Height = 18,
                    Location = new Point(16, 22),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                rowPanel.Controls.Add(lblQtyTxt);

                ComboBox cmbPrice = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font(ThemeConfig.AppFontFamily, 8F, FontStyle.Regular),
                    Width = 75,
                    Location = new Point(50, 20),
                    TabStop = false
                };
                ThemeConfig.ApplyComboBoxStyle(cmbPrice);

                try
                {
                    DataTable dtPrices = DatabaseHelper.ExecuteDataTable($"SELECT selling_price, price2, price3, price4 FROM parts WHERE id = {partId}");
                    if (dtPrices.Rows.Count > 0)
                    {
                        DataRow pr = dtPrices.Rows[0];
                        List<decimal> cand = new List<decimal>();
                        if (pr["selling_price"] != DBNull.Value && Convert.ToDecimal(pr["selling_price"]) > 0) cand.Add(Convert.ToDecimal(pr["selling_price"]));
                        if (pr["price2"] != DBNull.Value && Convert.ToDecimal(pr["price2"]) > 0) cand.Add(Convert.ToDecimal(pr["price2"]));
                        if (pr["price3"] != DBNull.Value && Convert.ToDecimal(pr["price3"]) > 0) cand.Add(Convert.ToDecimal(pr["price3"]));
                        if (pr["price4"] != DBNull.Value && Convert.ToDecimal(pr["price4"]) > 0) cand.Add(Convert.ToDecimal(pr["price4"]));

                        if (!cand.Contains(price)) cand.Add(price);

                        cmbPrice.DisplayMember = "Value";
                        cmbPrice.ValueMember = "Key";
                        var itemsList = new List<KeyValuePair<decimal, string>>();
                        foreach (var p in cand)
                            itemsList.Add(new KeyValuePair<decimal, string>(p, CurrencyService.Format(p)));

                        cmbPrice.DataSource = itemsList;
                        cmbPrice.SelectedValue = price;
                    }
                }
                catch { }

                cmbPrice.SelectedIndexChanged += (s, e) =>
                {
                    if (cmbPrice.SelectedValue is decimal newPrice && newPrice != price)
                    {
                        row["SellingPrice"] = newPrice;
                        RefreshCartDisplay();
                    }
                };
                rowPanel.Controls.Add(cmbPrice);

                // Row total  right aligned
                Label lblRowTotal = new Label
                {
                    Text = CurrencyService.Format(total),
                    Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold),
                    ForeColor = ThemeConfig.TextColorDark,
                    AutoSize = false,
                    Width = 80,
                    Height = 18,
                    TextAlign = ContentAlignment.MiddleRight,
                    BackColor = Color.Transparent
                };
                rowPanel.Controls.Add(lblRowTotal);

                // Inline  qty + controls (small, right side)
                int capId = partId;
                int bSz = 22;

                Button btnMinus = new Button
                {
                    Text = "-",
                    Size = new Size(bSz, bSz),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Regular),
                    Cursor = Cursors.Hand,
                    BackColor = ThemeConfig.SurfaceColor,
                    ForeColor = ThemeConfig.TextColorDark,
                    TabStop = false
                };
                btnMinus.FlatAppearance.BorderColor = ThemeConfig.BorderColor;
                btnMinus.FlatAppearance.BorderSize = 1;
                TextBox txtQtyInput = new TextBox
                {
                    Text = "1",
                    Size = new Size(42, bSz),
                    TextAlign = HorizontalAlignment.Center,
                    Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold),
                    ForeColor = ThemeConfig.TextColorDark,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(0)
                };
                txtQtyInput.KeyPress += (sender, ev) =>
                {
                    if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar)) ev.Handled = true;
                    if (ev.KeyChar == (char)Keys.Enter) { ev.Handled = true; rowPanel.Focus(); }
                };
                txtQtyInput.Leave += (sender, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(txtQtyInput.Text) || txtQtyInput.Text == "0") txtQtyInput.Text = "1";
                };
                int GetRowInputQty()
                {
                    if (int.TryParse(txtQtyInput.Text, out int q) && q > 0) return q;
                    return 1;
                }

                btnMinus.Click += (s, e) => RemoveOneFromCart(capId, GetRowInputQty());

                Button btnPlus = new Button
                {
                    Text = "+",
                    Size = new Size(bSz, bSz),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font(ThemeConfig.AppFontFamily, 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    BackColor = ThemeConfig.PrimaryColor,
                    ForeColor = Color.White,
                    TabStop = false
                };
                btnPlus.FlatAppearance.BorderSize = 0;
                // Circular + button
                btnPlus.Paint += (s2, pe2) =>
                {
                    pe2.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var parentBrush = new SolidBrush(ThemeConfig.GetParentColor(btnPlus)))
                        pe2.Graphics.FillRectangle(parentBrush, -1, -1, btnPlus.Width + 2, btnPlus.Height + 2);
                    using (var br = new SolidBrush(btnPlus.BackColor))
                        pe2.Graphics.FillEllipse(br, 0, 0, btnPlus.Width - 1, btnPlus.Height - 1);
                    TextRenderer.DrawText(pe2.Graphics, "+", btnPlus.Font,
                        new Rectangle(0, 0, btnPlus.Width, btnPlus.Height), Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                };
                btnPlus.Click += (s, e) =>
                {
                    int addQ = GetRowInputQty();
                    int stock = 0;
                    try { stock = DatabaseHelper.ExecuteScalar<int>($"SELECT quantity_in_stock FROM parts WHERE id={capId}"); } catch { stock = 999; }
                    foreach (DataRow dr in cartTable.Rows)
                    {
                        if (dr.RowState != DataRowState.Deleted && (int)dr["PartID"] == capId)
                        {
                            int curQty = (int)dr["Quantity"];
                            if (curQty + addQ > stock) { MessageHelper.ShowWarning(LocalizationManager.GetString("POS_NotEnoughStock", "Not enough stock.")); return; }
                            dr["Quantity"] = curQty + addQ;
                            break;
                        }
                    }
                    RefreshCartDisplay();
                };

                // Resize handler  positions elements
                rowPanel.Resize += (s, ev) =>
                {
                    int rX = rowPanel.Width - 16;
                    lblName.Width = rX - 100;
                    lblRowTotal.Location = new Point(rX - 80, 4);
                    btnPlus.Location = new Point(rX - bSz, 22);
                    txtQtyInput.Location = new Point(rX - bSz - 42, 24);
                    btnMinus.Location = new Point(rX - bSz - 42 - bSz, 22);
                    cmbPrice.Location = new Point(lblQtyTxt.Right, 20);
                };

                rowPanel.Controls.AddRange(new Control[] { lblRowTotal, btnMinus, txtQtyInput, btnPlus });
                pnlCartItems.Controls.Add(rowPanel);
            }

            pnlCartItems.ResumeLayout();
            UpdateTotal();
            UpdateProductCardQtyAll();

            // Update "Ordered Items" count badge
            if (_lblCartCount != null)
                _lblCartCount.Text = (cartTable?.Rows.Count ?? 0).ToString("D2");
        }



        // ---------------------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------------------
        private int GetCartQty(int partId)
        {
            if (cartTable == null) return 0;
            foreach (DataRow r in cartTable.Rows)
                if (r.RowState != DataRowState.Deleted && (int)r["PartID"] == partId)
                    return (int)r["Quantity"];
            return 0;
        }

        private void UpdateQtyControl(Control parent, int pid, int qty)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is Panel pnl && pnl.Name == "btnMinus_" + pid)
                {
                    pnl.Enabled = (qty > 0);
                    pnl.Invalidate();
                }
                else if (child is Label lbl && lbl.Tag is string tl && tl == "qtyLabel_" + pid)
                {
                    lbl.Text = qty.ToString();
                }
                if (child.HasChildren) UpdateQtyControl(child, pid, qty);
            }
        }

        private void UpdateProductCardQtyAll()
        {
            foreach (Control c in pnlProducts.Controls)
            {
                if (c is Panel card && card.Tag is int pid)
                {
                    int qty = GetCartQty(pid);
                    UpdateQtyControl(card, pid, qty);
                }
            }
        }

        private void SetCartQty(PartData part, int newQty)
        {
            if (newQty > part.QuantityInStock) newQty = part.QuantityInStock;

            foreach (DataRow r in cartTable.Rows)
            {
                if (r.RowState != DataRowState.Deleted && (int)r["PartID"] == part.Id)
                {
                    if (newQty <= 0)
                        cartTable.Rows.Remove(r);
                    else
                        r["Quantity"] = newQty;
                    RefreshCartDisplay();
                    return;
                }
            }

            if (newQty > 0)
            {
                cartTable.Rows.Add(part.Id, part.PartName, newQty, 0, part.SellingPrice);
                RefreshCartDisplay();
            }
        }

        private void RemoveOneFromCart(int partId, int qtyToRem = 1)
        {
            foreach (DataRow r in cartTable.Rows)
            {
                if (r.RowState != DataRowState.Deleted && (int)r["PartID"] == partId)
                {
                    int q = (int)r["Quantity"];
                    if (q <= 1)
                        cartTable.Rows.Remove(r);
                    else
                        r["Quantity"] = q - 1;
                    break;
                }
            }
            RefreshCartDisplay();
        }

        private void RemoveFromCart(int partId)
        {
            DataRow toRemove = null;
            foreach (DataRow r in cartTable.Rows)
                if (r.RowState != DataRowState.Deleted && (int)r["PartID"] == partId)
                { toRemove = r; break; }
            if (toRemove != null) cartTable.Rows.Remove(toRemove);
            RefreshCartDisplay();
        }

        private Panel CreateSeparator()
        {
            Panel sep = new Panel { Dock = DockStyle.Fill, BackColor = ThemeConfig.POS_SeparatorColor, Height = 1 };
            return sep;
        }

        // Shared rounded path helper
        private static GraphicsPath RoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ---------------------------------------------------------------------
        // IMAGE LOADING
        // ---------------------------------------------------------------------
        private static Bitmap LoadProductImage(string imagePath, string categoryName, int size = 80)
        {
            return Shaheen_InventoryManagement_Android.Helpers.CacheManager.GetProductImage(imagePath, categoryName, size);
        }

        // ---------------------------------------------------------------------
        // INITIALIZE CART  (business logic ï¿½ preserved)
        // ---------------------------------------------------------------------
        private void InitializeCart()
        {
            try
            {
                cartTable = new DataTable();
                cartTable.Columns.Add("PartID", typeof(int));
                cartTable.Columns.Add("PartName", typeof(string));
                cartTable.Columns.Add("Quantity", typeof(int));
                cartTable.Columns.Add("PrivatePrice", typeof(decimal));
                cartTable.Columns.Add("SellingPrice", typeof(decimal));
                cartTable.Columns.Add("Total", typeof(decimal), "Quantity * SellingPrice");
                LoadCustomers();
            }
            catch (Exception ex) { MessageHelper.ShowError("Error initializing POS: " + ex.Message); }
        }

        // ---------------------------------------------------------------------
        // LOAD CUSTOMERS  (preserved)
        // ---------------------------------------------------------------------
        private void LoadCustomers()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteDataTable("SELECT customer_id, full_name FROM customers ORDER BY full_name");
                DataRow row = dt.NewRow();
                row["customer_id"] = -1;
                row["full_name"] = LocalizationManager.GetString("POS_WalkIn");
                dt.Rows.InsertAt(row, 0);
                cmbCustomers.ValueMember = "customer_id";
                cmbCustomers.DisplayMember = "full_name";
                cmbCustomers.DataSource = dt;
            }
            catch { }
        }

        // ---------------------------------------------------------------------
        // APPLY PERMISSIONS  (preserved)
        // ---------------------------------------------------------------------
        private void ApplyPermissions() { }

        // ---------------------------------------------------------------------
        // REFRESH STATS  (preserved)
        // ---------------------------------------------------------------------
        public void RefreshStats()
        {
            try
            {
                if (cardTodayOrders != null)
                    cardTodayOrders.Value = _dashboardService.GetOrdersCount("Today").ToString();
                if (cardTodaySales != null)
                    cardTodaySales.Value = "$" + _dashboardService.GetSales("Today").ToString("N0");
                if (cardPending != null)
                    cardPending.Value = _dashboardService.GetPendingOrdersCount().ToString();
            }
            catch (Exception ex) { Console.WriteLine("Stats Error: " + ex.Message); }
        }

        // ---------------------------------------------------------------------
        // APPLY LOCALIZATION  (preserved + new controls)
        // ---------------------------------------------------------------------
        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            LocalizationManager.TranslateControl(this);
            Func<string, string> L = LocalizationManager.GetString;

            if (btnCheckout != null) btnCheckout.Text = L("POS_Checkout");
            // if (btnClearCart != null) btnClearCart.Text = L("POS_ClearCart");

            if (cardTodayOrders != null) cardTodayOrders.Title = L("POS_Orders");
            if (cardTodaySales != null) cardTodaySales.Title = L("POS_Sales");
            if (cardPending != null) cardPending.Title = L("POS_Pending");

            // Rebuild chips for language change
            if (pnlChips != null)
            {
                pnlChips.FlowDirection = LocalizationManager.IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                BuildCategoryChips();
            }
            if (pnlProducts != null)
                pnlProducts.FlowDirection = LocalizationManager.IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        // ---------------------------------------------------------------------
        // ADD TO CART  (preserved logic + RefreshCartDisplay)
        // ---------------------------------------------------------------------
        private void AddToCart(int id, string name, decimal price, int stock, int qtyToAdd = 1)
        {
            if (stock <= 0) { MessageHelper.ShowWarning(LocalizationManager.GetString("Error_OutOfStock")); return; }
            foreach (DataRow r in cartTable.Rows)
            {
                if (r.RowState == DataRowState.Deleted) continue;
                if ((int)r["PartID"] == id)
                {
                    int q = (int)r["Quantity"];
                    if (q + qtyToAdd > stock) { MessageHelper.ShowWarning(LocalizationManager.GetString("POS_NotEnoughStock", "Not enough stock.")); return; }
                    r["Quantity"] = q + qtyToAdd;
                    RefreshCartDisplay();
                    return;
                }
            }
            if (qtyToAdd > stock) { MessageHelper.ShowWarning(LocalizationManager.GetString("POS_NotEnoughStock", "Not enough stock.")); return; }
            cartTable.Rows.Add(id, name, qtyToAdd, 0, price);
            RefreshCartDisplay();
        }

        // ---------------------------------------------------------------------
        // UPDATE TOTAL  (preserved ï¿½ works with lblSubtotalVal, lblTaxVal, etc.)
        // ---------------------------------------------------------------------
        private void UpdateTotal()
        {
            if (cartTable == null) return;
            decimal s = 0;
            foreach (DataRow r in cartTable.Rows)
                if (r.RowState != DataRowState.Deleted) s += (decimal)r["Total"];

            decimal t = chkApplyVAT.Checked ? (s * 0.11m) : 0;
            decimal ship = chkApplyShipping.Checked ? numShipping.Value : 0;
            decimal g = s + t + ship;

            if (lblSubtotalVal != null) lblSubtotalVal.Text = CurrencyService.Format(s);

            if (lblTaxVal != null)
            {
                lblTaxVal.Text = CurrencyService.Format(t);
                lblTaxVal.ForeColor = chkApplyVAT.Checked ? ThemeConfig.TextColorDark : Color.Gray;
            }

            if (lblShippingVal != null)
            {
                lblShippingVal.Text = CurrencyService.Format(ship);
                lblShippingVal.Visible = !chkApplyShipping.Checked;
            }

            if (lblTotalVal != null) lblTotalVal.Text = CurrencyService.Format(g);

            // Update order counter
            _sessionOrderCount++;
            if (lblOrderNum != null) lblOrderNum.Text = "#" + _sessionOrderCount.ToString("D3");
        }

        // ---------------------------------------------------------------------
        // CHECKOUT  (preserved exactly)
        // ---------------------------------------------------------------------
        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("CartEmpty", (LocalizationManager.GetString("CartEmpty"))));
                return;
            }

            decimal total = 0;
            foreach (DataRow row in cartTable.Rows) total += (decimal)row["Total"];

            if (ModernMessageBox.Show(string.Format(LocalizationManager.GetString("ConfirmSale"), $"{total:N2}"), "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;

            try
            {
                List<OrderItem> items = new List<OrderItem>();
                foreach (DataRow row in cartTable.Rows)
                    items.Add(new OrderItem { PartId = (int)row["PartID"], Quantity = (int)row["Quantity"], UnitPrice = (decimal)row["SellingPrice"] });
                int customerId = Convert.ToInt32(cmbCustomers.SelectedValue);
                DateTime? dDateC = _shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo) ? _shippingDetails.DeliveryDate : (DateTime?)null;
                DateTime? pDateC = _shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo) ? _shippingDetails.PaymentDueDate : (DateTime?)null;
                string sAddrC = _shippingDetails?.ShippingTo;
                int orderId = new OrderService().PlaceOrder(customerId, items, total, true, "Completed", pDateC, sAddrC, dDateC); // true = Paid
                DatabaseHelper.LogTransaction("SALE", "Order #" + orderId, "Paid Total: $" + total);
                // Notify all connected web POS tablets in real-time
                InventoryBroadcaster.BroadcastStockChange("desktop-sale");
                MessageHelper.ShowSuccess("Order Sent! Order #" + orderId);
                cartTable.Rows.Clear();
                RefreshCartDisplay();
                RefreshStats();
            }
            catch (Exception ex) { MessageHelper.ShowError("Error: " + ex.Message); }
        }



        // ---------------------------------------------------------------------
        // PRINT RECEIPT  (preserved exactly)
        // ---------------------------------------------------------------------
        private void BtnPrintReceipt_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0) { MessageHelper.ShowWarning("Cart is empty!"); return; }
            System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
            try { pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("Receipt", 315, 700); } catch { }
            pd.PrintPage += PrintReceiptPage;
            var preview = new PrintPreviewDialog
            {
                Document = pd,
                Text = LocalizationManager.GetString("POS_PrintReceipt")
            };
            ThemeConfig.ApplyPrintPreviewTheme(preview);
            preview.ShowDialog();
        }

        private void PrintReceiptPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font fH = new Font("Segoe UI", 12, FontStyle.Bold), fS = new Font("Segoe UI", 9), fI = new Font("Consolas", 9);
            int y = 20, m = 10, w = Math.Min(e.PageBounds.Width, 300) - (m * 2);
            StringFormat cA = new StringFormat { Alignment = StringAlignment.Center }, rA = new StringFormat { Alignment = StringAlignment.Far };
            g.DrawString(ThemeConfig.CompanyName.ToUpper(), fH, Brushes.Black, new Rectangle(m, y, w, 25), cA); y += 30;
            g.DrawString("SALES RECEIPT", fS, Brushes.Black, new Rectangle(m, y, w, 20), cA); y += 20;
            g.DrawString(DateTime.Now.ToString("g"), fS, Brushes.Black, new Rectangle(m, y, w, 20), cA); y += 25;
            g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;
            g.DrawString("QTY", fI, Brushes.Black, m, y); g.DrawString("ITEM", fI, Brushes.Black, m + 40, y); g.DrawString("PRICE", fI, Brushes.Black, new Rectangle(m, y, w, 20), rA);
            y += 20; g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;
            foreach (DataRow r in cartTable.Rows)
            {
                string n = r["PartName"].ToString(); if (n.Length > 18) n = n.Substring(0, 15) + "...";
                g.DrawString(r["Quantity"].ToString(), fI, Brushes.Black, m, y); g.DrawString(n, fI, Brushes.Black, m + 40, y);
                g.DrawString(CurrencyService.Format((decimal)r["Total"]), fI, Brushes.Black, new Rectangle(m, y, w, 20), rA); y += 20;
            }
            y += 10; g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;

            decimal s = 0; foreach (DataRow r in cartTable.Rows) if (r.RowState != DataRowState.Deleted) s += (decimal)r["Total"];
            decimal t = chkApplyVAT.Checked ? (s * 0.11m) : 0;
            decimal ship = chkApplyShipping.Checked ? numShipping.Value : 0;

            g.DrawString("Subtotal:", fS, Brushes.Black, m, y); g.DrawString(CurrencyService.Format(s), fS, Brushes.Black, new Rectangle(m, y, w, 20), rA); y += 20;
            if (t > 0) { g.DrawString("VAT (11%):", fS, Brushes.Black, m, y); g.DrawString(CurrencyService.Format(t), fS, Brushes.Black, new Rectangle(m, y, w, 20), rA); y += 20; }
            if (ship > 0) { g.DrawString("Shipping:", fS, Brushes.Black, m, y); g.DrawString(CurrencyService.Format(ship), fS, Brushes.Black, new Rectangle(m, y, w, 20), rA); y += 20; }
            y += 5; g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;

            g.DrawString("GRAND TOTAL:", fH, Brushes.Black, m, y); g.DrawString(lblTotalVal.Text, fH, Brushes.Black, new Rectangle(m, y, w, 25), rA);
            y += 40;
            if (_shippingDetails != null && !string.IsNullOrWhiteSpace(_shippingDetails.ShippingTo))
            {
                g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;
                g.DrawString("--- SHIPPING DETAILS ---", fS, Brushes.Black, new Rectangle(m, y, w, 20), cA); y += 25;

                if (_shippingDetails.SelectedCustomerId > 0)
                {
                    string customerName = Shaheen_InventoryManagement_Android.DatabaseHelper.ExecuteScalar<string>($"SELECT COALESCE(full_name, '') FROM customers WHERE customer_id = {_shippingDetails.SelectedCustomerId}");
                    if (!string.IsNullOrEmpty(customerName))
                    {
                        g.DrawString("Customer: " + customerName, fS, Brushes.Black, new Rectangle(m, y, w, 20)); y += 20;
                    }
                }

                string address = _shippingDetails.ShippingTo.Replace("\r\n", ", ").Replace("\n", ", ");
                g.DrawString("Address: " + address, fS, Brushes.Black, new Rectangle(m, y, w, 40)); y += 40;
                g.DrawString("Delivery: " + _shippingDetails.DeliveryDate.ToShortDateString(), fS, Brushes.Black, m, y); y += 20;
                y += 10;
            }
            g.DrawString("Thank you!", fS, Brushes.Black, new Rectangle(m, y, w, 20), cA); e.HasMorePages = false;
        }



        // ---------------------------------------------------------------------
        // BARCODE SCANNER  (preserved exactly)
        // ---------------------------------------------------------------------
        private void POSForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!this.Visible) return;

            TimeSpan elapsed = DateTime.Now - _lastScanTime;
            if (elapsed.TotalMilliseconds > 100)
                _scanBuffer = "";
            _lastScanTime = DateTime.Now;

            if (e.KeyChar != (char)Keys.Enter)
                _scanBuffer += e.KeyChar;
        }

        public bool HandleKeyPress(Keys keyData)
        {
            if (keyData == Keys.Enter && this.Visible)
            {
                TimeSpan elapsed = DateTime.Now - _lastScanTime;
                if (elapsed.TotalMilliseconds <= 100 && !string.IsNullOrEmpty(_scanBuffer))
                {
                    string barcode = _scanBuffer.Trim();
                    _scanBuffer = "";

                    DataTable dt = DatabaseHelper.ExecuteDataTable($"SELECT id,part_name,selling_price,quantity_in_stock FROM parts WHERE (barcode='{barcode}' OR part_number='{barcode}') AND date_deleted IS NULL");
                    if (dt.Rows.Count > 0)
                    {
                        DataRow r = dt.Rows[0];
                        AddToCart(Convert.ToInt32(r["id"]), r["part_name"].ToString(), Convert.ToDecimal(r["selling_price"]), Convert.ToInt32(r["quantity_in_stock"]));
                    }
                    else
                    {
                        string notFoundMsg = LocalizationManager.GetString("POS_ProductNotFound", $"Item not found for barcode: {barcode}");
                        MessageHelper.ShowInfo(notFoundMsg);
                    }

                    return true; // Suppress Enter key so it doesn't click focused buttons
                }
            }
            return false;
        }

        // ---------------------------------------------------------------------
        // LOCAL GetRoundedRect alias  (kept for CreateCardPanel compatibility)
        // ---------------------------------------------------------------------
        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            return RoundedPath(rect, radius);
        }

        private Panel CreateCardPanel()
        {
            Panel p = new Panel();
            p.BackColor = ThemeConfig.SurfaceColor;
            p.BorderStyle = BorderStyle.None;
            p.Padding = new Padding(15);
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Color parentColor = ThemeConfig.GetParentColor(p);
                using (var brush = new SolidBrush(parentColor))
                    e.Graphics.FillRectangle(brush, -1, -1, p.Width + 2, p.Height + 2);
                Rectangle r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                using (var path = GetRoundedRect(r, 15))
                {
                    using (var brush = new SolidBrush(ThemeConfig.SurfaceColor))
                        e.Graphics.FillPath(brush, path);
                    using (var pen = new Pen(ThemeConfig.BorderColor, 1f))
                        e.Graphics.DrawPath(pen, path);
                }
            };
            return p;
        }
    }
}
