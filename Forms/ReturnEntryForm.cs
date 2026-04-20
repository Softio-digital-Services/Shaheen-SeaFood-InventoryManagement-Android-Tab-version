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
    public partial class ReturnEntryForm : BaseModalForm
    {
        private int _orderId;
        private DataGridView dgvItems;
        private TextBox txtReason;
        private Label lblTotalRefund;
        private DataTable _itemsTable;
        private OrderService _orderService;
        private ReturnService _returnService;

        public ReturnEntryForm(int orderId)
        {
            _orderId = orderId;
            _orderService = new OrderService();
            _returnService = new ReturnService();
            
            this.TitleText = LocalizationManager.GetString("Return_Title") + " - Order #" + orderId;
            this.Size = new Size(800, 600);

            InitializeForm();
            LoadOrderItems();
        }

        private void InitializeForm()
        {
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            this.ContentPanel.Controls.Add(pnlMain);

            // Item Grid
            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoGenerateColumns = false, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None };
            ThemeConfig.ApplyGridTheme(dgvItems);
            
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartID", DataPropertyName = "part_id", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartName", DataPropertyName = "part_name", HeaderText = LocalizationManager.GetString("POS_GridProduct"), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "QtyOrdered", DataPropertyName = "quantity", HeaderText = "Ordered", Width = 80, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", DataPropertyName = "price", HeaderText = LocalizationManager.GetString("POS_GridPrice"), Width = 100, ReadOnly = true });
            
            DataGridViewTextBoxColumn colReturn = new DataGridViewTextBoxColumn { Name = "QtyToReturn", HeaderText = LocalizationManager.GetString("Return_Qty"), Width = 100 };
            colReturn.DefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255); // Light blue to indicate editable
            dgvItems.Columns.Add(colReturn);
            
            dgvItems.CellValueChanged += DgvItems_CellValueChanged;

            Panel pnlGridWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 0, 10) };
            pnlGridWrapper.Controls.Add(dgvItems);
            pnlMain.Controls.Add(pnlGridWrapper);

            // Bottom Panel for summary and reason
            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 180 };
            pnlMain.Controls.Add(pnlBottom);

            Label lblReason = new Label { Text = LocalizationManager.GetString("AdjustStock_Reason") + ":", AutoSize = true, Location = new Point(0, 10), Font = ThemeConfig.SubHeaderFont };
            pnlBottom.Controls.Add(lblReason);
            
            txtReason = new TextBox { Multiline = true, Location = new Point(0, 35), Size = new Size(500, 60), Font = ThemeConfig.StandardFont };
            pnlBottom.Controls.Add(txtReason);

            lblTotalRefund = new Label { Text = "Total Refund: $0.00", Location = new Point(520, 35), Size = new Size(240, 30), Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.PrimaryColor, TextAlign = ContentAlignment.TopRight };
            pnlBottom.Controls.Add(lblTotalRefund);

            Button btnSubmit = new ModernButton { Text = LocalizationManager.GetString("Return_Action"), Size = new Size(160, 45), Location = new Point(600, 115) };
            ThemeConfig.ApplyPrimaryButton(btnSubmit);
            btnSubmit.Click += BtnSubmit_Click;
            pnlBottom.Controls.Add(btnSubmit);

            Button btnCancel = new ModernButton { Text = LocalizationManager.GetString("AddPart_Cancel"), Size = new Size(120, 45), Location = new Point(470, 115) };
            ThemeConfig.ApplySecondaryButton(btnCancel);
            btnCancel.Click += (s, e) => this.Close();
            pnlBottom.Controls.Add(btnCancel);
        }

        private void LoadOrderItems()
        {
            try
            {
                var items = _orderService.GetOrderItems(_orderId);
                _itemsTable = new DataTable();
                _itemsTable.Columns.Add("part_id", typeof(int));
                _itemsTable.Columns.Add("part_name", typeof(string));
                _itemsTable.Columns.Add("quantity", typeof(int));
                _itemsTable.Columns.Add("price", typeof(decimal));
                _itemsTable.Columns.Add("QtyToReturn", typeof(int));

                foreach (var item in items)
                {
                    _itemsTable.Rows.Add(item.PartId, item.PartName, item.Quantity, item.UnitPrice, 0);
                }

                dgvItems.DataSource = _itemsTable;
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError("Error loading items: " + ex.Message);
            }
        }

        private void DgvItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvItems.Columns[e.ColumnIndex].Name == "QtyToReturn")
            {
                CalculateTotal();
            }
        }

        private void CalculateTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.Cells["QtyToReturn"].Value != null && row.Cells["QtyToReturn"].Value != DBNull.Value)
                {
                    if (int.TryParse(row.Cells["QtyToReturn"].Value.ToString(), out int qty))
                    {
                        decimal price = (decimal)row.Cells["UnitPrice"].Value;
                        total += qty * price;
                    }
                }
            }
            lblTotalRefund.Text = "Total Refund: " + CurrencyService.Format(total);
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            List<ReturnItemInfo> returnItems = new List<ReturnItemInfo>();
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.Cells["QtyToReturn"].Value != null && int.TryParse(row.Cells["QtyToReturn"].Value.ToString(), out int qty) && qty > 0)
                {
                    int ordered = (int)row.Cells["QtyOrdered"].Value;
                    if (qty > ordered)
                    {
                        MessageHelper.ShowWarning("Return quantity cannot exceed ordered quantity for item: " + row.Cells["PartName"].Value);
                        return;
                    }

                    returnItems.Add(new ReturnItemInfo
                    {
                        PartId = (int)row.Cells["PartID"].Value,
                        Quantity = qty,
                        RefundAmount = qty * (decimal)row.Cells["UnitPrice"].Value
                    });
                }
            }

            if (returnItems.Count == 0)
            {
                MessageHelper.ShowWarning("Please enter at least one item to return.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageHelper.ShowWarning("Please provide a reason for the return.");
                return;
            }

            if (MessageHelper.ConfirmAction("Are you sure you want to process this return?"))
            {
                try
                {
                    _returnService.ProcessReturn(_orderId, returnItems, txtReason.Text);
                    MessageHelper.ShowSuccess("Return processed successfully!");
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
