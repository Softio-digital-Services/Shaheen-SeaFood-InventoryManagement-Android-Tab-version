using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Services;
using System.Drawing.Printing;

namespace GenericInventorySystem.Forms
{
    public class QuotationPreviewForm : BaseModalForm
    {
        private int _orderId;
        private OrderService _orderService;
        private Panel pnlContent;

        public QuotationPreviewForm(int orderId)
        {
            _orderId = orderId;
            _orderService = new OrderService();
            
            this.TitleText = (LocalizationManager.IsArabic ? "\u0645\u0639\u0627\u064a\u0646\u0629 \u0639\u0631\u0636 \u0627\u0644\u0633\u0639\u0631" : "Quotation Preview") + " - #" + orderId;
            this.Size = new Size(950, 950); // Increased width to ensure A4 fits

            this.ContentPanel.Padding = new Padding(20, 20, 20, 20); // Add safety margin

            InitializeUI();
            LoadData();
        }

        private void InitializeUI()
        {
            // Adaptive sizing handled by BaseModalForm.OnLoad
            
            SetFooterButtons(
                LocalizationManager.GetString("Tran_Print") ?? "Print",
                LocalizationManager.GetString("Tran_Export") ?? "Export",
                (s, e) => HandlePrint(),
                (s, e) => HandleExport(),
                LocalizationManager.GetString("Popup_Cancel") ?? "Close",
                (s, e) => this.Close()
            );

            // The actual document (A4-ish proportions)
            pnlContent = new Panel { 
                Width = 800, 
                Height = 1100, 

                Margin = new Padding(0, 0, 0, 40),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top
            };
            
            // Center in ContentPanel
            pnlContent.Left = (this.ContentPanel.Width - pnlContent.Width) / 2;
            this.ContentPanel.Controls.Add(pnlContent);
            
            this.ContentPanel.Resize += (s, e) => {
                pnlContent.Left = Math.Max(0, (this.ContentPanel.Width - pnlContent.Width) / 2);
            };
        }

        private void LoadData()
        {
            try
            {
                // Fetch Data
                var items = _orderService.GetOrderItems(_orderId);
                decimal total = 0;
                foreach(var item in items) total += (item.Quantity * item.UnitPrice);

                // Build UI on pnlContent
                RenderDocument(items, total);
            }
            catch(Exception ex)
            {
                MessageHelper.ShowError("Could not load quotation details: " + ex.Message);
            }
        }

        private void RenderDocument(List<OrderItem> items, decimal total)
        {
            pnlContent.Controls.Clear();
            pnlContent.BorderStyle = BorderStyle.None;
            
            int y = 40;

            // Header - Logo & Company Info
            PictureBox pbLogo = new PictureBox { Size = new Size(70, 70), Location = new Point(40, y), SizeMode = PictureBoxSizeMode.Zoom };
            pbLogo.Image = ThemeConfig.GetNuricon("pos");
            try 
            { 
                string logoPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "inventory_logo.png");
                if (System.IO.File.Exists(logoPath)) pbLogo.Image = Image.FromFile(logoPath);
            } catch { }
            pnlContent.Controls.Add(pbLogo);

            Label lblCompany = new Label { 
                Text = ThemeConfig.CompanyName.ToUpper(), 
                Font = new Font("Segoe UI", 24, FontStyle.Bold), 
                ForeColor = ThemeConfig.PrimaryColor,
                Location = new Point(130, y), 
                AutoSize = true 
            };
            pnlContent.Controls.Add(lblCompany);

            Label lblQuoteTitle = new Label {
                Text = LocalizationManager.IsArabic ? "\u0639\u0631\u0636 \u0633\u0639\u0631" : "QUOTATION",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Location = new Point(pnlContent.Width - 350, y + 5),
                Size = new Size(310, 40),
                TextAlign = LocalizationManager.IsArabic ? ContentAlignment.TopLeft : ContentAlignment.TopRight
            };
            pnlContent.Controls.Add(lblQuoteTitle);

            y += 45;
            Label lblCompInfo = new Label {
                Text = "[Street Address] | [City, ST ZIP]\nWebsite: somedomain.com | Phone: [000-000-0000]",
                Font = new Font("Segoe UI", 9),
                Location = new Point(130, y),
                Size = new Size(400, 40),
                ForeColor = Color.Gray
            };
            pnlContent.Controls.Add(lblCompInfo);

            // Quote Details Strip
            y += 60;
            Panel pnlDetails = new Panel { BackColor = Color.FromArgb(245, 247, 250), Location = new Point(40, y), Size = new Size(pnlContent.Width - 80, 40) };
            pnlContent.Controls.Add(pnlDetails);

