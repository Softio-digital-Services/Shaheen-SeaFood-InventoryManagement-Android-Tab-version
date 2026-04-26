using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text.Json;
using System.IO;
using System.Drawing.Drawing2D;

namespace GenericInventorySystem
{
    /// <summary>
    /// Centralized configuration for UI Theming and Branding.
    /// Updated to "Horizon UI" inspired Light Theme.
    /// </summary>
    public static class ThemeConfig
    {
        // ==========================================
        // BRANDING
        // ==========================================
        public static string CompanyName { get; set; } = "Generic Solutions";
        public static string AppTitle { get; set; } = "Generic POS Engine";

        static ThemeConfig()
        {
            try
            {
                string configPath = "appsettings.json";
                if (File.Exists(configPath))
                {
                    string jsonString = File.ReadAllText(configPath);
                    using (JsonDocument doc = JsonDocument.Parse(jsonString))
                    {
                        if (doc.RootElement.TryGetProperty("SystemBranding", out JsonElement branding))
                        {
                            if (branding.TryGetProperty("CompanyName", out JsonElement compName))
                                CompanyName = compName.GetString() ?? CompanyName;
                            
                            if (branding.TryGetProperty("AppName", out JsonElement appName))
                                AppTitle = appName.GetString() ?? AppTitle;
                        }
                    }
                }
            }
            catch { /* Fallback to default generic names */ }
        }

        // ==========================================
        // COLOR PALETTE (Light / Horizon Blue)
        // ==========================================
        
        // Primary Brand Color (Royal Blue)
        // Primary Brand Color (Royal Blue - Horizon Standard)
        public static Color PrimaryColor { get; } = Color.FromArgb(25, 118, 210); // Deep Blue
        public static Color PrimaryHoverColor { get; } = Color.FromArgb(13, 71, 161);

        // Gradient Colors for Primary Buttons
        public static Color GradientStart { get; } = Color.FromArgb(25, 118, 210); 
        public static Color GradientEnd { get; } = Color.FromArgb(13, 71, 161);   

        // Secondary / Text Colors
        public static Color SecondaryColor { get; } = Color.FromArgb(100, 116, 139); // Slate Gray
        public static Color SecondaryHoverColor { get; } = Color.FromArgb(210, 210, 220); 
        public static Color TextColorDark { get; } = Color.FromArgb(15, 23, 42);    
        public static Color TextColorLight { get; } = Color.White;
        public static Color TextColorWhite { get; } = Color.White;

        // Backgrounds
        public static Color BackgroundColor { get; } = Color.FromArgb(241, 245, 249); 
        public static Color SidebarColor { get; } = Color.FromArgb(248, 250, 252);     
        public static Color HeaderColor { get; } = Color.FromArgb(25, 118, 210);      
        public static Color ActiveBackColor { get; } = Color.FromArgb(232, 240, 254); 
        
        // Semantic Token Mapping
        public static Color SelectionBackColor { get; } = Color.FromArgb(237, 242, 247); // Light Gray-Blue selection
        public static Color BorderColor { get; } = Color.FromArgb(226, 232, 240); // Standard Border Color (Slate-200)
        public static Color MutedTextColor { get; } = Color.FromArgb(160, 174, 192); // Cool Gray for subtitles/muted text
        public static Color GridHeaderBgColor { get; } = Color.FromArgb(248, 250, 252); // Light Gray #F8FAFC

        // Status Colors
        public static Color SuccessColor { get; } = Color.FromArgb(5, 205, 153); // Green #05CD99
        public static Color SuccessLight { get; } = Color.FromArgb(230, 255, 250); // Light Green

        public static Color DangerColor { get; } = Color.FromArgb(238, 93, 80);  // Red #EE5D50 (Main Theme Red)
        public static Color DangerColorBright { get; } = Color.FromArgb(239, 68, 68); // Red-500 for clear visibility
        public static Color DangerLight { get; } = Color.FromArgb(255, 240, 240); // Light Red

        public static Color WarningColor { get; } = Color.FromArgb(255, 181, 71); // Orange #FFB547
        public static Color WarningLight { get; } = Color.FromArgb(255, 250, 235); // Light Orange
        
        // Status Badge Palette (Horizon UI / Tailwind inspired)
        public static Color SuccessBadgeBg { get; } = Color.FromArgb(240, 253, 244); // Light Green
        public static Color SuccessBadgeText { get; } = Color.FromArgb(21, 128, 61); // Dark Green
        public static Color SuccessBorder { get; } = Color.FromArgb(34, 197, 94);   // Green-500

        public static Color DangerBadgeBg { get; } = Color.FromArgb(254, 242, 242);  // Light Red
        public static Color DangerBadgeText { get; } = Color.FromArgb(185, 28, 28);  // Dark Red
        public static Color DangerBorder { get; } = Color.FromArgb(239, 68, 68);    // Red-500

        public static Color WarningBadgeBg { get; } = Color.FromArgb(255, 251, 235); // Light Yellow
        public static Color WarningBadgeText { get; } = Color.FromArgb(180, 83, 9);   // Dark Orange
        public static Color WarningBorder { get; } = Color.FromArgb(245, 158, 11);  // Orange-500

        public static Color InfoBadgeBg { get; } = Color.FromArgb(239, 246, 255);    // Light Blue
        public static Color InfoBadgeText { get; } = Color.FromArgb(29, 78, 216);    // Dark Blue
        public static Color InfoBorder { get; } = Color.FromArgb(59, 130, 246);      // Blue-500
        
        // Neon / Premium Accents
        public static Color NeonBlue { get; } = Color.FromArgb(0, 245, 255); // Vibrant Neon Blue
        public static Color NeonBlueAlpha { get; } = Color.FromArgb(100, 0, 245, 255);

        // Card Surface
        public static Color SurfaceColor { get; } = Color.White;


        // ==========================================
        // FONTS
        // ==========================================
        public static Font HeaderFont { get; } = new Font("Segoe UI", 14F, FontStyle.Bold); // Larger
        public static Font SubHeaderFont { get; } = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static Font StandardFont { get; } = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static Font SmallFont { get; } = new Font("Segoe UI", 8F, FontStyle.Regular);
        public static Font ButtonFont { get; } = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static Font SmallBoldFont { get; } = new Font("Segoe UI", 9F, FontStyle.Bold);
        public static Font MicroBoldFont { get; } = new Font("Segoe UI", 8F, FontStyle.Bold);
        public static Font EmojiFont { get; } = new Font("Segoe UI Emoji", 11F);
        public static Font EmojiFontLarge { get; } = new Font("Segoe UI Emoji", 14F);
        public static Font SymbolFont { get; } = new Font("Segoe UI Symbol", 11F);

