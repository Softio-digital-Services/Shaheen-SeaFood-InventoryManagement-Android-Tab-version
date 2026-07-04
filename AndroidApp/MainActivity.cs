using Android.App;
using Android.OS;
using Android.Webkit;
using Android.Views;
using System.IO;
using Shaheen_InventoryManagement_Android.Helpers;

namespace Shaheen_InventoryManagement_Android
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
        MainLauncher = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : Activity
    {
        private WebView _webView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            try
            {
                // Initialize the database and ensure the schema is created in local app storage
                DatabaseInitializer.Initialize();
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, "MainActivity.OnCreate.DatabaseInitialize");
            }

            // Create and configure WebView programmatically
            _webView = new WebView(this);
            _webView.Settings.JavaScriptEnabled = true;
            _webView.Settings.DomStorageEnabled = true;
            _webView.Settings.AllowFileAccess = true;
            _webView.Settings.AllowContentAccess = true;
            _webView.Settings.CacheMode = CacheModes.NoCache; // Disable cache to force loading updated assets
            
            // Add the bridge interface to the webview window before loading the URL
            _webView.AddJavascriptInterface(new WebAppInterface(this), "AndroidBridge");

            // Handle requests locally using our custom intercepting client
            _webView.SetWebViewClient(new LocalApiWebViewClient(this));

            // Load the web client's index.html with cache buster
            _webView.LoadUrl("http://local-api/wwwroot/index.html?t=" + System.DateTime.UtcNow.Ticks);

            SetContentView(_webView);
        }

        public override void OnBackPressed()
        {
            if (_webView != null && _webView.CanGoBack())
            {
                _webView.GoBack();
            }
            else
            {
                // Standard Android back behavior (closes the activity/app)
#pragma warning disable CA1422 // Validate platform compatibility
                base.OnBackPressed();
#pragma warning restore CA1422
            }
        }
    }
}
