using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace butcherPOS.Controls
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
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
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
