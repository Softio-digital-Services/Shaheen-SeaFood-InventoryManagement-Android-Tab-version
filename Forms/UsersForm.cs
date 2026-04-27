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
        private ModernTextBox txtSearch;

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
            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = ThemeConfig.BackgroundColor, Padding = new Padding(20) };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainLayout);

            // 1. Header
            Panel pnlHeader = new Panel { Dock = DockStyle.Fill, BackColor = ThemeConfig.BackgroundColor, Margin = new Padding(0) };
            lblUsersTitle = ThemeConfig.CreateStandardHeader("User Management");
            pnlHeader.Controls.Add(lblUsersTitle);

            txtSearch = new ModernTextBox { IsSearch = true, ShowLabel = false, PlaceholderText = "Search users...", Size = new Size(320, 40), Location = new Point(0, 55) };
            txtSearch.TextChanged += (s, e) => LoadData(txtSearch.Text);
            pnlHeader.Controls.Add(txtSearch);

            btnAddUser = new Button { Text = "", Size = new Size(160, 40) };
            btnAddUser.FlatStyle = FlatStyle.Flat;
            btnAddUser.FlatAppearance.BorderSize = 0;
            btnAddUser.Cursor = Cursors.Hand;
            btnAddUser.Paint += (s, e) => ThemeConfig.DrawIconButton(btnAddUser, e.Graphics, "add", "User_AddUser", Color.White, ThemeConfig.PrimaryColor, false);
            btnAddUser.Click += btnAddUser_Click;
            pnlHeader.Controls.Add(btnAddUser);
            
            pnlHeader.Resize += (s, e) => {
                if (LocalizationManager.IsArabic) {
                    txtSearch.Location = new Point(pnlHeader.Width - txtSearch.Width, 55);
                    btnAddUser.Location = new Point(0, 50);
                } else {
                    txtSearch.Location = new Point(0, 55);
                    btnAddUser.Location = new Point(pnlHeader.Width - btnAddUser.Width, 50);
                }
            };
            mainLayout.Controls.Add(pnlHeader, 0, 0);

            // 2. Grid
            dgvUsers = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = ThemeConfig.SurfaceColor, BorderStyle = BorderStyle.None, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, RowHeadersVisible = false, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true };
            dgvUsers.DataError += (s, e) => e.ThrowException = false;
            dgvUsers.CellClick += DgvUsers_CellClick;
            dgvUsers.CellMouseMove += DgvUsers_CellMouseMove;
            dgvUsers.CellMouseLeave += DgvUsers_CellMouseLeave;
            dgvUsers.CellPainting += DgvUsers_CellPainting;

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "id", HeaderText = "ID", DataPropertyName = "id", Width = 80 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "username", HeaderText = "Username", DataPropertyName = "username", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvUsers.Columns.Add(new DataGridViewButtonColumn { Name = "actions", HeaderText = "Actions", Text = "", UseColumnTextForButtonValue = true, Width = 140, FlatStyle = FlatStyle.Flat });

            Panel pnlGridCard = ThemeConfig.CreateCardPanel(dgvUsers);
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
            btnAddUser.Invalidate(); // Refresh the painted icon and text
            if (dgvUsers.Columns["id"] != null) dgvUsers.Columns["id"].HeaderText = isArabic ? "المعرف" : "ID";
            if (dgvUsers.Columns["username"] != null) dgvUsers.Columns["username"].HeaderText = isArabic ? "اسم المستخدم" : "Username";
            if (dgvUsers.Columns["actions"] != null) dgvUsers.Columns["actions"].HeaderText = isArabic ? "الإجراءات" : "Actions";
        }

        public void LoadData(string search = "")
        {
            try
            {
                string sql = "SELECT id, username FROM users";
                if (!string.IsNullOrEmpty(search))
                {
                    sql += $" WHERE username LIKE '%{search}%'";
                }
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

            if (username.ToLower() == "softio.admin")
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
                int iconSize = 24;
                int gap = 12;
                int startX = (cellRect.Width - (iconSize * 2 + gap)) / 2;

                // Edit Click (centered range)
                if (relativeX >= startX && relativeX <= startX + iconSize)
                {
                    PerformEdit(e.RowIndex);
                }
                // Delete Click (centered range)
                else if (relativeX >= startX + iconSize + gap && relativeX <= startX + iconSize * 2 + gap)
                {
                    if (username.ToLower() == "softio.admin")
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

                int iconSize = 24;
                int gap = 12;
                int startX = e.CellBounds.X + (e.CellBounds.Width - (iconSize * 2 + gap)) / 2;
                int centerY = e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2;

                // Edit Icon
                Rectangle editRect = new Rectangle(startX, centerY, iconSize, iconSize);
                Image imgEdit = ThemeConfig.GetNuricon("edit");
                if (imgEdit != null) e.Graphics.DrawImage(imgEdit, editRect);

                // Delete Icon
                Rectangle delRect = new Rectangle(startX + iconSize + gap, centerY, iconSize, iconSize);
                Image imgDelete = ThemeConfig.GetNuricon("delete");
                if (imgDelete != null) e.Graphics.DrawImage(imgDelete, delRect);
            }
        }
    }
}
