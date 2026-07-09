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
        Icon = "@drawable/app_icon",
        MainLauncher = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : Activity
    {
        private WebView _webView;
        public IValueCallback UploadMessage { get; set; }
        public static readonly int FileChooserRequestCode = 1001;

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

            // Set custom WebChromeClient to handle file chooser
            _webView.SetWebChromeClient(new MyWebChromeClient(this));

            // Load the web client's index.html with cache buster
            _webView.LoadUrl("http://local-api/wwwroot/index.html?t=" + System.DateTime.UtcNow.Ticks);

            SetContentView(_webView);
        }

        public bool ShowFileChooser(IValueCallback filePathCallback, WebChromeClient.FileChooserParams fileChooserParams)
        {
            if (UploadMessage != null)
            {
                UploadMessage.OnReceiveValue(null);
                UploadMessage = null;
            }

            UploadMessage = filePathCallback;

            try
            {
                var intent = fileChooserParams.CreateIntent();
                StartActivityForResult(intent, FileChooserRequestCode);
                return true;
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, "MainActivity.ShowFileChooser");
                UploadMessage = null;
                return false;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == FileChooserRequestCode)
            {
                if (UploadMessage == null)
                    return;

                Android.Net.Uri[] results = null;

                if (resultCode == Result.Ok && data != null)
                {
                    string dataString = data.DataString;
                    if (dataString != null)
                    {
                        results = new[] { Android.Net.Uri.Parse(dataString) };
                    }
                    else if (data.ClipData != null)
                    {
                        var clipData = data.ClipData;
                        results = new Android.Net.Uri[clipData.ItemCount];
                        for (int i = 0; i < clipData.ItemCount; i++)
                        {
                            results[i] = clipData.GetItemAt(i).Uri;
                        }
                    }
                }

                UploadMessage.OnReceiveValue(results);
                UploadMessage = null;
            }
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

    public class MyWebChromeClient : WebChromeClient
    {
        private readonly MainActivity _activity;

        public MyWebChromeClient(MainActivity activity)
        {
            _activity = activity;
        }

        public override bool OnShowFileChooser(WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
        {
            return _activity.ShowFileChooser(filePathCallback, fileChooserParams);
        }
    }
}
