using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Helpers.Plugins;

namespace GenericInventorySystem.Plugins
{
    /// <summary>
    /// Free built-in plugin -- adds Database Backup &amp; Restore via the sidebar.
    /// RequiresLicense = false -> available on all license types including TRIAL.
    /// </summary>
    public class BackupPlugin : ITabPlugin
    {
        public string Id          => "com.carparts.backup";
        public string Name        => "Backup & Restore";
        public string Version     => "1.0.0";
        public string Description => "Backup and restore the inventory database";
        public string Author      => "Car Parts Inventory System";

        public bool   RequiresLicense   => false;
        public string LicenseFeatureKey => "";

        public string TabId    => "btnBackup";
        public string TabTitle => LocalizationManager.IsArabic ? "\u0646\u0633\u062e\u0629 \u0627\u062d\u062a\u064a\u0627\u0637\u064a\u0629" : "Backup";
        public string TabIcon  => "backup";
        public int    TabOrder => 120;

        private PluginContext _ctx;

        public void Initialize(PluginContext context) => _ctx = context;
        public void Shutdown() { }

        public UserControl CreateTabContent() => new BackupPanel(_ctx);
    }

    /// <summary>Backup / Restore UI panel.</summary>
    public class BackupPanel : UserControl
    {
        private readonly PluginContext _ctx;
        private Label _lblLastBackup;

        public BackupPanel(PluginContext ctx)
        {
            _ctx = ctx;
            this.BackColor = ThemeConfig.BackgroundColor;
            this.Dock      = DockStyle.Fill;
            Build();
        }

