using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Helpers.Plugins;

namespace GenericInventorySystem.Plugins
{
    /// <summary>
    /// Free built-in plugin — adds a full-featured calculator to the sidebar.
    /// RequiresLicense = false → always visible for all license types.
    /// </summary>
    public class CalculatorPlugin : ITabPlugin
    {
        public string Id          => "com.carparts.calculator";
        public string Name        => "Calculator";
        public string Version     => "1.0.0";
        public string Description => "Full-featured in-app calculator";
        public string Author      => "Car Parts Inventory System";

        public bool   RequiresLicense   => false;
        public string LicenseFeatureKey => "";

        public string TabTitle => LocalizationManager.IsArabic ? "\u062d\u0627\u0633\u0628\u0629" : "Calculator";
        public string TabIcon  => "calculator";
        public int    TabOrder => 110;

        private PluginContext _ctx;

        public void Initialize(PluginContext context) => _ctx = context;
        public void Shutdown() { }

        public UserControl CreateTabContent() => new CalculatorPanel();
    }

    /// <summary>The calculator UI panel.</summary>
    public class CalculatorPanel : UserControl
    {
        private TextBox _display;
        private string  _current  = "0";
        private string  _operator = "";
        private double  _prev     = 0;
        private bool    _newEntry = true;

        public CalculatorPanel()
        {
            this.BackColor = ThemeConfig.BackgroundColor;
            this.Dock      = DockStyle.Fill;
            Build();
        }

