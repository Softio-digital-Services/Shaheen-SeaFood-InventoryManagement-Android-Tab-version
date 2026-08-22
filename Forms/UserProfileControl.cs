using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using Shaheen_InventoryManagement_Android.Helpers;

namespace Shaheen_InventoryManagement_Android.Forms
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class UserProfileControl : UserControl
    {
        private Panel pnlHeader;
        private Button btnBack;
        private Label lblTitle;

        private TableLayoutPanel layoutMain;
        
        // Left Profile Card Panel
        private Panel pnlProfileCard;
        private Panel pnlAvatar;
        private Label lblFullName;
        private Label lblUsername;
        private Label lblRole;
        private Label lblStatusLabel;
        private Panel pnlStatusBadge;
        private Label lblStatusText;
        private Button btnSwitchUser;
        private Button btnLogout;

        // Right Activity Log Panel
        private Panel pnlActivityLog;
        private Label lblLogTitle;
        private Panel pnlFilterArea;
        private Label lblFilterUser;
        private ComboBox cbUserFilter;
        private DataGridView dgvLogs;

        public UserProfileControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = ThemeConfig.BackgroundColor;
        }

        private void InitializeComponent()
        {
            // 1. Header Panel
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ThemeConfig.SurfaceColor,
                Padding = new Padding(15, 10, 15, 10)
            };
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeConfig.BorderColor, 1))
                {
                    e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
                }
            };

            btnBack = new Button
            {
                Text = "  ←  " + LocalizationManager.GetString("UserProfile_Back", "Back"),
                Font = ThemeConfig.SubHeaderFont,
                ForeColor = ThemeConfig.PrimaryColor,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 36),
                Location = new Point(15, 12),
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 1;
            btnBack.FlatAppearance.BorderColor = ThemeConfig.BorderColor;
            btnBack.Click += BtnBack_Click;

            lblTitle = new Label
            {
                Text = LocalizationManager.GetString("UserProfile_Title", "User Profile & Sessions"),
                Font = ThemeConfig.HeaderFont,
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = true,
                Location = new Point(140, 18)
            };

            pnlHeader.Controls.Add(btnBack);
            pnlHeader.Controls.Add(lblTitle);

            // 2. Main Content Split Layout
            layoutMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(20),
                BackColor = Color.Transparent
            };
            layoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F)); // Left info card
            layoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Right activity logs

            // 3. Left Side: Profile Card
            pnlProfileCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeConfig.SurfaceColor,
                Padding = new Padding(25),
                Margin = new Padding(0, 0, 15, 0)
            };
            pnlProfileCard.Paint += (s, e) => DrawRoundedCardBorder(pnlProfileCard, e.Graphics);

            // Circle Avatar panel with Custom Paint
            pnlAvatar = new Panel
            {
                Size = new Size(100, 100),
                Location = new Point(110, 35),
                BackColor = Color.Transparent
            };
            pnlAvatar.Paint += PnlAvatar_Paint;

            lblFullName = new Label
            {
                Font = new Font(ThemeConfig.AppFontFamily, 14F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(270, 30),
                Location = new Point(25, 145),
                AutoEllipsis = true
            };

            lblUsername = new Label
            {
                Font = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.SecondaryColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(270, 20),
                Location = new Point(25, 175)
            };

            lblRole = new Label
            {
                Font = ThemeConfig.SubHeaderFont,
                ForeColor = ThemeConfig.PrimaryColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(270, 22),
                Location = new Point(25, 200)
            };

            lblStatusLabel = new Label
            {
                Text = LocalizationManager.GetString("UserProfile_Status", "Status:"),
                Font = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.SecondaryColor,
                Location = new Point(95, 235),
                Size = new Size(50, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            pnlStatusBadge = new Panel
            {
                Size = new Size(80, 24),
                Location = new Point(150, 233),
                BackColor = ThemeConfig.SuccessBadgeBg
            };
            pnlStatusBadge.Paint += PnlStatusBadge_Paint;

            lblStatusText = new Label
            {
                Text = "Active",
                Font = ThemeConfig.MicroBoldFont,
                ForeColor = ThemeConfig.SuccessBadgeText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            pnlStatusBadge.Controls.Add(lblStatusText);

            // Buttons
            btnSwitchUser = new Button
            {
                Text = "🔄  " + LocalizationManager.GetString("UserProfile_SwitchUser", "Switch User"),
                Font = ThemeConfig.ButtonFont,
                ForeColor = ThemeConfig.PrimaryColor,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(270, 44),
                Location = new Point(25, 310),
                Cursor = Cursors.Hand
            };
            btnSwitchUser.FlatAppearance.BorderSize = 1;
            btnSwitchUser.FlatAppearance.BorderColor = ThemeConfig.BorderColor;
            btnSwitchUser.Click += BtnSwitchUser_Click;

            btnLogout = new Button
            {
                Text = "🚪  " + LocalizationManager.GetString("UserProfile_Logout", "Logout"),
                Font = ThemeConfig.ButtonFont,
                ForeColor = Color.White,
                BackColor = ThemeConfig.DangerColor,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(270, 44),
                Location = new Point(25, 365),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnLogout_Click;

            pnlProfileCard.Controls.AddRange(new Control[] {
                pnlAvatar, lblFullName, lblUsername, lblRole, 
                lblStatusLabel, pnlStatusBadge, btnSwitchUser, btnLogout
            });

            // 4. Right Side: Activity Log
            pnlActivityLog = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeConfig.SurfaceColor,
                Padding = new Padding(25),
                Margin = new Padding(0)
            };
            pnlActivityLog.Paint += (s, e) => DrawRoundedCardBorder(pnlActivityLog, e.Graphics);

            lblLogTitle = new Label
            {
                Text = LocalizationManager.GetString("UserProfile_ActivityLog", "Activity Log"),
                Font = ThemeConfig.CardTitleFont,
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = true,
                Location = new Point(25, 25)
            };

            pnlFilterArea = new Panel
            {
                Size = new Size(350, 40),
                Location = new Point(25, 55),
                BackColor = Color.Transparent
            };

            lblFilterUser = new Label
            {
                Text = LocalizationManager.GetString("UserProfile_FilterUser", "User:"),
                Font = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.SecondaryColor,
                Location = new Point(0, 10),
                Size = new Size(45, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            cbUserFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = ThemeConfig.StandardFont,
                Size = new Size(220, 26),
                Location = new Point(50, 7)
            };
            cbUserFilter.SelectedIndexChanged += CbUserFilter_SelectedIndexChanged;

            pnlFilterArea.Controls.AddRange(new Control[] { lblFilterUser, cbUserFilter });

            dgvLogs = new DataGridView
            {
                Dock = DockStyle.None,
                Location = new Point(25, 110),
                BackgroundColor = ThemeConfig.SurfaceColor,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = ThemeConfig.BorderColor,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ColumnHeadersHeight = 38,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            dgvLogs.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ThemeConfig.GridHeaderBgColor,
                ForeColor = ThemeConfig.TextColorDark,
                Font = ThemeConfig.SubHeaderFont,
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };
            dgvLogs.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ThemeConfig.SurfaceColor,
                ForeColor = ThemeConfig.TextColorDark,
                Font = ThemeConfig.StandardFont,
                SelectionBackColor = ThemeConfig.SelectionBackColor,
                SelectionForeColor = ThemeConfig.TextColorDark
            };
            dgvLogs.EnableHeadersVisualStyles = false;
            pnlActivityLog.SizeChanged += PnlActivityLog_SizeChanged;

            pnlActivityLog.Controls.AddRange(new Control[] { lblLogTitle, pnlFilterArea, dgvLogs });

            layoutMain.Controls.Add(pnlProfileCard, 0, 0);
            layoutMain.Controls.Add(pnlActivityLog, 1, 0);

            this.Controls.Add(layoutMain);
            this.Controls.Add(pnlHeader);
        }

        private void DrawRoundedCardBorder(Panel panel, Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = new GraphicsPath())
            {
                int r = 12; // corner radius
                path.AddArc(0, 0, r, r, 180, 90);
                path.AddArc(panel.Width - r - 1, 0, r, r, 270, 90);
                path.AddArc(panel.Width - r - 1, panel.Height - r - 1, r, r, 0, 90);
                path.AddArc(0, panel.Height - r - 1, r, r, 90, 90);
                path.CloseAllFigures();

                using (var pen = new Pen(ThemeConfig.BorderColor, 1))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private void PnlAvatar_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, pnlAvatar.Width - 1, pnlAvatar.Height - 1);
            
            // Draw gradient circle background
            using (var brush = new LinearGradientBrush(rect, ThemeConfig.GradientStart, ThemeConfig.GradientEnd, 45F))
            {
                e.Graphics.FillEllipse(brush, rect);
            }

            // Draw white border ring
            using (var pen = new Pen(Color.White, 3))
            {
                e.Graphics.DrawEllipse(pen, rect);
            }

            // Draw user initials in center
            string initial = string.IsNullOrEmpty(UserSession.FullName) ? "U" : UserSession.FullName.Trim().Substring(0, 1).ToUpper();
            using (var font = new Font(ThemeConfig.AppFontFamily, 28F, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                var size = e.Graphics.MeasureString(initial, font);
                e.Graphics.DrawString(initial, font, brush, (pnlAvatar.Width - size.Width) / 2, (pnlAvatar.Height - size.Height) / 2 + 1);
            }
        }

        private void PnlStatusBadge_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, pnlStatusBadge.Width - 1, pnlStatusBadge.Height - 1);
            int r = 8; // rounded corners
            using (var path = new GraphicsPath())
            {
                path.AddArc(0, 0, r, r, 180, 90);
                path.AddArc(pnlStatusBadge.Width - r - 1, 0, r, r, 270, 90);
                path.AddArc(pnlStatusBadge.Width - r - 1, pnlStatusBadge.Height - r - 1, r, r, 0, 90);
                path.AddArc(0, pnlStatusBadge.Height - r - 1, r, r, 90, 90);
                path.CloseAllFigures();

                using (var brush = new SolidBrush(pnlStatusBadge.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }
        }

        private void PnlActivityLog_SizeChanged(object sender, EventArgs e)
        {
            dgvLogs.Width = pnlActivityLog.Width - 50;
            dgvLogs.Height = pnlActivityLog.Height - 140;
        }

        public void LoadData()
        {
            // Set User Info
            lblFullName.Text = UserSession.FullName ?? "Guest User";
            lblUsername.Text = "@" + (UserSession.Username ?? "guest");
            lblRole.Text = UserSession.Role ?? "User";

            // Query active status from DB
            int isActive = 1;
            try
            {
                object result = DatabaseHelper.ExecuteScalar<object>(
                    "SELECT is_active FROM users WHERE username = @u",
                    new SqliteParameter("@u", UserSession.Username));
                if (result != null && result != DBNull.Value)
                {
                    isActive = Convert.ToInt32(result);
                }
            }
            catch { }

            if (isActive == 1)
            {
                pnlStatusBadge.BackColor = ThemeConfig.SuccessBadgeBg;
                lblStatusText.ForeColor = ThemeConfig.SuccessBadgeText;
                lblStatusText.Text = LocalizationManager.GetString("UserProfile_Active", "Active");
            }
            else
            {
                pnlStatusBadge.BackColor = ThemeConfig.DangerBadgeBg;
                lblStatusText.ForeColor = ThemeConfig.DangerBadgeText;
                lblStatusText.Text = LocalizationManager.GetString("UserProfile_Inactive", "Inactive");
            }

            pnlAvatar.Invalidate();

            // Populate filters and logs based on permission
            if (UserSession.IsAdmin)
            {
                pnlFilterArea.Visible = true;
                PopulateUserFilter();
            }
            else
            {
                pnlFilterArea.Visible = false;
            }

            LoadLogs();
        }

        private void PopulateUserFilter()
        {
            try
            {
                cbUserFilter.SelectedIndexChanged -= CbUserFilter_SelectedIndexChanged;
                
                string currentSelection = cbUserFilter.SelectedItem?.ToString();
                cbUserFilter.Items.Clear();
                cbUserFilter.Items.Add(LocalizationManager.GetString("UserProfile_AllUsers", "All Users"));

                string sql = "SELECT username FROM users UNION SELECT username FROM user_logs ORDER BY username";
                using (var dt = DatabaseHelper.ExecuteDataTable(sql))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        cbUserFilter.Items.Add(row["username"].ToString());
                    }
                }

                if (!string.IsNullOrEmpty(currentSelection) && cbUserFilter.Items.Contains(currentSelection))
                    cbUserFilter.SelectedItem = currentSelection;
                else
                    cbUserFilter.SelectedIndex = 0;

                cbUserFilter.SelectedIndexChanged += CbUserFilter_SelectedIndexChanged;
            }
            catch { }
        }

        private void LoadLogs()
        {
            try
            {
                string sql = "";
                SqliteParameter[] parameters = new SqliteParameter[0];

                if (UserSession.IsAdmin)
                {
                    string selectedUser = cbUserFilter.SelectedItem?.ToString();
                    string allUsersText = LocalizationManager.GetString("UserProfile_AllUsers", "All Users");

                    if (string.IsNullOrEmpty(selectedUser) || selectedUser == allUsersText)
                    {
                        sql = "SELECT timestamp, username, action FROM user_logs ORDER BY id DESC LIMIT 200";
                    }
                    else
                    {
                        sql = "SELECT timestamp, username, action FROM user_logs WHERE username = @u ORDER BY id DESC LIMIT 200";
                        parameters = new SqliteParameter[] { new SqliteParameter("@u", selectedUser) };
                    }
                }
                else
                {
                    sql = "SELECT timestamp, username, action FROM user_logs WHERE username = @u ORDER BY id DESC LIMIT 200";
                    parameters = new SqliteParameter[] { new SqliteParameter("@u", UserSession.Username) };
                }

                DataTable dtBind = new DataTable();
                dtBind.Columns.Add("Date");
                dtBind.Columns.Add("Time");
                if (UserSession.IsAdmin)
                {
                    dtBind.Columns.Add("User");
                }
                dtBind.Columns.Add("Activity");

                using (var dtRaw = DatabaseHelper.ExecuteDataTable(sql, parameters))
                {
                    foreach (DataRow row in dtRaw.Rows)
                    {
                        string tsStr = row["timestamp"].ToString();
                        string dateStr = "N/A";
                        string timeStr = "N/A";

                        if (DateTime.TryParse(tsStr, out DateTime dtParsed))
                        {
                            var localTime = dtParsed.ToLocalTime();
                            dateStr = localTime.ToString("yyyy-MM-dd");
                            timeStr = localTime.ToString("HH:mm:ss");
                        }

                        if (UserSession.IsAdmin)
                        {
                            dtBind.Rows.Add(dateStr, timeStr, row["username"].ToString(), row["action"].ToString());
                        }
                        else
                        {
                            dtBind.Rows.Add(dateStr, timeStr, row["action"].ToString());
                        }
                    }
                }

                dgvLogs.DataSource = dtBind;

                // Adjust column headers
                dgvLogs.Columns["Date"].Width = 100;
                dgvLogs.Columns["Time"].Width = 100;
                if (UserSession.IsAdmin)
                {
                    dgvLogs.Columns["User"].Width = 130;
                }
                dgvLogs.Columns[dgvLogs.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch { }
        }

        private void CbUserFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadLogs();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            var mainForm = this.FindForm() as MainForm;
            if (mainForm != null)
            {
                mainForm.NavigateBack();
            }
        }

        private void BtnSwitchUser_Click(object sender, EventArgs e)
        {
            var mainForm = this.FindForm() as MainForm;
            if (mainForm != null)
            {
                mainForm.PerformLogout(true);
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            var mainForm = this.FindForm() as MainForm;
            if (mainForm != null)
            {
                if (MessageHelper.ConfirmAction(LocalizationManager.GetString("Msg_ConfirmLogout", "Logout?")))
                {
                    mainForm.PerformLogout(false);
                }
            }
        }
    }
}
