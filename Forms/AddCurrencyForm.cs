using System;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Services;

namespace InventorySystem.Forms
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
                MessageHelper.ShowWarning("Please enter currency code first");
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
                    MessageHelper.ShowWarning(LocalizationManager.IsArabic ? "\u062a\u0639\u0630\u0651\u0631 \u062c\u0644\u0628 \u0627\u0644\u0633\u0639\u0631. \u064a\u0631\u062c\u0649 \u0625\u062f\u062e\u0627\u0644\u0647 \u064a\u062f\u0648\u064a\u0627\u064b." : "Could not fetch rate. Please enter manually.");
                }
            }
            finally
            {
                btnFetch.Enabled = true;
                btnFetch.Text = LocalizationManager.IsArabic ? "\u062c\u0644\u0628" : "Fetch";
            }
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            txtCode.LabelText = (isArabic ? LocalizationManager.GetString("Curr_ColCode") : "Currency Code") + " (e.g. EUR)";
            txtName.LabelText = (isArabic ? LocalizationManager.GetString("Curr_ColName") : "Currency Name") + " (e.g. Euro)";
            txtSymbol.LabelText = ("Symbol") + " (e.g. \u20ac)";
            numRate.LabelText = (isArabic ? LocalizationManager.GetString("Curr_ColRate") : "Exchange Rate") + " (1 USD = ?)";
            btnFetch.Text = LocalizationManager.IsArabic ? "\u062c\u0644\u062b" : "Fetch";
            btnFetch.Image = ThemeConfig.GetNuricon("sync");
            btnFetch.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnFetch.ImageAlign = ContentAlignment.MiddleLeft;
            btnFetch.Padding = new Padding(10, 0, 0, 0);
            
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
