using System;
using System.IO;
using System.Windows.Forms;
using System.Text.Json;

namespace InventorySystem
{
    /// <summary>
    /// Centralized database and file path configuration (SQLite)
    /// </summary>
    public static class DatabaseConfig
    {
        /// <summary>
        /// The SQLite database file is stored next to the .exe in a /Data subfolder.
        /// This works on any Windows PC without any SQL Server installation.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                string dbPath = DatabasePath;
                // Ensure directory exists
                string dir = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                return $"Data Source={dbPath};";
            }
        }

        /// <summary>
        /// Full path to the SQLite .db file.
        /// Stored in the application's Data folder, portable with the exe.
        /// </summary>
        public static string DatabasePath
        {
            get
            {
                string appPath = Application.StartupPath;
                string dir = Path.Combine(appPath, "Data");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                return Path.Combine(dir, "inventory.db");
            }
        }

        /// <summary>
        /// Gets the parts images directory path
        /// </summary>
        public static string PartsImagesDirectory
        {
            get
            {
                string imagesPath = Path.Combine(Application.StartupPath, "Parts_Images");
                if (!Directory.Exists(imagesPath))
                    Directory.CreateDirectory(imagesPath);
                return imagesPath;
            }
        }
    }
}
