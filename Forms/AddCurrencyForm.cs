using System;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Services;

namespace GenericInventorySystem.Forms
{
    public partial class AddCurrencyForm : BaseModalForm
    {
        public AddCurrencyForm()
        {
            InitializeComponent();
            
            this.TitleText = LocalizationManager.GetString("Curr_AddBtn");
            ApplyLocalization();

            btnFetch.Click += btnFetch_Click;

            SetFooterButtons(
                LocalizationManager.GetString("AddPart_Save"),
                LocalizationManager.GetString("Popup_Cancel"),
                btnSave_Click,
                (s, e) => this.Close()
            );
        }

        private async void btnFetch_Click(object sender, EventArgs e)
        {
            string code = txtCode.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(code))
            {
                MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "ÙŠØ±Ø¬Ù‰ Ø¥Ø¯Ø®Ø§Ù„ Ø±Ù…Ø² Ø§Ù„Ø¹Ù…Ù„Ø© Ø£ÙˆÙ„Ø§Ù‹" : "Please enter currency code first");
                return;
            }

            btnFetch.Enabled = false;
            btnFetch.Text = "...";

            try
            {
                var rate = await CurrencyService.FetchRateAsync(code);
                if (rate.HasValue)
                {
                    numRate.Value = rate.Value;
                }
                else
                {
                    MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "ØªØ¹Ø°Ø± Ø¬Ù„Ø¨ Ø§Ù„Ø³Ø¹Ø±. ÙŠØ±Ø¬Ù‰ Ø§Ù„Ø¥Ø¯Ø®Ø§Ù„ ÙŠØ¯ÙˆÙŠØ§Ù‹." : "Could not fetch rate. Please enter manually.");
                }
            }
            finally
            {
                btnFetch.Enabled = true;
                btnFetch.Text = LocalizationManager.IsArabic ? "Ø¬Ù„Ø¨" : "Fetch";
            }
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            txtCode.LabelText = (isArabic ? "Ø±Ù…Ø² Ø§Ù„Ø¹Ù…Ù„Ø©" : "Currency Code") + " (e.g. EUR)";
            txtName.LabelText = (isArabic ? "Ø§Ø³Ù… Ø§Ù„Ø¹Ù…Ù„Ø©" : "Currency Name") + " (e.g. Euro)";
            txtSymbol.LabelText = (isArabic ? "Ø§Ù„Ø±Ù…Ø²" : "Symbol") + " (e.g. \u20ac)";
            numRate.LabelText = (isArabic ? "Ø³Ø¹Ø± Ø§Ù„ØµØ±Ù " : "Exchange Rate") + " (1 USD = ?)";
            btnFetch.Text = isArabic ? "Ø¬Ù„Ø¨" : "Fetch";
            
            numRate.Value = 1.0000m;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.ValidateRequiredFields(txtCode, txtName)) return;

            string code = txtCode.Text.Trim().ToUpper();
            string name = txtName.Text.Trim();
            string symbol = txtSymbol.Text.Trim();
            decimal rate = numRate.Value;

            if (code.Length > 10)
            {
                MessageHelper.ShowWarning("Code too long");
                return;
            }

            try
            {
                // Check if exists
                bool exists = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM currency_rates WHERE code = @code", 
                    new Microsoft.Data.Sqlite.SqliteParameter("@code", code)) > 0;

                if (exists) {
                    MessageHelper.ShowWarning(LocalizationManager.GetString("Curr_MsgExists"));
                    return;
                }

                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO currency_rates (code, name, symbol, rate_vs_usd) VALUES (@code, @name, @symbol, @rate)",
                    new Microsoft.Data.Sqlite.SqliteParameter("@code", code),
                    new Microsoft.Data.Sqlite.SqliteParameter("@name", name),
                    new Microsoft.Data.Sqlite.SqliteParameter("@symbol", symbol),
                    new Microsoft.Data.Sqlite.SqliteParameter("@rate", rate)
                );

                CurrencyService.LoadRatesFromDb();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError("Error adding currency: " + ex.Message);
            }
        }
    }
}
