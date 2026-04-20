using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace GenericInventorySystem.Helpers
{
    public static class LocalizationManager
    {
        public static event EventHandler LanguageChanged;

        // Manually loaded Arabic resource set (bypasses satellite assemblies)
        private static ResourceSet _arabicResources;
        private static Dictionary<string, string> _arabicDictionary;
        private static bool _arabicResourcesLoaded = false;
        
        public static bool IsArabicBuild
        {
            get
            {
#if ARABIC_VERSION
                return true;
#else
                return false;
#endif
            }
        }
        
        public static void SetLanguage(string cultureCode)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
            
            // Keep the currency format consistent (e.g. '$') instead of changing to SAR when Arabic is selected
            var customCulture = (CultureInfo)new CultureInfo(cultureCode).Clone();
            customCulture.NumberFormat.CurrencySymbol = "$";
            Thread.CurrentThread.CurrentCulture = customCulture;
            
            // Load Arabic resources on first Arabic activation
            if (IsArabic && !_arabicResourcesLoaded)
            {
                LoadArabicResources();
            }
            
            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }
        
        private static void LoadArabicResources()
        {
            try
            {
                // Load from embedded resource stream
                var assembly = Assembly.GetExecutingAssembly();
                // The resource is embedded as "GenericInventorySystem.Properties.Resources.ar.resx"
                using (var stream = assembly.GetManifestResourceStream("GenericInventorySystem.Properties.Resources.ar"))
                {
                    if (stream != null)
                    {
                        _arabicResources = new ResourceSet(stream);
                        _arabicResourcesLoaded = true;
                        return;
                    }
                }
                
                // Fallback: Try loading from file path (for development)
                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string resxPath = Path.Combine(exeDir, "Properties", "Resources.ar.resx");
                if (!File.Exists(resxPath))
                {
                    // Try project source path
                    resxPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Properties", "Resources.ar.resx");
                }
                if (File.Exists(resxPath))
                {
                    using (var reader = new ResXResourceReader(resxPath))
                    {
                        _arabicResources = new ResourceSet(reader);
                        _arabicDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (System.Collections.DictionaryEntry entry in _arabicResources)
                        {
                            if (entry.Value is string s)
                                _arabicDictionary[entry.Key.ToString()] = s;
                        }
                    }
                    _arabicResourcesLoaded = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load Arabic resources: " + ex.Message);
            }
        }
        
        // Get a localized string: use Arabic set if available for Arabic, otherwise fallback to default ResourceManager
        public static string GetString(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";

            if (IsArabic && _arabicResourcesLoaded && _arabicDictionary != null)
            {
                if (_arabicDictionary.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
                    return value;
            }

            try
            {
                return Properties.Resources.ResourceManager.GetString(key) ?? key;
            }
            catch
            {
                return key;
            }
        }

        public static void TranslateControl(Control parent)
        {
            if (parent == null) return;

            foreach (Control c in parent.Controls)
            {
                if (c.HasChildren) TranslateControl(c);

                if (c is Button || c is Label || c is CheckBox || c is RadioButton)
                {
                    string translated = GetString(c.Name);
                    if (translated != c.Name && !string.IsNullOrEmpty(translated)) 
                    {
                        c.Text = translated;
                    }
                }
                
                if (c is TabControl tabCtrl)
                {
                    foreach (TabPage page in tabCtrl.TabPages)
                    {
                        string pageTrans = GetString(page.Name);
                        if (pageTrans != page.Name && !string.IsNullOrEmpty(pageTrans)) page.Text = pageTrans;
                        TranslateControl(page); // Recurse into pages
                    }
                }

                if (c is DataGridView dgv)
                {
                    foreach (DataGridViewColumn col in dgv.Columns)
                    {
                        string colTrans = GetString(col.Name);
                        if (colTrans != col.Name && !string.IsNullOrEmpty(colTrans)) col.HeaderText = colTrans;
                    }
                }
            }
        }
        
        public static string CurrentLanguage => Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
        public static bool IsArabic => CurrentLanguage == "ar";
        
        public static void ApplyRTL(Control control)
        {
            RightToLeft desiredRTL = IsArabic ? RightToLeft.Yes : RightToLeft.No;
            bool languageSwitched = control.RightToLeft != desiredRTL && control.RightToLeft != RightToLeft.Inherit; 
            
            // On first load, default is Inherit or No. If IsArabic is true, languageSwitched will be true if it was No.
            // If it was Inherit, it might not swap properly if we don't handle it. Let's force check the parent if inherited.
            bool needsSwap = false;
            
            if (control.RightToLeft == RightToLeft.Inherit) {
                needsSwap = IsArabic; // If it's inherited, it's fresh. Swap if Arabic.
            } else {
                needsSwap = control.RightToLeft != desiredRTL;
            }

            if (needsSwap)
            {
                // Handle Docking Swaps for Panels (Left <-> Right) when switching RTL state
                if (control is Panel p && (p.Dock == DockStyle.Left || p.Dock == DockStyle.Right))
                {
                    p.Dock = (p.Dock == DockStyle.Left) ? DockStyle.Right : DockStyle.Left;
                }
            }

            control.RightToLeft = desiredRTL;

            foreach (Control child in control.Controls)
            {
                ApplyRTL(child);
            }
        }
    }
}

