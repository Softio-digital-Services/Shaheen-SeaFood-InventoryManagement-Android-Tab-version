-- ============================================
-- Car Parts Inventory Management System
-- Database Schema
-- Transform from School Management System
-- ============================================

-- Categories Table
CREATE TABLE categories (
    id INT IDENTITY(1,1) PRIMARY KEY,
    category_name NVARCHAR(100) NOT NULL UNIQUE,
    description NVARCHAR(500) NULL,
    date_created DATETIME DEFAULT GETDATE()
);

-- Sample Categories
INSERT INTO categories (category_name, description) VALUES
('Engine Parts', 'Engine components and accessories'),
('Brake System', 'Brake pads, rotors, calipers, brake lines'),
('Suspension', 'Shocks, struts, control arms, bushings'),
('Electrical', 'Batteries, alternators, starters, sensors'),
('Body Parts', 'Bumpers, fenders, doors, hoods'),
('Filters', 'Oil filters, air filters, fuel filters, cabin filters'),
('Fluids & Lubricants', 'Engine oil, coolant, brake fluid, transmission fluid'),
('Accessories', 'Floor mats, seat covers, tools, cleaning products');

-- Suppliers Table
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

-- Sample Suppliers
INSERT INTO suppliers (supplier_code, supplier_name, contact_person, phone, email, status) VALUES
('SUP001', 'AutoParts Wholesale Inc', 'John Smith', '555-0100', 'john@autoparts.com', 'Active'),
('SUP002', 'Quality Motors Supply', 'Sarah Johnson', '555-0200', 'sarah@qualitymotors.com', 'Active'),
('SUP003', 'Global Auto Distributors', 'Mike Chen', '555-0300', 'mike@globalauto.com', 'Active');

-- Parts Table (Main Inventory)
CREATE TABLE parts (
    id INT IDENTITY(1,1) PRIMARY KEY,
    part_number NVARCHAR(50) NOT NULL UNIQUE,
    part_name NVARCHAR(200) NOT NULL,
    description NVARCHAR(MAX) NULL,
    category_id INT NOT NULL,
    supplier_id INT NULL,
    
    -- Pricing
    purchase_price DECIMAL(10,2) NOT NULL,
    selling_price DECIMAL(10,2) NOT NULL,
    
    -- Inventory
    quantity_in_stock INT NOT NULL DEFAULT 0,
    minimum_stock_level INT NOT NULL DEFAULT 10,
    reorder_quantity INT NOT NULL DEFAULT 50,
    
    -- Details
    location NVARCHAR(100) NULL,
    part_image NVARCHAR(500) NULL,
    barcode NVARCHAR(100) NULL,
    
    -- Status
    status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    date_added DATETIME DEFAULT GETDATE(),
    date_updated DATETIME NULL,
    date_deleted DATETIME NULL,
    
    FOREIGN KEY (category_id) REFERENCES categories(id),
    FOREIGN KEY (supplier_id) REFERENCES suppliers(id)
);

