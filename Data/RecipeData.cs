using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Shaheen_InventoryManagement_Android.Helpers;

namespace Shaheen_InventoryManagement_Android.Data
{
    public class RecipeData
    {
        public int Id { get; set; }
        public string RecipeName { get; set; }
        public string ItemNo { get; set; }
        public string Description { get; set; }
        public decimal SellingPrice { get; set; }
        public string Status { get; set; }
        public DateTime DateAdded { get; set; }
        public string RecipeImage { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public List<RecipePartData> Parts { get; set; } = new List<RecipePartData>();
        public decimal YieldQuantity { get; set; }
        public string YieldUnit { get; set; }

        public decimal TotalCost 
        {
            get 
            {
                if (!Helpers.UserSession.IsAdmin) return 0m;
                decimal cost = 0;
                foreach(var part in Parts) cost += part.TotalCost;
                return cost;
            }
        }

        private static int GetOrCreateCategoryId(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName)) return 0;
            object result = DatabaseHelper.ExecuteScalar<object>("SELECT id FROM categories WHERE category_name = @name",
                new SqliteParameter("@name", categoryName));
            if (result != null) return Convert.ToInt32(result);

            DatabaseHelper.ExecuteNonQuery("INSERT INTO categories (category_name, description) VALUES (@name, '')",
                new SqliteParameter("@name", categoryName));
            return (int)DatabaseHelper.ExecuteScalar<long>("SELECT last_insert_rowid()");
        }

        public static List<RecipeData> GetAllRecipes()
        {
            string sql = @"SELECT r.*, c.category_name 
                           FROM recipes r 
                           LEFT JOIN categories c ON r.category_id = c.id 
                           WHERE r.date_deleted IS NULL 
                           ORDER BY r.recipe_name";
            var list = DatabaseHelper.ExecuteQuery(sql, MapFromReader);
            foreach(var recipe in list)
            {
                recipe.Parts = RecipePartData.GetPartsForRecipe(recipe.Id);
            }
            return list;
        }
        
        public static RecipeData GetRecipe(int id)
        {
            string sql = @"SELECT r.*, c.category_name 
                           FROM recipes r 
                           LEFT JOIN categories c ON r.category_id = c.id 
                           WHERE r.id = @id AND r.date_deleted IS NULL";
            var list = DatabaseHelper.ExecuteQuery(sql, MapFromReader, new SqliteParameter("@id", id));
            if (list.Count > 0)
            {
                list[0].Parts = RecipePartData.GetPartsForRecipe(list[0].Id);
                return list[0];
            }
            return null;
        }

        public static int AddRecipe(RecipeData recipe)
        {
            int catId = GetOrCreateCategoryId(recipe.CategoryName);
            string sql = @"INSERT INTO recipes (recipe_name, description, selling_price, status, date_added, recipe_image, category_id, item_no, yield_quantity, yield_unit)
                           VALUES (@name, @desc, @price, @status, datetime('now'), @img, @catId, @item_no, @yieldQty, @yieldUnit);
                           SELECT last_insert_rowid();";
            
            var idObj = DatabaseHelper.ExecuteScalar<long>(sql,
                new SqliteParameter("@name", recipe.RecipeName),
                new SqliteParameter("@desc", recipe.Description ?? ""),
                new SqliteParameter("@price", recipe.SellingPrice),
                new SqliteParameter("@status", recipe.Status ?? "Active"),
                new SqliteParameter("@img", recipe.RecipeImage ?? ""),
                new SqliteParameter("@catId", catId),
                new SqliteParameter("@item_no", recipe.ItemNo ?? ""),
                new SqliteParameter("@yieldQty", recipe.YieldQuantity),
                new SqliteParameter("@yieldUnit", recipe.YieldUnit ?? "")
            );
            
            int recipeId = (int)idObj;
            
            // Add parts (No inventory deduction)
            foreach(var part in recipe.Parts)
            {
                string sqlPart = @"INSERT INTO recipe_parts (recipe_id, part_id, quantity, unit_of_measure)
                                   VALUES (@r_id, @p_id, @qty, @uom)";
                DatabaseHelper.ExecuteNonQuery(sqlPart,
                    new SqliteParameter("@r_id", recipeId),
                    new SqliteParameter("@p_id", part.PartId),
                    new SqliteParameter("@qty", part.Quantity),
                    new SqliteParameter("@uom", string.IsNullOrEmpty(part.UnitOfMeasure) ? (object)DBNull.Value : part.UnitOfMeasure)
                );
            }
            
            DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Created recipe");
            return recipeId;
        }

