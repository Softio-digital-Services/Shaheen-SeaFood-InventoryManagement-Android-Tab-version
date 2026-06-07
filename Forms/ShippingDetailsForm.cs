using System;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Data;
using InventorySystem.Controls;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
{
    public partial class ShippingDetailsForm : BaseModalForm
    {
        private ModernComboBox cmbCustomer;
        private ModernTextBox txtShippingTo;
        private FlatDateTimePicker dtpOrderDate;
        private FlatDateTimePicker dtpDeliveryDate;
        private FlatDateTimePicker dtpPaymentDueDate;
        
        public int SelectedCustomerId { get; private set; } = -1;
        public string ShippingTo { get; private set; }
        public DateTime OrderDate { get; private set; }
        public DateTime DeliveryDate { get; private set; }
        public DateTime PaymentDueDate { get; private set; }
        
        public ShippingDetailsForm()
        {
            this.TitleText = "Shipping Details";
            InitializeCustomComponent();
            
            SetFooterButtons(
                "Save",
                "Cancel",
                BtnSave_Click,
                BtnCancel_Click
            );
            
            LoadCustomers();
        }

        private void InitializeCustomComponent()
        {
            this.Size = new System.Drawing.Size(550, 600); // Increased height a bit for the labels

            TableLayoutPanel tlpMain = new TableLayoutPanel 
            { 
                Dock = DockStyle.Top, 
                ColumnCount = 1, 
                RowCount = 8, 
                AutoSize = true, 
                Padding = new Padding(20) 
            };
            for(int i = 0; i < 8; i++) tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Customer Row
            TableLayoutPanel pnlCustomer = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                ColumnCount = 2, 
                RowCount = 1, 
                AutoSize = true, 
                Margin = new Padding(0, 0, 0, 15) 
            };
            pnlCustomer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlCustomer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 45F));
            
            cmbCustomer = new ModernComboBox { LabelText = "Customer", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 5, 0) };
            
            Button btnAddCustomer = new Button 
            { 
                Width = 35, 
                Margin = new Padding(10, 25, 0, 0)
            };
            ThemeConfig.ApplyStandardAddButton(btnAddCustomer, "");
            btnAddCustomer.Click += BtnAddCustomer_Click;
            
            pnlCustomer.Controls.Add(cmbCustomer, 0, 0);
            pnlCustomer.Controls.Add(btnAddCustomer, 1, 0);
            tlpMain.Controls.Add(pnlCustomer, 0, 0);

            // Shipping To
            txtShippingTo = new ModernTextBox 
            { 
                LabelText = "Shipping To", 
                Dock = DockStyle.Fill, 
                Multiline = true, 
                Height = 80, 
                Margin = new Padding(0, 0, 0, 15) 
            };
            tlpMain.Controls.Add(txtShippingTo, 0, 1);

            // Order Date
            Label lblOrderDate = new Label { Text = "Order Date", Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.TextColorDark, AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
            dtpOrderDate = new FlatDateTimePicker { Dock = DockStyle.Fill, Height = 35, Margin = new Padding(0, 0, 0, 15) };
            tlpMain.Controls.Add(lblOrderDate, 0, 2);
            tlpMain.Controls.Add(dtpOrderDate, 0, 3);

            // Delivery Date
            Label lblDeliveryDate = new Label { Text = "Delivery Date", Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.TextColorDark, AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
            dtpDeliveryDate = new FlatDateTimePicker { Dock = DockStyle.Fill, Height = 35, Margin = new Padding(0, 0, 0, 15) };
            tlpMain.Controls.Add(lblDeliveryDate, 0, 4);
            tlpMain.Controls.Add(dtpDeliveryDate, 0, 5);

            // Payment Due Date
            Label lblPaymentDue = new Label { Text = "Payment Due Date", Font = ThemeConfig.SmallBoldFont, ForeColor = ThemeConfig.TextColorDark, AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
            dtpPaymentDueDate = new FlatDateTimePicker { Dock = DockStyle.Fill, Height = 35, Margin = new Padding(0, 0, 0, 15) };
            tlpMain.Controls.Add(lblPaymentDue, 0, 6);
            tlpMain.Controls.Add(dtpPaymentDueDate, 0, 7);

            this.ContentPanel.Controls.Add(tlpMain);
        }

        private void LoadCustomers()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteDataTable("SELECT ID, Name FROM Customers ORDER BY Name ASC");
                cmbCustomer.DataSource = dt;
                cmbCustomer.DisplayMember = "Name";
                cmbCustomer.ValueMember = "ID";
                cmbCustomer.SelectedIndex = -1;
            }
            catch { }
        }

        private void BtnAddCustomer_Click(object sender, EventArgs e)
        {
            using (var frm = new AddCustomerForm())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadCustomers();
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue != null)
            {
                SelectedCustomerId = Convert.ToInt32(cmbCustomer.SelectedValue);
            }
            
            ShippingTo = txtShippingTo.Text.Trim();
            OrderDate = dtpOrderDate.Value.GetValueOrDefault(DateTime.Now);
            DeliveryDate = dtpDeliveryDate.Value.GetValueOrDefault(DateTime.Now);
            PaymentDueDate = dtpPaymentDueDate.Value.GetValueOrDefault(DateTime.Now);
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
