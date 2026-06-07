using System;
using System.Windows.Forms;
using InventorySystem.Helpers.Plugins;
using InventorySystem.Helpers;

namespace InventorySystem.Plugins
{
    public class QuotationsPlugin : ITabPlugin
    {
        private PluginContext _context;

        public string Id => "com.softio.plugins.quotations";
        public string Name => "Quotations Management";
        public string Version => "1.0.0";
        public string Description => "Manage Quotations.";
        public string Author => "Softio Services";

        public bool RequiresLicense => false;
        public string LicenseFeatureKey => "Plugin_Quotations";

        public string TabId => "btnQuotations";
        public string TabTitle => LocalizationManager.GetString("Nav_Quotations");
        public string TabIcon => "quotations";
        public int TabOrder => 70;

        public void Initialize(PluginContext context)
        {
            _context = context;
        }

        public UserControl CreateTabContent()
        {
            var allowed = false;
            foreach (var role in new string[] { "Admin" }) {
                if (_context.UserRole == role || (_context.IsAdmin && role == "Admin")) allowed = true;
            }

            if (allowed)
            {
                var form = new InventorySystem.Forms.QuotationsForm();
                var loadMethod = form.GetType().GetMethod("LoadData") ?? form.GetType().GetMethod("LoadQuotations");
                if (loadMethod != null) { if (loadMethod.GetParameters().Length == 1) loadMethod.Invoke(form, new object[] { "" }); else loadMethod.Invoke(form, null); }
                return form;
            }
            
            return new UserControl { BackColor = System.Drawing.Color.Red };
        }

        public void Shutdown() { }
    }
}


