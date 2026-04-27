using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Services;

namespace GenericInventorySystem.Forms
{
    public class QuotationsForm : UserControl
    {
        private Label lblQuotationsTitle;
        private DataGridView dgvQuotes;
        private ModernTextBox txtSearch;
        private OrderService _orderService;
        private int _hoveredRow = -1;

        public QuotationsForm()
        {
            _orderService = new OrderService();
            InitializeComponent();
            LoadQuotations();

            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();

            // Currency Sync
            GenericInventorySystem.Services.CurrencyService.CurrencyChanged += (s, e) => { dgvQuotes.Invalidate(); };
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);

            if (dgvQuotes != null && dgvQuotes.Columns.Count > 0)
            {
                if (dgvQuotes.Columns.Contains("order_id"))    dgvQuotes.Columns["order_id"].HeaderText    = LocalizationManager.IsArabic ? "رقم" : "ID";
                if (dgvQuotes.Columns.Contains("order_date"))  dgvQuotes.Columns["order_date"].HeaderText  = LocalizationManager.IsArabic ? "التاريخ" : "Date";
                if (dgvQuotes.Columns.Contains("CustomerName")) dgvQuotes.Columns["CustomerName"].HeaderText = LocalizationManager.IsArabic ? "العميل" : "Customer";
                if (dgvQuotes.Columns.Contains("total_amount")) dgvQuotes.Columns["total_amount"].HeaderText = LocalizationManager.IsArabic ? "الإجمالي" : "Total";
                if (dgvQuotes.Columns.Contains("colActions"))   dgvQuotes.Columns["colActions"].HeaderText  = LocalizationManager.IsArabic ? "الإجراءات" : "Actions";
            }
            if (lblQuotationsTitle != null) lblQuotationsTitle.Text = LocalizationManager.IsArabic ? "عروض الأسعار للعملاء" : "Customer Quotations";
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Dock     = DockStyle.Fill;
            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock       = DockStyle.Fill;
            tlp.Padding    = new Padding(20);
            tlp.ColumnCount = 1;
            tlp.RowCount   = 3;
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // 0. Title
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // 1. Actions (Search/Currency)
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // 2. Content
            tlp.BackColor  = ThemeConfig.BackgroundColor;
            this.Controls.Add(tlp);

            // 0. Title
            lblQuotationsTitle = ThemeConfig.CreateStandardHeader(
                LocalizationManager.IsArabic ? "عروض الأسعار للعملاء" : "Customer Quotations");
            lblQuotationsTitle.Name = "lblQuotationsTitle";
            lblQuotationsTitle.Margin = new Padding(0);
            tlp.Controls.Add(lblQuotationsTitle, 0, 0);

            // 1. Actions Row
            Panel pnlActions = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            
            txtSearch = new ModernTextBox();
            txtSearch.IsSearch = true;
            txtSearch.ShowLabel = false;
            txtSearch.PlaceholderText = "Search quotations...";
            txtSearch.Size = new Size(320, 40);
            txtSearch.Location = new Point(0, 5);
            txtSearch.TextChanged += (s, e) => LoadQuotations(txtSearch.Text);
            pnlActions.Controls.Add(txtSearch);

            // Currency selector (aligned right)
            ModernComboBox cboCurrency = new ModernComboBox();
            cboCurrency.ShowLabel = false;
            cboCurrency.Size = new Size(120, 40);
            cboCurrency.Dock = DockStyle.Right;
            
            foreach (var c in CurrencyService.SupportedCurrencies)
                cboCurrency.Items.Add(c);

            // Pre-select active currency
            foreach (var item in cboCurrency.Items)
                if (item is CurrencyInfo ci && ci.Code == CurrencyService.ActiveCurrency)
                { cboCurrency.SelectedItem = item; break; }
            
            cboCurrency.InnerComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (cboCurrency.SelectedItem is CurrencyInfo sel)
                    CurrencyService.ActiveCurrency = sel.Code;
            };
            pnlActions.Controls.Add(cboCurrency);
            tlp.Controls.Add(pnlActions, 0, 1);


            // ─── Grid ─────────────────────────────────────────────────────
            dgvQuotes = new DataGridView();
            dgvQuotes.Dock              = DockStyle.Fill;
            dgvQuotes.AllowUserToAddRows = false;
            dgvQuotes.ReadOnly          = true;
            dgvQuotes.RowHeadersVisible = false;
            dgvQuotes.SelectionMode     = DataGridViewSelectionMode.FullRowSelect;
            dgvQuotes.BackgroundColor   = Color.White;
            dgvQuotes.BorderStyle       = BorderStyle.None;
            dgvQuotes.AutoGenerateColumns = false;
            dgvQuotes.MultiSelect       = false;
            dgvQuotes.DataError += (s, e) => { e.ThrowException = false; };

            ThemeConfig.ApplyGridTheme(dgvQuotes);

