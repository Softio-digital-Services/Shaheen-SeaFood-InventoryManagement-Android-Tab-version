using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using Shaheen_InventoryManagement_Android.Helpers;

namespace Shaheen_InventoryManagement_Android
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
                using var cmd = new SqliteCommand(sql, conn);
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
                using var cmd = new SqliteCommand(sql, conn);
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
                using var cmd = new SqliteCommand(sql, conn);
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
                using var conn = OpenConnection();
                using var cmd = new SqliteCommand(sql, conn);
                AddParams(cmd, parameters);
                using var reader = cmd.ExecuteReader();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Type t = reader.GetFieldType(i) ?? typeof(object);
                    dt.Columns.Add(reader.GetName(i), t);
                }

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        object val = reader.GetValue(i);
                        if (val == DBNull.Value)
                        {
                            row[i] = DBNull.Value;
                        }
                        else
                        {
                            Type targetType = dt.Columns[i].DataType;
                            if (val.GetType() == targetType || targetType == typeof(object))
                            {
                                row[i] = val;
                            }
                            else
                            {
                                try
                                {
                                    if (val is string s && string.IsNullOrWhiteSpace(s))
                                    {
                                        row[i] = targetType.IsValueType ? Activator.CreateInstance(targetType) : DBNull.Value;
                                    }
                                    else
                                    {
                                        row[i] = Convert.ChangeType(val, targetType);
                                    }
                                }
                                catch
                                {
                                    row[i] = targetType.IsValueType ? Activator.CreateInstance(targetType) : DBNull.Value;
                                }
                            }
                        }
                    }
                    dt.Rows.Add(row);
                }
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
                    new SqliteParameter("@part", partName),
                    new SqliteParameter("@desc", description),
                    new SqliteParameter("@user", "Admin"));
            }
            catch { }
        }

        public static void LogUserAction(string username, string fullName, string action)
        {
            try
            {
                string sql = "INSERT INTO user_logs (username, full_name, action, timestamp, machine_name) " +
                             "VALUES (@username, @fullName, @action, datetime('now'), @machine)";
                ExecuteNonQuery(sql,
                    new SqliteParameter("@username", username ?? "Guest"),
                    new SqliteParameter("@fullName", fullName ?? "Guest User"),
                    new SqliteParameter("@action", action),
                    new SqliteParameter("@machine", Environment.MachineName));
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "LogUserAction failed");
            }
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
                        item_no             TEXT,
                        part_name           TEXT NOT NULL,
                        description         TEXT,
                        category_id         INTEGER,
                        supplier_id         INTEGER,
                        purchase_price      REAL DEFAULT 0,
                        selling_price       REAL DEFAULT 0,
                        quantity_in_stock   REAL DEFAULT 0,
                        minimum_stock_level REAL DEFAULT 5,
                        reorder_quantity    INTEGER DEFAULT 10,
                        location            TEXT,
                        shelf               TEXT,
                        part_image          TEXT,
                        barcode             TEXT,
                        status              TEXT DEFAULT 'Active',
                        date_added          TEXT DEFAULT (datetime('now')),
                        date_deleted        TEXT,
                        item_type           TEXT DEFAULT 'Product',
                        unit_of_measure     TEXT,
                        batch_number        TEXT,
                        expiry_date         TEXT,
                        is_sales_item       INTEGER DEFAULT 1,
                        is_purchase_item    INTEGER DEFAULT 0,
                        is_inactive         INTEGER DEFAULT 0,
                        tax_rate            REAL DEFAULT 0,
                        is_stock_tracked    INTEGER DEFAULT 1,
                        price2              REAL DEFAULT 0,
                        price3              REAL DEFAULT 0,
                        price4              REAL DEFAULT 0,
                        big_unit            TEXT,
                        small_unit          TEXT,
                        conversion_value    REAL DEFAULT 1,
                        pack_size           REAL DEFAULT 1
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

                    CREATE TABLE IF NOT EXISTS user_logs (
                        id           INTEGER PRIMARY KEY AUTOINCREMENT,
                        username     TEXT NOT NULL,
                        full_name    TEXT,
                        action       TEXT NOT NULL,
                        timestamp    TEXT DEFAULT (datetime('now')),
                        machine_name TEXT
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
                        price           REAL,
                        item_type       TEXT DEFAULT 'Part',
                        recipe_id       INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS recipes (
                        id              INTEGER PRIMARY KEY AUTOINCREMENT,
                        recipe_name     TEXT NOT NULL UNIQUE,
                        item_no         TEXT,
                        description     TEXT,
                        selling_price   REAL DEFAULT 0,
                        status          TEXT DEFAULT 'Active',
                        date_added      TEXT DEFAULT (datetime('now')),
                        date_deleted    TEXT,
                        recipe_image    TEXT,
                        category_id     INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS recipe_parts (
                        id              INTEGER PRIMARY KEY AUTOINCREMENT,
                        recipe_id       INTEGER NOT NULL,
                        part_id         INTEGER NOT NULL,
                        quantity        REAL DEFAULT 1
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

                    CREATE TABLE IF NOT EXISTS expense_categories (
                        category_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        category_name TEXT NOT NULL UNIQUE
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
                // Add new unified product/service fields to parts
                if (!ColumnExists("parts", "item_type")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN item_type TEXT DEFAULT 'Product';");
                if (!ColumnExists("parts", "unit_of_measure")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN unit_of_measure TEXT;");
                if (!ColumnExists("parts", "batch_number")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN batch_number TEXT;");
                if (!ColumnExists("parts", "expiry_date")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN expiry_date TEXT;");
                if (!ColumnExists("parts", "is_sales_item")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN is_sales_item INTEGER DEFAULT 1;");
                if (!ColumnExists("parts", "is_purchase_item")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN is_purchase_item INTEGER DEFAULT 0;");
                if (!ColumnExists("parts", "is_inactive")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN is_inactive INTEGER DEFAULT 0;");
                if (!ColumnExists("parts", "tax_rate")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN tax_rate REAL DEFAULT 0;");
                if (!ColumnExists("parts", "is_stock_tracked")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN is_stock_tracked INTEGER DEFAULT 1;");
                if (!ColumnExists("parts", "price2")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN price2 REAL DEFAULT 0;");
                if (!ColumnExists("parts", "price3")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN price3 REAL DEFAULT 0;");
                if (!ColumnExists("parts", "price4")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN price4 REAL DEFAULT 0;");
                if (!ColumnExists("recipe_parts", "unit_of_measure")) ExecuteNonQuery("ALTER TABLE recipe_parts ADD COLUMN unit_of_measure TEXT;");

                // Add due_date to payments if missing
                if (!ColumnExists("payments", "due_date"))
                {
                    ExecuteNonQuery("ALTER TABLE payments ADD COLUMN due_date TEXT;");
                }

                // Add shipping fields to orders
                if (!ColumnExists("orders", "shipping_address")) ExecuteNonQuery("ALTER TABLE orders ADD COLUMN shipping_address TEXT;");
                if (!ColumnExists("orders", "delivery_date")) ExecuteNonQuery("ALTER TABLE orders ADD COLUMN delivery_date TEXT;");
                if (!ColumnExists("orders", "due_date")) ExecuteNonQuery("ALTER TABLE orders ADD COLUMN due_date TEXT;");

                // Add recipe fields to order_items if missing
                if (!ColumnExists("order_items", "item_type")) ExecuteNonQuery("ALTER TABLE order_items ADD COLUMN item_type TEXT DEFAULT 'Part';");
                if (!ColumnExists("order_items", "recipe_id")) ExecuteNonQuery("ALTER TABLE order_items ADD COLUMN recipe_id INTEGER;");
                if (!ColumnExists("order_items", "unit_of_measure")) ExecuteNonQuery("ALTER TABLE order_items ADD COLUMN unit_of_measure TEXT;");

                 // Add category_id to recipes if missing
                if (!ColumnExists("recipes", "category_id")) ExecuteNonQuery("ALTER TABLE recipes ADD COLUMN category_id INTEGER;");

                // Add item_no migrations
                if (!ColumnExists("parts", "item_no")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN item_no TEXT;");
                if (!ColumnExists("recipes", "item_no")) ExecuteNonQuery("ALTER TABLE recipes ADD COLUMN item_no TEXT;");

                // Add stock type and pack/piece columns to parts
                if (!ColumnExists("parts", "stock_type")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN stock_type TEXT DEFAULT 'Piece';");
                if (!ColumnExists("parts", "pack_items_number")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN pack_items_number INTEGER DEFAULT 0;");
                if (!ColumnExists("parts", "pack_price")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN pack_price REAL DEFAULT 0;");
                if (!ColumnExists("parts", "item_price")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN item_price REAL DEFAULT 0;");
                if (!ColumnExists("parts", "piece_price")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN piece_price REAL DEFAULT 0;");
                if (!ColumnExists("parts", "big_unit")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN big_unit TEXT;");
                if (!ColumnExists("parts", "small_unit")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN small_unit TEXT;");
                if (!ColumnExists("parts", "conversion_value")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN conversion_value REAL DEFAULT 1;");
                if (!ColumnExists("parts", "pack_size")) ExecuteNonQuery("ALTER TABLE parts ADD COLUMN pack_size REAL DEFAULT 1;");
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
