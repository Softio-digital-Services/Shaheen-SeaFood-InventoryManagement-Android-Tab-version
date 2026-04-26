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

        public void Clear()
        {
            this.Text = string.Empty;
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

        [Category("Appearance")]
        public bool IsSearch { get; set; } = false;

        private bool _isPassword = false;
        [Category("Appearance")]
        public bool IsPassword
        {
            get => _isPassword;
            set
            {
                _isPassword = value;
                txtInput.UseSystemPasswordChar = value;
                ResizeControls();
                pnlContainer?.Invalidate();
            }
        }

        private bool _isFocused = false;

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
            this.Size = new Size(350, 67); // Default size (25px Label + 42px Input)
            this.Padding = new Padding(0);

            InitializeControls();
        }

        private void InitializeControls()
        {
            // Label
            lblTitle = new Label();
            lblTitle.Text = ""; 
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
            pnlContainer.Resize += (s, e) => UpdateContainerRegion();
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
            
            txtInput.GotFocus += (s, e) => { _isFocused = true; pnlContainer.Invalidate(); UpdatePlaceholder(); };
            txtInput.LostFocus += (s, e) => { _isFocused = false; pnlContainer.Invalidate(); UpdatePlaceholder(); };

            pnlContainer.Controls.Add(txtInput);
            
            pnlContainer.Click += (s, e) => {
                if (IsPassword)
                {
                    Point p = pnlContainer.PointToClient(Cursor.Position);
                    if (p.X > pnlContainer.Width - 35)
                    {
                        txtInput.UseSystemPasswordChar = !txtInput.UseSystemPasswordChar;
                        pnlContainer.Invalidate();
                    }
                }
            };

            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (pnlContainer == null) return;
            int labelHeight = _showLabel ? 25 : 0;
            pnlContainer.Location = new Point(0, labelHeight);
            
            // Standardize height for single-line inputs
            if (!Multiline)
            {
                this.Height = labelHeight + 42; // Enforce 42px input height
            }
            
            pnlContainer.Size = new Size(this.Width, this.Height - labelHeight);
            UpdateContainerRegion();
            ResizeControls();
        }

        private void UpdateContainerRegion()
        {
            if (pnlContainer == null) return;
            using (var path = new GraphicsPath())
            {
                int radius = 12;
                int d = radius * 2;
                Rectangle r = new Rectangle(0, 0, pnlContainer.Width, pnlContainer.Height);
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                pnlContainer.Region = new Region(path);
            }
        }

        private void UpdatePlaceholder()
        {
            // Simple placeholder logic
            if (string.IsNullOrEmpty(txtInput.Text) && !_isFocused)
            {
                // Note: Real WinForms placeholder requires SendMessage EM_SETCUEBANNER
                // For now we rely on the Label or external logic, but we'll reserve the property.
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
                     int leftPadding = IsSearch ? 40 : 10;
                     int rightPadding = IsPassword ? 40 : 10;
                     txtInput.Width = pnlContainer.Width - leftPadding - rightPadding;
                     txtInput.Location = new Point(leftPadding, (pnlContainer.Height - txtInput.Height) / 2);
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
            using (var path = GetRoundedPath(new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1), 12))
            using (var pen = new Pen(_isFocused ? ThemeConfig.PrimaryColor : ThemeConfig.BorderColor, _isFocused ? 2f : 1.5f))
            {
                e.Graphics.DrawPath(pen, path);
            }

            // Draw Search Icon
            if (IsSearch)
            {
                Image searchIcon = ThemeConfig.GetNuricon("search");
                if (searchIcon != null)
                {
                    e.Graphics.DrawImage(searchIcon, new Rectangle(12, (pnl.Height - 20) / 2, 20, 20));
                }
            }

            // Draw Password Eye Icon
            if (IsPassword)
            {
                Image eyeIcon = ThemeConfig.GetNuricon("view");
                if (eyeIcon != null)
                {
                    Color eyeColor = txtInput.UseSystemPasswordChar ? ThemeConfig.SecondaryColor : ThemeConfig.PrimaryColor;
                    using (Image tintedEye = ThemeConfig.TintImage(eyeIcon, eyeColor))
                    {
                        e.Graphics.DrawImage(tintedEye, new Rectangle(pnl.Width - 32, (pnl.Height - 20) / 2, 20, 20));
                    }
                }
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
