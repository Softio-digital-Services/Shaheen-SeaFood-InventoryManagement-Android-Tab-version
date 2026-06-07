using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using InventorySystem.Helpers;

namespace InventorySystem.Services
{
    public class PurchaseService
    {
        public int CreatePurchaseOrder(int supplierId, List<PurchaseItemInfo> items, string notes)
        {
            try
            {
                decimal total = 0;
                foreach (var item in items) total += item.Quantity * item.CostPrice;

                string sql = "INSERT INTO purchase_orders (supplier_id, order_date, total_amount, status, notes) " +
                             "VALUES (@sid, datetime('now'), @total, 'Pending', @notes); SELECT last_insert_rowid();";
                
                object idObj = DatabaseHelper.ExecuteScalar<object>(sql,
                    new SqliteParameter("@sid", supplierId),
                    new SqliteParameter("@total", total),
                    new SqliteParameter("@notes", notes));

                int poId = Convert.ToInt32(idObj);

                foreach (var item in items)
                {
                    string itemSql = "INSERT INTO purchase_order_items (po_id, part_id, quantity, cost_price) VALUES (@poid, @pid, @qty, @cost)";
                    DatabaseHelper.ExecuteNonQuery(itemSql,
                        new SqliteParameter("@poid", poId),
                        new SqliteParameter("@pid", item.PartId),
                        new SqliteParameter("@qty", item.Quantity),
                        new SqliteParameter("@cost", item.CostPrice));
                }

                DatabaseHelper.LogTransaction("PO_CREATE", "PO #" + poId, "Total: " + total);
                return poId;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "PurchaseService.CreatePurchaseOrder");
                throw;
            }
        }

        public void MarkAsReceived(int poId)
        {
            try
            {
                // 1. Get status
                string status = DatabaseHelper.ExecuteScalar<string>("SELECT status FROM purchase_orders WHERE po_id = " + poId);
                if (status == "Received") throw new Exception("This purchase order has already been received.");

                // 2. Get items
                DataTable dtItems = DatabaseHelper.ExecuteDataTable("SELECT part_id, quantity, cost_price FROM purchase_order_items WHERE po_id = " + poId);

                // 3. Update stock and prices
                foreach (DataRow row in dtItems.Rows)
                {
                    int partId = (int)row["part_id"];
                    int qty = (int)row["quantity"];
                    decimal cost = (decimal)row["cost_price"];

                    // Update parts: stock and optionally update purchase_price
                    string sqlPart = "UPDATE parts SET quantity_in_stock = quantity_in_stock + @qty WHERE id = @pid";
                    DatabaseHelper.ExecuteNonQuery(sqlPart,
                        new SqliteParameter("@qty", qty),
                        new SqliteParameter("@pid", partId));

                    // Log movement
                    string sqlLog = "INSERT INTO stock_movements (part_id, movement_type, quantity, performed_by, notes, movement_date) " +
                                    "VALUES (@pid, 'IN', @qty, @user, @notes, datetime('now'))";
                    DatabaseHelper.ExecuteNonQuery(sqlLog,
                        new SqliteParameter("@pid", partId),
                        new SqliteParameter("@qty", qty),
                        new SqliteParameter("@user", UserSession.Username),
                        new SqliteParameter("@notes", "Received from PO #" + poId));
                }

                // 4. Update PO status
                DatabaseHelper.ExecuteNonQuery("UPDATE purchase_orders SET status = 'Received', received_date = datetime('now') WHERE po_id = " + poId);

                // 5. Update Supplier Balance
                DataTable poInfo = DatabaseHelper.ExecuteDataTable("SELECT supplier_id, total_amount FROM purchase_orders WHERE po_id = " + poId);
                if (poInfo.Rows.Count > 0)
                {
                    int supplierId = (int)poInfo.Rows[0]["supplier_id"];
                    decimal total = (decimal)poInfo.Rows[0]["total_amount"];

                    // Increase supplier debt/balance
                    DatabaseHelper.ExecuteNonQuery("UPDATE suppliers SET current_balance = current_balance + @total WHERE id = @sid",
                        new SqliteParameter("@total", total),
                        new SqliteParameter("@sid", supplierId));
                    
                    // Log to payments (as a negative/debt record)
                    string sqlLogPay = "INSERT INTO payments (entity_type, entity_id, amount, payment_date, notes) VALUES ('Supplier', @sid, @amount, datetime('now'), @notes)";
                    DatabaseHelper.ExecuteNonQuery(sqlLogPay,
                        new SqliteParameter("@sid", supplierId),
                        new SqliteParameter("@amount", -total), // Supplier balance increases (debt), but in payment ledger it's a liability
                        new SqliteParameter("@notes", "[PO] Stock Received #" + poId)
                    );
                }

                DatabaseHelper.LogTransaction("PO_RECEIVED", "PO #" + poId, "Stock updated.");
                GlobalEvents.RaiseInventoryUpdated();
                GlobalEvents.RaiseSuppliersUpdated();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "PurchaseService.MarkAsReceived");
                throw;
            }
        }

        public DataTable GetPurchaseOrders()
        {
            string sql = @"SELECT po.po_id, po.order_date, s.supplier_name, po.total_amount, po.status, po.received_date 
                           FROM purchase_orders po 
                           LEFT JOIN suppliers s ON po.supplier_id = s.id 
                           ORDER BY po.order_date DESC";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public List<PurchaseItemInfo> GetPOItems(int poId)
        {
            string sql = @"SELECT poi.part_id, p.part_name, poi.quantity, poi.cost_price 
                           FROM purchase_order_items poi 
                           JOIN parts p ON poi.part_id = p.id 
                           WHERE poi.po_id = " + poId;
            DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
            List<PurchaseItemInfo> list = new List<PurchaseItemInfo>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new PurchaseItemInfo { 
                    PartId = (int)r["part_id"], 
                    PartName = r["part_name"].ToString(),
                    Quantity = (int)r["quantity"], 
                    CostPrice = (decimal)r["cost_price"] 
                });
            }
            return list;
        }
    }

    public class PurchaseItemInfo
    {
        public int PartId { get; set; }
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
    }
}
