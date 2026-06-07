using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Services;
using InventorySystem.Controls;

namespace InventorySystem.Forms
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
            // Adaptive sizing handled by BaseModalForm.OnLoad

            InitializeForm();
            LoadOrderItems();
        }

        private void InitializeForm()
        {
            this.SuspendLayout();

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(20)
            };
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid space
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 220F)); // Bottom area

            // Item Grid
            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoGenerateColumns = false, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None };
            ThemeConfig.ApplyGridTheme(dgvItems);
            
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartID", DataPropertyName = "part_id", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartName", DataPropertyName = "part_name", HeaderText = LocalizationManager.GetString("POS_GridProduct"), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "QtyOrdered", DataPropertyName = "quantity", HeaderText = "Ordered", Width = 80, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", DataPropertyName = "price", HeaderText = LocalizationManager.GetString("POS_GridPrice"), Width = 100, ReadOnly = true });
            
            DataGridViewTextBoxColumn colReturn = new DataGridViewTextBoxColumn { Name = "QtyToReturn", DataPropertyName = "QtyToReturn", HeaderText = LocalizationManager.GetString("Return_Qty"), Width = 100 };
            colReturn.DefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
            dgvItems.Columns.Add(colReturn);
            
            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            
            // Allow easy editing of the return quantity
            dgvItems.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvItems.EditMode = DataGridViewEditMode.EditOnEnter;

            tlpMain.Controls.Add(dgvItems, 0, 0);

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

            // Reason Section (Left)
            TableLayoutPanel tlpReason = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            Label lblReason = new Label { Text = LocalizationManager.GetString("AdjustStock_Reason") + ":", AutoSize = true, Font = ThemeConfig.SubHeaderFont, Margin = new Padding(0, 0, 0, 5) };
            txtReason = new TextBox { Multiline = true, Dock = DockStyle.Fill, Font = ThemeConfig.StandardFont, Margin = new Padding(0, 0, 10, 0) };
            tlpReason.Controls.Add(lblReason, 0, 0);
            tlpReason.Controls.Add(txtReason, 0, 1);
            tlpBottom.Controls.Add(tlpReason, 0, 0);

            // Summary Section (Right)
            lblTotalRefund = new Label { Text = "Total Refund: $0.00", Dock = DockStyle.Fill, Font = ThemeConfig.HeaderFont, ForeColor = ThemeConfig.PrimaryColor, TextAlign = ContentAlignment.BottomRight };
            tlpBottom.Controls.Add(lblTotalRefund, 1, 1);
            tlpBottom.SetRowSpan(tlpReason, 2);

            tlpMain.Controls.Add(tlpBottom, 0, 1);
            this.ContentPanel.Controls.Add(tlpMain);

            SetFooterButtons(
                LocalizationManager.GetString("Return_Action"),
                LocalizationManager.GetString("AddPart_Cancel"),
                BtnSubmit_Click,
                (s, e) => this.Close()
            );

            this.ResumeLayout(false);
            this.PerformLayout();
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
                        string msg = LocalizationManager.IsArabic 
                            ? $"لا يمكن أن تتجاوز الكمية المرتجعة الكمية المطلوبة للصنف: {row.Cells["PartName"].Value}"
                            : $"Return quantity cannot exceed ordered quantity for item: {row.Cells["PartName"].Value}";
                        MessageHelper.ShowWarning(msg);
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
                string msg = LocalizationManager.GetString("Msg_ReturnOneItem");
                MessageHelper.ShowWarning(msg);
                return;
            }

            // Reason is now optional


            string confirmMsg = LocalizationManager.GetString("Msg_ConfirmReturn");
            if (MessageHelper.ConfirmAction(confirmMsg))
            {
                try
                {
                    _returnService.ProcessReturn(_orderId, returnItems, txtReason.Text);
                    MessageHelper.ShowSuccess(LocalizationManager.GetString("Msg_ReturnSuccess"));
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
