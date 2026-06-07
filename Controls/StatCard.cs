using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace InventorySystem.Controls
{
    public class StatCard : Panel
    {
        private Label _lblTitle;
        private Label _lblValue;
        private Label _lblSubtitle;
        private Panel _iconPanel;
        private string _iconText;
        private Color _themeColor = Color.Blue;

        public string Title 
        { 
            get => _lblTitle.Text; 
            set => _lblTitle.Text = value; 
        }

        public string Value 
        { 
            get => _lblValue.Text; 
            set => _lblValue.Text = value; 
        }

        public string Subtitle 
        { 
            get => _lblSubtitle.Text; 
            set => _lblSubtitle.Text = value; 
        }

        private Image _iconImage;

        public Image IconImage
        {
            get => _iconImage;
            set
            {
                _iconImage = value;
                if (_iconPanel != null) _iconPanel.Invalidate();
            }
        }

        public string Icon 
        { 
            get => _iconText; 
            set 
            {
                _iconText = value;
                if (_iconPanel != null) _iconPanel.Invalidate();
            } 
        }

        public Color ThemeColor
        {
            get => _themeColor;
            set 
            {
                _themeColor = value;
                UpdateTheme();
            }
        }

        private int _iconPadding = 8;
        public int IconPadding
        {
            get => _iconPadding;
            set
            {
                _iconPadding = value;
                if (_iconPanel != null) _iconPanel.Invalidate();
            }
        }

        public StatCard()
        {
            InitializeControls();
        }

        private void InitializeControls()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.Size = new Size(240, 110);
            this.Padding = new Padding(20);
            this.BackColor = Color.Transparent;
            
            // Container for Icon
            _iconPanel = new Panel();
            _iconPanel.Size = new Size(50, 50);
            _iconPanel.Location = new Point(this.Width - 65, 20); // Top Right
            _iconPanel.Paint += IconPanel_Paint;
            



            // Labels
            _lblTitle = new Label
            {
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeConfig.MutedTextColor, // Cool Gray
                AutoSize = true,
                Location = new Point(20, 15) // Adjusted Top
            };


            _lblValue = new Label
            {
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark, // Dark Navy
                AutoSize = true,
                Location = new Point(18, 40)
            };


            _lblSubtitle = new Label
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeConfig.MutedTextColor,
                AutoSize = true,
                Location = new Point(22, 85)
            };


            this.Controls.Add(_iconPanel);
            this.Controls.Add(_lblSubtitle);
            this.Controls.Add(_lblValue);
            this.Controls.Add(_lblTitle);

            // Initial Theme
            this.ThemeColor = Color.FromArgb(67, 24, 255); // Default Blue
            
            // Resize handling: reposition icon based on layout direction
            this.Resize += (s, e) => {
                bool isRtl = this.RightToLeft == RightToLeft.Yes;
                _iconPanel.Location = isRtl ? new Point(15, 25) : new Point(this.Width - 70, 25);
            };
            
            
            // RightToLeft changed: reposition everything
            this.RightToLeftChanged += (s, e) => RepositionForRTL();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RepositionForRTL();
        }

        private void RepositionForRTL()
        {
            // Guard: may be called before InitializeControls finishes
            if (_iconPanel == null || _lblTitle == null || _lblValue == null || _lblSubtitle == null) return;

            // Use IsArabic as the source of truth — the inherited RightToLeft property
            // may not have propagated yet when OnResize fires during initialization.
            bool isRtl = Helpers.LocalizationManager.IsArabic;
            
            // Icon Position
            _iconPanel.Location = isRtl ? new Point(15, 25) : new Point(this.Width - 70, 25);
            
            // X-Coordinates and Sizing
            int labelX, labelWidth;
            if (isRtl)
            {
                // In RTL: icon is on the LEFT (15–65). Labels span from 75 to (Width - 15).
                labelX    = 75;
                labelWidth = Math.Max(0, this.Width - labelX - 15);
                
                _lblTitle.AutoSize    = false;
                _lblValue.AutoSize    = false;
                _lblSubtitle.AutoSize = false;
                
                _lblTitle.Size    = new Size(labelWidth, 25);
                _lblValue.Size    = new Size(labelWidth, 45);
                _lblSubtitle.Size = new Size(labelWidth, 20);
            }
            else
            {
                // In LTR: icon is on the RIGHT (Width-70). Labels start at 20.
                labelX    = 20;
                labelWidth = this.Width - 90;
                
                _lblTitle.AutoSize    = true;
                _lblValue.AutoSize    = true;
                _lblSubtitle.AutoSize = true;
            }

            // Apply positions
            _lblTitle.Location    = new Point(labelX, 15);
            _lblValue.Location    = new Point(labelX - 2, 40);
            _lblSubtitle.Location = new Point(labelX + 2, 85);

            // ── TEXT ALIGNMENT ─────────────────────────────────────────────────────
            // WinForms mirrors ContentAlignment when a Label's RightToLeft == Yes.
            //   MiddleRight + RightToLeft.Yes → renders on the LEFT  (wrong)
            //   MiddleRight + RightToLeft.No  → renders on the RIGHT (correct)
            // We pin each label to RightToLeft.No so ContentAlignment.MiddleRight
            // always means "right edge of the label box" regardless of the parent
            // form's RTL setting.
            _lblTitle.RightToLeft    = RightToLeft.No;
            _lblValue.RightToLeft    = RightToLeft.No;
            _lblSubtitle.RightToLeft = RightToLeft.No;

            _lblTitle.TextAlign    = isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            _lblValue.TextAlign    = isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            _lblSubtitle.TextAlign = isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
        }

        // Call this after setting Value/Title to ensure labels move if their width changed
        public void FinalizeLayout()
        {
            RepositionForRTL();
        }

        private void UpdateTheme()
        {
            // if (_lblIcon != null) _lblIcon.ForeColor = _themeColor; // No longer needed
            if (_iconPanel != null) _iconPanel.Invalidate(); // Repaint background and icon
        }

        private void IconPanel_Paint(object sender, PaintEventArgs e)
        {
            // Draw Circle Background with High Quality
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            /* Circle background removed per user request
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(40, _themeColor))) // 40/255 Opacity
            {
                e.Graphics.FillEllipse(brush, 0, 0, _iconPanel.Width - 1, _iconPanel.Height - 1);
            }
            */

            // Draw Icon Image
            if (this.IconImage != null)
            {
                Rectangle imgRect = new Rectangle(IconPadding, IconPadding, _iconPanel.Width - (IconPadding * 2), _iconPanel.Height - (IconPadding * 2));
                e.Graphics.DrawImage(this.IconImage, imgRect);
            }
            // Fallback to text icon (emoji)
            else if (!string.IsNullOrEmpty(this.Icon))
            {
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    using (Brush brush = new SolidBrush(_themeColor))
                    {
                        RectangleF drawingRect = _iconPanel.ClientRectangle;
                        
                        // Targeted fix for Alert Icon which often visually renders lower than other emojis
                        if (this.Icon == "⚠️") 
                        {
                            drawingRect.Offset(0, -2); // Nudge up
                        }

                        // DrawString (GDI+)
                        e.Graphics.DrawString(this.Icon, new Font("Segoe UI Emoji", 18), brush, drawingRect, sf);
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            // 1. Clear background with parent color to solve "white corners bug"
            Color parentColor = ThemeConfig.GetParentColor(this);
            using (var brush = new SolidBrush(parentColor))
            {
                // Slightly larger rectangle to ensure no artifacts at edges/corners
                e.Graphics.FillRectangle(brush, -1, -1, this.Width + 2, this.Height + 2);
            }

            Rectangle r = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (var path = GetRoundedRect(r, 15))
            {
                // 2. Fill rounded card with surface color
                using (var brush = new SolidBrush(ThemeConfig.SurfaceColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // 3. Draw border
                using (var pen = new Pen(ThemeConfig.BorderColor, 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
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


