using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Shaheen_InventoryManagement_Android.Helpers;

namespace Shaheen_InventoryManagement_Android.Services
{
    public class OrderItem
    {
        public string ItemType { get; set; } = "Part"; // "Part" or "Recipe"
        public int PartId { get; set; }
        public int? RecipeId { get; set; }
        public string PartName { get; set; }
        public string Description { get; set; }
        public string PartImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }

    public class OrderService
    {
        /// <summary>
        /// Places a new order, updates stock, and manages customer balance.
        /// </summary>
        /// <param name="customerId">Customer ID (-1 for Walk-in)</param>
        /// <param name="items">List of items in the cart</param>
        /// <param name="totalAmount">Total amount of the order</param>
        /// <param name="isPaid">Whether the order is fully paid</param>
        /// <returns>The ID of the created order</returns>
        public int PlaceOrder(int customerId, List<OrderItem> items, decimal totalAmount, bool isPaid, string orderStatus = "Completed", DateTime? dueDate = null, string shippingAddress = null, DateTime? deliveryDate = null)
        {
            // 1. Determine Status
            // Walk-in is always paid (enforced by UI, but logic here: if walk-in, force paid?)
            bool isWalkIn = customerId == -1;
            string paymentStatus = isPaid ? "Paid" : "Unpaid";
            if (orderStatus == "Draft") paymentStatus = "Pending"; // Drafts are pending payment automatically
            
            decimal amountPaid = isPaid ? totalAmount : 0;
            
            // 2. Insert Order
            string sqlOrder;
            if (isWalkIn)
            {
                sqlOrder = "INSERT INTO orders (order_date, total_amount, payment_status, amount_paid, status, shipping_address, delivery_date, due_date) " +
                           "VALUES (datetime('now'), @total, @status, @paid, @ostatus, @shipAddr, @delDate, @dueDate); SELECT last_insert_rowid();";
            }
            else
            {
                sqlOrder = "INSERT INTO orders (order_date, total_amount, payment_status, amount_paid, customer_id, status, shipping_address, delivery_date, due_date) " +
                           "VALUES (datetime('now'), @total, @status, @paid, @cid, @ostatus, @shipAddr, @delDate, @dueDate); SELECT last_insert_rowid();";
            }

            long orderIdLong = DatabaseHelper.ExecuteScalar<long>(sqlOrder,
                new SqliteParameter("@total", totalAmount),
                new SqliteParameter("@status", paymentStatus),
                new SqliteParameter("@paid", amountPaid),
                new SqliteParameter("@cid", customerId),
                new SqliteParameter("@ostatus", orderStatus),
                new SqliteParameter("@shipAddr", string.IsNullOrEmpty(shippingAddress) ? (object)DBNull.Value : shippingAddress),
                new SqliteParameter("@delDate", deliveryDate.HasValue ? (object)deliveryDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : DBNull.Value),
                new SqliteParameter("@dueDate", dueDate.HasValue ? (object)dueDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : DBNull.Value)
            );
            int orderId = Convert.ToInt32(orderIdLong);

            // If Draft or Quotation, skip stock updates and balance updates
            if (orderStatus == "Draft" || orderStatus == "Quotation")
            {
                 // Still insert items so we can retrieve them, but DON'T update stock
                 foreach (var item in items)
                 {
                    string sqlItem = "INSERT INTO order_items (order_id, part_id, quantity, price, item_type, recipe_id) VALUES (@oid, @pid, @qty, @price, @itype, @rid)";
                    DatabaseHelper.ExecuteNonQuery(sqlItem,
                        new SqliteParameter("@oid", orderId),
                        new SqliteParameter("@pid", item.PartId),
                        new SqliteParameter("@qty", item.Quantity),
                        new SqliteParameter("@price", item.UnitPrice),
                        new SqliteParameter("@itype", item.ItemType),
                        new SqliteParameter("@rid", item.RecipeId.HasValue ? (object)item.RecipeId.Value : DBNull.Value)
                    );
                 }
                 return orderId;
            }

            // 3. Process Items & Update Stock (Normally)
            foreach (var item in items)
            {
                // Insert Order Item
                string sqlItem = "INSERT INTO order_items (order_id, part_id, quantity, price, item_type, recipe_id) VALUES (@oid, @pid, @qty, @price, @itype, @rid)";
                DatabaseHelper.ExecuteNonQuery(sqlItem,
                    new SqliteParameter("@oid", orderId),
                    new SqliteParameter("@pid", item.PartId),
                    new SqliteParameter("@qty", item.Quantity),
                    new SqliteParameter("@price", item.UnitPrice),
                    new SqliteParameter("@itype", item.ItemType),
                    new SqliteParameter("@rid", item.RecipeId.HasValue ? (object)item.RecipeId.Value : DBNull.Value)
                );

                // Update Stock
                if (item.ItemType == "Recipe" && item.RecipeId.HasValue)
                {
                    // Deduct components of the recipe
                    string sqlRecipeParts = @"SELECT rp.part_id, rp.quantity, rp.unit_of_measure, p.unit_of_measure as part_uom, p.stock_type, p.pack_items_number 
                                             FROM recipe_parts rp 
                                             JOIN parts p ON rp.part_id = p.id 
                                             WHERE rp.recipe_id = @rid";
                    var rParts = DatabaseHelper.ExecuteDataTable(sqlRecipeParts, new SqliteParameter("@rid", item.RecipeId.Value));
                    foreach (System.Data.DataRow rp in rParts.Rows)
                    {
                        int pId = Convert.ToInt32(rp["part_id"]);
                        double rpQty = Convert.ToDouble(rp["quantity"]);
                        string recipeUom = rp["unit_of_measure"]?.ToString();
                        string partUom = rp["part_uom"]?.ToString();
                        string stockType = rp["stock_type"]?.ToString();
                        int packItems = rp["pack_items_number"] != DBNull.Value ? Convert.ToInt32(rp["pack_items_number"]) : 1;

                        double qtyPerRecipeConverted = Shaheen_InventoryManagement_Android.Data.RecipePartData.GetConvertedQuantity(rpQty, recipeUom, partUom, stockType, packItems);
                        double totalDeduct = qtyPerRecipeConverted * item.Quantity;
                        
                        string sqlStock = "UPDATE parts SET quantity_in_stock = CASE WHEN quantity_in_stock - @qty < 0 THEN 0 ELSE quantity_in_stock - @qty END WHERE id = @pid";
                        DatabaseHelper.ExecuteNonQuery(sqlStock,
                            new SqliteParameter("@qty", totalDeduct),
                            new SqliteParameter("@pid", pId)
                        );
                    }
                }
                else
                {
                    // Regular part
                    string sqlStock = "UPDATE parts SET quantity_in_stock = CASE WHEN quantity_in_stock - @qty < 0 THEN 0 ELSE quantity_in_stock - @qty END WHERE id = @pid";
                    DatabaseHelper.ExecuteNonQuery(sqlStock,
                        new SqliteParameter("@qty", item.Quantity),
                        new SqliteParameter("@pid", item.PartId)
                    );
                }
            }

            // 4. Update Customer Balance (if unpaid)
            if (!isPaid && !isWalkIn)
            {
                string sqlBalance = "UPDATE customers SET current_balance = current_balance + @total";
                List<SqliteParameter> parameters = new List<SqliteParameter> {
                    new SqliteParameter("@total", totalAmount),
                    new SqliteParameter("@cid", customerId)
                };

                if (dueDate.HasValue)
                {
                    sqlBalance += ", payment_due_date = @dueDate";
                    parameters.Add(new SqliteParameter("@dueDate", dueDate.Value));
                }

                sqlBalance += " WHERE customer_id = @cid";
                DatabaseHelper.ExecuteNonQuery(sqlBalance, parameters.ToArray());
            }

            // 5. Record in Customer History (if not Walk-in)
            if (!isWalkIn)
            {
                // Record the Sale (Payment Due)
                string sqlSaleRecord = "INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes, due_date) VALUES ('Customer', @cid, @amount, datetime('now'), @notes, @ddate)";
                DatabaseHelper.ExecuteNonQuery(sqlSaleRecord,
                    new SqliteParameter("@cid", customerId),
                    new SqliteParameter("@amount", totalAmount),
                    new SqliteParameter("@notes", "[Sale] Order #" + orderId),
                    new SqliteParameter("@ddate", (object)dueDate ?? DBNull.Value)
                );

                if (isPaid)
                {
                    // Record the Payment (Payment Received) - POS auto-payment
                    string sqlPayRecord = "INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes) VALUES ('Customer', @cid, @amount, datetime('now'), @notes)";
                    DatabaseHelper.ExecuteNonQuery(sqlPayRecord,
                        new SqliteParameter("@cid", customerId),
                        new SqliteParameter("@amount", totalAmount),
                        new SqliteParameter("@notes", "[Payment] Order #" + orderId)
                    );
                }
            }

            // 6. Log
            DatabaseHelper.LogTransaction("SALE", "Order #" + orderId, $"Total: ${totalAmount:N2}");
            
            // 7. Global Synchronization
            GlobalEvents.RaiseOrdersUpdated();     // New Order created
            GlobalEvents.RaiseInventoryUpdated(); // Stock reduced
            if (!isPaid && !isWalkIn) GlobalEvents.RaiseCustomersUpdated(); // Balance updated

            return orderId;
        }

