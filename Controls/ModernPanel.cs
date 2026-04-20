using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GenericInventorySystem.Controls
{
    public class ModernPanel : Panel
    {
        public int BorderRadius { get; set; } = 30;
        public float GradientAngle { get; set; } = 90F;
        public Color GradientTopColor { get; set; } = Color.FromArgb(239, 246, 255); // Slight Blue Tint
        public Color GradientBottomColor { get; set; } = Color.White;

        public ModernPanel()
        {
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.Size = new Size(350, 200);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Gradient
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            using (LinearGradientBrush brush = new LinearGradientBrush(rect, this.GradientTopColor, this.GradientBottomColor, this.GradientAngle))
            using (GraphicsPath graphicsPath = GetRoundedPath(new Rectangle(0, 0, this.Width - 1, this.Height - 1), BorderRadius))
            {
                e.Graphics.FillPath(brush, graphicsPath);
                
                // Draw Rounded Border
                using (var pen = new Pen(ThemeConfig.BorderColor, 1.5f))
                {
                    e.Graphics.DrawPath(pen, path: graphicsPath);
                }
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