        private void Build()
        {
            bool ar = LocalizationManager.IsArabic;

            // Title removed as it's already in the Modal Header

            // Card container
            Panel card = new Panel();
            card.Width     = 480;
            card.Height    = 340;
            card.BackColor = ThemeConfig.SurfaceColor;
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = RoundRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 16))
                using (var pen  = new Pen(ThemeConfig.BorderColor, 1.5f))
                    e.Graphics.DrawPath(pen, path);
            };
            this.Resize += (s, e) => card.Location = new Point((this.Width - card.Width) / 2, 90);
            this.Controls.Add(card);

            int y = 30;

            // Last backup info
            _lblLastBackup = new Label
            {
                AutoSize  = false,
                Size      = new Size(440, 30),
                Location  = new Point(20, y),
                Font      = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.SecondaryColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Text      = GetLastBackupText()
            };
            card.Controls.Add(_lblLastBackup);
            y += 40;

            // Separator
            card.Controls.Add(new Panel { BackColor = ThemeConfig.BorderColor, Location = new Point(20, y), Size = new Size(440, 1) });
            y += 20;

            // Action buttons
            AddActionButton(card, ref y,
                ar ? "\u0625\u0646\u0634\u0627\u0621 \u0646\u0633\u062e\u0629 \u0627\u062d\u062a\u064a\u0627\u0637\u064a\u0629 \u0627\u0644\u0622\u0646" : "Create Backup Now",
                "backup", ThemeConfig.PrimaryColor, DoBackup);

            y += 10;

            AddActionButton(card, ref y,
                ar ? "\u0627\u0633\u062a\u0639\u0627\u062f\u0629 \u0646\u0633\u062e\u0629 \u0627\u062d\u062a\u064a\u0627\u0637\u064a\u0629" : "Restore from Backup",
                "restore_from_backup", ThemeConfig.WarningBorder, DoRestore);

            y += 10;

            AddActionButton(card, ref y,
                ar ? "\u0641\u062a\u062d \u0645\u062c\u0644\u062f \u0627\u0644\u0646\u0633\u062e" : "Open Backup Folder",
                "open_backup_folder", ThemeConfig.SecondaryColor, OpenBackupFolder);

            // Tip note
            Label note = new Label
            {
                AutoSize  = false,
                Size      = new Size(440, 40),
                Location  = new Point(20, y + 15),
                Font      = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.SecondaryColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Text      = ar
                    ? "\u062a\u0648\u0635\u064a\u0629: \u0642\u0645 \u0628\u0625\u0646\u0634\u0627\u0621 \u0646\u0633\u062e\u0629 \u0627\u062d\u062a\u064a\u0627\u0637\u064a\u0629 \u064a\u0648\u0645\u064a\u064b\u0627 \u0644\u062d\u0645\u0627\u064a\u0629 \u0628\u064a\u0627\u0646\u0627\u062a\u0643."
                    : "Tip: Create a daily backup to protect your data from accidental loss."
            };
            card.Controls.Add(note);
        }

        private void AddActionButton(Panel card, ref int y, string text, string icon, Color color, Action action)
        {
            Button btn = new Button
            {
                Text      = "  " + text,
                Size      = new Size(440, 50),
                Location  = new Point(20, y),
                FlatStyle = FlatStyle.Flat,
                Font      = ThemeConfig.ButtonFont,
                ForeColor = color,
                BackColor = ThemeConfig.SurfaceColor,
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(10, 0, 0, 0)
            };
            btn.FlatAppearance.BorderColor = color;
            btn.FlatAppearance.BorderSize  = 1;

            Image img = ThemeConfig.GetNuricon(icon);
            if (img != null)
            {
                btn.Image      = ResizeImg(img, 22, 22);
                btn.ImageAlign = ContentAlignment.MiddleLeft;
                btn.TextImageRelation = TextImageRelation.ImageBeforeText;
            }

            btn.Click += (s, e) => action?.Invoke();
            card.Controls.Add(btn);
            y += 60;
        }

        private void DoBackup()
        {
            try
            {
                string backupDir = GetBackupDirectory();
                Directory.CreateDirectory(backupDir);

                string dbFile = FindDatabaseFile();
                if (dbFile == null)
                {
                    MessageHelper.ShowError(LocalizationManager.IsArabic
                        ? "\u0644\u0645 \u064a\u062a\u0645 \u0627\u0644\u0639\u062b\u0648\u0631 \u0639\u0644\u0649 \u0645\u0644\u0641 \u0642\u0627\u0639\u062f\u0629 \u0627\u0644\u0628\u064a\u0627\u0646\u0627\u062a."
                        : "Database file not found.");
                    return;
                }

                string destFile = Path.Combine(backupDir,
                    $"backup_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(dbFile)}");
                File.Copy(dbFile, destFile, overwrite: true);

                File.WriteAllText(Path.Combine(backupDir, "last_backup.txt"), DateTime.Now.ToString("o"));
                _lblLastBackup.Text = GetLastBackupText();

                MessageHelper.ShowSuccess(LocalizationManager.IsArabic
                    ? $"\u062a\u0645\u062a \u0627\u0644\u0646\u0633\u062e\u0629 \u0628\u0646\u062c\u0627\u062d!\n{destFile}"
                    : $"Backup created successfully!\n{destFile}");
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError("Backup failed: " + ex.Message);
            }
        }

        private void DoRestore()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title  = LocalizationManager.IsArabic ? "\u0627\u062e\u062a\u0631 \u0645\u0644\u0641 \u0627\u0644\u0646\u0633\u062e\u0629" : "Select Backup File";
                dlg.Filter = "Database files (*.mdf;*.db;*.sqlite)|*.mdf;*.db;*.sqlite|All files (*.*)|*.*";
                dlg.InitialDirectory = GetBackupDirectory();

                if (dlg.ShowDialog() != DialogResult.OK) return;

                bool confirm = MessageHelper.ConfirmAction(LocalizationManager.IsArabic
                    ? "\u0633\u064a\u062a\u0645 \u0627\u0633\u062a\u0628\u062f\u0627\u0644 \u0642\u0627\u0639\u062f\u0629 \u0627\u0644\u0628\u064a\u0627\u0646\u0627\u062a. \u0647\u0644 \u0623\u0646\u062a \u0645\u062a\u0623\u0643\u062f\u061f"
                    : "This will replace the current database. Are you sure?");

                if (!confirm) return;

                try
                {
                    string dbFile = FindDatabaseFile();
                    if (dbFile == null) { MessageHelper.ShowError("Database file not found."); return; }

                    // Safety backup first
                    string safetyFile = Path.Combine(GetBackupDirectory(),
                        $"pre_restore_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(dbFile)}");
                    File.Copy(dbFile, safetyFile, overwrite: true);

                    File.Copy(dlg.FileName, dbFile, overwrite: true);

                    MessageHelper.ShowSuccess(LocalizationManager.IsArabic
                        ? "\u062a\u0645\u062a \u0627\u0633\u062a\u0639\u0627\u062f\u0629 \u0642\u0627\u0639\u062f\u0629 \u0627\u0644\u0628\u064a\u0627\u0646\u0627\u062a. \u064a\u0631\u062c\u0649 \u0625\u0639\u0627\u062f\u0629 \u062a\u0634\u063a\u064a\u0644 \u0627\u0644\u062a\u0637\u0628\u064a\u0642."
                        : "Database restored successfully. Please restart the application.");
                }
                catch (Exception ex)
                {
                    MessageHelper.ShowError("Restore failed: " + ex.Message);
                }
            }
        }

        private void OpenBackupFolder()
        {
            string dir = GetBackupDirectory();
            Directory.CreateDirectory(dir);
            System.Diagnostics.Process.Start("explorer.exe", dir);
        }

        private static string GetBackupDirectory()
            => Path.Combine(Application.StartupPath, "Backups");

        private static string FindDatabaseFile()
        {
            string dbPath = DatabaseConfig.DatabasePath;
            if (File.Exists(dbPath)) return dbPath;
            
            // Fallback for older versions or different structures
            string[] candidates =
            {
                Path.Combine(Application.StartupPath, "Database", "carparts.mdf"),
                Path.Combine(Application.StartupPath, "carparts.mdf"),
                Path.Combine(Application.StartupPath, "Data", "inventory_generic.mdf"),
                Path.Combine(Application.StartupPath, "carparts.db"),
            };
            foreach (var c in candidates)
                if (File.Exists(c)) return c;
            return null;
        }

        private static string GetLastBackupText()
        {
            string ts = Path.Combine(GetBackupDirectory(), "last_backup.txt");
            bool ar = LocalizationManager.IsArabic;
            if (!File.Exists(ts))
                return ar ? "\u0644\u0645 \u064a\u062a\u0645 \u0625\u0646\u0634\u0627\u0621 \u0646\u0633\u062e\u0629 \u0628\u0639\u062f" : "No backup created yet";
            try
            {
                DateTime dt = DateTime.Parse(File.ReadAllText(ts));
                return (ar ? "\u0622\u062e\u0631 \u0646\u0633\u062e\u0629: " : "Last backup: ") + dt.ToString("dd MMM yyyy  HH:mm");
            }
            catch { return "--"; }
        }

        private static Image ResizeImg(Image img, int w, int h)
        {
            var b = new System.Drawing.Bitmap(w, h);
            using (var g = System.Drawing.Graphics.FromImage(b))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, w, h);
            }
            return b;
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundRect(Rectangle r, int rad)
        {
            var p = new System.Drawing.Drawing2D.GraphicsPath();
            p.AddArc(r.X, r.Y, rad, rad, 180, 90);
            p.AddArc(r.Right - rad, r.Y, rad, rad, 270, 90);
            p.AddArc(r.Right - rad, r.Bottom - rad, rad, rad, 0, 90);
            p.AddArc(r.X, r.Bottom - rad, rad, rad, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}

