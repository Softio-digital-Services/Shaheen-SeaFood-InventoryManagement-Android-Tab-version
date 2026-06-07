using System;
using System.Windows.Forms;
using InventorySystem.Helpers.Plugins;
using InventorySystem.Helpers;

namespace InventorySystem.Plugins
{
    public class PurchaseOrdersPlugin : ITabPlugin
    {
        private PluginContext _context;

        public string Id => "com.softio.plugins.purchaseorders";
        public string Name => "Purchase Orders Management";
        public string Version => "1.0.0";
        public string Description => "Manage Purchase Orders.";
        public string Author => "Softio Services";

        public bool RequiresLicense => false;
        public string LicenseFeatureKey => "Plugin_PurchaseOrders";

        public string TabId => "btnPO";
        public string TabTitle => LocalizationManager.GetString("Nav_PurchaseOrders");
        public string TabIcon => "purchase_orders";
        public int TabOrder => 80;

        public void Initialize(PluginContext context)
        {
            _context = context;
        }

        public UserControl CreateTabContent()
        {
            var allowed = false;
            foreach (var role in new string[] { "Admin","Accountant" }) {
                if (_context.UserRole == role || (_context.IsAdmin && role == "Admin")) allowed = true;
            }

            if (allowed)
            {
                var form = new InventorySystem.Forms.PurchaseOrdersForm();
                var loadMethod = form.GetType().GetMethod("LoadData") ?? form.GetType().GetMethod("LoadQuotations");
                if (loadMethod != null) { if (loadMethod.GetParameters().Length == 1) loadMethod.Invoke(form, new object[] { "" }); else loadMethod.Invoke(form, null); }
                return form;
            }
            
            return new UserControl { BackColor = System.Drawing.Color.Red };
        }

        public void Shutdown() { }
    }
}


