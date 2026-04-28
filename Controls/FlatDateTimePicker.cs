using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace GenericInventorySystem.Controls
{
    // Replaces the previous inheritance from DateTimePicker to a full Custom Control
    public class FlatDateTimePicker : UserControl
    {
        private Label lblDate;
        private Panel pnlContainer;
        private DateTime? _value = null;

        public event EventHandler ValueChanged;

        public DateTime? Value
        {
            get => _value;
            set
            {
                _value = value;
                UpdateLabel();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DateTime MinDate { get; set; } = DateTime.MinValue;

        // Maintain compatibility with POsForm code
        public DateTimePickerFormat Format { get; set; } = DateTimePickerFormat.Short;
        
        // Hide standard properties we don't use but exist on UserControl
        // to prevent confusion
        
        public FlatDateTimePicker()
        {
            this.Size = new Size(200, 30); // Reduced height to fit inside 40px wrapper with padding
            this.Padding = new Padding(0);
            this.BackColor = Color.Transparent;
            
            InitializeControls();
        }

        private void InitializeControls()
        {
            pnlContainer = new Panel();
            pnlContainer.Dock = DockStyle.Fill;
            pnlContainer.Paint += PnlContainer_Paint;
            pnlContainer.Padding = new Padding(10, 0, 5, 0);
            pnlContainer.Cursor = Cursors.Hand;
            pnlContainer.Click += OpenCalendar;
            // So this control should just be transparent or white background.
            // CreateStyledInput sets innerControl.Width etc.
            
            // Wait, CreateStyledInput draws the border AROUND this control.
            // So we just need to render the Text and Icon.
            // And handle the click.
            
            lblDate = new Label();
            lblDate.Dock = DockStyle.Fill;
            lblDate.TextAlign = ContentAlignment.MiddleLeft;
            lblDate.Font = new Font("Segoe UI", 9.5F);
            lblDate.Text = _value.HasValue ? _value.Value.ToShortDateString() : "No Date";
            lblDate.Click += OpenCalendar; // Pass click through
            
            Label lblIcon = new Label();
            lblIcon.Text = "📅";
            lblIcon.Dock = DockStyle.Right;
            lblIcon.Width = 30;
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblIcon.Font = new Font("Segoe UI Emoji", 10);
            lblIcon.ForeColor = ThemeConfig.SecondaryColor;
            lblIcon.Click += OpenCalendar;


            pnlContainer.Controls.Add(lblDate);
            pnlContainer.Controls.Add(lblIcon);
            
            this.Controls.Add(pnlContainer);
        }

        private void OpenCalendar(object sender, EventArgs e)
        {
            CustomCalendarForm calendar = new CustomCalendarForm(Value, MinDate);
            
            // Center the popup relative to the input control
            int xOffset = (this.Width - calendar.Width) / 2;
            Point screenPoint = this.PointToScreen(new Point(xOffset, this.Height));
            
            // Prevent opening off-screen
            var screenBounds = Screen.FromPoint(screenPoint).WorkingArea;
            if (screenPoint.Y + calendar.Height > screenBounds.Bottom)
            {
                // Open upwards above the control
                screenPoint.Y = this.PointToScreen(new Point(xOffset, -calendar.Height)).Y;
            }
            
            calendar.Location = screenPoint;
            calendar.DateSelected += (s, args) => 
            {
                this.Value = calendar.SelectedDate;
            };
            calendar.Show();
        }

        private void PnlContainer_Paint(object sender, PaintEventArgs e)
        {
            var pnl = sender as Panel;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Clear corners with PARENT color to solve "white corners bug"
            Color parentColor = this.Parent?.BackColor ?? ThemeConfig.BackgroundColor;
            using (var brush = new SolidBrush(parentColor))
            {
                e.Graphics.FillRectangle(brush, pnl.ClientRectangle);
            }

            // 2. Draw Rounded Surface (White)
            Rectangle rect = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
            using (var path = GetRoundedPath(rect, 12))
            {
                using (var brush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // 3. Draw Border
                using (var pen = new Pen(ThemeConfig.BorderColor, 1.5f))
                {
                    e.Graphics.DrawPath(pen, path);
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

        private void UpdateLabel()
        {
            if(lblDate != null)
                lblDate.Text = _value.HasValue ? _value.Value.ToShortDateString() : "No Date";
        }
    }
}