            // ─── Columns (manual so we fully control them) ────────────────
            dgvQuotes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "order_id", HeaderText = "ID", DataPropertyName = "order_id",
                Width = 60, ReadOnly = true
            });
            dgvQuotes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "order_date", HeaderText = "Date", DataPropertyName = "order_date",
                Width = 160, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "g" }
            });
            dgvQuotes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomerName", HeaderText = "Customer", DataPropertyName = "CustomerName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true
            });

            var colTotal = new DataGridViewTextBoxColumn
            {
                Name = "total_amount", HeaderText = "Total", DataPropertyName = "total_amount",
                Width = 110, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font   = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = ThemeConfig.PrimaryColor,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            };
            dgvQuotes.Columns.Add(colTotal);
            dgvQuotes.CellFormatting += DgvQuotes_CellFormatting;

            // Hidden columns for IDs that come from DataSource
            dgvQuotes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "customer_id", DataPropertyName = "customer_id", Visible = false
            });

            // Actions column – painted manually
            dgvQuotes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colActions", HeaderText = "Actions", Width = 210,
                ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable
            });

            // ─── Events ───────────────────────────────────────────────────
            dgvQuotes.CellPainting     += DgvQuotes_CellPainting;
            dgvQuotes.CellClick        += DgvQuotes_CellClick;
            dgvQuotes.CellMouseMove    += (s, e) =>
            {
                if (e.RowIndex != _hoveredRow && e.RowIndex >= 0 &&
                    dgvQuotes.Columns[e.ColumnIndex].Name == "colActions")
                {
                    _hoveredRow = e.RowIndex;
                    dgvQuotes.InvalidateRow(e.RowIndex);
                    dgvQuotes.Cursor = Cursors.Hand;
                }
                else if (dgvQuotes.Columns[e.ColumnIndex].Name != "colActions")
                    dgvQuotes.Cursor = Cursors.Default;
            };
            dgvQuotes.CellMouseLeave   += (s, e) =>
            {
                _hoveredRow = -1;
                dgvQuotes.Cursor = Cursors.Default;
            };

            // Card Wrapper
            Panel pnlCard = ThemeConfig.CreateCardPanel(dgvQuotes);
            tlp.Controls.Add(pnlCard, 0, 2);

            this.ResumeLayout(false);
        }

        private void DgvQuotes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvQuotes.Columns[e.ColumnIndex].Name == "total_amount" && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal usdTotal))
                {
                    e.Value = GenericInventorySystem.Services.CurrencyService.Format(usdTotal);
                    e.FormattingApplied = true;
                }
            }
        }

        private void DgvQuotes_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // 3. Actions - Nuricon Icons
            if (dgvQuotes.Columns[e.ColumnIndex].Name == "colActions")
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                int btnW = 32, btnH = 32, gap = 15;
                int totalW = btnW * 3 + gap * 2;
                int startX = e.CellBounds.X + (e.CellBounds.Width - totalW) / 2;
                int startY = e.CellBounds.Y + (e.CellBounds.Height - btnH) / 2;

                Image imgView = ThemeConfig.GetNuricon("view");
                Image imgOk = ThemeConfig.GetNuricon("check");
                Image imgDel = ThemeConfig.GetNuricon("delete");

                if (imgView != null) e.Graphics.DrawImage(imgView, new Rectangle(startX, startY, btnW, btnH));
                if (imgOk != null) e.Graphics.DrawImage(imgOk, new Rectangle(startX + btnW + gap, startY, btnW, btnH));
                if (imgDel != null) e.Graphics.DrawImage(imgDel, new Rectangle(startX + (btnW + gap) * 2, startY, btnW, btnH));
            }
        }


        // ─── Click handling for the three painted buttons ─────────────────
        private void DgvQuotes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvQuotes.Columns[e.ColumnIndex].Name != "colActions") return;

            int orderId = Convert.ToInt32(dgvQuotes.Rows[e.RowIndex].Cells["order_id"].Value);

            // Figure out which sub-button was clicked using the mouse position
            Point cur      = dgvQuotes.PointToClient(Cursor.Position);
            Rectangle cell = dgvQuotes.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);

            int btnW = 62, gap = 5;
            int totalW = btnW * 3 + gap * 2;
            int startX = cell.X + (cell.Width - totalW) / 2;

            int relX = cur.X;

            if (relX >= startX && relX < startX + btnW)
            {
                // View / Print
                QuotationPreviewForm doc = new QuotationPreviewForm(orderId);
                doc.ShowDialog(this);
            }
            else if (relX >= startX + btnW + gap && relX < startX + btnW * 2 + gap)
            {
                // Checkout / Convert
                if (MessageHelper.ConfirmAction("Convert this quotation to a finalized Sales Order? This will reduce stock."))
                {
                    try
                    {
                        if (_orderService.ConvertToOrder(orderId))
                        {
                            MessageHelper.ShowSuccess("Quotation successfully converted to Order!");
                            LoadQuotations();
                        }
                    }
                    catch (Exception ex) { MessageHelper.ShowError(ex.Message); }
                }
            }
            else if (relX >= startX + (btnW + gap) * 2 && relX < startX + (btnW + gap) * 2 + btnW)
            {
                // Delete
                if (MessageHelper.ConfirmAction("Are you sure you want to delete this quotation?"))
                {
                    _orderService.DeleteOrder(orderId);
                    LoadQuotations();
                }
            }
        }

        // ─── Data loading ─────────────────────────────────────────────────
        public void LoadQuotations(string search = "")
        {
            DataTable dt = _orderService.GetQuotations();
            if (!string.IsNullOrEmpty(search))
            {
                // Simple client-side filter for now
                DataView dv = dt.DefaultView;
                dv.RowFilter = string.Format("CustomerName LIKE '%{0}%' OR order_id = {1}", search, int.TryParse(search, out int id) ? id : -1);
                dt = dv.ToTable();
            }
            dgvQuotes.DataSource = dt;

            // Ensure hidden columns stay hidden even after DataSource rebind
            if (dgvQuotes.Columns.Contains("customer_id"))
                dgvQuotes.Columns["customer_id"].Visible = false;
            if (dgvQuotes.Columns.Contains("status"))
                dgvQuotes.Columns["status"].Visible = false;

            // Keep actions column last
            if (dgvQuotes.Columns.Contains("colActions"))
                dgvQuotes.Columns["colActions"].DisplayIndex = dgvQuotes.Columns.Count - 1;

            ApplyLocalization();
        }
    }
}
