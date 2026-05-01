using System;
using Microsoft.Data.Sqlite;
using System.IO;

class Program {
    static void Main() {
        string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "Database", "inventory.db");
        if (!File.Exists(dbPath)) {
            Console.WriteLine("DB not found: " + dbPath);
            return;
        }
        
        string connStr = $"Data Source={dbPath}";
        using (var conn = new SqliteConnection(connStr)) {
            conn.Open();
            var cmd = new SqliteCommand("SELECT order_date FROM orders LIMIT 10", conn);
            using (var reader = cmd.ExecuteReader()) {
                while (reader.Read()) {
                    Console.WriteLine("OrderDate: [" + reader[0] + "]");
                }
            }
        }
    }
}
