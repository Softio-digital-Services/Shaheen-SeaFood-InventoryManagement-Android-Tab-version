using System;
using System.Data;
using System.Data.SqlClient;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Services
{
    public class InventoryService
    {
        public DataTable GetAllParts(string search = "", bool lowStockOnly = false, bool activeOnly = false, string category = null)
        {
            string sql = @"SELECT p.id as part_id, p.part_number, p.part_name, 
                           ISNULL(c.category_name, 'Category') as category_name,
                           p.quantity_in_stock, p.selling_price, p.status, p.part_image,
                           p.minimum_stock_level, p.location, p.barcode, p.shelf
                           FROM parts p
                           LEFT JOIN categories c ON p.category_id = c.id
                           WHERE p.date_deleted IS NULL";

            if (!string.IsNullOrEmpty(search))
                sql += $" AND (p.part_name LIKE '%{search}%' OR p.part_number LIKE '%{search}%')";
            if (lowStockOnly)
                sql += " AND p.quantity_in_stock <= p.minimum_stock_level";
            if (activeOnly)
                sql += " AND p.status = 'Active'";
            if (!string.IsNullOrEmpty(category))
                sql += $" AND c.category_name = '{category}'";

            sql += " ORDER BY p.id";

            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public void AddPart(string name, string number, string categoryName, int stock, decimal price, int minStock, string imagePath, string barcode, string location, string shelf, string status)
        {
             int categoryId = GetCategoryId(categoryName);
             
             string sql = "INSERT INTO parts (part_name, part_number, category_id, quantity_in_stock, selling_price, minimum_stock_level, part_image, status, barcode, location, shelf, date_added) " +
                          "VALUES (@name, @num, @cat, @stock, @price, @min, @img, @status, @barcode, @loc, @shelf, GETDATE())";
                          
             if (!DatabaseHelper.ExecuteNonQuery(sql,
                new SqlParameter("@name", name),
                new SqlParameter("@num", number),
                new SqlParameter("@cat", categoryId),
                new SqlParameter("@stock", stock),
                new SqlParameter("@price", price),
                new SqlParameter("@min", minStock),
                new SqlParameter("@img", imagePath ?? (object)DBNull.Value),
                new SqlParameter("@status", status ?? "Active"),
                new SqlParameter("@barcode", barcode ?? ""),
                new SqlParameter("@loc", location ?? ""),
                new SqlParameter("@shelf", shelf ?? "")
             ))
             {
                 throw new Exception("Failed to add part. Database operation failed.");
             }
             
             LogTransaction("ADD", $"Added Part: {name} ({number})", name);
             GlobalEvents.RaiseInventoryUpdated();
        }

        public void UpdatePart(int id, string partName, string partNumber, string categoryName, decimal sellingPrice, int quantityInStock, int minStockLevel, string imagePath, string barcode, string location, string shelf, string status)
        {
             int categoryId = GetCategoryId(categoryName);
             
             string sql = $"UPDATE parts SET part_name=@name, part_number=@num, category_id=@cat, selling_price=@price, " +
                         $"quantity_in_stock=@stock, minimum_stock_level=@min, status=@status, barcode=@barcode, location=@loc, shelf=@shelf ";
             
             if(imagePath != null) sql += ", part_image=@img ";
             
             sql += "WHERE id=@id";
             
             // Build params list
             var paramsList = new System.Collections.Generic.List<SqlParameter>();
             paramsList.Add(new SqlParameter("@name", partName));
             paramsList.Add(new SqlParameter("@num", partNumber));
             paramsList.Add(new SqlParameter("@cat", categoryId));
             paramsList.Add(new SqlParameter("@price", sellingPrice));
             paramsList.Add(new SqlParameter("@stock", quantityInStock));
             paramsList.Add(new SqlParameter("@min", minStockLevel));
             paramsList.Add(new SqlParameter("@status", status));
             paramsList.Add(new SqlParameter("@barcode", barcode ?? ""));
             paramsList.Add(new SqlParameter("@loc", location ?? ""));
             paramsList.Add(new SqlParameter("@shelf", shelf ?? ""));
             paramsList.Add(new SqlParameter("@id", id));
             if(imagePath != null) paramsList.Add(new SqlParameter("@img", imagePath));

             if (!DatabaseHelper.ExecuteNonQuery(sql, paramsList.ToArray()))
             {
                 throw new Exception("Failed to update part. Database operation failed.");
             }
             
             LogTransaction("EDIT", $"Updated Part ID: {id}", partName);
             GlobalEvents.RaiseInventoryUpdated();
        }

        private int GetCategoryId(string categoryName)
        {
            if(string.IsNullOrWhiteSpace(categoryName)) return 1; // Default
            
            string catSql = "SELECT id FROM categories WHERE category_name = @name";
            object result = DatabaseHelper.ExecuteScalar<object>(catSql, new SqlParameter("@name", categoryName));
            if(result != null) return Convert.ToInt32(result);
            
            // Create if not exists
            string insert = "INSERT INTO categories (category_name, description) VALUES (@name, ''); SELECT SCOPE_IDENTITY();";
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar<decimal>(insert, new SqlParameter("@name", categoryName)));
        }

        private void LogTransaction(string action, string description, string partName = "N/A")
        {
             try
             {
                 string sql = "INSERT INTO transactions (action_type, part_name, description, username, timestamp) VALUES (@action, @part, @desc, 'System', GETDATE())";
                 DatabaseHelper.ExecuteNonQuery(sql, 
                    new SqlParameter("@action", action),
                    new SqlParameter("@part", partName),
                    new SqlParameter("@desc", description));
             }
             catch { }
        }

        public void DeletePart(int partId)
        {
            string partName = DatabaseHelper.ExecuteScalar<string>($"SELECT part_name FROM parts WHERE id = {partId}");
            if (string.IsNullOrEmpty(partName)) partName = "N/A";
            
            DatabaseHelper.ExecuteNonQuery($"UPDATE parts SET date_deleted = GETDATE() WHERE id = {partId}");
            LogTransaction("DELETE", $"Deleted Part: {partName} (ID: {partId})", partName);
            GlobalEvents.RaiseInventoryUpdated();
        }

        public DataTable GetCategories()
        {
            return DatabaseHelper.ExecuteDataTable("SELECT id, category_name FROM categories ORDER BY category_name");
        }

        public bool PartExists(string partNumber)
        {
            string sql = "SELECT COUNT(*) FROM parts WHERE part_number = @partNumber AND date_deleted IS NULL";
            int count = DatabaseHelper.ExecuteScalar<int>(sql, new SqlParameter("@partNumber", partNumber));
            return count > 0;
        }

        public bool BarcodeExists(string barcode, int? excludePartId = null)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return false;
            string sql = "SELECT COUNT(*) FROM parts WHERE barcode = @barcode AND date_deleted IS NULL";
            var parameters = new System.Collections.Generic.List<SqlParameter> { new SqlParameter("@barcode", barcode) };
            
            if (excludePartId.HasValue)
            {
                sql += " AND id != @id";
                parameters.Add(new SqlParameter("@id", excludePartId.Value));
            }
            
            int count = DatabaseHelper.ExecuteScalar<int>(sql, parameters.ToArray());
            return count > 0;
        }

        public DataRow GetPartByBarcodeOrNumber(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;
            
            string sql = "SELECT id, part_name, part_number, barcode, selling_price FROM parts WHERE (barcode = @query OR part_number = @query OR part_name = @query) AND date_deleted IS NULL";
            DataTable dt = DatabaseHelper.ExecuteDataTable(sql, new SqlParameter("@query", query));
            if (dt.Rows.Count > 0) return dt.Rows[0];
            return null;
        }

        public void ImportPart(string partNumber, string partName, string categoryName, int quantity, int minStock, decimal unitPrice, string location, string status)
        {
            try
            {
                // Get or create category
                int categoryId = 1; // Default category
                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    string catSql = "SELECT id FROM categories WHERE category_name = @name";
                    object catResult = DatabaseHelper.ExecuteScalar<object>(catSql, new SqlParameter("@name", categoryName));
                    
                    if (catResult != null)
                    {
                        categoryId = Convert.ToInt32(catResult);
                    }
                    else
                    {
                        // Create new category
                        string insertCatSql = "INSERT INTO categories (category_name, description) VALUES (@name, ''); SELECT SCOPE_IDENTITY();";
                        catResult = DatabaseHelper.ExecuteScalar<object>(insertCatSql, new SqlParameter("@name", categoryName));
                        if (catResult != null)
                            categoryId = Convert.ToInt32(catResult);
                    }
                }

                // Insert part
                string sql = @"INSERT INTO parts (part_number, part_name, category_id, quantity_in_stock, 
                               minimum_stock_level, selling_price, location, status, date_added) 
                               VALUES (@partNumber, @partName, @categoryId, @quantity, @minStock, @price, @location, @status, GETDATE())";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@partNumber", partNumber),
                    new SqlParameter("@partName", partName),
                    new SqlParameter("@categoryId", categoryId),
                    new SqlParameter("@quantity", quantity),
                    new SqlParameter("@minStock", minStock),
                    new SqlParameter("@price", unitPrice),
                    new SqlParameter("@location", location ?? ""),
                    new SqlParameter("@status", status ?? "Active")
                );
                GlobalEvents.RaiseInventoryUpdated();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "InventoryService.ImportPart");
                throw;
            }
        }

        public void AdjustStock(int partId, int change, string reason)
        {
            try
            {
                string sql = "UPDATE parts SET quantity_in_stock = quantity_in_stock + @change WHERE id = @id";
                DatabaseHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@change", change),
                    new SqlParameter("@id", partId));

                // Log movement
                string action = change > 0 ? "ADJUST_IN" : "ADJUST_OUT";
                string partNameResult = DatabaseHelper.ExecuteScalar<object>($"SELECT part_name FROM parts WHERE id = {partId}").ToString();
                
                string sqlLog = "INSERT INTO stock_movements (part_id, movement_type, quantity, performed_by, notes, movement_date) " +
                                "VALUES (@pid, @type, @qty, @user, @notes, GETDATE())";
                DatabaseHelper.ExecuteNonQuery(sqlLog,
                    new SqlParameter("@pid", partId),
                    new SqlParameter("@type", "ADJUSTMENT"),
                    new SqlParameter("@qty", Math.Abs(change)),
                    new SqlParameter("@user", UserSession.Username),
                    new SqlParameter("@notes", reason)
                );

                LogTransaction(action, $"Adjusted stock of {partNameResult} by {change}. Reason: {reason}", partNameResult);
                GlobalEvents.RaiseInventoryUpdated();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "InventoryService.AdjustStock");
                throw;
            }
        }
    }

    public class CustomerService
    {
        public DataTable GetAllCustomers(string search = "")
        {
            string sql = "SELECT customer_id as ID, full_name as Name, phone as Phone, email as Email, address as Address, current_balance as 'Balance Due', credit_limit, payment_due_date, reminder_days FROM customers WHERE date_deleted IS NULL";
            if (!string.IsNullOrEmpty(search))
            {
                sql += $" AND (full_name LIKE '%{search}%' OR phone LIKE '%{search}%' OR email LIKE '%{search}%')";
            }
            sql += " ORDER BY current_balance DESC";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public int AddCustomer(string name, string phone, string email, string address, string type, decimal creditLimit = 1000, DateTime? dueDate = null, int reminderDays = 0)
        {
             string sql = "INSERT INTO customers (full_name, phone, email, address, current_balance, type, credit_limit, payment_due_date, reminder_days) " +
                          "VALUES (@name, @phone, @email, @addr, 0, @type, @credit, @due, @rem); SELECT SCOPE_IDENTITY();";
             
             object result = DatabaseHelper.ExecuteScalar<object>(sql,
                 new SqlParameter("@name", name),
                 new SqlParameter("@phone", phone),
                 new SqlParameter("@email", email),
                 new SqlParameter("@addr", address),
                 new SqlParameter("@type", type),
                 new SqlParameter("@credit", creditLimit),
                 new SqlParameter("@due", (object)dueDate ?? DBNull.Value),
                 new SqlParameter("@rem", reminderDays)
             );
             
             LogTransaction("CUSTOMER_ADD", $"Added Customer: {name} (Limit: {creditLimit})", name);
             GlobalEvents.RaiseCustomersUpdated();
             
             return result != null ? Convert.ToInt32(result) : -1;
        }

        private void LogTransaction(string action, string description, string entityName = "N/A")
        {
             try
             {
                 string sql = "INSERT INTO transactions (action_type, part_name, description, username, timestamp) VALUES (@action, @entity, @desc, 'System', GETDATE())";
                 DatabaseHelper.ExecuteNonQuery(sql, 
                    new SqlParameter("@action", action),
                    new SqlParameter("@entity", entityName),
                    new SqlParameter("@desc", description));
             }
             catch { }
        }

        public void UpdateBalance(int customerId, decimal amount)
        {
             DatabaseHelper.ExecuteNonQuery($"UPDATE customers SET current_balance = current_balance + {amount} WHERE customer_id = {customerId}");
             // Note: Payments/Orders are usually logged elsewhere, but we can log balance adjustments here if manual
        }

        public void UpdateCustomer(int id, string name, string phone, string email, string address, string type, decimal creditLimit, DateTime? dueDate, int reminderDays)
        {
            string sql = "UPDATE customers SET full_name=@name, phone=@phone, email=@email, address=@addr, type=@type, credit_limit=@credit, payment_due_date=@due, reminder_days=@rem WHERE customer_id=@id";
            DatabaseHelper.ExecuteNonQuery(sql,
                 new SqlParameter("@name", name),
                 new SqlParameter("@phone", phone),
                 new SqlParameter("@email", email),
                 new SqlParameter("@addr", address),
                 new SqlParameter("@type", type),
                 new SqlParameter("@credit", creditLimit),
                 new SqlParameter("@due", (object)dueDate ?? DBNull.Value),
                 new SqlParameter("@rem", reminderDays),
                 new SqlParameter("@id", id));
                 
            LogTransaction("CUSTOMER_UPDATE", $"Updated Customer: {name} (ID: {id})", name);
            GlobalEvents.RaiseCustomersUpdated();
        }

        public void DeleteCustomer(int id)
        {
            string customerName = DatabaseHelper.ExecuteScalar<string>($"SELECT name FROM customers WHERE customer_id = {id}");
            if (string.IsNullOrEmpty(customerName)) customerName = "N/A";

            DatabaseHelper.ExecuteNonQuery($"UPDATE customers SET date_deleted = GETDATE() WHERE customer_id = {id}");
            LogTransaction("CUSTOMER_DELETE", $"Deleted Customer: {customerName} (ID: {id})", customerName);
            GlobalEvents.RaiseCustomersUpdated();
        }
        
        public CustomerStats GetStats()
        {
             CustomerStats stats = new CustomerStats();
             try {
                stats.TotalCustomers = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM customers");
                stats.TotalDebt = DatabaseHelper.ExecuteScalar<decimal>("SELECT ISNULL(SUM(current_balance), 0) FROM customers");
                // ... others
             } catch {}
             return stats;
        }

        public bool CustomerExists(string email)
        {
            string sql = "SELECT COUNT(*) FROM customers WHERE email = @email AND date_deleted IS NULL";
            int count = DatabaseHelper.ExecuteScalar<int>(sql, new SqlParameter("@email", email));
            return count > 0;
        }

        public void ImportCustomer(string customerName, string email, string phone, string address, string city, string postalCode, string notes)
        {
            try
            {
                string sql = @"INSERT INTO customers (full_name, email, phone, address, current_balance, type, date_added) 
                               VALUES (@name, @email, @phone, @address, 0, 'Regular', GETDATE())";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@name", customerName),
                    new SqlParameter("@email", email ?? ""),
                    new SqlParameter("@phone", phone ?? ""),
                    new SqlParameter("@address", (address ?? "") + (string.IsNullOrEmpty(city) ? "" : ", " + city) + (string.IsNullOrEmpty(postalCode) ? "" : " " + postalCode))
                );
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "CustomerService.ImportCustomer");
                throw;
            }
            // Trigger update after import
            GlobalEvents.RaiseCustomersUpdated();
        }
    }
    
    public class CustomerStats 
    {
        public int TotalCustomers { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal TotalPaid { get; set; }
        public int ActiveCustomers { get; set; }
    }

    public class SupplierService
    {
        public DataTable GetAllSuppliers(string search = "")
        {
            string sql = "SELECT id as ID, supplier_name, phone, email, address, type, balance_due, payment_due_date, reminder_days FROM suppliers WHERE date_deleted IS NULL";
            if (!string.IsNullOrEmpty(search))
            {
                sql += $" AND (supplier_name LIKE '%{search}%' OR phone LIKE '%{search}%' OR email LIKE '%{search}%')";
            }
            sql += " ORDER BY supplier_name";
            return DatabaseHelper.ExecuteDataTable(sql);
        }

        public bool SupplierExists(string supplierName)
        {
            string sql = "SELECT COUNT(*) FROM suppliers WHERE supplier_name = @name AND date_deleted IS NULL";
            int count = DatabaseHelper.ExecuteScalar<int>(sql, new SqlParameter("@name", supplierName));
            return count > 0;
        }

        public void ImportSupplier(string supplierName, string contactPerson, string email, string phone, string address, string city, string postalCode, string website, string notes)
        {
            try
            {
                string fullAddress = (address ?? "") + (string.IsNullOrEmpty(city) ? "" : ", " + city) + (string.IsNullOrEmpty(postalCode) ? "" : " " + postalCode);
                
                string sql = @"INSERT INTO suppliers (supplier_name, phone, email, address, balance_due, date_added) 
                               VALUES (@name, @phone, @email, @address, 0, GETDATE())";

                DatabaseHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@name", supplierName),
                    new SqlParameter("@phone", phone ?? ""),
                    new SqlParameter("@email", email ?? ""),
                    new SqlParameter("@address", fullAddress)
                );
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "SupplierService.ImportSupplier");
                throw;
            }
        }

        public void AddSupplier(string name, string phone, string email, string address, string type, DateTime? dueDate = null, int reminderDays = 0)
        {
             string sql = "INSERT INTO suppliers (supplier_name, phone, email, address, type, balance_due, payment_due_date, reminder_days) " +
                          "VALUES (@name, @phone, @email, @addr, @type, 0, @due, @rem)";
             
             DatabaseHelper.ExecuteNonQuery(sql,
                 new SqlParameter("@name", name),
                 new SqlParameter("@phone", phone),
                 new SqlParameter("@email", email),
                 new SqlParameter("@addr", address),
                 new SqlParameter("@type", type),
                 new SqlParameter("@due", (object)dueDate ?? DBNull.Value),
                 new SqlParameter("@rem", reminderDays)
             );
             
             GlobalEvents.RaiseSuppliersUpdated();
        }

        public void UpdateSupplier(int id, string name, string phone, string email, string address, string type, DateTime? dueDate, int reminderDays)
        {
            string sql = "UPDATE suppliers SET supplier_name=@name, phone=@phone, email=@email, address=@addr, type=@type, payment_due_date=@due, reminder_days=@rem WHERE id=@id";
            DatabaseHelper.ExecuteNonQuery(sql,
                 new SqlParameter("@name", name),
                 new SqlParameter("@phone", phone),
                 new SqlParameter("@email", email),
                 new SqlParameter("@addr", address),
                 new SqlParameter("@type", type),
                 new SqlParameter("@due", (object)dueDate ?? DBNull.Value),
                 new SqlParameter("@rem", reminderDays),
                 new SqlParameter("@id", id));
                 
            GlobalEvents.RaiseSuppliersUpdated();
        }

        public void DeleteSupplier(int id)
        {
            string sql = "UPDATE suppliers SET date_deleted = GETDATE() WHERE id = @id";
            DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter("@id", id));
            GlobalEvents.RaiseSuppliersUpdated();
        }
    }
}
