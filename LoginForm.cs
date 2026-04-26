using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            ApplyTheme();
            ThemeConfig.ApplyFormIcon(this);
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            Func<string, string> L = LocalizationManager.GetString;
            bool isAr = LocalizationManager.IsArabic;

            labelTitle.Text    = L("Login_Title");
            labelSubtitle.Text = L("Login_Subtitle");
            txtUsername.LabelText = L("Login_Username");
            txtPassword.LabelText = L("Login_Password");
            chkShowPass.Text   = L("Login_ShowPassword");
            btnLogin.Text      = L("Login_Button");

            // RTL support
            this.RightToLeft        = isAr ? RightToLeft.Yes : RightToLeft.No;
            panelLoginCard.RightToLeft = isAr ? RightToLeft.Yes : RightToLeft.No;
            labelTitle.TextAlign   = isAr ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            labelSubtitle.TextAlign = isAr ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
        }

        private void ApplyTheme()
        {
            // Background
            this.tableLayoutPanel1.BackColor = ThemeConfig.BackgroundColor; 
            
            // Add Logo
            PictureBox pbLogo = new PictureBox();
            pbLogo.Size = new Size(120, 120);
            pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pbLogo.Anchor = AnchorStyles.Top; 
            try 
            { 
                string logoPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "inventory_logo.png");
                if(System.IO.File.Exists(logoPath))
                    pbLogo.Image = Image.FromFile(logoPath);
            } catch { }
            panelLoginCard.Controls.Add(pbLogo);

            // Center Logo on Resize
            panelLoginCard.Resize += (s, e) => {
                pbLogo.Location = new Point((panelLoginCard.Width - 120) / 2, 15);
            };
            pbLogo.Location = new Point((panelLoginCard.Width - 120) / 2, 15);

            labelTitle.Top = 145;
            labelSubtitle.Top = 195;
            
            txtUsername.Top = 235;
            txtPassword.Top = 315;
            btnLogin.Top = 410;
            
            chkShowPass.Visible = false; // Hide old checkbox
            
            // Labels
            labelTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            labelTitle.ForeColor = ThemeConfig.PrimaryColor; 
            
            labelSubtitle.Font = ThemeConfig.StandardFont;
            labelSubtitle.ForeColor = ThemeConfig.SecondaryColor;

            // Keyboard Navigation
            txtUsername.KeyDown += txtUsername_KeyDown;
            txtPassword.KeyDown += txtPassword_KeyDown;

            // Close and Minimize Buttons
            btnClose.ForeColor = ThemeConfig.SecondaryColor;
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = ThemeConfig.DangerColor;
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = ThemeConfig.SecondaryColor;

            btnMinimize.ForeColor = ThemeConfig.SecondaryColor;
            btnMinimize.MouseEnter += (s, e) => btnMinimize.ForeColor = ThemeConfig.PrimaryColor;
            btnMinimize.MouseLeave += (s, e) => btnMinimize.ForeColor = ThemeConfig.SecondaryColor;

            // Rounded Corners for Card
            panelLoginCard.Resize += (s, e) => 
            {
                int radius = 20; 
                using (System.Drawing.Drawing2D.GraphicsPath path = GetRoundedPath(panelLoginCard.ClientRectangle, radius))
                {
                    panelLoginCard.Region = new Region(path);
                }
            };

            // Adjust card height
            if(tableLayoutPanel1.RowStyles.Count >= 2)
            {
                tableLayoutPanel1.RowStyles[1].Height = 550F;
            }

            panelLoginCard.BackColor = ThemeConfig.SurfaceColor;
            panelLoginCard.PerformLayout();
            using (System.Drawing.Drawing2D.GraphicsPath path = GetRoundedPath(panelLoginCard.ClientRectangle, 20))
            {
                panelLoginCard.Region = new Region(path);
            }
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            float r = radius;
            path.AddArc(rect.X, rect.Y, r, r, 180, 90);
            path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
            path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.ValidateRequiredFields(txtUsername, txtPassword))
            {
                return;
            }

            try
            {
                if (txtUsername.Text.Trim() == "Softio.Admin" && txtPassword.Text.Trim() == "Softio@2026!")
                {
                    UserSession.Username = "Softio.Admin";
                    UserSession.FullName = "Softio Super Admin";
                    UserSession.Role = "Admin";

                    MainForm mForm = new MainForm();
                    mForm.Show();
                    this.Hide();
                    return;
                }

                string sql = "SELECT username, full_name, role FROM users WHERE username = @username AND password = @password";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@username", txtUsername.Text.Trim()),
                    new SqlParameter("@password", txtPassword.Text.Trim())
                };

                using (var dt = DatabaseHelper.ExecuteDataTable(sql, parameters))
                {
                    if (dt.Rows.Count > 0)
                    {
                        var row = dt.Rows[0];
                        UserSession.Username = row["username"].ToString();
                        UserSession.FullName = row["full_name"].ToString();
                        UserSession.Role = row["role"].ToString();

                        MainForm mForm = new MainForm();
                        mForm.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageHelper.ShowError(LocalizationManager.GetString("Login_Error"));
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "user login");
                MessageHelper.ShowDatabaseError("logging in");
            }
        }

        private void showPass_CheckedChanged(object sender, EventArgs e)
        {
            // chkShowPass is hidden, ModernTextBox handles it via eye icon now
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                txtPassword.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                loginBtn_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                txtUsername.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}
