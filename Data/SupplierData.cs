using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace GenericInventorySystem.Data
{
    public class SupplierData
    {
        public int Id { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Get all active suppliers
        /// </summary>
        public static List<SupplierData> GetAllSuppliers()
        {
            string sql = @"SELECT * FROM suppliers 
                          WHERE date_deleted IS NULL 
                          ORDER BY supplier_name";
            
            return DatabaseHelper.ExecuteQuery(sql, MapFromReader);
        }

        /// <summary>
        /// Get active suppliers for dropdown
        /// </summary>
        public static List<SupplierData> GetActiveSuppliers()
        {
            string sql = @"SELECT * FROM suppliers 
                          WHERE status = 'Active' AND date_deleted IS NULL 
                          ORDER BY supplier_name";
            
            return DatabaseHelper.ExecuteQuery(sql, MapFromReader);
        }

        /// <summary>
        /// Map reader to SupplierData
        /// </summary>
        private static SupplierData MapFromReader(SqlDataReader reader)
        {
            return new SupplierData
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                SupplierCode = reader.GetString(reader.GetOrdinal("supplier_code")),
                SupplierName = reader.GetString(reader.GetOrdinal("supplier_name")),
                ContactPerson = reader.IsDBNull(reader.GetOrdinal("contact_person")) ? "" : reader.GetString(reader.GetOrdinal("contact_person")),
                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString(reader.GetOrdinal("email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? "" : reader.GetString(reader.GetOrdinal("phone")),
                Address = reader.IsDBNull(reader.GetOrdinal("address")) ? "" : reader.GetString(reader.GetOrdinal("address")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                DateAdded = reader.GetDateTime(reader.GetOrdinal("date_added"))
            };
        }
        public static void AddSupplier(string name, string phone, string email, string address, string type, string contactPerson)
        {
             string code = "SUP-" + DateTime.Now.Ticks.ToString().Substring(10);
             string sql = "INSERT INTO suppliers (supplier_name, phone, email, address, balance_due, type, supplier_code, contact_person, date_added) " + 
                          "VALUES (@name, @phone, @email, @addr, 0, @type, @code, @contact, GETDATE())";
             
             DatabaseHelper.ExecuteNonQuery(sql,
                 new SqlParameter("@name", name),
                 new SqlParameter("@phone", phone),
                 new SqlParameter("@email", email),
                 new SqlParameter("@addr", address),
                 new SqlParameter("@type", type),
                 new SqlParameter("@code", code),
                 new SqlParameter("@contact", contactPerson ?? "")
             );
             
             LogTransaction("SUPPLIER_ADD", $"Added Supplier: {name} ({code})");
        }

        public static void UpdateSupplier(int id, string name, string phone, string email, string address, string type, string contactPerson)
        {
            string sql = "UPDATE suppliers SET supplier_name=@name, phone=@phone, email=@email, address=@addr, type=@type, contact_person=@contact WHERE id=@id";
             DatabaseHelper.ExecuteNonQuery(sql,
                 new SqlParameter("@name", name),
                 new SqlParameter("@phone", phone),
                 new SqlParameter("@email", email),
                 new SqlParameter("@addr", address),
                 new SqlParameter("@type", type),
                 new SqlParameter("@contact", contactPerson ?? ""),
                 new SqlParameter("@id", id));

             LogTransaction("SUPPLIER_UPDATE", $"Updated Supplier: {name} (ID: {id})");
        }

        public static void DeleteSupplier(int id)
        {
             DatabaseHelper.ExecuteNonQuery($"UPDATE suppliers SET date_deleted = GETDATE() WHERE id = {id}");
             LogTransaction("SUPPLIER_DELETE", $"Deleted Supplier ID: {id}");
        }

        private static void LogTransaction(string action, string description)
        {
            try
            {
                string sql = "INSERT INTO transactions (action_type, part_name, description, username) VALUES (@action, 'N/A', @desc, 'System')";
                DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter("@action", action), new SqlParameter("@desc", description));
            }
            catch { /* Ignore logging errors to prevent blocking main flow */ }
        }
    }
}
