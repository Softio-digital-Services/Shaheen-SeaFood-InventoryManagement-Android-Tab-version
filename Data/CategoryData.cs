using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace GenericInventorySystem.Data
{
    public class CategoryData
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }

        public static List<CategoryData> GetAllCategories()
        {
            string sql = "SELECT * FROM categories ORDER BY category_name";
            return DatabaseHelper.ExecuteQuery(sql, MapFromReader);
        }

        private static CategoryData MapFromReader(SqliteDataReader reader)
        {
            var cat = new CategoryData
            {
                Id           = reader.GetInt32(reader.GetOrdinal("id")),
                CategoryName = reader.GetString(reader.GetOrdinal("category_name")),
                Description  = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString(reader.GetOrdinal("description"))
            };

            try
            {
                int ord = reader.GetOrdinal("date_created");
                cat.DateCreated = reader.IsDBNull(ord) ? DateTime.Now
                    : DateTime.Parse(reader.GetString(ord));
            }
            catch { cat.DateCreated = DateTime.Now; }

            return cat;
        }

        public static void AddCategory(string name, string description)
        {
            string sql = "INSERT INTO categories (category_name, description, date_created) " +
                         "VALUES (@name, @desc, datetime('now'))";
            DatabaseHelper.ExecuteNonQuery(sql,
                new SqliteParameter("@name", name),
                new SqliteParameter("@desc", description));
        }
    }
}
