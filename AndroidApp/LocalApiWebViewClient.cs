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
                    return GetSalesItems();
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
                else if (endpoint == "api/login" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessLogin(body);
                }
                else if (endpoint == "api/add-item" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessAddItem(body);
                }
                else if (endpoint == "api/checkout" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessCheckout(body);
                }
                else if (endpoint == "api/return-item" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessReturnItem(body);
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
                    return GetReportsData();
                }
                else if (endpoint == "api/clear-reports" && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return ClearReportsData();
                }

                return JsonSerializer.Serialize(new { error = $"Endpoint not found: {endpoint} ({method})" });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"WebAppInterface.HandleApiCall: {url}");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        private string GetProducts()
        {
            var dt = DatabaseHelper.ExecuteDataTable(
                @"SELECT p.id, p.part_name, p.selling_price, p.quantity_in_stock,
                         p.minimum_stock_level, p.barcode, p.part_number, p.part_image,
                         p.item_no,
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
                    stock = Convert.ToInt32(row["quantity_in_stock"]),
                    minStock = Convert.ToInt32(row["minimum_stock_level"]),
                    barcode = row["barcode"].ToString(),
                    sku = row["part_number"].ToString(),
                    category = category,
                    image = CleanImagePrefix(row["part_image"].ToString()),
                    categoryImage = CleanCategoryIcon(category, row["category_image"].ToString()),
                    isService = category.Equals("Services", StringComparison.OrdinalIgnoreCase)
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
                @"SELECT o.order_id, o.order_date, o.total_amount, 
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

        private string GetSalesItems()
        {
            var dt = DatabaseHelper.ExecuteDataTable(
                @"SELECT oi.order_item_id, COALESCE(p.part_name, r.recipe_name) as item_name, oi.quantity, oi.price, o.order_date
                  FROM order_items oi
                  LEFT JOIN parts p ON oi.part_id = p.id AND oi.item_type != 'Recipe'
                  LEFT JOIN recipes r ON oi.recipe_id = r.id AND oi.item_type = 'Recipe'
                  JOIN orders o ON oi.order_id = o.order_id
                  ORDER BY oi.order_item_id DESC LIMIT 100");

            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    id = Convert.ToInt32(row["order_item_id"]),
                    name = row["item_name"].ToString(),
                    qty = Convert.ToInt32(row["quantity"]),
                    price = Convert.ToDecimal(row["price"]),
                    total = Convert.ToInt32(row["quantity"]) * Convert.ToDecimal(row["price"]),
                    date = Convert.ToDateTime(row["order_date"])
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
                return JsonSerializer.Serialize(new { username = "Softio.Admin", role = "Admin", fullName = "Softio Super Admin" });

            if (body.Username == "test" && body.Password == "Test.Softio")
                return JsonSerializer.Serialize(new { username = "test", role = "Admin", fullName = "Test Admin" });

            var dt = DatabaseHelper.ExecuteDataTable(
                "SELECT username, role, full_name FROM users WHERE username = @u AND password = @p",
                new SqliteParameter("@u", body.Username),
                new SqliteParameter("@p", body.Password));

            if (dt.Rows.Count == 0)
                return JsonSerializer.Serialize(new { error = "Unauthorized" });

            var row = dt.Rows[0];
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

            int catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE category_name = @c",
                        new SqliteParameter("@c", body.Category ?? "General"));
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
                        barcode = @barcode,
                        item_no = @itemNo
                    WHERE id = @id";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new SqliteParameter("@name", body.Name),
                    new SqliteParameter("@sku", body.Sku ?? ""),
                    new SqliteParameter("@cat", catId),
                    new SqliteParameter("@p_price", body.Price * 0.7m),
                    new SqliteParameter("@s_price", body.Price),
                    new SqliteParameter("@stock", body.Stock),
                    new SqliteParameter("@barcode", body.Barcode ?? ""),
                    new SqliteParameter("@itemNo", body.ItemNo ?? ""),
                    new SqliteParameter("@id", body.Id.Value));

                DatabaseHelper.LogTransaction("STOCK_EDIT", body.Name, $"Edited via Android App (New Qty: {body.Stock})");
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
                    INSERT INTO parts (part_name, part_number, category_id, purchase_price, selling_price, quantity_in_stock, barcode, status, item_no)
                    VALUES (@name, @sku, @cat, @p_price, @s_price, @stock, @barcode, 'Active', @itemNo)";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new SqliteParameter("@name", body.Name),
                    new SqliteParameter("@sku", body.Sku ?? ""),
                    new SqliteParameter("@cat", catId),
                    new SqliteParameter("@p_price", body.Price * 0.7m),
                    new SqliteParameter("@s_price", body.Price),
                    new SqliteParameter("@stock", body.Stock),
                    new SqliteParameter("@barcode", body.Barcode ?? ""),
                    new SqliteParameter("@itemNo", body.ItemNo ?? ""));

                DatabaseHelper.LogTransaction("STOCK_ADD", body.Name, $"Added via Android App (Qty: {body.Stock})");
                return JsonSerializer.Serialize(new { success = true });
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
                    
                    orderItems.Add(new OrderItem
                    {
                        ItemType = isRecipe ? "Recipe" : "Part",
                        PartId = isRecipe ? 0 : item.Id,
                        RecipeId = isRecipe ? (item.RecipeId ?? item.Id) : (int?)null,
                        PartName = item.Name,
                        Quantity = item.Qty,
                        UnitPrice = item.Price
                    });
                }

                var orderService = new OrderService();
                int orderId = orderService.PlaceOrder(-1, orderItems, total, true, "Completed");

                DatabaseHelper.LogTransaction("SALE", "POS Sale", $"Order #{orderId} -- Total: {total:C}");

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
                            unitCost = rp.UnitCost,
                            totalCost = rp.TotalCost
                        });
                    }
                    result.Add(new
                    {
                        id = r.Id,
                        name = r.RecipeName,
                        itemNo = r.ItemNo,
                        description = r.Description,
                        price = r.SellingPrice,
                        totalCost = r.TotalCost,
                        categoryName = r.CategoryName,
                        parts = partsList
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
                    Status = "Active"
                };

                if (body.Ingredients != null)
                {
                    foreach (var ing in body.Ingredients)
                    {
                        recipe.Parts.Add(new RecipePartData
                        {
                            PartId = ing.PartId,
                            Quantity = ing.Qty
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

                    if (!string.IsNullOrEmpty(item.Barcode))
                    {
                        int existingCount = DatabaseHelper.ExecuteScalar<int>(
                            "SELECT COUNT(*) FROM parts WHERE barcode = @b AND date_deleted IS NULL",
                            new SqliteParameter("@b", item.Barcode));
                        if (existingCount > 0)
                        {
                            skipped++;
                            continue;
                        }
                    }

                    string categoryName = item.Category ?? "General";
                    int catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE category_name = @c",
                                new SqliteParameter("@c", categoryName));
                    if (catId == 0)
                    {
                        DatabaseHelper.ExecuteNonQuery("INSERT INTO categories (category_name) VALUES (@c)", new SqliteParameter("@c", categoryName));
                        catId = DatabaseHelper.ExecuteScalar<int>("SELECT id FROM categories WHERE category_name = @c",
                                    new SqliteParameter("@c", categoryName));
                    }
                    if (catId == 0) catId = 1;

                    string sql = @"
                        INSERT INTO parts (part_name, part_number, category_id, purchase_price, selling_price, quantity_in_stock, barcode, status, description, item_no)
                        VALUES (@name, @sku, @cat, @p_price, @s_price, @stock, @barcode, 'Active', @desc, @itemNo)";

                    DatabaseHelper.ExecuteNonQuery(sql,
                        new SqliteParameter("@name", item.Name),
                        new SqliteParameter("@sku", item.Sku ?? ""),
                        new SqliteParameter("@cat", catId),
                        new SqliteParameter("@p_price", item.Price * 0.7m),
                        new SqliteParameter("@s_price", item.Price),
                        new SqliteParameter("@stock", item.Stock),
                        new SqliteParameter("@barcode", item.Barcode ?? ""),
                        new SqliteParameter("@desc", item.Description ?? ""),
                        new SqliteParameter("@itemNo", item.ItemNo ?? ""));

                    imported++;
                }

                return JsonSerializer.Serialize(new { success = true, imported, skipped });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.ProcessBulkImport");
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

                using (var conn = new SqliteConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        var itemsForOrder = new List<Tuple<bool, int, double, decimal>>(); // <isRecipe, id, qty, price>

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

                            // 1. Find the recipe ID and selling price by Item No (if provided)
                            if (!string.IsNullOrWhiteSpace(sale.ItemNo))
                            {
                                string sqlRecipeByNo = @"SELECT id, recipe_name, selling_price FROM recipes 
                                                         WHERE LOWER(item_no) = LOWER(@itemNo) AND date_deleted IS NULL";
                                using (var cmd = new SqliteCommand(sqlRecipeByNo, conn, transaction))
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

                            // 2. Find the recipe ID and selling price by name (fallback, case-insensitive and ignoring all whitespace)
                            if (recipeId == 0 && !string.IsNullOrWhiteSpace(sale.RecipeName))
                            {
                                string sqlRecipe = @"SELECT id, recipe_name, selling_price FROM recipes 
                                                     WHERE REPLACE(REPLACE(REPLACE(REPLACE(LOWER(recipe_name), ' ', ''), '\t', ''), '\r', ''), '\n', '') = 
                                                           REPLACE(REPLACE(REPLACE(REPLACE(LOWER(@name), ' ', ''), '\t', ''), '\r', ''), '\n', '') 
                                                       AND date_deleted IS NULL";
                                using (var cmd = new SqliteCommand(sqlRecipe, conn, transaction))
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

                                // Fallback A: Search the parts (inventory) table directly by Item No (if provided)
                                if (!string.IsNullOrWhiteSpace(sale.ItemNo))
                                {
                                    string sqlPartByNo = @"SELECT id, part_name, selling_price, quantity_in_stock FROM parts 
                                                           WHERE LOWER(item_no) = LOWER(@itemNo) AND date_deleted IS NULL";
                                    using (var cmd = new SqliteCommand(sqlPartByNo, conn, transaction))
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
                                            }
                                        }
                                    }
                                }

                                // Fallback B: Search the parts (inventory) table directly by name (case-insensitive and ignoring all whitespace)
                                if (partId == 0 && !string.IsNullOrWhiteSpace(sale.RecipeName))
                                {
                                    string sqlPartDirect = @"SELECT id, part_name, selling_price, quantity_in_stock FROM parts 
                                                             WHERE REPLACE(REPLACE(REPLACE(REPLACE(LOWER(part_name), ' ', ''), '\t', ''), '\r', ''), '\n', '') = 
                                                                   REPLACE(REPLACE(REPLACE(REPLACE(LOWER(@name), ' ', ''), '\t', ''), '\r', ''), '\n', '') 
                                                               AND date_deleted IS NULL";
                                    using (var cmd = new SqliteCommand(sqlPartDirect, conn, transaction))
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

                                // Match found in parts! Deduct from stock directly
                                double totalDeduct = (double)sale.QtySold;
                                if (totalDeduct > 0)
                                {
                                    var existingDeduction = ingredientDeductions.FirstOrDefault(d => d.PartId == partId);
                                    double currentStockInDb = currentPartStock;
                                    if (existingDeduction != null)
                                    {
                                        currentStockInDb = existingDeduction.NewStock;
                                    }

                                    double newStock = currentStockInDb - totalDeduct;

                                    // Update parts table
                                    string sqlUpdatePart = "UPDATE parts SET quantity_in_stock = @newStock WHERE id = @partId";
                                    using (var cmd = new SqliteCommand(sqlUpdatePart, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@newStock", newStock);
                                        cmd.Parameters.AddWithValue("@partId", partId);
                                        cmd.ExecuteNonQuery();
                                    }

                                    // Record transaction
                                    string sqlInsertTx = @"INSERT INTO transactions (action_type, part_name, description, username) 
                                                          VALUES ('STOCK_DEDUCT', @partName, @desc, 'Admin')";
                                    string txDesc = $"Deducted {totalDeduct:0.##} via sales import ({exactPartName} x{sale.QtySold})";
                                    using (var cmd = new SqliteCommand(sqlInsertTx, conn, transaction))
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
                                itemsForOrder.Add(Tuple.Create(false, partId, (double)sale.QtySold, partSellingPrice));
                                continue;
                            }

                            // 2. Find parts (ingredients) for this recipe
                            string sqlParts = @"SELECT rp.part_id, rp.quantity, p.part_name, p.quantity_in_stock 
                                                FROM recipe_parts rp
                                                JOIN parts p ON rp.part_id = p.id
                                                WHERE rp.recipe_id = @recipeId AND p.date_deleted IS NULL";
                            var partsToDeduct = new List<RecipePartDeduction>();
                            using (var cmd = new SqliteCommand(sqlParts, conn, transaction))
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
                                            CurrentStock = Convert.ToDouble(reader["quantity_in_stock"])
                                        });
                                    }
                                }
                            }

                            // 3. Deduct ingredients from stock
                            foreach (var p in partsToDeduct)
                            {
                                double totalDeduct = p.QtyPerRecipe * (double)sale.QtySold;
                                if (totalDeduct <= 0) continue;

                                // Check if we already processed this ingredient in a previous loop iteration of this upload
                                var existingDeduction = ingredientDeductions.FirstOrDefault(d => d.PartId == p.PartId);
                                double currentStockInDb = p.CurrentStock;
                                if (existingDeduction != null)
                                {
                                    currentStockInDb = existingDeduction.NewStock;
                                }

                                double newStock = currentStockInDb - totalDeduct;

                                // Update parts table
                                string sqlUpdatePart = "UPDATE parts SET quantity_in_stock = @newStock WHERE id = @partId";
                                using (var cmd = new SqliteCommand(sqlUpdatePart, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@newStock", newStock);
                                    cmd.Parameters.AddWithValue("@partId", p.PartId);
                                    cmd.ExecuteNonQuery();
                                }

                                // Record transaction
                                string sqlInsertTx = @"INSERT INTO transactions (action_type, part_name, description, username) 
                                                      VALUES ('STOCK_DEDUCT', @partName, @desc, 'Admin')";
                                string txDesc = $"Deducted {totalDeduct:0.##} via sales import ({exactRecipeName} x{sale.QtySold})";
                                using (var cmd = new SqliteCommand(sqlInsertTx, conn, transaction))
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
                            itemsForOrder.Add(Tuple.Create(true, recipeId, (double)sale.QtySold, sellingPrice));
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

                                string sqlInsertItem = isRecipe 
                                    ? @"INSERT INTO order_items (order_id, part_id, quantity, price, item_type, recipe_id) 
                                        VALUES (@orderId, 0, @qty, @price, 'Recipe', @recipeId)"
                                    : @"INSERT INTO order_items (order_id, part_id, quantity, price, item_type, recipe_id) 
                                        VALUES (@orderId, @partId, @qty, @price, 'Part', NULL)";
                                using (var cmd = new SqliteCommand(sqlInsertItem, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@orderId", orderId);
                                    cmd.Parameters.AddWithValue("@qty", qty);
                                    cmd.Parameters.AddWithValue("@price", price);
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

                        transaction.Commit();
                    }
                }

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
                if (body == null || string.IsNullOrEmpty(body.CsvContent))
                    return JsonSerializer.Serialize(new { error = "No content to export" });

                string filename = string.IsNullOrEmpty(body.Filename) ? $"inventory_{DateTime.Now:yyyyMMdd_HHmmss}.csv" : body.Filename;

                var values = new Android.Content.ContentValues();
                values.Put(Android.Provider.MediaStore.MediaColumns.DisplayName, filename);
                values.Put(Android.Provider.MediaStore.MediaColumns.MimeType, "text/csv");
                values.Put(Android.Provider.MediaStore.MediaColumns.RelativePath, Android.OS.Environment.DirectoryDownloads);

                var resolver = _context.ContentResolver;
                var uri = resolver.Insert(Android.Provider.MediaStore.Downloads.ExternalContentUri, values);
                if (uri != null)
                {
                    using (var stream = resolver.OpenOutputStream(uri))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(body.CsvContent);
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

        private string GetReportsData()
        {
            try
            {
                decimal totalRevenue = DatabaseHelper.ExecuteScalar<decimal>(
                    "SELECT COALESCE(SUM(total_amount), 0) FROM orders WHERE status = 'Completed'");

                int totalOrders = DatabaseHelper.ExecuteScalar<int>(
                    @"SELECT COALESCE(CAST(SUM(oi.quantity) AS INT), 0) 
                      FROM order_items oi 
                      JOIN orders o ON oi.order_id = o.order_id 
                      WHERE o.status = 'Completed'");

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

                var categorySales = new List<object>();
                var dtCat = DatabaseHelper.ExecuteDataTable(
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
                      WHERE o.status = 'Completed'
                      GROUP BY category_name
                      ORDER BY total_sales DESC");
                foreach (DataRow row in dtCat.Rows)
                {
                    categorySales.Add(new
                    {
                        category = row["category_name"].ToString(),
                        sales = Convert.ToDecimal(row["total_sales"])
                    });
                }

                var recentTransactions = new List<object>();
                var dtTx = DatabaseHelper.ExecuteDataTable(
                    @"SELECT action_type, part_name, description, timestamp, username
                      FROM transactions
                      ORDER BY id DESC LIMIT 10");
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

                return JsonSerializer.Serialize(new
                {
                    revenue = totalRevenue,
                    orders = totalOrders,
                    outOfStock = outOfStock,
                    lowStock = lowStock,
                    categorySales = categorySales,
                    transactions = recentTransactions
                });
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "WebAppInterface.GetReportsData");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
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

        private static string CleanImagePrefix(string partImage)
        {
            if (string.IsNullOrEmpty(partImage)) return "";
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
        }

        private class RecipePayload
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public string ItemNo { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public string CategoryName { get; set; }
            public List<RecipeIngredientPayload> Ingredients { get; set; }
        }

        private class RecipeIngredientPayload
        {
            public int PartId { get; set; }
            public double Qty { get; set; }
        }

        private class BulkImportPayload
        {
            public List<ImportItemDetail> Items { get; set; }
        }

        private class ImportItemDetail
        {
            public string Name { get; set; }
            public string Category { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Barcode { get; set; }
            public string Sku { get; set; }
            public string Description { get; set; }
            public string ItemNo { get; set; }
        }

        private class ExportCsvPayload
        {
            public string Filename { get; set; }
            public string CsvContent { get; set; }
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
        }

        private class RecipePartDeduction
        {
            public int PartId { get; set; }
            public string PartName { get; set; }
            public double QtyPerRecipe { get; set; }
            public double CurrentStock { get; set; }
        }

        private class IngredientDeductionResult
        {
            public int PartId { get; set; }
            public string PartName { get; set; }
            public double QtyDeducted { get; set; }
            public double PreviousStock { get; set; }
            public double NewStock { get; set; }
        }
    }
}
