using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using GenericInventorySystem.Services;

namespace GenericInventorySystem
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Set initial language based on build configuration
#if ARABIC_VERSION
            GenericInventorySystem.Helpers.LocalizationManager.SetLanguage("ar-SA");
#else
            GenericInventorySystem.Helpers.LocalizationManager.SetLanguage("en-US");
#endif
            
            // Expose background task for server hosting without blocking UI thread
            _ = Task.Run(() => StartApiServer());
            
            Application.ThreadException += (s, e) => {
                try { System.IO.File.AppendAllText("crash.txt", DateTime.Now.ToString() + ": " + e.Exception.ToString() + "\n\n"); } catch {}
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                try { System.IO.File.AppendAllText("crash.txt", DateTime.Now.ToString() + ": " + e.ExceptionObject.ToString() + "\n\n"); } catch {}
            };
            
            try 
            {
                // Initialize Database (Create if missing)
                GenericInventorySystem.Helpers.DatabaseInitializer.Initialize();
                
                // Ensure schema is up to date (add missing columns)
                DatabaseHelper.EnsureSchema();

                // Initialize currency tables and load cached rates
                GenericInventorySystem.Services.CurrencyService.EnsureTable();

                // Check License
                if (!GenericInventorySystem.Helpers.LicenseManager.HasValidLicense())
                {
                    // Show activation form
                    GenericInventorySystem.Forms.LicenseActivationForm activationForm = new GenericInventorySystem.Forms.LicenseActivationForm();
                    if (activationForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        // User cancelled activation - exit application
                        return;
                    }
                }

                // Check for expiring license and show warning
                var license = GenericInventorySystem.Helpers.LicenseManager.GetCurrentLicense();
                if (license != null && license.IsExpiringSoon() && !license.IsTrial())
                {
                    int daysLeft = license.DaysRemaining();
                    GenericInventorySystem.Forms.ModernMessageBox.Show(
                        $"Your license will expire in {daysLeft} days.\n\nPlease renew your license to continue using the software.",
                        "License Expiring Soon",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }

                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                GenericInventorySystem.Forms.ModernMessageBox.Show($"CRITICAL ERROR: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Application Crash", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void StartApiServer()
        {
            try
            {
                var builder = WebApplication.CreateBuilder();
                builder.Services.AddCors(c => c.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

                var app = builder.Build();
                app.UseCors();
                app.UseStaticFiles();

                // ── Status ───────────────────────────────────────────────────
                app.MapGet("/api/status", () => Microsoft.AspNetCore.Http.Results.Ok(new { status = "API Running", version = "2.0" }));

                // ── Products (live from DB) ───────────────────────────────────
                app.MapGet("/api/products", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable(
                            @"SELECT p.id, p.part_name, p.selling_price, p.quantity_in_stock,
                                     p.minimum_stock_level, p.barcode, p.part_number,
                                     ISNULL(c.category_name, 'General') AS category
                              FROM parts p
                              LEFT JOIN categories c ON p.category_id = c.id
                              WHERE p.date_deleted IS NULL AND p.status = 'Active'
                              ORDER BY c.category_name, p.part_name");

                        var products = new System.Collections.Generic.List<object>();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            products.Add(new {
                                id       = Convert.ToInt32(row["id"]),
                                name     = row["part_name"].ToString(),
                                price    = Convert.ToDecimal(row["selling_price"]),
                                stock    = Convert.ToInt32(row["quantity_in_stock"]),
                                minStock = Convert.ToInt32(row["minimum_stock_level"]),
                                barcode  = row["barcode"].ToString(),
                                sku      = row["part_number"].ToString(),
                                category = row["category"].ToString(),
                                isService = false
                            });
                        }

                        // Append hardcoded service items (not in parts table)
                        products.Add(new { id = -1, name = "Standard Labor (1hr)", price = 80.00m, stock = 999, minStock = 0, barcode = "", sku = "SVC-001", category = "Services", isService = true });
                        products.Add(new { id = -2, name = "Diagnostic Check",     price = 40.00m, stock = 999, minStock = 0, barcode = "", sku = "SVC-002", category = "Services", isService = true });

                        return Microsoft.AspNetCore.Http.Results.Ok(products);
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("DB error: " + ex.Message);
                    }
                });

                // ── Categories ────────────────────────────────────────────────
                app.MapGet("/api/categories", () =>
                {
                    try
                    {
                        var dt = DatabaseHelper.ExecuteDataTable("SELECT category_name FROM categories ORDER BY category_name");
                        var categories = new System.Collections.Generic.List<string>();
                        foreach (System.Data.DataRow row in dt.Rows)
                            categories.Add(row["category_name"].ToString());
                        
                        // Ensure "Services" is included as it's a special category in the API
                        if (!categories.Contains("Services")) categories.Add("Services");
                        
                        return Microsoft.AspNetCore.Http.Results.Ok(categories);
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("DB error: " + ex.Message);
                    }
                });

                // ── Login (POST) ─────────────────────────────────────────────
                app.MapPost("/api/login", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<LoginPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Password))
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Missing credentials");

                        var dt = DatabaseHelper.ExecuteDataTable(
                            "SELECT username, role, full_name FROM users WHERE username = @u AND password = @p",
                            new System.Data.SqlClient.SqlParameter("@u", body.Username),
                            new System.Data.SqlClient.SqlParameter("@p", body.Password));

                        if (dt.Rows.Count == 0)
                            return Microsoft.AspNetCore.Http.Results.Unauthorized();

                        var row = dt.Rows[0];
                        return Microsoft.AspNetCore.Http.Results.Ok(new {
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

                // ── Add Item (POST) ───────────────────────────────────────────
                app.MapPost("/api/add-item", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
                {
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<AddItemPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (body == null || string.IsNullOrEmpty(body.Name))
                            return Microsoft.AspNetCore.Http.Results.BadRequest("Missing name");

                        // 0. Check Barcode Uniqueness
                        if (!string.IsNullOrEmpty(body.Barcode))
                        {
                            int existingCount = DatabaseHelper.ExecuteScalar<int>(
                                "SELECT COUNT(*) FROM parts WHERE barcode = @b AND date_deleted IS NULL",
                                new System.Data.SqlClient.SqlParameter("@b", body.Barcode));
                            
                            if (existingCount > 0)
                                return Microsoft.AspNetCore.Http.Results.Conflict(new { error = "Barcode already exists for another item." });
                        }

                        // 1. Get Category ID (or default to General)
                        int catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE category_name = @c", 
                                    new System.Data.SqlClient.SqlParameter("@c", body.Category ?? "General"));
                        if (catId == 0) catId = 1; // Fallback to first category

                        // 2. Insert Part
                        string sql = @"
                            INSERT INTO parts (part_name, part_number, category_id, purchase_price, selling_price, quantity_in_stock, barcode, status)
                            VALUES (@name, @sku, @cat, @p_price, @s_price, @stock, @barcode, 'Active')";

                        DatabaseHelper.ExecuteNonQuery(sql,
                            new System.Data.SqlClient.SqlParameter("@name",    body.Name),
                            new System.Data.SqlClient.SqlParameter("@sku",     body.Sku ?? ""),
                            new System.Data.SqlClient.SqlParameter("@cat",     catId),
                            new System.Data.SqlClient.SqlParameter("@p_price", body.Price * 0.7m), // Estimate cost
                            new System.Data.SqlClient.SqlParameter("@s_price", body.Price),
                            new System.Data.SqlClient.SqlParameter("@stock",   body.Stock),
                            new System.Data.SqlClient.SqlParameter("@barcode", body.Barcode ?? ""));

                        DatabaseHelper.LogTransaction("STOCK_ADD", body.Name, $"Added via WebPOS (Qty: {body.Stock})");

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Failed to add item: " + ex.Message);
                    }
                });

                // ── Checkout (POST) ───────────────────────────────────────────
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

                        // 1. Insert order
                        string insertOrder = @"
                            INSERT INTO orders (order_date, customer_id, total_amount, status, payment_status, payment_method)
                            VALUES (GETDATE(), NULL, @total, 'Completed', 'Paid', 'POS');
                            SELECT SCOPE_IDENTITY();";
                        int orderId = DatabaseHelper.ExecuteScalar<int>(insertOrder,
                            new System.Data.SqlClient.SqlParameter("@total", total));

                        // 2. Insert line items & deduct stock
                        foreach (var item in body.Items)
                        {
                            if (item.Id > 0) // skip service items
                            {
                                DatabaseHelper.ExecuteNonQuery(
                                    "INSERT INTO order_items (order_id, part_id, quantity, price) VALUES (@oid, @pid, @qty, @price)",
                                    new System.Data.SqlClient.SqlParameter("@oid",   orderId),
                                    new System.Data.SqlClient.SqlParameter("@pid",   item.Id),
                                    new System.Data.SqlClient.SqlParameter("@qty",   item.Qty),
                                    new System.Data.SqlClient.SqlParameter("@price", item.Price));

                                DatabaseHelper.ExecuteNonQuery(
                                    "UPDATE parts SET quantity_in_stock = quantity_in_stock - @qty WHERE id = @pid",
                                    new System.Data.SqlClient.SqlParameter("@qty", item.Qty),
                                    new System.Data.SqlClient.SqlParameter("@pid", item.Id));
                            }
                        }

                        // 3. Log transaction
                        DatabaseHelper.ExecuteNonQuery(
                            "INSERT INTO transactions (action_type, part_name, description, username) VALUES ('SALE', 'POS Sale', @desc, 'WebPOS')",
                            new System.Data.SqlClient.SqlParameter("@desc", $"Order #{orderId} — Total: {total:C}"));

                        return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, orderId, total });
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem("Checkout failed: " + ex.Message);
                    }
                });

                app.Run("http://0.0.0.0:5000");
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
            public int     Id    { get; set; }
            public string  Name  { get; set; }
            public decimal Price { get; set; }
            public int     Qty   { get; set; }
        }
    }
}
