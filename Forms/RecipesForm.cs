using System;
using System.Drawing;
using System.Windows.Forms;
using Shaheen_InventoryManagement_Android.Data;
using Shaheen_InventoryManagement_Android.Helpers;
using Shaheen_InventoryManagement_Android.Controls;
using System.Linq;

namespace Shaheen_InventoryManagement_Android.Forms
{
    public partial class RecipesForm : UserControl
    {
        private DataGridView dgvRecipes;
        private ModernButton btnAdd;

        public RecipesForm()
        {
            InitializeComponent();
            ApplyTheme();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.dgvRecipes = new DataGridView();
            this.btnAdd = new ModernButton();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipes)).BeginInit();
            this.SuspendLayout();

            // Root layout
            TableLayoutPanel tlpRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
                Padding = new Padding(20, 16, 20, 16)
            };
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Header
            Label lblTitle = ThemeConfig.CreateStandardHeader("Recipes Management");
            btnAdd.Size = new Size(120, 35);
            btnAdd.Click += BtnAdd_Click;
            ThemeConfig.ApplyStandardAddButton(btnAdd, "New Recipe");

            Button btnDeleteSelected = new Button { Size = new Size(130, 35) };
            btnDeleteSelected.Click += BtnDeleteSelected_Click;
            ThemeConfig.ApplyStandardDeleteButton(btnDeleteSelected, "Delete");

            var tlpHeader = ThemeConfig.CreateGlobalFormHeader(lblTitle, null, new Control[] { btnAdd, btnDeleteSelected });
            tlpRoot.Controls.Add(tlpHeader, 0, 0);

            // Grid
            dgvRecipes.AllowUserToAddRows = false;
            dgvRecipes.ReadOnly = true;
            dgvRecipes.AutoGenerateColumns = false;
            dgvRecipes.BorderStyle = BorderStyle.None;
            dgvRecipes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRecipes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecipes.RowHeadersVisible = false;

            dgvRecipes.Columns.Add(new DataGridViewTextBoxColumn { Name = "id", DataPropertyName = "Id", Visible = false });
            dgvRecipes.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemNo", HeaderText = "Item No.", DataPropertyName = "ItemNo", FillWeight = 15 });
            dgvRecipes.Columns.Add(new DataGridViewTextBoxColumn { Name = "RecipeName", HeaderText = "Recipe Name", DataPropertyName = "RecipeName", FillWeight = 25 });
            dgvRecipes.Columns.Add(new DataGridViewTextBoxColumn { Name = "CategoryName", HeaderText = "Category", DataPropertyName = "CategoryName", FillWeight = 15 });
            dgvRecipes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", DataPropertyName = "Description", FillWeight = 35 });
            dgvRecipes.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalCost", HeaderText = "Total Cost", DataPropertyName = "TotalCost", FillWeight = 12, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
            dgvRecipes.Columns.Add(new DataGridViewTextBoxColumn { Name = "SellingPrice", HeaderText = "Selling Price", DataPropertyName = "SellingPrice", FillWeight = 13, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });

            var colActions = new DataGridViewTextBoxColumn { Name = "colActions", HeaderText = "Actions", ReadOnly = true, MinimumWidth = 90, Width = 90, AutoSizeMode = DataGridViewAutoSizeColumnMode.None };
            dgvRecipes.Columns.Add(colActions);

            dgvRecipes.CellPainting   += DgvRecipes_CellPainting;
            dgvRecipes.CellMouseClick += DgvRecipes_CellMouseClick;
            dgvRecipes.CellMouseMove  += DgvRecipes_CellMouseMove;
            dgvRecipes.CellMouseLeave += DgvRecipes_CellMouseLeave;

            Panel pnlGridCard = ThemeConfig.CreateCardPanel(dgvRecipes);
            pnlGridCard.Dock = DockStyle.Fill;
            pnlGridCard.Margin = new Padding(0, 16, 0, 0);
            tlpRoot.Controls.Add(pnlGridCard, 0, 1);

            this.Controls.Add(tlpRoot);
            this.Dock = DockStyle.Fill;
            this.BackColor = ThemeConfig.BackgroundColor;

            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipes)).EndInit();
            this.ResumeLayout(false);
        }

        private void ApplyTheme()
        {
            ThemeConfig.ApplyGridTheme(dgvRecipes);
        }

        private void LoadData()
        {
            try
            {
                var recipes = RecipeData.GetAllRecipes();
                dgvRecipes.DataSource = null;
                dgvRecipes.DataSource = recipes;
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Failed to load recipes: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new RecipeEditorForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnDeleteSelected_Click(object sender, EventArgs e)
        {
            if (dgvRecipes.SelectedRows.Count == 0)
            {
                MessageHelper.ShowWarning("Please select a recipe to delete.");
                return;
            }

            if (MessageHelper.ConfirmAction("Are you sure you want to delete the selected recipe(s)? Components will be returned to inventory."))
            {
                foreach (DataGridViewRow row in dgvRecipes.SelectedRows)
                {
                    if (row.Cells["id"].Value is int id)
                    {
                        RecipeData.DeleteRecipe(id);
                    }
                }
                LoadData();
            }
        }

        private void DgvRecipes_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (dgvRecipes.Columns[e.ColumnIndex].Name == "colActions")
            {
                e.Handled = true; e.PaintBackground(e.CellBounds, true);
                Image imgEdit = ThemeConfig.GetNuricon("edit"); 
                Image imgDel = ThemeConfig.GetNuricon("delete");
                Rectangle editRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + (e.CellBounds.Height - 32) / 2, 32, 32);
                Rectangle delRect  = new Rectangle(e.CellBounds.X + 48, e.CellBounds.Y + (e.CellBounds.Height - 32) / 2, 32, 32);
                
                if (imgEdit != null) e.Graphics.DrawImage(imgEdit, editRect);
                if (imgDel != null) e.Graphics.DrawImage(imgDel, delRect);
            }
        }

        private void DgvRecipes_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvRecipes.Columns[e.ColumnIndex].Name == "colActions")
            {
                if (e.X >= 10 && e.X <= 40) // Edit
                {
                    int recipeId = (int)dgvRecipes.Rows[e.RowIndex].Cells["id"].Value;
                    using (var form = new RecipeEditorForm(recipeId))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            LoadData();
                        }
                    }
                }
                else if (e.X >= 50 && e.X <= 80) // Delete
                {
                    if (MessageHelper.ConfirmAction("Are you sure you want to delete this recipe? Components will be returned to inventory."))
                    {
                        int id = (int)dgvRecipes.Rows[e.RowIndex].Cells["id"].Value;
                        RecipeData.DeleteRecipe(id);
                        LoadData();
                    }
                }
            }
        }

        private void DgvRecipes_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvRecipes.Cursor = (e.RowIndex >= 0 && dgvRecipes.Columns[e.ColumnIndex].Name == "colActions") ? Cursors.Hand : Cursors.Default;
        }

        private void DgvRecipes_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dgvRecipes.Cursor = Cursors.Default;
        }
    }
}
