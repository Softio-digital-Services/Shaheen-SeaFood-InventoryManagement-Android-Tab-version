using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Data;
using Android.Content;
using Android.Webkit;
using Java.Interop;
using Microsoft.Data.Sqlite;
using Shaheen_InventoryManagement_Android.Helpers;
using Shaheen_InventoryManagement_Android.Services;
using Shaheen_InventoryManagement_Android.Data;

namespace Shaheen_InventoryManagement_Android
{
    /// <summary>
    /// Custom WebViewClient that intercepts asset loads to serve HTML/CSS/JS files locally.
    /// It also registers a JavascriptInterface bridge to handle local API calls on the same thread.
    /// </summary>
    public class LocalApiWebViewClient : WebViewClient
    {
        private readonly Context _context;

        public LocalApiWebViewClient(Context context)
        {
            _context = context;
        }

        public override void OnPageStarted(WebView view, string url, Android.Graphics.Bitmap favicon)
        {
            base.OnPageStarted(view, url, favicon);
            
            // Add the bridge interface to the webview window
            view.AddJavascriptInterface(new WebAppInterface(view.Context), "AndroidBridge");
        }

        public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
        {
            string url = request.Url.ToString();

            // Intercept static files requested from local-api
            if (url.StartsWith("http://local-api/"))
            {
                string relativePath = url.Substring("http://local-api/".Length);
                if (relativePath.Contains("?"))
                {
                    relativePath = relativePath.Split('?')[0];
                }

                try
                {
                    // Open from Android Assets folder
                    var assetStream = _context.Assets.Open(relativePath);
                    string mimeType = GetMimeType(relativePath);
                    return new WebResourceResponse(mimeType, "utf-8", assetStream);
                }
                catch (Exception)
                {
                    // Fallback to empty response for missing files
                    return new WebResourceResponse("text/plain", "utf-8", new MemoryStream());
                }
            }

            return base.ShouldInterceptRequest(view, request);
        }

