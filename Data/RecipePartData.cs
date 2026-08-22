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
        
        // Custom unit properties
        public string StockType { get; set; }
        public int PackItemsNumber { get; set; }
        public decimal PackPrice { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal PiecePrice { get; set; }
        public string BigUnit { get; set; }
        public string SmallUnit { get; set; }
        public double ConversionValue { get; set; }
        public double PackSize { get; set; }
        public string PartUom { get; set; }

        public decimal CalculatedUnitCost
        {
            get
            {
                if (!Helpers.UserSession.IsAdmin) return 0m;
                return Helpers.IngredientCalculationEngine.CalculatedUnitCost(
                    UnitCost, UnitOfMeasure, BigUnit, SmallUnit, ConversionValue, PackSize, StockType, PackItemsNumber, PartUom);
            }
        }

        public decimal TotalCost => CalculatedUnitCost * (decimal)Quantity;

        public static List<RecipePartData> GetPartsForRecipe(int recipeId)
        {
            string sql = @"SELECT rp.*, p.part_name, p.part_number, p.purchase_price, p.unit_of_measure as part_uom,
                                  p.stock_type, p.pack_items_number, p.pack_price, p.item_price, p.piece_price,
                                  p.big_unit, p.small_unit, p.conversion_value, p.pack_size
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
            decimal purchasePrice = Safe<decimal>(r, "purchase_price", 0);
            decimal packPrice = Safe<decimal>(r, "pack_price", 0);

            var item = new RecipePartData
            {
                Id         = r.GetInt32(r.GetOrdinal("id")),
                RecipeId   = r.GetInt32(r.GetOrdinal("recipe_id")),
                PartId     = r.GetInt32(r.GetOrdinal("part_id")),
                Quantity   = Safe<double>(r, "quantity", 1),
                PartName   = Safe<string>(r, "part_name", ""),
                PartNumber = Safe<string>(r, "part_number", ""),
                UnitCost   = packPrice > 0 ? packPrice : purchasePrice,
                UnitOfMeasure = Safe<string>(r, "unit_of_measure", ""),
                
                // Custom unit mappings
                StockType  = Safe<string>(r, "stock_type", "Piece"),
                PackItemsNumber = Safe<int>(r, "pack_items_number", 0),
                PackPrice  = packPrice,
                ItemPrice  = Safe<decimal>(r, "item_price", 0),
                PiecePrice = Safe<decimal>(r, "piece_price", 0),
                BigUnit    = Safe<string>(r, "big_unit", ""),
                SmallUnit  = Safe<string>(r, "small_unit", ""),
                ConversionValue = Safe<double>(r, "conversion_value", 1.0),
                PackSize   = Safe<double>(r, "pack_size", 1.0),
                PartUom    = Safe<string>(r, "part_uom", "")
            };

            if (!Helpers.UserSession.IsAdmin)
            {
                item.UnitCost = 0m;
                item.PackPrice = 0m;
                item.ItemPrice = 0m;
                item.PiecePrice = 0m;
            }

            return item;
        }

        public static double GetConvertedQuantityDynamic(double qty, string recipeUom, string partUom, string stockType, int packItems, string bigUnit, string smallUnit, double convVal, double packSize)
        {
            if (!string.IsNullOrEmpty(bigUnit) && !string.IsNullOrEmpty(smallUnit))
            {
                double conv = convVal > 0 ? convVal : 1.0;
                double pSize = packSize > 0 ? packSize : 1.0;
                string rUom = (recipeUom ?? "").ToLower().Trim();
                string sUnit = smallUnit.ToLower().Trim();
                string bUnit = bigUnit.ToLower().Trim();

                bool isBigPack = bUnit == "pack" || bUnit == "package";
                double effectivePSize = isBigPack ? 1.0 : pSize;

                if (bUnit == sUnit)
                {
                    if (rUom == "pack") return qty;
                    return qty / effectivePSize;
                }

                if (rUom == sUnit)
                {
                    return qty / (effectivePSize * conv);
                }
                else if (rUom == bUnit)
                {
                    return qty / effectivePSize;
                }
                else if (rUom == "pack")
                {
                    return qty;
                }
                else
                {
                    return qty / (effectivePSize * conv);
                }
            }
            return GetConvertedQuantity(qty, recipeUom, partUom, stockType, packItems);
        }

        public static double GetConvertedQuantity(double qty, string recipeUom, string partUom, string stockType, int packItems)
        {
            if (string.IsNullOrEmpty(recipeUom)) recipeUom = "pcs";
            if (string.IsNullOrEmpty(partUom)) partUom = "pcs";
            recipeUom = recipeUom.ToLower().Trim();
            partUom = partUom.ToLower().Trim();

            if (stockType == "Pack")
            {
                if (packItems <= 0) packItems = 1;
                if (recipeUom == "pcs")
                {
                    return qty / packItems;
                }
                else if (recipeUom == "g")
                {
                    if (partUom.StartsWith("kilo") || partUom == "kg")
                    {
                        return qty / (packItems * 1000.0);
                    }
                    else
                    {
                        return qty / packItems;
                    }
                }
                else if (recipeUom == "kg")
                {
                    if (partUom.StartsWith("gram") || partUom == "g")
                    {
                        return qty * 1000.0 / packItems;
                    }
                    else
                    {
                        return qty / packItems;
                    }
                }
                else
                {
                    return qty / packItems;
                }
            }
            else
            {
                if ((partUom.StartsWith("kilo") || partUom == "kg") && (recipeUom == "g" || recipeUom.StartsWith("gram")))
                {
                    return qty / 1000.0;
                }
                else if ((partUom.StartsWith("gram") || partUom == "g") && (recipeUom == "kg" || recipeUom.StartsWith("kilo")))
                {
                    return qty * 1000.0;
                }
                else if ((partUom.StartsWith("liter") || partUom == "l") && (recipeUom == "ml"))
                {
                    return qty / 1000.0;
                }
                else if (partUom == "ml" && (recipeUom == "l" || recipeUom.StartsWith("liter")))
                {
                    return qty * 1000.0;
                }
                return qty;
            }
        }
    }
}
