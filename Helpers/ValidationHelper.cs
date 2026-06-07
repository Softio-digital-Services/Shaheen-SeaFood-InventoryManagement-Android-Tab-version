using System;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Controls;

namespace InventorySystem
{
    /// <summary>
    /// Centralized validation helper
    /// Provides reusable validation methods for form inputs with localization support
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validate that all required text fields contain values
        /// </summary>
        public static bool ValidateRequiredFields(params Control[] controls)
        {
            foreach (var control in controls)
            {
                if (!ValidateRequired(control, "")) return false;
            }
            return true;
        }

        /// <summary>
        /// Validate required fields with specific names for error messages
        /// </summary>
        public static bool ValidateRequiredFields(Control parent, Control[] controls, string[] fieldNames)
        {
            for (int i = 0; i < controls.Length; i++)
            {
                string name = i < fieldNames.Length ? fieldNames[i] : "";
                if (!ValidateRequired(controls[i], name)) return false;
            }
            return true;
        }

        /// <summary>
        /// Validate that a single control has a value
        /// </summary>
        public static bool ValidateRequired(Control control, string fieldName)
        {
            string text = "";
            bool isEmpty = false;

            if (control is ModernTextBox modernTxt)
            {
                text = modernTxt.Text;
                isEmpty = string.IsNullOrWhiteSpace(text);
                modernTxt.IsError = isEmpty; // Visual feedback
            }
            else if (control is TextBox textBox)
            {
                text = textBox.Text;
                isEmpty = string.IsNullOrWhiteSpace(text);
            }
            else if (control is ComboBox comboBox)
            {
                isEmpty = comboBox.SelectedIndex == -1;
            }
            else if (control is ModernComboBox modernCmb)
            {
                isEmpty = modernCmb.SelectedIndex == -1;
            }

            if (isEmpty)
            {
                string msg = string.IsNullOrEmpty(fieldName)
                    ? (LocalizationManager.IsArabic ? "يرجى ملء جميع الحقول المطلوبة" : "Please fill all required fields")
                    : (LocalizationManager.IsArabic ? $"الحقل '{fieldName}' مطلوب" : $"Field '{fieldName}' is required");
                
                ShowValidationError(msg);
                control.Focus();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validate that a string input is a valid decimal (Price/Amount)
        /// </summary>
        public static bool ValidateDecimal(string input, string fieldName, out decimal result)
        {
            if (!decimal.TryParse(input, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result))
            {
                string msg = LocalizationManager.IsArabic 
                    ? $"{fieldName} يجب أن يكون رقماً صالحاً (مثال: 10.50)" 
                    : $"{fieldName} must be a valid number (e.g. 10.50).";
                ShowValidationError(msg);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Alias for ValidateDecimal
        /// </summary>
        public static bool ValidateNumeric(string input, string fieldName, out decimal result)
        {
            return ValidateDecimal(input, fieldName, out result);
        }

        /// <summary>
        /// Validate that a string input is a valid integer (Quantity/Stock)
        /// </summary>
        public static bool ValidateInteger(string input, string fieldName, out int result)
        {
            if (!int.TryParse(input, out result))
            {
                string msg = LocalizationManager.IsArabic 
                    ? $"{fieldName} يجب أن يكون رقماً صحيحاً" 
                    : $"{fieldName} must be a valid whole number.";
                ShowValidationError(msg);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validate phone number format
        /// </summary>
        public static bool ValidatePhoneNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return true;
            
            int digitCount = 0;
            foreach (char c in input) if (char.IsDigit(c)) digitCount++;
            
            if (digitCount < 7)
            {
                ShowValidationError(LocalizationManager.IsArabic ? "رقم الهاتف غير صالح" : "Phone number appears invalid (too few digits).");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validate email format
        /// </summary>
        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                ShowValidationError(LocalizationManager.IsArabic ? "عنوان البريد الإلكتروني غير صالح" : "Invalid email address format.");
                return false;
            }
        }

        private static void ShowValidationError(string message)
        {
            MessageHelper.ShowWarning(message);
        }

        /// <summary>
        /// Formats a date into a human-readable "time ago" string
        /// </summary>
        public static string TimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now.Subtract(dateTime);
            bool ar = LocalizationManager.IsArabic;

            if (timeSpan <= TimeSpan.FromSeconds(60)) 
                return ar ? "منذ ثوانٍ" : string.Format("{0} seconds ago", timeSpan.Seconds);
            
            if (timeSpan <= TimeSpan.FromMinutes(60)) 
                return ar ? (timeSpan.Minutes > 1 ? $"منذ {timeSpan.Minutes} دقيقة" : "منذ دقيقة") 
                          : (timeSpan.Minutes > 1 ? $"about {timeSpan.Minutes} minutes ago" : "about a minute ago");
            
            if (timeSpan <= TimeSpan.FromHours(24)) 
                return ar ? (timeSpan.Hours > 1 ? $"منذ {timeSpan.Hours} ساعة" : "منذ ساعة") 
                          : (timeSpan.Hours > 1 ? $"about {timeSpan.Hours} hours ago" : "about an hour ago");
            
            if (timeSpan <= TimeSpan.FromDays(30)) 
                return ar ? (timeSpan.Days > 1 ? $"منذ {timeSpan.Days} يوم" : "أمس") 
                          : (timeSpan.Days > 1 ? $"about {timeSpan.Days} days ago" : "yesterday");

            return dateTime.ToString("yyyy-MM-dd");
        }
    }
}
