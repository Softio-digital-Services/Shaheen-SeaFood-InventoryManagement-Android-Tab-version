using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;
using GenericInventorySystem.Services;

namespace GenericInventorySystem.Forms
{
    /// <summary>
    /// Settings panel for managing supported currencies and their exchange rates.
    /// </summary>
    public class CurrencySettingsForm : BaseModalForm
    {
        private DataGridView dgvRates;
        private Label lblStatus;
        private Button btnRefresh;

        public CurrencySettingsForm()
        {
            this.Width = 800;
            InitializeForm();
            LoadRates();
        }

        private void InitializeForm()
        {
            GenericInventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            bool ar = GenericInventorySystem.Helpers.LocalizationManager.IsArabic;
            
            // Adaptive sizing handled by BaseModalForm.OnLoad
            this.TitleText = ar ? "إعدادات العملة" : "Currency Settings";

            this.SuspendLayout();

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(25)
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F)); // Header
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // Status
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Grid
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // Info

            // ─── Header ──────────────────────────────────────────────────
            TableLayoutPanel tlpHeader = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));

            tlpHeader.Controls.Add(new Control(), 0, 0); // Spacer

            Button btnAdd = new ModernButton
            {
                Text = ar ? "إضافة عملة" : "Add Currency",
                Height = 45,
                Dock = DockStyle.Top,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 10, 10, 0)
            };
            ThemeConfig.ApplySecondaryButton(btnAdd);
            btnAdd.Click += BtnAdd_Click;
            tlpHeader.Controls.Add(btnAdd, 1, 0);

            btnRefresh = new ModernButton
            {
                Text = ar ? "تحديث الأسعار" : "Refresh Live Rates",
                Height = 45,
                Dock = DockStyle.Top,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 10, 0, 0)
            };
            ThemeConfig.ApplyPrimaryButton(btnRefresh);
            btnRefresh.Click += BtnRefresh_Click;
            tlpHeader.Controls.Add(btnRefresh, 2, 0);

            tlpMain.Controls.Add(tlpHeader, 0, 0);

            // ─── Status label ─────────────────────────────────────────────
            lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ForeColor = ThemeConfig.SecondaryColor,
                Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                TextAlign = ContentAlignment.BottomLeft
            };
            tlpMain.Controls.Add(lblStatus, 0, 1);

            // ─── Grid ─────────────────────────────────────────────────────
            dgvRates = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvRates.DataError += (s, e) => { e.ThrowException = false; };
            ThemeConfig.ApplyGridTheme(dgvRates);

            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "code", HeaderText = ar ? "الرمز" : "Code", DataPropertyName = "code", Width = 60, ReadOnly = true });
            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "name", HeaderText = ar ? "الاسم" : "Name", DataPropertyName = "name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = false });
            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "symbol", HeaderText = ar ? "العلامة" : "Symbol", DataPropertyName = "symbol", Width = 70, ReadOnly = false, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "rate_vs_usd", HeaderText = ar ? "السعر مقابل USD" : "Rate vs USD", DataPropertyName = "rate_vs_usd", Width = 130, ReadOnly = false, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "last_updated", HeaderText = ar ? "آخر تحديث" : "Last Updated", DataPropertyName = "last_updated", Width = 150, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });

            // Context Menu for Deletion
            ContextMenuStrip cms = new ContextMenuStrip();
            var tsmiDelete = new ToolStripMenuItem(ar ? "حذف" : "Delete");
            tsmiDelete.Click += (s, e) => {
                if (dgvRates.CurrentRow != null) {
                    string code = dgvRates.CurrentRow.Cells["code"].Value.ToString();
                    if (code == "USD") {
                        MessageHelper.ShowWarning(ar ? "لا يمكن حذف العملة الأساسية (USD)" : "Cannot delete base currency (USD)");
                        return;
                    }
                    if (MessageHelper.ShowConfirm(ar ? $"هل أنت متأكد من حذف {code}؟" : $"Are you sure you want to delete {code}?")) {
                        DatabaseHelper.ExecuteNonQuery($"DELETE FROM currency_rates WHERE code = '{code}'");
                        CurrencyService.LoadRatesFromDb();
                        LoadRates();
                    }
                }
            };
            cms.Items.Add(tsmiDelete);
            dgvRates.ContextMenuStrip = cms;

            tlpMain.Controls.Add(dgvRates, 0, 2);

            // ─── Info box ─────────────────────────────────────────────────
            Label lblInfo = new Label
            {
                Text = ar 
                    ? "الأسعار بالنسبة للدولار. يمكنك تعديل الحقول واضغط Enter للحفظ. انقر بزر الماوس الأيمن للحذف." 
                    : "Rates are relative to USD. You can edit cells and press Enter. Right-click to delete.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = ThemeConfig.SecondaryColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            tlpMain.Controls.Add(lblInfo, 0, 3);

            this.ContentPanel.Controls.Add(tlpMain);

            SetFooterButtons(
                ar ? "حفظ التعديلات" : "Save Changes",
                ar ? "إغلاق" : "Close",
                BtnSave_Click,
                (s, e) => this.Close()
            );

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            bool ar = LocalizationManager.IsArabic;
            string code = Microsoft.VisualBasic.Interaction.InputBox(ar ? "أدخل رمز العملة (مثلاً SAR):" : "Enter Currency Code (e.g., SAR):", ar ? "إضافة عملة" : "Add Currency", "").ToUpper();
            if (string.IsNullOrWhiteSpace(code) || code.Length > 10) return;

            // Check if exists
            var check = DatabaseHelper.ExecuteScalar<int>($"SELECT COUNT(*) FROM currency_rates WHERE code = '{code}'");
            if (Convert.ToInt32(check) > 0) {
                MessageHelper.ShowWarning(ar ? "هذه العملة موجودة بالفعل" : "This currency already exists");
                return;
            }

            string name = Microsoft.VisualBasic.Interaction.InputBox(ar ? "أدخل اسم العملة:" : "Enter Currency Name:", ar ? "إضافة عملة" : "Add Currency", code);
            string symbol = Microsoft.VisualBasic.Interaction.InputBox(ar ? "أدخل رمز العملة:" : "Enter Currency Symbol:", ar ? "إضافة عملة" : "Add Currency", "$");

            DatabaseHelper.ExecuteNonQuery($"INSERT INTO currency_rates (code, name, symbol, rate_vs_usd) VALUES ('{code}', '{name}', '{symbol}', 1)");
            CurrencyService.LoadRatesFromDb();
            LoadRates();
        }

        private void LoadRates()
        {
            try
            {
                var dt = CurrencyService.GetAllCurrencies();
                dgvRates.DataSource = dt;

                // Find latest update
                string lastUpdate = "—";
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    if (row["last_updated"] != DBNull.Value)
                    {
                        lastUpdate = Convert.ToDateTime(row["last_updated"]).ToString("g");
                        break;
                    }
                }
                lblStatus.Text = GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? $"آخر تحديث: {lastUpdate}" : $"Last rate update: {lastUpdate}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? $"لم يتم الجلب: {ex.Message}" : "Could not load rates: " + ex.Message;
            }
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            bool ar = GenericInventorySystem.Helpers.LocalizationManager.IsArabic;
            btnRefresh.Enabled = false;
            btnRefresh.Text    = ar ? "جاري الجلب..." : "Fetching...";
            lblStatus.ForeColor = ThemeConfig.SecondaryColor;
            lblStatus.Text     = ar ? "جاري الاتصال بخدمة الأسعار..." : "Connecting to exchange rate service...";

            var rates = await CurrencyService.FetchLiveRatesAsync();

            if (rates != null && rates.Count > 1)
            {
                CurrencyService.SaveRatesToDb(rates);
                LoadRates();
                lblStatus.ForeColor = ThemeConfig.SuccessColor;
                lblStatus.Text      = ar ? $"تم التحديث: {DateTime.Now:g}" : $"Rates updated at {DateTime.Now:g}";
            }
            else
            {
                lblStatus.ForeColor = ThemeConfig.DangerColor;
                lblStatus.Text      = ar ? "تعذر الاتصال. يتم استخدام الأسعار المخزنة." : "Could not reach server. Using cached rates.";
            }

            btnRefresh.Enabled = true;
            btnRefresh.Text    = ar ? "تحديث الأسعار" : "Refresh Live Rates";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            dgvRates.EndEdit();
            foreach (DataGridViewRow row in dgvRates.Rows)
            {
                if (row.IsNewRow) continue;
                string code = row.Cells["code"].Value?.ToString();
                string name = row.Cells["name"].Value?.ToString();
                string symbol = row.Cells["symbol"].Value?.ToString();
                
                if (decimal.TryParse(row.Cells["rate_vs_usd"].Value?.ToString(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.CurrentCulture, out decimal rate))
                {
                    CurrencyService.UpdateCurrency(code, name, symbol, rate);
                }
            }
            
            lblStatus.ForeColor = ThemeConfig.SuccessColor;
            lblStatus.Text = GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? "تم حفظ جميع التعديلات" : "All changes saved successfully";
            LoadRates();
        }
    }
}
