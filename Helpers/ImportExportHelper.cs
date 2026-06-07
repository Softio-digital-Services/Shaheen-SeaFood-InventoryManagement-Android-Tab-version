using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Helper class for importing and exporting data to CSV and Excel formats
    /// </summary>
    public static class ImportExportHelper
    {
        #region CSV Export/Import

        /// <summary>
        /// Exports a DataTable to CSV file
        /// </summary>
        public static bool ExportToCsv(DataTable dataTable, string filePath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                // Write headers
                IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>()
                    .Select(column => EscapeCsvField(column.ColumnName));
                sb.AppendLine(string.Join(",", columnNames));

                // Write rows
                foreach (DataRow row in dataTable.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray
                        .Select(field => EscapeCsvField(field?.ToString() ?? ""));
                    sb.AppendLine(string.Join(",", fields));
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ImportExportHelper.ExportToCsv");
                return false;
            }
        }

        /// <summary>
        /// Imports data from CSV file to DataTable
        /// </summary>
        public static DataTable ImportFromCsv(string filePath)
        {
            try
            {
                DataTable dt = new DataTable();
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length == 0)
                    return dt;

                // Parse headers
                string[] headers = ParseCsvLine(lines[0]);
                foreach (string header in headers)
                {
                    dt.Columns.Add(header.Trim());
                }

                // Parse data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    string[] fields = ParseCsvLine(lines[i]);
                    
                    // Ensure we have the right number of fields
                    if (fields.Length == dt.Columns.Count)
                    {
                        dt.Rows.Add(fields);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ImportExportHelper.ImportFromCsv");
                return new DataTable();
            }
        }

        #endregion

        #region Excel Export/Import (Simple TSV Format)

        /// <summary>
        /// Exports a DataTable to Excel-compatible TSV file
        /// </summary>
        public static bool ExportToExcel(DataTable dataTable, string filePath, string sheetName = "Sheet1")
        {
            try
            {
                // For now, export as tab-separated values which Excel can open
                // This avoids the ClosedXML dependency issue
                StringBuilder sb = new StringBuilder();

                // Write headers
                IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>()
                    .Select(column => column.ColumnName);
                sb.AppendLine(string.Join("\t", columnNames));

                // Write rows
                foreach (DataRow row in dataTable.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray
                        .Select(field => field?.ToString()?.Replace("\t", " ") ?? "");
                    sb.AppendLine(string.Join("\t", fields));
                }

                // Save with .xls extension (Excel will open TSV files)
                string tsvPath = filePath.Replace(".xlsx", ".xls");
                File.WriteAllText(tsvPath, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ImportExportHelper.ExportToExcel");
                return false;
            }
        }

        /// <summary>
        /// Imports data from Excel-compatible TSV file to DataTable
        /// </summary>
        public static DataTable ImportFromExcel(string filePath, string sheetName = null)
        {
            try
            {
                DataTable dt = new DataTable();
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length == 0)
                    return dt;

                // Parse headers
                string[] headers = lines[0].Split('\t');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header.Trim());
                }

                // Parse data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    string[] fields = lines[i].Split('\t');
                    
                    // Ensure we have the right number of fields
                    if (fields.Length == dt.Columns.Count)
                    {
                        dt.Rows.Add(fields);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ImportExportHelper.ImportFromExcel");
                return new DataTable();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Escapes a CSV field by wrapping in quotes if needed
        /// </summary>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // If field contains comma, quote, or newline, wrap in quotes
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                // Escape existing quotes by doubling them
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }

        /// <summary>
        /// Parses a CSV line handling quoted fields
        /// </summary>
        private static string[] ParseCsvLine(string line)
        {
            List<string> fields = new List<string>();
            bool inQuotes = false;
            StringBuilder currentField = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // Check for escaped quote
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            // Add last field
            fields.Add(currentField.ToString());

            return fields.ToArray();
        }

        /// <summary>
        /// Creates a sample CSV template file
        /// </summary>
        public static void CreateCsvTemplate(string filePath, string[] headers)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Join(",", headers));
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ImportExportHelper.CreateCsvTemplate");
            }
        }

        #endregion
    }
}
