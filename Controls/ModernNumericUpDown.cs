using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Controls
{
    public class ModernNumericUpDown : UserControl
    {
        private TextBox txtInput;
        private Label lblTitle;
        private Panel pnlContainer;
        private Button btnUp;
        private Button btnDown;

        private decimal _value = 0;
        [Category("Appearance")]
        public decimal Value
        {
            get => _value;
            set 
            { 
                _value = Math.Max(_minimum, Math.Min(_maximum, value));
                UpdateText();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private decimal _minimum = 0;
        [Category("Appearance")]
        public decimal Minimum 
        { 
            get => _minimum; 
            set { _minimum = value; if (_value < value) Value = value; } 
        }

        private decimal _maximum = 100;
        [Category("Appearance")]
        public decimal Maximum 
        { 
            get => _maximum; 
            set { _maximum = value; if (_value > value) Value = value; } 
        }

        private int _decimalPlaces = 0;
        [Category("Appearance")]
        public int DecimalPlaces 
        { 
            get => _decimalPlaces; 
            set { _decimalPlaces = value; UpdateText(); } 
        }

        private decimal _increment = 1;
        [Category("Appearance")]
        public decimal Increment 
        { 
            get => _increment; 
            set => _increment = value; 
        }

        [Category("Appearance")]
        public string LabelText 
        { 
            get => lblTitle.Text; 
            set => lblTitle.Text = value; 
        }

        private bool _showLabel = true;
        [Category("Appearance")]
        public bool ShowLabel
        {
            get => _showLabel;
            set
            {
                _showLabel = value;
                lblTitle.Visible = value;
                UpdateLayout();
            }
        }

        public event EventHandler ValueChanged;

        public ModernNumericUpDown()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;
            this.Size = new Size(150, 67);
            
            InitializeControls();
        }

        private void InitializeControls()
        {
            // Label
            lblTitle = new Label { 
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), 
                ForeColor = ThemeConfig.TextColorDark, 
                AutoSize = true, 
                Location = new Point(5, 0) 
            };
            this.Controls.Add(lblTitle);

            // Container Panel
            pnlContainer = new Panel { 
                BackColor = Color.Transparent,
                Padding = new Padding(10, 8, 35, 5)
            };
            pnlContainer.Paint += PnlContainer_Paint;
            this.Controls.Add(pnlContainer);

            // Input TextBox
            txtInput = new TextBox { 
                BorderStyle = BorderStyle.None, 
                Font = new Font("Segoe UI", 10F), 
                ForeColor = ThemeConfig.TextColorDark, 
                BackColor = Color.White,
                Dock = DockStyle.Fill
            };
            txtInput.KeyPress += TxtInput_KeyPress;
            txtInput.Enter += (s, e) => {
                if (txtInput.IsHandleCreated) txtInput.BeginInvoke(new Action(() => { txtInput.Select(0, 0); txtInput.SelectionLength = 0; }));
            };
            txtInput.GotFocus += (s, e) => {
                if (txtInput.IsHandleCreated) txtInput.BeginInvoke(new Action(() => { txtInput.Select(0, 0); txtInput.SelectionLength = 0; }));
            };
            txtInput.LostFocus += (s, e) => {
                if (decimal.TryParse(txtInput.Text, out decimal val)) Value = val;
                else UpdateText();
            };
            pnlContainer.Controls.Add(txtInput);

            // Up Button
            btnUp = new Button { 
                Size = new Size(32, 21), 
                FlatStyle = FlatStyle.Flat, 
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            btnUp.FlatAppearance.BorderSize = 0;
            btnUp.Paint += (s, e) => DrawChevron(e.Graphics, true);
            btnUp.Click += (s, e) => { Value += _increment; };
            pnlContainer.Controls.Add(btnUp);

            // Down Button
            btnDown = new Button { 
                Size = new Size(32, 21), 
                FlatStyle = FlatStyle.Flat, 
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            btnDown.FlatAppearance.BorderSize = 0;
            btnDown.Paint += (s, e) => DrawChevron(e.Graphics, false);
            btnDown.Click += (s, e) => { Value -= _increment; };
            pnlContainer.Controls.Add(btnDown);

            UpdateLayout();
            UpdateText();
        }

        private void UpdateLayout()
        {
            if (pnlContainer == null) return;

            int labelHeight = _showLabel ? 25 : 0;
            pnlContainer.Location = new Point(0, labelHeight);
            pnlContainer.Size = new Size(this.Width, 42);
            
            if (btnUp != null) btnUp.Location = new Point(pnlContainer.Width - 32, 0);
            if (btnDown != null) btnDown.Location = new Point(pnlContainer.Width - 32, 21);
            
            this.Height = labelHeight + 42;
        }

        private void UpdateText()
        {
            if (txtInput != null)
                txtInput.Text = _value.ToString("F" + _decimalPlaces);
        }

        private void TxtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.') && (e.KeyChar != '-'))
                e.Handled = true;
            if ((e.KeyChar == '.') && (txtInput.Text.IndexOf('.') > -1))
                e.Handled = true;
            if ((e.KeyChar == '-') && (txtInput.Text.Length > 0 && txtInput.SelectionStart != 0))
                e.Handled = true;
        }

        private void DrawChevron(Graphics g, bool up)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Image icon = ThemeConfig.GetNuricon(up ? "chevron_up" : "chevron_down");
            if (icon != null)
            {
                using (var tinted = ThemeConfig.TintImage(icon, ThemeConfig.SecondaryColor))
                {
                    g.DrawImage(tinted, new Rectangle(8, 4, 16, 13));
                }
            }
        }

        private void PnlContainer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Clear corners
            Color parentColor = ThemeConfig.GetParentColor(this);
            using (var brush = new SolidBrush(parentColor))
                e.Graphics.FillRectangle(brush, -1, -1, pnlContainer.Width + 2, pnlContainer.Height + 2);

            Rectangle rect = new Rectangle(0, 0, pnlContainer.Width - 1, pnlContainer.Height - 1);
            using (var path = ThemeConfig.GetRoundedPathPublic(rect, 12))
            {
                using (var brush = new SolidBrush(Color.White)) e.Graphics.FillPath(brush, path);
                using (var pen = new Pen(ThemeConfig.BorderColor, 1.5f)) e.Graphics.DrawPath(pen, path);
            }

            // Separator for buttons
            e.Graphics.DrawLine(new Pen(ThemeConfig.BorderColor, 1f), pnlContainer.Width - 32, 5, pnlContainer.Width - 32, pnlContainer.Height - 5);
            e.Graphics.DrawLine(new Pen(ThemeConfig.BorderColor, 1f), pnlContainer.Width - 32, 21, pnlContainer.Width - 2, 21);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
            pnlContainer?.Invalidate();
        }
    }
}
