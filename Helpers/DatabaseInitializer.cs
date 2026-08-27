using System;
using System.IO;
#if !ANDROID
using System.Windows.Forms;
#endif
using Microsoft.Data.Sqlite;

namespace Shaheen_InventoryManagement_Android.Helpers
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            MigratePartsStockType();
            // SQLite creates the file automatically -- no CreateDatabase() needed
            DatabaseHelper.EnsureSchema();
            UpdateSchema();
            // SeedIfEmpty(); // Disabled to provide a fresh start for clients
        }

        /// <summary>
        /// Idempotent patches applied on every startup.
        /// Safe to re-run -- all guards use IF NOT EXISTS / INSERT OR IGNORE.
        /// </summary>
        private static void UpdateSchema()
        {
            // Ensure category_image column exists for existing databases
            DatabaseHelper.ExecuteNonQuery("ALTER TABLE categories ADD COLUMN category_image TEXT;");

            // Ensure admin user exists
            int adminCount = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM users WHERE username = 'Softio.Admin'");
            if (adminCount == 0)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO users (username, password, full_name, role) VALUES ('Softio.Admin', 'Softio@2026!', 'Softio Super Admin', 'Admin');"
                );
            }

            // Ensure Admin user exists
            int testAdminCount = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM users WHERE username = 'Admin'");
            if (testAdminCount == 0)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO users (username, password, full_name, role) VALUES ('Admin', 'Admin.Softio', 'Test Admin', 'Admin');"
                );
            }
            // Migrate any existing 'test' user to the new 'Admin' credentials
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE users SET username = 'Admin', password = 'Admin.Softio' WHERE username = 'test';"
            );

            // Ensure test staff user exists
            int testStaffCount = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM users WHERE username = 'staff'");
            if (testStaffCount == 0)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO users (username, password, full_name, role) VALUES ('staff', 'Staff.Softio', 'Test Staff', 'Staff');"
                );
            }

            // Ensure production user exists
            int prodUserCount = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM users WHERE LOWER(username) = 'production'");
            if (prodUserCount == 0)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO users (username, password, full_name, role) VALUES ('production', 'Productio@2026!', 'Production User', 'Production');"
                );
            }
            else
            {
                // Update password for production user in case they already exist with any other password
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE users SET password = 'Productio@2026!' WHERE LOWER(username) = 'production';"
                );
            }

            // Repair: Standardise status values (fix Arabic UI bug)
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE parts SET status = 'Active' WHERE status NOT IN ('Active', 'Inactive') AND date_deleted IS NULL;"
            );

            // Migration: Convert existing parts from pack-based to base-unit tracking
            DatabaseHelper.ExecuteNonQuery(
                @"UPDATE parts
                  SET 
                      quantity_in_stock = quantity_in_stock * pack_size,
                      minimum_stock_level = minimum_stock_level * pack_size,
                      purchase_price = purchase_price / pack_size,
                      pack_price = purchase_price / pack_size,
                      item_price = purchase_price / pack_size,
                      piece_price = (purchase_price / pack_size) / conversion_value,
                      pack_size = 1.0
                  WHERE pack_size IS NOT NULL AND pack_size != 1.0 AND date_deleted IS NULL;"
            );
        }

        /// <summary>
        /// Seeds demo data only when the database is brand new (no suppliers yet).
        /// </summary>
        private static void SeedIfEmpty()
        {
            int count = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM suppliers;");
            if (count > 0) return; // Already seeded

            // Suppliers
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO suppliers (supplier_name, phone, email, address, type, supplier_code)
                VALUES
                  ('Global Auto Parts', '555-0101', 'contact@globalautoparts.com', '123 Industrial Way', 'Wholesaler', 'SUP-001'),
                  ('Brake Systems Inc',  '555-0102', 'sales@brakesystems.com',     '456 Safety Blvd',    'Specialist',  'SUP-002');
            ");

            // Customers
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO customers (full_name, phone, email, address, type, current_balance)
                VALUES
                  ('John Smith',    '555-0201', 'john.smith@email.com',       '789 Maple Ave',   'Retail',    0),
                  ('Speedy Garage', '555-0202', 'manager@speedygarage.com', '321 Mechanic Ln', 'Corporate', 150.00);
            ");

            // Demo parts (category_id 1 = Seafood, 2 = Spices)
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO parts (part_name, part_number, description, category_id, supplier_id, purchase_price, selling_price, quantity_in_stock, minimum_stock_level, location, status)
                VALUES
                  ('Salmon Fillet',  'SF-001',  'Fresh salmon fillet',   1, 1, 12.00, 18.50, 50, 10, 'Freezer A', 'Active'),
                  ('Shrimp Raw',     'SHR-002', 'Jumbo white shrimp set',2, 2, 20.00, 35.00, 30,  8, 'Freezer B', 'Active'),
                  ('Tuna Steak',     'TUN-003', 'Yellowfin tuna steak',  1, 1, 15.00, 25.00, 40, 10, 'Freezer A', 'Active'),
                  ('Lemon Herb',     'HER-004', 'Herb seasoning mix',    2, 1,  2.00,  5.00,100, 20, 'Shelf C',   'Active'),
                  ('Lobster Tail',   'LOB-005', 'Maine lobster tail',    1, 1, 25.00, 45.00, 15,  5, 'Freezer B', 'Active');
            ");

            // Demo orders
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO orders (order_date, customer_id, total_amount, status, payment_status, payment_method)
                VALUES
                  (datetime('now','-1 day'), 1, 150.00, 'Completed', 'Paid', 'Cash'),
                  (datetime('now'),          2, 250.00, 'Completed', 'Paid', 'Card'),
                  (datetime('now'),          1,  85.00, 'Completed', 'Paid', 'Cash');
            ");

            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO order_items (order_id, part_id, quantity, price)
                VALUES (1,1,5,12.00),(1,2,2,45.00),(2,3,10,18.00),(2,4,2,8.00),(3,1,3,12.00),(3,5,1,60.00);
            ");
        }

        private static void MigratePartsStockType()
        {
            try
            {
                // Check if table exists first
                int tableExists = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='parts';");
                if (tableExists == 0) return;

                var dt = DatabaseHelper.ExecuteDataTable("PRAGMA table_info(parts)");
                bool isInteger = false;
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    if (row["name"].ToString().Equals("quantity_in_stock", StringComparison.OrdinalIgnoreCase))
                    {
                        string type = row["type"].ToString();
                        if (type.Equals("INTEGER", StringComparison.OrdinalIgnoreCase))
                        {
                            isInteger = true;
                        }
                        break;
                    }
                }

                if (isInteger)
                {
                    DatabaseHelper.ExecuteNonQuery("ALTER TABLE parts RENAME TO parts_old;");
                    DatabaseHelper.EnsureSchema();

                    var colsList = new System.Collections.Generic.List<string>();
                    var dtOld = DatabaseHelper.ExecuteDataTable("PRAGMA table_info(parts_old)");
                    foreach (System.Data.DataRow row in dtOld.Rows)
                    {
                        colsList.Add(row["name"].ToString());
                    }
                    string cols = string.Join(", ", colsList);

                    DatabaseHelper.ExecuteNonQuery($"INSERT INTO parts ({cols}) SELECT {cols} FROM parts_old;");
                    DatabaseHelper.ExecuteNonQuery("DROP TABLE parts_old;");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MigratePartsStockType error: " + ex.Message);
            }
        }
    }
}
