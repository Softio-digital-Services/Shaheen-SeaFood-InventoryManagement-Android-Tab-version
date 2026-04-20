using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace GenericInventorySystem.Data
{
    public class CategoryData
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Get all categories
        /// </summary>
        public static List<CategoryData> GetAllCategories()
        {
            string sql = "SELECT * FROM categories ORDER BY category_name";
            return DatabaseHelper.ExecuteQuery(sql, MapFromReader);
        }

        /// <summary>
        /// Map reader to CategoryData
        /// </summary>
        private static CategoryData MapFromReader(SqlDataReader reader)
        {
            var cat = new CategoryData
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                CategoryName = reader.GetString(reader.GetOrdinal("category_name"))
            };
            
            try 
            {
                int descOrd = reader.GetOrdinal("description");
                cat.Description = reader.IsDBNull(descOrd) ? "" : reader.GetString(descOrd);
            }
            catch 
            {
                cat.Description = "";
            }

            try 
            {
                int ord = reader.GetOrdinal("date_created");
                cat.DateCreated = reader.IsDBNull(ord) ? DateTime.Now : reader.GetDateTime(ord);
            }
            catch 
            {
                cat.DateCreated = DateTime.Now;
            }
            
            return cat;
        }
        public static void AddCategory(string name, string description)
        {
            string sql = "INSERT INTO categories (category_name, description, date_created) VALUES (@name, @desc, GETDATE())";
            DatabaseHelper.ExecuteNonQuery(sql, 
                new SqlParameter("@name", name),
                new SqlParameter("@desc", description));
        }
    }
}
