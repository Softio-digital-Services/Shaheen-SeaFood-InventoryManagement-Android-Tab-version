using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Services;

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
            
            this.TitleText = "Quotation Preview - #" + orderId;
            this.Size = new Size(950, 950); // Increased width to ensure A4 fits
            this.BackColor = ThemeConfig.BackgroundColor;
            this.ContentPanel.Padding = new Padding(20, 20, 20, 20); // Add safety margin

            InitializeUI();
            LoadData();
        }

        private void InitializeUI()
        {
            // Adaptive sizing handled by BaseModalForm.OnLoad
            
            SetFooterButtons(
                "Print / Export",
                "Close",
                (s, e) => MessageBox.Show("Print functionality would be integrated with a reporting library (like Crystal Reports or a PDF generator) in a full production environment. For now, this preview represents the final document layout.", "Print Functionality"),
                (s, e) => this.Close()
            );

            // The actual document (A4-ish proportions)
            pnlContent = new Panel { 
                Width = 800, 
                Height = 1100, 
                BackColor = Color.White, 
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
            int y = 40;

            // Header - Logo Placeholder
            PictureBox pbLogo = new PictureBox { Size = new Size(80, 80), Location = new Point(40, y), SizeMode = PictureBoxSizeMode.Zoom };
            pbLogo.Image = ThemeConfig.GetNuricon("pos"); // Use a generic themed icon if logo missing
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

            Label lblDocType = new Label {
                Text = "QUOTATION / PROPOSAL",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ThemeConfig.SecondaryColor,
                Location = new Point(pnlContent.Width - 350, y + 5),
                Size = new Size(310, 40),
                TextAlign = ContentAlignment.TopRight
            };
            pnlContent.Controls.Add(lblDocType);

            y += 90;

            // Details Section (Divider)
            Label divider = new Label { BackColor = ThemeConfig.BorderColor, Height = 2, Width = pnlContent.Width - 80, Location = new Point(40, y) };
            pnlContent.Controls.Add(divider);
            y += 20;

            // Customer Info
            Label lblBillToTitle = new Label { Text = "BILL TO:", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(40, y), AutoSize = true };
            pnlContent.Controls.Add(lblBillToTitle);

            // Fetch customer name
            string customerName = DatabaseHelper.ExecuteScalar<string>($@"
                SELECT ISNULL(c.full_name, 'Walk-in Customer') 
                FROM orders o LEFT JOIN customers c ON o.customer_id = c.customer_id 
                WHERE o.order_id = {_orderId}");

            Label lblCustomer = new Label { Text = customerName, Font = new Font("Segoe UI", 12), Location = new Point(40, y + 25), AutoSize = true };
            pnlContent.Controls.Add(lblCustomer);

            // Quote info
            Label lblQuoteDetails = new Label { 
                Text = $"Quote #: {_orderId}\nDate: {DateTime.Now:dd MMM yyyy}\nValidity: 15 Days", 
                Font = new Font("Segoe UI", 10), 
                Location = new Point(pnlContent.Width - 250, y), 
                Size = new Size(210, 80),
                TextAlign = ContentAlignment.TopRight
            };
            pnlContent.Controls.Add(lblQuoteDetails);

            y += 100;

            // Table
            DataGridView grid = new DataGridView();
            grid.Location = new Point(40, y);
            grid.Width = pnlContent.Width - 80;
            grid.AllowUserToAddRows = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.ScrollBars = ScrollBars.None;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.ClearSelection();
            grid.DataError += (s, ev) => { ev.ThrowException = false; };
            
            ThemeConfig.ApplyGridTheme(grid);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 244, 250);

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Item", HeaderText = "Item / Description", Width = 380, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "Qty", Width = 60, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "Price", Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total", Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) } });

            foreach(var item in items)
            {
                string displayName = item.PartName;
                if (!string.IsNullOrEmpty(item.Description))
                    displayName += "\n" + item.Description;

                grid.Rows.Add(displayName, item.Quantity, item.UnitPrice.ToString("C2"), (item.Quantity * item.UnitPrice).ToString("C2"));
            }

            // Adjust grid height based on content
            int gridHeight = grid.ColumnHeadersHeight;
            foreach (DataGridViewRow row in grid.Rows) gridHeight += row.Height;
            grid.Height = gridHeight + 5;

            pnlContent.Controls.Add(grid);
            y += grid.Height + 30;

            // Summary
            Label lblTotalLabel = new Label { Text = "GRAND TOTAL:", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(pnlContent.Width - 350, y), Size = new Size(150, 30), TextAlign = ContentAlignment.TopRight };
            Label lblTotalVal = new Label { Text = total.ToString("C2"), Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = ThemeConfig.PrimaryColor, Location = new Point(pnlContent.Width - 200, y - 5), Size = new Size(160, 40), TextAlign = ContentAlignment.TopRight };
            
            pnlContent.Controls.Add(lblTotalLabel);
            pnlContent.Controls.Add(lblTotalVal);

            y += 80;

            // Footer
            Label lblNotesTitle = new Label { Text = "Notes / Terms:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(40, y), AutoSize = true };
            Label lblNotes = new Label { Text = "1. This quotation is valid for 15 days from the date of issue.\n2. Prices are subject to availability at the time of order.\n3. Shipping cost may vary based on location.", Font = new Font("Segoe UI", 8, FontStyle.Italic), Location = new Point(40, y + 20), Size = new Size(400, 80), ForeColor = Color.Gray };
            pnlContent.Controls.Add(lblNotesTitle);
            pnlContent.Controls.Add(lblNotes);

            Label lblThankYou = new Label { Text = "Thank you for your business!", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(0, pnlContent.Height - 60), Size = new Size(pnlContent.Width, 30), TextAlign = ContentAlignment.MiddleCenter, ForeColor = ThemeConfig.SecondaryColor };
            pnlContent.Controls.Add(lblThankYou);
        }
    }
}