        public System.Data.DataTable GetDrafts()
        {
             // Get orders with status 'Draft'
             string sql = @"
                SELECT o.order_id, o.order_date, COALESCE(c.full_name, 'Walk-in') as CustomerName, o.total_amount, o.customer_id
                FROM orders o
                LEFT JOIN customers c ON o.customer_id = c.customer_id
                WHERE o.status = 'Draft'
                ORDER BY o.order_date DESC";
             return DatabaseHelper.ExecuteDataTable(sql);
        }

        public System.Data.DataTable GetQuotations()
        {
             // Get orders with status 'Quotation'
             string sql = @"
                SELECT o.order_id, o.order_date, COALESCE(c.full_name, 'Walk-in') as CustomerName, o.total_amount, o.customer_id
                FROM orders o
                LEFT JOIN customers c ON o.customer_id = c.customer_id
                WHERE o.status = 'Quotation'
                ORDER BY o.order_date DESC";
             return DatabaseHelper.ExecuteDataTable(sql);
        }

        public bool ConvertToOrder(int orderId)
        {
            try
            {
                // 1. Get current order details
                var items = GetOrderItems(orderId);
                
                // 2. Perform Stock Validation
                foreach (var item in items)
                {
                    if (item.ItemType == "Recipe" && item.RecipeId.HasValue)
                    {
                        string sqlRecipeParts = @"SELECT rp.part_id, rp.quantity, rp.unit_of_measure, p.unit_of_measure as part_uom, p.stock_type, p.pack_items_number 
                                                 FROM recipe_parts rp 
                                                 JOIN parts p ON rp.part_id = p.id 
                                                 WHERE rp.recipe_id = @rid";
                        var rParts = DatabaseHelper.ExecuteDataTable(sqlRecipeParts, new SqliteParameter("@rid", item.RecipeId.Value));
                        foreach (System.Data.DataRow rp in rParts.Rows)
                        {
                            int pId = Convert.ToInt32(rp["part_id"]);
                            double rpQty = Convert.ToDouble(rp["quantity"]);
                            string recipeUom = rp["unit_of_measure"]?.ToString();
                            string partUom = rp["part_uom"]?.ToString();
                            string stockType = rp["stock_type"]?.ToString();
                            int packItems = rp["pack_items_number"] != DBNull.Value ? Convert.ToInt32(rp["pack_items_number"]) : 1;

                            double qtyPerRecipeConverted = Shaheen_InventoryManagement_Android.Data.RecipePartData.GetConvertedQuantity(rpQty, recipeUom, partUom, stockType, packItems);
                            double totalRequired = qtyPerRecipeConverted * item.Quantity;

                            int currentStock = DatabaseHelper.ExecuteScalar<int>($"SELECT quantity_in_stock FROM parts WHERE id = {pId}");
                            if (currentStock < totalRequired)
                            {
                                string partName = DatabaseHelper.ExecuteScalar<string>($"SELECT part_name FROM parts WHERE id = {pId}");
                                throw new Exception($"Insufficient stock for component {partName} of Recipe {item.PartName}. Available: {currentStock}, Required: {totalRequired}");
                            }
                        }
                    }
                    else
                    {
                        int currentStock = DatabaseHelper.ExecuteScalar<int>($"SELECT quantity_in_stock FROM parts WHERE id = {item.PartId}");
                        if (currentStock < item.Quantity)
                        {
                            string partName = DatabaseHelper.ExecuteScalar<string>($"SELECT part_name FROM parts WHERE id = {item.PartId}");
                            throw new Exception($"Insufficient stock for {partName}. Available: {currentStock}, Required: {item.Quantity}");
                        }
                    }
                }

                // 3. Update Status to Completed
                DatabaseHelper.ExecuteNonQuery("UPDATE orders SET status = 'Completed', order_date = datetime('now') WHERE order_id = @oid", 
                    new SqliteParameter("@oid", orderId));

                // 4. Update Stock
                foreach (var item in items)
                {
                    if (item.ItemType == "Recipe" && item.RecipeId.HasValue)
                    {
                        string sqlRecipeParts = @"SELECT rp.part_id, rp.quantity, rp.unit_of_measure, p.unit_of_measure as part_uom, p.stock_type, p.pack_items_number 
                                                 FROM recipe_parts rp 
                                                 JOIN parts p ON rp.part_id = p.id 
                                                 WHERE rp.recipe_id = @rid";
                        var rParts = DatabaseHelper.ExecuteDataTable(sqlRecipeParts, new SqliteParameter("@rid", item.RecipeId.Value));
                        foreach (System.Data.DataRow rp in rParts.Rows)
                        {
                            int pId = Convert.ToInt32(rp["part_id"]);
                            double rpQty = Convert.ToDouble(rp["quantity"]);
                            string recipeUom = rp["unit_of_measure"]?.ToString();
                            string partUom = rp["part_uom"]?.ToString();
                            string stockType = rp["stock_type"]?.ToString();
                            int packItems = rp["pack_items_number"] != DBNull.Value ? Convert.ToInt32(rp["pack_items_number"]) : 1;

                            double qtyPerRecipeConverted = Shaheen_InventoryManagement_Android.Data.RecipePartData.GetConvertedQuantity(rpQty, recipeUom, partUom, stockType, packItems);
                            double totalDeduct = qtyPerRecipeConverted * item.Quantity;
                            DatabaseHelper.ExecuteNonQuery("UPDATE parts SET quantity_in_stock = CASE WHEN quantity_in_stock - @qty < 0 THEN 0 ELSE quantity_in_stock - @qty END WHERE id = @pid", 
                                new SqliteParameter("@qty", totalDeduct), 
                                new SqliteParameter("@pid", pId));
                        }
                    }
                    else
                    {
                        DatabaseHelper.ExecuteNonQuery("UPDATE parts SET quantity_in_stock = CASE WHEN quantity_in_stock - @qty < 0 THEN 0 ELSE quantity_in_stock - @qty END WHERE id = @pid", 
                            new SqliteParameter("@qty", item.Quantity), 
                            new SqliteParameter("@pid", item.PartId));
                    }
                }

                // 5. Log & Notify
                DatabaseHelper.LogTransaction("SALE", "Quote #" + orderId + " -> Order", "Converted quotation to order");
                GlobalEvents.RaiseOrdersUpdated();
                GlobalEvents.RaiseInventoryUpdated();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Conversion failed: " + ex.Message);
            }
        }

