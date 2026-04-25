using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Data;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    public partial class UsersForm : UserControl
    {
        private DataGridView dgvUsers;
        private Button btnAddUser;
        private Label lblUsersTitle;

        public UsersForm()
        {
            InitializeComponent();
            ApplyTheme();
            LoadData();
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Size = new Size(1100, 750);
            this.BackColor = ThemeConfig.BackgroundColor;

            // Main Layout
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 1;
            mainLayout.RowCount = 2; // Header, Content
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F)); // Header height
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Grid
            mainLayout.BackColor = ThemeConfig.BackgroundColor;
            this.Controls.Add(mainLayout);

            // 1. Header with Title and Add Button
            Panel pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Fill;
            pnlHeader.BackColor = ThemeConfig.BackgroundColor;
            pnlHeader.Padding = new Padding(20);
            pnlHeader.Margin = new Padding(0);

            lblUsersTitle = ThemeConfig.CreateStandardHeader("User Management");
            lblUsersTitle.Name = "lblUsersTitle";
            pnlHeader.Controls.Add(lblUsersTitle);

            btnAddUser = new ModernButton();
            btnAddUser.Text = "+ Add User";
            btnAddUser.Size = new Size(150, 40);
            ThemeConfig.ApplyPrimaryButton(btnAddUser);
            
            // Positioning button to match standard pattern
            btnAddUser.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddUser.Location = new Point(pnlHeader.Width - 170, 70); 
            btnAddUser.Click += btnAddUser_Click;
            pnlHeader.Controls.Add(btnAddUser);
            
            // Handle responsive positioning for the button
            pnlHeader.Resize += (s, e) => {
                if (LocalizationManager.IsArabic)
                    btnAddUser.Location = new Point(20, 70);
                else
                    btnAddUser.Location = new Point(pnlHeader.Width - btnAddUser.Width - 20, 70);
            };
            
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // 2. Grid Container (Card)
            Panel pnlGridCard = CreateCardPanel();
            pnlGridCard.Dock = DockStyle.Fill;
            pnlGridCard.Margin = new Padding(20, 10, 20, 20); // Standard margin
            
            dgvUsers = new DataGridView();
            dgvUsers.DataError += (s, e) => { e.ThrowException = false; };
            dgvUsers.Dock = DockStyle.Fill;
            dgvUsers.BackgroundColor = ThemeConfig.SurfaceColor;
            dgvUsers.BorderStyle = BorderStyle.None;
            dgvUsers.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            dgvUsers.EnableHeadersVisualStyles = false;
            dgvUsers.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvUsers.RowHeadersVisible = false;
            dgvUsers.AutoGenerateColumns = false;
            dgvUsers.AllowUserToAddRows = false;
            dgvUsers.ReadOnly = true;
            dgvUsers.CellClick += DgvUsers_CellClick;
            dgvUsers.CellMouseMove += DgvUsers_CellMouseMove;
            dgvUsers.CellMouseLeave += DgvUsers_CellMouseLeave;
            dgvUsers.CellPainting += DgvUsers_CellPainting;

            // Columns
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "id", HeaderText = "ID", DataPropertyName = "id", Width = 80 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "username", HeaderText = "Username", DataPropertyName = "username", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            
            // Actions Column (Edit + Delete)
            var btnCol = new DataGridViewButtonColumn 
            { 
                Name = "actions", 
                HeaderText = "Actions", 
                Text = "", 
                UseColumnTextForButtonValue = true, 
                Width = 140,
                FlatStyle = FlatStyle.Flat 
            };
            dgvUsers.Columns.Add(btnCol);

            pnlGridCard.Controls.Add(dgvUsers);
            mainLayout.Controls.Add(pnlGridCard, 0, 1);

            this.ResumeLayout(false);
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeConfig.BackgroundColor;
            ThemeConfig.ApplyGridTheme(dgvUsers);
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            lblUsersTitle.Text = isArabic ? "إدارة المستخدمين" : "User Management";
            btnAddUser.Text = isArabic ? "+ إضافة مستخدم" : "+ Add User";

            if (dgvUsers.Columns["id"] != null)
                dgvUsers.Columns["id"].HeaderText = isArabic ? "المعرف" : "ID";
            if (dgvUsers.Columns["username"] != null)
                dgvUsers.Columns["username"].HeaderText = isArabic ? "اسم المستخدم" : "Username";
            if (dgvUsers.Columns["actions"] != null)
                dgvUsers.Columns["actions"].HeaderText = isArabic ? "الإجراءات" : "Actions";
        }

        public void LoadData()
        {
            try
            {
                string sql = "SELECT id, username FROM users";
                DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
                dgvUsers.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(LocalizationManager.GetString("User_LoadError") + ": " + ex.Message);
            }
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            AddUserForm form = new AddUserForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void PerformEdit(int rowIndex)
        {
            int userId = Convert.ToInt32(dgvUsers.Rows[rowIndex].Cells["id"].Value);
            string username = dgvUsers.Rows[rowIndex].Cells["username"].Value.ToString();

            if (username.ToLower() == "admin")
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("User_AdminEditBlock"));
                return;
            }

            AddUserForm form = new AddUserForm(userId);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void DgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            
            if (dgvUsers.Columns[e.ColumnIndex].Name == "actions")
            {
                var id = dgvUsers.Rows[e.RowIndex].Cells["id"].Value;
                var username = dgvUsers.Rows[e.RowIndex].Cells["username"].Value.ToString();

                var mousePos = dgvUsers.PointToClient(Cursor.Position);
                var cellRect = dgvUsers.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                int relativeX = mousePos.X - cellRect.X;

                // Edit Click (match fixed offsets: 5 to 37)
                if (relativeX >= 5 && relativeX <= 37)
                {
                    PerformEdit(e.RowIndex);
                }
                // Delete Click (match fixed offsets: 45 to 77)
                else if (relativeX >= 45 && relativeX <= 77)
                {
                    if (username.ToLower() == "admin")
                    {
                        MessageHelper.ShowWarning(LocalizationManager.GetString("User_AdminDeleteBlock"));
                        return;
                    }

                    string confirmMsg = string.Format(LocalizationManager.GetString("User_DeleteConfirm"), username);

                    if (MessageHelper.ConfirmAction(confirmMsg))
                    {
                        try
                        {
                            DatabaseHelper.ExecuteNonQuery($"DELETE FROM users WHERE id = {id}");
                            LoadData();
                        }
                        catch (Exception ex)
                        {
                            MessageHelper.ShowError(LocalizationManager.GetString("User_DeleteError") + ": " + ex.Message);
                        }
                    }
                }
            }
        }
        
        private void DgvUsers_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
             if (e.RowIndex >= 0 && dgvUsers.Columns[e.ColumnIndex].Name == "actions")
             {
                 dgvUsers.Cursor = Cursors.Hand;
             }
             else
             {
                 dgvUsers.Cursor = Cursors.Default;
             }
        }

        private void DgvUsers_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
             dgvUsers.Cursor = Cursors.Default;
        }
        
        private void DgvUsers_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (dgvUsers.Columns[e.ColumnIndex].Name == "actions")
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                // Edit Icon
                Rectangle editRect = new Rectangle(e.CellBounds.X + 5, e.CellBounds.Y + 14, 32, 32);
                DrawNuriconButton(e.Graphics, editRect, ThemeConfig.GetNuricon("edit"));

                // Delete Icon
                Rectangle delRect = new Rectangle(e.CellBounds.X + 45, e.CellBounds.Y + 14, 32, 32);
                DrawNuriconButton(e.Graphics, delRect, ThemeConfig.GetNuricon("delete"));
            }
        }

        private void DrawNuriconButton(Graphics g, Rectangle rect, Image icon)
        {
            using (var path = GetRoundedRect(rect, 8))
            using (var pen = new Pen(ThemeConfig.BorderColor, 1))
            using (var brush = new SolidBrush(ThemeConfig.SurfaceColor))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
            if (icon != null) g.DrawImage(icon, new Rectangle(rect.X + 6, rect.Y + 6, 20, 20));
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Panel CreateCardPanel()
        {
            Panel p = new Panel();
            p.BackColor = ThemeConfig.SurfaceColor;
            p.Padding = new Padding(20);
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                
                using (var path = GetRoundedRect(r, 12))
                using (var pen = new Pen(ThemeConfig.BorderColor, 1))
                {
                    using(var brush = new SolidBrush(ThemeConfig.SurfaceColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                    e.Graphics.DrawPath(pen, path);
                }
            };
            return p;
        }
    }
}
