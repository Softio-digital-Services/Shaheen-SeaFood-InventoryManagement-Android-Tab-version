using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenericInventorySystem;

namespace GenericInventorySystem.Services
{
    /// <summary>
    /// Manages supported currencies and exchange-rate conversions.
    /// Base (storage) currency is always USD.
    /// </summary>
    public static class CurrencyService
    {
        // ─── State ────────────────────────────────────────────────────────
        private static string _activeCurrency = "USD";

        public static event EventHandler CurrencyChanged;
 
        public static string ActiveCurrency
        {
            get => _activeCurrency;
            set 
            { 
                if (_activeCurrency != value)
                {
                    _activeCurrency = value; 
                    CurrencyChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        // ─── Supported currencies ─────────────────────────────────────────
        public static readonly List<CurrencyInfo> SupportedCurrencies = new List<CurrencyInfo>
        {
            new CurrencyInfo("USD", "US Dollar",         "$"),
            new CurrencyInfo("EUR", "Euro",              "€"),
            new CurrencyInfo("LBP", "Lebanese Lira",    "ل.ل"),
        };

        // ─── Rate dictionary (base = USD) ─────────────────────────────────
        // Default fallback rates (updated at runtime from DB or API)
        private static Dictionary<string, decimal> _rates = new Dictionary<string, decimal>
        {
            { "USD", 1m },
            { "EUR", 0.92m },
            { "LBP", 89500m },
        };

        // ─── DB bootstrap ─────────────────────────────────────────────────
        public static void EnsureTable()
        {
            // Create table if it doesn't exist
            DatabaseHelper.ExecuteNonQuery(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'currency_rates')
                BEGIN
                    CREATE TABLE currency_rates (
                        code        NVARCHAR(10)  PRIMARY KEY,
                        name        NVARCHAR(100),
                        symbol      NVARCHAR(10),
                        rate_vs_usd DECIMAL(18,6) DEFAULT 1,
                        last_updated DATETIME DEFAULT GETDATE()
                    );
                    INSERT INTO currency_rates (code, name, symbol, rate_vs_usd) VALUES
                        ('USD', 'US Dollar',      '$',   1),
                        ('EUR', 'Euro',           '€',   0.92),
                        ('LBP', 'Lebanese Lira',  N'ل.ل', 89500);
                END");

            // Ensure orders table has currency columns
            DatabaseHelper.ExecuteNonQuery(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='orders' AND COLUMN_NAME='currency_code')
                    ALTER TABLE orders ADD currency_code NVARCHAR(10) DEFAULT 'USD';
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='orders' AND COLUMN_NAME='exchange_rate')
                    ALTER TABLE orders ADD exchange_rate DECIMAL(18,6) DEFAULT 1;");

            LoadRatesFromDb();
        }

        // ─── DB rate persistence ──────────────────────────────────────────
        public static void LoadRatesFromDb()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteDataTable("SELECT code, rate_vs_usd FROM currency_rates");
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    string code = row["code"].ToString();
                    decimal rate = Convert.ToDecimal(row["rate_vs_usd"]);
                    _rates[code] = rate;
                }
            }
            catch { /* silently keep defaults */ }
        }

        public static void SaveRatesToDb(Dictionary<string, decimal> newRates)
        {
            foreach (var kvp in newRates)
            {
                DatabaseHelper.ExecuteNonQuery(
                    $"UPDATE currency_rates SET rate_vs_usd = {kvp.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}, last_updated = GETDATE() WHERE code = '{kvp.Key}'");
                _rates[kvp.Key] = kvp.Value;
            }
        }

        // ─── Live API fetch ───────────────────────────────────────────────
        /// <summary>
        /// Fetches live rates from exchangerate.host (free, no key needed).
        /// Returns updated rates or null on failure.
        /// </summary>
        public static async Task<Dictionary<string, decimal>> FetchLiveRatesAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(8);
                    // exchangerate.host latest endpoint (base USD)
                    string url = "https://api.exchangerate.host/live?access_key=FREE&source=USD&currencies=EUR,LBP&format=1";
                    // Fallback: use Frankfurt API (no key)
                    string fallbackUrl = "https://api.frankfurter.app/latest?from=USD&to=EUR,LBP";

                    string json;
                    try
                    {
                        json = await client.GetStringAsync(fallbackUrl);
                    }
                    catch
                    {
                        return null;
                    }

                    // Parse Frankfurt JSON: {"amount":1,"base":"USD","date":"...","rates":{"EUR":0.92,"LBP":...}}
                    var result = new Dictionary<string, decimal> { { "USD", 1m } };
                    
                    // Simple JSON parsing (avoid heavy dependencies)
                    int ratesIdx = json.IndexOf("\"rates\":");
                    if (ratesIdx < 0) return null;

                    string ratesPart = json.Substring(ratesIdx);
                    foreach (string code in new[] { "EUR", "LBP" })
                    {
                        string key = $"\"{code}\":";
                        int idx = ratesPart.IndexOf(key);
                        if (idx < 0) continue;
                        idx += key.Length;
                        int end = ratesPart.IndexOfAny(new[] { ',', '}' }, idx);
                        string valStr = ratesPart.Substring(idx, end - idx).Trim();
                        if (decimal.TryParse(valStr, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out decimal val))
                        {
                            result[code] = val;
                        }
                    }
                    return result;
                }
            }
            catch
            {
                return null;
            }
        }

        // ─── Conversion helpers ───────────────────────────────────────────
        public static decimal ConvertAmount(decimal usdAmount, string toCurrency = null)
        {
            toCurrency = toCurrency ?? _activeCurrency;
            if (!_rates.TryGetValue(toCurrency, out decimal rate)) return usdAmount;
            return usdAmount * rate;
        }

        public static decimal GetRate(string currency = null)
        {
            currency = currency ?? _activeCurrency;
            return _rates.TryGetValue(currency, out decimal r) ? r : 1m;
        }

        public static string GetSymbol(string currency = null)
        {
            currency = currency ?? _activeCurrency;
            foreach (var c in SupportedCurrencies)
                if (c.Code == currency) return c.Symbol;
            return currency;
        }

        public static string Format(decimal usdAmount, string currency = null)
        {
            currency = currency ?? _activeCurrency;
            decimal converted = ConvertAmount(usdAmount, currency);
            string symbol = GetSymbol(currency);

            // LBP — no decimals, use thousands separator
            if (currency == "LBP")
                return $"{symbol} {converted:N0}";

            return $"{symbol}{converted:N2}";
        }

        /// <summary>Gets all currencies with current rates from DB.</summary>
        public static System.Data.DataTable GetAllCurrencies()
        {
            return DatabaseHelper.ExecuteDataTable(
                "SELECT code, name, symbol, rate_vs_usd, last_updated FROM currency_rates ORDER BY code");
        }
    }

    public class CurrencyInfo
    {
        public string Code   { get; }
        public string Name   { get; }
        public string Symbol { get; }
        public CurrencyInfo(string code, string name, string symbol)
        { Code = code; Name = name; Symbol = symbol; }
        public override string ToString() => $"{Code}  {Symbol}";
    }
}
