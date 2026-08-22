using System;
using System.IO;
#if !ANDROID
using System.Windows.Forms;
#endif
using System.Text.Json;

namespace Shaheen_InventoryManagement_Android
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
#if ANDROID
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Data");
#else
                string appPath = Application.StartupPath;
                string dir = Path.Combine(appPath, "Data");
#endif
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
#if ANDROID
                string imagesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Parts_Images");
#else
                string imagesPath = Path.Combine(Application.StartupPath, "Parts_Images");
#endif
                if (!Directory.Exists(imagesPath))
                    Directory.CreateDirectory(imagesPath);
                return imagesPath;
            }
        }
    }
}

