using System;
using Shaheen_InventoryManagement_Android.Data;

namespace Shaheen_InventoryManagement_Android.Helpers
{
    public static class IngredientCalculationEngine
    {
        public static decimal CalculateCostPerBigUnit(decimal packPrice, double packSize)
        {
            if (packSize <= 0) return packPrice;
            return packPrice / (decimal)packSize;
        }

        public static decimal CalculateCostPerSmallUnit(decimal packPrice, double packSize, double conversionValue)
        {
            if (packSize <= 0) return packPrice;
            double conv = conversionValue > 0 ? conversionValue : 1.0;
            return packPrice / (decimal)(packSize * conv);
        }

        public static double ConvertStockToBigUnit(double quantityInStockPacks, double packSize)
        {
            double pSize = packSize > 0 ? packSize : 1.0;
            return quantityInStockPacks * pSize;
        }

        public static double ConvertStockToPacks(double currentStockInBigUnit, double packSize)
        {
            double pSize = packSize > 0 ? packSize : 1.0;
            return currentStockInBigUnit / pSize;
        }

        public static double GetConvertedQuantityDynamic(double qty, string recipeUom, string partUom, string stockType, int packItems, string bigUnit, string smallUnit, double convVal, double packSize)
        {
            return RecipePartData.GetConvertedQuantityDynamic(qty, recipeUom, partUom, stockType, packItems, bigUnit, smallUnit, convVal, packSize);
        }

        public static decimal CalculatedUnitCost(decimal baseCost, string unitOfMeasure, string bigUnit, string smallUnit, double conversionValue, double packSize, string stockType, int packItemsNumber, string partUom)
        {
            if (!string.IsNullOrEmpty(bigUnit) && !string.IsNullOrEmpty(smallUnit))
            {
                double conv = conversionValue > 0 ? conversionValue : 1.0;
                double pSize = packSize > 0 ? packSize : 1.0;
                string big = (bigUnit ?? "").ToLower().Trim();
                decimal costPerBigUnit = (big == "pack" || big == "package") ? baseCost : (baseCost / (decimal)pSize);
                decimal costPerSmallUnit = costPerBigUnit / (decimal)conv;

                string uom = (unitOfMeasure ?? "").ToLower().Trim();
                string small = smallUnit.ToLower().Trim();

                if (big == small)
                {
                    if (uom == "pack") return baseCost;
                    return costPerBigUnit;
                }

                if (uom == small)
                {
                    return costPerSmallUnit;
                }
                if (uom == big)
                {
                    return costPerBigUnit;
                }
                if (uom == "pack")
                {
                    return baseCost;
                }
                return costPerSmallUnit;
            }
            else
            {
                // Fallback to standard Piece/Pack logic
                if (stockType == "Pack")
                {
                    int packItems = packItemsNumber > 0 ? packItemsNumber : 1;
                    decimal itemCost = baseCost / packItems;
                    string uom = (unitOfMeasure ?? "").ToLower().Trim();
                    if (uom == "pcs")
                    {
                        return itemCost;
                    }
                    if (uom == "g")
                    {
                        string pUom = (partUom ?? "").ToLower().Trim();
                        if (pUom.StartsWith("kilo") || pUom == "kg")
                        {
                            return itemCost / 1000m;
                        }
                        return itemCost;
                    }
                    if (uom == "kg")
                    {
                        string pUom = (partUom ?? "").ToLower().Trim();
                        if (pUom.StartsWith("gram") || pUom == "g")
                        {
                            return itemCost * 1000m;
                        }
                        return itemCost;
                    }
                    return itemCost;
                }
                else
                {
                    string recipeUom = (unitOfMeasure ?? "").ToLower().Trim();
                    string partUomClean = (partUom ?? "").ToLower().Trim();
                    if ((partUomClean.StartsWith("kilo") || partUomClean == "kg") && (recipeUom == "g" || recipeUom.StartsWith("gram")))
                    {
                        return baseCost / 1000m;
                    }
                    if ((partUomClean.StartsWith("gram") || partUomClean == "g") && (recipeUom == "kg" || recipeUom.StartsWith("kilo")))
                    {
                        return baseCost * 1000m;
                    }
                    if ((partUomClean.StartsWith("liter") || partUomClean == "l") && (recipeUom == "ml"))
                    {
                        return baseCost / 1000m;
                    }
                    if (partUomClean == "ml" && (recipeUom == "l" || recipeUom.StartsWith("liter")))
                    {
                        return baseCost * 1000m;
                    }
                    return baseCost;
                }
            }
        }
    }
}
