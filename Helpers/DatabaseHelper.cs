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
                // Suppliers table updates
                string[] supplierQueries = {
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('suppliers') AND name = 'contact_person') ALTER TABLE suppliers ADD contact_person NVARCHAR(200) NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('suppliers') AND name = 'supplier_code') ALTER TABLE suppliers ADD supplier_code NVARCHAR(50) NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('suppliers') AND name = 'type') ALTER TABLE suppliers ADD type NVARCHAR(50) NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('suppliers') AND name = 'date_deleted') ALTER TABLE suppliers ADD date_deleted DATETIME NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('suppliers') AND name = 'balance_due') ALTER TABLE suppliers ADD balance_due DECIMAL(18,2) DEFAULT 0;"
                };

                foreach (var q in supplierQueries) ExecuteNonQuery(q);

                // Customers table updates
                string[] customerQueries = {
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('customers') AND name = 'type') ALTER TABLE customers ADD type NVARCHAR(50) NULL;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('customers') AND name = 'current_balance') ALTER TABLE customers ADD current_balance DECIMAL(18,2) DEFAULT 0;",
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('customers') AND name = 'date_deleted') ALTER TABLE customers ADD date_deleted DATETIME NULL;"
                };

                foreach (var q in customerQueries) ExecuteNonQuery(q);

                // Ensure order_items exists
                string sqlItems = @"
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
                    END";
                ExecuteNonQuery(sqlItems);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "EnsureSchema failed");
            }
        }
    }
}
