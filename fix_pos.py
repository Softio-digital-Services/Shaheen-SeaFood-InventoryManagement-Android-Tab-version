import re

with open('Forms/POSForm.cs', 'r', encoding='utf-8') as f:
    content = f.read()

start_idx = content.find('public void RefreshCartDisplay()')
if start_idx == -1:
    print('Could not find RefreshCartDisplay')
    exit(1)

end_idx = content.find('private int GetCartQty(int partId)', start_idx)
if end_idx == -1:
    print('Could not find GetCartQty')
    exit(1)

end_idx = content.rfind('// -------', start_idx, end_idx)
if end_idx == -1:
    end_idx = content.find('private int GetCartQty', start_idx)

good_method = '''public void RefreshCartDisplay()
        {
            if (cartTable == null) return;
            pnlCartItems.SuspendLayout();
            foreach (Control c in pnlCartItems.Controls)
                c.Dispose();
            pnlCartItems.Controls.Clear();

            // Count total items for header
            int totalItems = 0;
            foreach (System.Data.DataRow dr in cartTable.Rows)
                if (dr.RowState != System.Data.DataRowState.Deleted) totalItems += (int)dr["Quantity"];

            if (_lblCartCount != null)
            {
                _lblCartCount.Text = totalItems.ToString("D2");
            }

            // Cart rows
            foreach (System.Data.DataRow row in cartTable.Rows)
            {
                if (row.RowState == System.Data.DataRowState.Deleted) continue;

                int     partId   = (int)row["PartID"];
                string  partName = row["PartName"].ToString();
                int     qty      = (int)row["Quantity"];
                decimal price    = (decimal)row["SellingPrice"];
                decimal total    = (decimal)row["Total"];

                Panel rowPanel = new Panel
                {
                    Height    = 46,
                    Dock      = DockStyle.Top,
                    BackColor = Color.Transparent,
                    Tag       = partId
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
                    Text      = partName,
                    Font      = ThemeConfig.SmallBoldFont ?? new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = ThemeConfig.TextColorDark,
                    AutoSize  = false,
                    Height    = 18,
                    Location  = new Point(16, 4),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                rowPanel.Controls.Add(lblName);

                // qty label
                Label lblQtyTxt = new Label
                {
                    Text      = $"{qty} × ",
                    Font      = ThemeConfig.SmallFont ?? new Font("Segoe UI", 8F),
                    ForeColor = ThemeConfig.SecondaryColor,
                    AutoSize  = true,
                    Height    = 16,
                    Location  = new Point(16, 24),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                rowPanel.Controls.Add(lblQtyTxt);

                ComboBox cmbPrice = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = ThemeConfig.SmallFont ?? new Font("Segoe UI", 8F),
                    Width = 70,
                    Location = new Point(50, 21), // Will be positioned in resize handler
                    TabStop = false
                };
                ThemeConfig.ApplyComboBoxStyle(cmbPrice);
                
                try 
                {
                    System.Data.DataTable dtPrices = DatabaseHelper.ExecuteDataTable($"SELECT selling_price, price2, price3, price4 FROM parts WHERE id = {partId}");
                    if (dtPrices.Rows.Count > 0)
                    {
                        System.Data.DataRow pr = dtPrices.Rows[0];
                        var cand = new System.Collections.Generic.List<decimal>();
                        if (pr["selling_price"] != DBNull.Value && Convert.ToDecimal(pr["selling_price"]) > 0) cand.Add(Convert.ToDecimal(pr["selling_price"]));
                        if (pr["price2"] != DBNull.Value && Convert.ToDecimal(pr["price2"]) > 0) cand.Add(Convert.ToDecimal(pr["price2"]));
                        if (pr["price3"] != DBNull.Value && Convert.ToDecimal(pr["price3"]) > 0) cand.Add(Convert.ToDecimal(pr["price3"]));
                        if (pr["price4"] != DBNull.Value && Convert.ToDecimal(pr["price4"]) > 0) cand.Add(Convert.ToDecimal(pr["price4"]));
                        
                        if (!cand.Contains(price)) cand.Add(price); 

                        cmbPrice.DisplayMember = "Value";
                        cmbPrice.ValueMember = "Key";
                        var itemsList = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<decimal, string>>();
                        foreach (var p in cand)
                        {
                            itemsList.Add(new System.Collections.Generic.KeyValuePair<decimal, string>(p, butcherPOS.Services.CurrencyService.Format(p)));
                        }
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

                // Row total
                Label lblRowTotal = new Label
                {
                    Text      = butcherPOS.Services.CurrencyService.Format(total),
                    Font      = ThemeConfig.SmallBoldFont ?? new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = ThemeConfig.TextColorDark,
                    AutoSize  = false,
                    Width     = 80,
                    Height    = 18,
                    TextAlign = ContentAlignment.MiddleRight,
                    BackColor = Color.Transparent
                };
                rowPanel.Controls.Add(lblRowTotal);

                int capId = partId;
                int bSz   = 22;

                Button btnMinus = new Button
                {
                    Text = "-", Size = new Size(bSz, bSz), FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F), Cursor = Cursors.Hand,
                    BackColor = ThemeConfig.SurfaceColor, ForeColor = ThemeConfig.TextColorDark, TabStop = false
                };
                btnMinus.FlatAppearance.BorderColor = ThemeConfig.BorderColor;
                btnMinus.FlatAppearance.BorderSize  = 1;
                btnMinus.Click += (s, e) => RemoveOneFromCart(capId);

                Label lblQty = new Label
                {
                    Text = qty.ToString(), Size = new Size(20, bSz),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = ThemeConfig.SmallBoldFont ?? new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    ForeColor = ThemeConfig.TextColorDark, BackColor = Color.Transparent
                };

                Button btnPlus = new Button
                {
                    Text = "+", Size = new Size(bSz, bSz), FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand,
                    BackColor = ThemeConfig.PrimaryColor, ForeColor = Color.White, TabStop = false
                };
                btnPlus.FlatAppearance.BorderSize = 0;
                btnPlus.Paint += (s2, pe2) =>
                {
                    pe2.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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
                    int stock = 0;
                    try { stock = DatabaseHelper.ExecuteScalar<int>($"SELECT quantity_in_stock FROM parts WHERE id={capId}"); } catch { stock = 999; }
                    foreach (System.Data.DataRow dr in cartTable.Rows)
                    {
                        if (dr.RowState != System.Data.DataRowState.Deleted && (int)dr["PartID"] == capId)
                        {
                            int curQty = (int)dr["Quantity"];
                            if (curQty + 1 > stock) { MessageHelper.ShowWarning(LocalizationManager.GetString("POS_NotEnoughStock") ?? "Not enough stock!"); return; }
                            dr["Quantity"] = curQty + 1;
                            break;
                        }
                    }
                    RefreshCartDisplay();
                };

                rowPanel.Resize += (s, ev) =>
                {
                    int rX = rowPanel.Width - 16;
                    lblName.Width     = rX - 100;
                    lblRowTotal.Location = new Point(rX - 80, 4);
                    btnPlus.Location     = new Point(rX - bSz, 22);
                    lblQty.Location      = new Point(rX - bSz - 20, 22);
                    btnMinus.Location    = new Point(rX - bSz - 20 - bSz, 22);
                    cmbPrice.Location    = new Point(lblQtyTxt.Right, 21);
                };

                rowPanel.Controls.AddRange(new Control[] { lblRowTotal, btnMinus, lblQty, btnPlus });
                pnlCartItems.Controls.Add(rowPanel);
            }

            pnlCartItems.ResumeLayout();
            UpdateTotal();
            UpdateProductCardQtyAll();

            // Update "Ordered Items" count badge
            if (_lblCartCount != null)
                _lblCartCount.Text = (cartTable?.Rows.Count ?? 0).ToString("D2");
        }

'''

new_content = content[:start_idx] + good_method + content[end_idx:]

with open('Forms/POSForm.cs', 'w', encoding='utf-8') as f:
    f.write(new_content)

print('Successfully restored and updated RefreshCartDisplay!')
