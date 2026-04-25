using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel;

namespace GenericInventorySystem.Forms
{
    public class BaseModalForm : Form
    {
        private Label lblTitle;
        private Button btnClose;
        private Panel pnlHeader;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Controls.ModernScrollPanel ContentPanel { get; private set; }
        public Panel FooterPanel { get; private set; }
        protected Controls.ModernButton PrimaryButton { get; private set; }
        protected Controls.ModernButton SecondaryButton { get; private set; }
        protected Controls.ModernButton TertiaryButton { get; private set; }

        public string TitleText 
        { 
            get => lblTitle.Text; 
            set => lblTitle.Text = value; 
        }
        public bool EnforceMinWidth { get; set; } = true;

        public BaseModalForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = ThemeConfig.SurfaceColor;
            this.Padding = new Padding(0); // Panels handle their own padding

            this.DoubleBuffered = true;
            this.Size = new Size(550, 700); 

            ThemeConfig.ApplyFormIcon(this);
            InitializeBaseComponents();
        }

        private void InitializeBaseComponents()
        {
            // Root container to manage layout without overlaps
            TableLayoutPanel tlpRoot = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0)
            };
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F)); // Header (increased for breathing room)
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Content
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F)); // Footer (reduced for better proportions)
            this.Controls.Add(tlpRoot);

            // 1. Header Panel
            pnlHeader = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            pnlHeader.MouseDown += Header_MouseDown;
            tlpRoot.Controls.Add(pnlHeader, 0, 0);

            // Title
            lblTitle = new Label {
                Text = "Modal Title",
                Font = ThemeConfig.HeaderFont,
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = true,
                Location = new Point(25, 25) // Better centering in 70px header
            };
            pnlHeader.Controls.Add(lblTitle);

            // Close Button (X)
            btnClose = new Button {
                Size = new Size(45, 32),
                Location = new Point(this.Width - 55, 19), // Better centering
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            ThemeConfig.ApplyWindowControl(btnClose, "Close");
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            pnlHeader.Controls.Add(btnClose);

            // 2. Content Panel
            ContentPanel = new Controls.ModernScrollPanel {
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 10, 30, 10),
                Margin = new Padding(0)
            };
            tlpRoot.Controls.Add(ContentPanel, 0, 1);

            // 3. Footer Panel
            FooterPanel = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent, // Match parent
                Padding = new Padding(30, 15, 30, 15), 
                Margin = new Padding(0)
            };
            
            // Add a subtle separator line at the top of the footer
            FooterPanel.Paint += (s, e) => {
                using (Pen p = new Pen(ThemeConfig.BorderColor, 1))
                {
                    e.Graphics.DrawLine(p, 0, 0, FooterPanel.Width, 0);
                }
            };

            tlpRoot.Controls.Add(FooterPanel, 0, 2);
        }

        public void ApplyGoldenRatio(int baseDimension, bool horizontal = false)
        {
            double phi = 1.618;
            if (horizontal)
            {
                this.Width = baseDimension;
                this.Height = (int)(baseDimension / phi);
            }
            else
            {
                this.Width = baseDimension;
                this.Height = (int)(baseDimension * phi);
            }
            
            // Ensure we center again after explicit size change
            CenterOnScreen();
        }

        public void FitToContent(int extraHeight = 0)
        {
            if (this.DesignMode) return;

            // Force layout
            this.PerformLayout();
            ContentPanel.PerformLayout();
            
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            int maxW = (int)(workingArea.Width * 0.85); // Up to 85% width
            int maxH = (int)(workingArea.Height * 0.85); // Up to 85% height
            
            // 1. Calculate Required Width
            int requiredWidth = this.Width;
            foreach (Control ctrl in ContentPanel.Controls)
            {
                if (ctrl.Visible)
                {
                    int w = ctrl.Width;
                    if (ctrl.AutoSize)
                        w = ctrl.GetPreferredSize(new Size(0, ContentPanel.Height)).Width;
                    
                    int right = ctrl.Left + w + ctrl.Margin.Right + ContentPanel.Padding.Horizontal + 40;
                    if (right > requiredWidth) requiredWidth = right;
                }
            }
            this.Width = Math.Min(requiredWidth, maxW);

            // 2. Calculate Required Height
            int headerH = 70;
            int footerH = 70;
            
            // Get preferred size of the content panel based on its current width
            Size preferredSize = ContentPanel.GetPreferredSize(new Size(ContentPanel.Width, 0));
            int contentHeight = preferredSize.Height;

            // Robust calculation: iterate controls for height
            int maxBottom = 0;
            foreach (Control ctrl in ContentPanel.Controls)
            {
                if (ctrl.Visible)
                {
                    int h = ctrl.Height;
                    if (ctrl.AutoSize)
                        h = ctrl.GetPreferredSize(new Size(ContentPanel.Width, 0)).Height;
                    
                    int bottom = ctrl.Top + h + ctrl.Margin.Bottom;
                    if (bottom > maxBottom) maxBottom = bottom;
                }
            }
            
            contentHeight = Math.Max(contentHeight, maxBottom);
            int totalRequiredHeight = headerH + footerH + contentHeight + ContentPanel.Padding.Vertical + extraHeight + 20; 
            
            // 3. Apply responsive constraints
            int minH = Math.Min(350, maxH); 
            this.Height = Math.Max(minH, Math.Min(totalRequiredHeight, maxH));
            
            CenterOnScreen();
            UpdateRegion();
        }

        private void CenterOnScreen()
        {
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Left = workingArea.Left + (workingArea.Width - this.Width) / 2;
            this.Top = workingArea.Top + (workingArea.Height - this.Height) / 2;
        }

        public void SetFooterButtons(string primaryText, string secondaryText, EventHandler onPrimaryClick, EventHandler onSecondaryClick, string tertiaryText = null, EventHandler onTertiaryClick = null)
        {
            FooterPanel.Controls.Clear();
            
            FlowLayoutPanel flpButtons = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0),
                BackColor = Color.Transparent
            };
            FooterPanel.Controls.Add(flpButtons);
            // 1. Primary Button
            if (!string.IsNullOrEmpty(primaryText))
            {
                PrimaryButton = new Controls.ModernButton { 
                    Text = primaryText, 
                    Size = new Size(130, 40),
                    Margin = new Padding(10, 0, 0, 0)
                };
                PrimaryButton.Click += onPrimaryClick;
                ThemeConfig.ApplyPrimaryButton(PrimaryButton);
                flpButtons.Controls.Add(PrimaryButton);
            }

            // 2. Secondary Button
            if (!string.IsNullOrEmpty(secondaryText))
            {
                SecondaryButton = new Controls.ModernButton { 
                    Text = secondaryText, 
                    Size = new Size(130, 40),
                    Margin = new Padding(10, 0, 0, 0)
                };
                SecondaryButton.Click += onSecondaryClick;
                ThemeConfig.ApplySecondaryButton(SecondaryButton);
                flpButtons.Controls.Add(SecondaryButton);
            }

            // 3. Tertiary Button
            if (!string.IsNullOrEmpty(tertiaryText))
            {
                TertiaryButton = new Controls.ModernButton { 
                    Text = tertiaryText, 
                    Size = new Size(130, 40),
                    Margin = new Padding(10, 0, 0, 0)
                };
                TertiaryButton.Click += onTertiaryClick;
                ThemeConfig.ApplySecondaryButton(TertiaryButton);
                flpButtons.Controls.Add(TertiaryButton);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Draw Rounded Border (Outline Only)
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            int radius = 16;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            
            using (GraphicsPath path = GetRoundedPath(rect, radius))
            using (Pen pen = new Pen(ThemeConfig.BorderColor, 1)) // Standard border
            {
                e.Graphics.DrawPath(pen, path); // Draw outline
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion(); // Update clipping region
            this.Invalidate(); // Redraw border
        }
        
        protected override void OnLoad(EventArgs e)
        {
           base.OnLoad(e);
           
           Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
           
           // Apply adaptive sizing if width hasn't been explicitly set to something huge
           if (this.Width < 200) this.Width = 600; 

           // 1. Cap Width (95% of Screen)
           if (this.Width > workingArea.Width * 0.95)
               this.Width = (int)(workingArea.Width * 0.95);

           // 2. Initial Height Cap
           if (this.Height > workingArea.Height * 0.9)
               this.Height = (int)(workingArea.Height * 0.9);

           // 3. Enforce Minimum Width
           int minWidth = Math.Min(500, workingArea.Width / 2);
           if (this.Width < minWidth) this.Width = minWidth;

           // 4. Fit to content to ensure we didn't squash
           FitToContent();
           
           CenterOnScreen();
           UpdateRegion();
        }

        private void UpdateRegion()
        {
            int radius = 16;
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            using (GraphicsPath path = GetRoundedPath(rect, radius))
            {
                this.Region = new Region(path);
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // Drag Logic
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
    }
}