            string customerId = DatabaseHelper.ExecuteScalar<string>($"SELECT customer_id FROM orders WHERE order_id = {_orderId}");
            Label lblQuoteInfo = new Label {
                Text = $"QUOTE #: {_orderId}   |   DATE: {DateTime.Now:dd MMM yyyy}   |   CUST ID: {customerId ?? "N/A"}   |   VALIDITY: 15 Days",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlDetails.Controls.Add(lblQuoteInfo);

            y += 60;

            // Customer Section
            Label lblCustHeader = new Label { Text = "CUSTOMER DETAILS", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = ThemeConfig.PrimaryColor, Location = new Point(40, y), AutoSize = true };
            pnlContent.Controls.Add(lblCustHeader);
            y += 25;

            string customerName = DatabaseHelper.ExecuteScalar<string>($@"
                SELECT COALESCE(c.full_name, 'Walk-in Customer') 
                FROM orders o LEFT JOIN customers c ON o.customer_id = c.customer_id 
                WHERE o.order_id = {_orderId}");

            Label lblCustInfo = new Label {
                Text = $"{customerName}\n[Company Name] | [Street Address] | [Phone]",
                Font = new Font("Segoe UI", 10),
                Location = new Point(40, y),
                Size = new Size(600, 45),
                ForeColor = Color.Black
            };
            pnlContent.Controls.Add(lblCustInfo);

            y += 60;

            // Table
            DataGridView grid = new DataGridView();
            grid.Location = new Point(40, y);
            grid.Width = pnlContent.Width - 80;
            grid.AllowUserToAddRows = false; grid.ReadOnly = true; grid.RowHeadersVisible = false;
            grid.BackgroundColor = Color.White; grid.BorderStyle = BorderStyle.None; grid.ScrollBars = ScrollBars.None;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.EnableHeadersVisualStyles = false;
            grid.AllowUserToResizeRows = false;
            grid.RowTemplate.Height = 75; // Professional height for photos
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.DefaultCellStyle.Padding = new Padding(5);
            
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            
            ThemeConfig.ApplyGridTheme(grid);
            grid.ColumnHeadersHeight = 40;
            grid.RowTemplate.Height = 60; // Minimum height for photos

            grid.Columns.Add(new DataGridViewImageColumn { Name = "Photo", HeaderText = "PHOTO", Width = 60, ImageLayout = DataGridViewImageCellLayout.Zoom });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Desc", HeaderText = "ITEM DESCRIPTION", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "QTY", Width = 60, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "PRICE", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "TOTAL", Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) } });

            foreach(var item in items)
            {
                Image partImg = ThemeConfig.GetNuricon("pos");
                try { if (!string.IsNullOrEmpty(item.PartImage) && System.IO.File.Exists(item.PartImage)) partImg = Image.FromFile(item.PartImage); } catch { }
                grid.Rows.Add(partImg, $"{item.PartName}\n{item.Description}", item.Quantity, item.UnitPrice.ToString("N2"), (item.Quantity * item.UnitPrice).ToString("N2"));
            }

            // Force auto-resize to fit content
            grid.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);

            // Calculate exact height based on rendered rows
            int totalRowsHeight = 0;
            foreach (DataGridViewRow row in grid.Rows) totalRowsHeight += row.Height;
            grid.Height = grid.ColumnHeadersHeight + totalRowsHeight + 10;
            pnlContent.Controls.Add(grid);
            
            y += grid.Height + 40;

            // Summary & Terms Side-by-Side
            Panel pnlSummaryWrap = new Panel { Location = new Point(40, y), Size = new Size(pnlContent.Width - 80, 180) };
            pnlContent.Controls.Add(pnlSummaryWrap);

            // Terms (Left)
            Label lblTermsHead = new Label { Text = "TERMS AND CONDITIONS", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = ThemeConfig.PrimaryColor, Location = new Point(0, 0), AutoSize = true };
            pnlSummaryWrap.Controls.Add(lblTermsHead);
            
            Label lblTerms = new Label {
                Text = "â€¢ Validity: 15 days from issue.\nâ€¢ Payment due prior to delivery.\nâ€¢ Acceptance indicates billing confirmation.\n\nAccepted By: __________________________",
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(0, 25),
                Size = new Size(400, 140),
                ForeColor = Color.DimGray
            };
            pnlSummaryWrap.Controls.Add(lblTerms);

            // Summary (Right)
            decimal taxRate = 0.0625m;
            decimal taxAmount = total * taxRate;
            decimal grandTotal = total + taxAmount;

            int sx = pnlSummaryWrap.Width - 280;
            string[] labels = { "Subtotal", "Taxable Amount", "Tax (6.25%)", "GRAND TOTAL" };
            string[] vals = { total.ToString("C2"), total.ToString("C2"), taxAmount.ToString("C2"), grandTotal.ToString("C2") };

            for (int i = 0; i < 4; i++) {
                bool isLast = (i == 3);
                Label lblL = new Label { Text = labels[i], Font = new Font("Segoe UI", isLast ? 10 : 9, isLast ? FontStyle.Bold : FontStyle.Regular), Location = new Point(sx, i * 28), Size = new Size(130, 25), TextAlign = ContentAlignment.MiddleRight };
                Label lblV = new Label { 
                    Text = vals[i], 
                    Font = new Font("Segoe UI", isLast ? 12 : 10, isLast ? FontStyle.Bold : FontStyle.Regular), 
                    Location = new Point(sx + 135, i * 28), 
                    Size = new Size(140, 25), 
                    TextAlign = ContentAlignment.MiddleRight,
                    ForeColor = isLast ? ThemeConfig.PrimaryColor : Color.Black
                };
                pnlSummaryWrap.Controls.Add(lblL);
                pnlSummaryWrap.Controls.Add(lblV);
            }

            y += 200;
            Label lblFinal = new Label {
                Text = "Thank you for your business! Please contact us if you have any questions.",
                Font = new Font("Segoe UI", 10, FontStyle.Bold | FontStyle.Italic),
                Location = new Point(0, y),
                Size = new Size(pnlContent.Width, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            pnlContent.Controls.Add(lblFinal);

            y += 40;
            Label lblContactFooter = new Label {
                Text = "Phone: +1 (555) 000-0000  |  Email: contact@acmecorp.com  |  Website: www.acmecorp.com",
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(0, y),
                Size = new Size(pnlContent.Width, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Silver
            };
            pnlContent.Controls.Add(lblContactFooter);

            // Ensure pnlContent is tall enough for all footer elements
            pnlContent.Height = Math.Max(1100, y + 60);
        }

        private void HandlePrint()
        {
            try
            {
                using (PrintDocument pd = new PrintDocument())
                {
                    pd.DocumentName = $"Quotation_{_orderId}";
                    pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);

                    pd.PrintPage += (s, e) => DrawDocumentToGraphics(e.Graphics, e.MarginBounds);

                    using (PrintDialog diag = new PrintDialog { Document = pd, UseEXDialog = true })
                    {
                        if (diag.ShowDialog() == DialogResult.OK)
                        {
                            pd.Print();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError("Printing failed: " + ex.Message);
            }
        }

        private void DrawDocumentToGraphics(Graphics g, Rectangle marginBounds)
        {
            // Create a bitmap of the content panel
            using (Bitmap bmp = new Bitmap(pnlContent.Width, pnlContent.Height))
            {
                pnlContent.DrawToBitmap(bmp, new Rectangle(0, 0, pnlContent.Width, pnlContent.Height));

                // Calculate scaling to fit the page width
                float printableWidth = marginBounds.Width;
                float scale = printableWidth / bmp.Width;

                int targetWidth = (int)(bmp.Width * scale);
                int targetHeight = (int)(bmp.Height * scale);

                // Draw the bitmap to the printer graphics
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmp, new Rectangle(marginBounds.Left, marginBounds.Top, targetWidth, targetHeight));
            }
        }

        private void HandleExport()
        {
            try
            {
                using (PrintDocument pd = new PrintDocument())
                {
                    // Find an available PDF printer
                    string pdfPrinter = null;
                    foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                    {
                        if (printer.Contains("PDF"))
                        {
                            pdfPrinter = printer;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(pdfPrinter))
                    {
                        MessageHelper.ShowWarning("No PDF printer found (e.g. Microsoft Print to PDF). Please use the 'Print' button and select a PDF printer manually.");
                        return;
                    }

                    using (SaveFileDialog diag = new SaveFileDialog())
                    {
                        diag.Filter = "PDF Document|*.pdf";
                        diag.FileName = $"Quotation_{_orderId}";
                        diag.Title = "Export to PDF";

                        if (diag.ShowDialog() == DialogResult.OK)
                        {
                            pd.PrinterSettings.PrinterName = pdfPrinter;
                            pd.PrinterSettings.PrintToFile = true;
                            pd.PrinterSettings.PrintFileName = diag.FileName;
                            pd.DocumentName = $"Quotation_{_orderId}";
                            pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);

                            pd.PrintPage += (s, e) => DrawDocumentToGraphics(e.Graphics, e.MarginBounds);
                            
                            pd.Print();
                            MessageHelper.ShowInfo("Quotation exported as PDF successfully!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError("Export failed: " + ex.Message);
            }
        }
    }
}
