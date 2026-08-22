using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Shaheen_InventoryManagement_Android.Data;
using Shaheen_InventoryManagement_Android.Helpers;
using Shaheen_InventoryManagement_Android.Controls;

namespace Shaheen_InventoryManagement_Android.Forms
{
    public partial class PartSelectionDialog : BaseModalForm
    {
        private ModernTextBox txtSearch;
        private DataGridView dgvParts;
        private ModernNumericUpDown numQuantity;
        private ModernComboBox cmbUom;

        public PartData SelectedPart { get; private set; }
        public double SelectedQuantity { get; private set; }
        public string SelectedUnitOfMeasure { get; private set; }

        public PartSelectionDialog()
        {
            InitializeComponent();
            ApplyTheme();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.TitleText = "Select Component Part";
            this.Width = 600;
            this.Height = 500;

            txtSearch = new ModernTextBox
            {
                PlaceholderText = "Search parts...",
                Dock = DockStyle.Top,
                IsSearch = true
            };
            txtSearch.TextChanged += (s, e) => FilterData();

            dgvParts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                Margin = new Padding(0, 10, 0, 10)
            };
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "id", DataPropertyName = "Id", Visible = false });
            dgvRecipesPartSelection(dgvParts);

            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10) };
            
            Label lblQty = new Label { Text = "Quantity:", AutoSize = true, Location = new Point(10, 20) };
            numQuantity = new ModernNumericUpDown { Location = new Point(80, 18), Width = 100, Minimum = 0.01m, Maximum = 10000m, DecimalPlaces = 2, Value = 1m, ShowLabel = false };

            Label lblUom = new Label { Text = "Unit:", AutoSize = true, Location = new Point(200, 20) };
            cmbUom = new ModernComboBox { Location = new Point(250, 18), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };

            dgvParts.SelectionChanged += (s, e) =>
            {
                cmbUom.Items.Clear();
                if (dgvParts.SelectedRows.Count > 0)
                {
                    var part = dgvParts.SelectedRows[0].DataBoundItem as PartData;
                    if (part != null)
                    {
                        if (!string.IsNullOrEmpty(part.BigUnit) && !string.IsNullOrEmpty(part.SmallUnit))
                        {
                            cmbUom.Items.Add(part.BigUnit);
                            cmbUom.Items.Add(part.SmallUnit);
                            cmbUom.SelectedIndex = 0;
                        }
                        else
                        {
                            cmbUom.Items.Add(string.IsNullOrEmpty(part.UnitOfMeasure) ? "pcs" : part.UnitOfMeasure);
                            cmbUom.SelectedIndex = 0;
                        }
                    }
                }
            };

            pnlBottom.Controls.Add(lblQty);
            pnlBottom.Controls.Add(numQuantity);
            pnlBottom.Controls.Add(lblUom);
            pnlBottom.Controls.Add(cmbUom);

            this.ContentPanel.Controls.Add(pnlBottom);
            this.ContentPanel.Controls.Add(txtSearch);
            this.ContentPanel.Controls.Add(dgvParts);

            SetFooterButtons("Select Component", "Cancel", BtnAdd_Click, (s, e) => this.Close());
        }

        private void dgvRecipesPartSelection(DataGridView dgv)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemNo", HeaderText = "Item No.", DataPropertyName = "ItemNo", FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartNumber", HeaderText = "SKU", DataPropertyName = "PartNumber", FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartName", HeaderText = "Name", DataPropertyName = "PartName", FillWeight = 35 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitOfMeasure", HeaderText = "UOM", DataPropertyName = "UnitOfMeasure", FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "QuantityInStock", HeaderText = "Stock", DataPropertyName = "QuantityInStock", FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "PurchasePrice", HeaderText = "Unit Cost", DataPropertyName = "PurchasePrice", FillWeight = 15, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
        }

        private void ApplyTheme()
        {
            ThemeConfig.ApplyGridTheme(dgvParts);
        }

        private List<PartData> _allParts;

        private void LoadData()
        {
            try
            {
                _allParts = PartData.GetAllParts();
                FilterData();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Failed to load parts: {ex.Message}");
            }
        }

        private void FilterData()
        {
            if (_allParts == null) return;
            string keyword = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(keyword) || keyword == "search parts...")
            {
                dgvParts.DataSource = _allParts;
            }
            else
            {
                dgvParts.DataSource = _allParts.Where(p => 
                    (p.PartName != null && p.PartName.ToLower().Contains(keyword)) ||
                    (p.PartNumber != null && p.PartNumber.ToLower().Contains(keyword)) ||
                    (p.ItemNo != null && p.ItemNo.ToLower().Contains(keyword))).ToList();
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (dgvParts.SelectedRows.Count == 0)
            {
                MessageHelper.ShowWarning("Please select a part from the list.");
                return;
            }

            var part = dgvParts.SelectedRows[0].DataBoundItem as PartData;
            if (part == null) return;

            if ((double)numQuantity.Value > part.QuantityInStock)
            {
                if (!MessageHelper.ConfirmAction($"Warning: You are adding {numQuantity.Value} but there is only {part.QuantityInStock} in stock. Proceed?"))
                {
                    return;
                }
            }

            SelectedPart = part;
            SelectedQuantity = (double)numQuantity.Value;
            SelectedUnitOfMeasure = cmbUom.SelectedItem?.ToString() ?? "pcs";
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
