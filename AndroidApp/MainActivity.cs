using Android.App;
using Android.OS;
using Android.Webkit;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
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

        public void PrintWebView(string jobName)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    var printManager = (Android.Print.PrintManager)GetSystemService(Context.PrintService);
                    var printAdapter = _webView.CreatePrintDocumentAdapter(jobName);
                    printManager.Print(jobName, printAdapter, new Android.Print.PrintAttributes.Builder().Build());
                }
                catch (System.Exception ex)
                {
                    ErrorLogger.LogError(ex, "MainActivity.PrintWebView");
                }
            });
        }

        private void ShowLicenseActivationDialog()
        {
            var dialog = new Dialog(this);
            dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
            dialog.SetCancelable(false);

            // Screen pixel scaling helper
            int DpToPx(int dp)
            {
                return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, Resources.DisplayMetrics);
            }

            bool isAr = LocalizationManager.IsArabic;

            // 1. Root Container (overlay)
            var rootLayout = new FrameLayout(this)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };
            rootLayout.SetBackgroundColor(Color.Argb(215, 10, 10, 10)); // Dark semi-transparent overlay (rgba(0,0,0,0.85))

            // Calculate a safe responsive width (e.g. 90% of screen width, max of 370dp)
            int screenWidth = Resources.DisplayMetrics.WidthPixels;
            int cardWidth = Math.Min(DpToPx(370), screenWidth - DpToPx(48));

            // 2. Card Layout (the central card)
            var cardLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical,
                Elevation = DpToPx(8)
            };
            var cardParams = new FrameLayout.LayoutParams(cardWidth, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = GravityFlags.Center
            };
            cardLayout.LayoutParameters = cardParams;
            cardLayout.SetPadding(DpToPx(24), DpToPx(24), DpToPx(24), DpToPx(24));

            var cardDrawable = new GradientDrawable();
            cardDrawable.SetShape(ShapeType.Rectangle);
            cardDrawable.SetColor(Color.White);
            cardDrawable.SetCornerRadius(DpToPx(28));
            cardDrawable.SetStroke(DpToPx(1), Color.Argb(30, 53, 93, 157)); // Border color
            cardLayout.Background = cardDrawable;

            // 3. Logo
            var pbLogo = new ImageView(this);
            try
            {
                using (var stream = Assets.Open("wwwroot/nuricon_POS_web.png"))
                {
                    var bitmap = BitmapFactory.DecodeStream(stream);
                    pbLogo.SetImageBitmap(bitmap);
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, "MainActivity.ShowLicenseActivationDialog.LoadLogo");
            }
            var logoParams = new LinearLayout.LayoutParams(DpToPx(85), DpToPx(85))
            {
                Gravity = GravityFlags.CenterHorizontal,
                BottomMargin = DpToPx(15)
            };
            pbLogo.LayoutParameters = logoParams;
            cardLayout.AddView(pbLogo);

            // 4. Title
            var lblTitle = new TextView(this)
            {
                Text = LocalizationManager.GetString("License_Title", "Software Activation"),
                TextSize = 20f,
                Gravity = GravityFlags.CenterHorizontal
            };
            lblTitle.SetTypeface(Typeface.SansSerif, TypefaceStyle.Bold);
            lblTitle.SetTextColor(Color.Rgb(53, 93, 157)); // Primary color
            var titleParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = GravityFlags.CenterHorizontal,
                BottomMargin = DpToPx(6)
            };
            lblTitle.LayoutParameters = titleParams;
            cardLayout.AddView(lblTitle);

            // 5. Description (Subtitle)
            var lblDesc = new TextView(this)
            {
                Text = LocalizationManager.GetString("License_Msg_Activate", "This product is unregistered. Please enter your customer name and license key to activate, or start a 30-day trial."),
                TextSize = 13f,
                Gravity = GravityFlags.CenterHorizontal
            };
            lblDesc.SetTextColor(Color.Rgb(100, 116, 139)); // Muted text color
            var descParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = GravityFlags.CenterHorizontal,
                BottomMargin = DpToPx(24)
            };
            lblDesc.LayoutParameters = descParams;
            cardLayout.AddView(lblDesc);

            // 6. Input Groups helper
            LinearLayout CreateInputGroup(string labelText, string hintText, out EditText editField, bool readOnly = false, bool isMonospace = false)
            {
                var group = new LinearLayout(this)
                {
                    Orientation = Orientation.Vertical
                };
                var groupParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    BottomMargin = DpToPx(16)
                };
                group.LayoutParameters = groupParams;

                var label = new TextView(this)
                {
                    Text = labelText.ToUpper(CultureInfo.CurrentCulture),
                    TextSize = 11f
                };
                label.SetTypeface(Typeface.SansSerif, TypefaceStyle.Bold);
                label.SetTextColor(Color.Rgb(100, 116, 139));
                var labelParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    BottomMargin = DpToPx(6)
                };
                label.LayoutParameters = labelParams;
                group.AddView(label);

                editField = new EditText(this)
                {
                    Hint = hintText,
                    TextSize = 14f,
                    Enabled = !readOnly
                };
                editField.SetTextColor(Color.Rgb(51, 51, 51)); // #333333
                editField.SetHintTextColor(Color.Rgb(180, 180, 180));
                editField.SetSingleLine(true);
                editField.Gravity = GravityFlags.CenterVertical;
                editField.SetPadding(DpToPx(16), DpToPx(10), DpToPx(16), DpToPx(10));

                if (isMonospace)
                {
                    editField.SetTypeface(Typeface.Monospace, TypefaceStyle.Normal);
                }

                var fieldBg = new GradientDrawable();
                fieldBg.SetShape(ShapeType.Rectangle);
                fieldBg.SetColor(Color.Argb(10, 53, 93, 157)); // soft primary tint
                fieldBg.SetCornerRadius(DpToPx(12));
                fieldBg.SetStroke(DpToPx(1), Color.Argb(35, 53, 93, 157));
                editField.Background = fieldBg;

                if (!readOnly)
                {
                    var finalBg = fieldBg;
                    editField.FocusChange += (s, e) =>
                    {
                        if (e.HasFocus)
                        {
                            finalBg.SetStroke(DpToPx(2), Color.Rgb(53, 93, 157));
                            finalBg.SetColor(Color.Argb(20, 53, 93, 157));
                        }
                        else
                        {
                            finalBg.SetStroke(DpToPx(1), Color.Argb(35, 53, 93, 157));
                            finalBg.SetColor(Color.Argb(10, 53, 93, 157));
                        }
                    };
                }

                group.AddView(editField);
                return group;
            }

            // 7. Hardware ID Input Group
            var hwLabelText = LocalizationManager.GetString("License_HardwareId", "Hardware ID");
            var hwGroup = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };
            var hwGroupParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {
                BottomMargin = DpToPx(16)
            };
            hwGroup.LayoutParameters = hwGroupParams;

            var lblHwLabel = new TextView(this)
            {
                Text = hwLabelText.ToUpper(CultureInfo.CurrentCulture),
                TextSize = 11f
            };
            lblHwLabel.SetTypeface(Typeface.SansSerif, TypefaceStyle.Bold);
            lblHwLabel.SetTextColor(Color.Rgb(100, 116, 139));
            var hwLabelParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                BottomMargin = DpToPx(6)
            };
            lblHwLabel.LayoutParameters = hwLabelParams;
            hwGroup.AddView(lblHwLabel);

            var hwRow = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            hwRow.SetGravity(GravityFlags.CenterVertical);
            hwRow.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            var txtHardwareId = new EditText(this)
            {
                Text = HardwareInfo.GetShortHardwareId(),
                Enabled = false,
                TextSize = 14f
            };
            txtHardwareId.SetTextColor(Color.Rgb(51, 51, 51));
            txtHardwareId.SetSingleLine(true);
            txtHardwareId.Gravity = GravityFlags.CenterVertical;
            txtHardwareId.SetPadding(DpToPx(16), DpToPx(10), DpToPx(16), DpToPx(10));
            txtHardwareId.SetTypeface(Typeface.Monospace, TypefaceStyle.Normal);

            var hwBgDrawable = new GradientDrawable();
            hwBgDrawable.SetShape(ShapeType.Rectangle);
            hwBgDrawable.SetColor(Color.Argb(10, 53, 93, 157));
            hwBgDrawable.SetCornerRadius(DpToPx(12));
            hwBgDrawable.SetStroke(DpToPx(1), Color.Argb(35, 53, 93, 157));
            txtHardwareId.Background = hwBgDrawable;

            var hwFieldParams = new LinearLayout.LayoutParams(0, DpToPx(45), 1f);
            txtHardwareId.LayoutParameters = hwFieldParams;

            var btnCopy = new Button(this)
            {
                Text = LocalizationManager.GetString("License_Copy", "Copy"),
                TextSize = 11f
            };
            btnCopy.SetTypeface(Typeface.SansSerif, TypefaceStyle.Bold);
            btnCopy.SetTextColor(Color.Rgb(53, 93, 157));
            btnCopy.SetPadding(0, 0, 0, 0);
            btnCopy.Gravity = GravityFlags.Center;

            var copyBtnBg = new GradientDrawable();
            copyBtnBg.SetShape(ShapeType.Rectangle);
            copyBtnBg.SetColor(Color.Rgb(241, 245, 249));
            copyBtnBg.SetCornerRadius(DpToPx(12));
            copyBtnBg.SetStroke(DpToPx(1), Color.Argb(35, 53, 93, 157));
            btnCopy.Background = copyBtnBg;

            var copyBtnParams = new LinearLayout.LayoutParams(DpToPx(70), DpToPx(45))
            {
                LeftMargin = isAr ? 0 : DpToPx(10),
                RightMargin = isAr ? DpToPx(10) : 0
            };
            btnCopy.LayoutParameters = copyBtnParams;

            btnCopy.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Down)
                {
                    copyBtnBg.SetColor(Color.Rgb(226, 232, 240));
                }
                else if (e.Event.Action == MotionEventActions.Up || e.Event.Action == MotionEventActions.Cancel)
                {
                    copyBtnBg.SetColor(Color.Rgb(241, 245, 249));
                }
                e.Handled = false;
            };

            btnCopy.Click += (s, e) =>
            {
                try
                {
                    var clipboard = (ClipboardManager)GetSystemService(Context.ClipboardService);
                    var clip = ClipData.NewPlainText("Hardware ID", txtHardwareId.Text);
                    clipboard.PrimaryClip = clip;
                    Toast.MakeText(this, LocalizationManager.GetString("License_Copied", "Hardware ID copied to clipboard!"), ToastLength.Short).Show();
                }
                catch (System.Exception ex)
                {
                    Toast.MakeText(this, LocalizationManager.GetString("License_CopyFailed", "Failed to copy to clipboard: ") + ex.Message, ToastLength.Short).Show();
                }
            };

            if (isAr)
            {
                hwRow.AddView(btnCopy);
                hwRow.AddView(txtHardwareId);
                // Adjust params
                hwFieldParams.LeftMargin = DpToPx(10);
                hwFieldParams.RightMargin = 0;
            }
            else
            {
                hwRow.AddView(txtHardwareId);
                hwRow.AddView(btnCopy);
            }
            hwGroup.AddView(hwRow);
            cardLayout.AddView(hwGroup);

            // 8. Customer Name Input Group
            EditText txtCustomer;
            var customerGroup = CreateInputGroup(
                LocalizationManager.GetString("License_CustomerName", "Customer / Company Name"),
                LocalizationManager.GetString("License_CustomerName", "Customer / Company Name"),
                out txtCustomer
            );
            cardLayout.AddView(customerGroup);

            // 9. License Key Input Group
            EditText txtKey;
            var keyGroup = CreateInputGroup(
                LocalizationManager.GetString("License_Key", "License Key"),
                LocalizationManager.GetString("License_Key", "License Key"),
                out txtKey
            );
            cardLayout.AddView(keyGroup);

            var buttonContainer = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal,
                LayoutDirection = isAr ? Android.Views.LayoutDirection.Rtl : Android.Views.LayoutDirection.Ltr
            };
            var btnContainerParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            {
                TopMargin = DpToPx(24)
            };
            buttonContainer.LayoutParameters = btnContainerParams;

            // Activate Button
            var btnActivate = new Button(this)
            {
                Text = LocalizationManager.GetString("License_Activate", "Activate"),
                TextSize = 13f
            };
            btnActivate.SetTypeface(Typeface.SansSerif, TypefaceStyle.Bold);
            btnActivate.SetTextColor(Color.White);

            var actBtnBg = new GradientDrawable();
            actBtnBg.SetShape(ShapeType.Rectangle);
            actBtnBg.SetColor(Color.Rgb(53, 93, 157)); // Primary brand color
            actBtnBg.SetCornerRadius(DpToPx(11));
            btnActivate.Background = actBtnBg;

            btnActivate.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Down)
                {
                    actBtnBg.SetColor(Color.Rgb(39, 73, 125));
                }
                else if (e.Event.Action == MotionEventActions.Up || e.Event.Action == MotionEventActions.Cancel)
                {
                    actBtnBg.SetColor(Color.Rgb(53, 93, 157));
                }
                e.Handled = false;
            };

            // Exit Button
            var btnExit = new Button(this)
            {
                Text = LocalizationManager.GetString("License_Exit", "Exit"),
                TextSize = 13f
            };
            btnExit.SetTypeface(Typeface.SansSerif, TypefaceStyle.Bold);
            btnExit.SetTextColor(Color.Rgb(15, 23, 42));

            var exitBtnBg = new GradientDrawable();
            exitBtnBg.SetShape(ShapeType.Rectangle);
            exitBtnBg.SetColor(Color.Rgb(241, 245, 249)); // Soft secondary color
            exitBtnBg.SetCornerRadius(DpToPx(11));
            btnExit.Background = exitBtnBg;

            btnExit.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Down)
                {
                    exitBtnBg.SetColor(Color.Rgb(226, 232, 240));
                }
                else if (e.Event.Action == MotionEventActions.Up || e.Event.Action == MotionEventActions.Cancel)
                {
                    exitBtnBg.SetColor(Color.Rgb(241, 245, 249));
                }
                e.Handled = false;
            };

            // Start Trial Button
            var btnStartTrial = new Button(this)
            {
                Text = LocalizationManager.GetString("License_StartTrial", "Start Trial"),
                TextSize = 13f
            };
            btnStartTrial.SetTypeface(Typeface.SansSerif, TypefaceStyle.Bold);
            btnStartTrial.SetTextColor(Color.Rgb(15, 23, 42));

            var trialBtnBg = new GradientDrawable();
            trialBtnBg.SetShape(ShapeType.Rectangle);
            trialBtnBg.SetColor(Color.Rgb(241, 245, 249)); // Soft secondary color
            trialBtnBg.SetCornerRadius(DpToPx(11));
            btnStartTrial.Background = trialBtnBg;

            btnStartTrial.Touch += (s, e) =>
            {
                if (e.Event.Action == MotionEventActions.Down)
                {
                    trialBtnBg.SetColor(Color.Rgb(226, 232, 240));
                }
                else if (e.Event.Action == MotionEventActions.Up || e.Event.Action == MotionEventActions.Cancel)
                {
                    trialBtnBg.SetColor(Color.Rgb(241, 245, 249));
                }
                e.Handled = false;
            };

            // Layout weights and margins for buttons: Activate | Exit | Start Trial
            var btnParams1 = new LinearLayout.LayoutParams(0, DpToPx(45), 1.2f);
            var btnParams2 = new LinearLayout.LayoutParams(0, DpToPx(45), 1f);
            var btnParams3 = new LinearLayout.LayoutParams(0, DpToPx(45), 1.3f);

            // Add standard padding/margin
            if (isAr)
            {
                btnParams1.LeftMargin = DpToPx(8);
                btnParams2.LeftMargin = DpToPx(8);
            }
            else
            {
                btnParams1.RightMargin = DpToPx(8);
                btnParams2.RightMargin = DpToPx(8);
            }

            btnActivate.LayoutParameters = btnParams1;
            btnExit.LayoutParameters = btnParams2;
            btnStartTrial.LayoutParameters = btnParams3;

            buttonContainer.AddView(btnActivate);
            buttonContainer.AddView(btnExit);
            buttonContainer.AddView(btnStartTrial);
            cardLayout.AddView(buttonContainer);

            // 11. Actions logic
            btnActivate.Click += (s, e) =>
            {
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

            btnExit.Click += (s, e) =>
            {
                dialog.Dismiss();
                Finish();
            };

            btnStartTrial.Click += (s, e) =>
            {
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

            // Create a ScrollView to prevent keyboard / screen size cutoff
            var scrollView = new ScrollView(this)
            {
                FillViewport = true,
                OverScrollMode = OverScrollMode.Never
            };
            var scrollParams = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = GravityFlags.Center,
                TopMargin = DpToPx(16),
                BottomMargin = DpToPx(16)
            };
            scrollView.LayoutParameters = scrollParams;
            scrollView.AddView(cardLayout);

            rootLayout.AddView(scrollView);
            dialog.SetContentView(rootLayout);
            dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

            dialog.Show();
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
