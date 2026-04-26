using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem
{
    /// <summary>
    /// Centralized database operations helper
    /// Provides generic, reusable database methods with automatic connection management
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        /// Execute a query and return a list of objects
        /// </summary>
        /// <typeparam name="T">Type of object to return</typeparam>
        /// <param name="sql">SQL query to execute</param>
        /// <param name="mapFunction">Function to map SqlDataReader row to object</param>
        /// <param name="parameters">Optional SQL parameters</param>
        /// <returns>List of objects</returns>
        public static List<T> ExecuteQuery<T>(string sql, Func<SqlDataReader, T> mapFunction, params SqlParameter[] parameters)
        {
            var results = new List<T>();
            
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null && parameters.Length > 0)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        
                        connection.Open();
                        
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(mapFunction(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ExecuteQuery: {sql}");
                throw; // Re-throw to allow caller to handle
            }
            
            return results;
        }

        /// <summary>
        /// Execute a non-query command (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="sql">SQL command to execute</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null && parameters.Length > 0)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        
                        connection.Open();
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ExecuteNonQuery: {sql}");
                return false;
            }
        }

        /// <summary>
        /// Execute a scalar query (returns single value like COUNT, MAX, etc.)
        /// </summary>
        /// <typeparam name="T">Type of value to return</typeparam>
        /// <param name="sql">SQL query to execute</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns>The scalar value</returns>
        public static T ExecuteScalar<T>(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null && parameters.Length > 0)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        
                        connection.Open();
                        object result = command.ExecuteScalar();
                        
                        if (result == null || result == DBNull.Value)
                        {
                            return default(T);
                        }
                        
                        return (T)Convert.ChangeType(result, typeof(T));
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ExecuteScalar: {sql}");
                System.Diagnostics.Debug.WriteLine($"DB ERROR: {ex.Message} in {sql}");
                return default(T);
            }
        }

        /// <summary>
        /// Check if a record exists in the database
        /// </summary>
        /// <param name="tableName">Table name to check</param>
        /// <param name="columnName">Column name to check</param>
        /// <param name="value">Value to search for</param>
        /// <param name="excludeDeleted">Whether to exclude soft-deleted records (checks date_delete IS NULL)</param>
        /// <returns>True if record exists, false otherwise</returns>
        public static bool RecordExists(string tableName, string columnName, string value, bool excludeDeleted = true)
        {
            string deleteClause = excludeDeleted ? " AND date_delete IS NULL" : "";
            string sql = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName} = @value{deleteClause}";
            
            int count = ExecuteScalar<int>(sql, new SqlParameter("@value", value));
            return count > 0;
        }

        /// <summary>
        /// Get count of records matching criteria
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="whereClause">WHERE clause (without the WHERE keyword)</param>
        /// <param name="parameters">SQL parameters for the WHERE clause</param>
        /// <returns>Count of matching records</returns>
        public static int GetCount(string tableName, string whereClause = "", params SqlParameter[] parameters)
        {
            string sql = $"SELECT COUNT(*) FROM {tableName}";
            if (!string.IsNullOrWhiteSpace(whereClause))
            {
                sql += $" WHERE {whereClause}";
            }
            
            return ExecuteScalar<int>(sql, parameters);
        }

        /// <summary>
        /// Execute a query and fill a DataTable (for DataGridView binding)
        /// </summary>
        /// <param name="sql">SQL query</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns>DataTable with results</returns>
        public static DataTable ExecuteDataTable(string sql, params SqlParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null && parameters.Length > 0)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ExecuteDataTable: {sql}");
                throw; // Re-throw so caller's try/catch handles it — never show a popup from here
            }
            
            return dataTable;
        }
        /// <summary>
        /// Log a user action to the database
        /// </summary>
        public static void LogTransaction(string action, string partName, string description)
        {
            try {
                string sql = "INSERT INTO transactions (index_action, part_name, description, username, date_added) VALUES (@action, @part, @desc, @user, GETDATE())";
                ExecuteNonQuery(sql, 
                    new SqlParameter("@action", action),
                    new SqlParameter("@part", partName),
                    new SqlParameter("@desc", description),
                    new SqlParameter("@user", "Admin"));
            } catch { }
        }

        /// <summary>
        /// Ensures all required tables and columns exist
        /// </summary>
        public static void EnsureSchema()
        {
            try
            {
                // 0. Base Tables for Car Parts System
                string sqlBase = @"
                    IF OBJECT_ID('categories', 'U') IS NULL
                    BEGIN
                        CREATE TABLE categories (
                            id INT IDENTITY(1,1) PRIMARY KEY,
                            category_name NVARCHAR(100) NOT NULL UNIQUE,
                            description NVARCHAR(500) NULL,
                            date_created DATETIME DEFAULT GETDATE()
                        );
                        INSERT INTO categories (category_name, description) VALUES ('General', 'General category');
                    END

                    IF OBJECT_ID('suppliers', 'U') IS NULL
                    BEGIN
                        CREATE TABLE suppliers (
                            id INT IDENTITY(1,1) PRIMARY KEY,
                            supplier_code NVARCHAR(50) NOT NULL UNIQUE,
                            supplier_name NVARCHAR(200) NOT NULL,
                            contact_person NVARCHAR(100) NULL,
                            email NVARCHAR(100) NULL,
                            phone NVARCHAR(20) NULL,
                            address NVARCHAR(500) NULL,
                            status NVARCHAR(20) NOT NULL DEFAULT 'Active',
                            date_added DATETIME DEFAULT GETDATE(),
                            date_deleted DATETIME NULL
                        );
                    END

                    IF OBJECT_ID('parts', 'U') IS NULL
                    BEGIN
                        CREATE TABLE parts (
                            id INT IDENTITY(1,1) PRIMARY KEY,
                            part_number NVARCHAR(50) NOT NULL UNIQUE,
                            part_name NVARCHAR(200) NOT NULL,
                            description NVARCHAR(MAX) NULL,
                            category_id INT NOT NULL,
                            supplier_id INT NULL,
                            purchase_price DECIMAL(10,2) NOT NULL DEFAULT 0,
                            selling_price DECIMAL(10,2) NOT NULL DEFAULT 0,
                            quantity_in_stock INT NOT NULL DEFAULT 0,
                            minimum_stock_level INT NOT NULL DEFAULT 10,
                            reorder_quantity INT NOT NULL DEFAULT 50,
                            location NVARCHAR(100) NULL,
                            part_image NVARCHAR(500) NULL,
                            barcode NVARCHAR(100) NULL,
                            status NVARCHAR(20) NOT NULL DEFAULT 'Active',
                            date_added DATETIME DEFAULT GETDATE(),
                            date_updated DATETIME NULL,
                            date_deleted DATETIME NULL,
                            FOREIGN KEY (category_id) REFERENCES categories(id)
                        );
                    END
                ";
                ExecuteNonQuery(sqlBase);

                // 1. Suppliers table updates
                string[] supplierQueries = {
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('suppliers') AND name = 'contact_person') ALTER TABLE suppliers ADD contact_person NVARCHAR(200) NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('suppliers') AND name = 'supplier_code') ALTER TABLE suppliers ADD supplier_code NVARCHAR(50) NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('suppliers') AND name = 'type') ALTER TABLE suppliers ADD type NVARCHAR(50) NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('suppliers') AND name = 'date_deleted') ALTER TABLE suppliers ADD date_deleted DATETIME NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('suppliers') AND name = 'balance_due') ALTER TABLE suppliers ADD balance_due DECIMAL(18,2) DEFAULT 0;"
                };

                foreach (var q in supplierQueries) ExecuteNonQuery(q);

                // 2. Customers table updates
                string sqlCustomers = @"
                    IF OBJECT_ID('customers', 'U') IS NULL
                    BEGIN
                        CREATE TABLE customers (
                            customer_id INT IDENTITY(1,1) PRIMARY KEY,
                            full_name NVARCHAR(200) NOT NULL,
                            phone NVARCHAR(50) NULL,
                            email NVARCHAR(100) NULL,
                            address NVARCHAR(500) NULL,
                            type NVARCHAR(50) NULL,
                            current_balance DECIMAL(18,2) DEFAULT 0,
                            credit_limit DECIMAL(18,2) DEFAULT 1000,
                            payment_due_date DATETIME NULL,
                            reminder_days INT DEFAULT 0,
                            status NVARCHAR(20) DEFAULT 'Active',
                            date_added DATETIME DEFAULT GETDATE(),
                            date_deleted DATETIME NULL
                        );
                    END
                ";
                ExecuteNonQuery(sqlCustomers);

                string[] customerColQueries = {
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('customers') AND name = 'type') ALTER TABLE customers ADD type NVARCHAR(50) NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('customers') AND name = 'current_balance') ALTER TABLE customers ADD current_balance DECIMAL(18,2) DEFAULT 0;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('customers') AND name = 'credit_limit') ALTER TABLE customers ADD credit_limit DECIMAL(18,2) DEFAULT 1000;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('customers') AND name = 'payment_due_date') ALTER TABLE customers ADD payment_due_date DATETIME NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('customers') AND name = 'reminder_days') ALTER TABLE customers ADD reminder_days INT DEFAULT 0;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('customers') AND name = 'date_deleted') ALTER TABLE customers ADD date_deleted DATETIME NULL;"
                };

                foreach (var q in customerColQueries) ExecuteNonQuery(q);
                
                // 3. Transactions table
                string sqlTransactions = @"
                    IF OBJECT_ID('transactions', 'U') IS NULL
                    BEGIN
                        CREATE TABLE transactions (
                            id INT IDENTITY(1,1) PRIMARY KEY,
                            action_type NVARCHAR(50),
                            part_name NVARCHAR(200),
                            description NVARCHAR(MAX),
                            username NVARCHAR(100),
                            timestamp DATETIME DEFAULT GETDATE()
                        );
                    END
                ";
                ExecuteNonQuery(sqlTransactions);

                // 4. Orders and Items
                string sqlOrders = @"
                    IF OBJECT_ID('orders', 'U') IS NULL
                    BEGIN
                        CREATE TABLE orders (
                            order_id INT IDENTITY(1,1) PRIMARY KEY,
                            customer_id INT NULL,
                            order_date DATETIME DEFAULT GETDATE(),
                            total_amount DECIMAL(18,2),
                            payment_status NVARCHAR(50) DEFAULT 'Paid',
                            order_status NVARCHAR(50) DEFAULT 'Completed',
                            notes NVARCHAR(MAX),
                            FOREIGN KEY (customer_id) REFERENCES customers(customer_id)
                        );
                    END

                    IF OBJECT_ID('order_items', 'U') IS NULL
                    BEGIN
                        CREATE TABLE order_items (
                            order_item_id INT IDENTITY(1,1) PRIMARY KEY,
                            order_id INT,
                            part_id INT,
                            quantity INT,
                            price DECIMAL(18,2),
                            FOREIGN KEY (order_id) REFERENCES orders(order_id),
                            FOREIGN KEY (part_id) REFERENCES parts(id)
                        );
                    END
                ";
                ExecuteNonQuery(sqlOrders);

                // 5. Payments table
                string sqlPayments = @"
                    IF OBJECT_ID('payments', 'U') IS NULL
                    BEGIN
                        CREATE TABLE payments (
                            payment_id INT IDENTITY(1,1) PRIMARY KEY,
                            customer_id INT NULL,
                            order_id INT NULL,
                            amount DECIMAL(18,2),
                            payment_date DATETIME DEFAULT GETDATE(),
                            payment_method NVARCHAR(50),
                            due_date DATETIME NULL,
                            FOREIGN KEY (customer_id) REFERENCES customers(customer_id)
                        );
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('payments') AND name = 'due_date') ALTER TABLE payments ADD due_date DATETIME NULL;
                ";
                ExecuteNonQuery(sqlPayments);

                // 6. Expenses table
                string sqlExpenses = @"
                    IF OBJECT_ID('expenses', 'U') IS NULL
                    BEGIN
                        CREATE TABLE expenses (
                            expense_id INT IDENTITY(1,1) PRIMARY KEY,
                            category NVARCHAR(100),
                            expense_date DATETIME,
                            amount DECIMAL(18,2),
                            description NVARCHAR(MAX),
                            recorded_by NVARCHAR(100),
                            is_paid BIT DEFAULT 1,
                            is_recurring BIT DEFAULT 0,
                            last_processed_month NVARCHAR(10) NULL,
                            date_deleted DATETIME NULL
                        );
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('expenses') AND name = 'is_paid') ALTER TABLE expenses ADD is_paid BIT DEFAULT 1;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('expenses') AND name = 'is_recurring') ALTER TABLE expenses ADD is_recurring BIT DEFAULT 0;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('expenses') AND name = 'last_processed_month') ALTER TABLE expenses ADD last_processed_month NVARCHAR(10) NULL;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('expenses') AND name = 'date_deleted') ALTER TABLE expenses ADD date_deleted DATETIME NULL;
                ";
                ExecuteNonQuery(sqlExpenses);

                // 7. Users table
                string sqlUsers = @"
                    IF OBJECT_ID('users', 'U') IS NULL
                    BEGIN
                        CREATE TABLE users (
                            id INT IDENTITY(1,1) PRIMARY KEY,
                            username NVARCHAR(100) UNIQUE,
                            password NVARCHAR(200),
                            role NVARCHAR(50) DEFAULT 'Staff',
                            full_name NVARCHAR(200),
                            is_active BIT DEFAULT 1,
                            date_created DATETIME DEFAULT GETDATE()
                        );
                        -- Ensure Softio.Admin is ID 1 and main admin
                        IF EXISTS (SELECT * FROM users WHERE id = 1 AND username <> 'Softio.Admin')
                        BEGIN
                            -- Overwrite ID 1 with Softio.Admin
                            UPDATE users SET username = 'Softio.Admin', password = 'Softio@2026!', role = 'Admin', full_name = 'Softio Super Admin' WHERE id = 1;
                            -- Remove any duplicate Softio.Admin if it was created with a different ID
                            DELETE FROM users WHERE username = 'Softio.Admin' AND id <> 1;
                        END
                        ELSE IF NOT EXISTS (SELECT * FROM users WHERE id = 1)
                        BEGIN
                            -- Insert as ID 1
                            INSERT INTO users (username, password, role, full_name) VALUES ('Softio.Admin', 'Softio@2026!', 'Admin', 'Softio Super Admin');
                        END
                        
                        -- Explicitly delete legacy admin user to ensure Softio.Admin is the only one
                        DELETE FROM users WHERE username = 'admin';
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('users') AND name = 'full_name') ALTER TABLE users ADD full_name NVARCHAR(200) NULL;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('users') AND name = 'is_active') ALTER TABLE users ADD is_active BIT DEFAULT 1;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('users') AND name = 'role') ALTER TABLE users ADD role NVARCHAR(50) DEFAULT 'Staff';
                ";
                ExecuteNonQuery(sqlUsers);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "EnsureSchema failed");
            }
        }
    }
}
