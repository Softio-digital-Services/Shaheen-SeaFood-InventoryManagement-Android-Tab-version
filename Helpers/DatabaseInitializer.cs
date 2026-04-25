using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace GenericInventorySystem.Helpers
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            string dbPath = DatabaseConfig.DatabasePath;
            bool dbExists = File.Exists(dbPath);

            // Ensure directory exists
            string dbDir = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }

            if (!dbExists)
            {
                CreateDatabase(dbPath);
                CreateSchema();
                SeedData();
            }
            else
            {
                // Ensure new schema elements exist if DB was created in previous version
                UpdateSchema(); 
            }
        }

        private static void UpdateSchema()
        {
             string sql = @"
                -- ═══ CORE TABLES (idempotent guards) ═══════════════════════

                IF OBJECT_ID('dbo.users', 'U') IS NULL
                BEGIN
                    CREATE TABLE users (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        username NVARCHAR(50) NOT NULL UNIQUE,
                        password NVARCHAR(100) NOT NULL,
                        full_name NVARCHAR(100),
                        role NVARCHAR(50) DEFAULT 'User',
                        date_created DATETIME DEFAULT GETDATE()
                    );
                    INSERT INTO users (username, password, full_name, role) VALUES ('Softio.Admin', 'Softio@2026!', 'Softio Super Admin', 'Admin');
                END

                IF OBJECT_ID('dbo.categories', 'U') IS NULL
                BEGIN
                    CREATE TABLE categories (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        category_name NVARCHAR(100) NOT NULL,
                        date_created DATETIME DEFAULT GETDATE(),
                        description VARCHAR(255) NULL
                    );
                    INSERT INTO categories (category_name) VALUES ('Engine'),('Brakes'),('Suspension'),('Electrical'),('Body'),('Interior'),('Accessories');
                END

                IF OBJECT_ID('dbo.suppliers', 'U') IS NULL
                BEGIN
                    CREATE TABLE suppliers (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        supplier_name NVARCHAR(100) NOT NULL,
                        phone NVARCHAR(20), email NVARCHAR(100), address NVARCHAR(255),
                        type NVARCHAR(50), balance_due DECIMAL(18,2) DEFAULT 0,
                        supplier_code NVARCHAR(50), date_added DATETIME DEFAULT GETDATE(),
                        date_deleted DATETIME NULL,
                        payment_due_date DATETIME NULL, reminder_days INT DEFAULT 0
                    );
                    INSERT INTO suppliers (supplier_name, phone, email, address, type, supplier_code)
                        VALUES ('Global Auto Parts','555-0101','contact@globalautoparts.com','123 Industrial Way','Wholesaler','SUP-001'),
                               ('Brake Systems Inc','555-0102','sales@brakesystems.com','456 Safety Blvd','Specialist','SUP-002');
                END

                IF OBJECT_ID('dbo.customers', 'U') IS NULL
                BEGIN
                    CREATE TABLE customers (
                        customer_id INT IDENTITY(1,1) PRIMARY KEY,
                        full_name NVARCHAR(100) NOT NULL,
                        phone NVARCHAR(20), email NVARCHAR(100), address NVARCHAR(255),
                        type NVARCHAR(50), current_balance DECIMAL(18,2) DEFAULT 0,
                        credit_limit DECIMAL(18,2) DEFAULT 1000,
                        date_added DATETIME DEFAULT GETDATE(), date_deleted DATETIME NULL,
                        payment_due_date DATETIME NULL, reminder_days INT DEFAULT 0
                    );
                    INSERT INTO customers (full_name, phone, email, address, type, current_balance)
                        VALUES ('John Smith','555-0201','john.smith@email.com','789 Maple Ave','Retail',0),
                               ('Speedy Garage','555-0202','manager@speedygarage.com','321 Mechanic Ln','Corporate',150.00);
                END

                IF OBJECT_ID('dbo.parts', 'U') IS NULL
                BEGIN
                    CREATE TABLE parts (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        part_name NVARCHAR(100) NOT NULL, part_number NVARCHAR(50),
                        description NVARCHAR(MAX), category_id INT, supplier_id INT,
                        purchase_price DECIMAL(18,2) DEFAULT 0, selling_price DECIMAL(18,2) DEFAULT 0,
                        quantity_in_stock INT DEFAULT 0, minimum_stock_level INT DEFAULT 5,
                        reorder_quantity INT DEFAULT 10, location NVARCHAR(100),
                        shelf NVARCHAR(50), part_image NVARCHAR(255), barcode NVARCHAR(100),
                        status NVARCHAR(50) DEFAULT 'Active',
                        date_added DATETIME DEFAULT GETDATE(), date_deleted DATETIME NULL
                    );
                    INSERT INTO parts (part_name, part_number, description, category_id, supplier_id, purchase_price, selling_price, quantity_in_stock, minimum_stock_level, location, status)
                        VALUES ('Oil Filter','OIL-001','Standard oil filter',1,1,5.00,12.00,50,10,'Shelf A','Active'),
                               ('Brake Pads','BRK-002','Front brake pads set',2,2,20.00,45.00,30,8,'Shelf B','Active'),
                               ('Air Filter','AIR-003','Engine air filter',1,1,8.00,18.00,40,10,'Shelf A','Active'),
                               ('Spark Plug','SPK-004','NGK spark plug',1,1,3.00,8.00,100,20,'Shelf C','Active'),
                               ('Timing Belt','TIM-005','Heavy duty timing belt',1,1,25.00,60.00,15,5,'Shelf B','Active');
                END

                IF OBJECT_ID('dbo.transactions', 'U') IS NULL
                BEGIN
                    CREATE TABLE transactions (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        timestamp DATETIME DEFAULT GETDATE(),
                        action_type NVARCHAR(50),
                        part_name NVARCHAR(100),
                        description NVARCHAR(255),
                        username NVARCHAR(50)
                    );
                END

                IF OBJECT_ID('dbo.payments', 'U') IS NULL
                BEGIN
                    CREATE TABLE payments (
                        payment_id INT IDENTITY(1,1) PRIMARY KEY,
                        entity_type NVARCHAR(50), entity_id INT,
                        amount DECIMAL(18,2),
                        payment_date DATETIME DEFAULT GETDATE(),
                        notes NVARCHAR(255)
                    );
                END

                -- ═══ SUPPLEMENTAL / NEWER TABLES ════════════════════════════

                -- Ensure orders table exists first (required by order_items FK)
                IF OBJECT_ID('dbo.orders', 'U') IS NULL
                BEGIN
                    CREATE TABLE orders (
                        order_id INT IDENTITY(1,1) PRIMARY KEY,
                        order_date DATETIME DEFAULT GETDATE(),
                        customer_id INT,
                        total_amount DECIMAL(18,2) DEFAULT 0,
                        status NVARCHAR(50) DEFAULT 'Completed',
                        payment_status NVARCHAR(50) DEFAULT 'Paid',
                        amount_paid DECIMAL(18,2) DEFAULT 0,
                        payment_method NVARCHAR(50) DEFAULT 'Cash'
                    );
                END

                IF OBJECT_ID('dbo.order_items', 'U') IS NULL
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
                
                -- Patch: Add date_created to categories if missing
                IF COL_LENGTH('categories', 'date_created') IS NULL
                BEGIN
                    ALTER TABLE categories ADD date_created DATETIME DEFAULT GETDATE();
                    EXEC('UPDATE categories SET date_created = GETDATE() WHERE date_created IS NULL');
                END

                -- Patch: Add description to categories if missing
                IF COL_LENGTH('categories', 'description') IS NULL
                BEGIN
                    ALTER TABLE categories ADD description VARCHAR(255) NULL;
                END

                -- Patch: Add date_deleted to customers
                IF COL_LENGTH('customers', 'date_deleted') IS NULL
                BEGIN
                    ALTER TABLE customers ADD date_deleted DATETIME NULL;
                END

                -- Patch: Add credit_limit to customers
                IF COL_LENGTH('customers', 'credit_limit') IS NULL
                BEGIN
                    ALTER TABLE customers ADD credit_limit DECIMAL(18,2) DEFAULT 1000;
                END

                -- Patch: Add date_deleted to suppliers
                IF COL_LENGTH('suppliers', 'date_deleted') IS NULL
                BEGIN
                     ALTER TABLE suppliers ADD date_deleted DATETIME NULL;
                END

                -- Patch: Add payment_due_date and reminder_days to suppliers
                IF COL_LENGTH('suppliers', 'payment_due_date') IS NULL
                BEGIN
                    ALTER TABLE suppliers ADD payment_due_date DATETIME NULL;
                    ALTER TABLE suppliers ADD reminder_days INT DEFAULT 0;
                END

                -- Patch: Add payment_due_date and reminder_days to customers
                IF COL_LENGTH('customers', 'payment_due_date') IS NULL
                BEGIN
                    ALTER TABLE customers ADD payment_due_date DATETIME NULL;
                    ALTER TABLE customers ADD reminder_days INT DEFAULT 0;
                END

                -- Purchase Orders
                IF OBJECT_ID('dbo.purchase_orders', 'U') IS NULL
                BEGIN
                    CREATE TABLE purchase_orders (
                        po_id INT IDENTITY(1,1) PRIMARY KEY,
                        supplier_id INT,
                        order_date DATETIME DEFAULT GETDATE(),
                        total_amount DECIMAL(18,2) DEFAULT 0,
                        status NVARCHAR(50) DEFAULT 'Pending',
                        received_date DATETIME NULL,
                        notes NVARCHAR(255),
                        FOREIGN KEY (supplier_id) REFERENCES suppliers(id)
                    );
                END

                -- Purchase Order Items
                IF OBJECT_ID('dbo.purchase_order_items', 'U') IS NULL
                BEGIN
                    CREATE TABLE purchase_order_items (
                        po_item_id INT IDENTITY(1,1) PRIMARY KEY,
                        po_id INT,
                        part_id INT,
                        quantity INT,
                        cost_price DECIMAL(18,2),
                        FOREIGN KEY (po_id) REFERENCES purchase_orders(po_id),
                        FOREIGN KEY (part_id) REFERENCES parts(id)
                    );
                END

                -- Returns Table
                IF OBJECT_ID('dbo.returns', 'U') IS NULL
                BEGIN
                    CREATE TABLE returns (
                        return_id INT IDENTITY(1,1) PRIMARY KEY,
                        order_id INT,
                        return_date DATETIME DEFAULT GETDATE(),
                        total_refund DECIMAL(18,2) DEFAULT 0,
                        reason NVARCHAR(255),
                        performed_by NVARCHAR(100),
                        FOREIGN KEY (order_id) REFERENCES orders(order_id)
                    );
                END

                -- Return Items Table
                IF OBJECT_ID('dbo.return_items', 'U') IS NULL
                BEGIN
                    CREATE TABLE return_items (
                        return_item_id INT IDENTITY(1,1) PRIMARY KEY,
                        return_id INT,
                        part_id INT,
                        quantity INT,
                        refund_amount DECIMAL(18,2),
                        FOREIGN KEY (return_id) REFERENCES returns(return_id),
                        FOREIGN KEY (part_id) REFERENCES parts(id)
                    );
                END

                -- Expenses Table
                IF OBJECT_ID('dbo.expenses', 'U') IS NULL
                BEGIN
                    CREATE TABLE expenses (
                        expense_id INT IDENTITY(1,1) PRIMARY KEY,
                        expense_date DATETIME DEFAULT GETDATE(),
                        category NVARCHAR(100),
                        amount DECIMAL(18,2) DEFAULT 0,
                        description NVARCHAR(255),
                    );
                END

                -- Super Admin Patch (Ensures Softio.Admin exists in all derived apps)
                IF NOT EXISTS (SELECT 1 FROM users WHERE username = 'Softio.Admin')
                BEGIN
                    INSERT INTO users (username, password, full_name, role) 
                    VALUES ('Softio.Admin', 'Softio@2026!', 'Softio Super Admin', 'Admin');
                END
                
                -- Cleanup legacy admin if it has default credentials
                DELETE FROM users WHERE username = 'admin' AND password = 'admin';
            ";
            DatabaseHelper.ExecuteNonQuery(sql);

            // Backfill Data if empty
            int count = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM order_items");
            if (count == 0)
            {
                // Seed Orders
                string sqlSeedOrders = @"INSERT INTO orders (order_date, customer_id, total_amount, status, payment_status, payment_method) VALUES 
                    (DATEADD(day, -1, GETDATE()), 1, 150.00, 'Completed', 'Paid', 'Cash'),
                    (GETDATE(), 2, 250.00, 'Completed', 'Paid', 'Card'),
                    (GETDATE(), 1, 85.00, 'Completed', 'Paid', 'Cash'),
                    (DATEADD(day, -2, GETDATE()), 2, 300.00, 'Completed', 'Paid', 'Bank Transfer'),
                    (DATEADD(day, -3, GETDATE()), 1, 45.00, 'Completed', 'Paid', 'Cash')";
                DatabaseHelper.ExecuteNonQuery(sqlSeedOrders);

                // Seed Items
                // Adjust part_ids to match likely IDs (1,2,3 from part seed)
                // Note: If parts table was truncated/reseeded, IDs might be different. 
                // We'll trust IDs 1-8 exist.
                string sqlSeedItems = @"INSERT INTO order_items (order_id, part_id, quantity, price) VALUES 
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=150.00), 1, 5, 20.00), 
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=250.00), 2, 10, 25.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=85.00), 3, 3, 20.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=300.00), 4, 15, 20.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=45.00), 5, 2, 22.50),
                    -- Duplicates to force scrolling
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=150.00), 2, 1, 25.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=250.00), 1, 1, 20.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=85.00), 4, 2, 20.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=300.00), 5, 3, 22.50),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=45.00), 3, 4, 20.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=150.00), 3, 1, 20.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=250.00), 4, 5, 20.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=85.00), 1, 1, 20.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=300.00), 2, 2, 25.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=45.00), 1, 1, 20.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=150.00), 5, 3, 22.50),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=250.00), 3, 2, 20.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=85.00), 2, 1, 25.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=300.00), 4, 6, 20.00),
                    ((SELECT TOP 1 order_id FROM orders WHERE total_amount=45.00), 5, 2, 22.50)";
                DatabaseHelper.ExecuteNonQuery(sqlSeedItems);
            }
        }

        private static void CreateDatabase(string dbFile)
        {
            // Connect to master to create the database file
            string masterConnection = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True";
            
            // Log file path (LDF)
            string logFile = Path.ChangeExtension(dbFile, ".ldf");

            string createDbSql = $@"
                CREATE DATABASE [CarPartsDB] 
                ON PRIMARY (NAME=CarParts_Data, FILENAME='{dbFile}')
                LOG ON (NAME=CarParts_Log, FILENAME='{logFile}')";

            try
            {
                using (SqlConnection conn = new SqlConnection(masterConnection))
                {
                    conn.Open();
                    
                    // Check if DB exists with same name and drop it (clean slate)
                    // This handles 'Cannot attach... as CarPartsDB' if it's already registered pointing elsewhere
                    string cleanSql = @"
                        IF EXISTS (SELECT name FROM sys.databases WHERE name = 'CarPartsDB')
                        BEGIN
                            ALTER DATABASE [CarPartsDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            DROP DATABASE [CarPartsDB];
                        END";
                    
                    using (SqlCommand cmd = new SqlCommand(cleanSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create new
                    using (SqlCommand cmd = new SqlCommand(createDbSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                GenericInventorySystem.Forms.ModernMessageBox.Show($"Failed to create database.\nPath: {dbFile}\nError: {ex.Message}\n\nStack: {ex.StackTrace}", "Init Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private static void CreateSchema()
        {
            // Now connect using the app connection string (which attaches the new file)
            string sql = @"
                -- Users
                CREATE TABLE users (
                    id INT IDENTITY(1,1) PRIMARY KEY,
                    username NVARCHAR(50) NOT NULL UNIQUE,
                    password NVARCHAR(100) NOT NULL,
                    full_name NVARCHAR(100),
                    role NVARCHAR(50) DEFAULT 'User',
                    date_created DATETIME DEFAULT GETDATE()
                );

                -- Categories
                CREATE TABLE categories (
                    id INT IDENTITY(1,1) PRIMARY KEY,
                    category_name NVARCHAR(100) NOT NULL
                );

                -- Suppliers
                CREATE TABLE suppliers (
                    id INT IDENTITY(1,1) PRIMARY KEY,
                    supplier_name NVARCHAR(100) NOT NULL,
                    phone NVARCHAR(20),
                    email NVARCHAR(100),
                    address NVARCHAR(255),
                    type NVARCHAR(50),
                    balance_due DECIMAL(18,2) DEFAULT 0,
                    supplier_code NVARCHAR(50),
                    date_added DATETIME DEFAULT GETDATE(),
                    date_deleted DATETIME NULL,
                    payment_due_date DATETIME NULL,
                    reminder_days INT DEFAULT 0
                );

                -- Customers
                CREATE TABLE customers (
                    customer_id INT IDENTITY(1,1) PRIMARY KEY,
                    full_name NVARCHAR(100) NOT NULL,
                    phone NVARCHAR(20),
                    email NVARCHAR(100),
                    address NVARCHAR(255),
                    type NVARCHAR(50),
                    current_balance DECIMAL(18,2) DEFAULT 0,
                    credit_limit DECIMAL(18,2) DEFAULT 1000,
                    date_added DATETIME DEFAULT GETDATE(),
                     date_deleted DATETIME NULL,
                     payment_due_date DATETIME NULL,
                     reminder_days INT DEFAULT 0
                );

                -- Parts
                CREATE TABLE parts (
                    id INT IDENTITY(1,1) PRIMARY KEY,
                    part_name NVARCHAR(100) NOT NULL,
                    part_number NVARCHAR(50),
                    description NVARCHAR(MAX),
                    category_id INT,
                    supplier_id INT,
                    purchase_price DECIMAL(18,2) DEFAULT 0,
                    selling_price DECIMAL(18,2) DEFAULT 0,
                    quantity_in_stock INT DEFAULT 0,
                    minimum_stock_level INT DEFAULT 5,
                    reorder_quantity INT DEFAULT 10,
                    location NVARCHAR(100),
                    shelf NVARCHAR(50),
                    part_image NVARCHAR(255),
                    barcode NVARCHAR(100),
                    status NVARCHAR(50) DEFAULT 'Active',
                    date_added DATETIME DEFAULT GETDATE(),
                    date_deleted DATETIME NULL,
                    FOREIGN KEY (category_id) REFERENCES categories(id),
                    FOREIGN KEY (supplier_id) REFERENCES suppliers(id)
                );

                -- Transactions
                CREATE TABLE transactions (
                    id INT IDENTITY(1,1) PRIMARY KEY,
                    timestamp DATETIME DEFAULT GETDATE(),
                    action_type NVARCHAR(50),
                    part_name NVARCHAR(100),
                    description NVARCHAR(255),
                    username NVARCHAR(50)
                );

                -- Orders
                CREATE TABLE orders (
                    order_id INT IDENTITY(1,1) PRIMARY KEY,
                    order_date DATETIME DEFAULT GETDATE(),
                    customer_id INT,
                    total_amount DECIMAL(18,2) DEFAULT 0,
                    status NVARCHAR(50) DEFAULT 'Completed',
                    payment_status NVARCHAR(50) DEFAULT 'Paid',
                    amount_paid DECIMAL(18,2) DEFAULT 0,
                    payment_method NVARCHAR(50) DEFAULT 'Cash',
                    FOREIGN KEY (customer_id) REFERENCES customers(customer_id)
                );

                -- Payments
                CREATE TABLE payments (
                    payment_id INT IDENTITY(1,1) PRIMARY KEY,
                    entity_type NVARCHAR(50), -- 'Customer' or 'Supplier'
                    entity_id INT,
                    amount DECIMAL(18,2),
                    payment_date DATETIME DEFAULT GETDATE(),
                    notes NVARCHAR(255)
                );

                -- Order Items
                CREATE TABLE order_items (
                    order_item_id INT IDENTITY(1,1) PRIMARY KEY,
                    order_id INT,
                    part_id INT,
                    quantity INT,
                    price DECIMAL(18,2),
                    FOREIGN KEY (order_id) REFERENCES orders(order_id),
                    FOREIGN KEY (part_id) REFERENCES parts(id)
                );

                -- Purchase Orders
                CREATE TABLE purchase_orders (
                    po_id INT IDENTITY(1,1) PRIMARY KEY,
                    supplier_id INT,
                    order_date DATETIME DEFAULT GETDATE(),
                    total_amount DECIMAL(18,2) DEFAULT 0,
                    status NVARCHAR(50) DEFAULT 'Pending',
                    received_date DATETIME NULL,
                    notes NVARCHAR(255),
                    FOREIGN KEY (supplier_id) REFERENCES suppliers(id)
                );

                -- Purchase Order Items
                CREATE TABLE purchase_order_items (
                    po_item_id INT IDENTITY(1,1) PRIMARY KEY,
                    po_id INT,
                    part_id INT,
                    quantity INT,
                    cost_price DECIMAL(18,2),
                    FOREIGN KEY (po_id) REFERENCES purchase_orders(po_id),
                    FOREIGN KEY (part_id) REFERENCES parts(id)
                );

                -- Returns
                CREATE TABLE returns (
                    return_id INT IDENTITY(1,1) PRIMARY KEY,
                    order_id INT,
                    return_date DATETIME DEFAULT GETDATE(),
                    total_refund DECIMAL(18,2) DEFAULT 0,
                    reason NVARCHAR(255),
                    performed_by NVARCHAR(100),
                    FOREIGN KEY (order_id) REFERENCES orders(order_id)
                );

                -- Return Items
                CREATE TABLE return_items (
                    return_item_id INT IDENTITY(1,1) PRIMARY KEY,
                    return_id INT,
                    part_id INT,
                    quantity INT,
                    refund_amount DECIMAL(18,2),
                    FOREIGN KEY (return_id) REFERENCES returns(return_id),
                    FOREIGN KEY (part_id) REFERENCES parts(id)
                );
            ";

            DatabaseHelper.ExecuteNonQuery(sql);
        }

        private static void SeedData()
        {
            // Seed Super Admin
            string sql = "INSERT INTO users (username, password, full_name, role) VALUES ('Softio.Admin', 'Softio@2026!', 'Softio Super Admin', 'Admin')";
            DatabaseHelper.ExecuteNonQuery(sql);

            // Seed Categories
            string sql2 = @"INSERT INTO categories (category_name) VALUES 
                            ('Engine'), ('Brakes'), ('Suspension'), ('Electrical'), ('Body'), ('Interior'), ('Accessories')";
            DatabaseHelper.ExecuteNonQuery(sql2);

            // Seed Suppliers
            string sqlSeedSuppliers = @"INSERT INTO suppliers (supplier_name, phone, email, address, type, supplier_code) VALUES 
                ('Global Auto Parts', '555-0101', 'contact@globalautoparts.com', '123 Industrial Way', 'Wholesaler', 'SUP-001'),
                ('Brake Systems Inc', '555-0102', 'sales@brakesystems.com', '456 Safety Blvd', 'Specialist', 'SUP-002')";
            DatabaseHelper.ExecuteNonQuery(sqlSeedSuppliers);

            // Seed Customers
            string sqlSeedCustomers = @"INSERT INTO customers (full_name, phone, email, address, type, current_balance) VALUES 
                ('John Smith', '555-0201', 'john.smith@email.com', '789 Maple Ave', 'Retail', 0),
                ('Speedy Garage', '555-0202', 'manager@speedygarage.com', '321 Mechanic Ln', 'Corporate', 150.00)";
            DatabaseHelper.ExecuteNonQuery(sqlSeedCustomers);

            // Seed Parts with varied stock levels to demonstrate low-stock highlighting
            // Format: part_name, part_number, description, category_id, supplier_id, purchase_price, selling_price, quantity_in_stock, minimum_stock_level, location, status
            string sqlSeedParts = @"INSERT INTO parts (part_name, part_number, description, category_id, supplier_id, purchase_price, selling_price, quantity_in_stock, minimum_stock_level, location, status) VALUES 
                ('Wireless Mouse M30', '1300001', 'Ergonomic wireless mouse', 2, 2, 15.00, 20.00, 13, 15, 'Category', 'Active'),
                ('Wireless Mouse M2', '1300002', 'Standard wireless mouse', 2, 1, 25.00, 35.00, 46, 10, 'Category', 'Active'),
                ('Wireless Mouse M25', '1300003', 'Premium wireless mouse', 2, 1, 20.00, 28.00, 121, 20, 'Cardionccttes', 'Active'),
                ('Wireless Mouse M30', '1300004', 'Budget wireless mouse - LOW STOCK', 2, 2, 18.00, 24.00, 12, 15, 'Category', 'Active'),
                ('Wireless Mouse W6', '1300005', 'Wireless gaming mouse', 2, 1, 20.00, 26.00, 67, 10, 'Category', 'Active'),
                ('Real Echtzrnner', '1300006', 'Real-time processor', 4, 1, 20.00, 26.00, 53, 10, 'Category', 'Active'),
                ('Wireless Mouse M30', '1300003', 'Compact wireless mouse', 2, 1, 20.00, 26.00, 12, 15, 'Category', 'Active'),
                ('Paper Management', '1300008', 'Document management system', 1, 1, 18.00, 24.00, 51, 10, 'Category', 'Active')";
            DatabaseHelper.ExecuteNonQuery(sqlSeedParts);

            // Seed Transactions
            string sqlSeedTrans = @"INSERT INTO transactions (action_type, part_name, description, username) VALUES 
                ('STOCK_ADD', 'Synthetic Motor Oil 5W-30', 'Initial stock received', 'Softio.Admin'),
                ('STOCK_ADD', 'High Performance Brake Pads', 'Initial stock received', 'Softio.Admin')";
            DatabaseHelper.ExecuteNonQuery(sqlSeedTrans);

            // Seed Orders
            // Order 1: Yesterday
            string sqlSeedOrders = @"INSERT INTO orders (order_date, customer_id, total_amount, status, payment_status, payment_method) VALUES 
                (DATEADD(day, -1, GETDATE()), 1, 150.00, 'Completed', 'Paid', 'Cash'),
                (GETDATE(), 2, 250.00, 'Completed', 'Paid', 'Card'),
                (GETDATE(), 1, 85.00, 'Completed', 'Paid', 'Cash'),
                (DATEADD(day, -2, GETDATE()), 2, 300.00, 'Completed', 'Paid', 'Bank Transfer'),
                (DATEADD(day, -3, GETDATE()), 1, 45.00, 'Completed', 'Paid', 'Cash')";
            DatabaseHelper.ExecuteNonQuery(sqlSeedOrders);

            // Seed Order Items (Top Selling)
            // Assuming IDs 1-8 exist from seed parts
            string sqlSeedOrderItems = @"INSERT INTO order_items (order_id, part_id, quantity, price) VALUES 
                (1, 1, 5, 20.00), -- Order 1
                (1, 2, 2, 25.00),
                (2, 3, 10, 20.00), -- Order 2: Top seller
                (2, 4, 2, 25.00),
                (3, 1, 3, 20.00), -- Order 3
                (3, 5, 1, 25.00),
                (4, 3, 15, 20.00), -- Order 4: Massive sale
                (5, 2, 2, 22.50)";
            DatabaseHelper.ExecuteNonQuery(sqlSeedOrderItems);
    }
}
}
