using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GenericInventorySystem.Forms
{
    public class BaseModalForm : Form
    {
        private Label lblTitle;
        private Button btnClose;
        private Panel pnlHeader;
        public Panel ContentPanel { get; private set; }

        public string TitleText 
        { 
            get => lblTitle.Text; 
            set => lblTitle.Text = value; 
        }

        public BaseModalForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = ThemeConfig.SurfaceColor;
            this.Padding = new Padding(2, 52, 2, 8); // Top for header, bottom for rounded corners safety

            this.DoubleBuffered = true;
            this.Size = new Size(500, 600); // Default

            ThemeConfig.ApplyFormIcon(this);
            InitializeBaseComponents();
        }

        private void InitializeBaseComponents()
        {
            // Header Panel (Clickable for dragging)
            pnlHeader = new Panel();
            pnlHeader.Height = 50;
            pnlHeader.BackColor = Color.Transparent;
            pnlHeader.MouseDown += Header_MouseDown;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Width = this.Width;
            pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Title
            lblTitle = new Label();
            lblTitle.Text = "Modal Title";
            lblTitle.Font = ThemeConfig.HeaderFont;
            lblTitle.ForeColor = ThemeConfig.TextColorDark;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 15);
            pnlHeader.Controls.Add(lblTitle);

            // Close Button (X)
            btnClose = new Button();
            btnClose.Size = new Size(45, 32);
            ThemeConfig.ApplyWindowControl(btnClose, "Close");
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            
            // Anchor right - Flush to top right corner
            btnClose.Location = new Point(this.Width - 45, 0);
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            
            pnlHeader.Controls.Add(btnClose);
            this.Controls.Add(pnlHeader);

            // Content Panel - fills rest of form via Dock
            ContentPanel = new Panel();
            ContentPanel.BackColor = Color.Transparent;
            ContentPanel.AutoScroll = true; // Safety measure
            ContentPanel.Dock = DockStyle.Fill;
            this.Controls.Add(ContentPanel);
            ContentPanel.BringToFront();
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

                // Note: Region is set in OnResize to prevent flickering
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
