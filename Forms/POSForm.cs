using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Data;
using System.Collections.Generic;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Services;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    public partial class POSForm : UserControl
    {
        private DataGridView dgvCart;
        private ComboBox cmbCustomers;
        private FlatDateTimePicker dtOrderDate;
        private FlatDateTimePicker dtDeliveryDate;
        private FlatDateTimePicker dtDueDate;
        private TextBox txtShippingAddress;
        private Label lblSubtotalVal, lblTaxVal, lblShippingVal, lblTotalVal;
        private CheckBox chkApplyVAT, chkApplyShipping;
        private NumericUpDown numShipping;
        private Button btnCheckout, btnAddItem, btnManageDrafts, btnClearCart, btnQuotation, btnPayLater, btnReturnItems;
        private StatCard cardTodayOrders, cardTodaySales, cardPending;
        private DataTable cartTable;
        private DashboardService _dashboardService;
        
        // Barcode Scanner Buffer
        private DateTime _lastScanTime = DateTime.Now;
        private string _scanBuffer = "";
        
        public POSForm() { InitializeComponent(); _dashboardService = new DashboardService(); LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization(); ApplyLocalization(); ApplyPermissions(); }

        private void ApplyLocalization() {
            LocalizationManager.ApplyRTL(this); LocalizationManager.TranslateControl(this);
            Func<string, string> L = LocalizationManager.GetString;
            Action<string, string> setText = (name, key) => { var ctrls = this.Controls.Find(name, true); if (ctrls.Length > 0) ctrls[0].Text = L(key); };
            setText("lblPOSTitle", "POS_Title"); setText("lblCustTitle", "POS_Customer"); setText("lblDateTitle", "POS_OrderDate"); setText("lblDelTitle", "POS_DeliveryDate");
            setText("lblDueTitle", "Tran_DueDateLabel");
            setText("lblAddrTitle", "POS_ShippingTo"); setText("lblLineItems", "POS_LineItems"); setText("lblTotalsTitle", "POS_OrderSummary"); setText("btnDraft", "POS_SaveDraft");
            if(btnQuotation != null) btnQuotation.Text = L("POS_SaveQuotation");
            if(btnAddItem != null) btnAddItem.Text = L("POS_AddItem");
            if(btnCheckout != null) btnCheckout.Text = L("POS_Checkout");
            if(btnPayLater != null) btnPayLater.Text = L("POS_PayLater");
            if(btnReturnItems != null) btnReturnItems.Text = L("Return_Action") ?? ("Return Items");
            setText("lblTotal_Subtotal", "POS_Subtotal"); setText("lblTotal_VAT (11%)", "POS_Tax"); setText("lblTotal_Shipping", "POS_Shipping"); setText("lblTotal_Grand Total", "POS_GrandTotal");
            if(btnManageDrafts != null) btnManageDrafts.Text = L("POS_ManageDrafts"); if(btnClearCart != null) btnClearCart.Text = L("POS_ClearCart");
            if(cardTodayOrders != null) cardTodayOrders.Title = L("POS_Orders"); if(cardTodaySales != null) cardTodaySales.Title = L("POS_Sales"); if(cardPending != null) cardPending.Title = L("POS_Pending");
            var btnPR = this.Controls.Find("btnPrintReceipt", true); if (btnPR.Length > 0) btnPR[0].Text = L("POS_PrintReceipt");
            if(dgvCart != null && dgvCart.Columns.Count > 0) {
                if(dgvCart.Columns.Contains("PartName")) dgvCart.Columns["PartName"].HeaderText = L("POS_GridProduct");
                if(dgvCart.Columns.Contains("Quantity")) dgvCart.Columns["Quantity"].HeaderText = L("POS_GridQty");
                if(dgvCart.Columns.Contains("Stock")) dgvCart.Columns["Stock"].HeaderText = L("POS_GridStock");
                if(dgvCart.Columns.Contains("SellingPrice")) dgvCart.Columns["SellingPrice"].HeaderText = L("POS_GridPrice");
                if(dgvCart.Columns.Contains("Total")) dgvCart.Columns["Total"].HeaderText = L("POS_GridTotal");
            }
        }

        protected override void OnVisibleChanged(EventArgs e) { base.OnVisibleChanged(e); if (this.Visible && !this.DesignMode) { if (cartTable == null) InitializeCart(); RefreshStats(); LoadCustomers(); } }

        public void RefreshStats() { try { cardTodayOrders.Value = _dashboardService.GetOrdersCount("Today").ToString(); cardTodaySales.Value = "$" + _dashboardService.GetSales("Today").ToString("N0"); cardPending.Value = _dashboardService.GetPendingOrdersCount().ToString(); } catch (Exception ex) { Console.WriteLine("Stats Error: " + ex.Message); } }

        private void InitializeComponent() {
            this.SuspendLayout(); this.Size = new Size(1100, 750); this.BackColor = ThemeConfig.BackgroundColor; 
            TableLayoutPanel tlpRoot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(20), BackColor = ThemeConfig.BackgroundColor };
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize)); tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 320F));
            this.Controls.Add(tlpRoot);

            Panel pnlHeader = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            Label lblPOSTitle = ThemeConfig.CreateStandardHeader("Create Sales Order");
            lblPOSTitle.Name = "lblPOSTitle";
            pnlHeader.Controls.Add(lblPOSTitle);
            tlpRoot.Controls.Add(pnlHeader, 0, 0);

            // The header panel now only contains the title. Buttons moved to Line Items panel.

            TableLayoutPanel tlpStats = new TableLayoutPanel { Dock = DockStyle.Top, Height = 110, ColumnCount = 3, Margin = new Padding(0, 5, 0, 10) };
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F)); tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F)); tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            cardTodayOrders = new StatCard { Title = "Orders", Value = "0", IconImage = ThemeConfig.GetNuricon("orders"), ThemeColor = ThemeConfig.PrimaryColor, Dock = DockStyle.Fill };
            cardTodaySales = new StatCard { Title = "Sales", Value = "$0", IconImage = ThemeConfig.GetNuricon("revenue"), ThemeColor = ThemeConfig.SuccessColor, Dock = DockStyle.Fill };
            cardPending = new StatCard { Title = "Pending", Value = "0", IconImage = ThemeConfig.GetNuricon("pending"), IconPadding = 12, ThemeColor = ThemeConfig.WarningColor, Dock = DockStyle.Fill };
            tlpStats.Controls.Add(cardTodayOrders, 0, 0); tlpStats.Controls.Add(cardTodaySales, 1, 0); tlpStats.Controls.Add(cardPending, 2, 0);
            tlpRoot.Controls.Add(tlpStats, 0, 1);

            Panel pnlInfo = CreateCardPanel(); pnlInfo.Padding = new Padding(0, 0, 0, 10); pnlInfo.Dock = DockStyle.Fill;
            TableLayoutPanel tblInfo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(15) };
            tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F)); tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tblInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 85F)); tblInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 85F)); tblInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlInfo.Controls.Add(tblInfo);

            Panel pnlCol1 = new Panel { Dock = DockStyle.Top, AutoSize = true, BackColor = Color.Transparent, Padding = new Padding(0,0,10,0) }; 
            pnlCol1.Controls.Add(new Label { Text = "Customer", Name = "lblCustTitle", AutoSize = false, Height = 25, Font = ThemeConfig.SubHeaderFont, ForeColor = ThemeConfig.TextColorDark, Dock = DockStyle.Top });
            cmbCustomers = new ComboBox(); ThemeConfig.ApplyComboBoxStyle(cmbCustomers);
            cmbCustomers.SelectedIndexChanged += (s, e) => {
                if (btnPayLater != null) {
                    int custId = -1;
                    if (cmbCustomers.SelectedValue is int id) custId = id;
                    else if (cmbCustomers.SelectedValue != null) int.TryParse(cmbCustomers.SelectedValue.ToString(), out custId);
                    btnPayLater.Enabled = custId != -1;
                }
            };
            Panel pnlCustWrapper = ThemeConfig.WrapInStyledInput(cmbCustomers, 42); pnlCustWrapper.Location = new Point(0, 26); pnlCustWrapper.Width = 160; pnlCustWrapper.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Button btnAddCust = new ModernButton { Name = "btnAddCust", Text = "", Image = ThemeConfig.GetNuricon("add"), TextImageRelation = TextImageRelation.Overlay, ImageAlign = ContentAlignment.MiddleCenter, Size = new Size(35, 42), Location = new Point(pnlCustWrapper.Right + 5, 26), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnAddCust.Click += (s, e) => { var form = new AddCustomerForm(); if(form.ShowDialog() == DialogResult.OK) { CustomerService svc = new CustomerService(); int newId = svc.AddCustomer(form.CustomerName, form.Phone, form.Email, form.Address, form.CustomerType, form.CreditLimit); LoadCustomers(); if(newId > 0) cmbCustomers.SelectedValue = newId; } };
            ThemeConfig.ApplyPrimaryButton(btnAddCust);
            pnlCol1.Controls.Add(pnlCustWrapper); pnlCol1.Controls.Add(btnAddCust);
            tblInfo.Controls.Add(pnlCol1, 0, 0);

            Panel pnlDate1 = new Panel { Dock = DockStyle.Top, Height = 75, Margin = new Padding(0,0,10,5) };
            pnlDate1.Controls.Add(new Label { Text = "Order Date", Name = "lblDateTitle", AutoSize = false, Height = 25, Font = ThemeConfig.SubHeaderFont, ForeColor = ThemeConfig.TextColorDark, Dock = DockStyle.Top });
            dtOrderDate = new FlatDateTimePicker { Width = 150 };
            Panel pnlDate1Input = ThemeConfig.WrapInStyledInput(dtOrderDate, 42);
            pnlDate1Input.Dock = DockStyle.Top;
            pnlDate1.Controls.Add(pnlDate1Input); pnlDate1Input.BringToFront(); 
            tblInfo.Controls.Add(pnlDate1, 0, 1);

            Panel pnlDate2 = new Panel { Dock = DockStyle.Top, Height = 75, Margin = new Padding(0,0,10,0) };
            pnlDate2.Controls.Add(new Label { Text = "Delivery", Name = "lblDelTitle", AutoSize = false, Height = 25, Font = ThemeConfig.SubHeaderFont, ForeColor = ThemeConfig.TextColorDark, Dock = DockStyle.Top });
            dtDeliveryDate = new FlatDateTimePicker { Width = 150, Value = null, MinDate = DateTime.Today };
            Panel pnlDate2Input = ThemeConfig.WrapInStyledInput(dtDeliveryDate, 42);
            pnlDate2Input.Dock = DockStyle.Top;
            pnlDate2.Controls.Add(pnlDate2Input); pnlDate2Input.BringToFront(); 
            tblInfo.Controls.Add(pnlDate2, 0, 2);

            Panel pnlDate3 = new Panel { Dock = DockStyle.Top, Height = 75, Margin = new Padding(0, 0, 10, 0) };
            pnlDate3.Controls.Add(new Label { Text = "Due Date", Name = "lblDueTitle", AutoSize = false, Height = 25, Font = ThemeConfig.SubHeaderFont, ForeColor = ThemeConfig.TextColorDark, Dock = DockStyle.Top });
            dtDueDate = new FlatDateTimePicker { Width = 150, Value = DateTime.Today.AddDays(30) };
            Panel pnlDate3Input = ThemeConfig.WrapInStyledInput(dtDueDate, 42);
            pnlDate3Input.Dock = DockStyle.Top;
            pnlDate3.Controls.Add(pnlDate3Input); pnlDate3Input.BringToFront(); 
            tblInfo.Controls.Add(pnlDate3, 1, 2);

            Panel pnlCol3 = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            pnlCol3.Controls.Add(new Label { Text = "Shipping To", Name = "lblAddrTitle", AutoSize = false, Height = 25, Font = ThemeConfig.SubHeaderFont, ForeColor = ThemeConfig.TextColorDark, Dock = DockStyle.Top });
            txtShippingAddress = new TextBox { Multiline = true, Dock = DockStyle.Fill, Font = ThemeConfig.StandardFont, BorderStyle = BorderStyle.None };
            Panel pnlAddrWrapper = ThemeConfig.WrapInStyledInput(txtShippingAddress, 210, true); pnlAddrWrapper.Dock = DockStyle.Fill;
            pnlCol3.Controls.Add(pnlAddrWrapper); pnlAddrWrapper.BringToFront(); tblInfo.Controls.Add(pnlCol3, 1, 0); tblInfo.SetRowSpan(pnlCol3, 2);

            Panel pnlItems = CreateCardPanel(); pnlItems.Dock = DockStyle.Fill; pnlItems.Margin = new Padding(0, 0, 0, 10);
            TableLayoutPanel tlpGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(20) };
            tlpGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); tlpGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlItems.Controls.Add(tlpGrid);

            Panel pnlGridHeader = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            pnlGridHeader.Controls.Add(new Label { Text = "Line Items", Name = "lblLineItems", Font = ThemeConfig.SubHeaderFont, ForeColor = ThemeConfig.TextColorDark, AutoSize = true, Location = new Point(0, 15) });
            
            FlowLayoutPanel gridButtonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                WrapContents = false,
                Height = 50,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = ThemeConfig.SurfaceColor // Fix corners
            };

            btnAddItem = new ModernButton { Size = new Size(135, 36), Text = LocalizationManager.GetString("POS_AddItem"), Image = ThemeConfig.GetNuricon("add"), TextImageRelation = TextImageRelation.ImageBeforeText, ImageAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), Margin = new Padding(5, 7, 0, 0) };
            btnAddItem.Click += BtnAddItem_Click; ThemeConfig.ApplyPrimaryButton(btnAddItem); 

            ModernButton btnBlindReturn = new ModernButton { Text = LocalizationManager.GetString("POS_ItemReturn"), Size = new Size(120, 36), Margin = new Padding(5, 7, 0, 0) };
            btnBlindReturn.Click += (s, e) => { var form = new BlindReturnForm(); form.ShowDialog(); };
            ThemeConfig.ApplyPaletteButton(btnBlindReturn, ThemeConfig.WarningColor);

            btnReturnItems = new ModernButton { Text = LocalizationManager.GetString("POS_ReturnItems"), Size = new Size(120, 36), Margin = new Padding(5, 7, 0, 0) };
            btnReturnItems.Click += BtnReturnItems_Click; ThemeConfig.ApplyPaletteButton(btnReturnItems, ThemeConfig.SecondaryColor);

            btnManageDrafts = new ModernButton { Text = LocalizationManager.GetString("POS_ManageDrafts"), Size = new Size(130, 36), Margin = new Padding(5, 7, 0, 0) };
            btnManageDrafts.Click += BtnLoadDraft_Click; ThemeConfig.ApplyPaletteButton(btnManageDrafts, ThemeConfig.WarningColor);

            btnClearCart = new ModernButton { Text = LocalizationManager.GetString("POS_ClearCart"), Size = new Size(110, 36), Margin = new Padding(0, 7, 0, 0) };
            btnClearCart.Click += (s, e) => { if(cartTable.Rows.Count > 0 && MessageHelper.ConfirmAction(LocalizationManager.GetString("POS_ClearCartConfirm"))) { cartTable.Rows.Clear(); UpdateTotal(); MessageHelper.ShowSuccess(LocalizationManager.GetString("POS_ClearCartSuccess")); } };
            ThemeConfig.ApplyPaletteButton(btnClearCart, ThemeConfig.DangerColor);

            gridButtonsPanel.Controls.Add(btnAddItem);
            gridButtonsPanel.Controls.Add(btnBlindReturn);
            gridButtonsPanel.Controls.Add(btnReturnItems);
            gridButtonsPanel.Controls.Add(btnManageDrafts);
            gridButtonsPanel.Controls.Add(btnClearCart);

            pnlGridHeader.Controls.Add(gridButtonsPanel);
            tlpGrid.Controls.Add(pnlGridHeader, 0, 0);

            dgvCart = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoGenerateColumns = false, BorderStyle = BorderStyle.None, BackgroundColor = ThemeConfig.SurfaceColor };
            dgvCart.CellContentClick += DgvCart_CellContentClick; dgvCart.CellValueChanged += DgvCart_CellValueChanged; dgvCart.CellPainting += DgvCart_CellPainting; 
            ThemeConfig.ApplyGridTheme(dgvCart); tlpGrid.Controls.Add(dgvCart, 0, 1); tlpRoot.Controls.Add(pnlItems, 0, 2);

            TableLayoutPanel tlpBottomArea = new TableLayoutPanel { Dock = DockStyle.Top, Height = 310, ColumnCount = 2, Margin = new Padding(0, 10, 0, 0), BackColor = Color.Transparent };
            tlpBottomArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F)); tlpBottomArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            tlpBottomArea.Controls.Add(pnlInfo, 0, 0);
            
            Panel pnlTotals = CreateCardPanel(); pnlTotals.Dock = DockStyle.Fill;
            pnlTotals.Controls.Add(new Label { Text = LocalizationManager.GetString("POS_OrderSummary"), Name = "lblTotalsTitle", Location = new Point(20, 15), AutoSize = true, Font = ThemeConfig.SubHeaderFont, ForeColor = ThemeConfig.TextColorDark });
            ComboBox cboCurrency = new ComboBox();
            cboCurrency.DropDownStyle = ComboBoxStyle.DropDownList;
            ThemeConfig.ApplyComboBoxStyle(cboCurrency);
            Panel currPanel = ThemeConfig.WrapInStyledInput(cboCurrency, 42); 
            currPanel.Width = 110; 
            currPanel.Location = new Point(pnlTotals.Width - currPanel.Width - 25, 15);
            currPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            foreach (var c in CurrencyService.SupportedCurrencies) cboCurrency.Items.Add(c);
            // Select USD by default
            for(int i=0; i<cboCurrency.Items.Count; i++) if((cboCurrency.Items[i] as CurrencyInfo)?.Code == "USD") { cboCurrency.SelectedIndex = i; break; }
            cboCurrency.SelectedIndexChanged += (s, e) => { if (cboCurrency.SelectedItem is CurrencyInfo selected) { CurrencyService.ActiveCurrency = selected.Code; UpdateTotal(); } };
            pnlTotals.Controls.Add(currPanel);

            Action<string, string, int, bool> addTotalRow = (l, v, y, b) => {
                 pnlTotals.Controls.Add(new Label { Text = l, Name = "lblTotal_" + l, Location = new Point(20, y), AutoSize = true, Font = ThemeConfig.StandardFont, ForeColor = ThemeConfig.SecondaryColor });
                 Label val = new Label { Text = v, Name = "lblVal_" + l, Size = new Size(130, 20), Location = new Point(pnlTotals.Width - 130 - 25, y), TextAlign = ContentAlignment.MiddleRight, Font = b ? ThemeConfig.SubHeaderFont : ThemeConfig.StandardFont, ForeColor = ThemeConfig.TextColorDark, Anchor = AnchorStyles.Top | AnchorStyles.Right };
                 pnlTotals.Controls.Add(val);
                 if(l == "Subtotal") lblSubtotalVal = val; else if(l.Contains("VAT")) lblTaxVal = val; else if(l == "Shipping") lblShippingVal = val; else if(l.Contains("Grand")) lblTotalVal = val;
             };
             addTotalRow(LocalizationManager.GetString("POS_Subtotal"), "$0.00", 78, false); 
             
             // Add VAT Checkbox
             chkApplyVAT = new CheckBox { Text = "", Checked = false, AutoSize = true, Location = new Point(20, 105), Cursor = Cursors.Hand };
             chkApplyVAT.CheckedChanged += (s, e) => UpdateTotal();
             pnlTotals.Controls.Add(chkApplyVAT);
             
             addTotalRow(LocalizationManager.GetString("POS_VAT"), "$0.00", 103, false); 
             var lblTax = pnlTotals.Controls.Find("lblTotal_" + LocalizationManager.GetString("POS_VAT"), true)[0];
             lblTax.Location = new Point(45, 103); 
             
             // Add Shipping Toggle and Input
             chkApplyShipping = new CheckBox { Text = "", Checked = false, AutoSize = true, Location = new Point(20, 130), Cursor = Cursors.Hand };
             chkApplyShipping.CheckedChanged += (s, e) => { numShipping.Visible = chkApplyShipping.Checked; UpdateTotal(); };
             pnlTotals.Controls.Add(chkApplyShipping);

             addTotalRow(LocalizationManager.GetString("POS_Shipping"), "$0.00", 128, false);
             var lblShip = pnlTotals.Controls.Find("lblTotal_" + LocalizationManager.GetString("POS_Shipping"), true)[0];
             lblShip.Location = new Point(45, 128);

             numShipping = new NumericUpDown { DecimalPlaces = 2, Width = 80, Location = new Point(pnlTotals.Width - 80 - 25, 126), Visible = false, Font = ThemeConfig.StandardFont, Anchor = AnchorStyles.Top | AnchorStyles.Right };
             numShipping.ValueChanged += (s, e) => UpdateTotal();
             pnlTotals.Controls.Add(numShipping); numShipping.BringToFront();

             addTotalRow(LocalizationManager.GetString("POS_GrandTotal"), "$0.00", 168, true);
                 FlowLayoutPanel pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 10, 10, 10), BackColor = ThemeConfig.SurfaceColor };
              
              btnCheckout = new ModernButton { Text = LocalizationManager.GetString("POS_Checkout"), Size = new Size(140, 40), Image = ThemeConfig.GetNuricon("pos"), TextImageRelation = TextImageRelation.ImageBeforeText, ImageAlign = ContentAlignment.MiddleLeft, Margin = new Padding(5, 0, 0, 0) };
              btnCheckout.Click += BtnCheckout_Click; ThemeConfig.ApplyPrimaryButton(btnCheckout); 

              btnPayLater = new ModernButton { Name = "btnPayLater", Size = new Size(150, 40), Text = LocalizationManager.GetString("POS_PayLater"), Image = ThemeConfig.GetNuricon("history"), TextImageRelation = TextImageRelation.ImageBeforeText, ImageAlign = ContentAlignment.MiddleLeft, Margin = new Padding(5, 0, 0, 0), Cursor = Cursors.Hand };
              btnPayLater.Enabled = false; btnPayLater.Click += BtnPayLater_Click; ThemeConfig.ApplyPaletteButton(btnPayLater, Color.FromArgb(255, 152, 0));

              Button btnDraft = new ModernButton { Name = "btnDraft", Size = new Size(140, 40), Text = LocalizationManager.GetString("POS_Draft"), Image = ThemeConfig.GetNuricon("export"), TextImageRelation = TextImageRelation.ImageBeforeText, ImageAlign = ContentAlignment.MiddleLeft, Margin = new Padding(5, 0, 0, 0), Cursor = Cursors.Hand };
              btnDraft.Click += BtnSaveDraft_Click; ThemeConfig.ApplySecondaryButton(btnDraft); 

              Button btnPrintReceipt = new ModernButton { Name = "btnPrintReceipt", Size = new Size(160, 40), Text = LocalizationManager.GetString("POS_Receipt"), Image = ThemeConfig.GetNuricon("print"), TextImageRelation = TextImageRelation.ImageBeforeText, ImageAlign = ContentAlignment.MiddleLeft, Margin = new Padding(5, 0, 0, 0), Cursor = Cursors.Hand };
              btnPrintReceipt.Click += BtnPrintReceipt_Click; ThemeConfig.ApplyPaletteButton(btnPrintReceipt, ThemeConfig.SecondaryColor);

              btnQuotation = new ModernButton { Name = "btnQuotation", Size = new Size(180, 40), Text = LocalizationManager.GetString("POS_Quotation"), Image = ThemeConfig.GetNuricon("quotations"), TextImageRelation = TextImageRelation.ImageBeforeText, ImageAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0), Cursor = Cursors.Hand };
              btnQuotation.Click += BtnSaveQuotation_Click; ThemeConfig.ApplyPaletteButton(btnQuotation, ThemeConfig.PrimaryColor);

              pnlButtons.Controls.Add(btnCheckout);
              pnlButtons.Controls.Add(btnPayLater);
              pnlButtons.Controls.Add(btnDraft);
              pnlButtons.Controls.Add(btnPrintReceipt);
              pnlButtons.Controls.Add(btnQuotation);
              pnlTotals.Controls.Add(pnlButtons);
             tlpBottomArea.Controls.Add(pnlTotals, 1, 0); tlpRoot.Controls.Add(tlpBottomArea, 0, 3);
            this.ResumeLayout(false);
        }

        private Panel CreateCardPanel()
        {
            Panel p = new Panel(); 
            p.BackColor = ThemeConfig.SurfaceColor; 
            p.BorderStyle = BorderStyle.None;
            p.Padding = new Padding(15);
            
            p.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                
                // 1. Clear corners with PARENT background color
                Color parentColor = ThemeConfig.GetParentColor(p);
                using (var brush = new SolidBrush(parentColor))
                {
                    e.Graphics.FillRectangle(brush, -1, -1, p.Width + 2, p.Height + 2);
                }

                // 2. Draw Rounded Surface (White)
                Rectangle r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                using (var path = GetRoundedRect(r, 15))
                {
                    using (var brush = new SolidBrush(ThemeConfig.SurfaceColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                    // Border
                    using (var pen = new Pen(ThemeConfig.BorderColor, 1f))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            };
            return p;
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath(); int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90); path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90); path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90); path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure(); return path;
        }

        private void InitializeCart()
        {
            try
            {
                cartTable = new DataTable();
                cartTable.Columns.Add("PartID", typeof(int)); cartTable.Columns.Add("PartName", typeof(string)); cartTable.Columns.Add("Quantity", typeof(int)); cartTable.Columns.Add("PrivatePrice", typeof(decimal)); cartTable.Columns.Add("SellingPrice", typeof(decimal)); cartTable.Columns.Add("Total", typeof(decimal), "Quantity * SellingPrice");
                
                dgvCart.Columns.Clear();
                dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartID", DataPropertyName = "PartID", Visible = false });
                dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartName", DataPropertyName = "PartName", HeaderText = LocalizationManager.GetString("POS_GridProduct"), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
                dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", DataPropertyName = "Quantity", HeaderText = LocalizationManager.GetString("POS_GridQty"), Width = 80, ReadOnly = false });
                DataGridViewTextBoxColumn colPrice = new DataGridViewTextBoxColumn { Name = "SellingPrice", DataPropertyName = "SellingPrice", HeaderText = LocalizationManager.GetString("POS_GridUnitCost"), Width = 100, ReadOnly = true };
                colPrice.DefaultCellStyle.Format = "c2"; colPrice.DefaultCellStyle.FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("en-US"); dgvCart.Columns.Add(colPrice);
                DataGridViewTextBoxColumn colTotal = new DataGridViewTextBoxColumn { Name = "Total", DataPropertyName = "Total", HeaderText = LocalizationManager.GetString("POS_GridTotal"), Width = 100, ReadOnly = true };
                colTotal.DefaultCellStyle.Format = "c2"; colTotal.DefaultCellStyle.FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("en-US"); dgvCart.Columns.Add(colTotal);
                DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn { Name = "colDelete", HeaderText = "", Text = "", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat, Width = 60 }; dgvCart.Columns.Add(btnDelete);
                dgvCart.DataSource = cartTable; LoadCustomers(); ThemeConfig.ApplyGridTheme(dgvCart);
            }
            catch (Exception ex) { MessageHelper.ShowError("Error initializing POS: " + ex.Message); }
        }

        private void LoadCustomers()
        {
            try {
                DataTable dt = DatabaseHelper.ExecuteDataTable("SELECT customer_id, full_name FROM customers ORDER BY full_name");
                DataRow row = dt.NewRow(); row["customer_id"] = -1; row["full_name"] = LocalizationManager.GetString("POS_WalkIn"); dt.Rows.InsertAt(row, 0);
                cmbCustomers.ValueMember = "customer_id";
                cmbCustomers.DisplayMember = "full_name";
                cmbCustomers.DataSource = dt;
            } catch { }
        }

        private void ApplyPermissions() { }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("CartEmpty") ?? (LocalizationManager.GetString("CartEmpty")));
                return;
            }
            
            decimal total = 0; foreach(DataRow row in cartTable.Rows) total += (decimal)row["Total"];
            
            if(ModernMessageBox.Show(string.Format(LocalizationManager.GetString("ConfirmSale"), $"{total:N2}"), "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;

            try {
                List<OrderItem> items = new List<OrderItem>();
                foreach(DataRow row in cartTable.Rows) items.Add(new OrderItem { PartId = (int)row["PartID"], Quantity = (int)row["Quantity"], UnitPrice = (decimal)row["SellingPrice"] });
                int customerId = Convert.ToInt32(cmbCustomers.SelectedValue);
                int orderId = new OrderService().PlaceOrder(customerId, items, total, true); // true = Paid
                DatabaseHelper.LogTransaction("SALE", "Order #" + orderId, "Paid Total: $" + total);
                // Notify all connected web POS tablets in real-time
                InventoryBroadcaster.BroadcastStockChange("desktop-sale");
                MessageHelper.ShowSuccess("Order Sent! Order #" + orderId); cartTable.Rows.Clear(); UpdateTotal(); RefreshStats(); 
            } catch(Exception ex) { MessageHelper.ShowError("Error: " + ex.Message); }
        }

        private void BtnPayLater_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("CartEmpty") ?? (LocalizationManager.GetString("CartEmpty")));
                return;
            }
            
            int customerId = Convert.ToInt32(cmbCustomers.SelectedValue);
            if (customerId == -1)
            {
                string msg = LocalizationManager.GetString("Msg_SelectCustomer");
                MessageHelper.ShowWarning(msg);
                return;
            }
            
            decimal total = 0; foreach(DataRow row in cartTable.Rows) total += (decimal)row["Total"];
            
            // Credit Limit Validation
            try {
                DataTable dt = DatabaseHelper.ExecuteDataTable($"SELECT current_balance, credit_limit FROM customers WHERE customer_id = {customerId}");
                if (dt.Rows.Count > 0) {
                    decimal currentBalance = (decimal)dt.Rows[0]["current_balance"];
                    decimal creditLimit = (decimal)dt.Rows[0]["credit_limit"];
                    if (currentBalance + total > creditLimit) {
                        string title = LocalizationManager.GetString("POS_CreditLimitExceeded");
                        if (string.IsNullOrEmpty(title)) title = "Credit Limit Exceeded";
                        
                        string msgFormat = LocalizationManager.GetString("POS_CreditLimitExceededMsg");
                        string msg;
                        if (string.IsNullOrEmpty(msgFormat)) {
                            msg = $"Customer has exceeded their credit limit.\nCurrent Balance: {currentBalance:C2}\nNew Amount: {total:C2}\nLimit: {creditLimit:C2}";
                        } else {
                            msg = string.Format(msgFormat, currentBalance, total, creditLimit);
                        }
                        
                        MessageHelper.ShowWarning(msg);
                        return;
                    }
                }
            } catch { }

            string confirmMsg = LocalizationManager.GetString("POS_ConfirmAddBill");
            if (string.IsNullOrEmpty(confirmMsg)) confirmMsg = "Confirm adding ${0} to customer balance?";
            if(ModernMessageBox.Show(string.Format(confirmMsg, $"{total:N2}"), "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;

            try {
                List<OrderItem> items = new List<OrderItem>();
                foreach(DataRow row in cartTable.Rows) items.Add(new OrderItem { PartId = (int)row["PartID"], Quantity = (int)row["Quantity"], UnitPrice = (decimal)row["SellingPrice"] });
                int orderId = new OrderService().PlaceOrder(customerId, items, total, false, "Completed", dtDueDate.Value); // false = Unpaid
                DatabaseHelper.LogTransaction("SALE_DEBT", "Order #" + orderId, "Unpaid Total: $" + total);
                // Notify all connected web POS tablets in real-time
                InventoryBroadcaster.BroadcastStockChange("desktop-pay-later");
                MessageHelper.ShowSuccess("Order Billed! Order #" + orderId); cartTable.Rows.Clear(); UpdateTotal(); RefreshStats(); 
            } catch(Exception ex) { MessageHelper.ShowError("Error: " + ex.Message); }
        }

        private void BtnPrintReceipt_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0) { MessageHelper.ShowWarning("Cart is empty!"); return; }
            System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
            try { pd.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("Receipt", 315, 600); } catch { }
            pd.PrintPage += PrintReceiptPage;
            var preview = new PrintPreviewDialog { 
                Document = pd, 
                Text = LocalizationManager.GetString("POS_PrintReceipt") 
            };
            ThemeConfig.ApplyPrintPreviewTheme(preview);
            preview.ShowDialog();
        }

        private void PrintReceiptPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics; Font fH = new Font("Segoe UI", 12, FontStyle.Bold), fS = new Font("Segoe UI", 9), fI = new Font("Consolas", 9);
            int y = 20, m = 10, w = Math.Min(e.PageBounds.Width, 300) - (m * 2);
            StringFormat cA = new StringFormat { Alignment = StringAlignment.Center }, rA = new StringFormat { Alignment = StringAlignment.Far };
            g.DrawString(ThemeConfig.CompanyName.ToUpper(), fH, Brushes.Black, new Rectangle(m, y, w, 25), cA); y += 30;
            g.DrawString("SALES RECEIPT", fS, Brushes.Black, new Rectangle(m, y, w, 20), cA); y += 20;
            g.DrawString(DateTime.Now.ToString("g"), fS, Brushes.Black, new Rectangle(m, y, w, 20), cA); y += 25;
            g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;
            g.DrawString("QTY", fI, Brushes.Black, m, y); g.DrawString("ITEM", fI, Brushes.Black, m + 40, y); g.DrawString("PRICE", fI, Brushes.Black, new Rectangle(m, y, w, 20), rA);
            y += 20; g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;
            foreach (DataRow r in cartTable.Rows) {
                string n = r["PartName"].ToString(); if (n.Length > 18) n = n.Substring(0, 15) + "...";
                g.DrawString(r["Quantity"].ToString(), fI, Brushes.Black, m, y); g.DrawString(n, fI, Brushes.Black, m + 40, y);
                g.DrawString(CurrencyService.Format((decimal)r["Total"]), fI, Brushes.Black, new Rectangle(m, y, w, 20), rA); y += 20;
            }
            y += 10; g.DrawLine(Pens.Black, m, y, m + w, y); y += 10;
            g.DrawString("GRAND TOTAL:", fH, Brushes.Black, m, y); g.DrawString(lblTotalVal.Text, fH, Brushes.Black, new Rectangle(m, y, w, 25), rA);
            y += 40; g.DrawString("Thank you!", fS, Brushes.Black, new Rectangle(m, y, w, 20), cA); e.HasMorePages = false;
        }
        private void BtnAddItem_Click(object sender, EventArgs e) { ProductSelectorForm s = new ProductSelectorForm(); if (s.ShowDialog() == DialogResult.OK) AddToCart(s.SelectedPartId, s.SelectedPartName, s.SelectedPrice, s.SelectedStock); }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (this.ParentForm != null)
            {
                this.ParentForm.KeyPreview = true;
                this.ParentForm.KeyPress += POSForm_KeyPress;
            }
        }

        private void POSForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!this.Visible) return;

            TimeSpan elapsed = DateTime.Now - _lastScanTime;
            if (elapsed.TotalMilliseconds > 100) 
            {
                _scanBuffer = "";
            }
            _lastScanTime = DateTime.Now;

            if (e.KeyChar != (char)Keys.Enter)
            {
                _scanBuffer += e.KeyChar;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && this.Visible)
            {
                TimeSpan elapsed = DateTime.Now - _lastScanTime;
                if (elapsed.TotalMilliseconds <= 100 && !string.IsNullOrEmpty(_scanBuffer))
                {
                    string barcode = _scanBuffer.Trim();
                    _scanBuffer = "";

                    DataTable dt = DatabaseHelper.ExecuteDataTable($"SELECT id,part_name,selling_price,quantity_in_stock FROM parts WHERE (barcode='{barcode}' OR part_number='{barcode}') AND date_deleted IS NULL");
                    if(dt.Rows.Count > 0) 
                    { 
                        DataRow r = dt.Rows[0]; 
                        AddToCart((int)r["id"], r["part_name"].ToString(), (decimal)r["selling_price"], (int)r["quantity_in_stock"]); 
                    }
                    else 
                    { 
                        string notFoundMsg = LocalizationManager.GetString("POS_ProductNotFound") ?? $"Item not found for barcode: {barcode}";
                        MessageHelper.ShowInfo(notFoundMsg); // Use Info (non-blocking notification) instead of Warning (modal)
                    }

                    return true; // Suppress Enter key so it doesn't click focused buttons
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void AddToCart(int id, string name, decimal price, int stock)
        {
            if (stock <= 0) { MessageHelper.ShowWarning(LocalizationManager.GetString("Error_OutOfStock")); return; }
            foreach(DataRow r in cartTable.Rows) if((int)r["PartID"] == id) { int q = (int)r["Quantity"]; if(q + 1 > stock) { MessageHelper.ShowWarning(LocalizationManager.GetString("POS_NotEnoughStock")); return; } r["Quantity"] = q + 1; UpdateTotal(); return; }
            cartTable.Rows.Add(id, name, 1, 0, price); UpdateTotal();
        }

        private void DgvCart_CellContentClick(object sender, DataGridViewCellEventArgs e) { if(e.RowIndex >= 0 && dgvCart.Columns[e.ColumnIndex].Name == "colDelete") { dgvCart.Rows.RemoveAt(e.RowIndex); UpdateTotal(); } }

        private bool _isUpdatingCart = false;
        private void DgvCart_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
             if (_isUpdatingCart) return;
             if(e.RowIndex >= 0 && dgvCart.Columns[e.ColumnIndex].Name == "Quantity") {
                 try {
                     _isUpdatingCart = true; int n = Convert.ToInt32(dgvCart.Rows[e.RowIndex].Cells["Quantity"].Value); int id = (int)dgvCart.Rows[e.RowIndex].Cells["PartID"].Value;
                     int s = DatabaseHelper.ExecuteScalar<int>($"SELECT quantity_in_stock FROM parts WHERE id={id}");
                     if (n > s) { this.BeginInvoke(new Action(()=> { MessageHelper.ShowWarning(string.Format(LocalizationManager.GetString("POS_NotEnoughStockQty"), s)); })); dgvCart.Rows[e.RowIndex].Cells["Quantity"].Value = Math.Max(1, s); }
                     else if (n < 1) dgvCart.Rows[e.RowIndex].Cells["Quantity"].Value = 1;
                     dgvCart.EndEdit(); if (dgvCart.BindingContext != null && dgvCart.BindingContext[cartTable] != null) dgvCart.BindingContext[cartTable].EndCurrentEdit();
                     UpdateTotal(); 
                 } catch { dgvCart.Rows[e.RowIndex].Cells["Quantity"].Value = 1; UpdateTotal(); } finally { _isUpdatingCart = false; }
             }
        }

        private void DgvCart_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvCart.Columns[e.ColumnIndex].Name == "colDelete") {
                e.Handled = true; e.PaintBackground(e.CellBounds, true);
                Rectangle r = new Rectangle(e.CellBounds.X + (e.CellBounds.Width-32)/2, e.CellBounds.Y + (e.CellBounds.Height-32)/2, 32, 32);
                using (var path = GetRoundedRect(r, 8)) using (var brush = new SolidBrush(ThemeConfig.SurfaceColor)) e.Graphics.FillPath(brush, path);
                Image img = ThemeConfig.GetNuricon("delete"); if (img != null) e.Graphics.DrawImage(img, new Rectangle(r.X + 6, r.Y + 6, 20, 20));
            }
        }

        private void UpdateTotal()
        {
            decimal s = 0; foreach(DataRow r in cartTable.Rows) if (r.RowState != DataRowState.Deleted) s += (decimal)r["Total"];
            decimal t = chkApplyVAT.Checked ? (s * 0.11m) : 0; 
            decimal ship = chkApplyShipping.Checked ? numShipping.Value : 0;
            decimal g = s + t + ship;
            
            lblSubtotalVal.Text = CurrencyService.Format(s); 
            lblTaxVal.Text = CurrencyService.Format(t);
            lblTaxVal.ForeColor = chkApplyVAT.Checked ? ThemeConfig.TextColorDark : Color.Gray;
            
            lblShippingVal.Text = CurrencyService.Format(ship);
            lblShippingVal.Visible = !chkApplyShipping.Checked; // Hide static label if input is visible
            
            lblTotalVal.Text = CurrencyService.Format(g);
        }

        private void BtnSaveDraft_Click(object sender, EventArgs e)
        {
             if (cartTable.Rows.Count == 0)
             {
                 MessageHelper.ShowWarning(LocalizationManager.GetString("CartEmpty") ?? (LocalizationManager.GetString("CartEmpty")));
                 return;
             }
             try {
                 List<OrderItem> items = new List<OrderItem>(); decimal total = 0;
                 foreach(DataRow r in cartTable.Rows) { total += (decimal)r["Total"]; items.Add(new OrderItem { PartId = (int)r["PartID"], Quantity = (int)r["Quantity"], UnitPrice = (decimal)r["SellingPrice"] }); }
                 new OrderService().PlaceOrder(Convert.ToInt32(cmbCustomers.SelectedValue), items, total, false, "Draft"); 
                 MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_DraftSaved")); cartTable.Rows.Clear(); UpdateTotal();
             } catch(Exception ex) { MessageHelper.ShowError("Failed: " + ex.Message); }
        }

        private void BtnSaveQuotation_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("CartEmpty") ?? (LocalizationManager.GetString("CartEmpty")));
                return;
            }
            try {
                List<OrderItem> items = new List<OrderItem>(); decimal total = 0;
                foreach(DataRow r in cartTable.Rows) { total += (decimal)r["Total"]; items.Add(new OrderItem { PartId = (int)r["PartID"], Quantity = (int)r["Quantity"], UnitPrice = (decimal)r["SellingPrice"] }); }
                new OrderService().PlaceOrder(Convert.ToInt32(cmbCustomers.SelectedValue), items, total, false, "Quotation"); 
                MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_Saved")); cartTable.Rows.Clear(); UpdateTotal(); GlobalEvents.RaiseOrdersUpdated();
            } catch(Exception ex) { MessageHelper.ShowError("Failed: " + ex.Message); }
        }

        private void BtnLoadDraft_Click(object sender, EventArgs e)
        {
             DataTable ds = new OrderService().GetDrafts(); if(ds.Rows.Count == 0) { MessageHelper.ShowInfo(LocalizationManager.GetString("POS_NoDrafts")); return; }
             BaseModalForm f = new BaseModalForm { TitleText = LocalizationManager.GetString("Title_SelectDraft"), Size = new Size(600, 450) };
             
             TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(10) };
             tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
             tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

             DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, DataSource = ds, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
             ThemeConfig.ApplyGridTheme(dgv); dgv.ColumnHeadersVisible = true;
             
             Button bl = new ModernButton { Text = "Load", Size = new Size(120, 45), Anchor = AnchorStyles.Right }; 
             ThemeConfig.ApplyPrimaryButton(bl);
             
             bl.Click += (s2, e2) => { if(dgv.SelectedRows.Count > 0) {
                 int oid = (int)dgv.SelectedRows[0].Cells["order_id"].Value;
                 object cid = dgv.SelectedRows[0].Cells["customer_id"].Value; LoadDraftIntoCart(oid, (cid == DBNull.Value) ? -1 : (int)cid); f.DialogResult = DialogResult.OK; f.Close();
             }};
             
             tlp.Controls.Add(dgv, 0, 0);
             tlp.Controls.Add(bl, 0, 1);
             
             f.ContentPanel.Controls.Add(tlp);
             f.ShowDialog();
        }

        private void LoadDraftIntoCart(int draftOrderId, int customerId)
        {
             try {
                 OrderService svc = new OrderService(); List<OrderItem> items = svc.GetOrderItems(draftOrderId);
                 if (items.Count == 0) { MessageHelper.ShowWarning("Empty draft."); return; }
                 cartTable.Rows.Clear();
                 foreach(var i in items) { int s = DatabaseHelper.ExecuteScalar<int>($"SELECT quantity_in_stock FROM parts WHERE id={i.PartId}"); cartTable.Rows.Add(i.PartId, i.PartName, Math.Min(i.Quantity, s), 0, i.UnitPrice); }
                 UpdateTotal(); cmbCustomers.SelectedValue = customerId; svc.DeleteOrder(draftOrderId); MessageHelper.ShowSuccess("Loaded!");
             } catch(Exception ex) { MessageHelper.ShowError("Error: " + ex.Message); }
        }

        private void BtnReturnItems_Click(object sender, EventArgs e)
        {
             bool ar = LocalizationManager.IsArabic;
             
             OrderIdPromptForm promptForm = new OrderIdPromptForm();
             if (promptForm.ShowDialog() != DialogResult.OK) return;

             int orderId = promptForm.OrderId;
             
             try {
                 var check = DatabaseHelper.ExecuteScalar<int>($"SELECT COUNT(*) FROM orders WHERE order_id = {orderId}");
                 if (check > 0)
                 {
                     var status = DatabaseHelper.ExecuteScalar<object>($"SELECT status FROM orders WHERE order_id = {orderId}")?.ToString();
                     if (status == "Quotation" || status == "Draft") {
                         MessageHelper.ShowWarning(ar ? "لا يمكن إرجاع طلبات الاقتباس أو المسودات." : "Cannot return Quotation or Draft orders.");
                         return;
                     }
                     ReturnEntryForm form = new ReturnEntryForm(orderId);
                     form.ShowDialog();
                 }
                 else MessageHelper.ShowWarning(ar ? "رقم الطلب غير موجود." : "Order ID not found.");
             } catch (Exception ex) { MessageHelper.ShowError(ex.Message); }
        }
    }
}
