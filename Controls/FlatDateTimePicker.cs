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
            this.Size = new Size(200, 30); 
            this.Padding = new Padding(0);
            this.BackColor = Color.White;
            if (Helpers.LocalizationManager.IsArabic) this.RightToLeft = RightToLeft.Yes;
            
            InitializeControls();
        }

        private void InitializeControls()
        {
            pnlContainer = new Panel();
            pnlContainer.Dock = DockStyle.Fill;
            pnlContainer.Padding = new Padding(5, 0, 5, 0);
            pnlContainer.Cursor = Cursors.Hand;
            pnlContainer.Click += OpenCalendar;
            
            lblDate = new Label();
            lblDate.Dock = DockStyle.Fill;
            lblDate.TextAlign = ContentAlignment.MiddleLeft;
            lblDate.Font = new Font("Segoe UI", 10F);
            lblDate.Text = _value.HasValue ? _value.Value.ToShortDateString() : "No Date";
            lblDate.Click += OpenCalendar;
            
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
            
            int xOffset = (this.Width - calendar.Width) / 2;
            Point screenPoint = this.PointToScreen(new Point(xOffset, this.Height));
            
            var screenBounds = Screen.FromPoint(screenPoint).WorkingArea;
            if (screenPoint.Y + calendar.Height > screenBounds.Bottom)
            {
                screenPoint.Y = this.PointToScreen(new Point(xOffset, -calendar.Height)).Y;
            }
            
            calendar.Location = screenPoint;
            calendar.DateSelected += (s, args) => 
            {
                this.Value = calendar.SelectedDate;
            };
            calendar.Show();
        }

        private void UpdateLabel()
        {
            if(lblDate != null)
                lblDate.Text = _value.HasValue ? _value.Value.ToShortDateString() : "No Date";
        }
    }
}