        public System.Data.DataRow GetOrderHeader(int orderId)
        {
            string sql = @"SELECT o.order_id, o.order_date, o.total_amount, o.status, o.payment_status, o.shipping_address, o.delivery_date, o.due_date, c.full_name as customer_name
                           FROM orders o
                           LEFT JOIN customers c ON o.customer_id = c.customer_id
                           WHERE o.order_id = @id";
            var dt = DatabaseHelper.ExecuteDataTable(sql, new SqliteParameter("@id", orderId));
            if (dt != null && dt.Rows.Count > 0) return dt.Rows[0];
            return null;
        }

        public List<OrderItem> GetOrderItems(int orderId)
        {
             string sql = @"
                SELECT i.part_id, i.recipe_id, i.item_type, i.quantity, i.price, 
                       p.part_name, p.description, p.part_image,
                       r.recipe_name, r.description as r_desc
                FROM order_items i
                LEFT JOIN parts p ON i.part_id = p.id
                LEFT JOIN recipes r ON i.recipe_id = r.id
                WHERE i.order_id = @oid";
             
             return DatabaseHelper.ExecuteQuery(sql, reader => new OrderItem 
             {
                 PartId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                 RecipeId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                 ItemType = reader.IsDBNull(2) ? "Part" : reader.GetString(2),
                 Quantity = reader.GetInt32(3),
                 UnitPrice = reader.GetDecimal(4),
                 PartName = (reader.IsDBNull(2) || reader.GetString(2) == "Part") ? 
                            (reader.IsDBNull(5) ? "" : reader.GetString(5)) : 
                            (reader.IsDBNull(8) ? "" : reader.GetString(8)),
                 Description = (reader.IsDBNull(2) || reader.GetString(2) == "Part") ? 
                               (reader.IsDBNull(6) ? "" : reader.GetString(6)) : 
                               (reader.IsDBNull(9) ? "" : reader.GetString(9)),
                 PartImage = (reader.IsDBNull(2) || reader.GetString(2) == "Part") ? 
                             (reader.IsDBNull(7) ? "" : reader.GetString(7)) : ""
             }, new SqliteParameter("@oid", orderId));
        }

        public void DeleteOrder(int orderId)
        {
             // Delete items first
             DatabaseHelper.ExecuteNonQuery($"DELETE FROM order_items WHERE order_id = {orderId}");
             DatabaseHelper.ExecuteNonQuery($"DELETE FROM orders WHERE order_id = {orderId}");
        }
    }
}