        private void Build()
        {
            // Title
            Label lbl = ThemeConfig.CreateStandardHeader(
                LocalizationManager.IsArabic ? "\u062d\u0627\u0633\u0628\u0629" : "Calculator");
            lbl.Dock = DockStyle.Top;
            this.Controls.Add(lbl);

            // Centered card
            Panel card = new Panel();
            card.Width     = 340;
            card.Height    = 460;
            card.BackColor = ThemeConfig.SurfaceColor;
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = RoundRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 16))
                using (var pen  = new Pen(ThemeConfig.BorderColor, 1.5f))
                    e.Graphics.DrawPath(pen, path);
            };

            this.Resize += (s, e) =>
            {
                card.Location = new Point((this.Width - card.Width) / 2, 80);
            };
            this.Controls.Add(card);

            // Display
            _display = new TextBox();
            _display.ReadOnly    = true;
            _display.Text        = "0";
            _display.Font        = new Font("Segoe UI", 28, FontStyle.Bold);
            _display.TextAlign   = HorizontalAlignment.Right;
            _display.BorderStyle = BorderStyle.None;
            _display.BackColor   = ThemeConfig.SurfaceColor;
            _display.ForeColor   = ThemeConfig.TextColorDark;
            _display.Location    = new Point(10, 15);
            _display.Size        = new Size(320, 55);
            card.Controls.Add(_display);

            // Separator
            Panel sep = new Panel { BackColor = ThemeConfig.BorderColor, Location = new Point(10, 75), Size = new Size(320, 1) };
            card.Controls.Add(sep);

            // Button grid
            string[][] rows =
            {
                new[] { "C", "\u00b1", "%", "\u00f7" },
                new[] { "7", "8", "9", "\u00d7" },
                new[] { "4", "5", "6", "\u2212" },
                new[] { "1", "2", "3", "+" },
                new[] { "0", ".", "\u232b", "=" }
            };

            string[] operators = { "\u00f7", "\u00d7", "\u2212", "+" };

            int bw = 72, bh = 58, gap = 8;
            int startX = 14, startY = 88;

            for (int r = 0; r < rows.Length; r++)
            {
                for (int c = 0; c < rows[r].Length; c++)
                {
                    string label = rows[r][c];
                    Button btn = new Button();
                    btn.Text      = label;
                    btn.Size      = new Size(bw, bh);
                    btn.Location  = new Point(startX + c * (bw + gap), startY + r * (bh + gap));
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Font      = new Font("Segoe UI", 14, FontStyle.Regular);
                    btn.Cursor    = Cursors.Hand;

                    if (label == "=")
                    {
                        btn.BackColor = ThemeConfig.PrimaryColor;
                        btn.ForeColor = Color.White;
                    }
                    else if (label == "C")
                    {
                        btn.BackColor = ThemeConfig.DangerColor;
                        btn.ForeColor = Color.White;
                    }
                    else if (operators.Contains(label))
                    {
                        btn.BackColor = ThemeConfig.ActiveBackColor;
                        btn.ForeColor = ThemeConfig.PrimaryColor;
                        btn.Font      = new Font("Segoe UI", 16, FontStyle.Bold);
                    }
                    else
                    {
                        btn.BackColor = ThemeConfig.SurfaceColor;
                        btn.ForeColor = ThemeConfig.TextColorDark;
                        btn.FlatAppearance.BorderColor = ThemeConfig.BorderColor;
                        btn.FlatAppearance.BorderSize  = 1;
                    }

                    DrawRounded(btn);

                    string captured = label;
                    btn.Click += (s, e) => PressButton(captured);
                    card.Controls.Add(btn);
                }
            }
        }

        private void PressButton(string key)
        {
            string[] operators = { "\u00f7", "\u00d7", "\u2212", "+" };
            switch (key)
            {
                case "C":
                    _current = "0"; _prev = 0; _operator = ""; _newEntry = true;
                    break;
                case "\u232b": // backspace
                    _current = _current.Length > 1 ? _current.Substring(0, _current.Length - 1) : "0";
                    break;
                case "\u00b1": // plus-minus
                    if (double.TryParse(_current, out double neg))
                        _current = (-neg).ToString();
                    break;
                case "%":
                    if (double.TryParse(_current, out double pct))
                        _current = (pct / 100).ToString();
                    break;
                case "=":
                    Compute();
                    _operator = "";
                    _newEntry = true;
                    break;
                case ".":
                    if (_newEntry) { _current = "0."; _newEntry = false; }
                    else if (!_current.Contains(".")) _current += ".";
                    break;
                default:
                    if (operators.Contains(key))
                    {
                        if (double.TryParse(_current, out double val))
                        {
                            if (!_newEntry) Compute();
                            _prev     = double.Parse(_current);
                            _operator = key;
                            _newEntry = true;
                        }
                    }
                    else // digit
                    {
                        if (_newEntry) { _current = key; _newEntry = false; }
                        else _current = (_current == "0") ? key : _current + key;
                    }
                    break;
            }
            _display.Text = _current;
        }

        private void Compute()
        {
            if (!double.TryParse(_current, out double b)) return;
            double result = _prev;
            if      (_operator == "\u00f7") result = b != 0 ? _prev / b : 0;
            else if (_operator == "\u00d7") result = _prev * b;
            else if (_operator == "\u2212") result = _prev - b;
            else if (_operator == "+")      result = _prev + b;
            _current = result % 1 == 0 ? ((long)result).ToString() : result.ToString("G10");
        }

        private static void DrawRounded(Button btn)
        {
            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (var path  = RoundRect(r, 10))
                using (var brush = new SolidBrush(btn.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                    if (btn.FlatAppearance.BorderSize > 0)
                        using (var pen = new Pen(ThemeConfig.BorderColor, 1))
                            e.Graphics.DrawPath(pen, path);
                }
                TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font,
                    r, btn.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundRect(Rectangle r, int rad)
        {
            var p = new System.Drawing.Drawing2D.GraphicsPath();
            p.AddArc(r.X, r.Y, rad, rad, 180, 90);
            p.AddArc(r.Right - rad, r.Y, rad, rad, 270, 90);
            p.AddArc(r.Right - rad, r.Bottom - rad, rad, rad, 0, 90);
            p.AddArc(r.X, r.Bottom - rad, rad, rad, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
