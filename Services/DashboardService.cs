using System;
using System.Data;
using System.Data.SqlClient;
using GenericInventorySystem.Helpers;
using System.Collections.Generic;

namespace GenericInventorySystem.Services
{
    public class Notification
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // "LowStock", "Order", "Info"
        public string Target { get; set; } // Form name or filter info
        public DateTime Timestamp { get; set; }
    }

    public class DashboardService
    {
        public decimal GetTotalInventoryValue()
        {
            return DatabaseHelper.ExecuteScalar<decimal>("SELECT ISNULL(SUM(selling_price * quantity_in_stock), 0) FROM parts");
        }

        public int GetTotalItems()
        {
            return DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM parts WHERE date_deleted IS NULL");
        }

        public int GetLowStockCount()
        {
            return DatabaseHelper.GetCount("parts", "quantity_in_stock <= minimum_stock_level AND date_deleted IS NULL");
        }

        public int GetOrdersCount(string scope = "Today")
        {
            if(scope == "Today")
                return DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM orders WHERE CAST(order_date AS DATE) = CAST(GETDATE() AS DATE)");
            return 0; // Extend if needed
        }

        public int GetPendingOrdersCount()
        {
             // Assuming this means "Unpaid" orders or generally active orders
             return DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM orders WHERE payment_status = 'Unpaid'");
        }

        public decimal GetSales(string scope = "Today")
        {
             if(scope == "Today")
                return DatabaseHelper.ExecuteScalar<decimal>("SELECT ISNULL(SUM(total_amount),0) FROM orders WHERE CAST(order_date AS DATE) = CAST(GETDATE() AS DATE)");
             return 0;
        }

        public DataTable GetTopSellingItems(int limit = 5)
        {
             string sql = $@"
                    SELECT TOP {limit} 
                        p.part_name, 
                        SUM(oi.quantity) as total_sold 
                    FROM order_items oi
                    INNER JOIN parts p ON oi.part_id = p.id
                    GROUP BY p.part_name
                    ORDER BY total_sold DESC";
             return DatabaseHelper.ExecuteDataTable(sql);
        }

        public Dictionary<string, int> GetStockDistribution()
        {
             string sql = @"
                    SELECT 
                        CASE 
                            WHEN quantity_in_stock = 0 THEN 'Out of Stock'
                            WHEN quantity_in_stock <= minimum_stock_level THEN 'Low Stock'
                            WHEN quantity_in_stock <= minimum_stock_level * 2 THEN 'Moderate Stock'
                            ELSE 'Well Stocked'
                        END as stock_level,
                        COUNT(*) as item_count
                    FROM parts
                    WHERE date_deleted IS NULL
                    GROUP BY 
                        CASE 
                            WHEN quantity_in_stock = 0 THEN 'Out of Stock'
                            WHEN quantity_in_stock <= minimum_stock_level THEN 'Low Stock'
                            WHEN quantity_in_stock <= minimum_stock_level * 2 THEN 'Moderate Stock'
                            ELSE 'Well Stocked'
                        END";
             DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
             Dictionary<string, int> data = new Dictionary<string, int>();
             foreach (DataRow row in dt.Rows)
             {
                 data[row["stock_level"].ToString()] = Convert.ToInt32(row["item_count"]);
             }
             return data;
        }

        public DataTable GetRecentActivity(int limit = 10)
        {
             return DatabaseHelper.ExecuteDataTable($"SELECT TOP {limit} action_type, description, timestamp FROM transactions ORDER BY timestamp DESC");
        }

        public Dictionary<string, decimal> GetWeeklyRevenue()
        {
            string sql = @"
                SELECT 
                    CAST(order_date AS DATE) as Date, 
                    SUM(total_amount) as Total 
                FROM orders 
                WHERE order_date >= DATEADD(day, -7, GETDATE()) 
                GROUP BY CAST(order_date AS DATE) 
                ORDER BY Date";
            DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
            Dictionary<string, decimal> data = new Dictionary<string, decimal>();
            
            // Fill in missing days if needed, but for now just return what we have
            foreach (DataRow row in dt.Rows)
            {
                 data[((DateTime)row["Date"]).ToString("dd/MM")] = Convert.ToDecimal(row["Total"]);
            }
            return data;
        }

        public Dictionary<string, int> GetMonthlySalesTrend()
        {
             string sql = @"
                SELECT 
                    CAST(order_date AS DATE) as Date, 
                    COUNT(*) as Count 
                FROM orders 
                WHERE order_date >= DATEADD(month, -1, GETDATE()) 
                GROUP BY CAST(order_date AS DATE) 
                ORDER BY Date";
            DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
             Dictionary<string, int> data = new Dictionary<string, int>();
            foreach (DataRow row in dt.Rows)
            {
                 data[((DateTime)row["Date"]).ToString("dd/MM")] = Convert.ToInt32(row["Count"]);
            }
            return data;
        }

        public DataTable GetSalesByCategory()
        {
            string sql = @"
                SELECT 
                    c.category_name, 
                    SUM(oi.quantity * oi.price) as total_sales
                FROM order_items oi
                INNER JOIN parts p ON oi.part_id = p.id
                INNER JOIN categories c ON p.category_id = c.id
                GROUP BY c.category_name
                ORDER BY total_sales DESC";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public Dictionary<string, decimal> GetMonthlyRevenue()
        {
            string sql = @"
                SELECT 
                    FORMAT(order_date, 'MMM') as Month,
                    SUM(total_amount) as Total,
                    MIN(order_date) as SortDate
                FROM orders 
                WHERE order_date >= DATEADD(month, -6, GETDATE())
                GROUP BY FORMAT(order_date, 'MMM')
                ORDER BY SortDate";
            DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
            Dictionary<string, decimal> data = new Dictionary<string, decimal>();
            foreach (DataRow row in dt.Rows)
            {
                data[row["Month"].ToString()] = Convert.ToDecimal(row["Total"]);
            }
            return data;
        }

        public decimal GetAverageOrderValue()
        {
            return DatabaseHelper.ExecuteScalar<decimal>("SELECT ISNULL(AVG(total_amount), 0) FROM orders");
        }

        public decimal GetTotalSalesYTD()
        {
            return DatabaseHelper.ExecuteScalar<decimal>("SELECT ISNULL(SUM(total_amount), 0) FROM orders WHERE YEAR(order_date) = YEAR(GETDATE())");
        }

        public List<Notification> GetNotifications()
        {
            List<Notification> notifications = new List<Notification>();

            // 1. Low Stock Notifications
            DataTable lowStockParts = DatabaseHelper.ExecuteDataTable(
                "SELECT TOP 5 part_name, quantity_in_stock, minimum_stock_level FROM parts WHERE quantity_in_stock <= minimum_stock_level AND date_deleted IS NULL");
            
            foreach (DataRow row in lowStockParts.Rows)
            {
                notifications.Add(new Notification {
                    Type = "LowStock",
                    Title = LocalizationManager.IsArabic ? "\u062A\u0646\u0628\u064A\u0647 \u0646\u0642\u0635 \u0627\u0644\u0645\u062E\u0632\u0648\u0646" : "Low Stock Alert",
                    Message = LocalizationManager.IsArabic ? $"{row["part_name"]} \u0642\u0627\u0631\u0628 \u0639\u0644\u0649 \u0627\u0644\u0646\u0641\u0627\u062F ({row["quantity_in_stock"]} \u0645\u062A\u0628\u0642\u064A)" : $"{row["part_name"]} is low ({row["quantity_in_stock"]} left)",
                    Target = "btnInventory",
                    Timestamp = DateTime.Now
                });
            }

            // 2. Recent Large Orders or Unpaid Orders
            DataTable recentOrders = DatabaseHelper.ExecuteDataTable(
                "SELECT TOP 3 o.order_id, c.full_name, o.total_amount, o.order_date FROM orders o LEFT JOIN customers c ON o.customer_id = c.customer_id WHERE o.order_date >= DATEADD(day, -1, GETDATE()) ORDER BY o.order_date DESC");

            foreach (DataRow row in recentOrders.Rows)
            {
                string val = CurrencyService.Format(Convert.ToDecimal(row["total_amount"]));
                notifications.Add(new Notification {
                    Type = "Order",
                    Title = LocalizationManager.IsArabic ? "\u0637\u0644\u0628 \u062D\u062F\u064A\u062B" : "Recent Order",
                    Message = LocalizationManager.IsArabic ? $"\u0637\u0644\u0628 #{row["order_id"]} \u0628\u0648\u0627\u0633\u0637\u0629 {row["full_name"] ?? "\u0639\u0645\u064A\u0644 \u0645\u0628\u0627\u0634\u0631"} - {val}" : $"Order #{row["order_id"]} by {row["full_name"] ?? "Walk-in"} - {val}",
                    Target = "btnHistory",
                    Timestamp = (DateTime)row["order_date"]
                });
            }

            return notifications;
        }
    }
}
