using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using InventorySystem.Helpers;

namespace InventorySystem.Services
{
    public class ExpenseService
    {
        public void ProcessRecurringExpenses()
        {
            try
            {
                string currentMonth = DateTime.Now.ToString("yyyy-MM");
                
                // 1. Check if we've already processed recurring expenses for this month
                // We use a dummy record or just check if any record has this month as 'last_processed_month'
                int processedCount = DatabaseHelper.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM expenses WHERE last_processed_month = @month",
                    new SqliteParameter("@month", currentMonth));

                if (processedCount > 0) return; // Already processed

                // 2. Find recurring templates from PREVIOUS months
                // A template is any record where is_recurring = 1
                // We'll take the most recent recurring record for each category/description pair
                string sqlTemplates = @"
                    SELECT category, amount, description, recorded_by 
                    FROM expenses 
                    WHERE is_recurring = 1 
                    AND expense_id IN (SELECT MAX(expense_id) FROM expenses WHERE is_recurring = 1 GROUP BY category, description)";

                DataTable templates = DatabaseHelper.ExecuteDataTable(sqlTemplates);

                if (templates.Rows.Count > 0)
                {
                    foreach (DataRow row in templates.Rows)
                    {
                        // Create a new UNPAID record for the current month
                        string sqlInsert = @"
                            INSERT INTO expenses (category, expense_date, amount, description, recorded_by, is_paid, is_recurring, last_processed_month)
                            VALUES (@cat, @date, @amt, @desc, @usr, 0, 1, @month)";

                        DatabaseHelper.ExecuteNonQuery(sqlInsert,
                            new SqliteParameter("@cat", row["category"]),
                            new SqliteParameter("@date", DateTime.Now),
                            new SqliteParameter("@amt", row["amount"]),
                            new SqliteParameter("@desc", row["description"].ToString() + " (Auto-Generated)"),
                            new SqliteParameter("@usr", "System"),
                            new SqliteParameter("@month", currentMonth));
                    }
                }
                else
                {
                    // If no templates found, just mark the month as processed so we don't keep checking
                    // We can insert a hidden system record or just skip. 
                    // To be safe, we'll insert a zero-amount system marker if no templates exist.
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO expenses (category, expense_date, amount, description, recorded_by, is_paid, is_recurring, last_processed_month) VALUES ('System', datetime('now'), 0, 'Month Marker', 'System', 1, 0, @month)",
                        new SqliteParameter("@month", currentMonth));
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ProcessRecurringExpenses failed");
            }
        }

        public int GetUnpaidExpensesCount()
        {
            return DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM expenses WHERE is_paid = 0 AND date_deleted IS NULL");
        }

        public decimal GetTotalExpenses(int month, int year)
        {
            string sql = "SELECT SUM(amount) FROM expenses WHERE MONTH(expense_date) = @m AND YEAR(expense_date) = @y AND date_deleted IS NULL";
            object result = DatabaseHelper.ExecuteScalar<object>(sql, 
                new SqliteParameter("@m", month),
                new SqliteParameter("@y", year));
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        public bool MarkAsPaid(int expenseId)
        {
            return DatabaseHelper.ExecuteNonQuery("UPDATE expenses SET is_paid = 1 WHERE expense_id = @id",
                new SqliteParameter("@id", expenseId));
        }
    }
}
