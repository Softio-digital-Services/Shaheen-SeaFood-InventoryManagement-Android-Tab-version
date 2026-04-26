using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GenericInventorySystem.Controls
{
    public class ModernButton : Button
    {
        public ModernButton()
        {
            // We set UserPaint to true to take full control
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Cursor = Cursors.Hand;
            this.BackColor = ThemeConfig.PrimaryColor;
            this.ForeColor = Color.White;
            this.Font = ThemeConfig.StandardFont;
            this.Size = new Size(150, 40);
            UpdateRegion();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            // Physically clip the control to rounded corners to hide the 'box' corners
            using (var path = new GraphicsPath())
            {
                int radius = 8;
                int d = radius * 2;
                Rectangle r = new Rectangle(0, 0, this.Width, this.Height);
                
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                
                this.Region = new Region(path);
            }
        }

        protected override bool ShowFocusCues => false;

        protected override void OnPaint(PaintEventArgs pevent)
        {
            // SKIP base.OnPaint(pevent) to prevent default border/focus rect
            
            // Invoke the custom painter directly
            ThemeConfig.DrawRoundedButton(this, pevent.Graphics);
        }
        
        // Handle Hover States manually since we might lose standard behavior if we get too aggressive, 
        // but UserPaint usually still fires MouseEnter/Leave. 
        // ThemeConfig.ApplyPrimaryButton attaches events for Hover colors.
        // Those event handlers UPDATE the BackColor property.
        // Changing BackColor triggers Invalidate(), which triggers OnPaint.
        // So the hover logic in ThemeConfig will still work perfectly!

        public override void NotifyDefault(bool value)
        {
            base.NotifyDefault(false);
        }
    }
}
