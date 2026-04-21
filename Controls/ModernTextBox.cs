using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GenericInventorySystem.Controls
{
    public class ModernTextBox : UserControl
    {
        private TextBox txtInput;
        private Label lblTitle;
        private Panel pnlContainer;

        // Exposed Properties
        public override string Text
        {
            get => txtInput.Text;
            set => txtInput.Text = value;
        }

        public new event KeyEventHandler KeyDown
        {
            add => txtInput.KeyDown += value;
            remove => txtInput.KeyDown -= value;
        }

        [Category("Appearance")]
        public string LabelText
        {
            get => lblTitle.Text;
            set => lblTitle.Text = value;
        }

        [Category("Behavior")]
        public bool UseSystemPasswordChar
        {
            get => txtInput.UseSystemPasswordChar;
            set => txtInput.UseSystemPasswordChar = value;
        }

        [Category("Behavior")]
        public bool Multiline
        {
            get => txtInput.Multiline;
            set 
            { 
                txtInput.Multiline = value;
                ResizeControls();
            }
        }

        [Category("Appearance")]
        public string PlaceholderText { get; set; } = "";

        private bool _showLabel = true;
        [Category("Appearance")]
        public bool ShowLabel
        {
            get => _showLabel;
            set
            {
                _showLabel = value;
                if (lblTitle != null) lblTitle.Visible = value;
                UpdateLayout();
            }
        }

        public ModernTextBox()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true; 

            this.BackColor = Color.Transparent; // Host container
            this.Size = new Size(350, 70); // Default size (Label + Input)
            this.Padding = new Padding(0);

            InitializeControls();
        }

        private void InitializeControls()
        {
            // Label
            lblTitle = new Label();
            lblTitle.Text = "Input Label";
            lblTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTitle.ForeColor = ThemeConfig.TextColorDark;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(5, 0); // Top left
            this.Controls.Add(lblTitle);

            // Container Panel (Rounded White Box)
            pnlContainer = new Panel();
            pnlContainer.BackColor = Color.White;
            pnlContainer.Location = new Point(0, 25);
            pnlContainer.Size = new Size(this.Width, this.Height - 25);
            pnlContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            pnlContainer.Paint += PnlContainer_Paint;
            pnlContainer.Padding = new Padding(10, 5, 10, 5);
            this.Controls.Add(pnlContainer);

            // TextBox
            txtInput = new TextBox();
            txtInput.BorderStyle = BorderStyle.None;
            txtInput.Font = new Font("Segoe UI", 10F); // premium size
            txtInput.ForeColor = ThemeConfig.TextColorDark;
            txtInput.Dock = DockStyle.Fill;
            txtInput.BackColor = Color.White;
            
            // Center vertically
            txtInput.Location = new Point(10, (pnlContainer.Height - txtInput.Height)/2);
            txtInput.TextChanged += (s, e) => {
                this.OnTextChanged(EventArgs.Empty);
                UpdatePlaceholder();
            };
            
            txtInput.GotFocus += (s, e) => UpdatePlaceholder();
            txtInput.LostFocus += (s, e) => UpdatePlaceholder();

            pnlContainer.Controls.Add(txtInput);
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (pnlContainer == null) return;
            int labelHeight = _showLabel ? 25 : 0;
            pnlContainer.Location = new Point(0, labelHeight);
            pnlContainer.Size = new Size(this.Width, this.Height - labelHeight);
            ResizeControls();
        }

        private void UpdatePlaceholder()
        {
            if (string.IsNullOrEmpty(txtInput.Text) && !txtInput.Focused)
            {
                // We don't have a real placeholder implementation in standard TextBox without PInvoke 
                // but we can simulate it by changing text/color if we were more advanced.
                // For now, let's just leave it as a property for future-proofing or use the LabelText effectively.
            }
        }

        private void ResizeControls()
        {
            if (pnlContainer != null && txtInput != null)
            {
                if(Multiline)
                {
                     txtInput.Dock = DockStyle.Fill;
                }
                else
                {
                     // Center Vertically manually if dock behaves weirdly with single line in large panel
                     txtInput.Dock = DockStyle.None;
                     txtInput.Width = pnlContainer.Width - 20;
                     txtInput.Location = new Point(10, (pnlContainer.Height - txtInput.Height) / 2);
                     txtInput.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResizeControls();
            pnlContainer?.Invalidate(); // Redraw border
        }

        private void PnlContainer_Paint(object sender, PaintEventArgs e)
        {
            var pnl = sender as Panel;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Draw Rounded Border
            using (var path = GetRoundedPath(new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1), 8))
            using (var pen = new Pen(ThemeConfig.BorderColor, 1.5f))
            {
                e.Graphics.DrawPath(pen, path);
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
    }
}
