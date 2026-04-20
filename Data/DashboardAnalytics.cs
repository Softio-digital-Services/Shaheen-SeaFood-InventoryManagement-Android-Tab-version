using System;
using System.Data;
using System.Collections.Generic;

namespace GenericInventorySystem.Data
{
    public static class DashboardAnalytics
    {
        public static decimal GetMonthlySales()
        {
            // SQL: Sum total_amount from orders where month/year matches current
            string sql = @"
                SELECT ISNULL(SUM(total_amount), 0) 
                FROM orders 
                WHERE MONTH(order_date) = MONTH(GETDATE()) 
                AND YEAR(order_date) = YEAR(GETDATE())";
            
            return DatabaseHelper.ExecuteScalar<decimal>(sql);
        }

        public static DataTable GetLowStockItems()
        {
            // SQL: Standard low stock query
            string sql = @"
                SELECT TOP 10 part_name, quantity_in_stock 
                FROM parts 
                WHERE quantity_in_stock <= minimum_stock_level 
                AND date_deleted IS NULL 
                ORDER BY quantity_in_stock ASC";

            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public static DataTable GetTopSellingItems()
        {
            // SQL: Join order_items + parts, group by part
            string sql = @"
                SELECT TOP 5 p.part_name, SUM(oi.quantity) as total_sold
                FROM order_items oi
                JOIN parts p ON oi.part_id = p.id
                WHERE p.date_deleted IS NULL
                GROUP BY p.part_name
                ORDER BY total_sold DESC";

            return DatabaseHelper.ExecuteDataTable(sql);
        }
    }
}
