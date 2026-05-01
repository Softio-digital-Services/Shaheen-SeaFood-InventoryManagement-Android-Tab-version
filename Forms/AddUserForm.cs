using System;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Data;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    public partial class AddUserForm : BaseModalForm
    {
        private ModernTextBox txtUsername;
        private ModernTextBox txtPassword;
        private ModernTextBox txtConfirmPassword;
        private ModernTextBox txtFullName;
        private ComboBox cmbRole;
        private Label lblSection;
        private Label lblRole;
        
        private int? _userId = null; // Null = Add mode, Value = Edit mode

        public AddUserForm(int? userId = null)
        {
            _userId = userId;
            InitializeComponent();
            ApplyTheme();
            
            if (_userId.HasValue)
            {
                LoadUserData(_userId.Value);
            }
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(500, 700);
            this.TitleText = _userId.HasValue ? "Edit User" : "Add New User";

            this.txtUsername = new ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
            this.txtPassword = new ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10), UseSystemPasswordChar = true };
            this.txtConfirmPassword = new ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10), UseSystemPasswordChar = true };
            this.txtFullName = new ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
            this.cmbRole = new ComboBox();
            this.lblSection = new Label { Name = "lblSection", AutoSize = true, Margin = new Padding(0, 0, 0, 15) };
            this.lblRole = new Label { Name = "lblRole", AutoSize = true, Margin = new Padding(0, 5, 0, 5) };

            this.SuspendLayout();

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 8,
                Padding = new Padding(25),
                AutoSize = true
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 8; i++) tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Section Title
            lblSection.Text = "Credentials";
            lblSection.Font = ThemeConfig.SubHeaderFont;
            lblSection.ForeColor = ThemeConfig.SecondaryColor;
            tlpMain.Controls.Add(lblSection, 0, 0);

            // Username
            txtUsername.LabelText = "Username";
            txtUsername.IsRequired = true;
            tlpMain.Controls.Add(txtUsername, 0, 1);

            // Full Name
            txtFullName.LabelText = "Full Name";
            tlpMain.Controls.Add(txtFullName, 0, 2);

            // Password
            txtPassword.LabelText = _userId.HasValue ? "Password (leave blank to keep current)" : "Password";
            txtPassword.IsRequired = !_userId.HasValue;
            tlpMain.Controls.Add(txtPassword, 0, 3);

            // Confirm
            txtConfirmPassword.LabelText = "Confirm Password";
            tlpMain.Controls.Add(txtConfirmPassword, 0, 4);

            // Role
            lblRole.Text = "Role";
            lblRole.Font = ThemeConfig.StandardFont;
            lblRole.ForeColor = ThemeConfig.SecondaryColor;
            tlpMain.Controls.Add(lblRole, 0, 5);

            ThemeConfig.ApplyComboBoxStyle(cmbRole);
            cmbRole.Items.AddRange(new object[] { "Admin", "Staff", "Accountant" });
            cmbRole.SelectedIndex = 1;

            Panel pnlRole = ThemeConfig.WrapInStyledInput(cmbRole, 42);
            pnlRole.Dock = DockStyle.Fill;
            pnlRole.Margin = new Padding(0, 0, 0, 20);
            tlpMain.Controls.Add(pnlRole, 0, 6);

            SetFooterButtons(
                _userId.HasValue ? "Update User" : "Save User",
                "Cancel",
                btnSave_Click,
                (s, e) => this.Close()
            );

            this.ContentPanel.Controls.Add(tlpMain);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ApplyTheme()
        {
            // Background handled by BaseModalForm (White)
            // Footer buttons are styled automatically by SetFooterButtons
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            this.TitleText = _userId.HasValue ? LocalizationManager.GetString("AddUser_TitleEdit") : LocalizationManager.GetString("AddUser_TitleNew");
            lblSection.Text = LocalizationManager.GetString("AddUser_Section");

            txtUsername.LabelText = LocalizationManager.GetString("AddUser_Username");
            txtFullName.LabelText = LocalizationManager.GetString("AddUser_FullName");
            txtPassword.LabelText = _userId.HasValue ? LocalizationManager.GetString("AddUser_PassEdit") : LocalizationManager.GetString("AddUser_PassNew");
            txtUsername.IsRequired = true;
            txtPassword.IsRequired = !_userId.HasValue;
            txtConfirmPassword.LabelText = LocalizationManager.GetString("AddUser_ConfirmPass");

            lblRole.Text = LocalizationManager.GetString("AddUser_Role");

            SetFooterButtons(
                _userId.HasValue ? LocalizationManager.GetString("AddUser_Update") : LocalizationManager.GetString("AddUser_Save"),
                LocalizationManager.GetString("Popup_Cancel"),
                btnSave_Click,
                (s, e) => this.Close()
            );

            string currentRole = cmbRole.SelectedItem?.ToString();
            cmbRole.Items.Clear();
            if (isArabic)
            {
                cmbRole.Items.AddRange(new object[] { LocalizationManager.GetString("Role_Admin"), LocalizationManager.GetString("Role_Staff"), LocalizationManager.GetString("Role_Accountant") });
                if (currentRole == "Admin" || currentRole == LocalizationManager.GetString("Role_Admin")) cmbRole.SelectedIndex = 0;
                else if (currentRole == "Accountant" || currentRole == LocalizationManager.GetString("Role_Accountant")) cmbRole.SelectedIndex = 2;
                else cmbRole.SelectedIndex = 1;
            }
            else
            {
                cmbRole.Items.AddRange(new object[] { "Admin", "Staff", "Accountant" });
                if (currentRole == LocalizationManager.GetString("Role_Admin") || currentRole == "Admin") cmbRole.SelectedIndex = 0;
                else if (currentRole == LocalizationManager.GetString("Role_Accountant") || currentRole == "Account accountant") cmbRole.SelectedIndex = 2;
                else cmbRole.SelectedIndex = 1;
            }
        }

        private void LoadUserData(int userId)
        {
            try
            {
                string sql = $"SELECT username, full_name, role FROM users WHERE id = {userId}";
                DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
                
                if (dt.Rows.Count > 0)
                {
                    txtUsername.Text = dt.Rows[0]["username"].ToString();
                    txtFullName.Text = dt.Rows[0]["full_name"].ToString();
                    string role = dt.Rows[0]["role"].ToString();
                    
                    int roleIndex = cmbRole.FindStringExact(role);
                    if (roleIndex >= 0) cmbRole.SelectedIndex = roleIndex;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "Loading User Data");
                MessageHelper.ShowError((LocalizationManager.IsArabic ? "Ø®Ø·Ø£ ÙÙŠ ØªØ­Ù…ÙŠÙ„ Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù…: " : "Error loading user: ") + ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();
                string confirm = txtConfirmPassword.Text.Trim();
                string fullName = txtFullName.Text.Trim();
                string role = cmbRole.SelectedItem?.ToString() ?? "User";

                if (!ValidationHelper.ValidateRequiredFields(txtUsername)) return;
                if (!_userId.HasValue && !ValidationHelper.ValidateRequiredFields(txtPassword)) return;

                // Password validation
                if (!_userId.HasValue && string.IsNullOrEmpty(password))
                {
                    string msg = LocalizationManager.IsArabic ? "ÙƒÙ„Ù…Ø© Ø§Ù„Ù…Ø±ÙˆØ± Ù…Ø·Ù„ÙˆØ¨Ø© Ù„Ù„Ù…Ø³ØªØ®Ø¯Ù…ÙŠÙ† Ø§Ù„Ø¬Ø¯Ø¯" : "Password is required for new users.";
                    MessageHelper.ShowWarning(msg);
                    return;
                }

                if (!string.IsNullOrEmpty(password) && password != confirm)
                {
                    string msg = LocalizationManager.IsArabic ? "ÙƒÙ„Ù…Ø§Øª Ø§Ù„Ù…Ø±ÙˆØ± ØºÙŠØ± Ù…ØªØ·Ø§Ø¨Ù‚Ø©" : "Passwords do not match.";
                    MessageHelper.ShowWarning(msg);
                    return;
                }

                if (_userId.HasValue)
                {
                    // Edit mode - UPDATE
                    string sql;
                    if (string.IsNullOrEmpty(password))
                    {
                        // Update without changing password
                        sql = "UPDATE users SET username = @user, full_name = @fullname, role = @role WHERE id = @id";
                        DatabaseHelper.ExecuteNonQuery(sql,
                            new SqliteParameter("@user", username),
                            new SqliteParameter("@fullname", string.IsNullOrEmpty(fullName) ? username : fullName),
                            new SqliteParameter("@role", role),
                            new SqliteParameter("@id", _userId.Value));
                    }
                    else
                    {
                        // Update with new password
                        sql = "UPDATE users SET username = @user, password = @pass, full_name = @fullname, role = @role WHERE id = @id";
                        DatabaseHelper.ExecuteNonQuery(sql,
                            new SqliteParameter("@user", username),
                            new SqliteParameter("@pass", password),
                            new SqliteParameter("@fullname", string.IsNullOrEmpty(fullName) ? username : fullName),
                            new SqliteParameter("@role", role),
                            new SqliteParameter("@id", _userId.Value));
                    }

                    MessageHelper.ShowSuccess(LocalizationManager.IsArabic ? "ØªÙ… ØªØ­Ø¯ÙŠØ« Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… Ø¨Ù†Ø¬Ø§Ø­!" : "User updated successfully!");
                }
                else
                {
                    // Add mode - INSERT
                    if (DatabaseHelper.RecordExists("users", "username", username))
                    {
                        string msg = LocalizationManager.IsArabic ? "Ø§Ø³Ù… Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… Ù…ÙˆØ¬ÙˆØ¯ Ø¨Ø§Ù„ÙØ¹Ù„" : "Username already exists.";
                        MessageHelper.ShowWarning(msg);
                        return;
                    }

                    string sql = "INSERT INTO users (username, password, full_name, role, date_created) VALUES (@user, @pass, @fullname, @role, datetime('now'))";
                    DatabaseHelper.ExecuteNonQuery(sql,
                        new SqliteParameter("@user", username),
                        new SqliteParameter("@pass", password),
                        new SqliteParameter("@fullname", string.IsNullOrEmpty(fullName) ? username : fullName),
                        new SqliteParameter("@role", role));

                    string successMsg = LocalizationManager.IsArabic 
                        ? $"ØªÙ… Ø¥Ø¶Ø§ÙØ© Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… Ø¨Ù†Ø¬Ø§Ø­!\nØ§Ø³Ù… Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù…: '{username}'" 
                        : $"User added successfully!\nUsername: '{username}'";
                    MessageHelper.ShowSuccess(successMsg);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, _userId.HasValue ? "Updating User" : "Adding User");
                MessageHelper.ShowError((LocalizationManager.IsArabic ? "Ø®Ø·Ø£ ÙÙŠ Ø­ÙØ¸ Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù…: " : "Error saving user: ") + ex.Message);
            }
        }
    }
}
