using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Shaheen_InventoryManagement_Android.Helpers;

namespace Shaheen_InventoryManagement_Android.Data
{
    public class RecipePartData
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int PartId { get; set; }
        public double Quantity { get; set; }
        
        // Joined fields from Parts table
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public decimal UnitCost { get; set; }
        public string UnitOfMeasure { get; set; }
        
        public decimal TotalCost => UnitCost * (decimal)Quantity;

        public static List<RecipePartData> GetPartsForRecipe(int recipeId)
        {
            string sql = @"SELECT rp.*, p.part_name, p.part_number, p.purchase_price, p.unit_of_measure
                           FROM recipe_parts rp
                           JOIN parts p ON rp.part_id = p.id
                           WHERE rp.recipe_id = @id";
            return DatabaseHelper.ExecuteQuery(sql, MapFromReader, new SqliteParameter("@id", recipeId));
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

        private static RecipePartData MapFromReader(SqliteDataReader r)
        {
            return new RecipePartData
            {
                Id         = r.GetInt32(r.GetOrdinal("id")),
                RecipeId   = r.GetInt32(r.GetOrdinal("recipe_id")),
                PartId     = r.GetInt32(r.GetOrdinal("part_id")),
                Quantity   = Safe<double>(r, "quantity", 1),
                PartName   = Safe<string>(r, "part_name", ""),
                PartNumber = Safe<string>(r, "part_number", ""),
                UnitCost   = Safe<decimal>(r, "purchase_price", 0),
                UnitOfMeasure = Safe<string>(r, "unit_of_measure", "")
            };
        }
    }
}
