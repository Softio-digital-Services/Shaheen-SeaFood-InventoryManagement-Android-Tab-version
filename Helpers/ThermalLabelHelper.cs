using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using InventorySystem.Services;
using InventorySystem.Forms;
using System.IO;

namespace InventorySystem.Helpers
{
    public class LabelPrintItem
    {
        public string Name { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
    public class ThermalLabelHelper
    {
        private static List<LabelPrintItem> _currentItems;
        private static int _itemIndex = 0;
        private static int _copyIndex = 0;

        /// <summary>
        /// Generates a print document optimized for 2x1 inch (50mm x 25mm) thermal labels.
        /// Opens a PrintPreviewDialog allowing the user to view labels before printing.
        /// </summary>
        public static void GenerateLabelPDF(List<LabelPrintItem> items)
        {
            _currentItems = items;
            _itemIndex = 0;
            _copyIndex = 0;

            PrintDocument pd = new PrintDocument();
            
            // Standard 2x1 inch label size (100 units = 1 inch)
            pd.DefaultPageSettings.PaperSize = new PaperSize("Label 2x1", 200, 100); 
            pd.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

            pd.PrintPage += Pd_PrintPage;

            // Use PrintPreviewDialog to allow viewing before printing
            using (PrintPreviewDialog preview = new PrintPreviewDialog())
            {
                preview.Document = pd;
                preview.Text = LocalizationManager.IsArabic ? "معاينة الملصقات" : "Label Preview";
                
                ThemeConfig.ApplyPrintPreviewTheme(preview);

                if (preview.ShowDialog() == DialogResult.OK)
                {
                    // Print dialog is usually handled within the preview window's print button
                }
            }
        }

        private static void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_itemIndex >= _currentItems.Count)
            {
                e.HasMorePages = false;
                return;
            }

            var item = _currentItems[_itemIndex];
            Graphics g = e.Graphics;
            BarcodeService bs = new BarcodeService();

            // Fonts
            Font nameFont = new Font("Segoe UI", 8, FontStyle.Bold);
            Font skuFont = new Font("Consolas", 7);
            Font priceFont = new Font("Segoe UI", 8, FontStyle.Bold);

            // 1. Draw Product Name (Centered top)
            string displayName = item.Name.Length > 25 ? item.Name.Substring(0, 22) + "..." : item.Name;
            g.DrawString(displayName, nameFont, Brushes.Black, new RectangleF(0, 5, 200, 20), new StringFormat { Alignment = StringAlignment.Center });

            // 2. Draw Barcode (Code 128)
            // Use high quality interpolation for the barcode image
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            Bitmap barcode = bs.RenderCode128(item.SKU, 180, 45);
            g.DrawImage(barcode, new Rectangle(10, 25, 180, 45));

            // 3. Draw SKU Text (Centered below barcode)
            g.DrawString(item.SKU, skuFont, Brushes.Black, new RectangleF(0, 71, 200, 14), new StringFormat { Alignment = StringAlignment.Center });

            // 4. Draw Price Text (Centered bottom)
            string priceStr = CurrencyService.Format(item.Price);
            g.DrawString(priceStr, priceFont, Brushes.Black, new RectangleF(0, 84, 200, 15), new StringFormat { Alignment = StringAlignment.Center });

            // Logic to handle multiple copies per item
            _copyIndex++;
            if (_copyIndex >= item.Quantity)
            {
                _copyIndex = 0;
                _itemIndex++;
            }

            // If we have more items or more copies of the current item, tell the printer to continue
            e.HasMorePages = (_itemIndex < _currentItems.Count);
        }
    }
}
