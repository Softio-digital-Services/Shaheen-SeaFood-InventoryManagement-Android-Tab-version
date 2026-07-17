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
                            @"SELECT p.id, p.part_name, p.selling_price, p.quantity_in_stock,
                                     p.minimum_stock_level, p.barcode, p.part_number, p.part_image,
                                     p.description,
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
                            if (!string.IsNullOrEmpty(partImage))
                            {
                                if (partImage.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                                    partImage = "/" + partImage;
                                else if (!partImage.StartsWith("/") && !partImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                    partImage = "/Assets/" + partImage;
                            }

                            products.Add(new
                            {
                                id = Convert.ToInt32(row["id"]),
                                name = row["part_name"].ToString(),
                                price = Convert.ToDecimal(row["selling_price"]),
                                stock = Convert.ToInt32(row["quantity_in_stock"]),
                                minStock = Convert.ToInt32(row["minimum_stock_level"]),
                                barcode = row["barcode"].ToString(),
                                sku = row["part_number"].ToString(),
                                category = category,
                                image = partImage,
                                categoryImage = catImage,
                                isService = category.Equals("Services", StringComparison.OrdinalIgnoreCase),
                                description = row["description"].ToString()
                            });
                        }

                        return Microsoft.AspNetCore.Http.Results.Ok(products);
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("DB error: " + ex.Message);
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
                            return Microsoft.AspNetCore.Http.Results.Ok(new { username = "Softio.Admin", role = "Admin", fullName = "Softio Super Admin" });

                        if (body.Username == "test" && body.Password == "Test.Softio")
                            return Microsoft.AspNetCore.Http.Results.Ok(new { username = "test", role = "Admin", fullName = "Test Admin" });

                        var dt = DatabaseHelper.ExecuteDataTable(
                            "SELECT username, role, full_name FROM users WHERE username = @u AND password = @p",
                            new Microsoft.Data.Sqlite.SqliteParameter("@u", body.Username),
                            new Microsoft.Data.Sqlite.SqliteParameter("@p", body.Password));

                        if (dt.Rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.Unauthorized();

                        var row = dt.Rows[0];
                        return Microsoft.AspNetCore.Http.Results.Ok(new
                        {
                            username = row["username"].ToString(),
                            role = row["role"].ToString(),
                            fullName = row["full_name"].ToString()
                        });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Login error: " + ex.Message);
                    }
                });

                // - Add Item (POST) -
                app.MapPost("/api/add-item", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<AddItemPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || string.IsNullOrEmpty(body.Name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Missing name");

                        if (!string.IsNullOrEmpty(body.Barcode))
                        {
                            int existingCount = DatabaseHelper.ExecuteScalar<int>(
                                "SELECT COUNT(*) FROM parts WHERE barcode = @b AND date_deleted IS NULL",
                                new Microsoft.Data.Sqlite.SqliteParameter("@b", body.Barcode));

                            if (existingCount > 0)
                                return Microsoft.AspNetCore.Http.Results.Conflict(new { error = "Barcode already exists for another item." });
                        }

                        int catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE category_name = @c",
                                    new Microsoft.Data.Sqlite.SqliteParameter("@c", body.Category ?? "General"));
                        if (catId == 0) catId = 1;

                        string sql = @"
                            INSERT INTO parts (part_name, part_number, category_id, purchase_price, selling_price, quantity_in_stock, barcode, status)
                            VALUES (@name, @sku, @cat, @p_price, @s_price, @stock, @barcode, 'Active')";

                        DatabaseHelper.ExecuteNonQuery(sql,
                            new Microsoft.Data.Sqlite.SqliteParameter("@name", body.Name),
                            new Microsoft.Data.Sqlite.SqliteParameter("@sku", body.Sku ?? ""),
                            new Microsoft.Data.Sqlite.SqliteParameter("@cat", catId),
                            new Microsoft.Data.Sqlite.SqliteParameter("@p_price", body.Price * 0.7m),
                            new Microsoft.Data.Sqlite.SqliteParameter("@s_price", body.Price),
                            new Microsoft.Data.Sqlite.SqliteParameter("@stock", body.Stock),
                            new Microsoft.Data.Sqlite.SqliteParameter("@barcode", body.Barcode ?? ""));

                        DatabaseHelper.LogTransaction("STOCK_ADD", body.Name, $"Added via WebPOS (Qty: {body.Stock})");

                        // - Broadcast real-time update to all connected clients -
                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Item '{body.Name}' added via Web POS");

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to add item: " + ex.Message);
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
                        foreach (var item in body.Items) total += item.Price * item.Qty;

                        // SQLite: insert order then get its rowid separately
                        DatabaseHelper.ExecuteNonQuery(
                            "INSERT INTO orders (order_date, customer_id, total_amount, status, payment_status, payment_method) " +
                            "VALUES (datetime('now'), NULL, @total, 'Completed', 'Paid', 'WebPOS')",
                            new Microsoft.Data.Sqlite.SqliteParameter("@total", total));
                        long orderId = DatabaseHelper.ExecuteScalar<long>("SELECT last_insert_rowid()");

                        foreach (var item in body.Items)
                        {
                            bool isRecipe = item.ItemType == "Recipe" || (item.RecipeId.HasValue && item.RecipeId.Value > 0);
                            int partId = isRecipe ? 0 : item.Id;
                            int? recipeId = isRecipe ? (item.RecipeId ?? item.Id) : (int?)null;
                            string itemType = isRecipe ? "Recipe" : "Part";

                            DatabaseHelper.ExecuteNonQuery(
                                "INSERT INTO order_items (order_id, part_id, quantity, price, item_type, recipe_id) VALUES (@oid, @pid, @qty, @price, @type, @rid)",
                                new Microsoft.Data.Sqlite.SqliteParameter("@oid", orderId),
                                new Microsoft.Data.Sqlite.SqliteParameter("@pid", partId),
                                new Microsoft.Data.Sqlite.SqliteParameter("@qty", item.Qty),
                                new Microsoft.Data.Sqlite.SqliteParameter("@price", item.Price),
                                new Microsoft.Data.Sqlite.SqliteParameter("@type", itemType),
                                new Microsoft.Data.Sqlite.SqliteParameter("@rid", (object)recipeId ?? DBNull.Value));

                            if (!isRecipe && item.Id > 0)
                            {
                                DatabaseHelper.ExecuteNonQuery(
                                    "UPDATE parts SET quantity_in_stock = quantity_in_stock - @qty WHERE id = @pid",
                                    new Microsoft.Data.Sqlite.SqliteParameter("@qty", item.Qty),
                                    new Microsoft.Data.Sqlite.SqliteParameter("@pid", item.Id));
                            }
                        }

                        DatabaseHelper.ExecuteNonQuery(
                            "INSERT INTO transactions (action_type, part_name, description, username) VALUES ('SALE', 'POS Sale', @desc, 'WebPOS')",
                            new Microsoft.Data.Sqlite.SqliteParameter("@desc", $"Order #{orderId} -- Total: {total:C}"));

                        // - Broadcast real-time update to ALL connected clients -
                        _ = InventoryBroadcaster.Broadcast("SaleCompleted", $"Order #{orderId} - Total: {total:F2}");

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

                // - Recent Sales (GET) -
                app.MapGet("/api/recent-sales", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT o.order_id, o.order_date, o.total_amount, 
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
                app.MapGet("/api/sales-items", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT oi.order_item_id, COALESCE(p.part_name, r.recipe_name) as item_name, oi.quantity, oi.price, o.order_date
                              FROM order_items oi
                              LEFT JOIN parts p ON oi.part_id = p.id AND oi.item_type != 'Recipe'
                              LEFT JOIN recipes r ON oi.recipe_id = r.id AND oi.item_type = 'Recipe'
                              JOIN orders o ON oi.order_id = o.order_id
                              ORDER BY oi.order_item_id DESC LIMIT 100");

                        var items = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            items.Add(new
                            {
                                id = Convert.ToInt32(row["order_item_id"]),
                                name = row["item_name"].ToString(),
                                qty = Convert.ToInt32(row["quantity"]),
                                price = Convert.ToDecimal(row["price"]),
                                total = Convert.ToInt32(row["quantity"]) * Convert.ToDecimal(row["price"]),
                                date = Convert.ToDateTime(row["order_date"])
                            });
                        }
                        return Microsoft.AspNetCore.Http.Results.Ok(items);
                    }
                    catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
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
        private class LoginPayload
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private class AddItemPayload
        {
            public string Name { get; set; }
            public string Category { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Barcode { get; set; }
            public string Sku { get; set; }
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

