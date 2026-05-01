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
        // â”€â”€â”€ State â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

        // â”€â”€â”€ Supported currencies â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private static List<CurrencyInfo> _supportedCurrencies = new List<CurrencyInfo>();
        public static List<CurrencyInfo> SupportedCurrencies => _supportedCurrencies;

        // â”€â”€â”€ Rate dictionary (base = USD) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Default fallback rates (updated at runtime from DB or API)
        private static Dictionary<string, decimal> _rates = new Dictionary<string, decimal>
        {
            { "USD", 1m },
            { "EUR", 0.92m },
            { "LBP", 89500m },
        };

        // â”€â”€â”€ DB bootstrap â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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
                        last_updated DATETIME DEFAULT datetime('now')
                    );
                    INSERT INTO currency_rates (code, name, symbol, rate_vs_usd) VALUES
                        ('USD', 'US Dollar',      '$',   1),
                        ('EUR', 'Euro',           'â‚¬',   0.92),
                        ('LBP', 'Lebanese Lira',  N'Ù„.Ù„', 89500);
                END");

            // Ensure orders table has currency columns
            DatabaseHelper.ExecuteNonQuery(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='orders' AND COLUMN_NAME='currency_code')
                    ALTER TABLE orders ADD currency_code NVARCHAR(10) DEFAULT 'USD';
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='orders' AND COLUMN_NAME='exchange_rate')
                    ALTER TABLE orders ADD exchange_rate DECIMAL(18,6) DEFAULT 1;");

            LoadRatesFromDb();
        }

        // â”€â”€â”€ DB rate persistence â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public static void LoadRatesFromDb()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteDataTable("SELECT code, name, symbol, rate_vs_usd FROM currency_rates");
                _rates.Clear();
                _supportedCurrencies.Clear();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    string code = row["code"].ToString();
                    string name = row["name"].ToString();
                    string symbol = row["symbol"].ToString();
                    decimal rate = Convert.ToDecimal(row["rate_vs_usd"]);
                    
                    _rates[code] = rate;
                    _supportedCurrencies.Add(new CurrencyInfo(code, name, symbol));
                }
            }
            catch { /* silently keep defaults if load fails */ }
        }

        public static void SaveRatesToDb(Dictionary<string, decimal> newRates)
        {
            foreach (var kvp in newRates)
            {
                DatabaseHelper.ExecuteNonQuery(
                    $"UPDATE currency_rates SET rate_vs_usd = {kvp.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}, last_updated = datetime('now') WHERE code = '{kvp.Key}'");
                _rates[kvp.Key] = kvp.Value;
            }
        }

        public static void UpdateCurrency(string code, string name, string symbol, decimal rate)
        {
            DatabaseHelper.ExecuteNonQuery(
                $"UPDATE currency_rates SET name = @name, symbol = @symbol, rate_vs_usd = @rate, last_updated = datetime('now') WHERE code = @code",
                new Microsoft.Data.Sqlite.SqliteParameter("@name", name),
                new Microsoft.Data.Sqlite.SqliteParameter("@symbol", symbol),
                new Microsoft.Data.Sqlite.SqliteParameter("@rate", rate),
                new Microsoft.Data.Sqlite.SqliteParameter("@code", code)
            );
            _rates[code] = rate;
            LoadRatesFromDb(); // Refresh internal list
        }

        // â”€â”€â”€ Live API fetch â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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
                    // Build dynamic URL for all non-USD currencies
                    List<string> codes = new List<string>();
                    foreach(var c in _supportedCurrencies) if(c.Code != "USD") codes.Add(c.Code);
                    string codeList = string.Join(",", codes);

                    string fallbackUrl = $"https://api.frankfurter.app/latest?from=USD&to={codeList}";

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
                    foreach (string code in codes)
                    {
                        string key = $"\"{code}\":";
                        int idx = ratesPart.IndexOf(key);
                        if (idx < 0) continue;
                        idx += key.Length;
                        int end = ratesPart.IndexOfAny(new[] { ',', '}' }, idx);
                        string valStr = ratesPart.Substring(idx, end - idx).Trim().Replace("\"", "");
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

        // â”€â”€â”€ Conversion helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

            // LBP â€” no decimals, use thousands separator
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
