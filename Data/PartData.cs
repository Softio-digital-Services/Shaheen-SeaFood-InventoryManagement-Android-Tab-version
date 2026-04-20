using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace GenericInventorySystem.Data
{
    public class PartData
    {
        // Properties
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
        public int QuantityInStock { get; set; }
        public int MinimumStockLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public string Location { get; set; }
        public string Shelf { get; set; }
        public string PartImage { get; set; }
        public string Barcode { get; set; }
        public string Status { get; set; }
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Get all active parts
        /// </summary>
        public static List<PartData> GetAllParts()
        {
            string sql = @"SELECT p.*, c.category_name, s.supplier_name 
                          FROM parts p
                          LEFT JOIN categories c ON p.category_id = c.id
                          LEFT JOIN suppliers s ON p.supplier_id = s.id
                          WHERE p.date_deleted IS NULL
                          ORDER BY p.part_name";

            return DatabaseHelper.ExecuteQuery(sql, MapFromReader);
        }

        /// <summary>
        /// Get parts below minimum stock level
        /// </summary>
        public static List<PartData> GetLowStockParts()
        {
            string sql = @"SELECT p.*, c.category_name, s.supplier_name 
                          FROM parts p
                          LEFT JOIN categories c ON p.category_id = c.id
                          LEFT JOIN suppliers s ON p.supplier_id = s.id
                          WHERE p.quantity_in_stock <= p.minimum_stock_level 
                          AND p.date_deleted IS NULL
                          ORDER BY p.quantity_in_stock";

            return DatabaseHelper.ExecuteQuery(sql, MapFromReader);
        }

        /// <summary>
        /// Search parts by keyword
        /// </summary>
        public static List<PartData> SearchParts(string keyword)
        {
            string sql = @"SELECT p.*, c.category_name, s.supplier_name 
                          FROM parts p
                          LEFT JOIN categories c ON p.category_id = c.id
                          LEFT JOIN suppliers s ON p.supplier_id = s.id
                          WHERE (p.part_number LIKE @keyword OR p.part_name LIKE @keyword)
                          AND p.date_deleted IS NULL
                          ORDER BY p.part_name";

            var parameters = new SqlParameter[] 
            { 
                new SqlParameter("@keyword", "%" + keyword + "%") 
            };

            return DatabaseHelper.ExecuteQuery(sql, MapFromReader, parameters);
        }

        /// <summary>
        /// Map database reader to PartData object
        /// </summary>
        private static PartData MapFromReader(SqlDataReader reader)
        {
            return new PartData
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                PartNumber = reader.GetString(reader.GetOrdinal("part_number")),
                PartName = reader.GetString(reader.GetOrdinal("part_name")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString(reader.GetOrdinal("description")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("category_id")),
                CategoryName = reader.IsDBNull(reader.GetOrdinal("category_name")) ? "" : reader.GetString(reader.GetOrdinal("category_name")),
                SupplierId = reader.IsDBNull(reader.GetOrdinal("supplier_id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("supplier_id")),
                SupplierName = reader.IsDBNull(reader.GetOrdinal("supplier_name")) ? "" : reader.GetString(reader.GetOrdinal("supplier_name")),
                PurchasePrice = reader.GetDecimal(reader.GetOrdinal("purchase_price")),
                SellingPrice = reader.GetDecimal(reader.GetOrdinal("selling_price")),
                QuantityInStock = reader.GetInt32(reader.GetOrdinal("quantity_in_stock")),
                MinimumStockLevel = reader.GetInt32(reader.GetOrdinal("minimum_stock_level")),
                ReorderQuantity = reader.GetInt32(reader.GetOrdinal("reorder_quantity")),
                Location = reader.IsDBNull(reader.GetOrdinal("location")) ? "" : reader.GetString(reader.GetOrdinal("location")),
                Shelf = reader.IsDBNull(reader.GetOrdinal("shelf")) ? "" : reader.GetString(reader.GetOrdinal("shelf")),
                PartImage = reader.IsDBNull(reader.GetOrdinal("part_image")) ? "" : reader.GetString(reader.GetOrdinal("part_image")),
                Barcode = reader.IsDBNull(reader.GetOrdinal("barcode")) ? "" : reader.GetString(reader.GetOrdinal("barcode")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                DateAdded = reader.GetDateTime(reader.GetOrdinal("date_added"))
            };
        }
    }
}
