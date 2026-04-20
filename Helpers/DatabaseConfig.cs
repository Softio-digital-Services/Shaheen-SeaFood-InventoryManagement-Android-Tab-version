using System;
using System.IO;
using System.Windows.Forms;
using System.Text.Json;

namespace GenericInventorySystem
{
    /// <summary>
    /// Centralized database and file path configuration
    /// </summary>
    public static class DatabaseConfig
    {
        /// <summary>
        /// Gets the dynamic connection string based on application location
        /// Database is stored locally in the application's Data directory
        /// </summary>
        private static string GetDatabaseFileName()
        {
            try
            {
                string configPath = "appsettings.json";
                if (File.Exists(configPath))
                {
                    string jsonString = File.ReadAllText(configPath);
                    using (JsonDocument doc = JsonDocument.Parse(jsonString))
                    {
                        if (doc.RootElement.TryGetProperty("DatabaseSettings", out JsonElement dbSettings))
                        {
                            if (dbSettings.TryGetProperty("DatabaseFileName", out JsonElement dbFile))
                                return dbFile.GetString() ?? "inventory_generic.mdf";
                        }
                    }
                }
            }
            catch { }
            return "inventory_generic.mdf";
        }

        public static string ConnectionString
        {
            get
            {
                // Store database locally in the application's directory
                string appPath = Application.StartupPath;
                string dbName = GetDatabaseFileName();
                string dbPath = Path.Combine(appPath, "Data", dbName);
                
                // Ensure directory exists
                string dbDirectory = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(dbDirectory))
                {
                    Directory.CreateDirectory(dbDirectory);
                }

                string dbIdentifier = GetDatabaseFileName().Replace(".mdf", "DB");
                return $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True;Connect Timeout=30;Database={dbIdentifier};Pooling=False";
            }
        }

        /// <summary>
        /// Gets the database file path
        /// </summary>
        public static string DatabasePath
        {
            get
            {
                string appPath = Application.StartupPath;
                return Path.Combine(appPath, "Data", GetDatabaseFileName());
            }
        }

        /// <summary>
        /// Gets the parts images directory path
        /// </summary>
        public static string PartsImagesDirectory
        {
            get
            {
                string appPath = Application.StartupPath;
                string imagesPath = Path.Combine(appPath, "Parts_Images");
                
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }
                
                return imagesPath;
            }
        }
    }
}
