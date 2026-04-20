using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Services
{
    public class ReturnService
    {
        public void ProcessReturn(int orderId, List<ReturnItemInfo> itemsToReturn, string reason)
        {
            try
            {
                // 1. Calculate total refund
                decimal totalRefund = 0;
                foreach (var item in itemsToReturn)
                {
                    totalRefund += item.RefundAmount;
                }

                // 2. Insert Return Record
                string sqlReturn = "INSERT INTO returns (order_id, return_date, total_refund, reason, performed_by) " +
                                  "VALUES (@oid, GETDATE(), @refund, @reason, @user); SELECT SCOPE_IDENTITY();";
                
                object returnIdObj = DatabaseHelper.ExecuteScalar<object>(sqlReturn,
                    new SqlParameter("@oid", orderId),
                    new SqlParameter("@refund", totalRefund),
                    new SqlParameter("@reason", reason),
                    new SqlParameter("@user", UserSession.Username));
                
                int returnId = Convert.ToInt32(returnIdObj);

                // 3. Process each item
                foreach (var item in itemsToReturn)
                {
                    // Insert return item record
                    string sqlItem = "INSERT INTO return_items (return_id, part_id, quantity, refund_amount) " +
                                     "VALUES (@rid, @pid, @qty, @refund)";
                    DatabaseHelper.ExecuteNonQuery(sqlItem,
                        new SqlParameter("@rid", returnId),
                        new SqlParameter("@pid", item.PartId),
                        new SqlParameter("@qty", item.Quantity),
                        new SqlParameter("@refund", item.RefundAmount));

                    // Update stock
                    string sqlStock = "UPDATE parts SET quantity_in_stock = quantity_in_stock + @qty WHERE id = @pid";
                    DatabaseHelper.ExecuteNonQuery(sqlStock,
                        new SqlParameter("@qty", item.Quantity),
                        new SqlParameter("@pid", item.PartId));

                    // Log stock movement
                    string sqlLog = "INSERT INTO stock_movements (part_id, movement_type, quantity, performed_by, notes, movement_date) " +
                                    "VALUES (@pid, 'RETURN', @qty, @user, @notes, GETDATE())";
                    DatabaseHelper.ExecuteNonQuery(sqlLog,
                        new SqlParameter("@pid", item.PartId),
                        new SqlParameter("@qty", item.Quantity),
                        new SqlParameter("@user", UserSession.Username),
                        new SqlParameter("@notes", "Returned from Order #" + orderId));
                }

                // 4. Update Customer Balance if applicable
                // First get customer_id from order
                object cidObj = DatabaseHelper.ExecuteScalar<object>("SELECT customer_id FROM orders WHERE order_id = " + orderId);
                if (cidObj != null && cidObj != DBNull.Value)
                {
                    int customerId = Convert.ToInt32(cidObj);
                    if (customerId > 0)
                    {
                        // Reduce customer balance
                        string sqlBalance = "UPDATE customers SET current_balance = current_balance - @refund WHERE customer_id = @cid";
                        DatabaseHelper.ExecuteNonQuery(sqlBalance,
                            new SqlParameter("@refund", totalRefund),
                            new SqlParameter("@cid", customerId));

                        // Record payment record (negative payment/credit note)
                        string sqlPayRecord = "INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes) VALUES ('Customer', @cid, @amount, GETDATE(), @notes)";
                        DatabaseHelper.ExecuteNonQuery(sqlPayRecord,
                            new SqlParameter("@cid", customerId),
                            new SqlParameter("@amount", -totalRefund),
                            new SqlParameter("@notes", "[Return] Refund for Order #" + orderId)
                        );
                    }
                }

                // 5. Update Order Status if fully returned (optional logic)
                // For now just log it
                DatabaseHelper.LogTransaction("RETURN", "Order #" + orderId, "Refund Total: " + totalRefund);
                
                GlobalEvents.RaiseOrdersUpdated();
                GlobalEvents.RaiseInventoryUpdated();
                GlobalEvents.RaiseCustomersUpdated();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ReturnService.ProcessReturn");
                throw;
            }
        }
    }

    public class ReturnItemInfo
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public decimal RefundAmount { get; set; }
    }
}
