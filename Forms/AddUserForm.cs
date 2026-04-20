using System;
using System.Data;
using System.Data.SqlClient;
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
        private Button btnSave;
        private Button btnCancel;
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
            this.Size = new System.Drawing.Size(450, 600);
            this.TitleText = _userId.HasValue ? "Edit User" : "Add New User";

            this.txtUsername = new ModernTextBox();
            this.txtPassword = new ModernTextBox();
            this.txtConfirmPassword = new ModernTextBox();
            this.txtFullName = new ModernTextBox();
            this.cmbRole = new ComboBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            
            this.lblSection = new Label();

            this.SuspendLayout();

            // 
            // Section Title
            // 
            lblSection.Text = "Credentials";
            lblSection.Font = ThemeConfig.SubHeaderFont;
            lblSection.Location = new System.Drawing.Point(30, 70);
            lblSection.AutoSize = true;
            lblSection.ForeColor = ThemeConfig.SecondaryColor;
            this.Controls.Add(lblSection);

            // 
            // Fields
            // 
            int startY = 110;
            int gap = 85;

            // Username
            txtUsername.LabelText = "Username *";
            txtUsername.Location = new Point(30, startY);
            txtUsername.Width = 360;
            this.Controls.Add(txtUsername);

            // Full Name
            txtFullName.LabelText = "Full Name";
            txtFullName.Location = new Point(30, startY + gap);
            txtFullName.Width = 360;
            this.Controls.Add(txtFullName);

            // Password
            txtPassword.LabelText = _userId.HasValue ? "Password (leave blank to keep current)" : "Password *";
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Location = new Point(30, startY + gap * 2);
            txtPassword.Width = 360;
            this.Controls.Add(txtPassword);

            // Confirm
            txtConfirmPassword.LabelText = "Confirm Password";
            txtConfirmPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.Location = new Point(30, startY + gap * 3);
            txtConfirmPassword.Width = 360;
            this.Controls.Add(txtConfirmPassword);

            // Role
            this.lblRole = new Label();
            lblRole.Text = "Role";
            lblRole.Font = ThemeConfig.StandardFont;
            lblRole.ForeColor = ThemeConfig.SecondaryColor;
            lblRole.Location = new Point(30, startY + gap * 4);
            lblRole.AutoSize = true;
            this.Controls.Add(lblRole);

            ThemeConfig.ApplyComboBoxStyle(cmbRole);
            cmbRole.Items.AddRange(new object[] { "Admin", "Staff", "Accountant" });
            cmbRole.SelectedIndex = 1; // Default to Staff

            Panel pnlRole = ThemeConfig.WrapInStyledInput(cmbRole, 40);
            pnlRole.Location = new Point(30, startY + gap * 4 + 25);
            pnlRole.Width = 360;
            this.Controls.Add(pnlRole);

            // 
            // Buttons
            // 
            btnCancel.Text = "Cancel";
            btnCancel.Size = new System.Drawing.Size(110, 40);
            btnCancel.Location = new System.Drawing.Point(170, 520); 
            btnCancel.Click += (s, e) => { this.Close(); };

            btnSave.Text = _userId.HasValue ? "Update User" : "Save User";
            btnSave.Size = new System.Drawing.Size(130, 40);
            btnSave.Location = new System.Drawing.Point(290, 520);
            btnSave.Click += new EventHandler(btnSave_Click);

            this.Controls.Add(btnSave); 
            this.Controls.Add(btnCancel);
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ApplyTheme()
        {
            // Background handled by BaseModalForm (White)
            ThemeConfig.ApplyPrimaryButton(btnSave);
            ThemeConfig.ApplySecondaryButton(btnCancel);
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
            txtConfirmPassword.LabelText = LocalizationManager.GetString("AddUser_ConfirmPass");

            lblRole.Text = LocalizationManager.GetString("AddUser_Role");

            btnCancel.Text = LocalizationManager.GetString("Popup_Cancel");
            btnSave.Text = _userId.HasValue ? LocalizationManager.GetString("AddUser_Update") : LocalizationManager.GetString("AddUser_Save");

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
                else if (currentRole == LocalizationManager.GetString("Role_Accountant") || currentRole == "Accountant") cmbRole.SelectedIndex = 2;
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
                MessageHelper.ShowError("Error loading user: " + ex.Message);
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

                if (string.IsNullOrEmpty(username))
                {
                    MessageHelper.ShowWarning("Username is required.");
                    return;
                }

                // Password validation
                if (!_userId.HasValue && string.IsNullOrEmpty(password))
                {
                    MessageHelper.ShowWarning("Password is required for new users.");
                    return;
                }

                if (!string.IsNullOrEmpty(password) && password != confirm)
                {
                    MessageHelper.ShowWarning("Passwords do not match.");
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
                            new SqlParameter("@user", username),
                            new SqlParameter("@fullname", string.IsNullOrEmpty(fullName) ? username : fullName),
                            new SqlParameter("@role", role),
                            new SqlParameter("@id", _userId.Value));
                    }
                    else
                    {
                        // Update with new password
                        sql = "UPDATE users SET username = @user, password = @pass, full_name = @fullname, role = @role WHERE id = @id";
                        DatabaseHelper.ExecuteNonQuery(sql,
                            new SqlParameter("@user", username),
                            new SqlParameter("@pass", password),
                            new SqlParameter("@fullname", string.IsNullOrEmpty(fullName) ? username : fullName),
                            new SqlParameter("@role", role),
                            new SqlParameter("@id", _userId.Value));
                    }

                    MessageHelper.ShowSuccess($"User updated successfully!");
                }
                else
                {
                    // Add mode - INSERT
                    if (DatabaseHelper.RecordExists("users", "username", username))
                    {
                        MessageHelper.ShowWarning("Username already exists.");
                        return;
                    }

                    string sql = "INSERT INTO users (username, password, full_name, role, date_created) VALUES (@user, @pass, @fullname, @role, GETDATE())";
                    DatabaseHelper.ExecuteNonQuery(sql,
                        new SqlParameter("@user", username),
                        new SqlParameter("@pass", password),
                        new SqlParameter("@fullname", string.IsNullOrEmpty(fullName) ? username : fullName),
                        new SqlParameter("@role", role));

                    MessageHelper.ShowSuccess($"User added successfully!\nUsername: '{username}'");
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, _userId.HasValue ? "Updating User" : "Adding User");
                MessageHelper.ShowError("Error saving user: " + ex.Message);
            }
        }
    }
}

