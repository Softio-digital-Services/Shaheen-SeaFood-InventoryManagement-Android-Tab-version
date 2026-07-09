using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Shaheen_InventoryManagement_Android.Data;
using Shaheen_InventoryManagement_Android.Helpers;
using Shaheen_InventoryManagement_Android.Controls;
using Shaheen_InventoryManagement_Android.Services;

namespace Shaheen_InventoryManagement_Android.Forms
{
    /// <summary>
    /// Inventory Management Screen â€” card-view + category sidebar layout.
    /// </summary>
    public partial class PartsForm : UserControl
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
        // â”€â”€ Toolbar buttons â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private Shaheen_InventoryManagement_Android.Controls.ModernButton btnAdd;
        private Button btnAddCategory;
        // private Button btnFilter;
        private Button btnImport;
        private Button btnExport;
        private ModernTextBox txtSearch;
        private ModernTextBox txtCategorySearch;

        // â”€â”€ Layout containers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private Panel pnlCategoryList;   // scrollable category rows
        private FlowLayoutPanel pnlCardFlow;       // card grid
        private DataGridView dgvParts;           // list (table) view
        private Panel pnlGridView;        // wraps dgvParts
        private Panel pnlCardView;        // wraps pnlCardFlow + header
        private Label lblItemCount;       // "Desserts (19)"
        private Panel btnToggleGrid;
        private Panel btnToggleCard;

        // â”€â”€ State â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private InventoryService _inventoryService;
        private bool _isCardView = true;
        private string _activeCategory = null;   // null = "All Items"
        private string _searchText = "";
        private bool _lowStockOnly = false;
        private bool _activeOnly = false;
        private int _categorySortMode = 0; // 0=Default, 1=NameAsc, 2=NameDesc, 3=CountDesc, 4=CountAsc
        private int _currentPage = 1;
        private int _pageSize = 50;
        private Label lblPageInfo;
        private Shaheen_InventoryManagement_Android.Controls.ModernButton btnPrevPage;
        private Shaheen_InventoryManagement_Android.Controls.ModernButton btnNextPage;

        // â”€â”€ Card layout constants â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private const int CardW = 160;
        private const int CardH = 200;
        private const int CardGap = 14;

        public PartsForm()
        {
            InitializeComponent();
            _inventoryService = new InventoryService();

            EventHandler langHandler = (s, e) => ApplyLocalization();
            EventHandler currHandler = (s, e) => { if (_isCardView) LoadCards(); else dgvParts?.Invalidate(); };
            EventHandler invHandler = (s, e) => { if (this.Visible) RefreshAll(); };

            Shaheen_InventoryManagement_Android.Helpers.LocalizationManager.LanguageChanged += langHandler;
            Shaheen_InventoryManagement_Android.Services.CurrencyService.CurrencyChanged += currHandler;

            ApplyLocalization();
            ApplyPermissions();
            RefreshAll();

            this.Disposed += (s, e) =>
            {
                Shaheen_InventoryManagement_Android.Helpers.LocalizationManager.LanguageChanged -= langHandler;
                Shaheen_InventoryManagement_Android.Services.CurrencyService.CurrencyChanged -= currHandler;
            };
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // INITIALIZATION
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void InitializeComponent()
        {
            this.btnAdd = new Shaheen_InventoryManagement_Android.Controls.ModernButton();
            this.btnImport = new Button();
            this.btnExport = new Button();
            this.txtSearch = new ModernTextBox();
            this.dgvParts = new DataGridView();

            ((System.ComponentModel.ISupportInitialize)(this.dgvParts)).BeginInit();
            this.SuspendLayout();

            // â”€â”€ Root: full-width column for header + body â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            TableLayoutPanel tlpRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(20, 16, 20, 16)
            };
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // â”€â”€ Header â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            Label lblInventoryTitle = ThemeConfig.CreateStandardHeader(
                LocalizationManager.GetString("Parts_Title"));
            lblInventoryTitle.Name = "lblInventoryTitle";

            txtSearch = new ModernTextBox
            {
                IsSearch = true,
                ShowLabel = false,
                PlaceholderText = LocalizationManager.GetString("Parts_Search"),
                Size = new Size(280, 35)
            };
            txtSearch.TextChanged += (s, e) =>
            {
                _searchText = (txtSearch.Text == LocalizationManager.GetString("Parts_Search") ||
                               txtSearch.Text == "Search...") ? "" : txtSearch.Text;
                RefreshAll();
            };

            btnAdd.Size = new Size(120, 35);
            btnAdd.Click += BtnAdd_Click;
            ThemeConfig.ApplyStandardAddButton(btnAdd, "New");

            btnAddCategory = new Button { Size = new Size(140, 35) };
            btnAddCategory.Click += BtnAddCategory_Click;
            ThemeConfig.ApplyStandardAddButton(btnAddCategory, "Parts_AddCategory");