        // ==========================================
        // HELPER METHODS
        // ==========================================

        /// <summary>
        /// Creates a standardized screen title label with consistent font, color, and margin.
        /// </summary>
        public static Label CreateStandardHeader(string text)
        {
            return new Label
            {
                Text = text,
                Font = HeaderFont,
                ForeColor = PrimaryColor,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0)
            };
        }

        public static void ApplyPrimaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = PrimaryColor;
            btn.ForeColor = TextColorLight;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            
            // Hover Effects
            btn.MouseEnter += (s, e) => btn.BackColor = PrimaryHoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = PrimaryColor;

            // Attach Rounded Painter
            btn.Paint -= Btn_PaintRounded; 
            btn.Paint += Btn_PaintRounded;
        }

        public static Color GetParentColor(Control ctrl)
        {
            Control p = ctrl.Parent;
            while (p != null && (p.BackColor == Color.Transparent || p.BackColor.A == 0))
                p = p.Parent;
            return p?.BackColor ?? BackgroundColor;
        }

        public static void DrawIconButton(Button btn, Graphics g, string iconName, string localizationKey, Color textColor, Color accentColor, bool isOutline)
        {
            if (btn == null) return;
            bool isArabic = GenericInventorySystem.Helpers.LocalizationManager.IsArabic;
            string text = GenericInventorySystem.Helpers.LocalizationManager.GetString(localizationKey);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);

            // Handle background clearing (to prevent artifacts from parent)
            using (var pb = new SolidBrush(GetParentColor(btn)))
                g.FillRectangle(pb, new Rectangle(0, 0, btn.Width, btn.Height));

            using (var path = GetRoundedPath(r, 8))
            {
                if (isOutline)
                {
                    using (Pen pen = new Pen(accentColor, 1.5f))
                        g.DrawPath(pen, path);
                }
                else
                {
                    using (SolidBrush brush = new SolidBrush(accentColor))
                        g.FillPath(brush, path);
                }
            }

            Image img = GetNuricon(iconName);
            int iconSize = 24;
            int margin = 8;
            
            // Icon Position
            int iconX = isArabic ? (btn.Width - iconSize - margin) : margin;
            int iconY = (btn.Height - iconSize) / 2;
            if (img != null) g.DrawImage(img, new Rectangle(iconX, iconY, iconSize, iconSize));

            // Text Position (Centered in the remaining area)
            int textX = isArabic ? margin : (iconX + iconSize + 4);
            int textW = btn.Width - iconSize - (margin * 2) - 4;
            Rectangle textRect = new Rectangle(textX, 0, textW, btn.Height);

            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding;
            if (isArabic) flags |= TextFormatFlags.RightToLeft;

            TextRenderer.DrawText(g, text, btn.Font, textRect, textColor, flags);
        }

        public static void ApplyComboBoxStyle(ComboBox cbo)
        {
            cbo.FlatStyle = FlatStyle.Flat;
            cbo.Font = StandardFont;
            cbo.BackColor = SurfaceColor;
            cbo.ForeColor = TextColorDark;
            cbo.Cursor = Cursors.Hand;
            
            // Standardizing DropDownStyle removed to preserve typability and UI.
            // if (cbo.DropDownStyle == ComboBoxStyle.DropDown)
            //    cbo.DropDownStyle = ComboBoxStyle.DropDownList;
            // but we can ensure the colors and fonts are perfect.
        }

        /// <summary>
        /// Wraps any control in a Panel that draws a consistent rounded border.
        /// Useful for ComboBox and DateTimePicker which don't support borders in Flat mode easily.
        /// </summary>
        public static Panel WrapInStyledInput(Control innerControl, int height, bool isMultiline = false)
        {
            Panel p = new Panel
            {
                Size      = new Size(200, height),
                BackColor = Color.White
            };

            innerControl.Dock = DockStyle.None;
            void positionControl() {
                if (innerControl == null) return;
                innerControl.Width = p.Width - 20;
                if (!isMultiline)
                    innerControl.Location = new Point(10, (p.Height - innerControl.Height) / 2);
                else {
                    innerControl.Location = new Point(10, 10);
                    innerControl.Height = p.Height - 20;
                }
            }

            p.Resize += (s, e) => {
                positionControl();
                using (var path = GetRoundedPath(new Rectangle(0, 0, p.Width, p.Height), 12))
                {
                    p.Region = new Region(path);
                }
                p.Invalidate();
            };

            // Initial setup
            positionControl();
            using (var path = GetRoundedPath(new Rectangle(0, 0, p.Width, p.Height), 12))
            {
                p.Region = new Region(path);
            }

            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                // Draw border inset by 1px
                using (var path = GetRoundedPath(new Rectangle(0, 0, p.Width - 1, p.Height - 1), 12))
                using (var pen = new Pen(BorderColor, 1.5f))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            };

            p.Controls.Add(innerControl);
            return p;
        }

        // ==========================================
        // CHARTING / ANALYTICS
        // ==========================================
        public static readonly Color[] ChartPalette = new Color[]
        {
            PrimaryColor,
            SuccessColor,
            WarningColor,
            DangerColor,
            SecondaryColor,
            Color.FromArgb(99, 102, 241), // Indigo
            Color.FromArgb(168, 85, 247), // Purple
            Color.FromArgb(236, 72, 153)  // Pink
        };

        public static void ApplyChartTheme(Chart chart)
        {
            chart.BackColor = SurfaceColor;
            chart.AntiAliasing = AntiAliasingStyles.All;
            chart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

            chart.ChartAreas.Clear();
            var area = chart.ChartAreas.Add("MainArea");
            area.BackColor = Color.Transparent;
            area.BorderWidth = 0;

            // Axis Styling
            area.AxisX.LabelStyle.Font = StandardFont;
            area.AxisY.LabelStyle.Font = StandardFont;
            area.AxisX.LabelStyle.ForeColor = SecondaryColor;
            area.AxisY.LabelStyle.ForeColor = SecondaryColor;

            area.AxisX.LineColor = Color.FromArgb(230, 230, 230);
            area.AxisY.LineColor = Color.Transparent;

            area.AxisX.MajorGrid.LineColor = Color.FromArgb(245, 245, 245);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(245, 245, 245);
            area.AxisX.MajorGrid.Enabled = false; // Usually cleaner without vertical lines

            // Title Styling
            foreach (var title in chart.Titles)
            {
                title.Font = SubHeaderFont;
                title.ForeColor = TextColorDark;
                title.Alignment = ContentAlignment.TopLeft;
                title.Docking = Docking.Top;
            }

            // Legend Styling
            chart.Legends.Clear();
            var legend = chart.Legends.Add("Default");
            legend.BackColor = Color.Transparent;
            legend.Font = StandardFont;
            legend.ForeColor = SecondaryColor;
            legend.Docking = Docking.Bottom;
            legend.Alignment = StringAlignment.Center;
        }

        /// <summary>
        /// Like ApplyPrimaryButton, but correctly renders emoji + text by using split-draw:
        /// the first space-delimited token is drawn with EmojiFont,
        /// the rest is drawn with SmallBoldFont. Call after setting BackColor.
        /// </summary>
        public static void ApplyEmojiButton(Button btn, Color backColor, Color hoverColor, Color textColor)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = backColor;
            btn.ForeColor = textColor;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;

            btn.Paint -= Btn_PaintEmojiButton;
            btn.Paint += Btn_PaintEmojiButton;
        }

        public static void ApplyHeaderIconStyle(Control c)
        {
            c.Cursor = Cursors.Hand;
            c.BackColor = Color.Transparent;
            
            c.MouseEnter += (s, e) => { c.Tag = true; c.Invalidate(); };
            c.MouseLeave += (s, e) => { c.Tag = false; c.Invalidate(); };
            
            if (c is PictureBox pb)
            {
                Image icon = pb.Image;
                pb.Image = null; // Prevent double-drawing (ghosting)

                pb.Paint += (s, e) => {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    if (pb.Tag != null && (bool)pb.Tag)
                    {
                        Rectangle r = new Rectangle(0, 0, pb.Width - 1, pb.Height - 1);
                        using (var path = GetRoundedPath(r, 6))
                        using (var b = new SolidBrush(Color.FromArgb(40, 255, 255, 255))) e.Graphics.FillPath(b, path);
                    }
                    
                    if (icon != null)
                    {
                        float ratio = Math.Min((float)pb.Width / icon.Width, (float)pb.Height / icon.Height) * 0.7f;
                        int nw = (int)(icon.Width * ratio), nh = (int)(icon.Height * ratio);
                        e.Graphics.DrawImage(icon, (pb.Width - nw) / 2, (pb.Height - nh) / 2, nw, nh);
                    }
                };
            }
        }

        public static void ApplyWindowControl(Button btn, string type)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.Text = string.Empty; // We draw everything via Paint
            btn.Cursor = Cursors.Default;
            btn.TabStop = false;

            // Remove existing paint handlers to prevent duplication
            btn.Paint -= WinCtrl_PaintMinimize;
            btn.Paint -= WinCtrl_PaintMaximize;
            btn.Paint -= WinCtrl_PaintClose;

            bool isDarkHeader = (btn.FindForm() is MainForm);
            Color defaultHover = isDarkHeader ? Color.FromArgb(40, 255, 255, 255) : Color.FromArgb(30, PrimaryColor);
            Color closeHover = DangerColor; // Consistent red hover for close

            if (type == "Close")
            {
                btn.Paint += WinCtrl_PaintClose;
                btn.MouseEnter += (s, e) => { btn.BackColor = closeHover; btn.Invalidate(); };
                btn.MouseLeave += (s, e) => { btn.BackColor = Color.Transparent; btn.Invalidate(); };
            }
            else if (type == "Maximize" || type == "Restore" || type == "Minimize")
            {
                if (type == "Minimize") btn.Paint += WinCtrl_PaintMinimize;
                else btn.Paint += WinCtrl_PaintMaximize;
                btn.MouseEnter += (s, e) => { btn.BackColor = defaultHover; btn.Invalidate(); };
                btn.MouseLeave += (s, e) => { btn.BackColor = Color.Transparent; btn.Invalidate(); };
            }
        }

        private static void WinCtrl_PaintMinimize(object sender, PaintEventArgs e)
        {
            var btn = sender as Button; if (btn == null) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (btn.BackColor != Color.Transparent)
            {
                Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (var path = GetRoundedPath(r, 6))
                using (var b = new SolidBrush(btn.BackColor)) g.FillPath(b, path);
            }
            int cx = btn.Width / 2, cy = btn.Height / 2;
            int lineW = 6;
            bool isDarkHeader = (btn.FindForm() is MainForm);
            Color iconColor = isDarkHeader ? Color.White : ThemeConfig.TextColorDark;
            using (var p = new Pen(iconColor, 1.5f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
                g.DrawLine(p, cx - lineW, cy + 2, cx + lineW, cy + 2);
        }


        private static void WinCtrl_PaintMaximize(object sender, PaintEventArgs e)
        {
            var btn = sender as Button; if (btn == null) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (btn.BackColor != Color.Transparent)
            {
                Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (var path = GetRoundedPath(r, 6))
                using (var b = new SolidBrush(btn.BackColor)) g.FillPath(b, path);
            }
            int cx = btn.Width / 2, cy = btn.Height / 2;
            bool isRestore = (btn.Tag as string) == "Restore";
            bool isDarkHeader = (btn.FindForm() is MainForm);
            Color iconColor = isDarkHeader ? Color.White : ThemeConfig.TextColorDark;
            using (var p = new Pen(iconColor, 1.5f))
            {
                if (isRestore)
                {
                    g.DrawRectangle(p, cx - 4, cy - 2, 7, 6);
                    g.DrawRectangle(p, cx - 2, cy - 4, 7, 6);
                }
                else
                {
                    g.DrawRectangle(p, cx - 5, cy - 4, 10, 8);
                }
            }
        }

        private static void WinCtrl_PaintClose(object sender, PaintEventArgs e)
        {
            var btn = sender as Button; if (btn == null) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (btn.BackColor != Color.Transparent)
            {
                Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (var path = GetRoundedPath(r, 6))
                using (var b = new SolidBrush(btn.BackColor)) 
                    g.FillPath(b, path);
            }

            int cx = btn.Width / 2, cy = btn.Height / 2;
            int s = 4; 
            bool isDarkHeader = (btn.FindForm() is MainForm);
            Color iconColor = isDarkHeader ? Color.White : ThemeConfig.TextColorDark;
            
            // If hovered (DangerColor background), force white icon for visibility
            if (btn.BackColor == DangerColor) iconColor = Color.White;
            
            using (var p = new Pen(iconColor, 1.5f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
            {
                g.DrawLine(p, cx - s, cy - s, cx + s, cy + s);
                g.DrawLine(p, cx + s, cy - s, cx - s, cy + s);
            }
        }

        private static void Btn_PaintEmojiButton(object sender, PaintEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 1. Clear background with parent color
            Rectangle r = new Rectangle(0, 0, btn.Width, btn.Height);
            if (btn.Parent != null)
                using (var pb = new SolidBrush(btn.Parent.BackColor))
                    g.FillRectangle(pb, r);

            // 2. Draw rounded background
            using (var path = GetRoundedPath(r, 8))
            using (var brush = new SolidBrush(btn.BackColor))
                g.FillPath(brush, path);

            // 3. Split text: first word = emoji, rest = label
            string full = btn.Text ?? string.Empty;
            int spaceIdx = full.IndexOf(' ');
            string emojiPart = spaceIdx > 0 ? full.Substring(0, spaceIdx) : full;
            string labelPart = spaceIdx > 0 ? full.Substring(spaceIdx + 1) : string.Empty;

            // 4. Measure to center
            Size emojiSize = TextRenderer.MeasureText(emojiPart, EmojiFont);
            Size labelSize = TextRenderer.MeasureText(labelPart, SmallBoldFont);
            int totalWidth = emojiSize.Width + labelSize.Width - 8; // -8 for GDI spacing overshoot
            int startX = (btn.Width - totalWidth) / 2;
            int centerY = (btn.Height - emojiSize.Height) / 2;

            // 5. Draw emoji
            TextRenderer.DrawText(g, emojiPart, EmojiFont,
                new Rectangle(startX, 0, emojiSize.Width, btn.Height),
                btn.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);

            // 6. Draw label text
            if (!string.IsNullOrEmpty(labelPart))
                TextRenderer.DrawText(g, labelPart, SmallBoldFont,
                    new Rectangle(startX + emojiSize.Width - 6, 0, labelSize.Width + 8, btn.Height),
                    btn.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
        }

        public static void DrawRoundedButton(Button btn, Graphics g)
        {
            if (btn == null) return;
            
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            Rectangle r = new Rectangle(0, 0, btn.Width, btn.Height);
            using (var path = GetRoundedPath(new Rectangle(0, 0, btn.Width - 1, btn.Height - 1), 8)) // 8px Radius
            using (var brush = new SolidBrush(btn.BackColor))
            {
                // Clear background artifact (Paint parent background)
                using (var parentBrush = new SolidBrush(GetParentColor(btn)))
                    g.FillRectangle(parentBrush, r);
                
                // Fill Rounded
                g.FillPath(brush, path);
                
                // Subtle Glowing Edge / Highlight
                using (var glowPen = new Pen(Color.FromArgb(80, Color.White), 1.2f))
                    g.DrawPath(glowPen, path);
                
                // Text Alignment & Format
                StringFormat sf = new StringFormat();
                sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
                
                // Horizontal
                switch (btn.TextAlign)
                {
                    case ContentAlignment.TopLeft:
                    case ContentAlignment.MiddleLeft:
                    case ContentAlignment.BottomLeft:
                        sf.Alignment = StringAlignment.Near;
                        break;
                    case ContentAlignment.TopCenter:
                    case ContentAlignment.MiddleCenter:
                    case ContentAlignment.BottomCenter:
                        sf.Alignment = StringAlignment.Center;
                        break;
                    case ContentAlignment.TopRight:
                    case ContentAlignment.MiddleRight:
                    case ContentAlignment.BottomRight:
                        sf.Alignment = StringAlignment.Far;
                        break;
                }
                
                // Vertical
                switch (btn.TextAlign)
                {
                    case ContentAlignment.TopLeft:
                    case ContentAlignment.TopCenter:
                    case ContentAlignment.TopRight:
                        sf.LineAlignment = StringAlignment.Near;
                        break;
                    case ContentAlignment.MiddleLeft:
                    case ContentAlignment.MiddleCenter:
                    case ContentAlignment.MiddleRight:
                        sf.LineAlignment = StringAlignment.Center;
                        break;
                    case ContentAlignment.BottomLeft:
                    case ContentAlignment.BottomCenter:
                    case ContentAlignment.BottomRight:
                        sf.LineAlignment = StringAlignment.Far;
                        break;
                }

                // Rect with Padding
                RectangleF contentRect = new RectangleF(
                    r.X + btn.Padding.Left,
                    r.Y + btn.Padding.Top,
                    r.Width - (btn.Padding.Right + btn.Padding.Left),
                    r.Height - (btn.Padding.Bottom + btn.Padding.Top));

                // Support Icons
                if (btn.Image != null)
                {
                    int iconSize = (int)(btn.Height * 0.6f);
                    int iconX = (int)contentRect.X;
                    int iconY = (int)(contentRect.Y + (contentRect.Height - iconSize) / 2);

                    if (btn.TextImageRelation == TextImageRelation.ImageBeforeText)
                    {
                        g.DrawImage(btn.Image, new Rectangle(iconX, iconY, iconSize, iconSize));
                        contentRect.X += iconSize + 4;
                        contentRect.Width -= iconSize + 4;
                    }
                    else if (btn.TextImageRelation == TextImageRelation.Overlay || btn.TextAlign == ContentAlignment.MiddleCenter)
                    {
                        // Draw centered or overlay
                        g.DrawImage(btn.Image, new Rectangle((int)(r.X + (r.Width - iconSize)/2), iconY, iconSize, iconSize));
                    }
                }

                // Draw Text using TextRenderer for better compatibility and RTL support
                TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
                if (GenericInventorySystem.Helpers.LocalizationManager.IsArabic)
                    flags |= TextFormatFlags.RightToLeft;

                Rectangle textRect = Rectangle.Round(contentRect);
                TextRenderer.DrawText(g, btn.Text, btn.Font, textRect, btn.ForeColor, flags);
            }
        }

        private static void Btn_PaintRounded(object sender, PaintEventArgs e)
        {
             DrawRoundedButton(sender as Button, e.Graphics);
        }

        private static System.Drawing.Drawing2D.GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            // Offset for border drawing if needed suitable for filling
            // For buttons, we want exact fill.
            
            int d = radius * 2;
            Rectangle r = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height); // Full Size

            // To prevent cut-off at edges, we might start a bit inside, 
            // but for a button, we typically want full paint.
            // However, GraphicsPath adding arcs needs care with right/bottom edges.
            // Correct logic for exact sizing:
            
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static GraphicsPath GetRoundedPathPublic(Rectangle rect, int radius)
        {
            return GetRoundedPath(rect, radius);
        }

        public static void ApplyDangerButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = DangerColor;
            btn.ForeColor = TextColorLight;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            
            // Hover Effects
            btn.MouseEnter += (s, e) => btn.BackColor = DangerColorBright;
            btn.MouseLeave += (s, e) => btn.BackColor = DangerColor;

            btn.Paint -= Btn_PaintRounded;
            btn.Paint += Btn_PaintRounded;
        }

        public static void ApplySecondaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.FromArgb(230, 230, 240); // Light Gray
            btn.ForeColor = TextColorDark;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            
            // Hover Effects
            btn.MouseEnter += (s, e) => btn.BackColor = SecondaryHoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(230, 230, 240);

            btn.Paint -= Btn_PaintRounded; 
            btn.Paint += Btn_PaintRounded;
        }

        public static void ApplyGridTheme(DataGridView grid)
        {
            grid.BackgroundColor = SurfaceColor;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.FromArgb(230, 230, 230);
            
            // Header
            // Header
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252); // Light Gray #F8FAFC
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextColorDark; // Navy Dark to match project
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SelectionBackColor; // Light Gray-Blue to match app selections
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextColorDark; 
            grid.ColumnHeadersDefaultCellStyle.Font = SmallBoldFont; // Reduced size
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None; // Cleaner
            grid.ColumnHeadersHeight = 45; // Slightly reduced height
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Rows
            grid.DefaultCellStyle.BackColor = SurfaceColor;
            grid.DefaultCellStyle.ForeColor = TextColorDark;
            grid.DefaultCellStyle.Font = StandardFont; 
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            // Modern Selection: Light Blue background with Dark Text (Like Horizon/Tailwind tables)
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(237, 242, 247); // Light Gray-Blue
            grid.DefaultCellStyle.SelectionForeColor = TextColorDark;
            
            grid.Padding = new Padding(12, 5, 5, 5); // More breathing room
            
            // Prevent Checkbox Columns from sorting (which wipes unbound states)
            // And ensure they are perfectly centered
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col is DataGridViewCheckBoxColumn)
                {
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
            grid.ColumnAdded += (s, e) =>
            {
                if (e.Column is DataGridViewCheckBoxColumn)
                {
                    e.Column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    e.Column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    e.Column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            };

            grid.RowHeadersVisible = false;
            grid.RowTemplate.Height = 60; // Standardized taller rows
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.White; // Clean look
            
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AllowUserToResizeRows = false;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(240, 240, 240); // Very subtle lines

            // Universal Checkbox "Select Row" Functionality
            grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (grid.IsCurrentCellDirty && grid.CurrentCell is DataGridViewCheckBoxCell)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };

            grid.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns.Contains("colCheck"))
                {
                    // Ignore clicks on action columns
                    if (grid.Columns[e.ColumnIndex].Name == "colActions") return;
                    
                    // If they clicked directly on the checkbox column, let native behavior handle it but commit immediately
                    if (grid.Columns[e.ColumnIndex].Name == "colCheck")
                    {
                         grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    }
                    else
                    {
                        // Clicked somewhere else on the row, manually toggle the checkbox
                        var cell = grid.Rows[e.RowIndex].Cells["colCheck"] as DataGridViewCheckBoxCell;
                        if (cell != null && !cell.ReadOnly)
                        {
                            bool currentVal = Convert.ToBoolean(cell.Value ?? false);
                            cell.Value = !currentVal;
                            grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                            grid.InvalidateCell(cell);
                        }
                    }
                }
            };

            grid.ColumnHeaderMouseClick += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "colCheck")
                {
                    grid.EndEdit();
                    if (grid.Rows.Count > 0)
                    {
                        bool allChecked = true;
                        foreach (DataGridViewRow row in grid.Rows)
                        {
                            var cell = row.Cells["colCheck"] as DataGridViewCheckBoxCell;
                            if (cell == null || !Convert.ToBoolean(cell.Value ?? false))
                            {
                                allChecked = false;
                                break;
                            }
                        }
                        
                        bool newState = !allChecked;
                        foreach (DataGridViewRow row in grid.Rows)
                        {
                            var cell = row.Cells["colCheck"] as DataGridViewCheckBoxCell;
                            if (cell != null && !cell.ReadOnly)
                            {
                                cell.Value = newState;
                            }
                        }
                        grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                        grid.Refresh();
                    }
                }
            };
            
            grid.Visible = true;
        }

        public static Image TintImage(Image source, Color tintColor)
        {
            if (source == null) return null;
            Bitmap bmp = new Bitmap(source.Width, source.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                var cm = new System.Drawing.Imaging.ColorMatrix(new float[][]
                {
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {tintColor.R/255f, tintColor.G/255f, tintColor.B/255f, 0, 1}
                });
                var attributes = new System.Drawing.Imaging.ImageAttributes();
                attributes.SetColorMatrix(cm);
                g.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
            }
            return bmp;
        }

        public static void ApplySidebarButton(Button btn, bool isActive)
        {
             btn.FlatStyle = FlatStyle.Flat;
             btn.FlatAppearance.BorderSize = 0;
             btn.BackColor = isActive ? ActiveBackColor : Color.Transparent; 
             btn.ForeColor = isActive ? PrimaryColor : Color.FromArgb(31, 41, 55); // Darker gray for inactive
             btn.Font = new Font("Segoe UI", 10F, isActive ? FontStyle.Bold : FontStyle.Regular);
             btn.Cursor = Cursors.Hand;
             btn.TextAlign = ContentAlignment.MiddleLeft;
             
             // Hover and Click Effects
             btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(244, 247, 254); // Match app background
             btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 235, 245);
             btn.FlatStyle = FlatStyle.Flat;
             btn.FlatAppearance.BorderSize = 0;
             
             // Remove gradient usage if previously attached
             // btn.Paint -= Btn_PaintGradient; // Method deleted
        }

        public static void ApplySidebarButtonIcon(Button btn, Image icon, bool isActive)
        {
            ApplySidebarButton(btn, isActive);
            btn.Image = icon;
            btn.ImageAlign = ContentAlignment.MiddleLeft; // Default for LTR
            btn.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn.Padding = new Padding(12, 0, 0, 0); // Standard padding for icon
        }

        public static Image GetNuricon(string name)
        {
            try
            {
                string filename = $"nuricon_{name}.png";
                
                // 1. Try StartupPath/Assets (Standard)
                string path = System.IO.Path.Combine(Application.StartupPath, "Assets", filename);
                Image img = null;
                if (System.IO.File.Exists(path)) img = Image.FromFile(path);

                // 2. Try StartupPath (Flat)
                if (img == null)
                {
                    path = System.IO.Path.Combine(Application.StartupPath, filename);
                    if (System.IO.File.Exists(path)) img = Image.FromFile(path);
                }

                // 3. Try climbing up for Dev Environment (bin/Debug/netX.X -> Assets)
                if (img == null)
                {
                    string currentDir = Application.StartupPath;
                    for (int i = 0; i < 4; i++) // Up to 4 levels
                    {
                        string checkPath = System.IO.Path.Combine(currentDir, "Assets", filename);
                        if (System.IO.File.Exists(checkPath)) { img = Image.FromFile(checkPath); break; }
                        
                        var parent = System.IO.Directory.GetParent(currentDir);
                        if (parent == null) break;
                        currentDir = parent.FullName;
                    }
                }

                if (img != null)
                {
                    // Ensure transparency even if generated with white background
                    Bitmap bmp = new Bitmap(img);
                    bmp.MakeTransparent(Color.White);
                    return bmp;
                }

                // 4. Fallback: Procedural generation if file missing
                return GenerateNuriconFallback(name);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Nuricon '{name}': {ex.Message}");
                return GenerateNuriconFallback(name); // Try fallback even on error
            }
        }

        private static Image GenerateNuriconFallback(string name)
        {
            try 
            {
                Bitmap bmp = new Bitmap(64, 64);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    // Default Gradient (Blue to Purple)
                    Color c1 = Color.FromArgb(37, 99, 235); 
                    Color c2 = Color.FromArgb(147, 51, 234); 

                    // Specific colors based on icon type
                    if (name.Contains("import")) { c1 = Color.FromArgb(22, 163, 74); c2 = Color.FromArgb(20, 184, 166); } 
                    else if (name.Contains("export")) { c1 = Color.FromArgb(37, 99, 235); c2 = Color.FromArgb(6, 182, 212); } 
                    else if (name.Contains("filter")) { c1 = Color.FromArgb(249, 115, 22); c2 = Color.FromArgb(236, 72, 153); }
                    else if (name.Contains("search")) { c1 = Color.FromArgb(79, 70, 229); c2 = Color.FromArgb(124, 58, 237); }
                    else if (name.Contains("orders")) { c1 = Color.FromArgb(217, 70, 239); c2 = Color.FromArgb(168, 85, 247); }
                    else if (name.Contains("revenue") || name.Contains("sales")) { c1 = Color.FromArgb(245, 158, 11); c2 = Color.FromArgb(252, 211, 77); }
                    else if (name == "warning") { c1 = Color.FromArgb(245, 158, 11); c2 = Color.FromArgb(251, 191, 36); }
                    else if (name == "info") { c1 = Color.FromArgb(59, 130, 246); c2 = Color.FromArgb(96, 165, 250); }
                    else if (name == "check" || name == "success") { c1 = Color.FromArgb(16, 185, 129); c2 = Color.FromArgb(52, 211, 153); }
                    else if (name == "add" || name == "plus") { c1 = Color.FromArgb(59, 130, 246); c2 = Color.FromArgb(147, 51, 234); }
                    else if (name == "pos") { c1 = Color.FromArgb(5, 205, 153); c2 = Color.FromArgb(20, 184, 166); }
                    else if (name == "inventory") { c1 = Color.FromArgb(99, 102, 241); c2 = Color.FromArgb(168, 85, 247); } // Indigo to Purple
                    else if (name == "customers") { c1 = Color.FromArgb(59, 130, 246); c2 = Color.FromArgb(37, 99, 235); } // Blue to Dark Blue
                    else if (name == "suppliers") { c1 = Color.FromArgb(20, 184, 166); c2 = Color.FromArgb(5, 150, 105); } // Teal to Green
                    else if (name == "reports") { c1 = Color.FromArgb(16, 185, 129); c2 = Color.FromArgb(5, 150, 105); } // Emerald
                    else if (name == "history" || name == "expenses") { c1 = Color.FromArgb(244, 63, 94); c2 = Color.FromArgb(225, 29, 72); } // Rose to Crimson
                    else if (name == "quotations") { c1 = Color.FromArgb(14, 165, 233); c2 = Color.FromArgb(2, 132, 199); } // Sky Blue
                    else if (name == "currencies") { c1 = Color.FromArgb(245, 158, 11); c2 = Color.FromArgb(217, 119, 6); } // Amber to Orange
                    else if (name == "user") { c1 = Color.FromArgb(79, 70, 229); c2 = Color.FromArgb(67, 56, 202); } // Indigo

                    using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(8, 8, 48, 48), c1, c2, 45f))
                    {
                        if (name == "search")
                        {
                            g.DrawEllipse(new Pen(brush, 6), 12, 12, 28, 28);
                            g.DrawLine(new Pen(brush, 8) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round }, 36, 36, 52, 52);
                        }
                        else if (name == "import")
                        {
                            g.FillPolygon(brush, new Point[] { new Point(32, 50), new Point(14, 28), new Point(50, 28) });
                            g.FillRectangle(brush, 26, 8, 12, 20);
                        }
                        else if (name == "export")
                        {
                            g.FillPolygon(brush, new Point[] { new Point(32, 8), new Point(14, 30), new Point(50, 30) });
                            g.FillRectangle(brush, 26, 30, 12, 20);
                        }
                        else if (name == "filter")
                        {
                            g.FillPolygon(brush, new Point[] { new Point(8, 8), new Point(56, 8), new Point(36, 34), new Point(36, 56), new Point(28, 56), new Point(28, 34) });
                        }
                        else if (name == "view")
                        {
                            // Eye shape
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            g.DrawArc(new Pen(brush, 5), 10, 15, 44, 34, 0, -180); // Bottom arc
                            g.DrawArc(new Pen(brush, 5), 10, 15, 44, 34, 0, 180);  // Top arc
                            g.FillEllipse(brush, 24, 24, 16, 16); // Pupil
                        }
                        else if (name == "currency")
                        {
                            // Dollar sign ($)
                            using (Font f = new Font("Segoe UI", 32, FontStyle.Bold))
                            {
                                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                                g.DrawString("$", f, brush, new Rectangle(0, 0, 64, 64), sf);
                            }
                        }
                        else if (name == "orders")
                        {
                            Rectangle r = new Rectangle(0, 0, 64, 64);
                            using (var bgBrush = new System.Drawing.Drawing2D.LinearGradientBrush(r, c1, c2, 45f))
                            {
                                g.FillEllipse(bgBrush, new Rectangle(2, 2, 60, 60));
                            }
                            using (var whiteBrush = new SolidBrush(Color.White))
                            using (var whitePen = new Pen(Color.White, 4) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
                            {
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                                g.DrawLine(whitePen, 16, 20, 24, 20);
                                g.DrawLine(whitePen, 24, 20, 30, 42);
                                g.DrawLine(whitePen, 30, 42, 48, 42);
                                g.DrawLine(whitePen, 48, 42, 52, 28);
                                g.DrawLine(whitePen, 26, 28, 52, 28);
                                g.FillEllipse(whiteBrush, 32, 46, 6, 6);
                                g.FillEllipse(whiteBrush, 44, 46, 6, 6);
                            }
                        }
                        else if (name == "revenue" || name == "sales")
                        {
                            Rectangle r = new Rectangle(0, 0, 64, 64);
                            using (var bgBrush = new System.Drawing.Drawing2D.LinearGradientBrush(r, c1, c2, 45f))
                            {
                                g.FillEllipse(bgBrush, new Rectangle(2, 2, 60, 60));
                            }
                            using (var whiteBrush = new SolidBrush(Color.White))
                            using (Font f = new Font("Segoe UI", 28, FontStyle.Bold))
                            {
                                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                                g.DrawString("$", f, whiteBrush, new Rectangle(0, 0, 64, 64), sf);
                            }
                        }
                        else if (name == "part")
                        {
                            // A modern box/cube shape for parts
                            using (var pen = new Pen(brush, 4) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round })
                            {
                                // Hexagon/Cube outline
                                Point[] points = new Point[] { 
                                    new Point(32, 10), new Point(54, 22), new Point(54, 46), 
                                    new Point(32, 58), new Point(10, 46), new Point(10, 22) 
                                };
                                g.DrawPolygon(pen, points);
                                // Inner lines for cube effect
                                g.DrawLine(pen, 32, 10, 32, 34);
                                g.DrawLine(pen, 32, 34, 10, 22);
                                g.DrawLine(pen, 32, 34, 54, 22);
                                g.DrawLine(pen, 32, 34, 32, 58);
                            }
                        }
                        else if (name == "warning")
                        {
                            g.FillPolygon(brush, new Point[] { new Point(32, 10), new Point(10, 50), new Point(54, 50) });
                            using (var whiteBrush = new SolidBrush(Color.White))
                            {
                                g.FillRectangle(whiteBrush, 29, 24, 6, 14);
                                g.FillEllipse(whiteBrush, 29, 42, 6, 6);
                            }
                        }
                        else if (name == "info")
                        {
                            g.FillEllipse(brush, 10, 10, 44, 44);
                            using (var whiteBrush = new SolidBrush(Color.White))
                            {
                                g.FillRectangle(whiteBrush, 29, 28, 6, 14);
                                g.FillEllipse(whiteBrush, 29, 18, 6, 6);
                            }
                        }
                        else if (name == "check" || name == "success")
                        {
                            g.FillEllipse(brush, 10, 10, 44, 44);
                             using (var whitePen = new Pen(Color.White, 6) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round, LineJoin = System.Drawing.Drawing2D.LineJoin.Round })
                             {
                                 g.DrawLines(whitePen, new Point[] { new Point(20, 32), new Point(28, 40), new Point(44, 24) });
                             }
                        }
                        else if (name == "add" || name == "plus")
                        {
                            using (var whitePen = new Pen(Color.White, 8) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
                            {
                                g.DrawLine(whitePen, 32, 16, 32, 48);
                                g.DrawLine(whitePen, 16, 32, 48, 32);
                            }
                        }
                        else if (name == "pos")
                        {
                            g.FillEllipse(brush, 10, 10, 44, 44);
                            using (var whiteBrush = new SolidBrush(Color.White))
                            {
                                // Simple cash register / screen shape
                                g.FillRectangle(whiteBrush, 20, 22, 24, 16); // Screen
                                g.FillRectangle(whiteBrush, 18, 38, 28, 4);  // Base
                            }
                        }
                        else if (name == "inventory")
                        {
                            g.FillEllipse(brush, 8, 8, 48, 48);
                            using (var whitePen = new Pen(Color.White, 3) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round })
                            {
                                // Modern Box/Cube in white
                                Point[] p = { new Point(32, 18), new Point(48, 26), new Point(48, 42), new Point(32, 50), new Point(16, 42), new Point(16, 26) };
                                g.DrawPolygon(whitePen, p);
                                g.DrawLine(whitePen, 32, 18, 32, 34);
                                g.DrawLine(whitePen, 32, 34, 48, 26);
                                g.DrawLine(whitePen, 32, 34, 16, 26);
                                g.DrawLine(whitePen, 32, 34, 32, 50);
                            }
                        }
                        else if (name == "customers" || name == "user")
                        {
                            g.FillEllipse(brush, 8, 8, 48, 48);
                            using (var whiteBrush = new SolidBrush(Color.White))
                            {
                                g.FillEllipse(whiteBrush, 24, 18, 16, 16); // Head
                                g.FillPie(whiteBrush, 16, 34, 32, 32, 180, 180); // Shoulders
                            }
                        }
                        else if (name == "suppliers")
                        {
                            g.FillEllipse(brush, 8, 8, 48, 48);
                            using (var whiteBrush = new SolidBrush(Color.White))
                            {
                                g.FillRectangle(whiteBrush, 16, 26, 24, 16); // Truck body
                                g.FillRectangle(whiteBrush, 40, 32, 8, 10);  // Truck head
                                g.FillEllipse(whiteBrush, 20, 42, 6, 6);     // Wheel 1
                                g.FillEllipse(whiteBrush, 36, 42, 6, 6);     // Wheel 2
                            }
                        }
                        else if (name == "reports")
                        {
                            g.FillEllipse(brush, 8, 8, 48, 48);
                            using (var whiteBrush = new SolidBrush(Color.White))
                            {
                                g.FillRectangle(whiteBrush, 18, 38, 8, 12); // Bar 1
                                g.FillRectangle(whiteBrush, 28, 28, 8, 22); // Bar 2
                                g.FillRectangle(whiteBrush, 38, 18, 8, 32); // Bar 3
                            }
                        }
                        else if (name == "history")
                        {
                            g.FillEllipse(brush, 8, 8, 48, 48);
                            using (var whitePen = new Pen(Color.White, 4) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
                            {
                                g.DrawArc(whitePen, 18, 18, 28, 28, 45, 270);
                                g.DrawLine(whitePen, 32, 22, 32, 32);
                                g.DrawLine(whitePen, 32, 32, 40, 32);
                            }
                        }
                        else if (name == "quotations")
                        {
                            g.FillEllipse(brush, 8, 8, 48, 48);
                            using (var whiteBrush = new SolidBrush(Color.White))
                            {
                                g.FillRectangle(whiteBrush, 20, 18, 24, 28); // Paper
                                g.FillRectangle(new SolidBrush(c1), 24, 24, 16, 2); // Line 1
                                g.FillRectangle(new SolidBrush(c1), 24, 30, 16, 2); // Line 2
                            }
                        }
                        else if (name == "currencies" || name == "expenses")
                        {
                            g.FillEllipse(brush, 8, 8, 48, 48);
                            using (var whiteBrush = new SolidBrush(Color.White))
                            using (Font f = new Font("Segoe UI", 24, FontStyle.Bold))
                            {
                                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                                g.DrawString("$", f, whiteBrush, new Rectangle(0, 0, 64, 64), sf);
                            }
                        }
                        else
                        {
                            // Basic Box for others
                            using (var path = GetRoundedPath(new Rectangle(12, 12, 40, 40), 10))
                                g.FillPath(brush, path);
                        }
                    }
                }
                return bmp;
            } catch { return null; }
        }
        public static void ApplyFormIcon(Form form)
        {
            try
            {
                string iconPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "inventory_icon.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    form.Icon = new Icon(iconPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply form icon: {ex.Message}");
            }
        }
        /// <summary>
        /// Creates a styled (Panel + ComboBox) currency selector that matches the app's
        /// search-bar aesthetic: rounded border, Segoe UI font, theme background.
        /// Returns a tuple(panel, comboBox) — add the Panel to the container.
        /// </summary>
        public static (Panel panel, ComboBox combo) CreateStyledCurrencySelector(int width = 140, int height = 36)
        {
            ComboBox cbo = new ComboBox
            {
                DropDownStyle    = ComboBoxStyle.DropDownList,
                Font             = StandardFont,
                ForeColor        = TextColorDark,
                BackColor        = SurfaceColor,
                FlatStyle        = FlatStyle.Flat,
                Dock             = DockStyle.Fill,
                Margin           = new System.Windows.Forms.Padding(6, 0, 6, 0)
            };
            // Remove default border by placing inside a painted panel
            Panel wrapper = new Panel
            {
                Size      = new System.Drawing.Size(width, height),
                BackColor = SurfaceColor,
                Padding   = new System.Windows.Forms.Padding(4, 4, 4, 0)
            };
            wrapper.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = GetRoundedPath(new System.Drawing.Rectangle(0, 0, wrapper.Width - 1, wrapper.Height - 1), 8))
                using (var pen  = new System.Drawing.Pen(BorderColor, 1.5f))
                using (var bg   = new System.Drawing.SolidBrush(SurfaceColor))
                {
                    e.Graphics.FillPath(bg, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };
            wrapper.Controls.Add(cbo);
            return (wrapper, cbo);
        }

        public static Panel CreateCardPanel(Control inner)
        {
            // The outer panel acts as the background container to avoid "white corners"
            Panel p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0) };
            
            // The actual card panel that draws the rounded background
            Panel card = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(5) };
            
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                
                // 1. Fill the entire area with PARENT background first to solve white corners
                Color parentColor = GetParentColor(card);
                using (var bgBrush = new SolidBrush(parentColor))
                    e.Graphics.FillRectangle(bgBrush, -1, -1, card.Width + 2, card.Height + 2);

                // 2. Draw Rounded Card with SurfaceColor
                Rectangle rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using (var cardBrush = new SolidBrush(SurfaceColor))
                using (var path = GetRoundedPath(rect, 15))
                {
                    e.Graphics.FillPath(cardBrush, path);
                    
                    // Subtle border to define the card
                    using (var pen = new Pen(BorderColor, 1f))
                        e.Graphics.DrawPath(pen, path);
                }
            };

            // To ensure the inner content doesn't cover the rounded corners, 
            // we use a nested container or specific margins.
            // But usually, the Grid inside looks fine if the card panel has some padding.
            card.Controls.Add(inner);
            inner.Dock = DockStyle.Fill;

            p.Controls.Add(card);
            return p;
        }


        public static void ApplyModernMenuTheme(ContextMenuStrip menu)
        {
            menu.Renderer = new ModernNotificationRenderer();
            menu.BackColor = SurfaceColor;
            menu.ShowImageMargin = true;
            menu.ShowCheckMargin = false;
            menu.DropShadowEnabled = true;
        }
    }

    public class ModernNotificationRenderer : ToolStripProfessionalRenderer
    {
        public ModernNotificationRenderer() : base(new ModernColorTable()) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                Rectangle rc = new Rectangle(4, 2, e.Item.Width - 8, e.Item.Height - 4);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using(var path = ThemeConfig.GetRoundedPathPublic(rc, 8))
                using(var brush = new SolidBrush(ThemeConfig.ActiveBackColor))
                    e.Graphics.FillPath(brush, path);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = ThemeConfig.TextColorDark;
            e.TextFont = ThemeConfig.StandardFont;
            
            // If it's a ToolStripMenuItem and has a tag, we might want to emphasize title
            base.OnRenderItemText(e);
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            Rectangle rc = e.ImageRectangle;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(e.Image, rc);
        }
        
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            Rectangle rc = new Rectangle(15, e.Item.Height / 2, e.Item.Width - 30, 1);
            using(var pen = new Pen(ThemeConfig.BorderColor))
                e.Graphics.DrawLine(pen, rc.Left, rc.Top, rc.Right, rc.Top);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
             // Draw a subtle rounded border for the whole menu if possible, 
             // but ContextMenuStrip is a top-level window. 
             // We just draw a standard thin border.
             using(var pen = new Pen(ThemeConfig.BorderColor))
             {
                 e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
             }
        }
    }

    public class ModernColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => ThemeConfig.ActiveBackColor;
        public override Color MenuItemBorder => Color.Transparent;
        public override Color ToolStripDropDownBackground => Color.White;
    }
}

