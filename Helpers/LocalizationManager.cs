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

        /// <summary>
        /// Recursively applies RTL (or LTR) to a control tree.
        /// - Sets RightToLeft on every control.
        /// - Mirrors DockStyle.Left <-> DockStyle.Right on Panels.
        /// - Reverses FlowDirection on FlowLayoutPanels.
        /// </summary>
        public static void ApplyRTL(Control control)
        {
            if (control == null) return;
            bool isAr = IsArabic;

            // Handle Docking Mirroring
            if (control is Panel p && (p.Dock == DockStyle.Left || p.Dock == DockStyle.Right))
            {
                if (isAr && p.Tag?.ToString() != "rtl_dock_swapped")
                {
                    p.Dock = (p.Dock == DockStyle.Left) ? DockStyle.Right : DockStyle.Left;
                    p.Tag = "rtl_dock_swapped";
                }
                else if (!isAr && p.Tag?.ToString() == "rtl_dock_swapped")
                {
                    p.Dock = (p.Dock == DockStyle.Left) ? DockStyle.Right : DockStyle.Left;
                    p.Tag = null;
                }
            }

            // Handle FlowLayoutPanel Mirroring
            if (control is FlowLayoutPanel flow)
            {
                if (isAr && flow.Tag?.ToString() != "rtl_flow_swapped")
                {
                    flow.FlowDirection = (flow.FlowDirection == FlowDirection.LeftToRight) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                    flow.Tag = "rtl_flow_swapped";
                }
                else if (!isAr && flow.Tag?.ToString() == "rtl_flow_swapped")
                {
                    flow.FlowDirection = (flow.FlowDirection == FlowDirection.LeftToRight) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                    flow.Tag = null;
                }
            }

            // Handle TableLayoutPanel Column Mirroring
            if (control is TableLayoutPanel tlp && tlp.ColumnCount > 1)
            {
                if (isAr && tlp.Tag?.ToString() != "rtl_tlp_swapped")
                {
                    MirrorTableLayout(tlp);
                    tlp.Tag = "rtl_tlp_swapped";
                }
                else if (!isAr && tlp.Tag?.ToString() == "rtl_tlp_swapped")
                {
                    MirrorTableLayout(tlp);
                    tlp.Tag = null;
                }
            }

            // Handle Absolute Location Mirroring for child controls (if parent is not a layout panel)
            if (isAr && !(control.Parent is TableLayoutPanel || control.Parent is FlowLayoutPanel))
            {
                if (control.Tag?.ToString() != "rtl_loc_swapped")
                {
                    control.Location = new Point(control.Parent.ClientSize.Width - control.Location.X - control.Width, control.Location.Y);
                    control.Tag = "rtl_loc_swapped";
                }
            }
            else if (!isAr && control.Tag?.ToString() == "rtl_loc_swapped")
            {
                control.Location = new Point(control.Parent.ClientSize.Width - control.Location.X - control.Width, control.Location.Y);
                control.Tag = null;
            }

            // Mirror Label/Button text alignment
            if (control is Label lbl)
            {
                if (isAr && (lbl.TextAlign == ContentAlignment.MiddleLeft || lbl.TextAlign == ContentAlignment.TopLeft || lbl.TextAlign == ContentAlignment.BottomLeft))
                {
                    if (lbl.TextAlign == ContentAlignment.MiddleLeft) lbl.TextAlign = ContentAlignment.MiddleRight;
                    else if (lbl.TextAlign == ContentAlignment.TopLeft) lbl.TextAlign = ContentAlignment.TopRight;
                    else if (lbl.TextAlign == ContentAlignment.BottomLeft) lbl.TextAlign = ContentAlignment.BottomRight;
                }
            }
            else if (control is Button btn)
            {
                if (isAr && btn.TextAlign == ContentAlignment.MiddleLeft) btn.TextAlign = ContentAlignment.MiddleRight;
            }

            control.RightToLeft = isAr ? RightToLeft.Yes : RightToLeft.No;

            foreach (Control child in control.Controls)
                ApplyRTL(child);
        }

        private static void MirrorTableLayout(TableLayoutPanel tlp)
        {
            int maxCol = tlp.ColumnCount - 1;
            var controls = new List<Control>();
            var positions = new List<TableLayoutPanelCellPosition>();

            foreach (Control c in tlp.Controls)
            {
                controls.Add(c);
                positions.Add(tlp.GetPositionFromControl(c));
            }

            for (int i = 0; i < controls.Count; i++)
            {
                tlp.SetColumn(controls[i], maxCol - positions[i].Column);
            }
        }

        /// <summary>
        /// Reverses the FlowDirection of a FlowLayoutPanel for RTL.
        /// </summary>
        public static void ApplyRTLToFlowLayout(FlowLayoutPanel flow)
        {
            if (flow == null) return;
            flow.FlowDirection = IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        /// <summary>
        /// Mirrors an X position inside a container for fixed-position controls.
        /// </summary>
        public static int MirrorX(Control control, Control parent)
        {
            return parent.ClientSize.Width - control.Location.X - control.Width;
        }
    }
}

