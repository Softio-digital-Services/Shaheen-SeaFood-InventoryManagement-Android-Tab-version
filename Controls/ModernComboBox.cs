using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Controls
{
    public class ModernComboBox : UserControl
    {
        private ComboBox cmbInput;
        private Label lblTitle;
        private Panel pnlContainer;

        public ComboBox InnerComboBox => cmbInput;

        [Category("Appearance")]
        public string LabelText
        {
            get => lblTitle.Text;
            set => lblTitle.Text = value;
        }

        public object SelectedItem
        {
            get => cmbInput.SelectedItem;
            set => cmbInput.SelectedItem = value;
        }

        public int SelectedIndex
        {
            get => cmbInput.SelectedIndex;
            set => cmbInput.SelectedIndex = value;
        }

        public ComboBox.ObjectCollection Items => cmbInput.Items;

        public ModernComboBox()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true; 

            this.BackColor = Color.Transparent;
            this.Size = new Size(200, 70);
            this.Padding = new Padding(0);

            InitializeControls();
        }

        private void InitializeControls()
        {
            // Label
            lblTitle = new Label();
            lblTitle.Text = "Category";
            lblTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTitle.ForeColor = ThemeConfig.TextColorDark;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(5, 0);
            this.Controls.Add(lblTitle);

            // Container Panel
            pnlContainer = new Panel();
            pnlContainer.BackColor = Color.White;
            pnlContainer.Location = new Point(0, 25);
            pnlContainer.Size = new Size(this.Width, this.Height - 25);
            pnlContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            pnlContainer.Paint += PnlContainer_Paint;
            pnlContainer.Padding = new Padding(10, 8, 10, 5);
            this.Controls.Add(pnlContainer);

            // ComboBox
            cmbInput = new ComboBox();
            cmbInput.FlatStyle = FlatStyle.Flat;
            cmbInput.Font = new Font("Segoe UI", 10F);
            cmbInput.ForeColor = ThemeConfig.TextColorDark;
            cmbInput.Dock = DockStyle.Fill;
            cmbInput.BackColor = Color.White;
            
            pnlContainer.Controls.Add(cmbInput);
        }

        private void PnlContainer_Paint(object sender, PaintEventArgs e)
        {
            var pnl = sender as Panel;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            pnlContainer?.Invalidate();
        }
    }
}
