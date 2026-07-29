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
                                     p.description, p.stock_type, p.pack_items_number, p.pack_price, p.item_price, p.piece_price,
                                     p.big_unit, p.small_unit, p.conversion_value, p.pack_size,
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
                                description = row["description"].ToString(),
                                stockType = row["stock_type"] != DBNull.Value ? row["stock_type"].ToString() : "Piece",
                                packItemsNumber = row["pack_items_number"] != DBNull.Value ? Convert.ToInt32(row["pack_items_number"]) : 0,
                                packPrice = row["pack_price"] != DBNull.Value ? Convert.ToDecimal(row["pack_price"]) : 0m,
                                itemPrice = row["item_price"] != DBNull.Value ? Convert.ToDecimal(row["item_price"]) : 0m,
                                piecePrice = row["piece_price"] != DBNull.Value ? Convert.ToDecimal(row["piece_price"]) : 0m,
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
                                    pack_size = @pack_size
                                WHERE id = @id";

                            DatabaseHelper.ExecuteNonQuery(sql,
                                new Microsoft.Data.Sqlite.SqliteParameter("@name", body.Name),
                                new Microsoft.Data.Sqlite.SqliteParameter("@sku", body.Sku ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@cat", catId),
                                new Microsoft.Data.Sqlite.SqliteParameter("@p_price", body.Price * 0.7m),
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
                                new Microsoft.Data.Sqlite.SqliteParameter("@id", body.Id.Value));

                            DatabaseHelper.LogTransaction("STOCK_EDIT", body.Name, $"Edited via WebPOS (New Qty: {body.Stock})");
                            _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Item '{body.Name}' updated via Web POS");
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
                                                   big_unit, small_unit, conversion_value, pack_size)
                                VALUES (@name, @sku, @cat, @p_price, @s_price, @stock, @min_stock, @barcode, 'Active', @itemNo, @uom,
                                        @stock_type, @pack_items_number, @pack_price, @item_price, @piece_price,
                                        @big_unit, @small_unit, @conversion_value, @pack_size)";

                            DatabaseHelper.ExecuteNonQuery(sql,
                                new Microsoft.Data.Sqlite.SqliteParameter("@name", body.Name),
                                new Microsoft.Data.Sqlite.SqliteParameter("@sku", body.Sku ?? ""),
                                new Microsoft.Data.Sqlite.SqliteParameter("@cat", catId),
                                new Microsoft.Data.Sqlite.SqliteParameter("@p_price", body.Price * 0.7m),
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
                                new Microsoft.Data.Sqlite.SqliteParameter("@pack_size", body.PackSize));

                            DatabaseHelper.LogTransaction("STOCK_ADD", body.Name, $"Added via WebPOS (Qty: {body.Stock})");
                            _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Item '{body.Name}' added via Web POS");
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

                            int catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c) AND date_deleted IS NULL",
                                        new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName));
                            if (catId == 0)
                            {
                                DatabaseHelper.ExecuteNonQuery("INSERT INTO categories (category_name) VALUES (@c)",
                                            new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName));
                                catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE LOWER(category_name) = LOWER(@c) AND date_deleted IS NULL",
                                            new Microsoft.Data.Sqlite.SqliteParameter("@c", categoryName));
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

                            imported++;
                        }

                        _ = InventoryBroadcaster.Broadcast("InventoryChanged", $"Imported {imported} items via Web POS");

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, imported, skipped });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Bulk import failed: " + ex.Message);
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
                                    if (string.IsNullOrWhiteSpace(sale.RecipeName) || sale.QtySold <= 0)
                                    {
                                        recipesSkipped++;
                                        continue;
                                    }

                                    int recipeId = 0;
                                    string exactRecipeName = "";
                                    decimal sellingPrice = 0;

                                    // 1. Find recipe by Item No
                                    if (!string.IsNullOrWhiteSpace(sale.ItemNo))
                                    {
                                        string sqlRecipeByNo = @"SELECT id, recipe_name, selling_price FROM recipes 
                                                                 WHERE LOWER(item_no) = LOWER(@itemNo) AND date_deleted IS NULL";
                                        using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlRecipeByNo, conn, transaction))
                                        {
                                            cmd.Parameters.AddWithValue("@itemNo", sale.ItemNo.Trim());
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
                                    if (recipeId == 0 && !string.IsNullOrWhiteSpace(sale.RecipeName))
                                    {
                                        string sqlRecipe = @"SELECT id, recipe_name, selling_price FROM recipes 
                                                             WHERE REPLACE(REPLACE(REPLACE(REPLACE(LOWER(recipe_name), ' ', ''), '\t', ''), '\r', ''), '\n', '') = 
                                                                   REPLACE(REPLACE(REPLACE(REPLACE(LOWER(@name), ' ', ''), '\t', ''), '\r', ''), '\n', '') 
                                                               AND date_deleted IS NULL";
                                        using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlRecipe, conn, transaction))
                                        {
                                            cmd.Parameters.AddWithValue("@name", sale.RecipeName.Trim());
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
                                        if (!string.IsNullOrWhiteSpace(sale.ItemNo))
                                        {
                                            string sqlPartByNo = @"SELECT id, part_name, selling_price, quantity_in_stock, unit_of_measure, stock_type, pack_items_number,
                                                                          big_unit, small_unit, conversion_value, pack_size, purchase_price FROM parts 
                                                                   WHERE LOWER(item_no) = LOWER(@itemNo) AND date_deleted IS NULL";
                                            using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlPartByNo, conn, transaction))
                                            {
                                                cmd.Parameters.AddWithValue("@itemNo", sale.ItemNo.Trim());
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
                                        if (partId == 0 && !string.IsNullOrWhiteSpace(sale.RecipeName))
                                        {
                                            string sqlPartDirect = @"SELECT id, part_name, selling_price, quantity_in_stock, unit_of_measure, stock_type, pack_items_number,
                                                                            big_unit, small_unit, conversion_value, pack_size, purchase_price FROM parts 
                                                                     WHERE REPLACE(REPLACE(REPLACE(REPLACE(LOWER(part_name), ' ', ''), '\t', ''), '\r', ''), '\n', '') = 
                                                                           REPLACE(REPLACE(REPLACE(REPLACE(LOWER(@name), ' ', ''), '\t', ''), '\r', ''), '\n', '') 
                                                                       AND date_deleted IS NULL";
                                            using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlPartDirect, conn, transaction))
                                            {
                                                cmd.Parameters.AddWithValue("@name", sale.RecipeName.Trim());
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
                                            string missingName = !string.IsNullOrWhiteSpace(sale.RecipeName) ? sale.RecipeName.Trim() : (!string.IsNullOrWhiteSpace(sale.ItemNo) ? "Item No " + sale.ItemNo : "Unknown");
                                            if (!skippedRecipes.Contains(missingName))
                                                skippedRecipes.Add(missingName);
                                            continue;
                                        }

                                        // Convert the quantity using the new unit conversion logic
                                        double totalDeduct = Data.RecipePartData.GetConvertedQuantityDynamic(
                                            sale.QtySold, sale.UnitOfMeasure, partUom, stockType, packItems, bigUnit, smallUnit, convVal, packSize);
                                        
                                        // Calculate the cost dynamically based on the unit of sale
                                        decimal calculatedCost = IngredientCalculationEngine.CalculatedUnitCost(
                                            packPrice, sale.UnitOfMeasure, bigUnit, smallUnit, convVal, packSize, stockType, packItems, partUom) * (decimal)sale.QtySold;

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
                                        itemsForOrder.Add(Tuple.Create(false, partId, (double)sale.QtySold, calculatedCost, sale.UnitOfMeasure));
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
                                        totalOrderAmount += item.Item4; // Item4 is already total price (qty * price) for parts or sellingPrice for recipe
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

                                        // If part, price is total cost, so unit price = price / qty
                                        decimal unitPrice = isRecipe ? price : (qty > 0 ? price / (decimal)qty : price);

                                        string sqlInsertItem = isRecipe 
                                            ? @"INSERT INTO order_items (order_id, part_id, quantity, price, item_type, recipe_id, unit_of_measure) 
                                                VALUES (@orderId, 0, @qty, @price, 'Recipe', @recipeId, @uom)"
                                            : @"INSERT INTO order_items (order_id, part_id, quantity, price, item_type, recipe_id, unit_of_measure) 
                                                VALUES (@orderId, @partId, @qty, @price, 'Part', NULL, @uom)";
                                        using (var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sqlInsertItem, conn, transaction))
                                        {
                                            cmd.Parameters.AddWithValue("@orderId", orderId);
                                            cmd.Parameters.AddWithValue("@qty", qty);
                                            cmd.Parameters.AddWithValue("@price", unitPrice);
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

                        if (body == null || string.IsNullOrEmpty(body.CsvContent))
                            return Microsoft.AspNetCore.Http.Results.BadRequest("No content to export");

                        string filename = string.IsNullOrEmpty(body.Filename) ? $"inventory_{DateTime.Now:yyyyMMdd_HHmmss}.csv" : body.Filename;

                        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        string downloadsDir = System.IO.Path.Combine(userProfile, "Downloads");
                        if (!System.IO.Directory.Exists(downloadsDir))
                        {
                            System.IO.Directory.CreateDirectory(downloadsDir);
                        }

                        string filePath = System.IO.Path.Combine(downloadsDir, filename);
                        System.IO.File.WriteAllText(filePath, body.CsvContent, System.Text.Encoding.UTF8);

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, path = filePath });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to export CSV: " + ex.Message);
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
                        string sql = @"SELECT action_type, part_name, description, timestamp, username
                                       FROM transactions
                                       WHERE action_type IN ('ADJUST_IN', 'ADJUST_OUT', 'STOCK_EDIT', 'STOCK_ADD')";
                        
                        var parameters = new System.Collections.Generic.List<Microsoft.Data.Sqlite.SqliteParameter>();
                        if (!string.IsNullOrEmpty(date))
                        {
                            sql += " AND date(timestamp) = @date";
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
        }

        private class AdjustStockPayload
        {
            public int PartId { get; set; }
            public double Change { get; set; }
            public string Reason { get; set; }
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

        private class BulkImportPayload
        {
            public System.Collections.Generic.List<ImportItemDetail> Items { get; set; }
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

