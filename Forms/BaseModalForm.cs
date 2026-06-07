using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
{
    public class BaseModalForm : Form
    {
        private Label lblTitle;
        private Button btnClose;
        private Button btnMaximize;
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
            set 
            { 
                lblTitle.Text = value; 
                UpdateTitlePosition();
            } 
        }
        public bool EnforceMinWidth { get; set; } = true;
        public Color BorderColor { get; set; } = ThemeConfig.PrimaryColor;

        public BaseModalForm()
        {
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = ThemeConfig.SurfaceColor;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Padding = new Padding(0);

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.Size = new Size(550, 700); 

            ThemeConfig.ApplyFormIcon(this);
            if (LocalizationManager.IsArabic) this.RightToLeft = RightToLeft.Yes;
            InitializeBaseComponents();
            UpdateRegion();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
            if (pnlHeader != null)
            {
                bool isAr = LocalizationManager.IsArabic;
                if (isAr)
                {
                    if (btnClose != null) btnClose.Location = new Point(12, 12);
                    if (btnMaximize != null) btnMaximize.Location = new Point(47, 12);
                }
                else
                {
                    if (btnClose != null) btnClose.Location = new Point(pnlHeader.Width - 35, 12);
                    if (btnMaximize != null) btnMaximize.Location = new Point(pnlHeader.Width - 70, 12);
                }
                UpdateTitlePosition();
            }
            this.Invalidate(); 
        }

        private void UpdateTitlePosition()
        {
            if (pnlHeader == null || lblTitle == null) return;
            
            bool isAr = LocalizationManager.IsArabic;
            if (isAr)
            {
                lblTitle.Location = new Point(pnlHeader.Width - lblTitle.Width - 25, 25);
            }
            else
            {
                lblTitle.Location = new Point(25, 25);
            }
        }

        private void UpdateRegion()
        {
            using (var path = new GraphicsPath())
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    var oldRegion = this.Region;
                    this.Region = null;
                    if (oldRegion != null) oldRegion.Dispose();
                    return;
                }

                int radius = 16;
                int d = radius * 2;
                Rectangle r = new Rectangle(0, 0, this.Width, this.Height);
                
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                
                var newRegion = new Region(path);
                var prevRegion = this.Region;
                this.Region = newRegion;
                if (prevRegion != null) prevRegion.Dispose();
            }
        }

        private void InitializeBaseComponents()
        {
            // Root container to manage layout without overlaps
            TableLayoutPanel tlpRoot = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                Padding = new Padding(0), // No more gutter
                BackColor = Color.Transparent 
            };
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F)); // Header (increased for breathing room)
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Content
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F)); // Footer (reduced for better proportions)
            this.Controls.Add(tlpRoot);

            // 1. Header Panel
            pnlHeader = new Panel {
                Dock = DockStyle.Fill,

                Margin = new Padding(0)
            };
            pnlHeader.MouseDown += Header_MouseDown;
            tlpRoot.Controls.Add(pnlHeader, 0, 0);

            // Title
            bool isAr = LocalizationManager.IsArabic;
            lblTitle = new Label {
                Text = "Modal Title",
                Font = ThemeConfig.HeaderFont,
                ForeColor = ThemeConfig.TextColorDark,
                AutoSize = true,
                Location = new Point(25, 25) 
            };
            pnlHeader.Controls.Add(lblTitle);

            // Close Button (X)
            btnClose = new Button {
                Size = new Size(32, 32),
                Cursor = Cursors.Hand,
                Anchor = isAr ? AnchorStyles.Top | AnchorStyles.Left : AnchorStyles.Top | AnchorStyles.Right
            };
            ThemeConfig.ApplyWindowControl(btnClose, "Close");
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            pnlHeader.Controls.Add(btnClose);

            // Maximize Button
            btnMaximize = new Button {
                Size = new Size(32, 32),
                Cursor = Cursors.Hand,
                Anchor = isAr ? AnchorStyles.Top | AnchorStyles.Left : AnchorStyles.Top | AnchorStyles.Right,
                Tag = "Maximize"
            };
            ThemeConfig.ApplyWindowControl(btnMaximize, "Maximize");
            btnMaximize.Click += (s, e) => {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                    btnMaximize.Tag = "Maximize";
                }
                else
                {
                    this.WindowState = FormWindowState.Maximized;
                    btnMaximize.Tag = "Restore";
                }
                ThemeConfig.ApplyWindowControl(btnMaximize, "Maximize");
            };
            pnlHeader.Controls.Add(btnMaximize);

            // Initial positioning (will be refined in Resize)
            btnClose.Location = new Point(pnlHeader.Width - 40, 15);
            btnMaximize.Location = new Point(pnlHeader.Width - 75, 15);

            // 2. Content Panel
            ContentPanel = new Controls.ModernScrollPanel {

                Dock = DockStyle.Fill,
                Padding = new Padding(30, 10, 30, 10),
                Margin = new Padding(0, 0, 5, 0) // Inset from right to avoid border overlap
            };
            tlpRoot.Controls.Add(ContentPanel, 0, 1);

            // 3. Footer Panel
            FooterPanel = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent, // Match parent
                Padding = new Padding(30, 15, 30, 15), 
                Margin = new Padding(0, 0, 5, 0) // Align with content panel
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
            // Start with absolute minimum for buttons, then grow
            int requiredWidth = EnforceMinWidth ? 500 : 350; 
            
            foreach (Control ctrl in ContentPanel.Controls)
            {
                if (ctrl.Visible)
                {
                    int w = ctrl.Width;
                    if (ctrl.AutoSize)
                        w = ctrl.GetPreferredSize(new Size(0, ContentPanel.Height)).Width;
                    
                    int right = w + ContentPanel.Padding.Horizontal + 60;
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
            int minH = Math.Min(180, maxH); // Reduced from 350 for message boxes
            this.Height = Math.Max(minH, Math.Min(totalRequiredHeight, maxH));
            
            CenterOnScreen();
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
                    Size = new Size(130, 35),
                    Margin = new Padding(10, 0, 0, 0),
                    Font = ThemeConfig.ButtonFont
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
                    Size = new Size(130, 35),
                    Margin = new Padding(10, 0, 0, 0),
                    Font = ThemeConfig.ButtonFont
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
                    Size = new Size(130, 35),
                    Margin = new Padding(10, 0, 0, 0),
                    Font = ThemeConfig.ButtonFont
                };
                TertiaryButton.Click += onTertiaryClick;
                ThemeConfig.ApplySecondaryButton(TertiaryButton);
                flpButtons.Controls.Add(TertiaryButton);
            }

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Region handles the sharp corner clipping, so we just clear and draw
            e.Graphics.Clear(this.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality; // Ensure smooth edges
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float radius = 16f;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            
            // 1. Draw the Solid Background (The Form itself)
            using (GraphicsPath path = GetRoundedPath(rect, radius))
            {
                using (SolidBrush brush = new SolidBrush(ThemeConfig.SurfaceColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // 2. Neon Border with Glow (Color depends on BorderColor)
                Color neonColor = BorderColor;
                
                // Outer glow layer (Subtle spread)
                using (Pen glow1 = new Pen(Color.FromArgb(25, neonColor), 5f))
                    e.Graphics.DrawPath(glow1, path);
                
                // Mid glow layer (Condensed)
                using (Pen glow2 = new Pen(Color.FromArgb(55, neonColor), 3f))
                    e.Graphics.DrawPath(glow2, path);
                    
                // Main Sharp Neon Border (Refined 1.5px for crispness)
                using (Pen mainPen = new Pen(neonColor, 1.5f))
                {
                    e.Graphics.DrawPath(mainPen, path);
                }
            }
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
            if (EnforceMinWidth)
            {
                int minWidth = Math.Min(500, workingArea.Width / 2);
                if (this.Width < minWidth) this.Width = minWidth;
            }
            else
            {
                if (this.Width < 350) this.Width = 350; // Smaller fallback for message boxes
            }

           // 4. Fit to content to ensure we didn't squash
           FitToContent();
           
           CenterOnScreen();
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2;
            RectangleF r = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
            
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Navigation via Enter or Arrows
            if (keyData == Keys.Enter || keyData == Keys.Down || keyData == Keys.Up)
            {
                Control active = this.ActiveControl;
                
                // Determine if the current control is a text input
                bool isTextBox = active is TextBox || (active != null && active.Parent is Controls.ModernTextBox);

                // For Arrows: ONLY handle if it's a regular text input
                if (keyData == Keys.Down || keyData == Keys.Up)
                {
                    if (!isTextBox) return base.ProcessCmdKey(ref msg, keyData);
                }

                // Special handling for multiline textboxes (Enter adds new line)
                if (active is TextBox tb && tb.Multiline && keyData == Keys.Enter)
                    return base.ProcessCmdKey(ref msg, keyData);
                
                // Execute navigation
                if (keyData == Keys.Enter || keyData == Keys.Down)
                {
                    this.SelectNextControl(active, true, true, true, true);
                    return true;
                }
                else if (keyData == Keys.Up)
                {
                    this.SelectNextControl(active, false, true, true, true);
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}