            Button btnDeleteSelected = new Button { Size = new Size(130, 35), Name = "btnDeleteSelected" };
            btnDeleteSelected.Click += (s, e) =>
            {
                var checkedIds = new List<int>();
                if (_isCardView)
                {
                    foreach (Control card in pnlCardFlow.Controls)
                    {
                        if (card is Panel)
                        {
                            foreach (Control c in card.Controls)
                            {
                                if (c is CheckBox chk && chk.Checked && chk.Tag is int pId)
                                {
                                    checkedIds.Add(pId);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (DataGridViewRow row in dgvParts.Rows)
                    {
                        var chkCell = row.Cells["colCheck"] as DataGridViewCheckBoxCell;
                        if (chkCell != null && Convert.ToBoolean(chkCell.Value ?? false))
                            if (int.TryParse(row.Cells["part_id"].Value?.ToString(), out int pId))
                                checkedIds.Add(pId);
                    }
                }

                if (checkedIds.Count == 0)
                {
                    string msg = LocalizationManager.GetString("Msg_SelectOne");
                    MessageHelper.ShowWarning(string.IsNullOrEmpty(msg) ? "Please select at least one item." : msg);
                    return;
                }

                string confirmMsg = LocalizationManager.GetString("Msg_ConfirmDelete");
                if (string.IsNullOrEmpty(confirmMsg)) confirmMsg = "Are you sure you want to delete {0} items?";

                if (MessageHelper.ConfirmAction(string.Format(confirmMsg, checkedIds.Count)))
                {
                    foreach (int i in checkedIds) _inventoryService.DeletePart(i);
                    RefreshAll();
                }
            };
            ThemeConfig.ApplyStandardDeleteButton(btnDeleteSelected, "Parts_Delete");

            btnImport.Size = new Size(100, 35);
            btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.FlatAppearance.BorderSize = 0;
            btnImport.Cursor = Cursors.Hand;
            btnImport.Click += BtnImport_Click;
            btnImport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnImport, e.Graphics, "import", "Parts_Import", Color.FromArgb(139, 92, 246), Color.FromArgb(139, 92, 246), true);

            btnExport.Size = new Size(100, 35);
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Cursor = Cursors.Hand;
            btnExport.Click += BtnExport_Click;
            btnExport.Paint += (s, e) => ThemeConfig.DrawIconButton(btnExport, e.Graphics, "export", "Parts_Export", ThemeConfig.PrimaryColor, ThemeConfig.PrimaryColor, true);

            var actionButtons = new Control[] { btnAdd, btnDeleteSelected, btnImport, btnExport };
            TableLayoutPanel tlpHeader = ThemeConfig.CreateGlobalFormHeader(lblInventoryTitle, txtSearch, actionButtons);
            tlpRoot.Controls.Add(tlpHeader, 0, 0);

            // â”€â”€ Body: sidebar + content â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            TableLayoutPanel tlpBody = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 8, 0, 0)
            };
            tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.Controls.Add(tlpBody, 0, 1);

            // â”€â”€ Left: Category Sidebar â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            Panel pnlSidebarOuter = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 12, 0),
                Tag = "surface"
            };
            pnlSidebarOuter.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedPath(new Rectangle(0, 0, pnlSidebarOuter.Width - 1, pnlSidebarOuter.Height - 1), 14))
                using (var brush = new SolidBrush(ThemeConfig.SurfaceColor))
                using (var pen = new Pen(ThemeConfig.BorderColor, 1f))
                {
                    pe.Graphics.FillPath(brush, path);
                    pe.Graphics.DrawPath(pen, path);
                }
            };
            tlpBody.Controls.Add(pnlSidebarOuter, 0, 0);

            // Sidebar inner layout
            TableLayoutPanel tlpSidebar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            tlpSidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));  // Title
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));  // Search
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Category rows
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));  // Add Category btn
            pnlSidebarOuter.Controls.Add(tlpSidebar);

            // Sidebar title Header Panel
            Panel pnlSidebarHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            tlpSidebar.Controls.Add(pnlSidebarHeader, 0, 0);

            // Sidebar title
            Label lblCatTitle = new Label
            {
                Text = LocalizationManager.GetString("Parts_Categories") == "Parts_Categories" ? "Categories" : LocalizationManager.GetString("Parts_Categories"),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Padding = new Padding(16, 0, 8, 0),
                AutoSize = true
            };
            pnlSidebarHeader.Controls.Add(lblCatTitle);

            Panel pbSortCat = new Panel
            {
                Size = new Size(26, 26),
                Location = new Point(230, 9),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            pbSortCat.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = ThemeConfig.GetRoundedPathPublic(new Rectangle(0, 0, pbSortCat.Width - 1, pbSortCat.Height - 1), 6))
                using (var pen = new Pen(ThemeConfig.BorderColor, 1f))
                    e.Graphics.DrawPath(pen, path);

                Image img = ThemeConfig.GetNuricon("filter") ?? ThemeConfig.GetNuricon("sort");
                if (img != null)
                {
                    using (var tinted = ThemeConfig.TintImage(img, ThemeConfig.SecondaryColor))
                    {
                        int iconSize = 16;
                        e.Graphics.DrawImage(tinted, new Rectangle((pbSortCat.Width - iconSize) / 2, (pbSortCat.Height - iconSize) / 2, iconSize, iconSize));
                    }
                }
            };
            pbSortCat.Click += (s, e) =>
            {
                var menu = new ContextMenuStrip();
                ThemeConfig.ApplyModernMenuTheme(menu);
                var m1 = menu.Items.Add("Default Order", null, (ms, me) => { _categorySortMode = 0; RefreshCategorySidebar(); });
                var m2 = menu.Items.Add("Name (A-Z)", null, (ms, me) => { _categorySortMode = 1; RefreshCategorySidebar(); });
                var m3 = menu.Items.Add("Name (Z-A)", null, (ms, me) => { _categorySortMode = 2; RefreshCategorySidebar(); });
                var m4 = menu.Items.Add("Highest Item Count", null, (ms, me) => { _categorySortMode = 3; RefreshCategorySidebar(); });
                var m5 = menu.Items.Add("Lowest Item Count", null, (ms, me) => { _categorySortMode = 4; RefreshCategorySidebar(); });

                if (_categorySortMode == 0) ((ToolStripMenuItem)m1).Checked = true;
                if (_categorySortMode == 1) ((ToolStripMenuItem)m2).Checked = true;
                if (_categorySortMode == 2) ((ToolStripMenuItem)m3).Checked = true;
                if (_categorySortMode == 3) ((ToolStripMenuItem)m4).Checked = true;
                if (_categorySortMode == 4) ((ToolStripMenuItem)m5).Checked = true;

                menu.Show(pbSortCat, new Point(0, pbSortCat.Height));
            };
            pnlSidebarHeader.Controls.Add(pbSortCat);

            // Category Search Box
            txtCategorySearch = new ModernTextBox
            {
                IsSearch = true,
                ShowLabel = false,
                PlaceholderText = "Search Categories...",
                Dock = DockStyle.Fill,
                Margin = new Padding(12, 0, 12, 8)
            };
            txtCategorySearch.TextChanged += (s, e) =>
            {
                string text = txtCategorySearch.Text;
                if (text != "Search Categories...")
                    RefreshCategorySidebar();
            };
            tlpSidebar.Controls.Add(txtCategorySearch, 0, 1);

            // Scrollable category list
            pnlCategoryList = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 4, 8, 4)
            };
            tlpSidebar.Controls.Add(pnlCategoryList, 0, 2);

            // Add Category button at bottom of sidebar
            Panel pnlAddCatWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = Color.Transparent };
            var btnSidebarAddCat = new Shaheen_InventoryManagement_Android.Controls.ModernButton
            {
                Dock = DockStyle.Fill,
                Height = 38
            };
            ThemeConfig.ApplyStandardAddButton(btnSidebarAddCat, LocalizationManager.GetString("Parts_AddCategory", "Add Category"));
            btnSidebarAddCat.Click += BtnAddCategory_Click;
            pnlAddCatWrapper.Controls.Add(btnSidebarAddCat);
            tlpSidebar.Controls.Add(pnlAddCatWrapper, 0, 3);

            // â”€â”€ Right: Content area â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            Panel pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Tag = "surface"
            };
            pnlContent.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedPath(new Rectangle(0, 0, pnlContent.Width - 1, pnlContent.Height - 1), 14))
                using (var brush = new SolidBrush(ThemeConfig.SurfaceColor))
                using (var pen = new Pen(ThemeConfig.BorderColor, 1f))
                {
                    pe.Graphics.FillPath(brush, path);
                    pe.Graphics.DrawPath(pen, path);
                }
            };
            tlpBody.Controls.Add(pnlContent, 1, 0);

            // Content layout
            TableLayoutPanel tlpContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent,
                Padding = new Padding(16)
            };
            tlpContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));  // sub-header
            tlpContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // cards / grid
            tlpContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // pagination footer

            Panel pnlPagination = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            btnPrevPage = new Shaheen_InventoryManagement_Android.Controls.ModernButton { Text = "< Prev", Size = new Size(80, 30), Location = new Point(0, 10), Cursor = Cursors.Hand };
            btnPrevPage.Click += (s, e) => { if (_currentPage > 1) { _currentPage--; RefreshAll(); } };
            ThemeConfig.ApplySecondaryButton(btnPrevPage);

            btnNextPage = new Shaheen_InventoryManagement_Android.Controls.ModernButton { Text = "Next >", Size = new Size(80, 30), Location = new Point(200, 10), Cursor = Cursors.Hand };
            btnNextPage.Click += (s, e) => { _currentPage++; RefreshAll(); };
            ThemeConfig.ApplySecondaryButton(btnNextPage);

            lblPageInfo = new Label { Text = "Page 1", AutoSize = false, Size = new Size(100, 30), Location = new Point(90, 10), TextAlign = ContentAlignment.MiddleCenter, Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.TextColorDark };

            pnlPagination.Controls.Add(btnPrevPage);
            pnlPagination.Controls.Add(lblPageInfo);
            pnlPagination.Controls.Add(btnNextPage);
            tlpContent.Controls.Add(pnlPagination, 0, 2);

            pnlContent.Controls.Add(tlpContent);

            // Content sub-header (title + count + view toggles)
            Panel pnlContentHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            tlpContent.Controls.Add(pnlContentHeader, 0, 0);

            lblItemCount = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                BackColor = Color.Transparent,
                Location = new Point(4, 8)
            };
            pnlContentHeader.Controls.Add(lblItemCount);

            // â”€â”€ Right toolbar: view toggles + filter button (matching green reference) â”€â”€â”€â”€â”€
            // Combined right-side control panel (FlowLayoutPanel for easier alignment)
            FlowLayoutPanel pnlRightControls = new FlowLayoutPanel
            {
                Size = new Size(110, 30),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            // View toggle group (card âŠž / list â‰¡)
            btnToggleCard = CreateToggleBtn("grid", true);
            btnToggleGrid = CreateToggleBtn("list", false);
            btnToggleCard.Click += (s, e) => SwitchView(true);
            btnToggleGrid.Click += (s, e) => SwitchView(false);

            // Filter button â€” panel-based for true transparency
            Panel btnContentFilter = new Panel
            {
                Size = new Size(26, 26),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Margin = new Padding(4, 1, 0, 0),
                Tag = ThemeConfig.SecondaryColor
            };
            btnContentFilter.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = ThemeConfig.GetRoundedPathPublic(new Rectangle(0, 0, btnContentFilter.Width - 1, btnContentFilter.Height - 1), 6))
                using (var pen = new Pen(ThemeConfig.BorderColor, 1f))
                    e.Graphics.DrawPath(pen, path);
                Image img = ThemeConfig.GetNuricon("filter");
                if (img != null)
                {
                    using (var tinted = ThemeConfig.TintImage(img, ThemeConfig.SecondaryColor))
                    {
                        int iconSize = 16;
                        e.Graphics.DrawImage(tinted, new Rectangle((btnContentFilter.Width - iconSize) / 2, (btnContentFilter.Height - iconSize) / 2, iconSize, iconSize));
                    }
                }
            };
            btnContentFilter.Click += BtnFilter_Click;

            pnlRightControls.Controls.Add(btnToggleCard);
            pnlRightControls.Controls.Add(btnToggleGrid);
            pnlRightControls.Controls.Add(btnContentFilter);

            pnlContentHeader.Resize += (s, e) =>
                pnlRightControls.Location = new Point(pnlContentHeader.Width - pnlRightControls.Width - 8, 4);
            pnlContentHeader.Controls.Add(pnlRightControls);

            // â”€â”€ Card view â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            pnlCardView = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            pnlCardFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(4, 4, 4, 4),
                BackColor = Color.Transparent,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };
            pnlCardView.Controls.Add(pnlCardFlow);
            tlpContent.Controls.Add(pnlCardView, 0, 1);

            // â”€â”€ List (DataGridView) view â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            pnlGridView = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };

            dgvParts.AllowUserToAddRows = false;
            dgvParts.ReadOnly = false;
            dgvParts.AutoGenerateColumns = false;
            dgvParts.BorderStyle = BorderStyle.None;
            dgvParts.BackgroundColor = ThemeConfig.SurfaceColor;
            dgvParts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvParts.CellPainting += DgvParts_CellPainting;
            dgvParts.CellFormatting += DgvParts_CellFormatting;
            dgvParts.CellMouseClick += DgvParts_CellMouseClick;
            dgvParts.CellMouseMove += DgvParts_CellMouseMove;
            dgvParts.CellMouseLeave += DgvParts_CellMouseLeave;
            dgvParts.DataError += (s, e) => e.ThrowException = false;

            Panel pnlGridCard = ThemeConfig.CreateCardPanel(dgvParts);
            pnlGridCard.Dock = DockStyle.Fill;
            pnlGridView.Controls.Add(pnlGridCard);
            tlpContent.Controls.Add(pnlGridView, 0, 1);

            // Define Columns
            dgvParts.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colCheck", HeaderText = "", Width = 30, FillWeight = 1, ReadOnly = false });
            var colImage = new DataGridViewImageColumn { Name = "colImage", HeaderText = "Image", Width = 60, ImageLayout = DataGridViewImageCellLayout.Zoom, FillWeight = 6, ReadOnly = true };
            colImage.DefaultCellStyle.Padding = new Padding(12);
            dgvParts.Columns.Add(colImage);
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colItemNo", HeaderText = "Item No.", DataPropertyName = "item_no", FillWeight = 10, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSKU", HeaderText = "SKU", DataPropertyName = "part_number", FillWeight = 10, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBarcode", HeaderText = "Barcode", DataPropertyName = "barcode", FillWeight = 10, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "Product", DataPropertyName = "part_name", FillWeight = 18, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCategory", HeaderText = "Category", DataPropertyName = "category_name", FillWeight = 12, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLocation", HeaderText = "Location", DataPropertyName = "location", FillWeight = 10, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colShelf", HeaderText = "Shelf", DataPropertyName = "shelf", FillWeight = 8, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStock", HeaderText = "Stock", DataPropertyName = "quantity_in_stock", FillWeight = 8, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "minimum_stock_level", HeaderText = "Min Stock", DataPropertyName = "minimum_stock_level", FillWeight = 8, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrice", HeaderText = "Price", DataPropertyName = "selling_price", FillWeight = 10, ReadOnly = true });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "Status", DataPropertyName = "status", FillWeight = 9, ReadOnly = true });
            var colActions = new DataGridViewButtonColumn { Name = "colActions", HeaderText = "Actions", ReadOnly = true, MinimumWidth = 130, Width = 130, AutoSizeMode = DataGridViewAutoSizeColumnMode.None };
            dgvParts.Columns.Add(colActions);
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "part_id", DataPropertyName = "part_id", Visible = false });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "part_image", DataPropertyName = "part_image", Visible = false });

            ThemeConfig.ApplyGridTheme(dgvParts);
            ThemeConfig.ApplyHeaderCheckBox(dgvParts, "colCheck");

            this.Controls.Add(tlpRoot);
            this.Dock = DockStyle.Fill;

            ((System.ComponentModel.ISupportInitialize)(this.dgvParts)).EndInit();
            this.ResumeLayout(false);
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // VIEW TOGGLE
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private Panel CreateToggleBtn(string iconName, bool startActive)
        {
            bool isActive = startActive;
            var pnl = new Panel
            {
                Size = new Size(26, 26),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 1, 4, 0),
                Tag = isActive  // store active state in Tag
            };

            pnl.Paint += (s, e) =>
            {
                bool active = pnl.Tag is bool b && b;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (var path = ThemeConfig.GetRoundedPathPublic(new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1), 6))
                {
                    if (active)
                    {
                        using (var brush = new SolidBrush(ThemeConfig.PrimaryColor))
                            e.Graphics.FillPath(brush, path);
                    }
                    else
                    {
                        using (var pen = new Pen(ThemeConfig.BorderColor, 1f))
                            e.Graphics.DrawPath(pen, path);
                    }
                }

                Image img = ThemeConfig.GetNuricon(iconName);
                if (img != null)
                {
                    Color tint = active ? Color.White : ThemeConfig.SecondaryColor;
                    using (var tinted = ThemeConfig.TintImage(img, tint))
                    {
                        int iconSize = 16;
                        e.Graphics.DrawImage(tinted, new Rectangle((pnl.Width - iconSize) / 2, (pnl.Height - iconSize) / 2, iconSize, iconSize));
                    }
                }
            };
            return pnl;
        }

        private void SwitchView(bool toCard)
        {
            _isCardView = toCard;
            pnlCardView.Visible = toCard;
            pnlGridView.Visible = !toCard;

            btnToggleCard.Tag = toCard;
            btnToggleGrid.Tag = !toCard;

            btnToggleCard.Invalidate();
            btnToggleGrid.Invalidate();

            if (toCard) LoadCards();
            else LoadData(_searchText, _lowStockOnly, _activeOnly, _activeCategory);
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // REFRESH ALL
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void RefreshAll()
        {
            RefreshCategorySidebar();
            if (_isCardView) LoadCards();
            else LoadData(_searchText, _lowStockOnly, _activeOnly, _activeCategory);
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // CATEGORY SIDEBAR
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void RefreshCategorySidebar()
        {
            pnlCategoryList.SuspendLayout();
            foreach (Control c in pnlCategoryList.Controls) c.Dispose();
            pnlCategoryList.Controls.Clear();

            int totalItems = CategoryData.GetTotalItemCount();
            pnlCategoryList.Controls.Add(BuildCategoryRow(null, "All Items", totalItems));

            try
            {
                var cats = CategoryData.GetAllCategories();

                string search = txtCategorySearch?.Text?.Trim().ToLower() ?? "";
                if (!string.IsNullOrEmpty(search) && search != "search categories...")
                {
                    cats = cats.Where(c => c.CategoryName.ToLower().Contains(search)).ToList();
                }

                var catWithCounts = cats.Select(c => new { Cat = c, Count = CategoryData.GetItemCount(c.CategoryName) }).ToList();

                if (_categorySortMode == 1)
                    catWithCounts = catWithCounts.OrderBy(x => x.Cat.CategoryName).ToList();
                else if (_categorySortMode == 2)
                    catWithCounts = catWithCounts.OrderByDescending(x => x.Cat.CategoryName).ToList();
                else if (_categorySortMode == 3)
                    catWithCounts = catWithCounts.OrderByDescending(x => x.Count).ThenBy(x => x.Cat.CategoryName).ToList();
                else if (_categorySortMode == 4)
                    catWithCounts = catWithCounts.OrderBy(x => x.Count).ThenBy(x => x.Cat.CategoryName).ToList();

                foreach (var item in catWithCounts)
                {
                    pnlCategoryList.Controls.Add(BuildCategoryRow(item.Cat, item.Cat.CategoryName, item.Count));
                }

                int othersCount = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM parts p LEFT JOIN categories c ON p.category_id = c.id WHERE p.date_deleted IS NULL AND (c.category_name IS NULL OR p.category_id = 0 OR c.category_name = '')");
                if (othersCount > 0)
                {
                    var othersCat = new CategoryData { CategoryName = "Others" };
                    pnlCategoryList.Controls.Add(BuildCategoryRow(othersCat, "Others", othersCount));
                }
            }
            catch { }

            pnlCategoryList.ResumeLayout();
        }

        private Panel BuildCategoryRow(CategoryData cat, string displayName, int count)
        {
            bool isActive = (_activeCategory == (cat?.CategoryName));

            // Wrapper for spacing
            Panel wrapper = new Panel
            {
                Height = 52,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 8),
                BackColor = Color.Transparent
            };

            // The actual card
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeConfig.SurfaceColor,
                Cursor = Cursors.Hand,
                Tag = cat
            };
            wrapper.Controls.Add(card);

            // Card custom painting (border & rounded corners)
            card.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedPath(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 8))
                using (var brush = new SolidBrush(card.BackColor))
                using (var pen = new Pen(isActive ? ThemeConfig.PrimaryColor : ThemeConfig.BorderColor, isActive ? 1.5f : 1f))
                {
                    pe.Graphics.FillPath(brush, path);
                    pe.Graphics.DrawPath(pen, path);
                }
            };

            Image iconImage = cat == null ? ThemeConfig.GetNuricon("dashboard") : ThemeConfig.GetNuricon("category_placeholder");
            if (cat != null && !string.IsNullOrEmpty(cat.CategoryImage))
            {
                try
                {
                    string fullPath = System.IO.Path.Combine(Application.StartupPath, cat.CategoryImage);
                    if (System.IO.File.Exists(fullPath))
                    {
                        using (var ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(fullPath)))
                        {
                            iconImage = Image.FromStream(ms);
                        }
                    }
                }
                catch { /* fallback to placeholder */ }
            }

            // Fake Icon / Image placeholder on the left
            PictureBox pbIcon = new PictureBox
            {
                Size = new Size(24, 24),
                Location = new Point(12, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = iconImage
            };
            if (pbIcon.Image == null) { pbIcon.BackColor = ThemeConfig.BorderColor; }
            card.Controls.Add(pbIcon);

            // Category name
            Label lblName = new Label
            {
                Text = displayName,
                Font = isActive ? new Font("Segoe UI", 9F, FontStyle.Bold) : new Font("Segoe UI", 9F),
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = false,
                Size = new Size(160, 44),
                Location = new Point(44, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblName);

            // Circular Count badge
            Label lblCount = new Label
            {
                Text = count.ToString(),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = isActive ? Color.White : ThemeConfig.TextColorDark,
                AutoSize = false,
                Size = new Size(24, 24),
                Location = new Point(214, 10),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = isActive ? ThemeConfig.PrimaryColor : ThemeConfig.BackgroundColor
            };
            lblCount.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                pe.Graphics.Clear(card.BackColor);
                using (var path = RoundedPath(new Rectangle(0, 0, lblCount.Width - 1, lblCount.Height - 1), lblCount.Width / 2))
                using (var brush = new SolidBrush(lblCount.BackColor))
                    pe.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(pe.Graphics, lblCount.Text, lblCount.Font,
                    new Rectangle(0, 0, lblCount.Width, lblCount.Height),
                    lblCount.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            card.Controls.Add(lblCount);

            // Click â€” filter by category
            EventHandler select = (s, e) =>
            {
                _activeCategory = cat?.CategoryName;
                RefreshCategorySidebar();
                if (_isCardView) LoadCards();
                else LoadData(_searchText, _lowStockOnly, _activeOnly, _activeCategory);
            };
            card.Click += select;
            pbIcon.Click += select;
            lblName.Click += select;
            lblCount.Click += select;

            // Right-click on real categories â†’ Edit
            if (cat != null)
            {
                Action editCategory = () =>
                {
                    using (AddCategoryForm f = new AddCategoryForm())
                    {
                        f.LoadCategoryData(cat.Id, cat.CategoryName, cat.Description, cat.CategoryImage);
                        if (f.ShowDialog() == DialogResult.OK)
                        {
                            _activeCategory = null;
                            RefreshAll();
                        }
                    }
                };

                Action deleteCategory = () =>
                {
                    int itemCount = CategoryData.GetItemCount(cat.CategoryName);
                    string warningMsg = itemCount > 0
                        ? $"Warning: There are {itemCount} items in this category.\n\nAre you sure you want to delete the category \"{cat.CategoryName}\"?"
                        : $"Delete category \"{cat.CategoryName}\"?";

                    if (MessageHelper.ConfirmAction(warningMsg))
                    {
                        try { CategoryData.DeleteCategory(cat.Id); }
                        catch (Exception ex) { MessageHelper.ShowError("Error deleting category: " + ex.Message); }
                        _activeCategory = null;
                        RefreshAll();
                    }
                };

                // Add Edit / Delete buttons that appear on hover
                PictureBox pbEdit = new PictureBox { Size = new Size(18, 18), Location = new Point(116, 13), Cursor = Cursors.Hand, Visible = false, BackColor = Color.Transparent };
                PictureBox pbDelete = new PictureBox { Size = new Size(18, 18), Location = new Point(140, 13), Cursor = Cursors.Hand, Visible = false, BackColor = Color.Transparent };

                pbEdit.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var img = ThemeConfig.TintImage(ThemeConfig.GetNuricon("edit"), ThemeConfig.SecondaryColor);
                    if (img != null) e.Graphics.DrawImage(img, new Rectangle(0, 0, pbEdit.Width, pbEdit.Height));
                };
                pbDelete.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var img = ThemeConfig.TintImage(ThemeConfig.GetNuricon("delete"), ThemeConfig.DangerColor);
                    if (img != null) e.Graphics.DrawImage(img, new Rectangle(0, 0, pbDelete.Width, pbDelete.Height));
                };

                pbEdit.Click += (s, e) => editCategory();
                pbDelete.Click += (s, e) => deleteCategory();

                card.Controls.Add(pbEdit);
                card.Controls.Add(pbDelete);
                pbEdit.BringToFront();
                pbDelete.BringToFront();

                void ShowActions(object s, EventArgs e) { pbEdit.Visible = true; pbDelete.Visible = true; }
                void HideActions(object s, EventArgs e)
                {
                    if (!card.ClientRectangle.Contains(card.PointToClient(Cursor.Position)))
                    {
                        pbEdit.Visible = false; pbDelete.Visible = false;
                    }
                }

                card.MouseEnter += ShowActions;
                pbIcon.MouseEnter += ShowActions;
                lblName.MouseEnter += ShowActions;
                lblCount.MouseEnter += ShowActions;

                card.MouseLeave += HideActions;
                pbIcon.MouseLeave += HideActions;
                lblName.MouseLeave += HideActions;
                lblCount.MouseLeave += HideActions;

                void ShowCatMenu(object s, EventArgs e)
                {
                    var menu = new ContextMenuStrip();
                    ThemeConfig.ApplyModernMenuTheme(menu);
                    menu.Items.Add("Edit Category", ThemeConfig.GetNuricon("edit"), (ms, me) => editCategory());
                    menu.Items.Add("Delete Category", ThemeConfig.GetNuricon("delete"), (ms, me) => deleteCategory());
                    menu.Show(card, new Point(10, card.Height));
                }
                card.MouseClick += (s, e) => { if (((MouseEventArgs)e).Button == MouseButtons.Right) ShowCatMenu(s, e); };
                lblName.MouseClick += (s, e) => { if (((MouseEventArgs)e).Button == MouseButtons.Right) ShowCatMenu(s, e); };
                pbIcon.MouseClick += (s, e) => { if (((MouseEventArgs)e).Button == MouseButtons.Right) ShowCatMenu(s, e); };
            }

            card.BringToFront();
            return wrapper;
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // CARD VIEW â€” LoadCards
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private async void LoadCards()
        {
            pnlCardFlow.SuspendLayout();
            foreach (Control c in pnlCardFlow.Controls) c.Dispose();
            pnlCardFlow.Controls.Clear();

            // "Add New" placeholder card
            pnlCardFlow.Controls.Add(BuildAddNewCard());

            try
            {
                int totalCount = await System.Threading.Tasks.Task.Run(() => _inventoryService.GetPartsCount(_searchText, _lowStockOnly, _activeOnly, _activeCategory));
                int totalPages = (int)Math.Ceiling(totalCount / (double)_pageSize);
                if (totalPages == 0) totalPages = 1;
                if (_currentPage > totalPages) _currentPage = totalPages;

                if (lblPageInfo != null) lblPageInfo.Text = $"Page {_currentPage} of {totalPages}";
                if (btnPrevPage != null) btnPrevPage.Enabled = _currentPage > 1;
                if (btnNextPage != null) btnNextPage.Enabled = _currentPage < totalPages;

                int offset = (_currentPage - 1) * _pageSize;

                DataTable dt = await System.Threading.Tasks.Task.Run(() => _inventoryService.GetAllParts(_searchText, _lowStockOnly, _activeOnly, _activeCategory, _pageSize, offset));

                // Update count label
                string catLabel = _activeCategory ?? "All Items";
                if (lblItemCount != null)
                    lblItemCount.Text = $"{catLabel} ({totalCount})";

                var controlsList = new System.Collections.Generic.List<Control>();
                foreach (DataRow row in dt.Rows)
                {
                    controlsList.Add(BuildProductCard(row));
                }

                pnlCardFlow.Controls.AddRange(controlsList.ToArray());
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Error loading inventory: {ex.Message}");
            }

            pnlCardFlow.ResumeLayout();
        }

        private Panel BuildAddNewCard()
        {
            Panel card = new Panel
            {
                Size = new Size(CardW, CardH),
                Margin = new Padding(CardGap / 2),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Tag = "surface"
            };
            card.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedPath(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 14))
                using (var pen = new Pen(ThemeConfig.SuccessColor, 1.5f) { DashStyle = DashStyle.Dash })
                    pe.Graphics.DrawPath(pen, path);
            };

            string addText = _activeCategory == null
                ? "New"
                : $"Add to {_activeCategory}";

            // Use a proper standard add button centered in the card
            var btnAdd = new Shaheen_InventoryManagement_Android.Controls.ModernButton
            {
                Size = new Size(CardW - 24, 40),
                Location = new Point(12, (CardH - 40) / 2)
            };
            ThemeConfig.ApplyStandardAddButton(btnAdd, addText);

            EventHandler addClick = (s, e) =>
            {
                using (AddProductServiceForm form = new AddProductServiceForm())
                {
                    if (form.ShowDialog() == DialogResult.OK) RefreshAll();
                }
            };
            card.Click += addClick;
            btnAdd.Click += addClick;

            card.Controls.Add(btnAdd);
            return card;
        }

        private Panel BuildProductCard(DataRow row)
        {
            int partId = Convert.ToInt32(row["part_id"]);
            string name = row["part_name"]?.ToString() ?? "";
            string sku = row["part_number"]?.ToString() ?? "";
            string category = row["category_name"]?.ToString() ?? "";
            decimal price = row["selling_price"] == DBNull.Value ? 0 : Convert.ToDecimal(row["selling_price"]);
            string status = row["status"]?.ToString() ?? "Active";
            string imgPath = row["part_image"]?.ToString() ?? "";
            string description = row["description"]?.ToString() ?? "";
            bool isActive = status.Equals("Active", StringComparison.OrdinalIgnoreCase);

            Panel card = new Panel
            {
                Size = new Size(CardW, CardH),
                Margin = new Padding(CardGap / 2),
                BackColor = ThemeConfig.SurfaceColor,
                Cursor = Cursors.Hand,
                Tag = partId
            };
            card.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedPath(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 14))
                using (var brush = new SolidBrush(ThemeConfig.SurfaceColor))
                using (var pen = new Pen(ThemeConfig.BorderColor, 1f))
                {
                    pe.Graphics.FillPath(brush, path);
                    pe.Graphics.DrawPath(pen, path);
                }
            };

            // Circular image
            PictureBox pb = new PictureBox
            {
                Size = new Size(64, 64),
                Location = new Point((CardW - 64) / 2, 18),
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            pb.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var clipPath = new GraphicsPath())
                {
                    clipPath.AddEllipse(0, 0, pb.Width, pb.Height);
                    pe.Graphics.SetClip(clipPath);
                }
                using (var b = new SolidBrush(ThemeConfig.BackgroundColor))
                    pe.Graphics.FillEllipse(b, 0, 0, pb.Width - 1, pb.Height - 1);

                var img = pb.Image;
                if (img != null)
                    pe.Graphics.DrawImage(img, new Rectangle(4, 4, pb.Width - 8, pb.Height - 8));

                using (var pen = new Pen(ThemeConfig.BorderColor, 1f))
                    pe.Graphics.DrawEllipse(pen, 0, 0, pb.Width - 1, pb.Height - 1);
            };
            pb.Image = CreateProductImage(imgPath, category);

            // Category label (muted)
            Label lblCat = new Label
            {
                Text = category,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = ThemeConfig.SecondaryColor,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(CardW - 12, 18),
                Location = new Point(6, 88),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Product name (bold)
            Label lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(CardW - 12, 18),
                Location = new Point(6, 106),
                TextAlign = ContentAlignment.TopCenter
            };

            // Product description
            Label lblDesc = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(CardW - 12, 16),
                Location = new Point(6, 124),
                TextAlign = ContentAlignment.TopCenter
            };

            // Price
            Label lblPrice = new Label
            {
                Text = Shaheen_InventoryManagement_Android.Services.CurrencyService.Format(price),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = ThemeConfig.PrimaryColor,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(CardW - 12, 22),
                Location = new Point(6, 143),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Status badge
            Label lblStatus = new Label
            {
                Text = isActive ? (LocalizationManager.GetString("Status_Active", "Active"))
                                : (LocalizationManager.GetString("Status_Inactive", "Inactive")),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = isActive ? ThemeConfig.SuccessBadgeText : ThemeConfig.DangerBadgeText,
                BackColor = isActive ? ThemeConfig.SuccessBadgeBg : ThemeConfig.DangerBadgeBg,
                AutoSize = false,
                Size = new Size(60, 20),
                Location = new Point((CardW - 60) / 2, 168),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblStatus.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedPath(new Rectangle(0, 0, lblStatus.Width - 1, lblStatus.Height - 1), 8))
                using (var fill = new SolidBrush(lblStatus.BackColor))
                    pe.Graphics.FillPath(fill, path);
                TextRenderer.DrawText(pe.Graphics, lblStatus.Text, lblStatus.Font,
                    new Rectangle(0, 0, lblStatus.Width, lblStatus.Height),
                    lblStatus.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            // Edit / Delete icons
            PictureBox pbEdit = new PictureBox
            {
                Size = new Size(22, 22),
                Location = new Point(CardW - 52, 5),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = ThemeConfig.GetNuricon("edit")
            };
            pbEdit.Click += (s, e) => OpenEditForm(row);

            PictureBox pbDelete = new PictureBox
            {
                Size = new Size(22, 22),
                Location = new Point(CardW - 28, 5),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = ThemeConfig.GetNuricon("delete")
            };
            pbDelete.Click += (s, e) =>
            {
                if (!UserSession.IsAdmin) { MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoPermissionDelete")); return; }
                if (MessageHelper.ConfirmAction("Delete this item?"))
                {
                    _inventoryService.DeletePart(partId);
                    RefreshAll();
                }
            };

            // Click on card = edit
            EventHandler editClick = (s, e) => OpenEditForm(row);
            card.Click += editClick;
            pb.Click += editClick;
            lblName.Click += editClick;
            lblCat.Click += editClick;
            lblPrice.Click += editClick;

            card.Controls.Add(pb);
            card.Controls.Add(lblCat);
            card.Controls.Add(lblName);
            card.Controls.Add(lblDesc);
            card.Controls.Add(lblPrice);
            card.Controls.Add(lblStatus);
            card.Controls.Add(pbEdit);
            card.Controls.Add(pbDelete);

            CheckBox chkSelect = new CheckBox
            {
                AutoSize = true,
                Location = new Point(8, 8),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Tag = partId
            };
            card.Controls.Add(chkSelect);

            // Hover effect
            void HoverEnter(object s, EventArgs e)
            {
                card.BackColor = Color.FromArgb(248, 248, 252);
                card.Invalidate();
            }
            void HoverLeave(object s, EventArgs e)
            {
                card.BackColor = ThemeConfig.SurfaceColor;
                card.Invalidate();
            }
            card.MouseEnter += HoverEnter; card.MouseLeave += HoverLeave;
            pb.MouseEnter += HoverEnter; pb.MouseLeave += HoverLeave;
            lblName.MouseEnter += HoverEnter; lblName.MouseLeave += HoverLeave;

            return card;
        }

        private void OpenEditForm(DataRow row)
        {
            if (!UserSession.IsAdmin) { MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoPermissionEdit")); return; }

            string id = row["part_id"]?.ToString();
            string name = row["part_name"]?.ToString() ?? "";
            string sku = row["part_number"]?.ToString() ?? "";
            string category = row["category_name"]?.ToString() ?? "";
            string status = row["status"]?.ToString() ?? "Active";
            string barcode = row["barcode"]?.ToString() ?? "";
            string location = row["location"]?.ToString() ?? "";
            string shelf = row["shelf"]?.ToString() ?? "";
            string image = row["part_image"]?.ToString() ?? "";

            int qty = row["quantity_in_stock"] == DBNull.Value ? 0 : Convert.ToInt32(row["quantity_in_stock"]);
            decimal price = row["selling_price"] == DBNull.Value ? 0 : Convert.ToDecimal(row["selling_price"]);
            int minStock = row["minimum_stock_level"] == DBNull.Value ? 0 : Convert.ToInt32(row["minimum_stock_level"]);

            var fullPart = PartData.GetAllParts().Find(p => p.Id == int.Parse(id));
            if (fullPart != null)
            {
                using (AddProductServiceForm form = new AddProductServiceForm())
                {
                    form.LoadPartData(fullPart);
                    if (form.ShowDialog() == DialogResult.OK) { RefreshAll(); MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_UpdateSuccess")); }
                }
            }
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // LIST VIEW â€” LoadData (existing DataGridView approach)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private async void LoadData(string search = "", bool lowStockOnly = false, bool activeOnly = false, string category = null)
        {
            try
            {
                dgvParts.DataSource = null;

                int totalCount = await System.Threading.Tasks.Task.Run(() => _inventoryService.GetPartsCount(search, lowStockOnly, activeOnly, category));
                int totalPages = (int)Math.Ceiling(totalCount / (double)_pageSize);
                if (totalPages == 0) totalPages = 1;
                if (_currentPage > totalPages) _currentPage = totalPages;

                if (lblPageInfo != null) lblPageInfo.Text = $"Page {_currentPage} of {totalPages}";
                if (btnPrevPage != null) btnPrevPage.Enabled = _currentPage > 1;
                if (btnNextPage != null) btnNextPage.Enabled = _currentPage < totalPages;

                int offset = (_currentPage - 1) * _pageSize;

                DataTable dt = await System.Threading.Tasks.Task.Run(() => _inventoryService.GetAllParts(search, lowStockOnly, activeOnly, category, _pageSize, offset));

                dgvParts.DataSource = dt;

                if (lblItemCount != null)
                    lblItemCount.Text = $"{(category ?? "All Items")} ({totalCount})";

                foreach (DataGridViewRow r in dgvParts.Rows)
                {
                    string imagePath = r.Cells["part_image"].Value?.ToString();
                    string categoryName = r.Cells["colCategory"].Value?.ToString();
                    r.Cells["colImage"].Value = CreateProductImage(imagePath, categoryName);
                }
            }
            catch (Exception ex) { MessageHelper.ShowError($"Error: {ex.Message}"); }
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // LOCALIZATION
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void ApplyLocalization()
        {
            Shaheen_InventoryManagement_Android.Helpers.LocalizationManager.ApplyRTL(this);
            Func<string, string> L = Shaheen_InventoryManagement_Android.Helpers.LocalizationManager.GetString;

            var ctrlTitle = this.Controls.Find("lblInventoryTitle", true);
            if (ctrlTitle.Length > 0) ctrlTitle[0].Text = L("Parts_Title");
            if (txtSearch != null) txtSearch.PlaceholderText = L("Parts_Search");

            if (btnAdd != null) ThemeConfig.ApplyStandardAddButton(btnAdd, "New");
            // if (btnFilter != null)  btnFilter.Invalidate();
            if (btnImport != null) btnImport.Invalidate();
            if (btnExport != null) btnExport.Invalidate();

            var ctrlDel = this.Controls.Find("btnDeleteSelected", true);
            if (ctrlDel.Length > 0 && ctrlDel[0] is Button bDel) ThemeConfig.ApplyStandardDeleteButton(bDel, "Parts_Delete");

            Shaheen_InventoryManagement_Android.Helpers.LocalizationManager.TranslateControl(this);

            if (dgvParts != null && dgvParts.Columns.Count > 0)
            {
                if (dgvParts.Columns.Contains("colImage")) dgvParts.Columns["colImage"].HeaderText = L("Parts_GridImage");
                if (dgvParts.Columns.Contains("colSKU")) dgvParts.Columns["colSKU"].HeaderText = L("Parts_GridSKU");
                if (dgvParts.Columns.Contains("colBarcode")) dgvParts.Columns["colBarcode"].HeaderText = L("Parts_GridBarcode");
                if (dgvParts.Columns.Contains("colName")) dgvParts.Columns["colName"].HeaderText = L("Parts_GridProduct");
                if (dgvParts.Columns.Contains("colCategory")) dgvParts.Columns["colCategory"].HeaderText = L("Parts_GridCategory");
                if (dgvParts.Columns.Contains("colLocation")) dgvParts.Columns["colLocation"].HeaderText = L("Parts_GridLocation");
                if (dgvParts.Columns.Contains("colShelf")) dgvParts.Columns["colShelf"].HeaderText = L("Parts_GridShelf");
                if (dgvParts.Columns.Contains("colStock")) dgvParts.Columns["colStock"].HeaderText = L("Parts_GridStock");
                if (dgvParts.Columns.Contains("minimum_stock_level")) dgvParts.Columns["minimum_stock_level"].HeaderText = L("Parts_GridMinStock");
                if (dgvParts.Columns.Contains("colPrice")) dgvParts.Columns["colPrice"].HeaderText = L("Parts_GridPrice");
                if (dgvParts.Columns.Contains("colStatus")) dgvParts.Columns["colStatus"].HeaderText = L("Parts_GridStatus");
                if (dgvParts.Columns.Contains("colActions")) dgvParts.Columns["colActions"].HeaderText = L("Parts_GridActions");
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible && !this.DesignMode)
            {
                RefreshAll();
                this.ActiveControl = null;
            }
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // IMAGE HELPER
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private Bitmap CreateProductImage(string imagePath = null, string category = null)
        {
            return Shaheen_InventoryManagement_Android.Helpers.CacheManager.GetProductImage(imagePath, category, 56);
        }


        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // DATAGRIDVIEW PAINTING (for list view)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void DgvParts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvParts.Rows[e.RowIndex];
            string category = row.Cells["colCategory"].Value?.ToString() ?? "";
            bool isService = category.Equals("Services", StringComparison.OrdinalIgnoreCase) ||
                             category.Equals("Service", StringComparison.OrdinalIgnoreCase);

            if (isService)
            {
                if (dgvParts.Columns[e.ColumnIndex].Name == "colStock") { e.Value = "-"; e.FormattingApplied = true; }
                if (dgvParts.Columns[e.ColumnIndex].Name == "minimum_stock_level") { e.Value = "-"; e.FormattingApplied = true; }
            }

            if (dgvParts.Columns[e.ColumnIndex].Name == "colPrice" && e.Value != null)
                if (decimal.TryParse(e.Value.ToString(), out decimal p)) { e.Value = Shaheen_InventoryManagement_Android.Services.CurrencyService.Format(p); e.FormattingApplied = true; }

            var stockCell = row.Cells["colStock"]; var minCell = row.Cells["minimum_stock_level"];
            if (!isService && stockCell.Value != null && minCell.Value != null)
            {
                if (int.TryParse(stockCell.Value.ToString(), out int stock) && int.TryParse(minCell.Value.ToString(), out int minS))
                {
                    if (stock <= minS) { row.DefaultCellStyle.BackColor = ThemeConfig.DangerBadgeBg; row.DefaultCellStyle.SelectionBackColor = ThemeConfig.DangerLight; row.DefaultCellStyle.SelectionForeColor = ThemeConfig.TextColorDark; }
                    else { row.DefaultCellStyle.BackColor = ThemeConfig.SurfaceColor; row.DefaultCellStyle.SelectionBackColor = ThemeConfig.SelectionBackColor; row.DefaultCellStyle.SelectionForeColor = ThemeConfig.TextColorDark; }
                }
            }
        }

        private void DgvParts_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (dgvParts.Columns[e.ColumnIndex].Name == "colStock")
            {
                var row = dgvParts.Rows[e.RowIndex];
                string cat = row.Cells["colCategory"].Value?.ToString() ?? "";
                bool isSvc = cat.Equals("Services", StringComparison.OrdinalIgnoreCase) || cat.Equals("Service", StringComparison.OrdinalIgnoreCase);
                if (isSvc) return;
                var minCell = row.Cells["minimum_stock_level"];
                if (e.Value != null && minCell.Value != null)
                {
                    int.TryParse(e.Value.ToString(), out int stock);
                    int.TryParse(minCell.Value.ToString(), out int minS);
                    if (stock <= minS)
                    {
                        e.Handled = true; e.PaintBackground(e.CellBounds, true);
                        SizeF ts = e.Graphics.MeasureString(e.Value.ToString(), e.CellStyle.Font);
                        float pillW = Math.Max(ts.Width + 16, 40);
                        float pillX = e.CellBounds.X + (e.CellBounds.Width - pillW) / 2;
                        RectangleF pillRect = new RectangleF(pillX, e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, pillW, 24);
                        using (var path = GetRoundedRect(Rectangle.Round(pillRect), 8))
                        using (var brush = new SolidBrush(ThemeConfig.DangerLight))
                            e.Graphics.FillPath(brush, path);
                        TextRenderer.DrawText(e.Graphics, e.Value.ToString(), ThemeConfig.SmallBoldFont, Rectangle.Round(pillRect), ThemeConfig.DangerBadgeText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        return;
                    }
                }
            }

            if (dgvParts.Columns[e.ColumnIndex].Name == "colStatus")
            {
                e.Handled = true; e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "Active";
                bool isAct = status.Equals("Active", StringComparison.OrdinalIgnoreCase);
                string dispStatus = isAct ? LocalizationManager.GetString("Status_Active") : LocalizationManager.GetString("Status_Inactive");
                Color borderC = isAct ? ThemeConfig.SuccessBorder : ThemeConfig.DangerBorder;
                Color txtC = isAct ? ThemeConfig.SuccessBadgeText : ThemeConfig.DangerBadgeText;
                Color fillC = isAct ? ThemeConfig.SuccessBadgeBg : ThemeConfig.DangerBadgeBg;
                Rectangle badgeRect = new Rectangle(e.CellBounds.X + 5, e.CellBounds.Y + 13, 60, 24);
                using (var path = GetRoundedRect(badgeRect, 10))
                using (var pen = new Pen(borderC, 1))
                using (var brush = new SolidBrush(fillC))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                    TextRenderer.DrawText(e.Graphics, dispStatus, ThemeConfig.MicroBoldFont, badgeRect, txtC, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }

            if (dgvParts.Columns[e.ColumnIndex].Name == "colActions")
            {
                e.Handled = true; e.PaintBackground(e.CellBounds, true);
                Image imgEdit = ThemeConfig.GetNuricon("edit"); Image imgDel = ThemeConfig.GetNuricon("delete");
                Rectangle editRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + (e.CellBounds.Height - 32) / 2, 32, 32);
                Rectangle delRect = new Rectangle(e.CellBounds.X + 48, e.CellBounds.Y + (e.CellBounds.Height - 32) / 2, 32, 32);
                Rectangle adjRect = new Rectangle(e.CellBounds.X + 88 + 2, e.CellBounds.Y + (e.CellBounds.Height - 28) / 2, 28, 28);
                if (imgEdit != null) e.Graphics.DrawImage(imgEdit, editRect);
                if (imgDel != null) e.Graphics.DrawImage(imgDel, delRect);
                Image imgAdj = ThemeConfig.GetNuricon("item_adjustment");
                if (imgAdj != null) e.Graphics.DrawImage(imgAdj, adjRect);
            }
        }

        private void DgvParts_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvParts.Columns[e.ColumnIndex].Name == "colActions")
            {
                if (e.X >= 10 && e.X <= 40)
                {
                    if (!UserSession.IsAdmin) { MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoPermissionEdit")); return; }
                    var row = dgvParts.Rows[e.RowIndex];
                    string id = row.Cells["part_id"].Value?.ToString();
                    if (string.IsNullOrEmpty(id)) return;
                    string name = row.Cells["colName"].Value?.ToString() ?? "";
                    string sku = row.Cells["colSKU"].Value?.ToString() ?? "";
                    int qty = Convert.ToInt32(row.Cells["colStock"].Value ?? 0);
                    decimal price = Convert.ToDecimal(row.Cells["colPrice"].Value ?? 0);
                    string status = row.Cells["colStatus"].Value?.ToString() ?? "Active";
                    string barcode = row.Cells["colBarcode"].Value?.ToString() ?? "";
                    string location = row.Cells["colLocation"].Value?.ToString() ?? "";
                    string shelf = row.Cells["colShelf"].Value?.ToString() ?? "";
                    string image = row.Cells["part_image"].Value?.ToString() ?? "";
                    string category = row.Cells["colCategory"].Value?.ToString() ?? "";
                    int minStock = Convert.ToInt32(row.Cells["minimum_stock_level"].Value ?? 0);

                    var fullPart = PartData.GetAllParts().Find(p => p.Id == int.Parse(id));
                    if (fullPart != null)
                    {
                        using (AddProductServiceForm form = new AddProductServiceForm())
                        { form.LoadPartData(fullPart); if (form.ShowDialog() == DialogResult.OK) { RefreshAll(); MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_UpdateSuccess")); } }
                    }
                }
                else if (e.X >= 50 && e.X <= 80)
                {
                    string id = dgvParts.Rows[e.RowIndex].Cells["part_id"].Value?.ToString();
                    if (string.IsNullOrEmpty(id)) return;
                    if (!UserSession.IsAdmin) { MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_NoPermissionDelete")); return; }
                    if (MessageHelper.ConfirmAction("Delete this item?")) { _inventoryService.DeletePart(int.Parse(id)); RefreshAll(); }
                }
                else if (e.X >= 90 && e.X <= 120)
                {
                    var row = dgvParts.Rows[e.RowIndex];
                    ShowAdjustmentDialog(int.Parse(row.Cells["part_id"].Value?.ToString() ?? "0"), row.Cells["colName"].Value?.ToString() ?? "");
                }
            }
        }

        private void DgvParts_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvParts.Cursor = (e.RowIndex >= 0 && dgvParts.Columns[e.ColumnIndex].Name == "colActions") ? Cursors.Hand : Cursors.Default;
        }

        private void DgvParts_CellMouseLeave(object sender, DataGridViewCellEventArgs e) => dgvParts.Cursor = Cursors.Default;

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // ADJUSTMENT DIALOG
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void ShowAdjustmentDialog(int partId, string partName)
        {
            string title = LocalizationManager.GetString("Msg_AdjustStock") + partName;
            BaseModalForm f = new BaseModalForm { TitleText = title, Size = new Size(450, 280) };
            TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 5, AutoSize = true, Padding = new Padding(10) };
            for (int i = 0; i < 5; i++) tlp.RowStyles.Add(i < 4 ? new RowStyle(SizeType.AutoSize) : new RowStyle(SizeType.Absolute, 60F));
            ModernNumericUpDown numQty = new ModernNumericUpDown { LabelText = LocalizationManager.GetString("Msg_AdjustInstr", "Enter quantity to add (+) or subtract (-):"), Width = 380, Minimum = -99999, Maximum = 99999, Margin = new Padding(0, 0, 0, 20) };
            Label lblReason = new Label { Text = LocalizationManager.GetString("Msg_Reason"), AutoSize = true, Font = ThemeConfig.StandardFont, Margin = new Padding(0, 0, 0, 5) };
            TextBox txtReason = new TextBox { Width = 380, Font = ThemeConfig.StandardFont, Margin = new Padding(0, 0, 0, 20), Multiline = true, Height = 80 };
            Button btnSave = new ModernButton { Text = LocalizationManager.GetString("Msg_Adjust"), Size = new Size(120, 35), Anchor = AnchorStyles.Right };
            ThemeConfig.ApplyPrimaryButton(btnSave);
            btnSave.Click += (s, e) =>
            {
                if (numQty.Value == 0) { MessageHelper.ShowWarning(LocalizationManager.GetString("Msg_AdjZero")); return; }
                if (string.IsNullOrWhiteSpace(txtReason.Text)) { MessageHelper.ShowWarning("Please provide a reason."); return; }
                try { _inventoryService.AdjustStock(partId, (int)numQty.Value, txtReason.Text); InventoryBroadcaster.BroadcastStockChange("desktop-adjustment"); MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_AdjSuccess")); f.DialogResult = DialogResult.OK; f.Close(); RefreshAll(); }
                catch (Exception ex) { MessageHelper.ShowError("Error: " + ex.Message); }
            };
            tlp.Controls.Add(numQty, 0, 1); tlp.Controls.Add(lblReason, 0, 2); tlp.Controls.Add(txtReason, 0, 3); tlp.Controls.Add(btnSave, 0, 4);
            f.ContentPanel.Controls.Add(tlp);
            LocalizationManager.ApplyRTL(f);
            f.ShowDialog();
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // BUTTON HANDLERS (unchanged)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (AddProductServiceForm form = new AddProductServiceForm())
            { if (form.ShowDialog() == DialogResult.OK) RefreshAll(); }
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ThemeConfig.ApplyModernMenuTheme(menu);
            menu.Items.Add("All Items", null, (s, a) => { _lowStockOnly = false; _activeOnly = false; _activeCategory = null; RefreshAll(); });
            menu.Items.Add("Low Stock Only", null, (s, a) => { _lowStockOnly = true; _activeOnly = false; RefreshAll(); });
            menu.Items.Add("Active Only", null, (s, a) => { _lowStockOnly = false; _activeOnly = true; RefreshAll(); });
            menu.Items.Add(new ToolStripSeparator());
            try
            {
                foreach (var cat in CategoryData.GetAllCategories())
                {
                    ToolStripMenuItem catItem = new ToolStripMenuItem(cat.CategoryName);
                    catItem.Click += (s, a) => { _activeCategory = cat.CategoryName; RefreshAll(); };
                    ToolStripMenuItem editItem = new ToolStripMenuItem("Edit Category", ThemeConfig.GetNuricon("edit"));
                    editItem.Click += (s, a) =>
                    {
                        using (AddCategoryForm f = new AddCategoryForm())
                        { f.LoadCategoryData(cat.Id, cat.CategoryName, cat.Description, cat.CategoryImage); if (f.ShowDialog() == DialogResult.OK) RefreshAll(); }
                    };
                    catItem.DropDownItems.Add(editItem);
                    menu.Items.Add(catItem);
                }
            }
            catch { }

            if (sender is Control btn)
            {
                menu.Show(btn, new Point(0, btn.Height));
            }
        }

        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            using (AddCategoryForm form = new AddCategoryForm())
            { if (form.ShowDialog() == DialogResult.OK) { RefreshAll(); MessageHelper.ShowSuccess("Category added!"); } }
        }

        private void BtnExport_Click(object sender, EventArgs e) => ExportToCsv();
        private void BtnImport_Click(object sender, EventArgs e) => ImportFromCsv();

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // PERMISSIONS
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void ApplyPermissions()
        {
            if (!UserSession.IsAdmin)
            {
                if (btnAdd != null) btnAdd.Visible = false;
                if (btnImport != null) btnImport.Visible = false;
                var ctrlDel = this.Controls.Find("btnDeleteSelected", true);
                if (ctrlDel.Length > 0) ctrlDel[0].Visible = false;
            }
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // DRAWING HELPERS
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private static GraphicsPath RoundedPath(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, rad, rad, 180, 90);
            p.AddArc(r.Right - rad, r.Y, rad, rad, 270, 90);
            p.AddArc(r.Right - rad, r.Bottom - rad, rad, rad, 0, 90);
            p.AddArc(r.X, r.Bottom - rad, rad, rad, 90, 90);
            p.CloseFigure();
            return p;
        }

        private static GraphicsPath GetRoundedRect(Rectangle rect, int radius) => RoundedPath(rect, radius);

        private static Image ResizeImage(Image img, int w, int h)
        {
            var bmp = new System.Drawing.Bitmap(w, h);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(img, 0, 0, w, h);
            }
            return bmp;
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // EXPORT / IMPORT (preserved exactly)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void ExportToCsv()
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog { Filter = "Excel Files (*.xls)|*.xls|CSV Files (*.csv)|*.csv", FileName = $"Items_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xls", Title = "Export Items to Excel or CSV" };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                DataTable dt = _inventoryService.GetAllParts(_searchText, _lowStockOnly, _activeOnly, _activeCategory);
                if (dt == null || dt.Rows.Count == 0) { MessageHelper.ShowWarning("No data to export."); return; }

                // Keep the exact same columns as the actual database query
                DataTable exportDt = dt.Copy();

                // Clear any inherited constraints (like Primary Key on part_id) so we can remove columns
                exportDt.PrimaryKey = null;
                exportDt.Constraints.Clear();

                // We can just remove 'part_id' or keep it. Let's remove part_id and part_image so we export raw data cleanly.
                if (exportDt.Columns.Contains("part_id")) exportDt.Columns.Remove("part_id");
                if (exportDt.Columns.Contains("part_image")) exportDt.Columns.Remove("part_image");

                bool success;
                string ext = System.IO.Path.GetExtension(dlg.FileName).ToLower();
                if (ext == ".xls")
                {
                    success = Helpers.ImportExportHelper.ExportToExcel(exportDt, dlg.FileName, "Items");
                }
                else
                {
                    success = Helpers.ImportExportHelper.ExportToCsv(exportDt, dlg.FileName);
                }

                if (success)
                    MessageHelper.ShowSuccess($"Exported {exportDt.Rows.Count} items successfully!");
                else
                    MessageHelper.ShowError("Failed to export data.");
            }
            catch (Exception ex) { MessageHelper.ShowError($"Export error: {ex.Message}"); }
        }

        private void ImportFromCsv()
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog { Filter = "Excel/CSV Files (*.xls;*.csv)|*.xls;*.csv|Excel Files (*.xls)|*.xls|CSV Files (*.csv)|*.csv", Title = "Import Items from Excel or CSV" };
                if (dlg.ShowDialog() != DialogResult.OK) return;

                DataTable dt;
                string ext = System.IO.Path.GetExtension(dlg.FileName).ToLower();
                if (ext == ".xls")
                {
                    dt = Helpers.ImportExportHelper.ImportFromExcel(dlg.FileName);
                }
                else
                {
                    dt = Helpers.ImportExportHelper.ImportFromCsv(dlg.FileName);
                }

                if (dt == null || dt.Rows.Count == 0) { MessageHelper.ShowWarning("No data found in the file."); return; }

                // Allow formats checking by finding column dynamically
                string nameColumn = null;
                string[] possibleNameCols = { "ingredient", "Ingredient", "name", "Name", "part_name", "PartName" };
                foreach (var col in possibleNameCols)
                {
                    if (dt.Columns.Contains(col))
                    {
                        nameColumn = col;
                        break;
                    }
                }

                if (nameColumn == null) { MessageHelper.ShowError("Invalid file format. Could not find ingredient or name column."); return; }

                bool isNewFormat = nameColumn != "PartName";
                bool isOldFormat = nameColumn == "PartName";

                int imported = 0, skipped = 0;
                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        string name = row[nameColumn].ToString();
                        string pn = isNewFormat && dt.Columns.Contains("part_number") ? row["part_number"].ToString() : (isOldFormat && dt.Columns.Contains("PartNumber") ? row["PartNumber"].ToString() : "");

                        if (string.IsNullOrWhiteSpace(name)) { skipped++; continue; }
                        if (!string.IsNullOrWhiteSpace(pn) && _inventoryService.PartExists(pn)) { skipped++; continue; }

                        var p = new Shaheen_InventoryManagement_Android.Data.PartData();
                        p.PartName = name;
                        p.PartNumber = pn;

                        string itemNoColumn = null;
                        string[] possibleItemNoCols = { "item_no", "itemno", "ItemNo", "Item No", "item no", "No." };
                        foreach (var col in possibleItemNoCols)
                        {
                            if (dt.Columns.Contains(col))
                            {
                                itemNoColumn = col;
                                break;
                            }
                        }
                        if (itemNoColumn != null) p.ItemNo = row[itemNoColumn].ToString();

                        if (isNewFormat)
                        {
                            if (dt.Columns.Contains("category_name")) p.CategoryName = row["category_name"].ToString();
                            if (dt.Columns.Contains("description")) p.Description = row["description"].ToString();
                            if (dt.Columns.Contains("quantity_in_stock")) p.QuantityInStock = int.TryParse(row["quantity_in_stock"].ToString(), out int q) ? q : 0;
                            if (dt.Columns.Contains("minimum_stock_level")) p.MinimumStockLevel = int.TryParse(row["minimum_stock_level"].ToString(), out int m) ? m : 0;
                            if (dt.Columns.Contains("reorder_quantity")) p.ReorderQuantity = int.TryParse(row["reorder_quantity"].ToString(), out int rq) ? rq : 0;
                            if (dt.Columns.Contains("purchase_price")) p.PurchasePrice = decimal.TryParse(row["purchase_price"].ToString(), out decimal pp) ? pp : 0;
                            if (dt.Columns.Contains("selling_price")) p.SellingPrice = decimal.TryParse(row["selling_price"].ToString(), out decimal sp) ? sp : 0;
                            if (dt.Columns.Contains("location")) p.Location = row["location"].ToString();
                            if (dt.Columns.Contains("shelf")) p.Shelf = row["shelf"].ToString();
                            if (dt.Columns.Contains("barcode")) p.Barcode = row["barcode"].ToString();
                            if (dt.Columns.Contains("status")) p.Status = row["status"].ToString();
                            if (dt.Columns.Contains("item_type")) p.ItemType = row["item_type"].ToString();
                            if (dt.Columns.Contains("unit_of_measure")) p.UnitOfMeasure = row["unit_of_measure"].ToString();
                            if (dt.Columns.Contains("batch_number")) p.BatchNumber = row["batch_number"].ToString();
                            if (dt.Columns.Contains("expiry_date")) p.ExpiryDate = row["expiry_date"].ToString();
                            if (dt.Columns.Contains("is_sales_item")) p.IsSalesItem = row["is_sales_item"].ToString() == "1" || row["is_sales_item"].ToString().ToLower() == "true";
                            if (dt.Columns.Contains("is_purchase_item")) p.IsPurchaseItem = row["is_purchase_item"].ToString() == "1" || row["is_purchase_item"].ToString().ToLower() == "true";
                            if (dt.Columns.Contains("is_inactive")) p.IsInactive = row["is_inactive"].ToString() == "1" || row["is_inactive"].ToString().ToLower() == "true";
                            if (dt.Columns.Contains("tax_rate")) p.TaxRate = decimal.TryParse(row["tax_rate"].ToString(), out decimal t) ? t : 0;
                            if (dt.Columns.Contains("is_stock_tracked")) p.IsStockTracked = row["is_stock_tracked"].ToString() == "1" || row["is_stock_tracked"].ToString().ToLower() == "true";
                            if (dt.Columns.Contains("price2")) p.Price2 = decimal.TryParse(row["price2"].ToString(), out decimal p2) ? p2 : 0;
                            if (dt.Columns.Contains("price3")) p.Price3 = decimal.TryParse(row["price3"].ToString(), out decimal p3) ? p3 : 0;
                            if (dt.Columns.Contains("price4")) p.Price4 = decimal.TryParse(row["price4"].ToString(), out decimal p4) ? p4 : 0;
                        }
                        else
                        {
                            if (dt.Columns.Contains("Category")) p.CategoryName = row["Category"].ToString();
                            if (dt.Columns.Contains("Quantity")) p.QuantityInStock = int.TryParse(row["Quantity"].ToString(), out int q) ? q : 0;
                            if (dt.Columns.Contains("MinimumStock")) p.MinimumStockLevel = int.TryParse(row["MinimumStock"].ToString(), out int m) ? m : 0;
                            if (dt.Columns.Contains("UnitPrice")) p.SellingPrice = decimal.TryParse(row["UnitPrice"].ToString(), out decimal sp) ? sp : 0;
                            if (dt.Columns.Contains("Location")) p.Location = row["Location"].ToString();
                            if (dt.Columns.Contains("Status")) p.Status = row["Status"].ToString();
                        }

                        _inventoryService.SaveProductService(p);
                        imported++;
                    }
                    catch { skipped++; }
                }
                RefreshAll();
                MessageHelper.ShowSuccess($"Import complete!\nImported: {imported}\nSkipped: {skipped}");
            }
            catch (Exception ex) { MessageHelper.ShowError($"Import error: {ex.Message}"); }
        }
    }
}
