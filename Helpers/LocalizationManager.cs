using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace InventorySystem.Helpers
{
    public static class LocalizationManager
    {
        public static event EventHandler LanguageChanged;

        // Manually loaded Arabic resource set (bypasses satellite assemblies)
        private static ResourceSet _arabicResources;
        private static Dictionary<string, string> _arabicDictionary;
        private static bool _arabicResourcesLoaded = false;

        // Track RTL state without stomping on Control.Tag

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

        private static string _currentLanguage = "en";

        public static void SetLanguage(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture; // Guarantee ThreadPool/API threads inherit this!
            _currentLanguage = culture.TwoLetterISOLanguageName;

            // Keep date and number formatting consistent (Invariant) to prevent database/parsing errors
            // but allow UICulture to handle translations.
            var customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.CurrencySymbol = "$";
            Thread.CurrentThread.CurrentCulture = customCulture;
            CultureInfo.DefaultThreadCurrentCulture = customCulture;

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
                // The resource is embedded as "InventorySystem.Properties.Resources.ar.resx"
                using (var stream = assembly.GetManifestResourceStream("InventorySystem.Properties.Resources.ar"))
                {
                    if (stream != null)
                    {
                        _arabicResources = new ResourceSet(stream);
                        _arabicResourcesLoaded = true;
                        return;
                    }
                }

                // Fallback: Try loading from file path
                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string resxPath = Path.Combine(exeDir, "Properties", "Resources.ar.resx");
                if (!File.Exists(resxPath))
                {
                    resxPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Properties", "Resources.ar.resx");
                }

                if (File.Exists(resxPath))
                {
                    _arabicDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    try
                    {
                        // Use XDocument to avoid ResXResourceReader encoding issues
                        var doc = System.Xml.Linq.XDocument.Load(resxPath);
                        foreach (var data in doc.Root.Elements("data"))
                        {
                            string name = data.Attribute("name")?.Value;
                            string val = data.Element("value")?.Value;
                            if (!string.IsNullOrEmpty(name) && val != null)
                            {
                                _arabicDictionary[name] = val;
                            }
                        }
                        _arabicResourcesLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("XDocument load failed: " + ex.Message);
                    }
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
                    // Skip setting native text for custom-painted standard buttons
                    bool isStandardButton = c is Button && c.Tag != null && c.Tag.ToString().StartsWith("standard_");
                    
                    if (!isStandardButton)
                    {
                        string translated = GetString(c.Name);
                        if (translated != c.Name && !string.IsNullOrEmpty(translated))
                        {
                            c.Text = translated;
                        }
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

        public static string CurrentLanguage => _currentLanguage;
        public static bool IsArabic => CurrentLanguage == "ar";

        // Track RTL state without stomping on Control.Tag
        private static ConditionalWeakTable<Control, HashSet<string>> _rtlStates = new ConditionalWeakTable<Control, HashSet<string>>();

        private static bool HasRtlState(Control control, string state)
        {
            if (_rtlStates.TryGetValue(control, out var states)) return states.Contains(state);
            return false;
        }

        private static void MirrorTextAlign(Control control)
        {
            if (control is Label lbl)
            {
                lbl.TextAlign = SwapAlignment(lbl.TextAlign);
            }
            else if (control is Button btn)
            {
                btn.TextAlign = SwapAlignment(btn.TextAlign);
            }
        }

        private static ContentAlignment SwapAlignment(ContentAlignment alignment)
        {
            switch (alignment)
            {
                case ContentAlignment.TopLeft: return ContentAlignment.TopRight;
                case ContentAlignment.TopRight: return ContentAlignment.TopLeft;
                case ContentAlignment.MiddleLeft: return ContentAlignment.MiddleRight;
                case ContentAlignment.MiddleRight: return ContentAlignment.MiddleLeft;
                case ContentAlignment.BottomLeft: return ContentAlignment.BottomRight;
                case ContentAlignment.BottomRight: return ContentAlignment.BottomLeft;
                default: return alignment;
            }
        }

        private static void AddRtlState(Control control, string state)
        {
            if (!_rtlStates.TryGetValue(control, out var states))
            {
                states = new HashSet<string>();
                _rtlStates.Add(control, states);
            }
            states.Add(state);
        }

        private static void RemoveRtlState(Control control, string state)
        {
            if (_rtlStates.TryGetValue(control, out var states)) states.Remove(state);
        }

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

            control.RightToLeft = isAr ? RightToLeft.Yes : RightToLeft.No;

            // Skip manual location/anchor mirroring for internal components of ModernTextBox and StatCard
            // as they handle their own internal layout logic.
            bool isInternalHandled = (control.Parent != null && (control.Parent.GetType().Name == "ModernTextBox" || control.Parent.GetType().Name == "StatCard" || control.Parent.GetType().Name == "ModernNumericUpDown" || control.Parent.GetType().Name == "ModernComboBox")) ||
                                     (control.Parent != null && control.Parent.Parent != null && (control.Parent.Parent.GetType().Name == "ModernTextBox" || control.Parent.Parent.GetType().Name == "StatCard" || control.Parent.Parent.GetType().Name == "ModernNumericUpDown" || control.Parent.Parent.GetType().Name == "ModernComboBox"));

            if (!isInternalHandled)
            {
                // Handle Docking Mirroring for all controls (Labels, Buttons, Panels, etc.)
                if (control.Dock == DockStyle.Left || control.Dock == DockStyle.Right)
                {
                    bool isSwapped = HasRtlState(control, "rtl_dock_swapped");
                    if (isAr && !isSwapped)
                    {
                        control.Dock = (control.Dock == DockStyle.Left) ? DockStyle.Right : DockStyle.Left;
                        AddRtlState(control, "rtl_dock_swapped");
                    }
                    else if (!isAr && isSwapped)
                    {
                        control.Dock = (control.Dock == DockStyle.Left) ? DockStyle.Right : DockStyle.Left;
                        RemoveRtlState(control, "rtl_dock_swapped");
                    }
                }
            }
            




            // Handle Chart Mirroring
            if (control.GetType().FullName == "System.Windows.Forms.DataVisualization.Charting.Chart")
            {
                dynamic chart = control;
                foreach (var title in chart.Titles)
                {
                    title.Alignment = isAr ? ContentAlignment.TopRight : ContentAlignment.TopLeft;
                }
                
                foreach (var legend in chart.Legends)
                {
                    if (legend.Docking == System.Windows.Forms.DataVisualization.Charting.Docking.Top || 
                        legend.Docking == System.Windows.Forms.DataVisualization.Charting.Docking.Bottom)
                    {
                        legend.Alignment = isAr ? StringAlignment.Far : StringAlignment.Near;
                    }
                }
            }

            // RightToLeft property handles TableLayoutPanel column mirroring automatically
            // No manual MirrorTableLayout needed.

            if (!isInternalHandled)
            {
                // Handle Absolute Location Mirroring for child controls (if parent is not a layout panel)
                if (isAr && control.Parent != null && !(control.Parent is TableLayoutPanel || control.Parent is FlowLayoutPanel))
                {
                    if (!HasRtlState(control, "rtl_loc_swapped"))
                    {
                        control.Location = new Point(control.Parent.ClientSize.Width - control.Location.X - control.Width, control.Location.Y);
                        AddRtlState(control, "rtl_loc_swapped");
                    }
                }
                else if (!isAr && control.Parent != null)
                {
                    if (HasRtlState(control, "rtl_loc_swapped"))
                    {
                        control.Location = new Point(control.Parent.ClientSize.Width - control.Location.X - control.Width, control.Location.Y);
                        RemoveRtlState(control, "rtl_loc_swapped");
                    }
                }

                // Swap Anchors (Only for controls that are not Docked, as setting Anchor resets Dock to None)
                // Skip for TLP/FLP children as these containers handle mirroring via RightToLeft property
                bool isAnchorSwapped = HasRtlState(control, "rtl_anchor_swapped");
                bool isLayoutChild = control.Parent != null && (control.Parent is TableLayoutPanel || control.Parent is FlowLayoutPanel);

                if (isAr && !isAnchorSwapped && control.Dock == DockStyle.None)
                {
                    if ((control.Anchor & AnchorStyles.Left) == AnchorStyles.Left && (control.Anchor & AnchorStyles.Right) != AnchorStyles.Right)
                    {
                        control.Anchor = (control.Anchor & ~AnchorStyles.Left) | AnchorStyles.Right;
                        AddRtlState(control, "rtl_anchor_swapped");
                    }
                    else if ((control.Anchor & AnchorStyles.Right) == AnchorStyles.Right && (control.Anchor & AnchorStyles.Left) != AnchorStyles.Left)
                    {
                        control.Anchor = (control.Anchor & ~AnchorStyles.Right) | AnchorStyles.Left;
                        AddRtlState(control, "rtl_anchor_swapped");
                    }
                }
                else if (!isAr && isAnchorSwapped && control.Dock == DockStyle.None)
                {
                    if ((control.Anchor & AnchorStyles.Left) == AnchorStyles.Left && (control.Anchor & AnchorStyles.Right) != AnchorStyles.Right)
                    {
                        control.Anchor = (control.Anchor & ~AnchorStyles.Left) | AnchorStyles.Right;
                    }
                    else if ((control.Anchor & AnchorStyles.Right) == AnchorStyles.Right && (control.Anchor & AnchorStyles.Left) != AnchorStyles.Left)
                    {
                        control.Anchor = (control.Anchor & ~AnchorStyles.Right) | AnchorStyles.Left;
                    }
                    RemoveRtlState(control, "rtl_anchor_swapped");
                }
            }



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

