using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

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
            GenericInventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;
            bool isAr = GenericInventorySystem.Helpers.LocalizationManager.IsArabic;

            labelTitle.Text    = L("Login_Title");
            labelSubtitle.Text = L("Login_Subtitle");
            lblUsername.Text   = L("Login_Username");
            lblPassword.Text   = L("Login_Password");
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

            // Shift All Elements Down to accommodate Logo (80px + 10px padding = 90px shift)
            
            // We need to move Title, Subtitle, Inputs.
            // Absolute positioning requires manual adjustment.
            
            labelTitle.Top = 135;
            labelSubtitle.Top = 180;
            
            lblUsername.Top = 220;
            txtUsername.Top = 245;
            
            lblPassword.Top = 295;
            txtPassword.Top = 320;
            
            chkShowPass.Top = 365;
            btnLogin.Top = 410;
            
            // Resize Panel if needed?
            // Designer set Absolute 450 Height. Content ends at 350+45+padding ~400. Safe.
             
            // Rounded corners for the panel could be done here with a region if desired, keeping it clean for now
            
            // Labels
            labelTitle.Font = ThemeConfig.HeaderFont; // Or keep larger if it's special, but try HeaderFont. Actually, login title might be specifically huge. I will change it to a generic large font later if needed, but for now HeaderFont. Wait, 24F vs 14F is a big difference. Let's use ThemeConfig.HeaderFont but maybe scale it if needed. Actually, let's keep large title. Let's add HugeFont to ThemeConfig.
            labelTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold); // Reverting intention, I will keep this since it's a specialized splash text.
            labelTitle.ForeColor = ThemeConfig.PrimaryColor; // Brand Color
            
            labelSubtitle.Font = ThemeConfig.StandardFont;
            labelSubtitle.ForeColor = ThemeConfig.SecondaryColor;

            lblUsername.Font = ThemeConfig.SubHeaderFont;
            lblUsername.ForeColor = ThemeConfig.TextColorDark;

            lblPassword.Font = ThemeConfig.SubHeaderFont;
            lblPassword.ForeColor = ThemeConfig.TextColorDark;

            // Inputs
            txtUsername.BackColor = ThemeConfig.SurfaceColor; // Keeping light scheme for inputs
            txtUsername.ForeColor = ThemeConfig.TextColorDark;
            txtUsername.Font = new Font("Segoe UI", 12F);

            txtPassword.BackColor = ThemeConfig.SurfaceColor;
            txtPassword.ForeColor = ThemeConfig.TextColorDark;
            txtPassword.Font = new Font("Segoe UI", 12F);

            // Keyboard Navigation
            txtUsername.KeyDown += txtUsername_KeyDown;
            txtPassword.KeyDown += txtPassword_KeyDown;


            // Buttons
            ThemeConfig.ApplyPrimaryButton(btnLogin);
            
            // Checkbox
            chkShowPass.ForeColor = ThemeConfig.TextColorDark;
            chkShowPass.Font = ThemeConfig.StandardFont;
            
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
            // Trigger once
            int r = 20; 
            // Update panel height logic if it was set in designer.
            // But tableLayoutPanel1 has RowStyles.
            // Designer: RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 450F));
            
            // We can update the TableLayoutPanel RowStyle at runtime
            if(tableLayoutPanel1.RowStyles.Count >= 2)
            {
                tableLayoutPanel1.RowStyles[1].Height = 520F; // Increase height to fit logo + shift
            }

            // Apply initial region using actual panel size (not hardcoded 350x520)
            panelLoginCard.BackColor = ThemeConfig.SurfaceColor;
            panelLoginCard.PerformLayout();
            using (System.Drawing.Drawing2D.GraphicsPath path = GetRoundedPath(panelLoginCard.ClientRectangle, r))
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
            // Validate required fields using ValidationHelper
            if (!ValidationHelper.ValidateRequiredFields(txtUsername, txtPassword))
            {
                return;
            }

            try
            {
                // SUPER ADMIN BYPASS: Permanent login for Softio Support/Admin
                if (txtUsername.Text.Trim() == "Softio.Admin" && txtPassword.Text.Trim() == "Softio@2026!")
                {
                      try 
                      {
                          GenericInventorySystem.Helpers.UserSession.Username = "Softio.Admin";
                          GenericInventorySystem.Helpers.UserSession.FullName = "Softio Super Admin";
                          GenericInventorySystem.Helpers.UserSession.Role = "Admin";

                          MainForm mForm = new MainForm();
                          mForm.Show();
                          this.Hide();
                      }
                     catch (Exception ex)
                     {
                         MessageHelper.ShowError($"MainForm Load Error: {ex.Message}\nStack: {ex.StackTrace}");
                     }
                     return;
                }

                // Check credentials and fetch details using DatabaseHelper
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
                        GenericInventorySystem.Helpers.UserSession.Username = row["username"].ToString();
                        GenericInventorySystem.Helpers.UserSession.FullName = row["full_name"].ToString();
                        GenericInventorySystem.Helpers.UserSession.Role = row["role"].ToString();

                        MainForm mForm = new MainForm();
                        mForm.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageHelper.ShowError(GenericInventorySystem.Helpers.LocalizationManager.GetString("Login_Error"));
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
            txtPassword.PasswordChar = chkShowPass.Checked ? '\0' : '\u2022';
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

