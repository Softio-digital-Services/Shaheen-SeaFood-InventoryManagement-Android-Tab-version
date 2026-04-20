using System.Windows.Forms;

namespace GenericInventorySystem
{
    /// <summary>
    /// Centralized validation helper
    /// Provides reusable validation methods for form inputs
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validate that all required text fields contain values
        /// </summary>
        /// <param name="controls">Array of controls to validate</param>
        /// <returns>True if all controls have values, false otherwise</returns>
        public static bool ValidateRequiredFields(params Control[] controls)
        {
            foreach (var control in controls)
            {
                if (control is TextBox textBox)
                {
                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        MessageHelper.ShowError("Please fill all required fields");
                        textBox.Focus();
                        return false;
                    }
                }
                else if (control is ComboBox comboBox)
                {
                    if (comboBox.SelectedIndex == -1)
                    {
                        MessageHelper.ShowError("Please select all required options");
                        comboBox.Focus();
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Validate that an image has been selected
        /// </summary>
        /// <param name="pictureBox">The picture box control</param>
        /// <param name="imagePath">The image file path</param>
        /// <returns>True if image is selected, false otherwise</returns>
        public static bool ValidateImageSelected(PictureBox pictureBox, string imagePath)
        {
            if (pictureBox.Image == null || string.IsNullOrWhiteSpace(imagePath))
            {
                MessageHelper.ShowError("Please select an image");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validate that a grid row is selected
        /// </summary>
        /// <param name="idField">The ID field to check</param>
        /// <returns>True if a row is selected, false otherwise</returns>
        public static bool ValidateRowSelected(TextBox idField)
        {
            if (string.IsNullOrWhiteSpace(idField.Text))
            {
                MessageHelper.ShowError("Please select an item first");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Validate that a string input is a valid integer
        /// </summary>
        public static bool ValidateInteger(string input, string fieldName, out int result)
        {
            if (!int.TryParse(input, out result))
            {
                MessageHelper.ShowError($"{fieldName} must be a valid whole number.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validate that a string input is a valid decimal
        /// </summary>
        public static bool ValidateDecimal(string input, string fieldName, out decimal result)
        {
            if (!decimal.TryParse(input, out result))
            {
                MessageHelper.ShowError($"{fieldName} must be a valid number (e.g. 10.50).");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validate phone number format (simple digit check)
        /// </summary>
        public static bool ValidatePhoneNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return true; // Optional allowed? If required, check empty first.
            
            // Simple check: must have at least 7 digits
            int digitCount = 0;
            foreach (char c in input) if (char.IsDigit(c)) digitCount++;
            
            if (digitCount < 7)
            {
                MessageHelper.ShowError("Phone number appears invalid (too few digits).");
                return false;
            }
            return true;
        }

        public static string TimeAgo(System.DateTime dateTime)
        {
            var timeSpan = System.DateTime.Now.Subtract(dateTime);
            if (timeSpan <= System.TimeSpan.FromSeconds(60)) return string.Format("{0} seconds ago", timeSpan.Seconds);
            if (timeSpan <= System.TimeSpan.FromMinutes(60)) return timeSpan.Minutes > 1 ? string.Format("about {0} minutes ago", timeSpan.Minutes) : "about a minute ago";
            if (timeSpan <= System.TimeSpan.FromHours(24)) return timeSpan.Hours > 1 ? string.Format("about {0} hours ago", timeSpan.Hours) : "about an hour ago";
            if (timeSpan <= System.TimeSpan.FromDays(30)) return timeSpan.Days > 1 ? string.Format("about {0} days ago", timeSpan.Days) : "yesterday";
            if (timeSpan <= System.TimeSpan.FromDays(365)) return timeSpan.Days > 30 ? string.Format("about {0} months ago", timeSpan.Days / 30) : "about a month ago";
            return timeSpan.Days > 365 ? string.Format("about {0} years ago", timeSpan.Days / 365) : "about a year ago";
        }
    }
}
