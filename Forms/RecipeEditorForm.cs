using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Shaheen_InventoryManagement_Android.Data;
using Shaheen_InventoryManagement_Android.Helpers;
using Shaheen_InventoryManagement_Android.Controls;

namespace Shaheen_InventoryManagement_Android.Forms
{
    public partial class RecipeEditorForm : BaseModalForm
    {
        private int? _recipeId;
        private RecipeData _recipe;
        private BindingList<RecipePartData> _partsList;

        private ModernTextBox txtItemNo;
        private ModernTextBox txtName;
        private ModernTextBox txtDesc;
        private ModernNumericUpDown numPrice;
        private ModernComboBox cmbCategory;
        private DataGridView dgvParts;
        private Label lblTotalCost;
        
        private PictureBox pbImage;
        private ModernButton btnUpload;
        private string _currentImagePath = null;
        
        public RecipeEditorForm(int? recipeId = null)
        {
            _recipeId = recipeId;
            InitializeComponent();
            ApplyTheme();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.TitleText = _recipeId.HasValue ? "Edit Recipe" : "New Recipe";
            this.Width = 950;
            this.Height = 650;

            TableLayoutPanel tlpForm = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5,
                Padding = new Padding(20)
            };
            tlpForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
            tlpForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Left Image Column
            Panel pnlLeft = new Panel { Width = 220, Height = 250, Margin = new Padding(0,0,20,15) };
            pbImage = new PictureBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = ThemeConfig.BackgroundColor };
            btnUpload = new ModernButton { Text = "Upload Image", Dock = DockStyle.Bottom, Height = 35, Margin = new Padding(0,10,0,0) };
            btnUpload.Click += btnUpload_Click;
            ThemeConfig.ApplyPrimaryButton(btnUpload);
            pnlLeft.Controls.Add(pbImage);
            pnlLeft.Controls.Add(btnUpload);
            tlpForm.Controls.Add(pnlLeft, 0, 0);
            tlpForm.SetRowSpan(pnlLeft, 4);

            // Name & Item No.
            FlowLayoutPanel flpName = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            txtItemNo = new ModernTextBox { LabelText = "Item No.", Width = 150, Margin = new Padding(0, 0, 20, 0) };
            txtName = new ModernTextBox { LabelText = "Recipe Name", Width = 470, Margin = new Padding(0) };
            flpName.Controls.Add(txtItemNo);
            flpName.Controls.Add(txtName);
            tlpForm.Controls.Add(flpName, 1, 0);

            // Description
            txtDesc = new ModernTextBox { LabelText = "Description", Dock = DockStyle.Fill, Multiline = true, Height = 60 };
            tlpForm.Controls.Add(txtDesc, 1, 1);

            // Selling Price & Add Component
            FlowLayoutPanel flpPrice = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            numPrice = new ModernNumericUpDown { LabelText = "Selling Price", Width = 150, DecimalPlaces = 2, Maximum = 1000000m, Margin = new Padding(0, 0, 20, 0) };
            cmbCategory = new ModernComboBox { LabelText = "Category", Width = 200, Margin = new Padding(0, 0, 20, 0), DropDownStyle = ComboBoxStyle.DropDown };
            ModernButton btnAddPart = new ModernButton { Text = "Add Component", Size = new Size(160, 35), Margin = new Padding(0, 25, 0, 0) };
            btnAddPart.Click += BtnAddPart_Click;
            ThemeConfig.ApplyStandardAddButton(btnAddPart, "Add Component");
            flpPrice.Controls.Add(numPrice);
            flpPrice.Controls.Add(cmbCategory);
            flpPrice.Controls.Add(btnAddPart);
            tlpForm.Controls.Add(flpPrice, 1, 2);

