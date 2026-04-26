using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Services;
using GenericInventorySystem.Controls;

namespace GenericInventorySystem.Forms
{
    public partial class BlindReturnForm : BaseModalForm
    {
        private DataGridView dgvItems;
        private TextBox txtReason;
        private ComboBox cmbCustomer;
        private Label lblTotalRefund;
        private DataTable _itemsTable;
        private InventoryService _inventoryService;
        private ReturnService _returnService;
        
        // Barcode Scanner Buffer
        private DateTime _lastScanTime = DateTime.Now;
        private string _scanBuffer = "";

        public BlindReturnForm()
        {
            _inventoryService = new InventoryService();
            _returnService = new ReturnService();
            
            this.TitleText = LocalizationManager.IsArabic ? "إرجاع صنف (بدون فاتورة)" : "Item Return (No Receipt)";

            InitializeForm();
        }

        private void InitializeForm()
        {
            this.SuspendLayout();

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20)
            };
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Search area
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid space
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F)); // Bottom area

            // Label indicating scanner is active
            Panel pnlSearch = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            Label lblScannerReady = new Label { Text = LocalizationManager.IsArabic ? "جاهز لمسح الباركود..." : "Ready to scan barcode...", Font = ThemeConfig.SubHeaderFont, ForeColor = ThemeConfig.SecondaryColor, AutoSize = true, Location = new Point(0, 15) };
            pnlSearch.Controls.Add(lblScannerReady);
            tlpMain.Controls.Add(pnlSearch, 0, 0);

            // Global Barcode Scan Support
            this.KeyPreview = true;
            this.KeyPress += BlindReturnForm_KeyPress;

            // Item Grid
            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoGenerateColumns = false, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None };
            ThemeConfig.ApplyGridTheme(dgvItems);
            
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartID", DataPropertyName = "part_id", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartName", DataPropertyName = "part_name", HeaderText = LocalizationManager.IsArabic ? "اسم الصنف" : "Product", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", DataPropertyName = "price", HeaderText = LocalizationManager.IsArabic ? "السعر" : "Price", Width = 100, ReadOnly = true });
            
            DataGridViewTextBoxColumn colReturn = new DataGridViewTextBoxColumn { Name = "QtyToReturn", DataPropertyName = "quantity", HeaderText = LocalizationManager.IsArabic ? "الكمية المرتجعة" : "Return Qty", Width = 120 };
            colReturn.DefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
            dgvItems.Columns.Add(colReturn);
            
            DataGridViewButtonColumn colRemove = new DataGridViewButtonColumn { Name = "Remove", HeaderText = "", Text = "X", UseColumnTextForButtonValue = true, Width = 50, FlatStyle = FlatStyle.Flat };
            colRemove.DefaultCellStyle.ForeColor = ThemeConfig.DangerColor;
            dgvItems.Columns.Add(colRemove);

            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            dgvItems.CellContentClick += DgvItems_CellContentClick;
            tlpMain.Controls.Add(dgvItems, 0, 1);

            _itemsTable = new DataTable();
            _itemsTable.Columns.Add("part_id", typeof(int));
            _itemsTable.Columns.Add("part_name", typeof(string));
            _itemsTable.Columns.Add("price", typeof(decimal));
            _itemsTable.Columns.Add("quantity", typeof(int));
            dgvItems.DataSource = _itemsTable;

            // Bottom Area
            TableLayoutPanel tlpBottom = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0, 10, 0, 0)
            };
            tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpBottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

            // Reason and Customer Section (Left)
            TableLayoutPanel tlpReason = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            tlpReason.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpReason.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tlpReason.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpReason.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Label lblCustomer = new Label { Text = (LocalizationManager.IsArabic ? "العميل (اختياري، للخصم من الرصيد)" : "Customer (Optional, to credit balance)") + ":", AutoSize = true, Font = ThemeConfig.SubHeaderFont, Margin = new Padding(0, 0, 0, 5) };
            cmbCustomer = new ComboBox { Dock = DockStyle.Fill, Font = ThemeConfig.StandardFont, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 10, 10) };
            ThemeConfig.ApplyComboBoxStyle(cmbCustomer);

            Label lblReason = new Label { Text = (LocalizationManager.IsArabic ? "سبب الإرجاع" : "Return Reason") + ":", AutoSize = true, Font = ThemeConfig.SubHeaderFont, Margin = new Padding(0, 0, 0, 5) };
            txtReason = new TextBox { Multiline = true, Dock = DockStyle.Fill, Font = ThemeConfig.StandardFont, Margin = new Padding(0, 0, 10, 0) };
            
            tlpReason.Controls.Add(lblCustomer, 0, 0);
            tlpReason.Controls.Add(cmbCustomer, 0, 1);
            tlpReason.Controls.Add(lblReason, 0, 2);
            tlpReason.Controls.Add(txtReason, 0, 3);
            
            tlpBottom.Controls.Add(tlpReason, 0, 0);

            LoadCustomers();

            // Summary Section (Right)
            lblTotalRefund = new Label { Text = LocalizationManager.IsArabic ? "إجمالي المبلغ: $0.00" : "Total Refund: $0.00", Dock = DockStyle.Fill, Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.PrimaryColor, TextAlign = ContentAlignment.TopRight };
            tlpBottom.Controls.Add(lblTotalRefund, 1, 0);

            tlpMain.Controls.Add(tlpBottom, 0, 2);
            this.ContentPanel.Controls.Add(tlpMain);

            SetFooterButtons(
                LocalizationManager.IsArabic ? "إتمام الإرجاع" : "Process Return",
                LocalizationManager.IsArabic ? "إلغاء" : "Cancel",
                BtnSubmit_Click,
                (s, e) => this.Close()
            );

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadCustomers()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteDataTable("SELECT id, name FROM customers WHERE date_deleted IS NULL");
                dt.Rows.InsertAt(dt.NewRow(), 0);
                dt.Rows[0]["id"] = -1;
                dt.Rows[0]["name"] = LocalizationManager.IsArabic ? "-- لا يوجد / إرجاع نقدي --" : "-- None / Cash Return --";
                cmbCustomer.DataSource = dt;
                cmbCustomer.DisplayMember = "name";
                cmbCustomer.ValueMember = "id";
            }
            catch { }
        }

        private void BlindReturnForm_KeyPress(object sender, KeyPressEventArgs e)
        {
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

                    DataRow partInfo = _inventoryService.GetPartByBarcodeOrNumber(barcode);
                    
                    if (partInfo != null)
                    {
                        int partId = Convert.ToInt32(partInfo["id"]);
                        string partName = partInfo["part_name"].ToString();
                        decimal price = Convert.ToDecimal(partInfo["selling_price"]);

                        bool exists = false;
                        foreach (DataRow row in _itemsTable.Rows)
                        {
                            if (Convert.ToInt32(row["part_id"]) == partId)
                            {
                                row["quantity"] = Convert.ToInt32(row["quantity"]) + 1;
                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            _itemsTable.Rows.Add(partId, partName, price, 1);
                        }

                        CalculateTotal();
                    }
                    else
                    {
                        string warnMsg = LocalizationManager.IsArabic 
                            ? $"لم يتم العثور على الصنف في المخزون (الباركود: {barcode}). لا يمكن إرجاع هذا الصنف." 
                            : $"Item not found in inventory (Barcode: {barcode}). Cannot return this item.";
                        MessageHelper.ShowWarning(warnMsg);
                    }

                    return true; // Suppress Enter key!
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void DgvItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvItems.Columns[e.ColumnIndex].Name == "QtyToReturn")
            {
                CalculateTotal();
            }
        }

        private void DgvItems_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvItems.Columns[e.ColumnIndex].Name == "Remove")
            {
                _itemsTable.Rows.RemoveAt(e.RowIndex);
                CalculateTotal();
            }
        }

        private void CalculateTotal()
        {
            decimal total = 0;
            foreach (DataRow row in _itemsTable.Rows)
            {
                if (row["quantity"] != DBNull.Value && int.TryParse(row["quantity"].ToString(), out int qty))
                {
                    decimal price = Convert.ToDecimal(row["price"]);
                    total += qty * price;
                }
            }
            lblTotalRefund.Text = (LocalizationManager.IsArabic ? "إجمالي المبلغ: " : "Total Refund: ") + CurrencyService.Format(total);
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            List<ReturnItemInfo> returnItems = new List<ReturnItemInfo>();
            foreach (DataRow row in _itemsTable.Rows)
            {
                if (row["quantity"] != DBNull.Value && int.TryParse(row["quantity"].ToString(), out int qty) && qty > 0)
                {
                    returnItems.Add(new ReturnItemInfo
                    {
                        PartId = Convert.ToInt32(row["part_id"]),
                        Quantity = qty,
                        RefundAmount = qty * Convert.ToDecimal(row["price"])
                    });
                }
            }

            if (returnItems.Count == 0)
            {
                string msg = LocalizationManager.IsArabic ? "يرجى إدخال صنف واحد على الأقل لإرجاعه." : "Please enter at least one item to return.";
                MessageHelper.ShowWarning(msg);
                return;
            }

            // Reason is now optional


            int customerId = -1;
            if (cmbCustomer.SelectedValue != null && int.TryParse(cmbCustomer.SelectedValue.ToString(), out int cid) && cid > 0)
            {
                customerId = cid;
            }

            string confirmMsg = LocalizationManager.IsArabic ? "هل أنت متأكد من رغبتك في معالجة هذا المرتجع؟" : "Are you sure you want to process this return?";
            if (MessageHelper.ConfirmAction(confirmMsg))
            {
                try
                {
                    _returnService.ProcessBlindReturn(returnItems, txtReason.Text, customerId);
                    MessageHelper.ShowSuccess(LocalizationManager.IsArabic ? "تمت معالجة المرتجع بنجاح!" : "Return processed successfully!");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageHelper.ShowError("Failed to process return: " + ex.Message);
                }
            }
        }
    }
}
