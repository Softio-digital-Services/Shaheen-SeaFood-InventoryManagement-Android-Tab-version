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
        private DataGridView dgvQuotes;
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
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Dock     = DockStyle.Fill;
            this.BackColor = ThemeConfig.BackgroundColor;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock       = DockStyle.Fill;
            tlp.Padding    = new Padding(20);
            tlp.ColumnCount = 1;
            tlp.RowCount   = 2;
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlp.BackColor  = ThemeConfig.BackgroundColor;
            this.Controls.Add(tlp);

            // ─── Header ───────────────────────────────────────────────────
            Panel pnlHeader = new Panel { Dock = DockStyle.Fill, BackColor = ThemeConfig.BackgroundColor };
            Label lblTitle  = ThemeConfig.CreateStandardHeader(
                LocalizationManager.IsArabic ? "عروض الأسعار للعملاء" : "Customer Quotations");
            pnlHeader.Controls.Add(lblTitle);

            // ─── Currency selector (aligned right) ────────────────────────
            ComboBox cboCurrency = new ComboBox();
            ThemeConfig.ApplyComboBoxStyle(cboCurrency);
            Panel currPanel = ThemeConfig.WrapInStyledInput(cboCurrency, 36); currPanel.Width = 110;
            currPanel.Location = new Point(pnlHeader.Width - 130, 45);
            currPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            
            foreach (var c in CurrencyService.SupportedCurrencies)
                cboCurrency.Items.Add(c);

            // Pre-select active currency
            foreach (var item in cboCurrency.Items)
                if (item is CurrencyInfo ci && ci.Code == CurrencyService.ActiveCurrency)
                { cboCurrency.SelectedItem = item; break; }
            
            cboCurrency.SelectedIndexChanged += (s, e) =>
            {
                if (cboCurrency.SelectedItem is CurrencyInfo sel)
                    CurrencyService.ActiveCurrency = sel.Code;
            };

            // Keep currency panel right-aligned on Resize
            pnlHeader.Resize += (s, e) =>
            {
                currPanel.Location = new Point(pnlHeader.Width - 130, 45);
            };
            
            pnlHeader.Controls.Add(currPanel);
            tlp.Controls.Add(pnlHeader, 0, 0);

            // ─── Grid ─────────────────────────────────────────────────────
            // Wrap in a card-like Panel
            Panel pnlCard = new Panel();
            pnlCard.Dock        = DockStyle.Fill;
            pnlCard.BackColor   = Color.White;
            pnlCard.Padding     = new Padding(0);
            pnlCard.Paint      += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(ThemeConfig.BorderColor, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
            };

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

            pnlCard.Controls.Add(dgvQuotes);
            tlp.Controls.Add(pnlCard, 0, 1);

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

        private void DrawActionButton(Graphics g, Rectangle rect, string text, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = GetRoundedRect(rect, 6))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(20, color)))
            using (Pen border = new Pen(color, 1.2f))
            {
                g.FillPath(bg, path);
                g.DrawPath(border, path);
            }
            TextRenderer.DrawText(g, text, ThemeConfig.SmallBoldFont, rect, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
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
        public void LoadQuotations()
        {
            DataTable dt = _orderService.GetQuotations();
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