-- Sample Parts
INSERT INTO parts (part_number, part_name, description, category_id, supplier_id, purchase_price, selling_price, quantity_in_stock, minimum_stock_level, location, status)
VALUES 
('BRK-001', 'Brake Pad Set - Front', 'Ceramic brake pads for front wheels, fits most sedans', 2, 1, 35.00, 65.00, 25, 10, 'A-12', 'Active'),
('BRK-002', 'Brake Rotor - Front', 'Vented brake rotor, 12 inch diameter', 2, 1, 45.00, 85.00, 18, 8, 'A-14', 'Active'),
('OIL-001', 'Engine Oil 5W-30', 'Synthetic motor oil 5W-30, 5 liter bottle', 7, 2, 18.00, 35.00, 50, 20, 'C-05', 'Active'),
('OIL-002', 'Engine Oil 10W-40', 'Conventional motor oil 10W-40, 5 liter bottle', 7, 2, 15.00, 28.00, 45, 20, 'C-06', 'Active'),
('FLT-001', 'Air Filter', 'High-performance air filter, universal fit', 6, 1, 12.00, 25.00, 30, 15, 'B-08', 'Active'),
('FLT-002', 'Oil Filter', 'Standard oil filter, fits most engines', 6, 1, 8.00, 16.00, 60, 25, 'B-10', 'Active'),
('FLT-003', 'Fuel Filter', 'Inline fuel filter', 6, 1, 10.00, 22.00, 35, 15, 'B-12', 'Active'),
('ENG-001', 'Spark Plugs Set (4pc)', 'Iridium spark plugs, set of 4', 1, 3, 25.00, 48.00, 40, 12, 'D-02', 'Active'),
('ENG-002', 'Timing Belt', 'Rubber timing belt with teeth', 1, 3, 55.00, 98.00, 15, 8, 'D-05', 'Active'),
('SUS-001', 'Shock Absorber - Front', 'Gas-filled shock absorber', 3, 2, 75.00, 135.00, 12, 6, 'E-10', 'Active'),
('SUS-002', 'Control Arm Bushing', 'Polyurethane bushing kit', 3, 2, 20.00, 38.00, 25, 10, 'E-15', 'Active'),
('ELC-001', 'Car Battery 12V', '12-volt lead-acid battery, 60Ah', 4, 3, 85.00, 145.00, 8, 5, 'F-01', 'Active'),
('ELC-002', 'Alternator', 'OEM replacement alternator, 120A', 4, 3, 120.00, 225.00, 6, 3, 'F-08', 'Active'),
('ACC-001', 'Floor Mats Set', 'All-weather floor mats, 4-piece', 8, 2, 22.00, 45.00, 35, 10, 'G-05', 'Active'),
('ACC-002', 'Windshield Wipers', 'Premium wiper blades, pair', 8, 1, 18.00, 32.00, 40, 15, 'G-12', 'Active');

-- Stock Movements Table
CREATE TABLE stock_movements (
    id INT IDENTITY(1,1) PRIMARY KEY,
    part_id INT NOT NULL,
    movement_type NVARCHAR(20) NOT NULL,  -- 'IN', 'OUT', 'ADJUSTMENT', 'RETURN'
    quantity INT NOT NULL,
    
    -- Reference
    reference_type NVARCHAR(50) NULL,
    reference_id INT NULL,
    
    notes NVARCHAR(500) NULL,
    performed_by NVARCHAR(100) NOT NULL,
    movement_date DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (part_id) REFERENCES parts(id)
);

-- Users Table
CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(50) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL,
    full_name NVARCHAR(100) NOT NULL,
    role NVARCHAR(20) NOT NULL DEFAULT 'Staff',
    status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    date_created DATETIME DEFAULT GETDATE()
);

-- Insert default admin user
INSERT INTO users (username, password, full_name, role) 
VALUES ('admin', 'admin', 'System Administrator', 'Admin');

-- Create Indexes
CREATE INDEX idx_parts_number ON parts(part_number);
CREATE INDEX idx_parts_name ON parts(part_name);
CREATE INDEX idx_parts_stock ON parts(quantity_in_stock);
CREATE INDEX idx_parts_category ON parts(category_id);
CREATE INDEX idx_parts_supplier ON parts(supplier_id);
CREATE INDEX idx_movements_part ON stock_movements(part_id);
CREATE INDEX idx_movements_date ON stock_movements(movement_date);
CREATE INDEX idx_suppliers_code ON suppliers(supplier_code);

-- Sample Stock Movements
INSERT INTO stock_movements (part_id, movement_type, quantity, reference_type, notes, performed_by)
VALUES 
(1, 'IN', 25, 'PURCHASE', 'Initial stock', 'admin'),
(2, 'IN', 18, 'PURCHASE', 'Initial stock', 'admin'),
(3, 'IN', 50, 'PURCHASE', 'Initial stock', 'admin'),
(4, 'IN', 45, 'PURCHASE', 'Initial stock', 'admin'),
(5, 'IN', 30, 'PURCHASE', 'Initial stock', 'admin');

PRINT 'Car Parts Inventory Database created successfully!';
PRINT 'Login: admin/admin';
PRINT 'Sample data: 15 parts, 8 categories, 3 suppliers';
