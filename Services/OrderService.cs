using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Services
{
    public class OrderItem
    {
        public int PartId { get; set; }
        public string PartName { get; set; }
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
        public int PlaceOrder(int customerId, List<OrderItem> items, decimal totalAmount, bool isPaid, string orderStatus = "Completed", DateTime? dueDate = null)
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
                sqlOrder = "INSERT INTO orders (order_date, total_amount, payment_status, amount_paid, status) " +
                           "VALUES (GETDATE(), @total, @status, @paid, @ostatus); SELECT SCOPE_IDENTITY();";
            }
            else
            {
                sqlOrder = "INSERT INTO orders (order_date, total_amount, payment_status, amount_paid, customer_id, status) " +
                           "VALUES (GETDATE(), @total, @status, @paid, @cid, @ostatus); SELECT SCOPE_IDENTITY();";
            }

            decimal orderIdDec = DatabaseHelper.ExecuteScalar<decimal>(sqlOrder,
                new SqlParameter("@total", totalAmount),
                new SqlParameter("@status", paymentStatus),
                new SqlParameter("@paid", amountPaid),
                new SqlParameter("@cid", customerId),
                new SqlParameter("@ostatus", orderStatus)
            );
            int orderId = Convert.ToInt32(orderIdDec);

            // If Draft or Quotation, skip stock updates and balance updates
            if (orderStatus == "Draft" || orderStatus == "Quotation")
            {
                 // Still insert items so we can retrieve them, but DON'T update stock
                 foreach (var item in items)
                 {
                    string sqlItem = "INSERT INTO order_items (order_id, part_id, quantity, price) VALUES (@oid, @pid, @qty, @price)";
                    DatabaseHelper.ExecuteNonQuery(sqlItem,
                        new SqlParameter("@oid", orderId),
                        new SqlParameter("@pid", item.PartId),
                        new SqlParameter("@qty", item.Quantity),
                        new SqlParameter("@price", item.UnitPrice)
                    );
                 }
                 return orderId;
            }

            // 3. Process Items & Update Stock (Normally)
            foreach (var item in items)
            {
                // Insert Order Item
                string sqlItem = "INSERT INTO order_items (order_id, part_id, quantity, price) VALUES (@oid, @pid, @qty, @price)";
                DatabaseHelper.ExecuteNonQuery(sqlItem,
                    new SqlParameter("@oid", orderId),
                    new SqlParameter("@pid", item.PartId),
                    new SqlParameter("@qty", item.Quantity),
                    new SqlParameter("@price", item.UnitPrice)
                );

                // Update Stock
                string sqlStock = "UPDATE parts SET quantity_in_stock = quantity_in_stock - @qty WHERE id = @pid";
                DatabaseHelper.ExecuteNonQuery(sqlStock,
                    new SqlParameter("@qty", item.Quantity),
                    new SqlParameter("@pid", item.PartId)
                );
            }

            // 4. Update Customer Balance (if unpaid)
            if (!isPaid && !isWalkIn)
            {
                string sqlBalance = "UPDATE customers SET current_balance = current_balance + @total";
                List<SqlParameter> parameters = new List<SqlParameter> {
                    new SqlParameter("@total", totalAmount),
                    new SqlParameter("@cid", customerId)
                };

                if (dueDate.HasValue)
                {
                    sqlBalance += ", payment_due_date = @dueDate";
                    parameters.Add(new SqlParameter("@dueDate", dueDate.Value));
                }

                sqlBalance += " WHERE customer_id = @cid";
                DatabaseHelper.ExecuteNonQuery(sqlBalance, parameters.ToArray());
            }

            // 5. Record in Customer History (if not Walk-in)
            if (!isWalkIn)
            {
                // Record the Sale (Payment Due)
                string sqlSaleRecord = "INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes, due_date) VALUES ('Customer', @cid, @amount, GETDATE(), @notes, @ddate)";
                DatabaseHelper.ExecuteNonQuery(sqlSaleRecord,
                    new SqlParameter("@cid", customerId),
                    new SqlParameter("@amount", totalAmount),
                    new SqlParameter("@notes", "[Sale] Order #" + orderId),
                    new SqlParameter("@ddate", (object)dueDate ?? DBNull.Value)
                );

                if (isPaid)
                {
                    // Record the Payment (Payment Received) - POS auto-payment
                    string sqlPayRecord = "INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes) VALUES ('Customer', @cid, @amount, GETDATE(), @notes)";
                    DatabaseHelper.ExecuteNonQuery(sqlPayRecord,
                        new SqlParameter("@cid", customerId),
                        new SqlParameter("@amount", totalAmount),
                        new SqlParameter("@notes", "[Payment] Order #" + orderId)
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
                SELECT o.order_id, o.order_date, ISNULL(c.full_name, 'Walk-in') as CustomerName, o.total_amount, o.customer_id
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
                SELECT o.order_id, o.order_date, ISNULL(c.full_name, 'Walk-in') as CustomerName, o.total_amount, o.customer_id
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
                    int currentStock = DatabaseHelper.ExecuteScalar<int>($"SELECT quantity_in_stock FROM parts WHERE id = {item.PartId}");
                    if (currentStock < item.Quantity)
                    {
                        string partName = DatabaseHelper.ExecuteScalar<string>($"SELECT part_name FROM parts WHERE id = {item.PartId}");
                        throw new Exception($"Insufficient stock for {partName}. Available: {currentStock}, Required: {item.Quantity}");
                    }
                }

                // 3. Update Status to Completed
                DatabaseHelper.ExecuteNonQuery("UPDATE orders SET status = 'Completed', order_date = GETDATE() WHERE order_id = @oid", 
                    new SqlParameter("@oid", orderId));

                // 4. Update Stock
                foreach (var item in items)
                {
                    DatabaseHelper.ExecuteNonQuery("UPDATE parts SET quantity_in_stock = quantity_in_stock - @qty WHERE id = @pid", 
                        new SqlParameter("@qty", item.Quantity), 
                        new SqlParameter("@pid", item.PartId));
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

        public List<OrderItem> GetOrderItems(int orderId)
        {
             string sql = @"
                SELECT i.part_id, p.part_name, i.quantity, i.price
                FROM order_items i
                JOIN parts p ON i.part_id = p.id
                WHERE i.order_id = @oid";
             
             return DatabaseHelper.ExecuteQuery(sql, reader => new OrderItem 
             {
                 PartId = reader.GetInt32(0),
                 PartName = reader.GetString(1),
                 Quantity = reader.GetInt32(2),
                 UnitPrice = reader.GetDecimal(3)
             }, new SqlParameter("@oid", orderId));
        }

        public void DeleteOrder(int orderId)
        {
             // Delete items first
             DatabaseHelper.ExecuteNonQuery($"DELETE FROM order_items WHERE order_id = {orderId}");
             DatabaseHelper.ExecuteNonQuery($"DELETE FROM orders WHERE order_id = {orderId}");
        }
    }
}
