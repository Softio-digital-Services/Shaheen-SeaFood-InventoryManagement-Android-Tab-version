using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem
{
    /// <summary>
    /// Centralized database operations helper -- SQLite backend.
    /// Drop-in replacement for the previous SQL Server version.
    /// </summary>
    public static class DatabaseHelper
    {
        // - helpers -

        private static SqliteConnection OpenConnection()
        {
            var conn = new SqliteConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            return conn;
        }

        private static void AddParams(SqliteCommand cmd, SqliteParameter[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
        }

        // - public API -

        public static List<T> ExecuteQuery<T>(string sql, Func<SqliteDataReader, T> map, params SqliteParameter[] parameters)
        {
            var results = new List<T>();
            try
            {
                using var conn = OpenConnection();
                using var cmd  = new SqliteCommand(sql, conn);
                AddParams(cmd, parameters);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    results.Add(map(reader));
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ExecuteQuery: {sql}");
                throw;
            }
            return results;
        }

        public static bool ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
        {
            try
            {
                using var conn = OpenConnection();
                using var cmd  = new SqliteCommand(sql, conn);
                AddParams(cmd, parameters);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ExecuteNonQuery: {sql}");
                return false;
            }
        }

        public static T ExecuteScalar<T>(string sql, params SqliteParameter[] parameters)
        {
            try
            {
                using var conn = OpenConnection();
                using var cmd  = new SqliteCommand(sql, conn);
                AddParams(cmd, parameters);
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    return default(T);
                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ExecuteScalar: {sql}");
                return default(T);
            }
        }

        public static bool RecordExists(string tableName, string columnName, string value, bool excludeDeleted = true)
        {
            string del = excludeDeleted ? " AND date_deleted IS NULL" : "";
            string sql = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName} = @value{del}";
            return ExecuteScalar<int>(sql, new SqliteParameter("@value", value)) > 0;
        }

        public static int GetCount(string tableName, string whereClause = "", params SqliteParameter[] parameters)
        {
            string sql = $"SELECT COUNT(*) FROM {tableName}";
            if (!string.IsNullOrWhiteSpace(whereClause))
                sql += $" WHERE {whereClause}";
            return ExecuteScalar<int>(sql, parameters);
        }

        /// <summary>
        /// Fills a DataTable from a query -- replaces SqlDataAdapter for SQLite.
        /// </summary>
        public static DataTable ExecuteDataTable(string sql, params SqliteParameter[] parameters)
        {
            var dt = new DataTable();
            try
            {
                using var conn   = OpenConnection();
                using var cmd    = new SqliteCommand(sql, conn);
                AddParams(cmd, parameters);
                using var reader = cmd.ExecuteReader();
                // Load schema + data
                dt.Load(reader);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ExecuteDataTable: {sql}");
                throw;
            }
            return dt;
        }

        public static void LogTransaction(string action, string partName, string description)
        {
            try
            {
                string sql = "INSERT INTO transactions (action_type, part_name, description, username, timestamp) " +
                             "VALUES (@action, @part, @desc, @user, datetime('now'))";
                ExecuteNonQuery(sql,
                    new SqliteParameter("@action", action),
                    new SqliteParameter("@part",   partName),
                    new SqliteParameter("@desc",   description),
                    new SqliteParameter("@user",   "Admin"));
            }
            catch { }
        }

        /// <summary>
        /// Ensures all required tables and columns exist (SQLite-compatible).
        /// Uses CREATE TABLE IF NOT EXISTS + PRAGMA table_info for column checks.
        /// </summary>
        public static void EnsureSchema()
        {
            try
            {
                // Tables -- SQLite CREATE IF NOT EXISTS is idempotent
                string sqlTables = @"
                    CREATE TABLE IF NOT EXISTS categories (
                        id              INTEGER PRIMARY KEY AUTOINCREMENT,
                        category_name   TEXT NOT NULL UNIQUE,
                        description     TEXT,
                        category_image  TEXT,
                        date_created    TEXT DEFAULT (datetime('now'))
                    );

                    CREATE TABLE IF NOT EXISTS suppliers (
                        id              INTEGER PRIMARY KEY AUTOINCREMENT,
                        supplier_code   TEXT,
                        supplier_name   TEXT NOT NULL,
                        contact_person  TEXT,
                        email           TEXT,
                        phone           TEXT,
                        address         TEXT,
                        type            TEXT,
                        balance_due     REAL DEFAULT 0,
                        payment_due_date TEXT,
                        reminder_days   INTEGER DEFAULT 0,
                        status          TEXT DEFAULT 'Active',
                        date_added      TEXT DEFAULT (datetime('now')),
                        date_deleted    TEXT
                    );

                    CREATE TABLE IF NOT EXISTS customers (
                        customer_id     INTEGER PRIMARY KEY AUTOINCREMENT,
                        full_name       TEXT NOT NULL,
                        phone           TEXT,
                        email           TEXT,
                        address         TEXT,
                        type            TEXT,
                        current_balance REAL DEFAULT 0,
                        credit_limit    REAL DEFAULT 1000,
                        payment_due_date TEXT,
                        reminder_days   INTEGER DEFAULT 0,
                        status          TEXT DEFAULT 'Active',
                        date_added      TEXT DEFAULT (datetime('now')),
                        date_deleted    TEXT
                    );

                    CREATE TABLE IF NOT EXISTS parts (
                        id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                        part_number         TEXT,
                        part_name           TEXT NOT NULL,
                        description         TEXT,
                        category_id         INTEGER,
                        supplier_id         INTEGER,
                        purchase_price      REAL DEFAULT 0,
                        selling_price       REAL DEFAULT 0,
                        quantity_in_stock   INTEGER DEFAULT 0,
                        minimum_stock_level INTEGER DEFAULT 5,
                        reorder_quantity    INTEGER DEFAULT 10,
                        location            TEXT,
                        shelf               TEXT,
                        part_image          TEXT,
                        barcode             TEXT,
                        status              TEXT DEFAULT 'Active',
                        date_added          TEXT DEFAULT (datetime('now')),
                        date_deleted        TEXT
                    );

                    CREATE TABLE IF NOT EXISTS transactions (
                        id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        action_type TEXT,
                        part_name   TEXT,
                        description TEXT,
                        username    TEXT,
                        timestamp   TEXT DEFAULT (datetime('now'))
                    );

                    CREATE TABLE IF NOT EXISTS users (
                        id           INTEGER PRIMARY KEY AUTOINCREMENT,
                        username     TEXT NOT NULL UNIQUE,
                        password     TEXT NOT NULL,
                        full_name    TEXT,
                        role         TEXT DEFAULT 'User',
                        is_active    INTEGER DEFAULT 1,
                        date_created TEXT DEFAULT (datetime('now'))
                    );

                    CREATE TABLE IF NOT EXISTS orders (
                        order_id        INTEGER PRIMARY KEY AUTOINCREMENT,
                        customer_id     INTEGER,
                        order_date      TEXT DEFAULT (datetime('now')),
                        total_amount    REAL DEFAULT 0,
                        status          TEXT DEFAULT 'Completed',
                        payment_status  TEXT DEFAULT 'Paid',
                        amount_paid     REAL DEFAULT 0,
                        payment_method  TEXT DEFAULT 'Cash'
                    );

                    CREATE TABLE IF NOT EXISTS order_items (
                        order_item_id   INTEGER PRIMARY KEY AUTOINCREMENT,
                        order_id        INTEGER,
                        part_id         INTEGER,
                        quantity        INTEGER,
                        price           REAL
                    );

                    CREATE TABLE IF NOT EXISTS payments (
                        payment_id      INTEGER PRIMARY KEY AUTOINCREMENT,
                        entity_type     TEXT,
                        entity_id       INTEGER,
                        amount          REAL,
                        payment_date    TEXT DEFAULT (datetime('now')),
                        notes           TEXT,
                        due_date        TEXT
                    );

                    CREATE TABLE IF NOT EXISTS purchase_orders (
                        po_id           INTEGER PRIMARY KEY AUTOINCREMENT,
                        supplier_id     INTEGER,
                        order_date      TEXT DEFAULT (datetime('now')),
                        total_amount    REAL DEFAULT 0,
                        status          TEXT DEFAULT 'Pending',
                        received_date   TEXT,
                        notes           TEXT
                    );

                    CREATE TABLE IF NOT EXISTS purchase_order_items (
                        po_item_id      INTEGER PRIMARY KEY AUTOINCREMENT,
                        po_id           INTEGER,
                        part_id         INTEGER,
                        quantity        INTEGER,
                        cost_price      REAL
                    );

                    CREATE TABLE IF NOT EXISTS returns (
                        return_id       INTEGER PRIMARY KEY AUTOINCREMENT,
                        order_id        INTEGER,
                        return_date     TEXT DEFAULT (datetime('now')),
                        total_refund    REAL DEFAULT 0,
                        reason          TEXT,
                        performed_by    TEXT
                    );

                    CREATE TABLE IF NOT EXISTS return_items (
                        return_item_id  INTEGER PRIMARY KEY AUTOINCREMENT,
                        return_id       INTEGER,
                        part_id         INTEGER,
                        quantity        INTEGER,
                        refund_amount   REAL
                    );

                    CREATE TABLE IF NOT EXISTS expenses (
                        expense_id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        expense_date        TEXT DEFAULT (datetime('now')),
                        category            TEXT,
                        amount              REAL DEFAULT 0,
                        description         TEXT,
                        recorded_by         TEXT,
                        is_paid             INTEGER DEFAULT 1,
                        is_recurring        INTEGER DEFAULT 0,
                        last_processed_month TEXT,
                        date_deleted        TEXT
                    );
                ";

                // SQLite doesn't support multiple statements in one call -- split them
                foreach (var stmt in sqlTables.Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = stmt.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        ExecuteNonQuery(trimmed + ";");
                }

                // --- MIGRATIONS ---
                // Add due_date to payments if missing
                if (!ColumnExists("payments", "due_date"))
                {
                    ExecuteNonQuery("ALTER TABLE payments ADD COLUMN due_date TEXT;");
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "EnsureSchema failed");
            }
        }

        private static bool ColumnExists(string tableName, string columnName)
        {
            try
            {
                DataTable dt = ExecuteDataTable($"PRAGMA table_info({tableName})");
                foreach (DataRow row in dt.Rows)
                {
                    if (row["name"].ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch { }
            return false;
        }
    }
}
