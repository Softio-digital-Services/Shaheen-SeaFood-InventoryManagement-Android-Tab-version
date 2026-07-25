using System;
using System.Drawing;
using System.Windows.Forms;
using Shaheen_InventoryManagement_Android.Helpers;
using Shaheen_InventoryManagement_Android.Controls;

namespace Shaheen_InventoryManagement_Android.Forms
{
    public class LicenseForm : Form
    {
        private bool _isDragging = false;
        private Point _dragStartPoint = Point.Empty;

        private TableLayoutPanel tableLayoutPanel1;
        private Panel panelCard;
        private Button btnClose;
        private Button btnMinimize;
        private Label labelTitle;
        private Label labelSubtitle;

        private ModernTextBox txtCustomerName;
        private ModernTextBox txtLicenseKey;
        private ModernTextBox txtHardwareId;
        private ModernButton btnCopy;
        private TableLayoutPanel tlpRepoHw;

        private ModernButton btnActivate;
        private ModernButton btnStartTrial;
        private ModernButton btnExit;

        public LicenseForm()
        {
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "License Activation";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ThemeConfig.ApplyFormIcon(this);

            // Initialize controls
            tableLayoutPanel1 = new TableLayoutPanel();
            panelCard = new Panel();
            btnClose = new Button();
            btnMinimize = new Button();
            labelTitle = new Label();
            labelSubtitle = new Label();
            txtCustomerName = new ModernTextBox();
            txtLicenseKey = new ModernTextBox();
            btnActivate = new ModernButton();
            btnStartTrial = new ModernButton();
            btnExit = new ModernButton();

            // tableLayoutPanel1 (Background Container)
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350F)); // Card Width
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(panelCard, 1, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 620F)); // Card Height
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.Controls.Add(tableLayoutPanel1);

            // panelCard (The White Box)
            panelCard.BackColor = Color.White;
            panelCard.Controls.Add(btnMinimize);
            panelCard.Controls.Add(btnClose);
            panelCard.Controls.Add(labelTitle);
            panelCard.Controls.Add(labelSubtitle);
            panelCard.Controls.Add(txtCustomerName);
            panelCard.Controls.Add(txtLicenseKey);
            panelCard.Controls.Add(btnActivate);
            panelCard.Controls.Add(btnStartTrial);
            panelCard.Controls.Add(btnExit);
            panelCard.Dock = System.Windows.Forms.DockStyle.Fill;
            panelCard.Name = "panelCard";

            // btnClose (X)
            btnClose.Location = new Point(300, 5);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(45, 38);
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            btnClose.BringToFront();

            // btnMinimize (--)
            btnMinimize.Location = new Point(255, 5);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(45, 38);
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMinimize.BringToFront();

            // labelTitle
            labelTitle.Location = new Point(35, 140);
            labelTitle.Name = "labelTitle";

            // labelSubtitle
            labelSubtitle.Location = new Point(35, 195);
            labelSubtitle.Name = "labelSubtitle";

            // Hardware ID section
            bool isAr = LocalizationManager.IsArabic;
            tlpRepoHw = new TableLayoutPanel
            {
                Location = new Point(35, 270),
                Size = new Size(280, 67),
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = Color.Transparent,
                Name = "tlpRepoHw"
            };
            tlpRepoHw.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRepoHw.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85F));

            txtHardwareId = new ModernTextBox
            {
                Text = HardwareInfo.GetShortHardwareId(),
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Name = "txtHardwareId"
            };

            btnCopy = new ModernButton
            {
                Size = new Size(75, 35),
                Margin = isAr ? new Padding(0, 25, 10, 0) : new Padding(10, 25, 0, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Name = "btnCopy"
            };
            btnCopy.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(txtHardwareId.Text);
                    MessageHelper.ShowSuccess(LocalizationManager.GetString("License_Copied", "Hardware ID copied to clipboard!"));
                }
                catch (Exception ex)
                {
                    MessageHelper.ShowError(LocalizationManager.GetString("License_CopyFailed", "Failed to copy to clipboard: ") + ex.Message);
                }
            };

            if (isAr)
            {
                tlpRepoHw.Controls.Add(btnCopy, 0, 0);
                tlpRepoHw.Controls.Add(txtHardwareId, 1, 0);
            }
            else
            {
                tlpRepoHw.Controls.Add(txtHardwareId, 0, 0);
                tlpRepoHw.Controls.Add(btnCopy, 1, 0);
            }
            panelCard.Controls.Add(tlpRepoHw);

            // txtCustomerName
            txtCustomerName.Location = new Point(35, 345);
            txtCustomerName.Size = new Size(280, 67);
            txtCustomerName.Name = "txtCustomerName";

            // txtLicenseKey
            txtLicenseKey.Location = new Point(35, 420);
            txtLicenseKey.Size = new Size(280, 67);
            txtLicenseKey.Name = "txtLicenseKey";

            // btnActivate
            btnActivate.Cursor = System.Windows.Forms.Cursors.Hand;
            btnActivate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnActivate.Location = new Point(35, 500);
            btnActivate.Size = new Size(280, 45);
            btnActivate.Name = "btnActivate";
            btnActivate.Click += (s, e) => ActivateLicense();

            // btnStartTrial
            btnStartTrial.Cursor = System.Windows.Forms.Cursors.Hand;
            btnStartTrial.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnStartTrial.Location = new Point(35, 555);
            btnStartTrial.Size = new Size(135, 40);
            btnStartTrial.Name = "btnStartTrial";
            btnStartTrial.Click += (s, e) => StartTrial();

            // btnExit
            btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnExit.Location = new Point(180, 555);
            btnExit.Size = new Size(135, 40);
            btnExit.Name = "btnExit";
            btnExit.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            ApplyTheme();
            ApplyLocalization();
            SetupDragging();

            this.Shown += (s, e) => {
                txtCustomerName.Focus();
            };
        }

        private void SetupDragging()
        {
            this.MouseDown += OnDraggingMouseDown;
            this.MouseMove += OnDraggingMouseMove;
            this.MouseUp += OnDraggingMouseUp;

            if (tableLayoutPanel1 != null)
            {
                tableLayoutPanel1.MouseDown += OnDraggingMouseDown;
                tableLayoutPanel1.MouseMove += OnDraggingMouseMove;
                tableLayoutPanel1.MouseUp += OnDraggingMouseUp;
            }

            if (panelCard != null)
            {
                panelCard.MouseDown += OnDraggingMouseDown;
                panelCard.MouseMove += OnDraggingMouseMove;
                panelCard.MouseUp += OnDraggingMouseUp;

                labelTitle.MouseDown += OnDraggingMouseDown;
                labelTitle.MouseMove += OnDraggingMouseMove;
                labelTitle.MouseUp += OnDraggingMouseUp;

                labelSubtitle.MouseDown += OnDraggingMouseDown;
                labelSubtitle.MouseMove += OnDraggingMouseMove;
                labelSubtitle.MouseUp += OnDraggingMouseUp;
            }
        }

        private void OnDraggingMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void OnDraggingMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - _dragStartPoint.X, p.Y - _dragStartPoint.Y);
            }
        }

        private void OnDraggingMouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            string L(string key, string defVal) => LocalizationManager.GetString(key, defVal);
            bool isAr = LocalizationManager.IsArabic;

            labelTitle.Text = L("License_Title", "Software Activation");
            labelSubtitle.Text = L("License_Msg_Activate", "This product is unregistered. Please enter your customer name and license key to activate, or start a 30-day trial.");
            txtHardwareId.LabelText = L("License_HardwareId", "Hardware ID");
            txtCustomerName.LabelText = L("License_CustomerName", "Customer / Company Name");
            txtLicenseKey.LabelText = L("License_Key", "License Key");
            btnCopy.Text = L("License_Copy", "Copy");
            btnActivate.Text = L("License_Activate", "Activate");
            btnStartTrial.Text = L("License_StartTrial", "Start Trial");
            btnExit.Text = L("License_Exit", "Exit");

            this.RightToLeft = isAr ? RightToLeft.Yes : RightToLeft.No;
            panelCard.RightToLeft = isAr ? RightToLeft.Yes : RightToLeft.No;

            // Center Titles
            labelTitle.AutoSize = false;
            labelTitle.Width = panelCard.Width;
            labelTitle.TextAlign = ContentAlignment.MiddleCenter;
            labelTitle.Left = 0;

            labelSubtitle.AutoSize = false;
            labelSubtitle.Width = panelCard.Width - 70;
            labelSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            labelSubtitle.Left = 35;

            if (isAr)
            {
                btnStartTrial.Left = 180;
                btnExit.Left = 35;
            }
            else
            {
                btnStartTrial.Left = 35;
                btnExit.Left = 180;
            }
        }

        private void ApplyTheme()
        {
            this.tableLayoutPanel1.BackColor = ThemeConfig.BackgroundColor; 

            // Add Logo
            PictureBox pbLogo = new PictureBox();
            pbLogo.Size = new Size(120, 120);
            pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pbLogo.Anchor = AnchorStyles.Top; 
            try 
            { 
                string logoPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "logo.png");
                if (System.IO.File.Exists(logoPath))
                    pbLogo.Image = Image.FromFile(logoPath);
            } 
            catch { }
            panelCard.Controls.Add(pbLogo);

            // Center Logo on Resize
            panelCard.Resize += (s, e) => {
                pbLogo.Location = new Point((panelCard.Width - 120) / 2, 15);
            };
            pbLogo.Location = new Point((panelCard.Width - 120) / 2, 15);

            labelTitle.Top = 145;
            labelSubtitle.Top = 195;
            labelSubtitle.Height = 65;

            tlpRepoHw.Top = 270;
            txtCustomerName.Top = 345;
            txtLicenseKey.Top = 420;
            
            btnActivate.Top = 500;
            btnStartTrial.Top = 555;
            btnExit.Top = 555;

            labelTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            labelTitle.ForeColor = ThemeConfig.PrimaryColor; 

            labelSubtitle.Font = ThemeConfig.StandardFont;
            labelSubtitle.ForeColor = ThemeConfig.SecondaryColor;

            ThemeConfig.ApplyWindowControl(btnClose, "Close");
            ThemeConfig.ApplyWindowControl(btnMinimize, "Minimize");

            btnClose.ForeColor = ThemeConfig.SecondaryColor;
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = ThemeConfig.DangerColor;
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = ThemeConfig.SecondaryColor;

            btnMinimize.ForeColor = ThemeConfig.SecondaryColor;
            btnMinimize.MouseEnter += (s, e) => btnMinimize.ForeColor = ThemeConfig.PrimaryColor;
            btnMinimize.MouseLeave += (s, e) => btnMinimize.ForeColor = ThemeConfig.SecondaryColor;

            // Apply standard button colors via ThemeConfig styles or manual values
            btnActivate.BackColor = ThemeConfig.PrimaryColor;
            btnActivate.ForeColor = Color.White;
            btnActivate.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

            btnStartTrial.BackColor = Color.FromArgb(230, 230, 240);
            btnStartTrial.ForeColor = ThemeConfig.TextColorDark;
            btnStartTrial.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStartTrial.MouseEnter += (s, e) => btnStartTrial.BackColor = ThemeConfig.SecondaryHoverColor;
            btnStartTrial.MouseLeave += (s, e) => btnStartTrial.BackColor = Color.FromArgb(230, 230, 240);

            btnExit.BackColor = Color.FromArgb(230, 230, 240);
            btnExit.ForeColor = ThemeConfig.TextColorDark;
            btnExit.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExit.MouseEnter += (s, e) => btnExit.BackColor = ThemeConfig.SecondaryHoverColor;
            btnExit.MouseLeave += (s, e) => btnExit.BackColor = Color.FromArgb(230, 230, 240);

            // Rounded Corners for Card
            panelCard.Resize += (s, e) => 
            {
                int radius = 20; 
                using (System.Drawing.Drawing2D.GraphicsPath path = GetRoundedPath(panelCard.ClientRectangle, radius))
                {
                    panelCard.Region = new Region(path);
                }
            };

            if (tableLayoutPanel1.RowStyles.Count >= 2)
            {
                tableLayoutPanel1.RowStyles[1].Height = 620F;
            }

            panelCard.BackColor = ThemeConfig.SurfaceColor;
            panelCard.PerformLayout();
            using (System.Drawing.Drawing2D.GraphicsPath path = GetRoundedPath(panelCard.ClientRectangle, 20))
            {
                panelCard.Region = new Region(path);
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

        private void ActivateLicense()
        {
            string key = txtLicenseKey.Text.Trim();
            string customer = txtCustomerName.Text.Trim();

            if (string.IsNullOrWhiteSpace(customer))
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("License_CustomerRequired", "Customer Name is required."));
                txtCustomerName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("License_KeyRequired", "License Key is required."));
                txtLicenseKey.Focus();
                return;
            }

            var activated = LicenseManager.ActivateLicense(key, customer);
            if (activated != null)
            {
                MessageHelper.ShowSuccess(LocalizationManager.GetString("License_ActivatedSuccessfully", "License activated successfully!"));
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageHelper.ShowError(LocalizationManager.GetString("License_ActivationFailed", "Invalid License Key or Customer Name for this machine."));
            }
        }

        private void StartTrial()
        {
            LicenseKey existing = LicenseManager.GetCurrentLicense();
            if (existing != null && existing.IsTrial() && DateTime.Now > existing.ExpirationDate)
            {
                MessageHelper.ShowError(LocalizationManager.GetString("License_TrialExpired", "Your trial period has already expired. Please activate a full license key."));
                return;
            }

            var trialLicense = LicenseManager.StartTrial();
            if (trialLicense != null)
            {
                MessageHelper.ShowSuccess(LocalizationManager.GetString("License_TrialStarted", "30-day trial period started successfully!"));
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageHelper.ShowError(LocalizationManager.GetString("License_TrialFailed", "Failed to start trial. Please contact support."));
            }
        }
    }
}
