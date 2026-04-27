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

        public int GetPaymentRemindersCount()
        {
            int customerReminders = DatabaseHelper.ExecuteScalar<int>(
                @"SELECT COUNT(*) FROM customers 
                  WHERE date_deleted IS NULL AND payment_due_date IS NOT NULL AND current_balance > 0 
                  AND DATEDIFF(day, GETDATE(), payment_due_date) <= reminder_days");

            int supplierReminders = DatabaseHelper.ExecuteScalar<int>(
                @"SELECT COUNT(*) FROM suppliers 
                  WHERE date_deleted IS NULL AND payment_due_date IS NOT NULL AND balance_due > 0 
                  AND DATEDIFF(day, GETDATE(), payment_due_date) <= reminder_days");

            return customerReminders + supplierReminders;
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
             {
                decimal revenue = DatabaseHelper.ExecuteScalar<decimal>("SELECT ISNULL(SUM(total_amount),0) FROM orders WHERE CAST(order_date AS DATE) = CAST(GETDATE() AS DATE)");
                decimal expenses = DatabaseHelper.ExecuteScalar<decimal>("SELECT ISNULL(SUM(amount),0) FROM expenses WHERE CAST(expense_date AS DATE) = CAST(GETDATE() AS DATE) AND date_deleted IS NULL");
                return revenue - expenses;
             }
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
                SELECT Date, SUM(Total) as NetTotal FROM (
                    SELECT CAST(order_date AS DATE) as Date, SUM(total_amount) as Total FROM orders WHERE order_date >= DATEADD(day, -7, GETDATE()) GROUP BY CAST(order_date AS DATE)
                    UNION ALL
                    SELECT CAST(expense_date AS DATE) as Date, -SUM(amount) as Total FROM expenses WHERE expense_date >= DATEADD(day, -7, GETDATE()) AND date_deleted IS NULL GROUP BY CAST(expense_date AS DATE)
                ) t GROUP BY Date ORDER BY Date";
            
            DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
            Dictionary<string, decimal> data = new Dictionary<string, decimal>();
            foreach (DataRow row in dt.Rows)
            {
                 data[((DateTime)row["Date"]).ToString("dd/MM")] = Convert.ToDecimal(row["NetTotal"]);
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
                SELECT Month, SUM(Total) as NetTotal, MIN(SortDate) as SortDate FROM (
                    SELECT FORMAT(order_date, 'MMM') as Month, SUM(total_amount) as Total, MIN(order_date) as SortDate FROM orders WHERE order_date >= DATEADD(month, -6, GETDATE()) GROUP BY FORMAT(order_date, 'MMM')
                    UNION ALL
                    SELECT FORMAT(expense_date, 'MMM') as Month, -SUM(amount) as Total, MIN(expense_date) as SortDate FROM expenses WHERE expense_date >= DATEADD(month, -6, GETDATE()) AND date_deleted IS NULL GROUP BY FORMAT(expense_date, 'MMM')
                ) t GROUP BY Month ORDER BY SortDate";
            
            DataTable dt = DatabaseHelper.ExecuteDataTable(sql);
            Dictionary<string, decimal> data = new Dictionary<string, decimal>();
            foreach (DataRow row in dt.Rows)
            {
                data[row["Month"].ToString()] = Convert.ToDecimal(row["NetTotal"]);
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
                    Title = LocalizationManager.GetString("Notif_LowStock"),
                    Message = string.Format(LocalizationManager.GetString("Notif_LowStockMsg"), row["part_name"], row["quantity_in_stock"]),
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
                string customerName = row["full_name"]?.ToString() ?? LocalizationManager.GetString("Notif_WalkIn");
                notifications.Add(new Notification {
                    Type = "Order",
                    Title = LocalizationManager.GetString("Notif_RecentOrder"),
                    Message = string.Format(LocalizationManager.GetString("Notif_RecentOrderMsg"), row["order_id"], customerName, val),
                    Target = "btnHistory",
                    Timestamp = (DateTime)row["order_date"]
                });
            }

            // 3. Customer Payment Reminders
            DataTable customerReminders = DatabaseHelper.ExecuteDataTable(
                @"SELECT full_name, payment_due_date, current_balance, reminder_days 
                  FROM customers 
                  WHERE date_deleted IS NULL 
                  AND payment_due_date IS NOT NULL 
                  AND current_balance > 0
                  AND DATEDIFF(day, GETDATE(), payment_due_date) <= reminder_days");

            foreach (DataRow row in customerReminders.Rows)
            {
                DateTime dueDate = (DateTime)row["payment_due_date"];
                int diff = (dueDate.Date - DateTime.Today).Days;
                string statusMsg = diff < 0 ? LocalizationManager.GetString("Notif_Overdue") : string.Format(LocalizationManager.GetString("Notif_DueIn"), diff);
                
                notifications.Add(new Notification {
                    Type = diff < 0 ? "Alert" : "Info",
                    Title = LocalizationManager.GetString("Notif_CustomerPaymentDue"),
                    Message = $"{row["full_name"]} - {statusMsg} ({CurrencyService.Format(Convert.ToDecimal(row["current_balance"]))})",
                    Target = "btnCustomers",
                    Timestamp = dueDate
                });
            }

            // 4. Supplier Payment Reminders
            DataTable supplierReminders = DatabaseHelper.ExecuteDataTable(
                @"SELECT supplier_name, payment_due_date, balance_due, reminder_days 
                  FROM suppliers 
                  WHERE date_deleted IS NULL 
                  AND payment_due_date IS NOT NULL 
                  AND balance_due > 0
                  AND DATEDIFF(day, GETDATE(), payment_due_date) <= reminder_days");

            foreach (DataRow row in supplierReminders.Rows)
            {
                DateTime dueDate = (DateTime)row["payment_due_date"];
                int diff = (dueDate.Date - DateTime.Today).Days;
                string statusMsg = diff < 0 ? LocalizationManager.GetString("Notif_Overdue") : string.Format(LocalizationManager.GetString("Notif_DueIn"), diff);

                notifications.Add(new Notification {
                    Type = diff < 0 ? "Alert" : "Info",
                    Title = LocalizationManager.GetString("Notif_SupplierPaymentDue"),
                    Message = $"{row["supplier_name"]} - {statusMsg} ({CurrencyService.Format(Convert.ToDecimal(row["balance_due"]))})",
                    Target = "btnSuppliers",
                    Timestamp = dueDate
                });
            }

            // 5. Unpaid Expenses
            int unpaidCount = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM expenses WHERE is_paid = 0 AND date_deleted IS NULL");
            if (unpaidCount > 0)
            {
                notifications.Add(new Notification {
                    Type = "Alert",
                    Title = LocalizationManager.GetString("Notif_UnpaidExpenses"),
                    Message = string.Format(LocalizationManager.GetString("Notif_UnpaidExpensesMsg"), unpaidCount),
                    Target = "btnMonthlyExpenses",
                    Timestamp = DateTime.Now
                });
            }

            // 6. Monthly Expense Report (Every 25th)
            if (DateTime.Now.Day >= 25)
            {
                decimal total = new ExpenseService().GetTotalExpenses(DateTime.Now.Month, DateTime.Now.Year);
                if (total > 0)
                {
                    notifications.Add(new Notification {
                        Type = "Info",
                        Title = LocalizationManager.GetString("Notif_MonthlyExpensesReady"),
                        Message = string.Format(LocalizationManager.GetString("Notif_MonthlyExpensesReadyMsg"), CurrencyService.Format(total)),
                        Target = "btnMonthlyExpenses",
                        Timestamp = DateTime.Now
                    });
                }
            }

            return notifications;
        }
    }
}
