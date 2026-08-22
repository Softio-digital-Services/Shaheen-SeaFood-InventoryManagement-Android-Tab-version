using System.Drawing;

namespace Shaheen_InventoryManagement_Android
{
    /// <summary>
    /// Lightweight platform theme config for the Android app.
    /// Exposes core colors needed by the local Web API.
    /// </summary>
    public static class ThemeConfig
    {
        public static Color PrimaryColor { get; } = Color.FromArgb(14, 165, 233); // Sky 500
        public static Color SecondaryColor { get; } = Color.FromArgb(100, 116, 139); // Slate Gray
    }
}
