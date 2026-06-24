using System;
using System.Windows.Forms;
using Shaheen_InventoryManagement_Android.Helpers.Plugins;
using Shaheen_InventoryManagement_Android.Helpers;

namespace Shaheen_InventoryManagement_Android.Plugins
{
    public class RecipesPlugin : ITabPlugin
    {
        private PluginContext _context;

        public string Id => "com.softio.plugins.recipes";
        public string Name => "Recipes Management";
        public string Version => "1.0.0";
        public string Description => "Manage Assemblies and Recipes.";
        public string Author => "Softio Services";

        public bool RequiresLicense => false;
        public string LicenseFeatureKey => "";

        public string TabId => "btnRecipes";
        public string TabTitle => "Recipes";
        public string TabIcon => "recipe"; // Use the custom recipe icon
        public int TabOrder => 35; // Put it somewhere below inventory

        public void Initialize(PluginContext context)
        {
            _context = context;
        }

        public UserControl CreateTabContent()
        {
            var allowed = false;
            foreach (var role in new string[] { "Admin", "Staff" }) {
                if (_context.UserRole == role || (_context.IsAdmin && role == "Admin")) allowed = true;
            }

            if (allowed)
            {
                var form = new Shaheen_InventoryManagement_Android.Forms.RecipesForm();
                return form;
            }
            
            return new UserControl { BackColor = System.Drawing.Color.Red };
        }

        public void Shutdown() { }
    }
}
