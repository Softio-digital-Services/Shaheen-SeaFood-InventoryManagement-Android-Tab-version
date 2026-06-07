using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventorySystem.Controls
{
    /// <summary>
    /// A clean, themed scrollable panel using standard WinForms scrolling for maximum stability.
    /// Drop-in replacement for Panel with AutoScroll.
    /// </summary>
    public class ModernScrollPanel : Panel
    {
        public ModernScrollPanel()
        {
            this.AutoScroll = true;
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;
        }

        // Maintain compatibility with legacy calls
        public void AddControl(Control c)
        {
            this.Controls.Add(c);
            c.BringToFront();
        }
    }
}
