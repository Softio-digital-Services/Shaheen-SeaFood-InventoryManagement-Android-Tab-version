using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace Shaheen_InventoryManagement_Android.Data
{
    public class PartData
    {
        public int Id { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public double QuantityInStock { get; set; }
        public int MinimumStockLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public string Location { get; set; }
        public string Shelf { get; set; }
        public string PartImage { get; set; }
        public string Barcode { get; set; }
        public string Status { get; set; }
        public DateTime DateAdded { get; set; }
        public string ItemNo { get; set; }

        public string ItemType { get; set; } = "Product";
        public string UnitOfMeasure { get; set; }
        public string BatchNumber { get; set; }
        public string ExpiryDate { get; set; }
        public bool IsSalesItem { get; set; } = true;
        public bool IsPurchaseItem { get; set; } = false;
        public bool IsInactive { get; set; } = false;
        public decimal TaxRate { get; set; } = 0;
        public bool IsStockTracked { get; set; } = true;
        public decimal Price2 { get; set; } = 0;
        public decimal Price3 { get; set; } = 0;
        public decimal Price4 { get; set; } = 0;

        public string StockType { get; set; } = "Piece";
        public int PackItemsNumber { get; set; } = 0;
        public decimal PackPrice { get; set; } = 0;
        public decimal ItemPrice { get; set; } = 0;
        public decimal PiecePrice { get; set; } = 0;
        public string BigUnit { get; set; }
        public string SmallUnit { get; set; }
        public double ConversionValue { get; set; } = 1.0;
        public double PackSize { get; set; } = 1.0;

        public static List<PartData> GetAllParts(string categoryName = null, int limit = 0, int offset = 0)
        {
            string sql = @"SELECT p.*, c.category_name, s.supplier_name
                           FROM parts p
                           LEFT JOIN categories c ON p.category_id = c.id
                           LEFT JOIN suppliers  s ON p.supplier_id = s.id
                           WHERE p.date_deleted IS NULL";
            if (!string.IsNullOrEmpty(categoryName))
            {
                if (categoryName == "Others") sql += " AND (c.category_name IS NULL OR c.category_name = '')";
                else sql += " AND c.category_name = @cat";
            }
            sql += " ORDER BY p.part_name";
            if (limit > 0) sql += $" LIMIT {limit} OFFSET {offset}";
            return DatabaseHelper.ExecuteQuery(sql, MapFromReader, new SqliteParameter("@cat", categoryName));
        }

        public static int GetAllPartsCount(string categoryName = null)
        {
            string sql = "SELECT COUNT(*) FROM parts p LEFT JOIN categories c ON p.category_id = c.id WHERE p.date_deleted IS NULL";
            if (!string.IsNullOrEmpty(categoryName))
            {
                if (categoryName == "Others") sql += " AND (c.category_name IS NULL OR c.category_name = '')";
                else sql += " AND c.category_name = @cat";
            }
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar<object>(sql, new SqliteParameter("@cat", categoryName)) ?? 0);
        }

        public static List<PartData> GetLowStockParts()
        {
            string sql = @"SELECT p.*, c.category_name, s.supplier_name
                           FROM parts p
                           LEFT JOIN categories c ON p.category_id = c.id
                           LEFT JOIN suppliers  s ON p.supplier_id = s.id
                           WHERE p.quantity_in_stock <= p.minimum_stock_level
                           AND p.date_deleted IS NULL
                           ORDER BY p.quantity_in_stock";
            return DatabaseHelper.ExecuteQuery(sql, MapFromReader);
        }

        public static List<PartData> SearchParts(string keyword, string categoryName = null, int limit = 0, int offset = 0)
        {
            string sql = @"SELECT p.*, c.category_name, s.supplier_name
                           FROM parts p
                            LEFT JOIN categories c ON p.category_id = c.id
                            LEFT JOIN suppliers  s ON p.supplier_id = s.id
                            WHERE (p.part_number LIKE @kw OR p.part_name LIKE @kw OR p.item_no LIKE @kw)
                            AND p.date_deleted IS NULL";
            if (!string.IsNullOrEmpty(categoryName))
            {
                if (categoryName == "Others") sql += " AND (c.category_name IS NULL OR c.category_name = '')";
                else sql += " AND c.category_name = @cat";
            }
            sql += " ORDER BY p.part_name";
            if (limit > 0) sql += $" LIMIT {limit} OFFSET {offset}";
            return DatabaseHelper.ExecuteQuery(sql, MapFromReader,
                new SqliteParameter("@kw", "%" + keyword + "%"), new SqliteParameter("@cat", categoryName));
        }

        public static int SearchPartsCount(string keyword, string categoryName = null)
        {
            string sql = "SELECT COUNT(*) FROM parts p LEFT JOIN categories c ON p.category_id = c.id WHERE (p.part_number LIKE @kw OR p.part_name LIKE @kw OR p.item_no LIKE @kw) AND p.date_deleted IS NULL";
            if (!string.IsNullOrEmpty(categoryName))
            {
                if (categoryName == "Others") sql += " AND (c.category_name IS NULL OR c.category_name = '')";
                else sql += " AND c.category_name = @cat";
            }
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar<object>(sql, new SqliteParameter("@kw", "%" + keyword + "%"), new SqliteParameter("@cat", categoryName)) ?? 0);
        }

        private static T Safe<T>(SqliteDataReader r, string col, T fallback = default)
        {
            try
            {
                int ord = r.GetOrdinal(col);
                if (r.IsDBNull(ord)) return fallback;
                object v = r.GetValue(ord);
                return (T)Convert.ChangeType(v, typeof(T));
            }
            catch { return fallback; }
        }

        private static PartData MapFromReader(SqliteDataReader r)
        {
            var p = new PartData
            {
                Id = r.GetInt32(r.GetOrdinal("id")),
                PartNumber = Safe<string>(r, "part_number", ""),
                ItemNo = Safe<string>(r, "item_no", ""),
                PartName = Safe<string>(r, "part_name", ""),
                Description = Safe<string>(r, "description", ""),
                CategoryId = Safe<int>(r, "category_id", 0),
                CategoryName = Safe<string>(r, "category_name", ""),
                SupplierId = r.IsDBNull(r.GetOrdinal("supplier_id")) ? (int?)null : r.GetInt32(r.GetOrdinal("supplier_id")),
                SupplierName = Safe<string>(r, "supplier_name", ""),
                PurchasePrice = Safe<decimal>(r, "purchase_price", 0),
                SellingPrice = Safe<decimal>(r, "selling_price", 0),
                QuantityInStock = Safe<double>(r, "quantity_in_stock", 0.0),
                MinimumStockLevel = Safe<int>(r, "minimum_stock_level", 0),
                ReorderQuantity = Safe<int>(r, "reorder_quantity", 0),
                Location = Safe<string>(r, "location", ""),
                Shelf = Safe<string>(r, "shelf", ""),
                PartImage = Safe<string>(r, "part_image", ""),
                Barcode = Safe<string>(r, "barcode", ""),
                Status = Safe<string>(r, "status", "Active"),
                DateAdded = DateTime.TryParse(Safe<string>(r, "date_added", ""), out DateTime da) ? da : DateTime.Now,
                ItemType = Safe<string>(r, "item_type", "Product"),
                UnitOfMeasure = Safe<string>(r, "unit_of_measure", ""),
                BatchNumber = Safe<string>(r, "batch_number", ""),
                ExpiryDate = Safe<string>(r, "expiry_date", ""),
                IsSalesItem = Safe<int>(r, "is_sales_item", 1) == 1,
                IsPurchaseItem = Safe<int>(r, "is_purchase_item", 0) == 1,
                IsInactive = Safe<int>(r, "is_inactive", 0) == 1,
                TaxRate = Safe<decimal>(r, "tax_rate", 0),
                IsStockTracked = Safe<int>(r, "is_stock_tracked", 1) == 1,
                Price2 = Safe<decimal>(r, "price2", 0),
                Price3 = Safe<decimal>(r, "price3", 0),
                Price4 = Safe<decimal>(r, "price4", 0),
                StockType = Safe<string>(r, "stock_type", "Piece"),
                PackItemsNumber = Safe<int>(r, "pack_items_number", 0),
                PackPrice = Safe<decimal>(r, "pack_price", 0),
                ItemPrice = Safe<decimal>(r, "item_price", 0),
                PiecePrice = Safe<decimal>(r, "piece_price", 0),
                BigUnit = Safe<string>(r, "big_unit", ""),
                SmallUnit = Safe<string>(r, "small_unit", ""),
                ConversionValue = Safe<double>(r, "conversion_value", 1.0),
                PackSize = Safe<double>(r, "pack_size", 1.0)
            };

            if (!Helpers.UserSession.IsAdmin)
            {
                p.PurchasePrice = 0m;
                p.SellingPrice = 0m;
                p.PackPrice = 0m;
                p.ItemPrice = 0m;
                p.PiecePrice = 0m;
                p.Price2 = 0m;
                p.Price3 = 0m;
                p.Price4 = 0m;
            }

            return p;
        }
    }
}