            // Total Cost (Calculated)
            FlowLayoutPanel flpCost = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = new Padding(0, 10, 0, 0) };
            flpCost.Controls.Add(new Label { Text = "Total Cost:", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 0, 5, 0) });
            lblTotalCost = new Label { Text = "0.00", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            flpCost.Controls.Add(lblTotalCost);
            tlpForm.Controls.Add(flpCost, 1, 3);

            // Parts Grid Section
            Panel pnlGridSection = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 10, 0, 0) };

            dgvParts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Margin = new Padding(0, 10, 0, 0)
            };
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartNumber", HeaderText = "SKU", DataPropertyName = "PartNumber", FillWeight = 20 });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "PartName", HeaderText = "Name", DataPropertyName = "PartName", FillWeight = 40 });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Quantity", DataPropertyName = "Quantity", FillWeight = 15 });
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalCost", HeaderText = "Cost", DataPropertyName = "TotalCost", FillWeight = 15, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
            
            var colActions = new DataGridViewTextBoxColumn { Name = "colActions", HeaderText = "Actions", ReadOnly = true, MinimumWidth = 90, Width = 90, AutoSizeMode = DataGridViewAutoSizeColumnMode.None };
            dgvParts.Columns.Add(colActions);
            dgvParts.CellPainting   += DgvParts_CellPainting;
            dgvParts.CellMouseClick += DgvParts_CellMouseClick;
            dgvParts.CellMouseMove  += DgvParts_CellMouseMove;
            dgvParts.CellMouseLeave += DgvParts_CellMouseLeave;

            Panel pnlGridCard = ThemeConfig.CreateCardPanel(dgvParts);
            pnlGridCard.Dock = DockStyle.Fill;
            pnlGridCard.Margin = new Padding(0, 10, 0, 0);

            pnlGridSection.Controls.Add(pnlGridCard);

            tlpForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 65F));
            tlpForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 85F));
            tlpForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            tlpForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tlpForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tlpForm.Controls.Add(pnlGridSection, 0, 4);
            tlpForm.SetColumnSpan(pnlGridSection, 2);

            this.ContentPanel.Controls.Add(tlpForm);

            SetFooterButtons("Save", "Cancel", BtnSave_Click, (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); });
        }

        private void ApplyTheme()
        {
            ThemeConfig.ApplyGridTheme(dgvParts);
        }

        private void LoadData()
        {
            try
            {
                var cats = CategoryData.GetAllCategories();
                cmbCategory.DisplayMember = "CategoryName";
                cmbCategory.ValueMember = "CategoryName";
                cmbCategory.DataSource = cats;
            }
            catch { }

            if (_recipeId.HasValue)
            {
                _recipe = RecipeData.GetRecipe(_recipeId.Value);
                if (_recipe != null)
                {
                    txtName.Text = _recipe.RecipeName;
                    txtItemNo.Text = _recipe.ItemNo;
                    txtDesc.Text = _recipe.Description;
                    numPrice.Value = _recipe.SellingPrice;
                    cmbCategory.Text = _recipe.CategoryName;
                    _currentImagePath = _recipe.RecipeImage;
                    _partsList = new BindingList<RecipePartData>(_recipe.Parts);
                }
            }
            else
            {
                _recipe = new RecipeData();
                _partsList = new BindingList<RecipePartData>();
                _currentImagePath = null;
            }

            UpdateImagePreview();
            dgvParts.DataSource = _partsList;
            UpdateTotalCost();
        }

        private void UpdateImagePreview()
        {
            if (string.IsNullOrEmpty(_currentImagePath)) { pbImage.Image = null; return; }
            try {
                string fullPath = System.IO.Path.Combine(Application.StartupPath, _currentImagePath);
                if (System.IO.File.Exists(fullPath)) {
                    using (var ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(fullPath))) {
                        var old = pbImage.Image; pbImage.Image = null; if (old != null) old.Dispose();
                        pbImage.Image = System.Drawing.Image.FromStream(ms);
                    }
                }
            } catch { pbImage.Image = null; }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" };
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                try {
                    string assetsDir = System.IO.Path.Combine(Application.StartupPath, "Assets", "Recipes");
                    if(!System.IO.Directory.Exists(assetsDir)) System.IO.Directory.CreateDirectory(assetsDir);
                    string newName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(ofd.FileName);
                    string destPath = System.IO.Path.Combine(assetsDir, newName);
                    System.IO.File.Copy(ofd.FileName, destPath, true);
                    _currentImagePath = "Assets/Recipes/" + newName;
                    UpdateImagePreview();
                } catch(Exception ex) { MessageHelper.ShowError("Error uploading: " + ex.Message); }
            }
        }

        private void UpdateTotalCost()
        {
            decimal total = _partsList.Sum(p => p.TotalCost);
            lblTotalCost.Text = total.ToString("C2");
        }

        private void BtnAddPart_Click(object sender, EventArgs e)
        {
            using (var dialog = new PartSelectionDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var existing = _partsList.FirstOrDefault(p => p.PartId == dialog.SelectedPart.Id);
                    if (existing != null)
                    {
                        existing.Quantity += dialog.SelectedQuantity;
                        dgvParts.Refresh();
                    }
                    else
                    {
                        _partsList.Add(new RecipePartData
                        {
                            PartId = dialog.SelectedPart.Id,
                            PartName = dialog.SelectedPart.PartName,
                            PartNumber = dialog.SelectedPart.PartNumber,
                            UnitCost = dialog.SelectedPart.PurchasePrice,
                            Quantity = dialog.SelectedQuantity
                        });
                    }
                    UpdateTotalCost();
                }
            }
        }

        private void DgvParts_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvParts.Columns[e.ColumnIndex].Name == "colActions")
            {
                var prevMode = e.Graphics.SmoothingMode;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                e.Handled = true; e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                Image imgDel = ThemeConfig.GetNuricon("delete");
                Rectangle delRect = new Rectangle(e.CellBounds.X + (e.CellBounds.Width - 32) / 2, e.CellBounds.Y + (e.CellBounds.Height - 32) / 2, 32, 32);
                if (imgDel != null) e.Graphics.DrawImage(imgDel, delRect);

                e.Graphics.SmoothingMode = prevMode;
            }
        }

        private void DgvParts_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvParts.Columns[e.ColumnIndex].Name == "colActions")
            {
                _partsList.RemoveAt(e.RowIndex);
                UpdateTotalCost();
            }
        }

        private void DgvParts_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvParts.Cursor = (e.RowIndex >= 0 && dgvParts.Columns[e.ColumnIndex].Name == "colActions") ? Cursors.Hand : Cursors.Default;
        }

        private void DgvParts_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dgvParts.Cursor = Cursors.Default;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageHelper.ShowWarning("Please enter a recipe name.");
                return;
            }

            if (RecipeData.RecipeNameExists(txtName.Text.Trim(), _recipeId))
            {
                MessageHelper.ShowWarning("A Recipe with this exact name already exists.");
                return;
            }

            if (_partsList.Count == 0)
            {
                MessageHelper.ShowWarning("A recipe must have at least one component part.");
                return;
            }

            _recipe.RecipeName = txtName.Text.Trim();
            _recipe.ItemNo = txtItemNo.Text.Trim();
            _recipe.Description = txtDesc.Text.Trim();
            _recipe.SellingPrice = numPrice.Value;
            _recipe.CategoryName = cmbCategory.Text;
            _recipe.RecipeImage = _currentImagePath;
            _recipe.Parts = _partsList.ToList();

            try
            {
                if (_recipeId.HasValue)
                {
                    RecipeData.UpdateRecipe(_recipe);
                }
                else
                {
                    RecipeData.AddRecipe(_recipe);
                }
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Failed to save recipe: {ex.Message}");
            }
        }
    }
}