        public static void UpdateRecipe(RecipeData recipe)
        {
            int catId = GetOrCreateCategoryId(recipe.CategoryName);
            string sql = @"UPDATE recipes SET 
                            recipe_name = @name, 
                            description = @desc, 
                            selling_price = @price,
                            status = @status,
                            recipe_image = @img,
                            category_id = @catId,
                            item_no = @item_no,
                            yield_quantity = @yieldQty,
                            yield_unit = @yieldUnit
                           WHERE id = @id";
            DatabaseHelper.ExecuteNonQuery(sql,
                new SqliteParameter("@name", recipe.RecipeName),
                new SqliteParameter("@desc", recipe.Description ?? ""),
                new SqliteParameter("@price", recipe.SellingPrice),
                new SqliteParameter("@status", recipe.Status ?? "Active"),
                new SqliteParameter("@img", recipe.RecipeImage ?? ""),
                new SqliteParameter("@catId", catId),
                new SqliteParameter("@item_no", recipe.ItemNo ?? ""),
                new SqliteParameter("@yieldQty", recipe.YieldQuantity),
                new SqliteParameter("@yieldUnit", recipe.YieldUnit ?? ""),
                new SqliteParameter("@id", recipe.Id)
            );
            
            // Delete old parts and re-insert (No inventory deduction)
            DatabaseHelper.ExecuteNonQuery("DELETE FROM recipe_parts WHERE recipe_id = @id", new SqliteParameter("@id", recipe.Id));
            
            foreach(var part in recipe.Parts)
            {
                string sqlPart = @"INSERT INTO recipe_parts (recipe_id, part_id, quantity, unit_of_measure)
                                   VALUES (@r_id, @p_id, @qty, @uom)";
                DatabaseHelper.ExecuteNonQuery(sqlPart,
                    new SqliteParameter("@r_id", recipe.Id),
                    new SqliteParameter("@p_id", part.PartId),
                    new SqliteParameter("@qty", part.Quantity),
                    new SqliteParameter("@uom", string.IsNullOrEmpty(part.UnitOfMeasure) ? (object)DBNull.Value : part.UnitOfMeasure)
                );
            }
            DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Edited recipe");
        }

        public static void DeleteRecipe(int id)
        {
            // Soft delete (No inventory restoration)
            string sql = "UPDATE recipes SET date_deleted = datetime('now') WHERE id = @id";
            DatabaseHelper.ExecuteNonQuery(sql, new SqliteParameter("@id", id));
            DatabaseHelper.LogUserAction(Helpers.UserSession.Username, Helpers.UserSession.FullName, "Deleted recipe");
        }

        public static bool RecipeNameExists(string recipeName, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(recipeName)) return false;
            string sql = "SELECT COUNT(*) FROM recipes WHERE LOWER(recipe_name) = LOWER(@name) AND date_deleted IS NULL";
            var parameters = new System.Collections.Generic.List<SqliteParameter> { new SqliteParameter("@name", recipeName) };
            if (excludeId.HasValue)
            {
                sql += " AND id != @id";
                parameters.Add(new SqliteParameter("@id", excludeId.Value));
            }
            return DatabaseHelper.ExecuteScalar<int>(sql, parameters.ToArray()) > 0;
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

        private static RecipeData MapFromReader(SqliteDataReader r)
        {
            return new RecipeData
            {
                Id           = r.GetInt32(r.GetOrdinal("id")),
                RecipeName   = Safe<string>(r, "recipe_name", ""),
                ItemNo       = Safe<string>(r, "item_no", ""),
                Description  = Safe<string>(r, "description", ""),
                SellingPrice = Safe<decimal>(r, "selling_price", 0),
                Status       = Safe<string>(r, "status", "Active"),
                RecipeImage  = Safe<string>(r, "recipe_image", ""),
                CategoryId   = Safe<int>(r, "category_id", 0),
                CategoryName = Safe<string>(r, "category_name", ""),
                YieldQuantity = Safe<decimal>(r, "yield_quantity", 1),
                YieldUnit    = Safe<string>(r, "yield_unit", ""),
                DateAdded    = DateTime.TryParse(Safe<string>(r, "date_added", ""), out DateTime da) ? da : DateTime.Now
            };
        }
    }
}
