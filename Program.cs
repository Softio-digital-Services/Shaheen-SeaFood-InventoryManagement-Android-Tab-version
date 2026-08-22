using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shaheen_InventoryManagement_Android.Services;
using Shaheen_InventoryManagement_Android.Helpers;
using Microsoft.Data.Sqlite;

namespace Shaheen_InventoryManagement_Android
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "--test-license")
            {
                RunLicenseTests();
                return;
            }
            if (args != null && args.Length > 0 && args[0] == "--check-current-license")
            {
                CheckCurrentLicense();
                return;
            }
            if (args != null && args.Length > 0 && args[0] == "--activate-license")
            {
                var activated = Helpers.LicenseManager.ActivateLicense("CHAHN-YEAR1-00000-27365-01211", "Test User");
                if (activated != null) {
                    Console.WriteLine("License activated successfully!");
                } else {
                    Console.WriteLine("Failed to activate license.");
                }
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Set initial language to Arabic for testing
            Shaheen_InventoryManagement_Android.Helpers.LocalizationManager.SetLanguage("en-US");

            // Set initial language to English
            //Shaheen_InventoryManagement_Android.Helpers.LocalizationManager.SetLanguage("ar");

            Application.ThreadException += (s, e) =>
            {
                try { System.IO.File.AppendAllText("crash.txt", DateTime.Now.ToString() + ": " + e.Exception.ToString() + "\n\n"); } catch { }
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try { System.IO.File.AppendAllText("crash.txt", DateTime.Now.ToString() + ": " + e.ExceptionObject.ToString() + "\n\n"); } catch { }
            };

            try
            {
                // Check if a valid license exists
                if (!Shaheen_InventoryManagement_Android.Helpers.LicenseManager.HasValidLicense())
                {
                    using (var licenseForm = new Shaheen_InventoryManagement_Android.Forms.LicenseForm())
                    {
                        if (licenseForm.ShowDialog() != DialogResult.OK)
                        {
                            return; // Exit application if license is not activated/trial not started
                        }
                    }
                }

                // Expose background task for server hosting without blocking UI thread
                _ = Task.Run(() => StartApiServer());

                // Initialize Database (Create if missing)
                Shaheen_InventoryManagement_Android.Helpers.DatabaseInitializer.Initialize();

                // Ensure schema is up to date (add missing columns)
                DatabaseHelper.EnsureSchema();

                // Initialize currency tables and load cached rates
                Shaheen_InventoryManagement_Android.Services.CurrencyService.EnsureTable();

                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                Shaheen_InventoryManagement_Android.Forms.ModernMessageBox.Show(
                    string.Format(LocalizationManager.GetString("Msg_CriticalError"), ex.Message) + $"\n\n{LocalizationManager.GetString("Msg_StackTrace")}\n{ex.StackTrace}",
                    LocalizationManager.GetString("Error_AppCrash"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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

        private static void StartApiServer()
        {
            try
            {
                string certPath = System.IO.Path.Combine(Application.StartupPath, "Database", "pos_cert.pfx");
                const string certPassword = "SoftioPos2026!";

                var builder = WebApplication.CreateBuilder();

                // Configure Kestrel for both HTTP and HTTPS
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(5000); // HTTP
                    if (System.IO.File.Exists(certPath))
                    {
                        options.ListenAnyIP(5001, listenOptions =>
                        {
                            listenOptions.UseHttps(certPath, certPassword);
                        });
                    }
                });

                builder.Services.AddCors(c => c.AddDefaultPolicy(p =>
                    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

                // - SignalR for real-time sync -
                builder.Services.AddSignalR();

                var app = builder.Build();

                // --- Discovery Logic ---
                string localIp = "localhost";
                try
                {
                    var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                    localIp = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() ?? "localhost";
                }
                catch { }

                // Register HubContext so WinForms can broadcast events
                InventoryBroadcaster.HubContext = app.Services
                    .GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<InventoryHub>>();

                app.UseCors();

                // Camera requires HTTPS OR the Chrome Flag (chrome://flags/#unsafely-treat-insecure-origin-as-secure)
                // We'll allow both HTTP and HTTPS to co-exist for easier access
                // if (!builder.Environment.IsDevelopment()) { app.UseHsts(); }
                // app.UseHttpsRedirection();

                app.UseDefaultFiles();
                app.UseStaticFiles();

                // Serve desktop assets to the web portal
                string assetsPath = System.IO.Path.Combine(builder.Environment.ContentRootPath, "Assets");
                if (System.IO.Directory.Exists(assetsPath))
                {
                    app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
                    {
                        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(assetsPath),
                        RequestPath = "/Assets"
                    });
                }

                // - SignalR Hub endpoint -
                app.MapHub<InventoryHub>("/hubs/inventory");

                // - Status -
                app.MapGet("/api/status", () => Microsoft.AspNetCore.Http.Results.Ok(new { status = "API Running", version = "2.0", realtime = "SignalR Active" }));

                // - Config/Language -
                app.MapGet("/api/config", () => Microsoft.AspNetCore.Http.Results.Ok(new
                {
                    language = LocalizationManager.IsArabic ? "ar" : "en",
                    isArabic = LocalizationManager.IsArabic,
                    primaryColor = System.Drawing.ColorTranslator.ToHtml(ThemeConfig.PrimaryColor),
                    primaryRgb = $"{ThemeConfig.PrimaryColor.R}, {ThemeConfig.PrimaryColor.G}, {ThemeConfig.PrimaryColor.B}"
                }));

                // Wire up dynamic language broadcast to connected web portals
                LocalizationManager.LanguageChanged += (s, e) =>
                {
                    _ = InventoryBroadcaster.Broadcast("LanguageChanged", LocalizationManager.IsArabic ? "ar" : "en");
                };

                // - Products (live from DB) -
                app.MapGet("/api/products", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT p.id, p.part_name, p.selling_price, p.purchase_price, p.quantity_in_stock,
                                     p.minimum_stock_level, p.barcode, p.part_number, p.part_image,
                                     p.description, p.stock_type, p.pack_items_number, p.pack_price, p.item_price, p.piece_price,
                                     p.big_unit, p.small_unit, p.conversion_value, p.pack_size, p.item_no,
                                     COALESCE(c.category_name, 'General') AS category,
                                     c.category_image
                              FROM parts p
                              LEFT JOIN categories c ON p.category_id = c.id
                              WHERE p.date_deleted IS NULL AND p.status = 'Active'
                              ORDER BY c.category_name, p.part_name");

                        var products = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            string partImage = row["part_image"].ToString();
                            string catImage = row["category_image"].ToString();
                            string category = row["category"].ToString();

                            // Fully dynamic category icon resolution
                            if (string.IsNullOrEmpty(catImage))
                            {
                                string cleanCat = category.ToLower().Trim();

                                // Try finding a matching icon (SVG preferred, then PNG)
                                string[] extensions = { ".svg", ".png" };
                                bool found = false;

                                foreach (var ext in extensions)
                                {
                                    string iconFile = $"nuricon_{cleanCat}{ext}";
                                    if (System.IO.File.Exists(System.IO.Path.Combine(assetsPath, iconFile)))
                                    {
                                        catImage = "/Assets/" + iconFile;
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    // Fallback to specific keywords based on actual file existence
                                    if (cleanCat.Contains("service")) catImage = "/Assets/nuricon_pos.png";
                                    else if (cleanCat.Contains("accessory")) catImage = "/Assets/nuricon_inventory.svg";
                                    else if (cleanCat.Contains("engine")) catImage = "/Assets/nuricon_inventory.svg";
                                    else if (cleanCat.Contains("brake")) catImage = "/Assets/nuricon_inventory.svg";
                                    else catImage = "/Assets/nuricon_inventory.svg"; // Final default
                                }
                            }
                            else
                            {
                                // Clean up catImage: prevent /Assets/Assets/ double prefix
                                if (catImage.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                                    catImage = "/" + catImage;
                                else if (!catImage.StartsWith("/") && !catImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                    catImage = "/Assets/" + catImage;
                            }

                            // Clean up partImage: prevent /Assets/Assets/ double prefix
                            if (!string.IsNullOrEmpty(partImage) && IsImagePath(partImage))
                            {
                                if (partImage.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Keep base64 data url as-is
                                }
                                else if (partImage.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                                    partImage = "/" + partImage;
                                else if (!partImage.StartsWith("/") && !partImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                    partImage = "/Assets/" + partImage;
                            }

                            products.Add(new
                            {
                                id = Convert.ToInt32(row["id"]),
                                name = row["part_name"].ToString(),
                                price = Convert.ToDecimal(row["selling_price"]),
                                stock = Convert.ToDouble(row["quantity_in_stock"]),
                                minStock = Convert.ToDouble(row["minimum_stock_level"]),
                                barcode = row["barcode"].ToString(),
                                sku = row["part_number"].ToString(),
                                itemNo = row["item_no"].ToString(),
                                category = category,
                                image = partImage,
                                categoryImage = catImage,
                                isService = category.Equals("Services", StringComparison.OrdinalIgnoreCase),
                                description = row["description"].ToString(),
                                stockType = row["stock_type"] != DBNull.Value ? row["stock_type"].ToString() : "Piece",
                                packItemsNumber = row["pack_items_number"] != DBNull.Value ? Convert.ToInt32(row["pack_items_number"]) : 0,
                                purchasePrice = (!Helpers.UserSession.IsAdmin) ? 0m : (row["purchase_price"] != DBNull.Value ? Convert.ToDecimal(row["purchase_price"]) : 0m),
                                packPrice = (!Helpers.UserSession.IsAdmin) ? 0m : (row["pack_price"] != DBNull.Value ? Convert.ToDecimal(row["pack_price"]) : 0m),
                                itemPrice = (!Helpers.UserSession.IsAdmin) ? 0m : (row["item_price"] != DBNull.Value ? Convert.ToDecimal(row["item_price"]) : 0m),
                                piecePrice = (!Helpers.UserSession.IsAdmin) ? 0m : (row["piece_price"] != DBNull.Value ? Convert.ToDecimal(row["piece_price"]) : 0m),
                                bigUnit = row["big_unit"] != DBNull.Value ? row["big_unit"].ToString() : "",
                                smallUnit = row["small_unit"] != DBNull.Value ? row["small_unit"].ToString() : "",
                                conversionValue = row["conversion_value"] != DBNull.Value ? Convert.ToDouble(row["conversion_value"]) : 1.0,
                                packSize = row["pack_size"] != DBNull.Value ? Convert.ToDouble(row["pack_size"]) : 1.0
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(products);
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("DB error: " + ex.Message);
                    }
                });

                // - Reports & Analytics (GET) -
                app.MapGet("/api/reports", (Microsoft.AspNetCore.Http.HttpContext context) =>
                {
                    try
                    {
                        int month = DateTime.Now.Month;
                        int year = DateTime.Now.Year;
                        
                        var query = context.Request.Query;
                        if (query.ContainsKey("month")) int.TryParse(query["month"], out month);
                        if (query.ContainsKey("year")) int.TryParse(query["year"], out year);

                        int? day = null;
                        if (query.ContainsKey("day") && int.TryParse(query["day"], out int d)) day = d;

                        DateTime targetMonthStart = day.HasValue ? new DateTime(year, month, day.Value, 0, 0, 0) : new DateTime(year, month, 1, 0, 0, 0);
                        DateTime targetMonthEnd = day.HasValue ? new DateTime(year, month, day.Value, 23, 59, 59) : new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
                        string startStr = targetMonthStart.ToString("yyyy-MM-dd HH:mm:ss");
                        string endStr = targetMonthEnd.ToString("yyyy-MM-dd HH:mm:ss");

                        // 1. Basic legacy KPIs
                        decimal totalRevenue = DatabaseHelper.ExecuteScalar<decimal>(
                            "SELECT COALESCE(SUM(total_amount), 0) FROM orders WHERE status = 'Completed' AND order_date >= @start AND order_date <= @end",
                            new Microsoft.Data.Sqlite.SqliteParameter("@start", startStr),
                            new Microsoft.Data.Sqlite.SqliteParameter("@end", endStr));

                        int totalOrders = DatabaseHelper.ExecuteScalar<int>(
                            @"SELECT COALESCE(COUNT(DISTINCT o.order_id), 0) 
                              FROM orders o 
                              WHERE o.status = 'Completed' AND o.order_date >= @start AND o.order_date <= @end",
                            new Microsoft.Data.Sqlite.SqliteParameter("@start", startStr),
                            new Microsoft.Data.Sqlite.SqliteParameter("@end", endStr));

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

                        // Category Sales
                        var categorySales = new List<object>();
                        using (var dtCat = DatabaseHelper.ExecuteDataTable(
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
                            new Microsoft.Data.Sqlite.SqliteParameter("@start", startStr),
                            new Microsoft.Data.Sqlite.SqliteParameter("@end", endStr)))
                        {
                            foreach (System.Data.DataRow row in dtCat.Rows)
                            {
                                categorySales.Add(new
                                {
                                    category = row["category_name"].ToString(),
                                    sales = Convert.ToDecimal(row["total_sales"])
                                });
                            }
                        }

                        // Recent Transactions
                        var recentTransactions = new List<object>();
                        using (var dtTx = DatabaseHelper.ExecuteDataTable(
                            @"SELECT action_type, part_name, description, timestamp, username
                              FROM transactions
                              WHERE timestamp >= @start AND timestamp <= @end
                              ORDER BY id DESC LIMIT 50",
                            new Microsoft.Data.Sqlite.SqliteParameter("@start", startStr),
                            new Microsoft.Data.Sqlite.SqliteParameter("@end", endStr)))
                        {
                            foreach (System.Data.DataRow row in dtTx.Rows)
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
                        }

                        // Parts dictionary
                        var partsDict = new Dictionary<int, ReportPartItem>();
                        var partsByName = new Dictionary<string, ReportPartItem>(StringComparer.OrdinalIgnoreCase);
                        using (var dtParts = DatabaseHelper.ExecuteDataTable(
                            @"SELECT id, part_name, quantity_in_stock, pack_size, purchase_price, pack_price,
                                     big_unit, small_unit, conversion_value, unit_of_measure, stock_type,
                                     pack_items_number, minimum_stock_level
                              FROM parts WHERE date_deleted IS NULL"))
                        {
                            foreach (System.Data.DataRow r in dtParts.Rows)
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
                        }

                        // Recipe sales
                        double totalRecipesSold = 0;
                        double totalIngredientsSold = 0;
                        decimal totalCogs = 0;

                        var recipeSummaries = new List<object>();
                        using (var dtRecipeSales = DatabaseHelper.ExecuteDataTable(
                            @"SELECT oi.recipe_id, r.recipe_name, SUM(oi.quantity) as qty_sold, SUM(oi.quantity * oi.price) as revenue
                              FROM order_items oi
                              JOIN orders o ON oi.order_id = o.order_id
                              JOIN recipes r ON oi.recipe_id = r.id
                              WHERE o.status = 'Completed' AND o.order_date >= @start AND o.order_date <= @end
                              GROUP BY oi.recipe_id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@start", startStr),
                            new Microsoft.Data.Sqlite.SqliteParameter("@end", endStr)))
                        {
                            foreach (System.Data.DataRow rRow in dtRecipeSales.Rows)
                            {
                                int recipeId = Convert.ToInt32(rRow["recipe_id"]);
                                string name = rRow["recipe_name"].ToString();
                                double qtySold = Convert.ToDouble(rRow["qty_sold"]);
                                decimal revenue = Convert.ToDecimal(rRow["revenue"]);

                                decimal recipeUnitCost = 0;
                                using (var dtRecipeParts = DatabaseHelper.ExecuteDataTable(
                                    @"SELECT rp.part_id, rp.quantity, rp.unit_of_measure
                                      FROM recipe_parts rp
                                      WHERE rp.recipe_id = @rid",
                                    new Microsoft.Data.Sqlite.SqliteParameter("@rid", recipeId)))
                                {
                                    foreach (System.Data.DataRow rpRow in dtRecipeParts.Rows)
                                    {
                                        int partId = Convert.ToInt32(rpRow["part_id"]);
                                        double rpQty = Convert.ToDouble(rpRow["quantity"]);
                                        string rpUom = rpRow["unit_of_measure"]?.ToString();

                                        if (partsDict.TryGetValue(partId, out var part))
                                        {
                                            double rpQtyPacks = Shaheen_InventoryManagement_Android.Data.RecipePartData.GetConvertedQuantityDynamic(
                                                rpQty, rpUom, part.PartUom, part.StockType, part.PackItemsNumber, part.BigUnit, part.SmallUnit, part.ConversionValue, part.PackSize);
                                            recipeUnitCost += (decimal)rpQtyPacks * part.PackPrice;
                                            part.UsedInRecipesPacks += rpQtyPacks * qtySold;
                                        }
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
                        }

                        // Direct Sales
                        using (var dtDirectSales = DatabaseHelper.ExecuteDataTable(
                            @"SELECT oi.part_id, oi.quantity, oi.price, oi.unit_of_measure
                              FROM order_items oi
                              JOIN orders o ON oi.order_id = o.order_id
                              WHERE o.status = 'Completed' AND oi.item_type = 'Part' AND o.order_date >= @start AND o.order_date <= @end",
                            new Microsoft.Data.Sqlite.SqliteParameter("@start", startStr),
                            new Microsoft.Data.Sqlite.SqliteParameter("@end", endStr)))
                        {
                            foreach (System.Data.DataRow dsRow in dtDirectSales.Rows)
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
                        }

                        // Transactions
                        using (var dtAllTx = DatabaseHelper.ExecuteDataTable(
                            @"SELECT action_type, part_name, description, timestamp
                              FROM transactions
                              WHERE timestamp >= @start",
                            new Microsoft.Data.Sqlite.SqliteParameter("@start", startStr)))
                        {
                            foreach (System.Data.DataRow txRow in dtAllTx.Rows)
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
                                            if (changePacks > 0) part.PurchasedPacks += changePacks;
                                            else part.AdjustedPacks += changePacks;
                                        }
                                        else
                                        {
                                            part.ChangesAfterPeriodPacks += changePacks;
                                        }
                                    }
                                }
                            }
                        }

                        // Sales After
                        using (var dtSalesAfter = DatabaseHelper.ExecuteDataTable(
                            @"SELECT oi.part_id, oi.recipe_id, oi.quantity, oi.item_type, oi.unit_of_measure
                              FROM order_items oi
                              JOIN orders o ON oi.order_id = o.order_id
                              WHERE o.status = 'Completed' AND o.order_date > @end",
                            new Microsoft.Data.Sqlite.SqliteParameter("@end", endStr)))
                        {
                            foreach (System.Data.DataRow row in dtSalesAfter.Rows)
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
                                    using (var dtRP = DatabaseHelper.ExecuteDataTable("SELECT part_id, quantity, unit_of_measure FROM recipe_parts WHERE recipe_id = " + recipeId))
                                    {
                                        foreach (System.Data.DataRow rp in dtRP.Rows)
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
                            }
                        }

                        // Ingredient Consumption
                        var ingredientConsumption = new List<object>();
                        decimal totalInventoryValue = 0;
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

                        // Stock Movements
                        var movements = new List<StockMovementItem>();
                        using (var dtMovSales = DatabaseHelper.ExecuteDataTable(
                            @"SELECT oi.part_id, oi.quantity, oi.price, oi.unit_of_measure, o.order_date
                              FROM order_items oi
                              JOIN orders o ON oi.order_id = o.order_id
                              WHERE o.status = 'Completed' AND oi.item_type = 'Part' AND o.order_date >= @start AND o.order_date <= @end",
                            new Microsoft.Data.Sqlite.SqliteParameter("@start", startStr),
                            new Microsoft.Data.Sqlite.SqliteParameter("@end", endStr)))
                        {
                            foreach (System.Data.DataRow row in dtMovSales.Rows)
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
                        }

                        using (var dtMovRecipes = DatabaseHelper.ExecuteDataTable(
                            @"SELECT oi.recipe_id, oi.quantity, o.order_date
                              FROM order_items oi
                              JOIN orders o ON oi.order_id = o.order_id
                              WHERE o.status = 'Completed' AND oi.item_type = 'Recipe' AND o.order_date >= @start AND o.order_date <= @end",
                            new Microsoft.Data.Sqlite.SqliteParameter("@start", startStr),
                            new Microsoft.Data.Sqlite.SqliteParameter("@end", endStr)))
                        {
                            foreach (System.Data.DataRow row in dtMovRecipes.Rows)
                            {
                                int recipeId = Convert.ToInt32(row["recipe_id"]);
                                double qty = Convert.ToDouble(row["quantity"]);
                                DateTime date = DateTime.Parse(row["order_date"].ToString());

                                using (var dtRP = DatabaseHelper.ExecuteDataTable(
                                    @"SELECT rp.part_id, rp.quantity, rp.unit_of_measure 
                                      FROM recipe_parts rp 
                                      WHERE rp.recipe_id = @rid",
                                    new Microsoft.Data.Sqlite.SqliteParameter("@rid", recipeId)))
                                {
                                    foreach (System.Data.DataRow rp in dtRP.Rows)
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
                            }
                        }

                        using (var dtMovTx = DatabaseHelper.ExecuteDataTable(
                            @"SELECT action_type, part_name, description, timestamp
                              FROM transactions
                              WHERE timestamp >= @start AND timestamp <= @end",
                            new Microsoft.Data.Sqlite.SqliteParameter("@start", startStr),
                            new Microsoft.Data.Sqlite.SqliteParameter("@end", endStr)))
                        {
                            foreach (System.Data.DataRow row in dtMovTx.Rows)
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
                        }

                        movements = movements.OrderBy(m => m.Date).ToList();

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

                        // Charts
                        var salesByDay = new Dictionary<string, decimal>();
                        using (var dtDailySales = DatabaseHelper.ExecuteDataTable(
                            @"SELECT strftime('%Y-%m-%d', order_date) as day, SUM(total_amount) as daily_revenue
                              FROM orders
                              WHERE status = 'Completed' AND order_date >= @start AND order_date <= @end
                              GROUP BY day
                              ORDER BY day ASC",
                            new Microsoft.Data.Sqlite.SqliteParameter("@start", startStr),
                            new Microsoft.Data.Sqlite.SqliteParameter("@end", endStr)))
                        {
                            foreach (System.Data.DataRow r in dtDailySales.Rows)
                            {
                                salesByDay[r["day"].ToString()] = Convert.ToDecimal(r["daily_revenue"]);
                            }
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
                                new Microsoft.Data.Sqlite.SqliteParameter("@s", mStart.ToString("yyyy-MM-dd HH:mm:ss")),
                                new Microsoft.Data.Sqlite.SqliteParameter("@e", mEnd.ToString("yyyy-MM-dd HH:mm:ss")));
                            revenueTrend.Add(new
                            {
                                month = mStart.ToString("MMM yyyy"),
                                revenue = mRev
                            });
                        }

                        decimal grossProfit = (!Helpers.UserSession.IsAdmin) ? 0m : (totalRevenue - totalCogs);
                        decimal profitMargin = (!Helpers.UserSession.IsAdmin) ? 0m : (totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0m);

                        return Microsoft.AspNetCore.Http.Results.Ok(new
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
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to get reports data: " + ex.Message);
                    }
                });

                // - Delete Product (DELETE) -
                app.MapDelete("/api/products/{id}", (int id) =>
                {
                    try
                    {
                        string partName = DatabaseHelper.ExecuteScalar<string>("SELECT part_name FROM parts WHERE id = @id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id)) ?? "N/A";
                        
                        DatabaseHelper.ExecuteNonQuery("UPDATE parts SET date_deleted = datetime('now') WHERE id = @id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));
                        
                        DatabaseHelper.LogTransaction("DELETE", partName, $"Deleted Part: {partName} (ID: {id}) via WebPOS");
                        
                        // - Broadcast real-time update to all connected clients -
                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Item '{partName}' deleted via Web POS");
                        
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to delete item: " + ex.Message);
                    }
                });

                // - Categories -
                app.MapGet("/api/categories", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable("SELECT category_name FROM categories ORDER BY category_name");
                        var categories = new System.Collections.Generic.List<string>();
                        foreach (System.Data.DataRow row in dt.Rows)
                            categories.Add(row["category_name"].ToString());

                        return Microsoft.AspNetCore.Http.Results.Ok(categories);
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("DB error: " + ex.Message);
                    }
                });

                // - Sync Session (POST) -
                app.MapPost("/api/sync-session", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var payload = await System.Text.Json.JsonSerializer.DeserializeAsync<SyncSessionPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
                    return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                });

                // - Login (POST) -
                app.MapPost("/api/login", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<LoginPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password))
                             return Microsoft.AspNetCore.Http.Results.BadRequest("Missing credentials");

                        // Allow Softio super-admin through the web POS too
                        if (body.Username == "Softio.Admin" && body.Password == "Softio@2026!")
                        {
                            Helpers.UserSession.Username = "Softio.Admin";
                            Helpers.UserSession.Role = "Admin";
                            Helpers.UserSession.FullName = "Softio Super Admin";
                            DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Logged in");
                            return Microsoft.AspNetCore.Http.Results.Ok(new { username = "Softio.Admin", role = "Admin", fullName = "Softio Super Admin" });
                        }

                        if (body.Username?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true && body.Password == "Admin.Softio")
                        {
                            Helpers.UserSession.Username = "Admin";
                            Helpers.UserSession.Role = "Admin";
                            Helpers.UserSession.FullName = "Test Admin";
                            DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Logged in");
                            return Microsoft.AspNetCore.Http.Results.Ok(new { username = "Admin", role = "Admin", fullName = "Test Admin" });
                        }

                        var dt = DatabaseHelper.ExecuteDataTable(
                            "SELECT username, role, full_name FROM users WHERE username = @u AND password = @p",
                            new Microsoft.Data.Sqlite.SqliteParameter("@u", body.Username),
                            new Microsoft.Data.Sqlite.SqliteParameter("@p", body.Password));

                        if (dt.Rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.Unauthorized();

                        var row = dt.Rows[0];
                        Helpers.UserSession.Username = row["username"].ToString();
                        Helpers.UserSession.Role = row["role"].ToString();
                        Helpers.UserSession.FullName = row["full_name"].ToString();
                        DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Logged in");

                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            username = row["username"].ToString(),
                            role = row["role"].ToString(),
                            fullName = row["full_name"].ToString()
                        });
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, "API.Login");
                        return Microsoft.AspNetCore.Http.Results.StatusCode(500);
                    }
                });

                app.MapPost("/api/logout", () =>
                {
                    DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Logged out");
                    Helpers.UserSession.Clear();
                    return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                });

                app.MapGet("/api/logs", (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        string filterUser = request.Query["username"];

                        if (Helpers.UserSession.Role != "Admin")
                        {
                            filterUser = Helpers.UserSession.Username;
                        }

                        string sql = "";
                        Microsoft.Data.Sqlite.SqliteParameter[] parameters;

                        if (string.IsNullOrEmpty(filterUser) || filterUser.Equals("all", StringComparison.OrdinalIgnoreCase))
                        {
                            sql = "SELECT timestamp, username, action FROM user_logs ORDER BY id DESC LIMIT 200";
                            parameters = new Microsoft.Data.Sqlite.SqliteParameter[0];
                        }
                        else
                        {
                            sql = "SELECT timestamp, username, action FROM user_logs WHERE username = @u ORDER BY id DESC LIMIT 200";
                            parameters = new Microsoft.Data.Sqlite.SqliteParameter[] { new Microsoft.Data.Sqlite.SqliteParameter("@u", filterUser) };
                        }

                        using (var dt = DatabaseHelper.ExecuteDataTable(sql, parameters))
                        {
                            var list = dt.Rows.Cast<System.Data.DataRow>().Select(row => new
                            {
                                timestamp = row["timestamp"]?.ToString(),
                                username = row["username"]?.ToString(),
                                action = row["action"]?.ToString()
                            }).ToList();

                            return Microsoft.AspNetCore.Http.Results.Ok(list);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to fetch logs: " + ex.Message);
                    }
                });

                // - Get Users (GET) -
                app.MapGet("/api/users", () =>
                {
                    try
                    {
                        if (!Helpers.UserSession.IsAdmin)
                            return Microsoft.AspNetCore.Http.Results.Json(new { error = "Unauthorized" }, statusCode: 403);

                        var dt = DatabaseHelper.ExecuteDataTable("SELECT id, username, password, full_name, role, is_active, date_created FROM users ORDER BY username");
                        var users = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            users.Add(new
                            {
                                id = Convert.ToInt32(row["id"]),
                                username = row["username"].ToString(),
                                password = row["password"].ToString(),
                                fullName = row["full_name"] != DBNull.Value ? row["full_name"].ToString() : "",
                                role = row["role"] != DBNull.Value ? row["role"].ToString() : "User",
                                isActive = row["is_active"] != DBNull.Value ? Convert.ToInt32(row["is_active"]) : 1,
                                dateCreated = row["date_created"] != DBNull.Value ? row["date_created"].ToString() : ""
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(users);
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("DB error: " + ex.Message);
                    }
                });

                // - Save User (POST) -
                app.MapPost("/api/users", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        if (!Helpers.UserSession.IsAdmin)
                            return Microsoft.AspNetCore.Http.Results.Json(new { error = "Unauthorized" }, statusCode: 403);

                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<UserPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password))
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Missing required fields");

                        if (body.Id.HasValue && body.Id.Value > 0)
                        {
                            // Update
                            int existingCount = DatabaseHelper.ExecuteScalar<int>(
                                "SELECT COUNT(*) FROM users WHERE username = @u AND id != @id",
                                new Microsoft.Data.Sqlite.SqliteParameter("@u", body.Username),
                                new Microsoft.Data.Sqlite.SqliteParameter("@id", body.Id.Value));

                            if (existingCount > 0)
                                return Microsoft.AspNetCore.Http.Results.Conflict(new { error = "Username already exists." });

                            DatabaseHelper.ExecuteNonQuery(
                                "UPDATE users SET username = @u, password = @p, full_name = @fn, role = @role, is_active = @active WHERE id = @id",
                                new Microsoft.Data.Sqlite.SqliteParameter("@u", body.Username),
                                new Microsoft.Data.Sqlite.SqliteParameter("@p", body.Password),
                                new Microsoft.Data.Sqlite.SqliteParameter("@fn", body.FullName ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@role", body.Role ?? "User"),
                                new Microsoft.Data.Sqlite.SqliteParameter("@active", body.IsActive),
                                new Microsoft.Data.Sqlite.SqliteParameter("@id", body.Id.Value));

                            return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                        }
                        else
                        {
                            // Insert
                            int existingCount = DatabaseHelper.ExecuteScalar<int>(
                                "SELECT COUNT(*) FROM users WHERE username = @u",
                                new Microsoft.Data.Sqlite.SqliteParameter("@u", body.Username));

                            if (existingCount > 0)
                                return Microsoft.AspNetCore.Http.Results.Conflict(new { error = "Username already exists." });

                            DatabaseHelper.ExecuteNonQuery(
                                "INSERT INTO users (username, password, full_name, role, is_active) VALUES (@u, @p, @fn, @role, @active)",
                                new Microsoft.Data.Sqlite.SqliteParameter("@u", body.Username),
                                new Microsoft.Data.Sqlite.SqliteParameter("@p", body.Password),
                                new Microsoft.Data.Sqlite.SqliteParameter("@fn", body.FullName ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@role", body.Role ?? "User"),
                                new Microsoft.Data.Sqlite.SqliteParameter("@active", body.IsActive));

                            return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to save user: " + ex.Message);
                    }
                });

                // - Delete User (DELETE) -
                app.MapDelete("/api/users/{id}", (int id) =>
                {
                    try
                    {
                        if (!Helpers.UserSession.IsAdmin)
                            return Microsoft.AspNetCore.Http.Results.Json(new { error = "Unauthorized" }, statusCode: 403);

                        string targetUsername = DatabaseHelper.ExecuteScalar<string>("SELECT username FROM users WHERE id = @id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));

                        if (string.IsNullOrEmpty(targetUsername))
                            return Microsoft.AspNetCore.Http.Results.NotFound("User not found");

                        if (targetUsername.Equals(Helpers.UserSession.Username, StringComparison.OrdinalIgnoreCase))
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Cannot delete currently logged-in user");

                        if (targetUsername.Equals("Softio.Admin", StringComparison.OrdinalIgnoreCase))
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Cannot delete super admin user");

                        DatabaseHelper.ExecuteNonQuery("DELETE FROM users WHERE id = @id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to delete user: " + ex.Message);
                    }
                });

                app.MapPost("/api/add-item", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<AddItemPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || string.IsNullOrEmpty(body.Name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Missing name");

                        string categoryName = (body.Category ?? "General").Trim();
                        if (string.IsNullOrEmpty(categoryName)) categoryName = "General";

                        int catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                    new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName));
                        if (catId == 0)
                        {
                            DatabaseHelper.ExecuteNonQuery("INSERT INTO categories (category_name) VALUES (@c)",
                                        new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName));
                            catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                        new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName));
                        }
                        if (catId == 0) catId = 1;

                        if (body.Id.HasValue && body.Id.Value > 0)
                        {
                            // Update existing item
                            if (!string.IsNullOrEmpty(body.Barcode))
                            {
                                int existingCount = DatabaseHelper.ExecuteScalar<int>(
                                    "SELECT COUNT(*) FROM parts WHERE barcode = @b AND id != @id AND date_deleted IS NULL",
                                    new Microsoft.Data.Sqlite.SqliteParameter("@b", body.Barcode),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@id", body.Id.Value));

                                if (existingCount > 0)
                                    return Microsoft.AspNetCore.Http.Results.Conflict(new { error = "Barcode already exists for another item." });
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
                                new Microsoft.Data.Sqlite.SqliteParameter("@id", body.Id.Value));

                            decimal pPrice = body.Price;
                            decimal sPrice = body.Price;
                            decimal packPrice = body.PackPrice;
                            decimal itemPrice = body.ItemPrice;
                            decimal piecePrice = body.PiecePrice;

                            if (!Helpers.UserSession.IsAdmin)
                            {
                                using (var dtItem = DatabaseHelper.ExecuteDataTable(
                                    "SELECT purchase_price, selling_price, pack_price, item_price, piece_price FROM parts WHERE id = @id",
                                    new Microsoft.Data.Sqlite.SqliteParameter("@id", body.Id.Value)))
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
                                new Microsoft.Data.Sqlite.SqliteParameter("@name", body.Name),
                                new Microsoft.Data.Sqlite.SqliteParameter("@sku", body.Sku ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@cat", catId),
                                new Microsoft.Data.Sqlite.SqliteParameter("@p_price", pPrice),
                                new Microsoft.Data.Sqlite.SqliteParameter("@s_price", sPrice),
                                new Microsoft.Data.Sqlite.SqliteParameter("@stock", body.Stock),
                                new Microsoft.Data.Sqlite.SqliteParameter("@min_stock", body.MinStock),
                                new Microsoft.Data.Sqlite.SqliteParameter("@barcode", body.Barcode ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@itemNo", body.ItemNo ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@uom", body.UnitOfMeasure ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@stock_type", body.StockType ?? "Piece"),
                                new Microsoft.Data.Sqlite.SqliteParameter("@pack_items_number", body.PackItemsNumber),
                                new Microsoft.Data.Sqlite.SqliteParameter("@pack_price", packPrice),
                                new Microsoft.Data.Sqlite.SqliteParameter("@item_price", itemPrice),
                                new Microsoft.Data.Sqlite.SqliteParameter("@piece_price", piecePrice),
                                new Microsoft.Data.Sqlite.SqliteParameter("@big_unit", body.BigUnit ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@small_unit", body.SmallUnit ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@conversion_value", body.ConversionValue),
                                new Microsoft.Data.Sqlite.SqliteParameter("@pack_size", body.PackSize),
                                new Microsoft.Data.Sqlite.SqliteParameter("@part_image", body.Image ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@id", body.Id.Value));

                            double delta = body.Stock - oldStock;
                            if (delta != 0)
                            {
                                string action = delta > 0 ? "ADJUST_IN" : "ADJUST_OUT";
                                DatabaseHelper.LogTransaction(action, body.Name, $"Adjusted stock of {body.Name} by {delta:F4}. Reason: Product Edit (New Qty: {body.Stock})");
                            }
                            else
                            {
                                DatabaseHelper.LogTransaction("STOCK_EDIT", body.Name, $"Edited via WebPOS (New Qty: {body.Stock})");
                            }
                            _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Item '{body.Name}' updated via Web POS");
                            DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Edited ingredient");
                            return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                        }
                        else
                        {
                            // Insert new item
                            if (!string.IsNullOrEmpty(body.Barcode))
                            {
                                int existingCount = DatabaseHelper.ExecuteScalar<int>(
                                    "SELECT COUNT(*) FROM parts WHERE barcode = @b AND date_deleted IS NULL",
                                    new Microsoft.Data.Sqlite.SqliteParameter("@b", body.Barcode));

                                if (existingCount > 0)
                                    return Microsoft.AspNetCore.Http.Results.Conflict(new { error = "Barcode already exists for another item." });
                            }

                            string sql = @"
                                INSERT INTO parts (part_name, part_number, category_id, purchase_price, selling_price, quantity_in_stock, minimum_stock_level, barcode, status, item_no, unit_of_measure,
                                                   stock_type, pack_items_number, pack_price, item_price, piece_price,
                                                   big_unit, small_unit, conversion_value, pack_size, part_image)
                                VALUES (@name, @sku, @cat, @p_price, @s_price, @stock, @min_stock, @barcode, 'Active', @itemNo, @uom,
                                        @stock_type, @pack_items_number, @pack_price, @item_price, @piece_price,
                                        @big_unit, @small_unit, @conversion_value, @pack_size, @part_image)";

                            DatabaseHelper.ExecuteNonQuery(sql,
                                new Microsoft.Data.Sqlite.SqliteParameter("@name", body.Name),
                                new Microsoft.Data.Sqlite.SqliteParameter("@sku", body.Sku ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@cat", catId),
                                new Microsoft.Data.Sqlite.SqliteParameter("@p_price", body.Price),
                                new Microsoft.Data.Sqlite.SqliteParameter("@s_price", body.Price),
                                new Microsoft.Data.Sqlite.SqliteParameter("@stock", body.Stock),
                                new Microsoft.Data.Sqlite.SqliteParameter("@min_stock", body.MinStock),
                                new Microsoft.Data.Sqlite.SqliteParameter("@barcode", body.Barcode ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@itemNo", body.ItemNo ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@uom", body.UnitOfMeasure ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@stock_type", body.StockType ?? "Piece"),
                                new Microsoft.Data.Sqlite.SqliteParameter("@pack_items_number", body.PackItemsNumber),
                                new Microsoft.Data.Sqlite.SqliteParameter("@pack_price", body.PackPrice),
                                new Microsoft.Data.Sqlite.SqliteParameter("@item_price", body.ItemPrice),
                                new Microsoft.Data.Sqlite.SqliteParameter("@piece_price", body.PiecePrice),
                                new Microsoft.Data.Sqlite.SqliteParameter("@big_unit", body.BigUnit ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@small_unit", body.SmallUnit ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@conversion_value", body.ConversionValue),
                                new Microsoft.Data.Sqlite.SqliteParameter("@pack_size", body.PackSize),
                                new Microsoft.Data.Sqlite.SqliteParameter("@part_image", body.Image ?? ""));

                            DatabaseHelper.LogTransaction("STOCK_ADD", body.Name, $"Added via WebPOS (Qty: {body.Stock})");
                            _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Item '{body.Name}' added via Web POS");
                            DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Added ingredient");
                            return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to add/update item: " + ex.Message);
                    }
                });

                // - Bulk Import Items (POST) -
                app.MapPost("/api/import-items", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<BulkImportPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || body.Items == null || body.Items.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest("No items to import");

                        int imported = 0;
                        int skipped = 0;

                        foreach (var item in body.Items)
                        {
                            if (string.IsNullOrWhiteSpace(item.Name))
                            {
                                skipped++;
                                continue;
                            }

                            // Check if name or barcode already exists to support updates
                            int existingId = 0;
                            double currentPacks = 0.0;
                            if (!string.IsNullOrEmpty(item.Barcode))
                            {
                                using (var dt = DatabaseHelper.ExecuteDataTable("SELECT id, quantity_in_stock FROM parts WHERE barcode = @b AND date_deleted IS NULL",
                                    new Microsoft.Data.Sqlite.SqliteParameter("@b", item.Barcode.Trim())))
                                {
                                    if (dt.Rows.Count > 0)
                                    {
                                        existingId = Convert.ToInt32(dt.Rows[0]["id"]);
                                        currentPacks = Convert.ToDouble(dt.Rows[0]["quantity_in_stock"]);
                                    }
                                }
                            }
                            if (existingId == 0 && !string.IsNullOrEmpty(item.Sku))
                            {
                                using (var dt = DatabaseHelper.ExecuteDataTable("SELECT id, quantity_in_stock FROM parts WHERE part_number = @s AND date_deleted IS NULL",
                                    new Microsoft.Data.Sqlite.SqliteParameter("@s", item.Sku.Trim())))
                                {
                                    if (dt.Rows.Count > 0)
                                    {
                                        existingId = Convert.ToInt32(dt.Rows[0]["id"]);
                                        currentPacks = Convert.ToDouble(dt.Rows[0]["quantity_in_stock"]);
                                    }
                                }
                            }
                            if (existingId == 0 && !string.IsNullOrEmpty(item.Name))
                            {
                                using (var dt = DatabaseHelper.ExecuteDataTable("SELECT id, quantity_in_stock FROM parts WHERE LOWER(part_name) = LOWER(@n) AND date_deleted IS NULL",
                                    new Microsoft.Data.Sqlite.SqliteParameter("@n", item.Name.Trim())))
                                {
                                    if (dt.Rows.Count > 0)
                                    {
                                        existingId = Convert.ToInt32(dt.Rows[0]["id"]);
                                        currentPacks = Convert.ToDouble(dt.Rows[0]["quantity_in_stock"]);
                                    }
                                }
                            }

                            string categoryName = (item.Category ?? "General").Trim();
                            if (string.IsNullOrEmpty(categoryName)) categoryName = "General";

                            int catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                        new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName));
                            if (catId == 0)
                            {
                                try
                                {
                                    DatabaseHelper.ExecuteNonQuery("INSERT INTO categories (category_name) VALUES (@c)",
                                                new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName));
                                    catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                                new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName));
                                }
                                catch
                                {
                                    catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                                new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName));
                                }
                            }
                            if (catId == 0) catId = 1;

                            // Calculate costs using our dynamic engine
                            double pSize = item.PackSize > 0 ? item.PackSize : 1.0;
                            double conv = item.ConversionValue > 0 ? item.ConversionValue : 1.0;
                            decimal packPrice = item.PackPrice;
                            if (packPrice == 0 && item.Price > 0) packPrice = item.Price; // fallback

                            decimal itemCost = IngredientCalculationEngine.CalculateCostPerBigUnit(packPrice, pSize);
                            decimal pieceCost = IngredientCalculationEngine.CalculateCostPerSmallUnit(packPrice, pSize, conv);

                            // Convert current stock to packs
                            double stockPacks = IngredientCalculationEngine.ConvertStockToPacks(item.Stock, pSize);

                            if (existingId > 0)
                            {
                                // Update existing item
                                string sqlUpdate = @"
                                    UPDATE parts SET part_name = @name, part_number = @sku, category_id = @cat, purchase_price = @p_price, selling_price = @s_price,
                                                     quantity_in_stock = @stock, barcode = @barcode, description = @desc, item_no = @itemNo,
                                                     big_unit = @big, small_unit = @small, conversion_value = @conv, pack_size = @pack_size,
                                                     pack_price = @pack_price, piece_price = @piece_price, item_price = @item_price, minimum_stock_level = @min
                                    WHERE id = @id";

                                DatabaseHelper.ExecuteNonQuery(sqlUpdate,
                                    new Microsoft.Data.Sqlite.SqliteParameter("@name", item.Name),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@sku", item.Sku ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@cat", catId),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@p_price", packPrice),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@s_price", item.Price),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@stock", stockPacks),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@barcode", item.Barcode ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@desc", item.Description ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@itemNo", item.ItemNo ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@big", item.BigUnit ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@small", item.SmallUnit ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@conv", conv),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@pack_size", pSize),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@pack_price", packPrice),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@piece_price", pieceCost),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@item_price", itemCost),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@min", item.MinStock > 0 ? item.MinStock : 5),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@id", existingId));
                            }
                            else
                            {
                                // Insert new item
                                string sql = @"
                                    INSERT INTO parts (part_name, part_number, category_id, purchase_price, selling_price, quantity_in_stock, barcode, status, description, item_no,
                                                       big_unit, small_unit, conversion_value, pack_size, pack_price, piece_price, item_price, minimum_stock_level)
                                    VALUES (@name, @sku, @cat, @p_price, @s_price, @stock, @barcode, 'Active', @desc, @itemNo,
                                            @big, @small, @conv, @pack_size, @pack_price, @piece_price, @item_price, @min)";

                                DatabaseHelper.ExecuteNonQuery(sql,
                                    new Microsoft.Data.Sqlite.SqliteParameter("@name", item.Name),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@sku", item.Sku ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@cat", catId),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@p_price", packPrice),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@s_price", item.Price),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@stock", stockPacks),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@barcode", item.Barcode ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@desc", item.Description ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@itemNo", item.ItemNo ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@big", item.BigUnit ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@small", item.SmallUnit ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@conv", conv),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@pack_size", pSize),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@pack_price", packPrice),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@piece_price", pieceCost),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@item_price", itemCost),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@min", item.MinStock > 0 ? item.MinStock : 5));
                            }

                            double delta = stockPacks - currentPacks;
                            if (delta != 0)
                            {
                                string action = delta > 0 ? "ADJUST_IN" : "ADJUST_OUT";
                                DatabaseHelper.LogTransaction(action, item.Name, $"Adjusted stock of {item.Name} by {delta:F4}. Reason: Bulk Import (New Qty: {stockPacks})");
                            }

                            imported++;
                        }

                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Imported {imported} items via Web POS");
                        if (imported > 0)
                        {
                            DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Imported inventory");
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, imported, skipped });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Bulk import failed: " + ex.Message);
                    }
                });

                // - Bulk Import Recipes (POST) -
                app.MapPost("/api/import-recipes", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<BulkImportRecipesPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || body.Recipes == null || body.Recipes.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest("No recipes to import");

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
                                        new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName.Trim()));
                            if (catId == 0)
                            {
                                try
                                {
                                    DatabaseHelper.ExecuteNonQuery("INSERT INTO categories (category_name) VALUES (@c)", new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName.Trim()));
                                    catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                                new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName.Trim()));
                                }
                                catch
                                {
                                    catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c)",
                                                new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName.Trim()));
                                }
                            }
                            if (catId == 0) catId = 1;

                            // Check if recipe already exists (active or soft-deleted)
                            int existingId = DatabaseHelper.ExecuteScalar<int>(
                                "SELECT id FROM recipes WHERE LOWER(recipe_name) = LOWER(@n)",
                                new Microsoft.Data.Sqlite.SqliteParameter("@n", recipeItem.Name.Trim()));

                            int recipeId = 0;
                            if (existingId > 0)
                            {
                                // Update recipe and restore it if soft-deleted
                                string sqlUpdate = @"UPDATE recipes SET description = @desc, selling_price = @price, category_id = @catId, item_no = @itemNo, date_deleted = NULL, status = 'Active' WHERE id = @id";
                                DatabaseHelper.ExecuteNonQuery(sqlUpdate,
                                    new Microsoft.Data.Sqlite.SqliteParameter("@desc", recipeItem.Description ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@price", recipeItem.Price),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@catId", catId),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@itemNo", recipeItem.ItemNo ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@id", existingId));
                                recipeId = existingId;

                                // Delete existing ingredients mapping
                                DatabaseHelper.ExecuteNonQuery("DELETE FROM recipe_parts WHERE recipe_id = @r_id", new Microsoft.Data.Sqlite.SqliteParameter("@r_id", recipeId));
                            }
                            else
                            {
                                // Insert recipe
                                string sqlInsert = @"INSERT INTO recipes (recipe_name, description, selling_price, status, date_added, category_id, item_no)
                                                     VALUES (@name, @desc, @price, 'Active', datetime('now'), @catId, @itemNo);
                                                     SELECT last_insert_rowid();";
                                recipeId = (int)DatabaseHelper.ExecuteScalar<long>(sqlInsert,
                                    new Microsoft.Data.Sqlite.SqliteParameter("@name", recipeItem.Name.Trim()),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@desc", recipeItem.Description ?? ""),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@price", recipeItem.Price),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@catId", catId),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@itemNo", recipeItem.ItemNo ?? ""));
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
                                        new Microsoft.Data.Sqlite.SqliteParameter("@pname", ing.Name.Trim()));

                                    if (partId > 0)
                                    {
                                        DatabaseHelper.ExecuteNonQuery(
                                            "INSERT INTO recipe_parts (recipe_id, part_id, quantity, unit_of_measure) VALUES (@r_id, @p_id, @qty, @uom)",
                                            new Microsoft.Data.Sqlite.SqliteParameter("@r_id", recipeId),
                                            new Microsoft.Data.Sqlite.SqliteParameter("@p_id", partId),
                                            new Microsoft.Data.Sqlite.SqliteParameter("@qty", ing.Qty),
                                            new Microsoft.Data.Sqlite.SqliteParameter("@uom", ing.UnitOfMeasure ?? ""));
                                    }
                                }
                            }

                            imported++;
                        }

                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Imported {imported} recipes via Web POS");

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, imported, skipped });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Bulk recipe import failed: " + ex.Message);
                    }
                });

                // - Bulk Import Sales (POST) -
                app.MapPost("/api/import-sales", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<DailySalesImportPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || body.Sales == null || body.Sales.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest("No sales data to process");

                        int recipesProcessed = 0;
                        int recipesSkipped = 0;
                        var skippedRecipes = new List<string>();
                        var ingredientDeductions = new List<IngredientDeductionResult>();

                        using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseConfig.ConnectionString))
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

                                     // 1. Find recipe by Item No
                                     if (!string.IsNullOrWhiteSpace(cleanItemNo))
                                     {
                                         string sqlRecipeByNo = @"SELECT id, recipe_name, selling_price FROM recipes 
                                                                  WHERE TRIM(LOWER(item_no)) = TRIM(LOWER(@itemNo)) AND date_deleted IS NULL";
                                         using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlRecipeByNo, conn, transaction))
                                         {
                                             cmd.Parameters.AddWithValue("@itemNo", cleanItemNo);
                                             using (var reader = cmd.ExecuteReader())
                                             {
                                                 if (reader.Read())
                                                 {
                                                     recipeId = Convert.ToInt32(reader["id"]);
                                                     exactRecipeName = reader["recipe_name"].ToString();
                                                     sellingPrice = Convert.ToDecimal(reader["selling_price"]);
                                                 }
                                             }
                                         }
                                     }

                                     // 2. Find recipe by name
                                     if (recipeId == 0 && !string.IsNullOrWhiteSpace(cleanName))
                                     {
                                         string sqlRecipe = @"SELECT id, recipe_name, selling_price FROM recipes 
                                                              WHERE REPLACE(REPLACE(REPLACE(REPLACE(LOWER(recipe_name), ' ', ''), '\t', ''), '\r', ''), '\n', '') = 
                                                                    REPLACE(REPLACE(REPLACE(REPLACE(LOWER(@name), ' ', ''), '\t', ''), '\r', ''), '\n', '') 
                                                                AND date_deleted IS NULL";
                                         using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlRecipe, conn, transaction))
                                         {
                                             cmd.Parameters.AddWithValue("@name", cleanName);
                                             using (var reader = cmd.ExecuteReader())
                                             {
                                                 if (reader.Read())
                                                 {
                                                     recipeId = Convert.ToInt32(reader["id"]);
                                                     exactRecipeName = reader["recipe_name"].ToString();
                                                     sellingPrice = Convert.ToDecimal(reader["selling_price"]);
                                                 }
                                             }
                                         }
                                     }

                                     if (recipeId == 0)
                                     {
                                         int partId = 0;
                                         string exactPartName = "";
                                         decimal partSellingPrice = 0;
                                         double currentPartStock = 0;
                                         
                                         // Unit parameters
                                         string partUom = "";
                                         string stockType = "";
                                         int packItems = 1;
                                         string bigUnit = "";
                                         string smallUnit = "";
                                         double convVal = 1.0;
                                         double packSize = 1.0;
                                         decimal packPrice = 0;

                                         // Fallback A: Search parts directly by Item No
                                         if (!string.IsNullOrWhiteSpace(cleanItemNo))
                                         {
                                             string sqlPartByNo = @"SELECT id, part_name, selling_price, quantity_in_stock, unit_of_measure, stock_type, pack_items_number,
                                                                           big_unit, small_unit, conversion_value, pack_size, purchase_price FROM parts 
                                                                    WHERE TRIM(LOWER(item_no)) = TRIM(LOWER(@itemNo)) AND date_deleted IS NULL";
                                             using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlPartByNo, conn, transaction))
                                             {
                                                 cmd.Parameters.AddWithValue("@itemNo", cleanItemNo);
                                                 using (var reader = cmd.ExecuteReader())
                                                 {
                                                     if (reader.Read())
                                                     {
                                                         partId = Convert.ToInt32(reader["id"]);
                                                         exactPartName = reader["part_name"].ToString();
                                                         partSellingPrice = Convert.ToDecimal(reader["selling_price"]);
                                                         currentPartStock = Convert.ToDouble(reader["quantity_in_stock"]);
                                                         
                                                         partUom = reader["unit_of_measure"]?.ToString() ?? "";
                                                         stockType = reader["stock_type"]?.ToString() ?? "Piece";
                                                         packItems = reader["pack_items_number"] != DBNull.Value ? Convert.ToInt32(reader["pack_items_number"]) : 1;
                                                         bigUnit = reader["big_unit"]?.ToString() ?? "";
                                                         smallUnit = reader["small_unit"]?.ToString() ?? "";
                                                         convVal = reader["conversion_value"] != DBNull.Value ? Convert.ToDouble(reader["conversion_value"]) : 1.0;
                                                         packSize = reader["pack_size"] != DBNull.Value ? Convert.ToDouble(reader["pack_size"]) : 1.0;
                                                         packPrice = reader["purchase_price"] != DBNull.Value ? Convert.ToDecimal(reader["purchase_price"]) : 0m;
                                                     }
                                                 }
                                             }
                                         }

                                         // Fallback B: Search parts directly by name
                                         if (partId == 0 && !string.IsNullOrWhiteSpace(cleanName))
                                         {
                                             string sqlPartDirect = @"SELECT id, part_name, selling_price, quantity_in_stock, unit_of_measure, stock_type, pack_items_number,
                                                                             big_unit, small_unit, conversion_value, pack_size, purchase_price FROM parts 
                                                                      WHERE REPLACE(REPLACE(REPLACE(REPLACE(LOWER(part_name), ' ', ''), '\t', ''), '\r', ''), '\n', '') = 
                                                                            REPLACE(REPLACE(REPLACE(REPLACE(LOWER(@name), ' ', ''), '\t', ''), '\r', ''), '\n', '') 
                                                                        AND date_deleted IS NULL";
                                             using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlPartDirect, conn, transaction))
                                             {
                                                 cmd.Parameters.AddWithValue("@name", cleanName);
                                                 using (var reader = cmd.ExecuteReader())
                                                 {
                                                     if (reader.Read())
                                                     {
                                                         partId = Convert.ToInt32(reader["id"]);
                                                         exactPartName = reader["part_name"].ToString();
                                                         partSellingPrice = Convert.ToDecimal(reader["selling_price"]);
                                                         currentPartStock = Convert.ToDouble(reader["quantity_in_stock"]);
                                                         
                                                         partUom = reader["unit_of_measure"]?.ToString() ?? "";
                                                         stockType = reader["stock_type"]?.ToString() ?? "Piece";
                                                         packItems = reader["pack_items_number"] != DBNull.Value ? Convert.ToInt32(reader["pack_items_number"]) : 1;
                                                         bigUnit = reader["big_unit"]?.ToString() ?? "";
                                                         smallUnit = reader["small_unit"]?.ToString() ?? "";
                                                         convVal = reader["conversion_value"] != DBNull.Value ? Convert.ToDouble(reader["conversion_value"]) : 1.0;
                                                         packSize = reader["pack_size"] != DBNull.Value ? Convert.ToDouble(reader["pack_size"]) : 1.0;
                                                         packPrice = reader["purchase_price"] != DBNull.Value ? Convert.ToDecimal(reader["purchase_price"]) : 0m;
                                                     }
                                                 }
                                             }
                                         }

                                         if (partId == 0)
                                         {
                                             recipesSkipped++;
                                             string missingName = !string.IsNullOrWhiteSpace(cleanName) ? cleanName : (!string.IsNullOrWhiteSpace(cleanItemNo) ? "Item No " + cleanItemNo : "Unknown");
                                             if (!skippedRecipes.Contains(missingName))
                                                 skippedRecipes.Add(missingName);
                                             continue;
                                         }

                                         // Convert the quantity using the new unit conversion logic
                                         double totalDeduct = Data.RecipePartData.GetConvertedQuantityDynamic(
                                             sale.QtySold, sale.UnitOfMeasure, partUom, stockType, packItems, bigUnit, smallUnit, convVal, packSize);
                                         
                                         // Calculate the cost dynamically based on the unit of sale
                                         decimal unitCost = IngredientCalculationEngine.CalculatedUnitCost(
                                             packPrice, sale.UnitOfMeasure, bigUnit, smallUnit, convVal, packSize, stockType, packItems, partUom);

                                         if (totalDeduct > 0)
                                         {
                                             var existingDeduction = ingredientDeductions.FirstOrDefault(d => d.PartId == partId);
                                             double currentStockInDb = currentPartStock;
                                             if (existingDeduction != null)
                                             {
                                                 currentStockInDb = existingDeduction.NewStock;
                                             }

                                             double newStock = Math.Max(0, currentStockInDb - totalDeduct);

                                             // Update parts table
                                             string sqlUpdatePart = "UPDATE parts SET quantity_in_stock = @newStock WHERE id = @partId";
                                             using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlUpdatePart, conn, transaction))
                                             {
                                                 cmd.Parameters.AddWithValue("@newStock", newStock);
                                                 cmd.Parameters.AddWithValue("@partId", partId);
                                                 cmd.ExecuteNonQuery();
                                             }

                                             // Record transaction
                                             string sqlInsertTx = @"INSERT INTO transactions (action_type, part_name, description, username) 
                                                                   VALUES ('STOCK_DEDUCT', @partName, @desc, 'Admin')";
                                             string txDesc = $"Deducted {totalDeduct:0.##} via sales import ({exactPartName} x{sale.QtySold})";
                                             using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlInsertTx, conn, transaction))
                                             {
                                                 cmd.Parameters.AddWithValue("@partName", exactPartName);
                                                 cmd.Parameters.AddWithValue("@desc", txDesc);
                                                 cmd.ExecuteNonQuery();
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
                                                     PartId = partId,
                                                     PartName = exactPartName,
                                                     QtyDeducted = totalDeduct,
                                                     PreviousStock = currentPartStock,
                                                     NewStock = newStock
                                                 });
                                             }
                                         }

                                         recipesProcessed++;
                                         itemsForOrder.Add(Tuple.Create(false, partId, (double)sale.QtySold, unitCost, sale.UnitOfMeasure));
                                         continue;
                                     }

                                     // 2. Recipe case: find its ingredients
                                     string sqlParts = @"SELECT rp.part_id, rp.quantity, rp.unit_of_measure as recipe_uom, p.part_name, p.quantity_in_stock, p.unit_of_measure as part_uom, p.stock_type, p.pack_items_number,
                                                                p.big_unit, p.small_unit, p.conversion_value, p.pack_size
                                                         FROM recipe_parts rp
                                                         JOIN parts p ON rp.part_id = p.id
                                                         WHERE rp.recipe_id = @recipeId AND p.date_deleted IS NULL";
                                     var partsToDeduct = new List<RecipePartDeduction>();
                                     using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlParts, conn, transaction))
                                     {
                                         cmd.Parameters.AddWithValue("@recipeId", recipeId);
                                         using (var reader = cmd.ExecuteReader())
                                         {
                                             while (reader.Read())
                                             {
                                                 partsToDeduct.Add(new RecipePartDeduction
                                                 {
                                                     PartId = Convert.ToInt32(reader["part_id"]),
                                                     PartName = reader["part_name"].ToString(),
                                                     QtyPerRecipe = Convert.ToDouble(reader["quantity"]),
                                                     CurrentStock = Convert.ToDouble(reader["quantity_in_stock"]),
                                                     RecipeUom = reader["recipe_uom"]?.ToString(),
                                                     PartUom = reader["part_uom"]?.ToString(),
                                                     StockType = reader["stock_type"]?.ToString(),
                                                     PackItems = reader["pack_items_number"] != DBNull.Value ? Convert.ToInt32(reader["pack_items_number"]) : 1,
                                                     BigUnit = reader["big_unit"]?.ToString() ?? "",
                                                     SmallUnit = reader["small_unit"]?.ToString() ?? "",
                                                     ConversionValue = reader["conversion_value"] != DBNull.Value ? Convert.ToDouble(reader["conversion_value"]) : 1.0,
                                                     PackSize = reader["pack_size"] != DBNull.Value ? Convert.ToDouble(reader["pack_size"]) : 1.0
                                                 });
                                             }
                                         }
                                     }

                                     if (partsToDeduct.Count == 0)
                                     {
                                         // Fallback: Check if this recipe name/item_no exists directly as a part (ingredient/product) in parts table
                                         // and deduct it directly!
                                         int fallbackPartId = 0;
                                         string fallbackPartName = "";
                                         double fallbackPartStock = 0;
                                         string fallbackPartUom = "";
                                         string fallbackStockType = "";
                                         int fallbackPackItems = 1;
                                         string fallbackBigUnit = "";
                                         string fallbackSmallUnit = "";
                                         double fallbackConvVal = 1.0;
                                         double fallbackPackSize = 1.0;
                                         decimal fallbackPackPrice = 0;

                                         // Search parts by Item No first
                                         if (!string.IsNullOrWhiteSpace(cleanItemNo))
                                         {
                                             string sqlPartByNo = @"SELECT id, part_name, quantity_in_stock, unit_of_measure, stock_type, pack_items_number,
                                                                           big_unit, small_unit, conversion_value, pack_size, purchase_price FROM parts 
                                                                    WHERE TRIM(LOWER(item_no)) = TRIM(LOWER(@itemNo)) AND date_deleted IS NULL";
                                             using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlPartByNo, conn, transaction))
                                             {
                                                 cmd.Parameters.AddWithValue("@itemNo", cleanItemNo);
                                                 using (var reader = cmd.ExecuteReader())
                                                 {
                                                     if (reader.Read())
                                                     {
                                                         fallbackPartId = Convert.ToInt32(reader["id"]);
                                                         fallbackPartName = reader["part_name"].ToString();
                                                         fallbackPartStock = Convert.ToDouble(reader["quantity_in_stock"]);
                                                         fallbackPartUom = reader["unit_of_measure"]?.ToString() ?? "";
                                                         fallbackStockType = reader["stock_type"]?.ToString() ?? "Piece";
                                                         fallbackPackItems = reader["pack_items_number"] != DBNull.Value ? Convert.ToInt32(reader["pack_items_number"]) : 1;
                                                         fallbackBigUnit = reader["big_unit"]?.ToString() ?? "";
                                                         fallbackSmallUnit = reader["small_unit"]?.ToString() ?? "";
                                                         fallbackConvVal = reader["conversion_value"] != DBNull.Value ? Convert.ToDouble(reader["conversion_value"]) : 1.0;
                                                         fallbackPackSize = reader["pack_size"] != DBNull.Value ? Convert.ToDouble(reader["pack_size"]) : 1.0;
                                                         fallbackPackPrice = reader["purchase_price"] != DBNull.Value ? Convert.ToDecimal(reader["purchase_price"]) : 0m;
                                                     }
                                                 }
                                             }
                                         }

                                         // Search parts by Name next
                                         if (fallbackPartId == 0 && !string.IsNullOrWhiteSpace(cleanName))
                                         {
                                             string sqlPartDirect = @"SELECT id, part_name, quantity_in_stock, unit_of_measure, stock_type, pack_items_number,
                                                                             big_unit, small_unit, conversion_value, pack_size, purchase_price FROM parts 
                                                                      WHERE REPLACE(REPLACE(REPLACE(REPLACE(LOWER(part_name), ' ', ''), '\t', ''), '\r', ''), '\n', '') = 
                                                                            REPLACE(REPLACE(REPLACE(REPLACE(LOWER(@name), ' ', ''), '\t', ''), '\r', ''), '\n', '') 
                                                                        AND date_deleted IS NULL";
                                             using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlPartDirect, conn, transaction))
                                             {
                                                 cmd.Parameters.AddWithValue("@name", cleanName);
                                                 using (var reader = cmd.ExecuteReader())
                                                 {
                                                     if (reader.Read())
                                                     {
                                                         fallbackPartId = Convert.ToInt32(reader["id"]);
                                                         fallbackPartName = reader["part_name"].ToString();
                                                         fallbackPartStock = Convert.ToDouble(reader["quantity_in_stock"]);
                                                         fallbackPartUom = reader["unit_of_measure"]?.ToString() ?? "";
                                                         fallbackStockType = reader["stock_type"]?.ToString() ?? "Piece";
                                                         fallbackPackItems = reader["pack_items_number"] != DBNull.Value ? Convert.ToInt32(reader["pack_items_number"]) : 1;
                                                         fallbackBigUnit = reader["big_unit"]?.ToString() ?? "";
                                                         fallbackSmallUnit = reader["small_unit"]?.ToString() ?? "";
                                                         fallbackConvVal = reader["conversion_value"] != DBNull.Value ? Convert.ToDouble(reader["conversion_value"]) : 1.0;
                                                         fallbackPackSize = reader["pack_size"] != DBNull.Value ? Convert.ToDouble(reader["pack_size"]) : 1.0;
                                                         fallbackPackPrice = reader["purchase_price"] != DBNull.Value ? Convert.ToDecimal(reader["purchase_price"]) : 0m;
                                                     }
                                                 }
                                             }
                                         }

                                         if (fallbackPartId > 0)
                                         {
                                             double totalDeduct = Data.RecipePartData.GetConvertedQuantityDynamic(
                                                 sale.QtySold, sale.UnitOfMeasure, fallbackPartUom, fallbackStockType, fallbackPackItems, fallbackBigUnit, fallbackSmallUnit, fallbackConvVal, fallbackPackSize);
                                             
                                             decimal unitCost = IngredientCalculationEngine.CalculatedUnitCost(
                                                 fallbackPackPrice, sale.UnitOfMeasure, fallbackBigUnit, fallbackSmallUnit, fallbackConvVal, fallbackPackSize, fallbackStockType, fallbackPackItems, fallbackPartUom);

                                             if (totalDeduct > 0)
                                             {
                                                 var existingDeduction = ingredientDeductions.FirstOrDefault(d => d.PartId == fallbackPartId);
                                                 double currentStockInDb = fallbackPartStock;
                                                 if (existingDeduction != null)
                                                 {
                                                     currentStockInDb = existingDeduction.NewStock;
                                                 }

                                                 double newStock = Math.Max(0, currentStockInDb - totalDeduct);

                                                 // Update parts table
                                                 string sqlUpdatePart = "UPDATE parts SET quantity_in_stock = @newStock WHERE id = @partId";
                                                 using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlUpdatePart, conn, transaction))
                                                 {
                                                     cmd.Parameters.AddWithValue("@newStock", newStock);
                                                     cmd.Parameters.AddWithValue("@partId", fallbackPartId);
                                                     cmd.ExecuteNonQuery();
                                                 }

                                                 // Record transaction
                                                 string sqlInsertTx = @"INSERT INTO transactions (action_type, part_name, description, username) 
                                                                       VALUES ('STOCK_DEDUCT', @partName, @desc, 'Admin')";
                                                 string txDesc = $"Deducted {totalDeduct:0.##} via sales import ({fallbackPartName} x{sale.QtySold})";
                                                 using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlInsertTx, conn, transaction))
                                                 {
                                                     cmd.Parameters.AddWithValue("@partName", fallbackPartName);
                                                     cmd.Parameters.AddWithValue("@desc", txDesc);
                                                     cmd.ExecuteNonQuery();
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
                                                         PartId = fallbackPartId,
                                                         PartName = fallbackPartName,
                                                         QtyDeducted = totalDeduct,
                                                         PreviousStock = fallbackPartStock,
                                                         NewStock = newStock
                                                     });
                                                 }
                                             }
                                             
                                             recipesProcessed++;
                                             itemsForOrder.Add(Tuple.Create(false, fallbackPartId, (double)sale.QtySold, unitCost, sale.UnitOfMeasure));
                                             continue;
                                         }
                                     }

                                     // Deduct ingredients
                                     foreach (var p in partsToDeduct)
                                     {
                                         double qtyPerRecipeConverted = Data.RecipePartData.GetConvertedQuantityDynamic(
                                             p.QtyPerRecipe, p.RecipeUom, p.PartUom, p.StockType, p.PackItems, p.BigUnit, p.SmallUnit, p.ConversionValue, p.PackSize);
                                         double totalDeduct = qtyPerRecipeConverted * (double)sale.QtySold;
                                         if (totalDeduct <= 0) continue;

                                         var existingDeduction = ingredientDeductions.FirstOrDefault(d => d.PartId == p.PartId);
                                         double currentStockInDb = p.CurrentStock;
                                         if (existingDeduction != null)
                                         {
                                             currentStockInDb = existingDeduction.NewStock;
                                         }

                                         double newStock = Math.Max(0, currentStockInDb - totalDeduct);

                                         // Update database
                                         string sqlUpdatePart = "UPDATE parts SET quantity_in_stock = @newStock WHERE id = @partId";
                                         using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlUpdatePart, conn, transaction))
                                         {
                                             cmd.Parameters.AddWithValue("@newStock", newStock);
                                             cmd.Parameters.AddWithValue("@partId", p.PartId);
                                             cmd.ExecuteNonQuery();
                                         }

                                         // Record transaction
                                         string sqlInsertTx = @"INSERT INTO transactions (action_type, part_name, description, username) 
                                                               VALUES ('STOCK_DEDUCT', @partName, @desc, 'Admin')";
                                         string txDesc = $"Deducted {totalDeduct:0.##} via sales import ({exactRecipeName} x{sale.QtySold})";
                                         using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlInsertTx, conn, transaction))
                                         {
                                             cmd.Parameters.AddWithValue("@partName", p.PartName);
                                             cmd.Parameters.AddWithValue("@desc", txDesc);
                                             cmd.ExecuteNonQuery();
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
                                                 PreviousStock = p.CurrentStock,
                                                 NewStock = newStock
                                             });
                                         }
                                     }

                                     recipesProcessed++;
                                     itemsForOrder.Add(Tuple.Create(true, recipeId, (double)sale.QtySold, sellingPrice, "pack"));
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
                                     using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlInsertOrder, conn, transaction))
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
                                         using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlInsertItem, conn, transaction))
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
                                     using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlInsertTxOrder, conn, transaction))
                                     {
                                         cmd.Parameters.AddWithValue("@partName", "POS Sale");
                                         cmd.Parameters.AddWithValue("@desc", orderDesc);
                                         cmd.ExecuteNonQuery();
                                     }
                                 }
                                             transaction.Commit();
                            }
                        }

                        // Broadcast inventory update
                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Daily sales imported ({recipesProcessed} processed)");

                        DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Imported sales");
                        return Microsoft.AspNetCore.Http.Results.Ok(new
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
                        return Microsoft.AspNetCore.Http.Results.Problem("Import sales failed: " + ex.Message);
                    }
                });

                // - Export CSV (POST) -
                app.MapPost("/api/export-csv", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ExportCsvPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || (string.IsNullOrEmpty(body.CsvContent) && string.IsNullOrEmpty(body.Base64Content)))
                            return Microsoft.AspNetCore.Http.Results.BadRequest("No content to export");

                        string filename = string.IsNullOrEmpty(body.Filename) ? $"inventory_{DateTime.Now:yyyyMMdd_HHmmss}.csv" : body.Filename;

                        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        string downloadsDir = System.IO.Path.Combine(userProfile, "Downloads");
                        if (!System.IO.Directory.Exists(downloadsDir))
                        {
                            System.IO.Directory.CreateDirectory(downloadsDir);
                        }

                        string filePath = System.IO.Path.Combine(downloadsDir, filename);
                        if (!string.IsNullOrEmpty(body.Base64Content))
                        {
                            byte[] bytes = Convert.FromBase64String(body.Base64Content);
                            System.IO.File.WriteAllBytes(filePath, bytes);
                        }
                        else
                        {
                            System.IO.File.WriteAllText(filePath, body.CsvContent, System.Text.Encoding.UTF8);
                        }

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, path = filePath });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to export: " + ex.Message);
                    }
                });

                // - Adjust Stock (POST) -
                app.MapPost("/api/adjust-stock", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<AdjustStockPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || body.PartId <= 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Invalid product ID");

                        var inventoryService = new Shaheen_InventoryManagement_Android.Services.InventoryService();
                        inventoryService.AdjustStock(body.PartId, body.Change, body.Reason ?? "Manual Adjustment");

                        // Broadcast update
                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Stock adjusted for item ID {body.PartId}");

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to adjust stock: " + ex.Message);
                    }
                });

                // - Stock Transactions (GET) -
                app.MapGet("/api/stock-transactions", (string date) =>
                {
                    try
                    {
                        string sql = @"SELECT action_type, part_name, description, datetime(timestamp, 'localtime') as timestamp, username
                                       FROM transactions
                                       WHERE action_type IN ('ADJUST_IN', 'ADJUST_OUT', 'STOCK_EDIT', 'STOCK_ADD', 'STOCK_DEDUCT')";
                        
                        var parameters = new System.Collections.Generic.List<Microsoft.Data.Sqlite.SqliteParameter>();
                        if (!string.IsNullOrEmpty(date))
                        {
                            sql += " AND date(datetime(timestamp, 'localtime')) = @date";
                            parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("@date", date));
                        }
                        sql += " ORDER BY id DESC";

                        var dt = DatabaseHelper.ExecuteDataTable(sql, parameters.ToArray());
                        var list = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
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
                        return Microsoft.AspNetCore.Http.Results.Ok(list);
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to fetch transactions: " + ex.Message);
                    }
                });

                // - Checkout (POST) -
                app.MapPost("/api/checkout", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<CheckoutPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || body.Items == null || body.Items.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Empty cart");

                        decimal total = 0;
                        var orderItems = new List<OrderItem>();
                        foreach (var item in body.Items)
                        {
                            total += item.Price * item.Qty;
                            bool isRecipe = item.ItemType == "Recipe" || (item.RecipeId.HasValue && item.RecipeId.Value > 0);
                            orderItems.Add(new OrderItem
                            {
                                PartId = isRecipe ? 0 : item.Id,
                                RecipeId = isRecipe ? (item.RecipeId ?? item.Id) : (int?)null,
                                ItemType = isRecipe ? "Recipe" : "Part",
                                Quantity = item.Qty,
                                UnitPrice = item.Price,
                                UnitOfMeasure = item.UnitOfMeasure ?? "pack"
                            });
                        }

                        var orderService = new OrderService();
                        int orderId = orderService.PlaceOrder(-1, orderItems, total, true, "Completed");

                        DatabaseHelper.LogTransaction("SALE", "POS Sale", $"Order #{orderId} -- Total: {total:C}");

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, orderId, total });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Checkout failed: " + ex.Message);
                    }
                });

                // - Currencies (GET) -
                app.MapGet("/api/currencies", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable("SELECT code, name, symbol, rate_vs_usd FROM currency_rates ORDER BY code");
                        var currencies = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            currencies.Add(new
                            {
                                code = row["code"].ToString(),
                                name = row["name"].ToString(),
                                symbol = row["symbol"].ToString(),
                                rate = Convert.ToDecimal(row["rate_vs_usd"])
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(currencies);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Recipes (GET) -
                app.MapGet("/api/recipes", () =>
                {
                    try
                    {
                        var list = Shaheen_InventoryManagement_Android.Data.RecipeData.GetAllRecipes();
                        var result = new System.Collections.Generic.List<object>();
                        foreach (var r in list)
                        {
                            var partsList = new System.Collections.Generic.List<object>();
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
                             string recipeImage = r.RecipeImage ?? "";
                             if (!string.IsNullOrEmpty(recipeImage) && IsImagePath(recipeImage))
                             {
                                 if (recipeImage.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) { }
                                 else if (recipeImage.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                                     recipeImage = "/" + recipeImage;
                                 else if (!recipeImage.StartsWith("/") && !recipeImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                     recipeImage = "/Assets/" + recipeImage;
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
                                 image = recipeImage,
                                 parts = partsList
                             });
                         }
                         return Microsoft.AspNetCore.Http.Results.Ok(result);
                     }
                     catch (Exception ex)
                     {
                         ErrorLogger.LogError(ex, "Program.GetRecipes");
                         return Microsoft.AspNetCore.Http.Results.Problem(ex.Message);
                     }
                 });
 
                // - Recipes Sales Frequency (GET) -
                app.MapGet("/api/recipes/sales-frequency", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT recipe_id, SUM(quantity) as sales_count
                              FROM order_items
                              WHERE item_type = 'Recipe' AND recipe_id IS NOT NULL AND recipe_id > 0
                              GROUP BY recipe_id");

                        var freq = new System.Collections.Generic.Dictionary<int, double>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            if (row["recipe_id"] != DBNull.Value && row["sales_count"] != DBNull.Value)
                            {
                                int rId = Convert.ToInt32(row["recipe_id"]);
                                double count = Convert.ToDouble(row["sales_count"]);
                                freq[rId] = count;
                            }
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(freq);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, "Program.GetRecipesSalesFrequency");
                        return Microsoft.AspNetCore.Http.Results.Problem(ex.Message);
                    }
                });

                 // - Save Recipe (POST) -
                 app.MapPost("/api/recipes", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                 {
                     try
                     {
                         var body = await System.Text.Json.JsonSerializer.DeserializeAsync<RecipePayload>(request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                         if (body == null || string.IsNullOrEmpty(body.Name))
                             return Microsoft.AspNetCore.Http.Results.BadRequest("Missing recipe name");
 
                         var recipe = new Shaheen_InventoryManagement_Android.Data.RecipeData
                         {
                             RecipeName = body.Name,
                             ItemNo = body.ItemNo ?? "",
                             Description = body.Description ?? "",
                             SellingPrice = body.Price,
                             CategoryName = body.CategoryName ?? "",
                             RecipeImage = body.Image ?? "",
                             Status = "Active"
                         };

                        if (body.Ingredients != null)
                        {
                            foreach (var ing in body.Ingredients)
                            {
                                recipe.Parts.Add(new Shaheen_InventoryManagement_Android.Data.RecipePartData
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
                            Shaheen_InventoryManagement_Android.Data.RecipeData.UpdateRecipe(recipe);
                        }
                        else
                        {
                            Shaheen_InventoryManagement_Android.Data.RecipeData.AddRecipe(recipe);
                        }

                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Recipe saved: {recipe.RecipeName}");
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, "Program.SaveRecipe");
                        return Microsoft.AspNetCore.Http.Results.Problem(ex.Message);
                    }
                });

                // - Delete Recipe (DELETE) -
                app.MapDelete("/api/recipes/{id}", (int id) =>
                {
                    try
                    {
                        Shaheen_InventoryManagement_Android.Data.RecipeData.DeleteRecipe(id);
                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Recipe deleted: ID {id}");
                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, "Program.DeleteRecipe");
                        return Microsoft.AspNetCore.Http.Results.Problem(ex.Message);
                    }
                });

                app.MapGet("/api/sales-export", () =>
                {
                    try
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
                            var list = dt.Rows.Cast<System.Data.DataRow>().Select(row => new
                            {
                                itemNo = row["item_no"]?.ToString() ?? "",
                                itemName = row["item_name"]?.ToString() ?? "",
                                qtySold = row["qty_sold"] != DBNull.Value ? Convert.ToInt32(row["qty_sold"]) : 0,
                                unit = row["unit"]?.ToString() ?? "pcs"
                            }).ToList();

                            return Microsoft.AspNetCore.Http.Results.Ok(list);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to export sales: " + ex.Message);
                    }
                });

                // - Recent Sales (GET) -
                app.MapGet("/api/recent-sales", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT o.order_id, datetime(o.order_date, 'localtime') as order_date, o.total_amount, 
                                     COALESCE(c.full_name, 'Cash Customer') as customer_name
                              FROM orders o
                              LEFT JOIN customers c ON o.customer_id = c.customer_id
                              WHERE o.status != 'Cancelled'
                              ORDER BY o.order_id DESC LIMIT 50");

                        var sales = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            sales.Add(new
                            {
                                orderId = Convert.ToInt32(row["order_id"]),
                                date = Convert.ToDateTime(row["order_date"]),
                                total = Convert.ToDecimal(row["total_amount"]),
                                customer = row["customer_name"].ToString()
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(sales);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Sales Items (GET) -
                app.MapGet("/api/sales-items", (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        string dateParam = request.Query["date"];
                        string query = @"SELECT oi.order_item_id, COALESCE(p.part_name, r.recipe_name) as item_name, oi.quantity, oi.price, datetime(o.order_date, 'localtime') as order_date, oi.unit_of_measure
                                         FROM order_items oi
                                         LEFT JOIN parts p ON oi.part_id = p.id AND oi.item_type != 'Recipe'
                                         LEFT JOIN recipes r ON oi.recipe_id = r.id AND oi.item_type = 'Recipe'
                                         JOIN orders o ON oi.order_id = o.order_id";

                        System.Data.DataTable dt;
                        if (!string.IsNullOrEmpty(dateParam))
                        {
                            query += " WHERE date(datetime(o.order_date, 'localtime')) = @date ORDER BY oi.order_item_id DESC";
                            dt = DatabaseHelper.ExecuteDataTable(query, new SqliteParameter("@date", dateParam));
                        }
                        else
                        {
                            query += " ORDER BY oi.order_item_id DESC LIMIT 100";
                            dt = DatabaseHelper.ExecuteDataTable(query);
                        }

                        var items = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            items.Add(new
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
                        return Microsoft.AspNetCore.Http.Results.Ok(items);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - License Info (GET) -
                app.MapGet("/api/license-info", () =>
                {
                    try
                    {
                        var license = Helpers.LicenseManager.GetCurrentLicense();
                        if (license == null)
                        {
                            return Microsoft.AspNetCore.Http.Results.Ok(new
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

                        return Microsoft.AspNetCore.Http.Results.Ok(new
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
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Factory Reset (POST) -
                app.MapPost("/api/factory-reset", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<FactoryResetPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || !VerifyAdminCredentials(body.AdminUsername, body.AdminPassword))
                        {
                            return Microsoft.AspNetCore.Http.Results.Json(new { success = false, message = "Invalid Admin username or password." }, statusCode: 401);
                        }

                        PerformFactoryReset();

                        // Broadcast reset event to all clients
                        _ = InventoryBroadcaster.Broadcast("FactoryResetComplete", "Database reset complete");

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Factory reset failed: " + ex.Message);
                    }
                });

                // - Order Details (GET) -
                app.MapGet("/api/order-details/{id}", (int id) =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT oi.part_id, p.part_name, oi.quantity, oi.price
                              FROM order_items oi
                              JOIN parts p ON oi.part_id = p.id
                              WHERE oi.order_id = @id",
                            new Microsoft.Data.Sqlite.SqliteParameter("@id", id));

                        var items = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            items.Add(new
                            {
                                partId = Convert.ToInt32(row["part_id"]),
                                name = row["part_name"].ToString(),
                                qty = Convert.ToInt32(row["quantity"]),
                                price = Convert.ToDecimal(row["price"])
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(items);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // - Return Item (POST) -
                app.MapPost("/api/return-item", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ReturnPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || body.Items == null || body.Items.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.BadRequest("No items to return");

                        var returnService = new ReturnService();
                        var items = new System.Collections.Generic.List<ReturnItemInfo>();
                        foreach (var i in body.Items)
                        {
                            items.Add(new ReturnItemInfo
                            {
                                PartId = i.PartId,
                                Quantity = i.Qty,
                                RefundAmount = i.RefundAmount
                            });
                        }

                        // Use a dummy user or extract from context if we have one
                        UserSession.Username = "WebPOS";

                        returnService.ProcessReturn(body.OrderId, items, body.Reason);

                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Return processed for Order #{body.OrderId}");

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
                });

                // Ports are configured via Kestrel above
                app.Run();
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("server_error.txt", DateTime.Now.ToString() + ": " + ex.ToString() + "\n");
            }
        }


        // Payload models for API
        private class UserPayload
        {
            public int? Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string FullName { get; set; }
            public string Role { get; set; }
            public int IsActive { get; set; }
        }

        private class SyncSessionPayload
        {
            public string Username { get; set; }
            public string Role { get; set; }
        }

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
            public string UnitOfMeasure { get; set; }
            public string ItemNo { get; set; }
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

        private class AdjustStockPayload
        {
            public int PartId { get; set; }
            public double Change { get; set; }
            public string Reason { get; set; }
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

        private class CheckoutPayload
        {
            public System.Collections.Generic.List<CheckoutItem> Items { get; set; }
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

        private class ReturnPayload
        {
            public int OrderId { get; set; }
            public string Reason { get; set; }
            public System.Collections.Generic.List<ReturnItemDetail> Items { get; set; }
        }
        private class ReturnItemDetail
        {
            public int PartId { get; set; }
            public int Qty { get; set; }
            public decimal RefundAmount { get; set; }
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
            public System.Collections.Generic.List<RecipeIngredientPayload> Ingredients { get; set; }
        }

        private class RecipeIngredientPayload
        {
            public int PartId { get; set; }
            public double Qty { get; set; }
            public string UnitOfMeasure { get; set; }
        }

        private class BulkImportPayload
        {
            public System.Collections.Generic.List<ImportItemDetail> Items { get; set; }
        }

        private class BulkImportRecipesPayload
        {
            public System.Collections.Generic.List<ImportRecipeItem> Recipes { get; set; }
        }

        private class ImportRecipeItem
        {
            public string Name { get; set; }
            public string CategoryName { get; set; }
            public decimal Price { get; set; }
            public string Description { get; set; }
            public string ItemNo { get; set; }
            public System.Collections.Generic.List<ImportRecipeIngredient> Ingredients { get; set; }
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

        private class DailySalesImportPayload
        {
            public System.Collections.Generic.List<DailySaleDetail> Sales { get; set; }
        }

        private class DailySaleDetail
        {
            public string ItemNo { get; set; }
            public string RecipeName { get; set; }
            public int QtySold { get; set; }
            public string UnitOfMeasure { get; set; }
        }

        private class ExportCsvPayload
        {
            public string Filename { get; set; }
            public string CsvContent { get; set; }
            public string Base64Content { get; set; }
        }

        private class RecipePartDeduction
        {
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

        private class IngredientDeductionResult
        {
            public int PartId { get; set; }
            public string PartName { get; set; }
            public double QtyDeducted { get; set; }
            public double PreviousStock { get; set; }
            public double NewStock { get; set; }
        }

        private static void CheckCurrentLicense()
        {
            try
            {
                var license = Helpers.LicenseManager.GetCurrentLicense();
                if (license == null)
                {
                    Console.WriteLine("Current License: NULL (No license stored)");
                }
                else
                {
                    Console.WriteLine($"Current License: Key={license.Key}, Type={license.LicenseType}, Customer={license.CustomerName}, Expiry={license.ExpirationDate}, IsValid={license.IsValid()}, IsTrial={license.IsTrial()}");
                    Console.WriteLine($"HasValidLicense: {Helpers.LicenseManager.HasValidLicense()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception checking license: {ex.Message}");
            }
        }

        private static void RunLicenseTests()
        {
            Console.WriteLine("Running licensing system tests...");
            try
            {
                // Clean up previous licenses
                Helpers.LicenseManager.DeactivateLicense();
                if (Helpers.LicenseManager.HasValidLicense())
                {
                    Console.WriteLine("Error: License still reported as valid after deactivation.");
                    Environment.Exit(1);
                }

                // Validate test key
                string testKey = "CHAHN-YEAR1-00000-27365-01211";
                string testCustomer = "Test User";
                
                bool isValid = Helpers.LicenseManager.ValidateLicenseKey(testKey, testCustomer);
                if (!isValid)
                {
                    Console.WriteLine("Error: Valid license key CHAHN-YEAR1-00000-27365-01211 was rejected for customer Test User.");
                    Environment.Exit(1);
                }

                // Verify check on invalid customer
                bool isValidIncorrect = Helpers.LicenseManager.ValidateLicenseKey("CHAHN-YEAR1-00000-27365-99999", testCustomer);
                if (isValidIncorrect)
                {
                    Console.WriteLine("Error: Invalid checksum key was validated successfully.");
                    Environment.Exit(1);
                }

                // Activate license
                var activated = Helpers.LicenseManager.ActivateLicense(testKey, testCustomer);
                if (activated == null)
                {
                    Console.WriteLine("Error: Failed to activate license.");
                    Environment.Exit(1);
                }

                if (!Helpers.LicenseManager.HasValidLicense())
                {
                    Console.WriteLine("Error: HasValidLicense returned false after activation.");
                    Environment.Exit(1);
                }

                // Clean up
                Helpers.LicenseManager.DeactivateLicense();
                Console.WriteLine("Licensing system tests PASSED successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in licensing tests: {ex.Message}");
                Environment.Exit(1);
            }
        }

        public class FactoryResetPayload
        {
            public string AdminUsername { get; set; }
            public string AdminPassword { get; set; }
        }

        public static bool VerifyAdminCredentials(string username, string password)
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

        public static void PerformFactoryReset()
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

            DatabaseHelper.LogUserAction("System", "Factory Reset", "All application data reset to initial state");
        }
    }
}

// ============================================================
//  SignalR Hub -- manages real-time WebSocket connections
// ============================================================
namespace Shaheen_InventoryManagement_Android
{
    using Microsoft.AspNetCore.SignalR;

    /// <summary>
    /// SignalR Hub for real-time inventory synchronization.
    /// Connected clients (web POS tablets and WinForms app) receive
    /// live push events whenever stock or sales data changes.
    /// </summary>
    public class InventoryHub : Hub
    {
        /// <summary>Called by any client to trigger a refresh on all others.</summary>
        public async Task RequestRefresh(string reason = "manual")
        {
            await Clients.Others.SendAsync("StockUpdated", reason);
        }
    }

    /// <summary>
    /// Static broadcaster: lets WinForms code push events to ALL
    /// connected web clients (tablets) with a single line of code.
    /// Usage: _ = InventoryBroadcaster.Broadcast("SaleCompleted", "Order #42");
    /// </summary>
    public static class InventoryBroadcaster
    {
        public static IHubContext<InventoryHub> HubContext { get; set; }

        /// <summary>
        /// Broadcasts a named event + message to every connected SignalR client.
        /// Safe to call fire-and-forget: _ = InventoryBroadcaster.Broadcast(...)
        /// </summary>
        public static async System.Threading.Tasks.Task Broadcast(string eventName, string message = "")
        {
            try
            {
                if (HubContext != null)
                    await HubContext.Clients.All.SendAsync(eventName, message);
            }
            catch { /* Never crash the caller due to broadcast failure */ }
        }

        /// <summary>Convenience: broadcast a generic stock-changed event.</summary>
        public static void BroadcastStockChange(string reason = "desktop")
        {
            _ = Broadcast("StockUpdated", reason);
        }
    }
}