        private string GetMimeType(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".html": return "text/html";
                case ".css": return "text/css";
                case ".js": return "application/javascript";
                case ".json": return "application/json";
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".svg": return "image/svg+xml";
                case ".ico": return "image/x-icon";
                default: return "application/octet-stream";
            }
        }
    }

    /// <summary>
    /// Java-exposed interface to listen to fetch requests from Javascript.
    /// Executes all operations in native C# and SQLite locally.
    /// </summary>
    public class WebAppInterface : Java.Lang.Object
    {
        private readonly Context _context;

        public WebAppInterface(Context context)
        {
            _context = context;
        }

        [JavascriptInterface]
        [Export("handleApiCall")]
        public string HandleApiCall(string url, string method, string body)
        {
            try
            {
                // Parse optional date parameter from original query string
                string dateParam = null;
                if (url.Contains("?"))
                {
                    var parts = url.Split('?');
                    if (parts.Length > 1)
                    {
                        var queryParts = parts[1].Split('&');
                        foreach (var qp in queryParts)
                        {
                            var kv = qp.Split('=');
                            if (kv.Length == 2 && kv[0].Equals("date", StringComparison.OrdinalIgnoreCase))
                            {
                                dateParam = kv[1];
                            }
                        }
                    }
                }

                // Normalize URL to retrieve the endpoint path
                string endpoint = url;
                if (endpoint.StartsWith("http://local-api"))
                    endpoint = endpoint.Substring("http://local-api".Length);
                else if (endpoint.StartsWith("/"))
                    endpoint = endpoint.Substring(1);

                if (endpoint.Contains("?"))
                    endpoint = endpoint.Split('?')[0];

                endpoint = endpoint.Trim('/');

                // Route to appropriate local helper
                if (endpoint == "api/status")
                {
                    return JsonSerializer.Serialize(new { status = "API Running", version = "2.0", realtime = "SignalR Offline (Local Mode)" });
                }
                else if (endpoint == "api/config")
                {
                    return JsonSerializer.Serialize(new
                    {
                        language = LocalizationManager.IsArabic ? "ar" : "en",
                        isArabic = LocalizationManager.IsArabic,
                        primaryColor = "#0ea5e9",
                        primaryRgb = "14, 165, 233"
                    });
                }
                else if (endpoint == "api/products")
                {
                    return GetProducts();
                }
                else if (endpoint == "api/categories")
                {
                    return GetCategories();
                }
                else if (endpoint == "api/currencies")
                {
                    return GetCurrencies();
                }
                else if (endpoint == "api/recent-sales")
                {
                    return GetRecentSales();
                }
                else if (endpoint == "api/sales-items")
                {
                    return GetSalesItems(dateParam);
                }
                else if (endpoint == "api/sales-export" && method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    string sql = @"
                        SELECT 
                            COALESCE(p.item_no, r.item_no, '') as item_no,
                            COALESCE(p.part_name, r.recipe_name) as item_name,
                            SUM(oi.quantity) as qty_sold,
                            COALESCE(oi.unit_of_measure, p.big_unit, p.unit_of_measure, 'pcs') as unit
                        FROM orders o
                        JOIN order_items oi ON o.order_id = oi.order_id
                        LEFT JOIN parts p ON oi.part_id = p.id AND oi.item_type != 'Recipe'
                        LEFT JOIN recipes r ON oi.recipe_id = r.id AND oi.item_type = 'Recipe'
                        WHERE o.status != 'Cancelled'
                        GROUP BY COALESCE(p.item_no, r.item_no, ''), COALESCE(p.part_name, r.recipe_name), COALESCE(oi.unit_of_measure, p.big_unit, p.unit_of_measure, 'pcs')";

                    using (var dt = DatabaseHelper.ExecuteDataTable(sql))
                    {
                        var list = dt.Rows.Cast<DataRow>().Select(row => new
                        {
                            itemNo = row["item_no"]?.ToString() ?? "",
                            itemName = row["item_name"]?.ToString() ?? "",
                            qtySold = row["qty_sold"] != DBNull.Value ? Convert.ToInt32(row["qty_sold"]) : 0,
                            unit = row["unit"]?.ToString() ?? "pcs"
                        }).ToList();

                        return JsonSerializer.Serialize(list);
                    }
                }
                else if (endpoint.StartsWith("api/order-details/"))
                {
                    var idStr = endpoint.Substring("api/order-details/".Length);
                    if (int.TryParse(idStr, out int orderId))
                    {
                        return GetOrderDetails(orderId);
                    }
                    return JsonSerializer.Serialize(new { error = "Invalid order ID" });
                }
                else if (endpoint == "api/sync-session" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var payload = JsonSerializer.Deserialize<SyncSessionPayload>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            Helpers.UserSession.Username = payload.Username;
                            Helpers.UserSession.Role = payload.Role;
                            if (payload.Username == "Softio.Admin")
                            {
                                Helpers.UserSession.FullName = "Softio Super Admin";
                            }
                            else
                            {
                                var name = DatabaseHelper.ExecuteScalar<string>("SELECT full_name FROM users WHERE username = @u", new SqliteParameter("@u", payload.Username));
                                Helpers.UserSession.FullName = name ?? payload.Username;
                            }
                        }
                    }
                    catch { }
                    return JsonSerializer.Serialize(new { success = true });
                }
                else if (endpoint == "api/login" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessLogin(body);
                }
                else if (endpoint == "api/logout" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Logged out");
                    Helpers.UserSession.Clear();
                    return JsonSerializer.Serialize(new { success = true });
                }
                else if (endpoint == "api/license-info")
                {
                    try
                    {
                        var license = Helpers.LicenseManager.GetCurrentLicense();
                        if (license == null)
                        {
                            return JsonSerializer.Serialize(new
                            {
                                key = "None",
                                customerName = "None",
                                licenseType = "None",
                                activationDate = "N/A",
                                expirationDate = "N/A",
                                daysRemaining = 0,
                                status = "No license stored"
                            });
                        }

                        return JsonSerializer.Serialize(new
                        {
                            key = license.Key,
                            customerName = license.CustomerName,
                            licenseType = license.LicenseType,
                            activationDate = license.ActivationDate.ToString("yyyy-MM-dd"),
                            expirationDate = license.ExpirationDate.ToString("yyyy-MM-dd"),
                            daysRemaining = license.DaysRemaining(),
                            status = license.IsValid() ? "Active / Valid" : "Expired / Invalid"
                        });
                    }
                    catch (Exception ex)
                    {
                        return JsonSerializer.Serialize(new { error = ex.Message });
                    }
                }
                else if (endpoint == "api/factory-reset" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var bodyObj = JsonSerializer.Deserialize<FactoryResetPayload>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (bodyObj == null || !VerifyAdminCredentials(bodyObj.AdminUsername, bodyObj.AdminPassword))
                        {
                            return JsonSerializer.Serialize(new { success = false, message = "Invalid Admin username or password." });
                        }

                        PerformFactoryReset();

                        return JsonSerializer.Serialize(new { success = true });
                    }
                    catch (Exception ex)
                    {
                        return JsonSerializer.Serialize(new { success = false, error = ex.Message });
                    }
                }
                else if (endpoint == "api/add-item" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessAddItem(body);
                }
                else if (endpoint == "api/adjust-stock" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessAdjustStock(body);
                }
                else if (endpoint == "api/stock-transactions")
                {
                    string date = "";
                    int qIdx = url.IndexOf('?');
                    if (qIdx >= 0)
                    {
                        var query = url.Substring(qIdx + 1);
                        var parts = query.Split('&');
                        foreach (var p in parts)
                        {
                            if (p.StartsWith("date=")) date = Uri.UnescapeDataString(p.Substring("date=".Length));
                        }
                    }
                    return GetStockTransactions(date);
                }
                else if (endpoint == "api/checkout" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessCheckout(body);
                }
                else if (endpoint == "api/return-item" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessReturnItem(body);
                }
                else if (endpoint == "api/recipes/sales-frequency" && method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    return GetRecipesSalesFrequency();
                }
                else if (endpoint == "api/recipes" && method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    return GetRecipes();
                }
                else if (endpoint == "api/recipes" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessSaveRecipe(body);
                }
                else if (endpoint.StartsWith("api/recipes/") && method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                {
                    var idStr = endpoint.Substring("api/recipes/".Length);
                    if (int.TryParse(idStr, out int recipeId))
                    {
                        RecipeData.DeleteRecipe(recipeId);
                        return JsonSerializer.Serialize(new { success = true });
                    }
                    return JsonSerializer.Serialize(new { error = "Invalid recipe ID" });
                }
                else if (endpoint.StartsWith("api/products/") && method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                {
                    var idStr = endpoint.Substring("api/products/".Length);
                    if (int.TryParse(idStr, out int partId))
                    {
                        var service = new Shaheen_InventoryManagement_Android.Services.InventoryService();
                        service.DeletePart(partId);
                        return JsonSerializer.Serialize(new { success = true });
                    }
                    return JsonSerializer.Serialize(new { error = "Invalid product ID" });
                }
                else if (endpoint == "api/import-items" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessBulkImport(body);
                }
                else if (endpoint == "api/import-recipes" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessBulkRecipesImport(body);
                }
                else if (endpoint == "api/import-sales" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessDailySalesImport(body);
                }
                else if (endpoint == "api/export-csv" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessExportCsv(body);
                }
                else if (endpoint == "api/reports" && method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    int month = DateTime.Now.Month;
                    int year = DateTime.Now.Year;
                    int? day = null;
                    int qIdx = url.IndexOf('?');
                    if (qIdx >= 0)
                    {
                        var query = url.Substring(qIdx + 1);
                        var parts = query.Split('&');
                        foreach (var p in parts)
                        {
                            if (p.StartsWith("month=")) int.TryParse(Uri.UnescapeDataString(p.Substring("month=".Length)), out month);
                            if (p.StartsWith("year=")) int.TryParse(Uri.UnescapeDataString(p.Substring("year=".Length)), out year);
                            if (p.StartsWith("day="))
                            {
                                if (int.TryParse(Uri.UnescapeDataString(p.Substring("day=".Length)), out var d)) day = d;
                            }
                        }
                    }
                    return GetMonthlyReportData(month, year, day);
                }
                else if (endpoint == "api/clear-reports" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ClearReportsData();
                }
                else if (endpoint == "api/logs" && method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    string filterUser = null;
                    int qIdx = url.IndexOf('?');
                    if (qIdx >= 0)
                    {
                        var query = url.Substring(qIdx + 1);
                        var parts = query.Split('&');
                        foreach (var p in parts)
                        {
                            if (p.StartsWith("username=")) filterUser = Uri.UnescapeDataString(p.Substring("username=".Length));
                        }
                    }

                    if (Helpers.UserSession.Role != "Admin")
                    {
                        filterUser = Helpers.UserSession.Username;
                    }

                    string sql = "";
                    SqliteParameter[] parameters;

                    if (string.IsNullOrEmpty(filterUser) || filterUser.Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        sql = "SELECT timestamp, username, action FROM user_logs ORDER BY id DESC LIMIT 200";
                        parameters = new SqliteParameter[0];
                    }
                    else
                    {
                        sql = "SELECT timestamp, username, action FROM user_logs WHERE username = @u ORDER BY id DESC LIMIT 200";
                        parameters = new SqliteParameter[] { new SqliteParameter("@u", filterUser) };
                    }

                    using (var dt = DatabaseHelper.ExecuteDataTable(sql, parameters))
                    {
                        var list = dt.Rows.Cast<DataRow>().Select(row => new
                        {
                            timestamp = row["timestamp"]?.ToString(),
                            username = row["username"]?.ToString(),
                            action = row["action"]?.ToString()
                        }).ToList();

                        return JsonSerializer.Serialize(list);
                    }
                }

                return JsonSerializer.Serialize(new { error = $"Endpoint not found: {endpoint} ({method})" });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"WebAppInterface.HandleApiCall: {url}");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [JavascriptInterface]
        [Export("printPage")]
        public void PrintPage(string jobName)
        {
            try
            {
                if (_context is MainActivity mainActivity)
                {
                    mainActivity.PrintWebView(jobName);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.PrintPage");
            }
        }

        private string GetProducts()
        {
            var dt = DatabaseHelper.ExecuteDataTable(
                @"SELECT p.id, p.part_name, p.selling_price, p.quantity_in_stock,
                         p.minimum_stock_level, p.barcode, p.part_number, p.part_image,
                         p.item_no, p.unit_of_measure, p.purchase_price,
                         p.stock_type, p.pack_items_number, p.pack_price, p.item_price, p.piece_price,
                         p.big_unit, p.small_unit, p.conversion_value, p.pack_size,
                         COALESCE(c.category_name, 'General') AS category,
                         c.category_image
                  FROM parts p
                  LEFT JOIN categories c ON p.category_id = c.id
                  WHERE p.date_deleted IS NULL AND p.status = 'Active'
                  ORDER BY c.category_name, p.part_name");

            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                string category = row["category"].ToString();
                list.Add(new
                {
                    id = Convert.ToInt32(row["id"]),
                    name = row["part_name"].ToString(),
                    itemNo = row["item_no"].ToString(),
                    price = Convert.ToDecimal(row["selling_price"]),
                    purchasePrice = (!Helpers.UserSession.IsAdmin) ? 0m : Convert.ToDecimal(row["purchase_price"]),
                    unitOfMeasure = row["unit_of_measure"].ToString(),
                    stock = Convert.ToDouble(row["quantity_in_stock"]),
                    minStock = Convert.ToDouble(row["minimum_stock_level"]),
                    barcode = row["barcode"].ToString(),
                    sku = row["part_number"].ToString(),
                    category = category,
                    image = CleanImagePrefix(row["part_image"].ToString()),
                    categoryImage = CleanCategoryIcon(category, row["category_image"].ToString()),
                    isService = category.Equals("Services", StringComparison.OrdinalIgnoreCase),
                    stockType = row["stock_type"] != DBNull.Value ? row["stock_type"].ToString() : "Piece",
                    packItemsNumber = row["pack_items_number"] != DBNull.Value ? Convert.ToInt32(row["pack_items_number"]) : 0,
                    packPrice = (!Helpers.UserSession.IsAdmin) ? 0m : (row["pack_price"] != DBNull.Value ? Convert.ToDecimal(row["pack_price"]) : 0m),
                    itemPrice = (!Helpers.UserSession.IsAdmin) ? 0m : (row["item_price"] != DBNull.Value ? Convert.ToDecimal(row["item_price"]) : 0m),
                    piecePrice = (!Helpers.UserSession.IsAdmin) ? 0m : (row["piece_price"] != DBNull.Value ? Convert.ToDecimal(row["piece_price"]) : 0m),
                    bigUnit = row["big_unit"] != DBNull.Value ? row["big_unit"].ToString() : "",
                    smallUnit = row["small_unit"] != DBNull.Value ? row["small_unit"].ToString() : "",
                    conversionValue = row["conversion_value"] != DBNull.Value ? Convert.ToDouble(row["conversion_value"]) : 1.0,
                    packSize = row["pack_size"] != DBNull.Value ? Convert.ToDouble(row["pack_size"]) : 1.0
                });
            }
            return JsonSerializer.Serialize(list);
        }

        private string GetCategories()
        {
            var dt = DatabaseHelper.ExecuteDataTable("SELECT category_name FROM categories ORDER BY category_name");
            var list = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(row["category_name"].ToString());
            }
            return JsonSerializer.Serialize(list);
        }

        private string GetCurrencies()
        {
            var dt = DatabaseHelper.ExecuteDataTable("SELECT code, name, symbol, rate_vs_usd FROM currency_rates ORDER BY code");
            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    code = row["code"].ToString(),
                    name = row["name"].ToString(),
                    symbol = row["symbol"].ToString(),
                    rate = Convert.ToDecimal(row["rate_vs_usd"])
                });
            }
            return JsonSerializer.Serialize(list);
        }

        private string GetRecentSales()
        {
            var dt = DatabaseHelper.ExecuteDataTable(
                @"SELECT o.order_id, datetime(o.order_date, 'localtime') as order_date, o.total_amount, 
                         COALESCE(c.full_name, 'Cash Customer') as customer_name
                  FROM orders o
                  LEFT JOIN customers c ON o.customer_id = c.customer_id
                  WHERE o.status != 'Cancelled'
                  ORDER BY o.order_id DESC LIMIT 50");

            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    orderId = Convert.ToInt32(row["order_id"]),
                    date = Convert.ToDateTime(row["order_date"]),
                    total = Convert.ToDecimal(row["total_amount"]),
                    customer = row["customer_name"].ToString()
                });
            }
            return JsonSerializer.Serialize(list);
        }

        private string GetSalesItems(string dateParam = null)
        {
            string sql = @"SELECT oi.order_item_id, COALESCE(p.part_name, r.recipe_name) as item_name, oi.quantity, oi.price, datetime(o.order_date, 'localtime') as order_date, oi.unit_of_measure
                           FROM order_items oi
                           LEFT JOIN parts p ON oi.part_id = p.id AND oi.item_type != 'Recipe'
                           LEFT JOIN recipes r ON oi.recipe_id = r.id AND oi.item_type = 'Recipe'
                           JOIN orders o ON oi.order_id = o.order_id ";

            System.Data.DataTable dt;
            if (!string.IsNullOrEmpty(dateParam))
            {
                sql += " WHERE date(datetime(o.order_date, 'localtime')) = @date ORDER BY oi.order_item_id DESC";
                dt = DatabaseHelper.ExecuteDataTable(sql, new SqliteParameter("@date", dateParam));
            }
            else
            {
                sql += " ORDER BY oi.order_item_id DESC LIMIT 100";
                dt = DatabaseHelper.ExecuteDataTable(sql);
            }

            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    id = Convert.ToInt32(row["order_item_id"]),
                    name = row["item_name"].ToString(),
                    qty = Convert.ToDouble(row["quantity"]),
                    price = Convert.ToDecimal(row["price"]),
                    total = Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"]),
                    date = Convert.ToDateTime(row["order_date"]),
                    unitOfMeasure = row["unit_of_measure"] != DBNull.Value ? row["unit_of_measure"].ToString() : ""
                });
            }
            return JsonSerializer.Serialize(list);
        }

        private string GetOrderDetails(int orderId)
        {
            var dt = DatabaseHelper.ExecuteDataTable(
                @"SELECT oi.part_id, p.part_name, oi.quantity, oi.price
                  FROM order_items oi
                  JOIN parts p ON oi.part_id = p.id
                  WHERE oi.order_id = @id",
                new SqliteParameter("@id", orderId));

            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    partId = Convert.ToInt32(row["part_id"]),
                    name = row["part_name"].ToString(),
                    qty = Convert.ToInt32(row["quantity"]),
                    price = Convert.ToDecimal(row["price"])
                });
            }
            return JsonSerializer.Serialize(list);
        }

        private string ProcessLogin(string bodyJson)
        {
            var body = JsonSerializer.Deserialize<LoginPayload>(bodyJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (body == null || string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password))
                return JsonSerializer.Serialize(new { error = "Missing credentials" });

            if (body.Username == "Softio.Admin" && body.Password == "Softio@2026!")
            {
                Helpers.UserSession.Username = "Softio.Admin";
                Helpers.UserSession.Role = "Admin";
                Helpers.UserSession.FullName = "Softio Super Admin";
                DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Logged in");
                return JsonSerializer.Serialize(new { username = "Softio.Admin", role = "Admin", fullName = "Softio Super Admin" });
            }

            if (body.Username?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true && body.Password == "Admin.Softio")
            {
                Helpers.UserSession.Username = "Admin";
                Helpers.UserSession.Role = "Admin";
                Helpers.UserSession.FullName = "Test Admin";
                DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Logged in");
                return JsonSerializer.Serialize(new { username = "Admin", role = "Admin", fullName = "Test Admin" });
            }

            string cleanUsername = body.Username?.Trim() ?? "";
            string cleanPassword = body.Password?.Trim() ?? "";

            if (cleanUsername.Equals("production", StringComparison.OrdinalIgnoreCase) && 
                (cleanPassword == "Productio@2026!" || cleanPassword == "Production@2026!"))
            {
                Helpers.UserSession.Username = "production";
                Helpers.UserSession.Role = "Production";
                Helpers.UserSession.FullName = "Production User";
                DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Logged in");
                return JsonSerializer.Serialize(new { username = "production", role = "Production", fullName = "Production User" });
            }

            var dt = DatabaseHelper.ExecuteDataTable(
                "SELECT username, role, full_name FROM users WHERE username = @u AND password = @p",
                new SqliteParameter("@u", body.Username),
                new SqliteParameter("@p", body.Password));

            if (dt.Rows.Count == 0)
                return JsonSerializer.Serialize(new { error = "Unauthorized" });

            var row = dt.Rows[0];
            Helpers.UserSession.Username = row["username"].ToString();
            Helpers.UserSession.Role = row["role"].ToString();
            Helpers.UserSession.FullName = row["full_name"].ToString();
            DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Logged in");

            return JsonSerializer.Serialize(new
            {
                username = row["username"].ToString(),
                role = row["role"].ToString(),
                fullName = row["full_name"].ToString()
            });
        }

        private string ProcessAddItem(string bodyJson)
        {
            var body = JsonSerializer.Deserialize<AddItemPayload>(bodyJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (body == null || string.IsNullOrEmpty(body.Name))
                return JsonSerializer.Serialize(new { error = "Missing name" });

            string categoryName = (body.Category ?? "General").Trim();
            if (string.IsNullOrEmpty(categoryName)) categoryName = "General";

            int catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                        new SqliteParameter("@c", categoryName));
            if (catId == 0)
            {
                DatabaseHelper.ExecuteNonQuery("INSERT INTO categories (category_name) VALUES (@c)",
                            new SqliteParameter("@c", categoryName));
                catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                            new SqliteParameter("@c", categoryName));
            }
            if (catId == 0) catId = 1;

             if (body.Id.HasValue && body.Id.Value > 0)
            {
                // Update existing item
                if (!string.IsNullOrEmpty(body.Barcode))
                {
                    int existingCount = DatabaseHelper.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM parts WHERE barcode = @b AND id != @id AND date_deleted IS NULL",
                        new SqliteParameter("@b", body.Barcode),
                        new SqliteParameter("@id", body.Id.Value));

                    if (existingCount > 0)
                        return JsonSerializer.Serialize(new { error = "Barcode already exists for another item." });
                }

                  string sql = @"
                    UPDATE parts SET 
                        part_name = @name, 
                        part_number = @sku, 
                        category_id = @cat, 
                        purchase_price = @p_price, 
                        selling_price = @s_price, 
                        quantity_in_stock = @stock, 
                        minimum_stock_level = @min_stock,
                        barcode = @barcode,
                        item_no = @itemNo,
                        unit_of_measure = @uom,
                        stock_type = @stock_type,
                        pack_items_number = @pack_items_number,
                        pack_price = @pack_price,
                        item_price = @item_price,
                        piece_price = @piece_price,
                        big_unit = @big_unit,
                        small_unit = @small_unit,
                        conversion_value = @conversion_value,
                        pack_size = @pack_size,
                        part_image = @part_image
                    WHERE id = @id";

                double oldStock = DatabaseHelper.ExecuteScalar<double>(
                    "SELECT COALESCE(quantity_in_stock, 0) FROM parts WHERE id = @id",
                    new SqliteParameter("@id", body.Id.Value));

                decimal pPrice = body.Price;
                decimal sPrice = body.Price;
                decimal packPrice = body.PackPrice;
                decimal itemPrice = body.ItemPrice;
                decimal piecePrice = body.PiecePrice;

                if (!Helpers.UserSession.IsAdmin)
                {
                    using (var dtItem = DatabaseHelper.ExecuteDataTable(
                        "SELECT purchase_price, selling_price, pack_price, item_price, piece_price FROM parts WHERE id = @id",
                        new SqliteParameter("@id", body.Id.Value)))
                    {
                        if (dtItem.Rows.Count > 0)
                        {
                            var row = dtItem.Rows[0];
                            pPrice = row["purchase_price"] != DBNull.Value ? Convert.ToDecimal(row["purchase_price"]) : 0m;
                            sPrice = row["selling_price"] != DBNull.Value ? Convert.ToDecimal(row["selling_price"]) : 0m;
                            packPrice = row["pack_price"] != DBNull.Value ? Convert.ToDecimal(row["pack_price"]) : 0m;
                            itemPrice = row["item_price"] != DBNull.Value ? Convert.ToDecimal(row["item_price"]) : 0m;
                            piecePrice = row["piece_price"] != DBNull.Value ? Convert.ToDecimal(row["piece_price"]) : 0m;
                        }
                    }
                }

                DatabaseHelper.ExecuteNonQuery(sql,
                    new SqliteParameter("@name", body.Name),
                    new SqliteParameter("@sku", body.Sku ?? ""),
                    new SqliteParameter("@cat", catId),
                    new SqliteParameter("@p_price", pPrice),
                    new SqliteParameter("@s_price", sPrice),
                    new SqliteParameter("@stock", body.Stock),
                    new SqliteParameter("@min_stock", body.MinStock),
                    new SqliteParameter("@barcode", body.Barcode ?? ""),
                    new SqliteParameter("@itemNo", body.ItemNo ?? ""),
                    new SqliteParameter("@uom", body.UnitOfMeasure ?? ""),
                    new SqliteParameter("@stock_type", body.StockType ?? "Piece"),
                    new SqliteParameter("@pack_items_number", body.PackItemsNumber),
                    new SqliteParameter("@pack_price", packPrice),
                    new SqliteParameter("@item_price", itemPrice),
                    new SqliteParameter("@piece_price", piecePrice),
                    new SqliteParameter("@big_unit", body.BigUnit ?? ""),
                    new SqliteParameter("@small_unit", body.SmallUnit ?? ""),
                    new SqliteParameter("@conversion_value", body.ConversionValue),
                    new SqliteParameter("@pack_size", body.PackSize),
                    new SqliteParameter("@part_image", body.Image ?? ""),
                    new SqliteParameter("@id", body.Id.Value));

                double delta = body.Stock - oldStock;
                if (delta != 0)
                {
                    string action = delta > 0 ? "ADJUST_IN" : "ADJUST_OUT";
                    DatabaseHelper.LogTransaction(action, body.Name, $"Adjusted stock of {body.Name} by {delta:F4}. Reason: Product Edit (New Qty: {body.Stock})");
                }
                else
                {
                    DatabaseHelper.LogTransaction("STOCK_EDIT", body.Name, $"Edited via Android App (New Qty: {body.Stock})");
                }
                DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Edited ingredient");
                GlobalEvents.RaiseInventoryUpdated();
                return JsonSerializer.Serialize(new { success = true });
            }
            else
            {
                // Insert new item
                if (!string.IsNullOrEmpty(body.Barcode))
                {
                    int existingCount = DatabaseHelper.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM parts WHERE barcode = @b AND date_deleted IS NULL",
                        new SqliteParameter("@b", body.Barcode));

                    if (existingCount > 0)
                        return JsonSerializer.Serialize(new { error = "Barcode already exists for another item." });
                }

                string sql = @"
                    INSERT INTO parts (part_name, part_number, category_id, purchase_price, selling_price, quantity_in_stock, minimum_stock_level, barcode, status, item_no, unit_of_measure,
                                       stock_type, pack_items_number, pack_price, item_price, piece_price,
                                       big_unit, small_unit, conversion_value, pack_size, part_image)
                    VALUES (@name, @sku, @cat, @p_price, @s_price, @stock, @min_stock, @barcode, 'Active', @itemNo, @uom,
                            @stock_type, @pack_items_number, @pack_price, @item_price, @piece_price,
                            @big_unit, @small_unit, @conversion_value, @pack_size, @part_image)";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new SqliteParameter("@name", body.Name),
                    new SqliteParameter("@sku", body.Sku ?? ""),
                    new SqliteParameter("@cat", catId),
                    new SqliteParameter("@p_price", body.Price),
                    new SqliteParameter("@s_price", body.Price),
                    new SqliteParameter("@stock", body.Stock),
                    new SqliteParameter("@min_stock", body.MinStock),
                    new SqliteParameter("@barcode", body.Barcode ?? ""),
                    new SqliteParameter("@itemNo", body.ItemNo ?? ""),
                    new SqliteParameter("@uom", body.UnitOfMeasure ?? ""),
                    new SqliteParameter("@stock_type", body.StockType ?? "Piece"),
                    new SqliteParameter("@pack_items_number", body.PackItemsNumber),
                    new SqliteParameter("@pack_price", body.PackPrice),
                    new SqliteParameter("@item_price", body.ItemPrice),
                    new SqliteParameter("@piece_price", body.PiecePrice),
                    new SqliteParameter("@big_unit", body.BigUnit ?? ""),
                    new SqliteParameter("@small_unit", body.SmallUnit ?? ""),
                    new SqliteParameter("@conversion_value", body.ConversionValue),
                    new SqliteParameter("@pack_size", body.PackSize),
                    new SqliteParameter("@part_image", body.Image ?? ""));

                DatabaseHelper.LogTransaction("STOCK_ADD", body.Name, $"Added via Android App (Qty: {body.Stock})");
                DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Added ingredient");
                GlobalEvents.RaiseInventoryUpdated();
                return JsonSerializer.Serialize(new { success = true });
            }
        }

        private string ProcessAdjustStock(string bodyJson)
        {
            try
            {
                var body = JsonSerializer.Deserialize<AdjustStockPayload>(bodyJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (body == null || body.PartId <= 0)
                    return JsonSerializer.Serialize(new { error = "Invalid product ID" });

                var inventoryService = new Shaheen_InventoryManagement_Android.Services.InventoryService();
                inventoryService.AdjustStock(body.PartId, body.Change, body.Reason ?? "Manual Adjustment");

                return JsonSerializer.Serialize(new { success = true });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.ProcessAdjustStock");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private string GetStockTransactions(string date)
        {
            try
            {
                string sql = @"SELECT action_type, part_name, description, datetime(timestamp, 'localtime') as timestamp, username
                               FROM transactions
                               WHERE action_type IN ('ADJUST_IN', 'ADJUST_OUT', 'STOCK_EDIT', 'STOCK_ADD', 'STOCK_DEDUCT')";
                
                var parameters = new List<SqliteParameter>();
                if (!string.IsNullOrEmpty(date))
                {
                    sql += " AND date(datetime(timestamp, 'localtime')) = @date";
                    parameters.Add(new SqliteParameter("@date", date));
                }
                sql += " ORDER BY id DESC";

                var dt = DatabaseHelper.ExecuteDataTable(sql, parameters.ToArray());
                var list = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new
                    {
                        action = row["action_type"].ToString(),
                        item = row["part_name"].ToString(),
                        desc = row["description"].ToString(),
                        time = row["timestamp"].ToString(),
                        user = row["username"].ToString()
                    });
                }
                return JsonSerializer.Serialize(list);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.GetStockTransactions");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private string ProcessCheckout(string bodyJson)
        {
            try
            {
                var body = JsonSerializer.Deserialize<CheckoutPayload>(bodyJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (body == null || body.Items == null || body.Items.Count == 0)
                    return JsonSerializer.Serialize(new { error = "Empty cart" });

                decimal total = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in body.Items)
                {
                    total += item.Price * item.Qty;
                    bool isRecipe = item.ItemType == "Recipe" || (item.RecipeId.HasValue && item.RecipeId.Value > 0);
                    
                    string uom = item.UnitOfMeasure;
                    if (isRecipe)
                    {
                        uom = "";
                    }
                    else if (string.IsNullOrEmpty(uom) || uom == "pack")
                    {
                        var dt = DatabaseHelper.ExecuteDataTable("SELECT small_unit, unit_of_measure FROM parts WHERE id = @id", new SqliteParameter("@id", item.Id));
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            var row = dt.Rows[0];
                            string smallUnit = row["small_unit"] != DBNull.Value ? row["small_unit"].ToString() : "";
                            string unitOfMeasure = row["unit_of_measure"] != DBNull.Value ? row["unit_of_measure"].ToString() : "";
                            uom = !string.IsNullOrEmpty(smallUnit) ? smallUnit : unitOfMeasure;
                        }
                    }
                    if (string.IsNullOrEmpty(uom))
                    {
                        uom = isRecipe ? "" : "pcs";
                    }
                    
                    orderItems.Add(new OrderItem
                    {
                        ItemType = isRecipe ? "Recipe" : "Part",
                        PartId = isRecipe ? 0 : item.Id,
                        RecipeId = isRecipe ? (item.RecipeId ?? item.Id) : (int?)null,
                        PartName = item.Name,
                        Quantity = item.Qty,
                        UnitPrice = item.Price,
                        UnitOfMeasure = uom
                    });
                }

                var orderService = new OrderService();
                int orderId = orderService.PlaceOrder(-1, orderItems, total, true, "Completed");

                DatabaseHelper.LogTransaction("SALE", "POS Sale", $"Order #{orderId} -- Total: {total:C}");

                GlobalEvents.RaiseInventoryUpdated();
                return JsonSerializer.Serialize(new { success = true, orderId, total });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.ProcessCheckout");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private string ProcessReturnItem(string bodyJson)
        {
            var body = JsonSerializer.Deserialize<ReturnPayload>(bodyJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (body == null || body.Items == null || body.Items.Count == 0)
                return JsonSerializer.Serialize(new { error = "No items to return" });

            var returnService = new ReturnService();
            var items = new List<ReturnItemInfo>();
            foreach (var i in body.Items)
            {
                items.Add(new ReturnItemInfo
                {
                    PartId = i.PartId,
                    Quantity = i.Qty,
                    RefundAmount = i.RefundAmount
                });
            }

            UserSession.Username = "AndroidWebPOS";
            returnService.ProcessReturn(body.OrderId, items, body.Reason);
            GlobalEvents.RaiseInventoryUpdated();
            return JsonSerializer.Serialize(new { success = true });
        }

        private string GetRecipes()
        {
            try
            {
                var list = RecipeData.GetAllRecipes();
                var result = new List<object>();
                foreach (var r in list)
                {
                    var partsList = new List<object>();
                    foreach (var rp in r.Parts)
                    {
                        partsList.Add(new
                        {
                            partId = rp.PartId,
                            partName = rp.PartName,
                            qty = rp.Quantity,
                            unitCost = (!Helpers.UserSession.IsAdmin) ? 0m : rp.UnitCost,
                            totalCost = (!Helpers.UserSession.IsAdmin) ? 0m : rp.TotalCost,
                            unitOfMeasure = rp.UnitOfMeasure
                        });
                    }
                    result.Add(new
                    {
                        id = r.Id,
                        name = r.RecipeName,
                        itemNo = r.ItemNo,
                        description = r.Description,
                        price = r.SellingPrice,
                        totalCost = (!Helpers.UserSession.IsAdmin) ? 0m : r.TotalCost,
                        categoryName = r.CategoryName,
                        image = CleanImagePrefix(r.RecipeImage),
                        parts = partsList,
                        yieldQuantity = r.YieldQuantity,
                        yieldUnit = r.YieldUnit
                    });
                }
                return JsonSerializer.Serialize(result);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.GetRecipes");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private string GetRecipesSalesFrequency()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteDataTable(
                    @"SELECT recipe_id, SUM(quantity) as sales_count
                      FROM order_items
                      WHERE item_type = 'Recipe' AND recipe_id IS NOT NULL AND recipe_id > 0
                      GROUP BY recipe_id");

                var freq = new Dictionary<int, double>();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    if (row["recipe_id"] != DBNull.Value && row["sales_count"] != DBNull.Value)
                    {
                        int rId = Convert.ToInt32(row["recipe_id"]);
                        double count = Convert.ToDouble(row["sales_count"]);
                        freq[rId] = count;
                    }
                }
                return JsonSerializer.Serialize(freq);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.GetRecipesSalesFrequency");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private string ProcessSaveRecipe(string bodyJson)
        {
            try
            {
                var body = JsonSerializer.Deserialize<RecipePayload>(bodyJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (body == null || string.IsNullOrEmpty(body.Name))
                    return JsonSerializer.Serialize(new { error = "Missing recipe name" });

                var recipe = new RecipeData
                {
                    RecipeName = body.Name,
                    ItemNo = body.ItemNo ?? "",
                    Description = body.Description ?? "",
                    SellingPrice = body.Price,
                    CategoryName = body.CategoryName ?? "",
                    RecipeImage = body.Image ?? "",
                    YieldQuantity = body.YieldQuantity ?? 1m,
                    YieldUnit = body.YieldUnit ?? "",
                    Status = "Active"
                };

                if (body.Ingredients != null)
                {
                    foreach (var ing in body.Ingredients)
                    {
                        recipe.Parts.Add(new RecipePartData
                        {
                            PartId = ing.PartId,
                            Quantity = ing.Qty,
                            UnitOfMeasure = ing.UnitOfMeasure
                        });
                    }
                }

                if (body.Id.HasValue && body.Id.Value > 0)
                {
                    recipe.Id = body.Id.Value;
                    RecipeData.UpdateRecipe(recipe);
                }
                else
                {
                    RecipeData.AddRecipe(recipe);
                }

                return JsonSerializer.Serialize(new { success = true });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.ProcessSaveRecipe");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private string ProcessBulkImport(string bodyJson)
        {
            try
            {
                var body = JsonSerializer.Deserialize<BulkImportPayload>(bodyJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (body == null || body.Items == null || body.Items.Count == 0)
                    return JsonSerializer.Serialize(new { error = "No items to import" });

                int imported = 0;
                int skipped = 0;

                foreach (var item in body.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.Name))
                    {
                        skipped++;
                        continue;
                    }

                    int existingId = 0;
                    if (!string.IsNullOrEmpty(item.Barcode))
                    {
                        existingId = DatabaseHelper.ExecuteScalar<int>(
                            "SELECT id FROM parts WHERE barcode = @b AND date_deleted IS NULL",
                            new SqliteParameter("@b", item.Barcode.Trim()));
                    }
                    if (existingId == 0 && !string.IsNullOrEmpty(item.Sku))
                    {
                        existingId = DatabaseHelper.ExecuteScalar<int>(
                            "SELECT id FROM parts WHERE part_number = @s AND date_deleted IS NULL",
                            new SqliteParameter("@s", item.Sku.Trim()));
                    }
                    if (existingId == 0 && !string.IsNullOrEmpty(item.Name))
                    {
                        existingId = DatabaseHelper.ExecuteScalar<int>(
                            "SELECT id FROM parts WHERE LOWER(part_name) = LOWER(@n) AND date_deleted IS NULL",
                            new SqliteParameter("@n", item.Name.Trim()));
                    }

                    string categoryName = item.Category ?? "General";
                    int catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                new SqliteParameter("@c", categoryName.Trim()));
                    if (catId == 0)
                    {
                        try
                        {
                            DatabaseHelper.ExecuteNonQuery("INSERT INTO categories (category_name) VALUES (@c)", new SqliteParameter("@c", categoryName.Trim()));
                            catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                        new SqliteParameter("@c", categoryName.Trim()));
                        }
                        catch
                        {
                            catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                        new SqliteParameter("@c", categoryName.Trim()));
                        }
                    }
                    if (catId == 0) catId = 1;

                    double pSize = item.PackSize > 0 ? item.PackSize : 1.0;
                    double conv = item.ConversionValue > 0 ? item.ConversionValue : 1.0;
                    decimal packPrice = item.PackPrice;
                    if (packPrice == 0 && item.Price > 0) packPrice = item.Price; // fallback

                    decimal itemCost = IngredientCalculationEngine.CalculateCostPerBigUnit(packPrice, pSize);
                    decimal pieceCost = IngredientCalculationEngine.CalculateCostPerSmallUnit(packPrice, pSize, conv);
                    double stockPacks = IngredientCalculationEngine.ConvertStockToPacks(item.Stock, pSize);

                    double oldStock = 0;
                    if (existingId > 0)
                    {
                        oldStock = DatabaseHelper.ExecuteScalar<double>(
                            "SELECT COALESCE(quantity_in_stock, 0) FROM parts WHERE id = @id",
                            new SqliteParameter("@id", existingId));
                    }

                    if (existingId > 0)
                    {
                        string sqlUpdate = @"
                            UPDATE parts SET part_name = @name, part_number = @sku, category_id = @cat, purchase_price = @p_price, selling_price = @s_price,
                                             quantity_in_stock = @stock, barcode = @barcode, description = @desc, item_no = @itemNo,
                                             big_unit = @big, small_unit = @small, conversion_value = @conv, pack_size = @pack_size,
                                             pack_price = @pack_price, piece_price = @piece_price, item_price = @item_price, minimum_stock_level = @min
                            WHERE id = @id";

                        DatabaseHelper.ExecuteNonQuery(sqlUpdate,
                            new SqliteParameter("@name", item.Name),
                            new SqliteParameter("@sku", item.Sku ?? ""),
                            new SqliteParameter("@cat", catId),
                            new SqliteParameter("@p_price", packPrice),
                            new SqliteParameter("@s_price", item.Price),
                            new SqliteParameter("@stock", stockPacks),
                            new SqliteParameter("@barcode", item.Barcode ?? ""),
                            new SqliteParameter("@desc", item.Description ?? ""),
                            new SqliteParameter("@itemNo", item.ItemNo ?? ""),
                            new SqliteParameter("@big", item.BigUnit ?? ""),
                            new SqliteParameter("@small", item.SmallUnit ?? ""),
                            new SqliteParameter("@conv", conv),
                            new SqliteParameter("@pack_size", pSize),
                            new SqliteParameter("@pack_price", packPrice),
                            new SqliteParameter("@piece_price", pieceCost),
                            new SqliteParameter("@item_price", itemCost),
                            new SqliteParameter("@min", item.MinStock > 0 ? item.MinStock : 5),
                            new SqliteParameter("@id", existingId));
                    }
                    else
                    {
                        string sql = @"
                            INSERT INTO parts (part_name, part_number, category_id, purchase_price, selling_price, quantity_in_stock, barcode, status, description, item_no,
                                               big_unit, small_unit, conversion_value, pack_size, pack_price, piece_price, item_price, minimum_stock_level)
                            VALUES (@name, @sku, @cat, @p_price, @s_price, @stock, @barcode, 'Active', @desc, @itemNo,
                                    @big, @small, @conv, @pack_size, @pack_price, @piece_price, @item_price, @min)";

                        DatabaseHelper.ExecuteNonQuery(sql,
                            new SqliteParameter("@name", item.Name),
                            new SqliteParameter("@sku", item.Sku ?? ""),
                            new SqliteParameter("@cat", catId),
                            new SqliteParameter("@p_price", packPrice),
                            new SqliteParameter("@s_price", item.Price),
                            new SqliteParameter("@stock", stockPacks),
                            new SqliteParameter("@barcode", item.Barcode ?? ""),
                            new SqliteParameter("@desc", item.Description ?? ""),
                            new SqliteParameter("@itemNo", item.ItemNo ?? ""),
                            new SqliteParameter("@big", item.BigUnit ?? ""),
                            new SqliteParameter("@small", item.SmallUnit ?? ""),
                            new SqliteParameter("@conv", conv),
                            new SqliteParameter("@pack_size", pSize),
                            new SqliteParameter("@pack_price", packPrice),
                            new SqliteParameter("@piece_price", pieceCost),
                            new SqliteParameter("@item_price", itemCost),
                            new SqliteParameter("@min", item.MinStock > 0 ? item.MinStock : 5));
                    }

                    double delta = stockPacks - oldStock;
                    if (delta != 0)
                    {
                        string action = delta > 0 ? "ADJUST_IN" : "ADJUST_OUT";
                        DatabaseHelper.LogTransaction(action, item.Name, $"Adjusted stock of {item.Name} by {delta:F4}. Reason: Bulk Import (New Qty: {stockPacks})");
                    }

                    imported++;
                }

                if (imported > 0)
                {
                    DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Imported inventory");
                }
                return JsonSerializer.Serialize(new { success = true, imported, skipped });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.ProcessBulkImport");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private string ProcessBulkRecipesImport(string bodyJson)
        {
            try
            {
                var body = JsonSerializer.Deserialize<BulkImportRecipesPayload>(bodyJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (body == null || body.Recipes == null || body.Recipes.Count == 0)
                    return JsonSerializer.Serialize(new { error = "No recipes to import" });

                int imported = 0;
                int skipped = 0;

                foreach (var recipeItem in body.Recipes)
                {
                    if (string.IsNullOrWhiteSpace(recipeItem.Name))
                    {
                        skipped++;
                        continue;
                    }

                    // Get or create category
                    string categoryName = recipeItem.CategoryName ?? "General";
                    if (string.IsNullOrWhiteSpace(categoryName)) categoryName = "General";

                    int catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                new SqliteParameter("@c", categoryName.Trim()));
                    if (catId == 0)
                    {
                        try
                        {
                            DatabaseHelper.ExecuteNonQuery("INSERT INTO categories (category_name) VALUES (@c)", new SqliteParameter("@c", categoryName.Trim()));
                            catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                        new SqliteParameter("@c", categoryName.Trim()));
                        }
                        catch
                        {
                            catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                        new SqliteParameter("@c", categoryName.Trim()));
                        }
                    }
                    if (catId == 0) catId = 1;

                    // Check if recipe already exists (active or soft-deleted)
                    int existingId = DatabaseHelper.ExecuteScalar<int>(
                        "SELECT id FROM recipes WHERE LOWER(recipe_name) = LOWER(@n)",
                        new SqliteParameter("@n", recipeItem.Name.Trim()));

                    int recipeId = 0;
                    if (existingId > 0)
                    {
                        // Update recipe and restore it if soft-deleted
                        string sqlUpdate = @"UPDATE recipes SET description = @desc, selling_price = @price, category_id = @catId, item_no = @itemNo, date_deleted = NULL, status = 'Active' WHERE id = @id";
                        DatabaseHelper.ExecuteNonQuery(sqlUpdate,
                            new SqliteParameter("@desc", recipeItem.Description ?? ""),
                            new SqliteParameter("@price", recipeItem.Price),
                            new SqliteParameter("@catId", catId),
                            new SqliteParameter("@itemNo", recipeItem.ItemNo ?? ""),
                            new SqliteParameter("@id", existingId));
                        recipeId = existingId;

                        // Delete existing ingredients mapping
                        DatabaseHelper.ExecuteNonQuery("DELETE FROM recipe_parts WHERE recipe_id = @r_id", new SqliteParameter("@r_id", recipeId));
                    }
                    else
                    {
                        // Insert recipe
                        string sqlInsert = @"INSERT INTO recipes (recipe_name, description, selling_price, status, date_added, category_id, item_no)
                                             VALUES (@name, @desc, @price, 'Active', datetime('now'), @catId, @itemNo);
                                             SELECT last_insert_rowid();";
                        recipeId = (int)DatabaseHelper.ExecuteScalar<long>(sqlInsert,
                            new SqliteParameter("@name", recipeItem.Name.Trim()),
                            new SqliteParameter("@desc", recipeItem.Description ?? ""),
                            new SqliteParameter("@price", recipeItem.Price),
                            new SqliteParameter("@catId", catId),
                            new SqliteParameter("@itemNo", recipeItem.ItemNo ?? ""));
                    }

                    if (recipeId == 0)
                    {
                        skipped++;
                        continue;
                    }

                    // Insert ingredients/parts
                    if (recipeItem.Ingredients != null)
                    {
                        foreach (var ing in recipeItem.Ingredients)
                        {
                            if (string.IsNullOrWhiteSpace(ing.Name)) continue;

                            // Lookup ingredient (part) ID by name
                            int partId = DatabaseHelper.ExecuteScalar<int>(
                                "SELECT id FROM parts WHERE LOWER(part_name) = LOWER(@pname) AND date_deleted IS NULL",
                                new SqliteParameter("@pname", ing.Name.Trim()));

                            if (partId > 0)
                            {
                                DatabaseHelper.ExecuteNonQuery(
                                    "INSERT INTO recipe_parts (recipe_id, part_id, quantity, unit_of_measure) VALUES (@r_id, @p_id, @qty, @uom)",
                                    new SqliteParameter("@r_id", recipeId),
                                    new SqliteParameter("@p_id", partId),
                                    new SqliteParameter("@qty", ing.Qty),
                                    new SqliteParameter("@uom", ing.UnitOfMeasure ?? ""));
                            }
                        }
                    }

                    imported++;
                }

                return JsonSerializer.Serialize(new { success = true, imported, skipped });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.ProcessBulkRecipesImport");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private string ProcessDailySalesImport(string bodyJson)
        {
            try
            {
                var body = JsonSerializer.Deserialize<DailySalesImportPayload>(bodyJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (body == null || body.Sales == null || body.Sales.Count == 0)
                    return JsonSerializer.Serialize(new { error = "No sales data to process" });

                int recipesProcessed = 0;
                int recipesSkipped = 0;
                var skippedRecipes = new List<string>();
                var ingredientDeductions = new List<IngredientDeductionResult>();

                // Cache active recipes, parts, and recipe parts in-memory
                var recipes = new List<RecipeCacheItem>();
                var parts = new List<PartCacheItem>();
                var recipeParts = new List<RecipePartDeduction>();

                using (var conn = new SqliteConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    // Load recipes
                    string sqlRecipes = "SELECT id, recipe_name, item_no, selling_price FROM recipes WHERE date_deleted IS NULL";
                    using (var cmd = new SqliteCommand(sqlRecipes, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            recipes.Add(new RecipeCacheItem
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                RecipeName = reader["recipe_name"].ToString(),
                                ItemNo = reader["item_no"]?.ToString() ?? "",
                                SellingPrice = reader["selling_price"] != DBNull.Value ? Convert.ToDecimal(reader["selling_price"]) : 0m
                            });
                        }
                    }

                    // Load parts
                    string sqlParts = @"SELECT id, part_name, item_no, selling_price, quantity_in_stock, unit_of_measure, stock_type, pack_items_number,
                                               big_unit, small_unit, conversion_value, pack_size, purchase_price 
                                        FROM parts 
                                        WHERE date_deleted IS NULL";
                    using (var cmd = new SqliteCommand(sqlParts, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            parts.Add(new PartCacheItem
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                PartName = reader["part_name"].ToString(),
                                ItemNo = reader["item_no"]?.ToString() ?? "",
                                SellingPrice = reader["selling_price"] != DBNull.Value ? Convert.ToDecimal(reader["selling_price"]) : 0m,
                                QuantityInStock = reader["quantity_in_stock"] != DBNull.Value ? Convert.ToDouble(reader["quantity_in_stock"]) : 0.0,
                                UnitOfMeasure = reader["unit_of_measure"]?.ToString() ?? "",
                                StockType = reader["stock_type"]?.ToString() ?? "Piece",
                                PackItemsNumber = reader["pack_items_number"] != DBNull.Value ? Convert.ToInt32(reader["pack_items_number"]) : 1,
                                BigUnit = reader["big_unit"]?.ToString() ?? "",
                                SmallUnit = reader["small_unit"]?.ToString() ?? "",
                                ConversionValue = reader["conversion_value"] != DBNull.Value ? Convert.ToDouble(reader["conversion_value"]) : 1.0,
                                PackSize = reader["pack_size"] != DBNull.Value ? Convert.ToDouble(reader["pack_size"]) : 1.0,
                                PurchasePrice = reader["purchase_price"] != DBNull.Value ? Convert.ToDecimal(reader["purchase_price"]) : 0m
                            });
                        }
                    }

                    // Load recipe parts
                    string sqlRecipeParts = @"SELECT rp.recipe_id, rp.part_id, rp.quantity, rp.unit_of_measure as recipe_uom, p.part_name, p.quantity_in_stock, p.unit_of_measure as part_uom, p.stock_type, p.pack_items_number,
                                                     p.big_unit, p.small_unit, p.conversion_value, p.pack_size
                                              FROM recipe_parts rp
                                              JOIN parts p ON rp.part_id = p.id
                                              WHERE p.date_deleted IS NULL";
                    using (var cmd = new SqliteCommand(sqlRecipeParts, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            recipeParts.Add(new RecipePartDeduction
                            {
                                RecipeId = Convert.ToInt32(reader["recipe_id"]),
                                PartId = Convert.ToInt32(reader["part_id"]),
                                PartName = reader["part_name"].ToString(),
                                QtyPerRecipe = Convert.ToDouble(reader["quantity"]),
                                CurrentStock = Convert.ToDouble(reader["quantity_in_stock"]),
                                RecipeUom = reader["recipe_uom"]?.ToString() ?? "",
                                PartUom = reader["part_uom"]?.ToString() ?? "",
                                StockType = reader["stock_type"]?.ToString() ?? "Piece",
                                PackItems = reader["pack_items_number"] != DBNull.Value ? Convert.ToInt32(reader["pack_items_number"]) : 1,
                                BigUnit = reader["big_unit"]?.ToString() ?? "",
                                SmallUnit = reader["small_unit"]?.ToString() ?? "",
                                ConversionValue = reader["conversion_value"] != DBNull.Value ? Convert.ToDouble(reader["conversion_value"]) : 1.0,
                                PackSize = reader["pack_size"] != DBNull.Value ? Convert.ToDouble(reader["pack_size"]) : 1.0
                            });
                        }
                    }
                }

                // Helper functions for matching
                Func<string, string, bool> itemNoMatches = (no1, no2) =>
                {
                    if (no1 == null || no2 == null) return false;
                    return no1.Trim().Equals(no2.Trim(), StringComparison.OrdinalIgnoreCase);
                };

                Func<string, string, bool> nameMatches = (name1, name2) =>
                {
                    if (name1 == null || name2 == null) return false;
                    string clean1 = name1.ToLower().Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
                    string clean2 = name2.ToLower().Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
                    return clean1 == clean2;
                };

                using (var conn = new SqliteConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        var itemsForOrder = new List<Tuple<bool, int, double, decimal, string>>(); // <isRecipe, id, qty, price, uom>

                        foreach (var sale in body.Sales)
                        {
                            string cleanName = (sale.RecipeName ?? "").Replace("\u00A0", " ").Trim();
                            string cleanItemNo = (sale.ItemNo ?? "").Replace("\u00A0", " ").Trim();

                            if (string.IsNullOrWhiteSpace(cleanName) && string.IsNullOrWhiteSpace(cleanItemNo))
                            {
                                recipesSkipped++;
                                continue;
                            }
                            if (sale.QtySold <= 0)
                            {
                                recipesSkipped++;
                                continue;
                            }

                            int recipeId = 0;
                            string exactRecipeName = "";
                            decimal sellingPrice = 0;

                            // 1. Find the recipe ID and selling price by Item No (if provided)
                            RecipeCacheItem matchedRecipe = null;
                            if (!string.IsNullOrWhiteSpace(cleanItemNo))
                            {
                                matchedRecipe = recipes.FirstOrDefault(r => itemNoMatches(r.ItemNo, cleanItemNo));
                            }

                            // 2. Find the recipe ID and selling price by name (fallback, case-insensitive and ignoring all whitespace)
                            if (matchedRecipe == null && !string.IsNullOrWhiteSpace(cleanName))
                            {
                                matchedRecipe = recipes.FirstOrDefault(r => nameMatches(r.RecipeName, cleanName));
                            }

                            if (matchedRecipe != null)
                            {
                                recipeId = matchedRecipe.Id;
                                exactRecipeName = matchedRecipe.RecipeName;
                                sellingPrice = matchedRecipe.SellingPrice;
                            }

                            if (recipeId == 0)
                            {
                                PartCacheItem matchedPart = null;
                                
                                // Fallback A: Search the parts (inventory) table directly by Item No (if provided)
                                if (!string.IsNullOrWhiteSpace(cleanItemNo))
                                {
                                    matchedPart = parts.FirstOrDefault(p => itemNoMatches(p.ItemNo, cleanItemNo));
                                }

                                // Fallback B: Search the parts (inventory) table directly by name (case-insensitive and ignoring all whitespace)
                                if (matchedPart == null && !string.IsNullOrWhiteSpace(cleanName))
                                {
                                    matchedPart = parts.FirstOrDefault(p => nameMatches(p.PartName, cleanName));
                                }

                                if (matchedPart == null)
                                {
                                    recipesSkipped++;
                                    string missingName = !string.IsNullOrWhiteSpace(cleanName) ? cleanName : (!string.IsNullOrWhiteSpace(cleanItemNo) ? "Item No " + cleanItemNo : "Unknown");
                                    if (!skippedRecipes.Contains(missingName))
                                        skippedRecipes.Add(missingName);
                                    continue;
                                }

                                string saleUom = sale.UnitOfMeasure;
                                if (string.IsNullOrEmpty(saleUom))
                                {
                                    saleUom = !string.IsNullOrEmpty(matchedPart.SmallUnit) ? matchedPart.SmallUnit : matchedPart.UnitOfMeasure;
                                }

                                // Convert the quantity using the new unit conversion logic
                                double totalDeduct = RecipePartData.GetConvertedQuantityDynamic(
                                    sale.QtySold, saleUom, matchedPart.UnitOfMeasure, matchedPart.StockType, matchedPart.PackItemsNumber, matchedPart.BigUnit, matchedPart.SmallUnit, matchedPart.ConversionValue, matchedPart.PackSize);

                                // Calculate the selling price dynamically based on the unit of sale
                                decimal unitPrice = IngredientCalculationEngine.CalculatedUnitCost(
                                    matchedPart.SellingPrice, saleUom, matchedPart.BigUnit, matchedPart.SmallUnit, matchedPart.ConversionValue, matchedPart.PackSize, matchedPart.StockType, matchedPart.PackItemsNumber, matchedPart.UnitOfMeasure);
                                decimal roundedPrice = Math.Round(unitPrice, 2, MidpointRounding.AwayFromZero);

                                if (totalDeduct > 0)
                                {
                                    var existingDeduction = ingredientDeductions.FirstOrDefault(d => d.PartId == matchedPart.Id);
                                    double currentStockInDb = matchedPart.QuantityInStock;
                                    if (existingDeduction != null)
                                    {
                                        currentStockInDb = existingDeduction.NewStock;
                                    }

                                    double newStock = Math.Max(0, currentStockInDb - totalDeduct);
                                    matchedPart.QuantityInStock = newStock;

                                    if (existingDeduction != null)
                                    {
                                        existingDeduction.QtyDeducted += totalDeduct;
                                        existingDeduction.NewStock = newStock;
                                    }
                                    else
                                    {
                                        ingredientDeductions.Add(new IngredientDeductionResult
                                        {
                                            PartId = matchedPart.Id,
                                            PartName = matchedPart.PartName,
                                            QtyDeducted = totalDeduct,
                                            PreviousStock = currentStockInDb,
                                            NewStock = newStock
                                        });
                                    }
                                }

                                recipesProcessed++;
                                itemsForOrder.Add(Tuple.Create(false, matchedPart.Id, (double)sale.QtySold, roundedPrice, saleUom));
                                continue;
                            }

                            // 2. Find parts (ingredients) for this recipe from in-memory cache
                            var partsToDeduct = recipeParts.Where(rp => rp.RecipeId == recipeId).ToList();

                            if (partsToDeduct.Count == 0)
                            {
                                // Fallback: Check if this recipe name/item_no exists directly as a part (ingredient/product) in parts table
                                // and deduct it directly!
                                PartCacheItem fallbackPart = null;

                                // Search parts by Item No first
                                if (!string.IsNullOrWhiteSpace(cleanItemNo))
                                {
                                    fallbackPart = parts.FirstOrDefault(p => itemNoMatches(p.ItemNo, cleanItemNo));
                                }

                                // Search parts by Name next
                                if (fallbackPart == null && !string.IsNullOrWhiteSpace(cleanName))
                                {
                                    fallbackPart = parts.FirstOrDefault(p => nameMatches(p.PartName, cleanName));
                                }

                                if (fallbackPart != null)
                                {
                                    string saleUom = sale.UnitOfMeasure;
                                    if (string.IsNullOrEmpty(saleUom))
                                    {
                                        saleUom = !string.IsNullOrEmpty(fallbackPart.SmallUnit) ? fallbackPart.SmallUnit : fallbackPart.UnitOfMeasure;
                                    }

                                    double totalDeduct = RecipePartData.GetConvertedQuantityDynamic(
                                        sale.QtySold, saleUom, fallbackPart.UnitOfMeasure, fallbackPart.StockType, fallbackPart.PackItemsNumber, fallbackPart.BigUnit, fallbackPart.SmallUnit, fallbackPart.ConversionValue, fallbackPart.PackSize);
                                    
                                    decimal unitPrice = IngredientCalculationEngine.CalculatedUnitCost(
                                        fallbackPart.SellingPrice, saleUom, fallbackPart.BigUnit, fallbackPart.SmallUnit, fallbackPart.ConversionValue, fallbackPart.PackSize, fallbackPart.StockType, fallbackPart.PackItemsNumber, fallbackPart.UnitOfMeasure);
                                    decimal roundedPrice = Math.Round(unitPrice, 2, MidpointRounding.AwayFromZero);

                                    if (totalDeduct > 0)
                                    {
                                        var existingDeduction = ingredientDeductions.FirstOrDefault(d => d.PartId == fallbackPart.Id);
                                        double currentStockInDb = fallbackPart.QuantityInStock;
                                        if (existingDeduction != null)
                                        {
                                            currentStockInDb = existingDeduction.NewStock;
                                        }

                                        double newStock = Math.Max(0, currentStockInDb - totalDeduct);
                                        fallbackPart.QuantityInStock = newStock;

                                        if (existingDeduction != null)
                                        {
                                            existingDeduction.QtyDeducted += totalDeduct;
                                            existingDeduction.NewStock = newStock;
                                        }
                                        else
                                        {
                                            ingredientDeductions.Add(new IngredientDeductionResult
                                            {
                                                PartId = fallbackPart.Id,
                                                PartName = fallbackPart.PartName,
                                                QtyDeducted = totalDeduct,
                                                PreviousStock = currentStockInDb,
                                                NewStock = newStock
                                            });
                                        }
                                    }
                                    
                                    recipesProcessed++;
                                    itemsForOrder.Add(Tuple.Create(false, fallbackPart.Id, (double)sale.QtySold, roundedPrice, saleUom));
                                    continue;
                                }
                            }

                            // 3. Deduct ingredients from stock
                            foreach (var p in partsToDeduct)
                            {
                                double qtyPerRecipeConverted = RecipePartData.GetConvertedQuantityDynamic(
                                    p.QtyPerRecipe, p.RecipeUom, p.PartUom, p.StockType, p.PackItems, p.BigUnit, p.SmallUnit, p.ConversionValue, p.PackSize);
                                double totalDeduct = qtyPerRecipeConverted * (double)sale.QtySold;
                                if (totalDeduct <= 0) continue;

                                // Look up corresponding cached part to get latest stock level
                                var cachedPart = parts.FirstOrDefault(x => x.Id == p.PartId);
                                double currentStock = cachedPart != null ? cachedPart.QuantityInStock : p.CurrentStock;

                                // Check if we already processed this ingredient in a previous loop iteration of this upload
                                var existingDeduction = ingredientDeductions.FirstOrDefault(d => d.PartId == p.PartId);
                                if (existingDeduction != null)
                                {
                                    currentStock = existingDeduction.NewStock;
                                }

                                double newStock = Math.Max(0, currentStock - totalDeduct);
                                if (cachedPart != null)
                                {
                                    cachedPart.QuantityInStock = newStock;
                                }

                                if (existingDeduction != null)
                                {
                                    existingDeduction.QtyDeducted += totalDeduct;
                                    existingDeduction.NewStock = newStock;
                                }
                                else
                                {
                                    ingredientDeductions.Add(new IngredientDeductionResult
                                    {
                                        PartId = p.PartId,
                                        PartName = p.PartName,
                                        QtyDeducted = totalDeduct,
                                        PreviousStock = currentStock,
                                        NewStock = newStock
                                    });
                                }
                            }

                            recipesProcessed++;
                            itemsForOrder.Add(Tuple.Create(true, recipeId, (double)sale.QtySold, sellingPrice, ""));
                        }

                        if (itemsForOrder.Count > 0)
                        {
                            decimal totalOrderAmount = 0;
                            foreach (var item in itemsForOrder)
                            {
                                totalOrderAmount += item.Item4 * (decimal)item.Item3;
                            }

                            string sqlInsertOrder = @"INSERT INTO orders (order_date, total_amount, payment_status, amount_paid, status) 
                                                      VALUES (datetime('now'), @total, 'Paid', @paid, 'Completed');
                                                      SELECT last_insert_rowid();";
                            long orderId = 0;
                            using (var cmd = new SqliteCommand(sqlInsertOrder, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@total", totalOrderAmount);
                                cmd.Parameters.AddWithValue("@paid", totalOrderAmount);
                                orderId = (long)cmd.ExecuteScalar();
                            }

                            foreach (var item in itemsForOrder)
                            {
                                bool isRecipe = item.Item1;
                                int itemId = item.Item2;
                                double qty = item.Item3;
                                decimal price = item.Item4;
                                string uom = item.Item5;

                                string sqlInsertItem = isRecipe 
                                    ? @"INSERT INTO order_items (order_id, part_id, quantity, price, item_type, recipe_id, unit_of_measure) 
                                        VALUES (@orderId, 0, @qty, @price, 'Recipe', @recipeId, @uom)"
                                    : @"INSERT INTO order_items (order_id, part_id, quantity, price, item_type, recipe_id, unit_of_measure) 
                                        VALUES (@orderId, @partId, @qty, @price, 'Part', NULL, @uom)";
                                using (var cmd = new SqliteCommand(sqlInsertItem, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@orderId", orderId);
                                    cmd.Parameters.AddWithValue("@qty", qty);
                                    cmd.Parameters.AddWithValue("@price", price);
                                    cmd.Parameters.AddWithValue("@uom", uom ?? "pack");
                                    if (isRecipe)
                                    {
                                        cmd.Parameters.AddWithValue("@recipeId", itemId);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@partId", itemId);
                                    }
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            string sqlInsertTxOrder = @"INSERT INTO transactions (action_type, part_name, description, username) 
                                                        VALUES ('SALE', @partName, @desc, 'Admin')";
                            string orderDesc = $"Order #{orderId} placed via daily sales import -- Total: {totalOrderAmount:C}";
                            using (var cmd = new SqliteCommand(sqlInsertTxOrder, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@partName", "POS Sale");
                                cmd.Parameters.AddWithValue("@desc", orderDesc);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Apply aggregated database updates and record transactions
                        foreach (var d in ingredientDeductions)
                        {
                            string sqlUpdatePart = "UPDATE parts SET quantity_in_stock = @newStock WHERE id = @partId";
                            using (var cmd = new SqliteCommand(sqlUpdatePart, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@newStock", d.NewStock);
                                cmd.Parameters.AddWithValue("@partId", d.PartId);
                                cmd.ExecuteNonQuery();
                            }

                            string sqlInsertTx = @"INSERT INTO transactions (action_type, part_name, description, username) 
                                                  VALUES ('STOCK_DEDUCT', @partName, @desc, 'Admin')";
                            string txDesc = $"Deducted {d.QtyDeducted:0.##} via sales import -- Stock: {d.PreviousStock:0.##} -> {d.NewStock:0.##} (ID: {d.PartId})";
                            using (var cmd = new SqliteCommand(sqlInsertTx, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@partName", d.PartName);
                                cmd.Parameters.AddWithValue("@desc", txDesc);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }

                DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Imported sales");
                GlobalEvents.RaiseInventoryUpdated();
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    processed = recipesProcessed,
                    skipped = recipesSkipped,
                    skippedNames = skippedRecipes,
                    deductions = ingredientDeductions
                });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.ProcessDailySalesImport");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private string ProcessExportCsv(string bodyJson)
        {
            try
            {
                var body = JsonSerializer.Deserialize<ExportCsvPayload>(bodyJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (body == null || (string.IsNullOrEmpty(body.CsvContent) && string.IsNullOrEmpty(body.Base64Content)))
                    return JsonSerializer.Serialize(new { error = "No content to export" });

                string filename = string.IsNullOrEmpty(body.Filename) ? $"inventory_{DateTime.Now:yyyyMMdd_HHmmss}.csv" : body.Filename;

                var values = new Android.Content.ContentValues();
                values.Put(Android.Provider.MediaStore.MediaColumns.DisplayName, filename);
                
                string mimeType = "text/csv";
                if (filename.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
                else if (filename.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    mimeType = "application/vnd.ms-excel";
                }
                values.Put(Android.Provider.MediaStore.MediaColumns.MimeType, mimeType);
                values.Put(Android.Provider.MediaStore.MediaColumns.RelativePath, Android.OS.Environment.DirectoryDownloads);

                var resolver = _context.ContentResolver;
                var uri = resolver.Insert(Android.Provider.MediaStore.Downloads.ExternalContentUri, values);
                if (uri != null)
                {
                    using (var stream = resolver.OpenOutputStream(uri))
                    {
                        if (!string.IsNullOrEmpty(body.Base64Content))
                        {
                            byte[] bytes = Convert.FromBase64String(body.Base64Content);
                            stream.Write(bytes, 0, bytes.Length);
                        }
                        else
                        {
                            using (var writer = new StreamWriter(stream))
                            {
                                writer.Write(body.CsvContent);
                            }
                        }
                    }
                    return JsonSerializer.Serialize(new { success = true, path = $"Downloads/{filename}" });
                }

                return JsonSerializer.Serialize(new { error = "Failed to create file in Downloads directory" });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.ProcessExportCsv");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private string GetMonthlyReportData(int month, int year, int? day = null)
        {
            try
            {
                DateTime targetMonthStart = day.HasValue ? new DateTime(year, month, day.Value, 0, 0, 0) : new DateTime(year, month, 1, 0, 0, 0);
                DateTime targetMonthEnd = day.HasValue ? new DateTime(year, month, day.Value, 23, 59, 59) : new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
                string startStr = targetMonthStart.ToString("yyyy-MM-dd HH:mm:ss");
                string endStr = targetMonthEnd.ToString("yyyy-MM-dd HH:mm:ss");

                // Basic legacy KPIs
                decimal totalRevenue = DatabaseHelper.ExecuteScalar<decimal>(
                    "SELECT COALESCE(SUM(total_amount), 0) FROM orders WHERE status = 'Completed' AND order_date >= @start AND order_date <= @end",
                    new SqliteParameter("@start", startStr),
                    new SqliteParameter("@end", endStr));

                int totalOrders = DatabaseHelper.ExecuteScalar<int>(
                    @"SELECT COALESCE(COUNT(DISTINCT o.order_id), 0) 
                      FROM orders o 
                      WHERE o.status = 'Completed' AND o.order_date >= @start AND o.order_date <= @end",
                    new SqliteParameter("@start", startStr),
                    new SqliteParameter("@end", endStr));

                int outOfStock = DatabaseHelper.ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM parts 
                      WHERE CAST(quantity_in_stock AS REAL) <= 0 
                        AND date_deleted IS NULL 
                        AND (status IS NULL OR LOWER(status) = 'active')");

                int lowStock = DatabaseHelper.ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM parts 
                      WHERE CAST(quantity_in_stock AS REAL) <= CAST(minimum_stock_level AS REAL) 
                        AND CAST(quantity_in_stock AS REAL) > 0 
                        AND date_deleted IS NULL 
                        AND (status IS NULL OR LOWER(status) = 'active')");

                // Categories Breakdown for Compatibility
                var categorySales = new List<object>();
                DataTable dtCat = DatabaseHelper.ExecuteDataTable(
                    @"SELECT 
                          CASE 
                              WHEN oi.item_type = 'Recipe' THEN 'Recipes'
                              ELSE COALESCE(c.category_name, 'General')
                          END as category_name,
                          SUM(oi.quantity * oi.price) as total_sales
                      FROM order_items oi
                      LEFT JOIN parts p ON oi.part_id = p.id AND oi.item_type = 'Part'
                      LEFT JOIN categories c ON p.category_id = c.id AND oi.item_type = 'Part'
                      JOIN orders o ON oi.order_id = o.order_id
                      WHERE o.status = 'Completed' AND o.order_date >= @start AND o.order_date <= @end
                      GROUP BY category_name
                      ORDER BY total_sales DESC",
                    new SqliteParameter("@start", startStr),
                    new SqliteParameter("@end", endStr));
                foreach (DataRow row in dtCat.Rows)
                {
                    categorySales.Add(new
                    {
                        category = row["category_name"].ToString(),
                        sales = Convert.ToDecimal(row["total_sales"])
                    });
                }

                // Recent transactions inside the target period
                var recentTransactions = new List<object>();
                DataTable dtTx = DatabaseHelper.ExecuteDataTable(
                    @"SELECT action_type, part_name, description, timestamp, username
                      FROM transactions
                      WHERE timestamp >= @start AND timestamp <= @end
                      ORDER BY id DESC LIMIT 50",
                    new SqliteParameter("@start", startStr),
                    new SqliteParameter("@end", endStr));
                foreach (DataRow row in dtTx.Rows)
                {
                    recentTransactions.Add(new
                    {
                        action = row["action_type"].ToString(),
                        item = row["part_name"].ToString(),
                        desc = row["description"].ToString(),
                        time = row["timestamp"].ToString(),
                        user = row["username"].ToString()
                    });
                }

                // --- 1. Load active parts details ---
                DataTable dtParts = DatabaseHelper.ExecuteDataTable(
                    @"SELECT id, part_name, quantity_in_stock, pack_size, purchase_price, pack_price, 
                             big_unit, small_unit, conversion_value, unit_of_measure, stock_type,
                             pack_items_number, minimum_stock_level
                      FROM parts WHERE date_deleted IS NULL");

                var partsDict = new Dictionary<int, ReportPartItem>();
                var partsByName = new Dictionary<string, ReportPartItem>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow r in dtParts.Rows)
                {
                    int id = Convert.ToInt32(r["id"]);
                    var pi = new ReportPartItem
                    {
                        Id = id,
                        Name = r["part_name"].ToString(),
                        CurrentStock = Convert.ToDouble(r["quantity_in_stock"]),
                        PackSize = r["pack_size"] != DBNull.Value ? Convert.ToDouble(r["pack_size"]) : 1.0,
                        PackPrice = r["pack_price"] != DBNull.Value && Convert.ToDecimal(r["pack_price"]) > 0 ? Convert.ToDecimal(r["pack_price"]) : (r["purchase_price"] != DBNull.Value ? Convert.ToDecimal(r["purchase_price"]) : 0m),
                        BigUnit = r["big_unit"]?.ToString() ?? "",
                        SmallUnit = r["small_unit"]?.ToString() ?? "",
                        ConversionValue = r["conversion_value"] != DBNull.Value ? Convert.ToDouble(r["conversion_value"]) : 1.0,
                        PartUom = r["unit_of_measure"]?.ToString() ?? "",
                        StockType = r["stock_type"]?.ToString() ?? "",
                        PackItemsNumber = r["pack_items_number"] != DBNull.Value ? Convert.ToInt32(r["pack_items_number"]) : 1,
                        MinStock = r["minimum_stock_level"] != DBNull.Value ? Convert.ToInt32(r["minimum_stock_level"]) : 5
                    };
                    partsDict[id] = pi;
                    partsByName[pi.Name.Trim()] = pi;
                }

                // --- 2. Query all Order Items & Recipe parts in selected period ---
                double totalRecipesSold = 0;
                double totalIngredientsSold = 0;
                decimal totalCogs = 0;

                // Load all recipe sales
                DataTable dtRecipeSales = DatabaseHelper.ExecuteDataTable(
                    @"SELECT oi.recipe_id, r.recipe_name, SUM(oi.quantity) as qty_sold, SUM(oi.quantity * oi.price) as revenue
                      FROM order_items oi
                      JOIN orders o ON oi.order_id = o.order_id
                      JOIN recipes r ON oi.recipe_id = r.id
                      WHERE o.status = 'Completed' AND o.order_date >= @start AND o.order_date <= @end
                      GROUP BY oi.recipe_id",
                    new SqliteParameter("@start", startStr),
                    new SqliteParameter("@end", endStr));

                var recipeSummaries = new List<object>();
                foreach (DataRow rRow in dtRecipeSales.Rows)
                {
                    int recipeId = Convert.ToInt32(rRow["recipe_id"]);
                    string name = rRow["recipe_name"].ToString();
                    double qtySold = Convert.ToDouble(rRow["qty_sold"]);
                    decimal revenue = Convert.ToDecimal(rRow["revenue"]);

                    // Calculate ingredient cost of this recipe
                    decimal recipeUnitCost = 0;
                    DataTable dtRecipeParts = DatabaseHelper.ExecuteDataTable(
                        @"SELECT rp.part_id, rp.quantity, rp.unit_of_measure
                          FROM recipe_parts rp
                          WHERE rp.recipe_id = @rid",
                        new SqliteParameter("@rid", recipeId));
                    foreach (DataRow rpRow in dtRecipeParts.Rows)
                    {
                        int partId = Convert.ToInt32(rpRow["part_id"]);
                        double rpQty = Convert.ToDouble(rpRow["quantity"]);
                        string rpUom = rpRow["unit_of_measure"]?.ToString();

                        if (partsDict.TryGetValue(partId, out var part))
                        {
                            double rpQtyPacks = Shaheen_InventoryManagement_Android.Data.RecipePartData.GetConvertedQuantityDynamic(
                                rpQty, rpUom, part.PartUom, part.StockType, part.PackItemsNumber, part.BigUnit, part.SmallUnit, part.ConversionValue, part.PackSize);
                            recipeUnitCost += (decimal)rpQtyPacks * part.PackPrice;

                            // Update part consumption for target month
                            part.UsedInRecipesPacks += rpQtyPacks * qtySold;
                        }
                    }

                    decimal totalCost = recipeUnitCost * (decimal)qtySold;
                    totalCogs += totalCost;
                    totalRecipesSold += qtySold;

                    recipeSummaries.Add(new
                    {
                        name = name,
                        qtySold = qtySold,
                        revenue = revenue,
                        cost = (!Helpers.UserSession.IsAdmin) ? 0m : totalCost,
                        profit = (!Helpers.UserSession.IsAdmin) ? 0m : (revenue - totalCost)
                    });
                }

                // Load direct ingredient sales
                DataTable dtDirectSales = DatabaseHelper.ExecuteDataTable(
                    @"SELECT oi.part_id, oi.quantity, oi.price, oi.unit_of_measure
                      FROM order_items oi
                      JOIN orders o ON oi.order_id = o.order_id
                      WHERE o.status = 'Completed' AND oi.item_type = 'Part' AND o.order_date >= @start AND o.order_date <= @end",
                    new SqliteParameter("@start", startStr),
                    new SqliteParameter("@end", endStr));

                foreach (DataRow dsRow in dtDirectSales.Rows)
                {
                    int partId = Convert.ToInt32(dsRow["part_id"]);
                    double qty = Convert.ToDouble(dsRow["quantity"]);
                    decimal price = Convert.ToDecimal(dsRow["price"]);
                    string uom = dsRow["unit_of_measure"]?.ToString();

                    if (partsDict.TryGetValue(partId, out var part))
                    {
                        double qtyPacks = Shaheen_InventoryManagement_Android.Data.RecipePartData.GetConvertedQuantityDynamic(
                            qty, uom, part.PartUom, part.StockType, part.PackItemsNumber, part.BigUnit, part.SmallUnit, part.ConversionValue, part.PackSize);
                        part.SoldDirectlyPacks += qtyPacks;

                        decimal unitCost = IngredientCalculationEngine.CalculatedUnitCost(
                            part.PackPrice, uom, part.BigUnit, part.SmallUnit, part.ConversionValue, part.PackSize, part.StockType, part.PackItemsNumber, part.PartUom);
                        totalCogs += unitCost * (decimal)qty;
                        totalIngredientsSold += qty;
                    }
                }

                // --- 3. Process transactions to compute changes and history ---
                DataTable dtAllTx = DatabaseHelper.ExecuteDataTable(
                    @"SELECT action_type, part_name, description, timestamp
                      FROM transactions
                      WHERE timestamp >= @start",
                    new SqliteParameter("@start", startStr));

                foreach (DataRow txRow in dtAllTx.Rows)
                {
                    string actionType = txRow["action_type"].ToString();
                    string partName = txRow["part_name"].ToString().Trim();
                    string desc = txRow["description"].ToString();
                    DateTime txDate = DateTime.Parse(txRow["timestamp"].ToString());

                    if (partsByName.TryGetValue(partName, out var part))
                    {
                        double changePacks = 0;
                        var matchAdjust = System.Text.RegularExpressions.Regex.Match(desc, @"by\s+(-?\d+\.?\d*)");
                        if (matchAdjust.Success)
                        {
                            double.TryParse(matchAdjust.Groups[1].Value, out changePacks);
                        }
                        else
                        {
                            var matchAdd = System.Text.RegularExpressions.Regex.Match(desc, @"Qty:\s*(\d+\.?\d*)");
                            if (matchAdd.Success)
                            {
                                double.TryParse(matchAdd.Groups[1].Value, out changePacks);
                            }
                        }

                        if (changePacks != 0)
                        {
                            if (txDate <= targetMonthEnd)
                            {
                                if (changePacks > 0)
                                {
                                    part.PurchasedPacks += changePacks;
                                }
                                else
                                {
                                    part.AdjustedPacks += changePacks;
                                }
                            }
                            else
                            {
                                part.ChangesAfterPeriodPacks += changePacks;
                            }
                        }
                    }
                }

                // Also need to track sales/recipe consumption that occurred AFTER targetMonthEnd
                DataTable dtSalesAfter = DatabaseHelper.ExecuteDataTable(
                    @"SELECT oi.part_id, oi.recipe_id, oi.quantity, oi.item_type, oi.unit_of_measure
                      FROM order_items oi
                      JOIN orders o ON oi.order_id = o.order_id
                      WHERE o.status = 'Completed' AND o.order_date > @end",
                    new SqliteParameter("@end", endStr));

                foreach (DataRow row in dtSalesAfter.Rows)
                {
                    string itemType = row["item_type"].ToString();
                    if (itemType == "Part")
                    {
                        int partId = Convert.ToInt32(row["part_id"]);
                        double qty = Convert.ToDouble(row["quantity"]);
                        string uom = row["unit_of_measure"]?.ToString();
                        if (partsDict.TryGetValue(partId, out var part))
                        {
                            double qtyPacks = Shaheen_InventoryManagement_Android.Data.RecipePartData.GetConvertedQuantityDynamic(
                                qty, uom, part.PartUom, part.StockType, part.PackItemsNumber, part.BigUnit, part.SmallUnit, part.ConversionValue, part.PackSize);
                            part.ChangesAfterPeriodPacks -= qtyPacks;
                        }
                    }
                    else if (itemType == "Recipe")
                    {
                        int recipeId = Convert.ToInt32(row["recipe_id"]);
                        double qty = Convert.ToDouble(row["quantity"]);
                        DataTable dtRP = DatabaseHelper.ExecuteDataTable("SELECT part_id, quantity, unit_of_measure FROM recipe_parts WHERE recipe_id = " + recipeId);
                        foreach (DataRow rp in dtRP.Rows)
                        {
                            int partId = Convert.ToInt32(rp["part_id"]);
                            double rpQty = Convert.ToDouble(rp["quantity"]);
                            string rpUom = rp["unit_of_measure"]?.ToString();
                            if (partsDict.TryGetValue(partId, out var part))
                            {
                                double rpQtyPacks = Shaheen_InventoryManagement_Android.Data.RecipePartData.GetConvertedQuantityDynamic(
                                    rpQty, rpUom, part.PartUom, part.StockType, part.PackItemsNumber, part.BigUnit, part.SmallUnit, part.ConversionValue, part.PackSize);
                                part.ChangesAfterPeriodPacks -= rpQtyPacks * qty;
                            }
                        }
                    }
                }

                // --- 4. Populate Ingredient Consumption list ---
                var ingredientConsumption = new List<object>();
                decimal totalInventoryValue = 0;
                double totalPurchasedPacks = 0;
                decimal totalPurchasedCost = 0;

                foreach (var pi in partsDict.Values)
                {
                    double closingStockPacks = pi.CurrentStock - pi.ChangesAfterPeriodPacks;
                    double usagePacks = pi.UsedInRecipesPacks + pi.SoldDirectlyPacks;
                    double openingStockPacks = closingStockPacks + usagePacks - pi.PurchasedPacks - pi.AdjustedPacks;

                    openingStockPacks = Math.Max(0, openingStockPacks);
                    closingStockPacks = Math.Max(0, closingStockPacks);

                    double openingStockBig = openingStockPacks * pi.PackSize;
                    double purchasedBig = pi.PurchasedPacks * pi.PackSize;
                    double usedInRecipesBig = pi.UsedInRecipesPacks * pi.PackSize;
                    double soldDirectlyBig = pi.SoldDirectlyPacks * pi.PackSize;
                    double closingStockBig = closingStockPacks * pi.PackSize;

                    totalInventoryValue += (decimal)pi.CurrentStock * pi.PackPrice;
                    totalPurchasedPacks += pi.PurchasedPacks;
                    totalPurchasedCost += (decimal)pi.PurchasedPacks * pi.PackPrice;

                    string unitToShow = string.IsNullOrEmpty(pi.BigUnit) ? (string.IsNullOrEmpty(pi.PartUom) ? "pcs" : pi.PartUom) : pi.BigUnit;

                    ingredientConsumption.Add(new
                    {
                        name = pi.Name,
                        openingStock = Math.Round(openingStockBig, 2),
                        purchased = Math.Round(purchasedBig, 2),
                        usedInRecipes = Math.Round(usedInRecipesBig, 2),
                        soldDirectly = Math.Round(soldDirectlyBig, 2),
                        closingStock = Math.Round(closingStockBig, 2),
                        unit = unitToShow
                    });
                }

                // --- 5. Generate Stock Movement History ---
                var movements = new List<StockMovementItem>();

                // POS Ingredient Sales
                DataTable dtMovSales = DatabaseHelper.ExecuteDataTable(
                    @"SELECT oi.part_id, oi.quantity, oi.price, oi.unit_of_measure, o.order_date
                      FROM order_items oi
                      JOIN orders o ON oi.order_id = o.order_id
                      WHERE o.status = 'Completed' AND oi.item_type = 'Part' AND o.order_date >= @start AND o.order_date <= @end",
                    new SqliteParameter("@start", startStr),
                    new SqliteParameter("@end", endStr));

                foreach (DataRow row in dtMovSales.Rows)
                {
                    int partId = Convert.ToInt32(row["part_id"]);
                    double qty = Convert.ToDouble(row["quantity"]);
                    string uom = row["unit_of_measure"]?.ToString();
                    DateTime date = DateTime.Parse(row["order_date"].ToString());

                    if (partsDict.TryGetValue(partId, out var part))
                    {
                        double qtyPacks = Shaheen_InventoryManagement_Android.Data.RecipePartData.GetConvertedQuantityDynamic(
                            qty, uom, part.PartUom, part.StockType, part.PackItemsNumber, part.BigUnit, part.SmallUnit, part.ConversionValue, part.PackSize);

                        movements.Add(new StockMovementItem
                        {
                            Date = date,
                            PartId = partId,
                            PartName = part.Name,
                            Type = "Ingredient Sale",
                            Quantity = qtyPacks * part.PackSize,
                            Unit = string.IsNullOrEmpty(part.BigUnit) ? "pcs" : part.BigUnit,
                            PacksChange = -qtyPacks
                        });
                    }
                }

                // POS Recipe Consumptions
                DataTable dtMovRecipes = DatabaseHelper.ExecuteDataTable(
                    @"SELECT oi.recipe_id, oi.quantity, o.order_date
                      FROM order_items oi
                      JOIN orders o ON oi.order_id = o.order_id
                      WHERE o.status = 'Completed' AND oi.item_type = 'Recipe' AND o.order_date >= @start AND o.order_date <= @end",
                    new SqliteParameter("@start", startStr),
                    new SqliteParameter("@end", endStr));

                foreach (DataRow row in dtMovRecipes.Rows)
                {
                    int recipeId = Convert.ToInt32(row["recipe_id"]);
                    double qty = Convert.ToDouble(row["quantity"]);
                    DateTime date = DateTime.Parse(row["order_date"].ToString());

                    DataTable dtRP = DatabaseHelper.ExecuteDataTable(
                        @"SELECT rp.part_id, rp.quantity, rp.unit_of_measure 
                          FROM recipe_parts rp 
                          WHERE rp.recipe_id = @rid",
                        new SqliteParameter("@rid", recipeId));

                    foreach (DataRow rp in dtRP.Rows)
                    {
                        int partId = Convert.ToInt32(rp["part_id"]);
                        double rpQty = Convert.ToDouble(rp["quantity"]);
                        string rpUom = rp["unit_of_measure"]?.ToString();

                        if (partsDict.TryGetValue(partId, out var part))
                        {
                            double rpQtyPacks = Shaheen_InventoryManagement_Android.Data.RecipePartData.GetConvertedQuantityDynamic(
                                rpQty, rpUom, part.PartUom, part.StockType, part.PackItemsNumber, part.BigUnit, part.SmallUnit, part.ConversionValue, part.PackSize);

                            movements.Add(new StockMovementItem
                            {
                                Date = date,
                                PartId = partId,
                                PartName = part.Name,
                                Type = "Recipe Consumption",
                                Quantity = rpQtyPacks * qty * part.PackSize,
                                Unit = string.IsNullOrEmpty(part.BigUnit) ? "pcs" : part.BigUnit,
                                PacksChange = -(rpQtyPacks * qty)
                            });
                        }
                    }
                }

                // Manual Adjustments from Transactions
                DataTable dtMovTx = DatabaseHelper.ExecuteDataTable(
                    @"SELECT action_type, part_name, description, timestamp
                      FROM transactions
                      WHERE timestamp >= @start AND timestamp <= @end",
                    new SqliteParameter("@start", startStr),
                    new SqliteParameter("@end", endStr));

                foreach (DataRow row in dtMovTx.Rows)
                {
                    string actionType = row["action_type"].ToString();
                    string partName = row["part_name"].ToString().Trim();
                    string desc = row["description"].ToString();
                    DateTime date = DateTime.Parse(row["timestamp"].ToString());

                    if (partsByName.TryGetValue(partName, out var part))
                    {
                        double changePacks = 0;
                        var matchAdjust = System.Text.RegularExpressions.Regex.Match(desc, @"by\s+(-?\d+\.?\d*)");
                        if (matchAdjust.Success)
                        {
                            double.TryParse(matchAdjust.Groups[1].Value, out changePacks);
                        }
                        else
                        {
                            var matchAdd = System.Text.RegularExpressions.Regex.Match(desc, @"Qty:\s*(\d+\.?\d*)");
                            if (matchAdd.Success)
                            {
                                double.TryParse(matchAdd.Groups[1].Value, out changePacks);
                            }
                        }

                        if (changePacks != 0)
                        {
                            movements.Add(new StockMovementItem
                            {
                                Date = date,
                                PartId = part.Id,
                                PartName = part.Name,
                                Type = changePacks > 0 ? "Purchase" : "Manual Stock Adjustment",
                                Quantity = Math.Abs(changePacks * part.PackSize),
                                Unit = string.IsNullOrEmpty(part.BigUnit) ? "pcs" : part.BigUnit,
                                PacksChange = changePacks
                            });
                        }
                    }
                }

                // Sort movements chronologically
                movements = movements.OrderBy(m => m.Date).ToList();

                // Compute running stock movement history
                var runningPartPacks = new Dictionary<int, double>();
                var stockMovementList = new List<object>();

                foreach (var pi in partsDict.Values)
                {
                    double closingStockPacks = pi.CurrentStock - pi.ChangesAfterPeriodPacks;
                    double usagePacks = pi.UsedInRecipesPacks + pi.SoldDirectlyPacks;
                    double openingStockPacks = closingStockPacks + usagePacks - pi.PurchasedPacks - pi.AdjustedPacks;
                    runningPartPacks[pi.Id] = Math.Max(0, openingStockPacks);
                }

                foreach (var mov in movements)
                {
                    double currentPacks = runningPartPacks.ContainsKey(mov.PartId) ? runningPartPacks[mov.PartId] : 0;
                    currentPacks += mov.PacksChange;
                    runningPartPacks[mov.PartId] = currentPacks;

                    var part = partsDict[mov.PartId];
                    stockMovementList.Add(new
                    {
                        date = mov.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                        ingredient = mov.PartName,
                        type = mov.Type,
                        quantity = Math.Round(mov.Quantity, 2),
                        unit = mov.Unit,
                        remainingStock = Math.Round(Math.Max(0, currentPacks * part.PackSize), 2)
                    });
                }

                // --- 6. Charts Data ---
                var salesByDay = new Dictionary<string, decimal>();
                DataTable dtDailySales = DatabaseHelper.ExecuteDataTable(
                    @"SELECT strftime('%Y-%m-%d', order_date) as day, SUM(total_amount) as daily_revenue
                      FROM orders
                      WHERE status = 'Completed' AND order_date >= @start AND order_date <= @end
                      GROUP BY day
                      ORDER BY day ASC",
                    new SqliteParameter("@start", startStr),
                    new SqliteParameter("@end", endStr));
                foreach (DataRow r in dtDailySales.Rows)
                {
                    salesByDay[r["day"].ToString()] = Convert.ToDecimal(r["daily_revenue"]);
                }

                var bestRecipes = recipeSummaries
                    .Select(x => (dynamic)x)
                    .OrderByDescending(x => (double)x.qtySold)
                    .Take(5)
                    .Select(x => new { name = x.name, qty = x.qtySold })
                    .ToList();

                var mostIngredients = ingredientConsumption
                    .Select(x => (dynamic)x)
                    .OrderByDescending(x => (double)x.usedInRecipes + (double)x.soldDirectly)
                    .Take(5)
                    .Select(x => new { name = x.name, qty = x.usedInRecipes + x.soldDirectly })
                    .ToList();

                var revenueTrend = new List<object>();
                for (int i = 5; i >= 0; i--)
                {
                    DateTime mStart = targetMonthStart.AddMonths(-i);
                    DateTime mEnd = new DateTime(mStart.Year, mStart.Month, DateTime.DaysInMonth(mStart.Year, mStart.Month), 23, 59, 59);
                    decimal mRev = DatabaseHelper.ExecuteScalar<decimal>(
                        "SELECT COALESCE(SUM(total_amount), 0) FROM orders WHERE status = 'Completed' AND order_date >= @s AND order_date <= @e",
                        new SqliteParameter("@s", mStart.ToString("yyyy-MM-dd HH:mm:ss")),
                        new SqliteParameter("@e", mEnd.ToString("yyyy-MM-dd HH:mm:ss")));
                    revenueTrend.Add(new
                    {
                        month = mStart.ToString("MMM yyyy"),
                        revenue = mRev
                    });
                }

                decimal grossProfit = (!Helpers.UserSession.IsAdmin) ? 0m : (totalRevenue - totalCogs);
                decimal profitMargin = (!Helpers.UserSession.IsAdmin) ? 0m : (totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0m);

                return JsonSerializer.Serialize(new
                {
                    revenue = totalRevenue,
                    orders = totalOrders,
                    outOfStock = outOfStock,
                    lowStock = lowStock,
                    categorySales = categorySales,
                    transactions = recentTransactions,
                    cogs = (!Helpers.UserSession.IsAdmin) ? 0m : totalCogs,
                    grossProfit = grossProfit,
                    profitMargin = Math.Round(profitMargin, 2),
                    recipeSummary = recipeSummaries,
                    ingredientConsumption = ingredientConsumption,
                    stockMovement = stockMovementList,
                    financialSummary = new
                    {
                        revenue = totalRevenue,
                        purchaseCost = (!Helpers.UserSession.IsAdmin) ? 0m : totalPurchasedCost,
                        ingredientCost = (!Helpers.UserSession.IsAdmin) ? 0m : totalCogs,
                        grossProfit = grossProfit,
                        profitMargin = Math.Round(profitMargin, 2),
                        inventoryValue = (!Helpers.UserSession.IsAdmin) ? 0m : totalInventoryValue
                    },
                    chartsData = new
                    {
                        salesByDay = salesByDay,
                        bestRecipes = bestRecipes,
                        mostIngredients = mostIngredients,
                        revenueTrend = revenueTrend
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.GetMonthlyReportData");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private class ReportPartItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double CurrentStock { get; set; }
            public double PackSize { get; set; }
            public decimal PackPrice { get; set; }
            public string BigUnit { get; set; }
            public string SmallUnit { get; set; }
            public double ConversionValue { get; set; }
            public string PartUom { get; set; }
            public string StockType { get; set; }
            public int PackItemsNumber { get; set; }
            public int MinStock { get; set; }
            public double UsedInRecipesPacks { get; set; }
            public double SoldDirectlyPacks { get; set; }
            public double PurchasedPacks { get; set; }
            public double AdjustedPacks { get; set; }
            public double ChangesAfterPeriodPacks { get; set; }
        }

        private class StockMovementItem
        {
            public DateTime Date { get; set; }
            public int PartId { get; set; }
            public string PartName { get; set; }
            public string Type { get; set; }
            public double Quantity { get; set; }
            public string Unit { get; set; }
            public double PacksChange { get; set; }
        }

        private string ClearReportsData()
        {
            try
            {
                using (var conn = new SqliteConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        // 1. Delete completed order items
                        string sqlDeleteItems = @"
                            DELETE FROM order_items 
                            WHERE order_id IN (SELECT order_id FROM orders WHERE status = 'Completed')";
                        using (var cmd = new SqliteCommand(sqlDeleteItems, conn, transaction))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Delete completed orders
                        string sqlDeleteOrders = "DELETE FROM orders WHERE status = 'Completed'";
                        using (var cmd = new SqliteCommand(sqlDeleteOrders, conn, transaction))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        // 3. Clear payments
                        string sqlDeletePayments = "DELETE FROM payments";
                        using (var cmd = new SqliteCommand(sqlDeletePayments, conn, transaction))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        // 4. Clear returns and return items
                        string sqlDeleteReturnItems = "DELETE FROM return_items";
                        using (var cmd = new SqliteCommand(sqlDeleteReturnItems, conn, transaction))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        string sqlDeleteReturns = "DELETE FROM returns";
                        using (var cmd = new SqliteCommand(sqlDeleteReturns, conn, transaction))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        // 5. Clear transactions
                        string sqlDeleteTransactions = "DELETE FROM transactions";
                        using (var cmd = new SqliteCommand(sqlDeleteTransactions, conn, transaction))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                return JsonSerializer.Serialize(new { success = true });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.ClearReportsData");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private static bool IsImagePath(string val)
        {
            if (string.IsNullOrEmpty(val)) return false;
            if (val.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                val.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                val.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
                val.Contains('/') || val.Contains('\\'))
            {
                return true;
            }
            string lower = val.ToLower();
            return lower.Contains(".png") || lower.Contains(".jpg") || lower.Contains(".jpeg") || 
                   lower.Contains(".gif") || lower.Contains(".svg") || lower.Contains(".webp") || lower.Contains(".ico");
        }

        private static string CleanImagePrefix(string partImage)
        {
            if (string.IsNullOrEmpty(partImage)) return "";
            if (!IsImagePath(partImage)) return partImage; // Return emoji or short code as-is

            if (partImage.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return partImage;
            if (partImage.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                return "/" + partImage;
            if (!partImage.StartsWith("/") && !partImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return "/Assets/" + partImage;
            return partImage;
        }

        private static string CleanCategoryIcon(string category, string catImage)
        {
            if (string.IsNullOrEmpty(catImage))
            {
                string cleanCat = category.ToLower().Trim();
                if (cleanCat.Contains("service")) return "/Assets/nuricon_pos.png";
                return "/Assets/nuricon_inventory.svg";
            }
            if (catImage.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                return "/" + catImage;
            if (!catImage.StartsWith("/") && !catImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return "/Assets/" + catImage;
            return catImage;
        }

        // DTO models
        private class LoginPayload
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private class AddItemPayload
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Barcode { get; set; }
            public string Sku { get; set; }
            public string ItemNo { get; set; }
            public string UnitOfMeasure { get; set; }
            public string StockType { get; set; }
            public int PackItemsNumber { get; set; }
            public decimal PackPrice { get; set; }
            public decimal ItemPrice { get; set; }
            public decimal PiecePrice { get; set; }
            public int MinStock { get; set; }
            public string BigUnit { get; set; }
            public string SmallUnit { get; set; }
            public double ConversionValue { get; set; }
            public double PackSize { get; set; }
            public string Image { get; set; }
        }

        private class SyncSessionPayload
        {
            public string Username { get; set; }
            public string Role { get; set; }
        }

        private class AdjustStockPayload
        {
            public int PartId { get; set; }
            public double Change { get; set; }
            public string Reason { get; set; }
        }

        private class CheckoutPayload
        {
            public List<CheckoutItem> Items { get; set; }
        }
        private class CheckoutItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Qty { get; set; }
            public string ItemType { get; set; }
            public int? RecipeId { get; set; }
            public string UnitOfMeasure { get; set; }
        }

        private class RecipePayload
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public string ItemNo { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public string CategoryName { get; set; }
            public string Image { get; set; }
            public decimal? YieldQuantity { get; set; }
            public string YieldUnit { get; set; }
            public List<RecipeIngredientPayload> Ingredients { get; set; }
        }

        private class RecipeIngredientPayload
        {
            public int PartId { get; set; }
            public double Qty { get; set; }
            public string UnitOfMeasure { get; set; }
        }

        private class BulkImportPayload
        {
            public List<ImportItemDetail> Items { get; set; }
        }

        private class BulkImportRecipesPayload
        {
            public List<ImportRecipeItem> Recipes { get; set; }
        }

        private class ImportRecipeItem
        {
            public string Name { get; set; }
            public string CategoryName { get; set; }
            public decimal Price { get; set; }
            public string Description { get; set; }
            public string ItemNo { get; set; }
            public List<ImportRecipeIngredient> Ingredients { get; set; }
        }

        private class ImportRecipeIngredient
        {
            public string Name { get; set; }
            public double Qty { get; set; }
            public string UnitOfMeasure { get; set; }
        }

        private class ImportItemDetail
        {
            public string Name { get; set; }
            public string Category { get; set; }
            public decimal Price { get; set; }
            public double Stock { get; set; }
            public string Barcode { get; set; }
            public string Sku { get; set; }
            public string Description { get; set; }
            public string ItemNo { get; set; }
            public string BigUnit { get; set; }
            public string SmallUnit { get; set; }
            public double ConversionValue { get; set; }
            public double PackSize { get; set; }
            public decimal PackPrice { get; set; }
            public int MinStock { get; set; }
        }

        private class ExportCsvPayload
        {
            public string Filename { get; set; }
            public string CsvContent { get; set; }
            public string Base64Content { get; set; }
        }

        private class ReturnPayload
        {
            public int OrderId { get; set; }
            public string Reason { get; set; }
            public List<ReturnItemDetail> Items { get; set; }
        }
        private class ReturnItemDetail
        {
            public int PartId { get; set; }
            public int Qty { get; set; }
            public decimal RefundAmount { get; set; }
        }

        private class DailySalesImportPayload
        {
            public List<DailySaleDetail> Sales { get; set; }
        }

        private class DailySaleDetail
        {
            public string ItemNo { get; set; }
            public string RecipeName { get; set; }
            public int QtySold { get; set; }
            public string UnitOfMeasure { get; set; }
        }

        private class RecipePartDeduction
        {
            public int RecipeId { get; set; }
            public int PartId { get; set; }
            public string PartName { get; set; }
            public double QtyPerRecipe { get; set; }
            public double CurrentStock { get; set; }
            public string RecipeUom { get; set; }
            public string PartUom { get; set; }
            public string StockType { get; set; }
            public int PackItems { get; set; }
            public string BigUnit { get; set; }
            public string SmallUnit { get; set; }
            public double ConversionValue { get; set; }
            public double PackSize { get; set; }
        }

        private class RecipeCacheItem
        {
            public int Id { get; set; }
            public string RecipeName { get; set; }
            public string ItemNo { get; set; }
            public decimal SellingPrice { get; set; }
        }

        private class PartCacheItem
        {
            public int Id { get; set; }
            public string PartName { get; set; }
            public string ItemNo { get; set; }
            public decimal SellingPrice { get; set; }
            public double QuantityInStock { get; set; }
            public string UnitOfMeasure { get; set; }
            public string StockType { get; set; }
            public int PackItemsNumber { get; set; }
            public string BigUnit { get; set; }
            public string SmallUnit { get; set; }
            public double ConversionValue { get; set; }
            public double PackSize { get; set; }
            public decimal PurchasePrice { get; set; }
        }

        private class IngredientDeductionResult
        {
            public int PartId { get; set; }
            public string PartName { get; set; }
            public double QtyDeducted { get; set; }
            public double PreviousStock { get; set; }
            public double NewStock { get; set; }
        }

        private class FactoryResetPayload
        {
            public string AdminUsername { get; set; }
            public string AdminPassword { get; set; }
        }

        private bool VerifyAdminCredentials(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            if (username == "Softio.Admin" && password == "Softio@2026!")
                return true;

            if (username.Equals("Admin", StringComparison.OrdinalIgnoreCase) && password == "Admin.Softio")
                return true;

            try
            {
                var dt = DatabaseHelper.ExecuteDataTable(
                    "SELECT role FROM users WHERE username = @u AND password = @p AND is_active = 1",
                    new SqliteParameter("@u", username),
                    new SqliteParameter("@p", password));

                if (dt.Rows.Count > 0)
                {
                    string role = dt.Rows[0]["role"].ToString();
                    return role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch { }

            return false;
        }

        private void PerformFactoryReset()
        {
            var tables = new[]
            {
                "parts", "recipes", "recipe_parts", "orders", "order_items",
                "transactions", "payments", "purchase_orders", "purchase_order_items",
                "returns", "return_items", "expenses", "expense_categories",
                "categories", "suppliers", "customers", "user_logs", "users"
            };

            foreach (var table in tables)
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery($"DELETE FROM {table};");
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, $"FactoryReset delete table {table} failed");
                }
            }

            try
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM sqlite_sequence;");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "FactoryReset sqlite_sequence clear failed");
            }

            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO users (username, password, full_name, role, is_active) VALUES ('Softio.Admin', 'Softio@2026!', 'Softio Super Admin', 'Admin', 1);"
            );
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO users (username, password, full_name, role, is_active) VALUES ('Admin', 'Admin.Softio', 'Test Admin', 'Admin', 1);"
            );
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO users (username, password, full_name, role, is_active) VALUES ('staff', 'Staff.Softio', 'Test Staff', 'Staff', 1);"
            );
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO users (username, password, full_name, role, is_active) VALUES ('production', 'Productio@2026!', 'Production User', 'Production', 1);"
            );

            DatabaseHelper.LogUserAction("System", "Factory Reset", "All application data reset to initial state");
        }
    }
}
