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
        private DateTime _value = DateTime.Now;

        public event EventHandler ValueChanged;

        public DateTime Value
        {
            get => _value;
            set
            {
                _value = value;
                UpdateLabel();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

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
            pnlContainer.BackColor = Color.White;
            pnlContainer.Padding = new Padding(10, 0, 5, 0);
            pnlContainer.Cursor = Cursors.Hand;
            pnlContainer.Click += OpenCalendar;
            // Paint border handled by Parent wrapper in POSForm? 
            // POSForm POSForm uses CreateStyledInput which wraps THIS control in ANOTHER panel with border.
            // So this control should just be transparent or white background.
            // CreateStyledInput sets innerControl.Width etc.
            
            // Wait, CreateStyledInput draws the border AROUND this control.
            // So we just need to render the Text and Icon.
            // And handle the click.
            
            lblDate = new Label();
            lblDate.Dock = DockStyle.Fill;
            lblDate.TextAlign = ContentAlignment.MiddleLeft;
            lblDate.Font = new Font("Segoe UI", 9.5F);
            lblDate.Text = _value.ToShortDateString();
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
            CustomCalendarForm calendar = new CustomCalendarForm(Value);
            
            // Center the popup relative to the input control
            int xOffset = (this.Width - calendar.Width) / 2;
            Point screenPoint = this.PointToScreen(new Point(xOffset, this.Height));
            
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
                lblDate.Text = _value.ToShortDateString();
        }
    }
}
