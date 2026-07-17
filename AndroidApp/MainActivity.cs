using Android.App;
using Android.OS;
using Android.Webkit;
using Android.Views;
using Android.Widget;
using Android.Content;
using System.IO;
using System.Globalization;
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

            // Check if a valid license/trial is active on this device
            if (LicenseManager.HasValidLicense())
            {
                InitializeWebView();
            }
            else
            {
                ShowLicenseActivationDialog();
            }
        }

        private void InitializeWebView()
        {
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

        private void ShowLicenseActivationDialog()
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle(LocalizationManager.GetString("License_Title", "Software Activation"));
            builder.SetCancelable(false);

            // Container Layout
            var container = new LinearLayout(this);
            container.Orientation = Orientation.Vertical;
            container.SetPadding(50, 40, 50, 40);

            // Description
            var lblDesc = new TextView(this)
            {
                Text = LocalizationManager.GetString("License_Msg_Activate", "This product is unregistered. Please enter your customer name and license key to activate, or start a 30-day trial."),
                TextSize = 16f
            };
            lblDesc.SetPadding(0, 0, 0, 20);
            container.AddView(lblDesc);

            // Hardware ID Display
            var lblHwId = new TextView(this)
            {
                Text = LocalizationManager.GetString("License_HardwareId", "Hardware ID") + ": " + HardwareInfo.GetShortHardwareId(),
                TextSize = 14f,
                Typeface = Android.Graphics.Typeface.Monospace
            };
            lblHwId.SetPadding(0, 0, 0, 30);
            container.AddView(lblHwId);

            // Customer Name Input
            var txtCustomer = new EditText(this) { Hint = LocalizationManager.GetString("License_CustomerName", "Customer / Company Name") };
            container.AddView(txtCustomer);

            // License Key Input
            var txtKey = new EditText(this) { Hint = LocalizationManager.GetString("License_Key", "License Key") };
            container.AddView(txtKey);

            builder.SetView(container);

            builder.SetPositiveButton(LocalizationManager.GetString("License_Activate", "Activate"), (s, e) => {
                // Handled custom below to prevent auto-closing on invalid key
            });

            builder.SetNegativeButton(LocalizationManager.GetString("License_StartTrial", "Start Trial"), (s, e) => {
                // Handled custom below
            });

            builder.SetNeutralButton(LocalizationManager.GetString("License_Exit", "Exit"), (s, e) => {
                Finish();
            });

            var dialog = builder.Create();
            dialog.Show();

            // Override buttons to prevent automatic dismissal on invalid key or click
            dialog.GetButton((int)DialogButtonType.Positive).Click += (s, e) => {
                string customer = txtCustomer.Text.Trim();
                string key = txtKey.Text.Trim();

                if (string.IsNullOrEmpty(customer))
                {
                    txtCustomer.Error = LocalizationManager.GetString("License_CustomerRequired", "Customer Name is required.");
                    return;
                }
                if (string.IsNullOrEmpty(key))
                {
                    txtKey.Error = LocalizationManager.GetString("License_KeyRequired", "License Key is required.");
                    return;
                }

                var activated = LicenseManager.ActivateLicense(key, customer);
                if (activated != null)
                {
                    Toast.MakeText(this, LocalizationManager.GetString("License_ActivatedSuccessfully", "License activated successfully!"), ToastLength.Short).Show();
                    dialog.Dismiss();
                    InitializeWebView();
                }
                else
                {
                    Toast.MakeText(this, LocalizationManager.GetString("License_ActivationFailed", "Invalid License Key or Customer Name for this machine."), ToastLength.Long).Show();
                }
            };

            dialog.GetButton((int)DialogButtonType.Negative).Click += (s, e) => {
                LicenseKey existing = LicenseManager.GetCurrentLicense();
                if (existing != null && existing.IsTrial() && System.DateTime.Now > existing.ExpirationDate)
                {
                    Toast.MakeText(this, LocalizationManager.GetString("License_TrialExpired", "Your trial period has already expired. Please activate a full license key."), ToastLength.Long).Show();
                    return;
                }

                var trial = LicenseManager.StartTrial();
                if (trial != null)
                {
                    Toast.MakeText(this, LocalizationManager.GetString("License_TrialStarted", "30-day trial period started successfully!"), ToastLength.Short).Show();
                    dialog.Dismiss();
                    InitializeWebView();
                }
                else
                {
                    Toast.MakeText(this, LocalizationManager.GetString("License_TrialFailed", "Failed to start trial. Please contact support."), ToastLength.Long).Show();
                }
            };
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
