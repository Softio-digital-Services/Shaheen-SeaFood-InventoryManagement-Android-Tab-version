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
    public class CurrencySettingsForm : Form
    {
        private DataGridView dgvRates;
        private Label lblStatus;
        private Button btnRefresh;

        public CurrencySettingsForm()
        {
            InitializeForm();
            LoadRates();
        }

        private void InitializeForm()
        {
            GenericInventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            bool ar = GenericInventorySystem.Helpers.LocalizationManager.IsArabic;
            
            this.Text            = ar ? "\u0625\u0639\u062F\u0627\u062F\u0627\u062A \u0627\u0644\u0639\u0645\u0644\u0629" : "Currency Settings";
            this.Size            = new Size(700, 500);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.BackColor       = ThemeConfig.BackgroundColor;
            this.Font            = ThemeConfig.StandardFont;

            // ─── Title ────────────────────────────────────────────────────
            Label lblTitle = ThemeConfig.CreateStandardHeader(ar ? "\u0627\u0644\u0639\u0645\u0644\u0627\u062A \u0648\u0623\u0633\u0639\u0627\u0631 \u0627\u0644\u0635\u0631\u0641" : "Currency & Exchange Rates");
            lblTitle.Location = new Point(20, 16);
            this.Controls.Add(lblTitle);

            // ─── Subtitle ─────────────────────────────────────────────────
            Label lblSub = new Label
            {
                Text      = ar ? "\u0627\u0644\u0623\u0633\u0639\u0627\u0631 \u0628\u0627\u0644\u0646\u0633\u0628\u0629 \u0644\u0644\u062F\u0648\u0644\u0627\u0631. \u0627\u0646\u0642\u0631 \u062A\u062D\u062F\u064A\u062B \u0644\u062C\u0644\u0628 \u0623\u062D\u062F\u062B \u0627\u0644\u0623\u0633\u0639\u0627\u0631." : "Rates are relative to USD. Click Refresh to fetch live rates.",
                Font      = ThemeConfig.StandardFont,
                ForeColor = ThemeConfig.SecondaryColor,
                Location  = new Point(22, 55),
                AutoSize  = true
            };
            this.Controls.Add(lblSub);

            // ─── Refresh button ───────────────────────────────────────────
            btnRefresh = new ModernButton
            {
                Text      = ar ? "\u062A\u062D\u062F\u064A\u062B \u0627\u0644\u0623\u0633\u0639\u0627\u0631" : "Refresh Live Rates",
                Size      = new Size(175, 38),
                Location  = new Point(490, 38), // Aligned with table right edge
                Cursor    = Cursors.Hand
            };
            ThemeConfig.ApplyPrimaryButton(btnRefresh);
            btnRefresh.Click += BtnRefresh_Click;
            this.Controls.Add(btnRefresh);

            // ─── Status label ─────────────────────────────────────────────
            lblStatus = new Label
            {
                Location  = new Point(22, 85),
                AutoSize  = true,
                ForeColor = ThemeConfig.SecondaryColor,
                Font      = new Font("Segoe UI", 8f, FontStyle.Italic)
            };
            this.Controls.Add(lblStatus);

            // ─── Grid ─────────────────────────────────────────────────────
            Panel pnlCard = new Panel
            {
                Location  = new Point(20, 110),
                Size      = new Size(645, 240),
                BackColor = Color.White
            };
            pnlCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeConfig.BorderColor, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
            };
            this.Controls.Add(pnlCard);

            dgvRates = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                AllowUserToAddRows    = false,
                RowHeadersVisible     = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                AutoGenerateColumns   = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvRates.DataError += (s, e) => { e.ThrowException = false; };
            ThemeConfig.ApplyGridTheme(dgvRates);

            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "code",         HeaderText = ar ? "\u0627\u0644\u0631\u0645\u0632" : "Code",        DataPropertyName = "code",         Width = 60, ReadOnly = true });
            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "name",         HeaderText = ar ? "\u0627\u0644\u0627\u0633\u0645" : "Name",        DataPropertyName = "name",         AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "symbol",       HeaderText = ar ? "\u0627\u0644\u0639\u0644\u0627\u0645\u0629" : "Symbol",      DataPropertyName = "symbol",       Width = 70, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "rate_vs_usd",  HeaderText = ar ? "\u0627\u0644\u0633\u0639\u0631 \u0645\u0642\u0627\u0628\u0644 USD" : "Rate vs USD", DataPropertyName = "rate_vs_usd",  Width = 130, ReadOnly = false, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvRates.Columns.Add(new DataGridViewTextBoxColumn { Name = "last_updated", HeaderText = ar ? "\u0622\u062E\u0631 \u062A\u062D\u062F\u064A\u062B" : "Last Updated",DataPropertyName = "last_updated",  Width = 150, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });

            pnlCard.Controls.Add(dgvRates);

            // ─── Info box ─────────────────────────────────────────────────
            Label lblInfo = new Label
            {
                Text      = ar ? "\u064A\u0645\u0643\u0646\u0643 \u062A\u0639\u062F\u064A\u0644 '\u0627\u0644\u0633\u0639\u0631 \u0645\u0642\u0627\u0628\u0644 USD' \u064A\u062F\u0648\u064A\u0627\u064B \u0648\u0627\u0636\u063A\u0637 Enter \u0644\u0644\u062D\u0641\u0638." : "You can manually edit the 'Rate vs USD' column and press Enter to save.",
                Location  = new Point(22, 360),
                Size      = new Size(645, 24),
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = ThemeConfig.SecondaryColor
            };
            this.Controls.Add(lblInfo);

            // ─── Save / Close buttons ─────────────────────────────────────
            Button btnSave = new ModernButton
            {
                Text      = ar ? "\u062D\u0641\u0638 \u0627\u0644\u0623\u0633\u0639\u0627\u0631 \u0627\u0644\u064A\u062F\u0648\u064A\u0629" : "Save Manual Rates",
                Size      = new Size(160, 38),
                Location  = new Point(20, 390),
                Cursor    = Cursors.Hand
            };
            ThemeConfig.ApplyPrimaryButton(btnSave);
            btnSave.BackColor = ThemeConfig.SuccessColor; // Override color to green for save
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            Button btnClose = new ModernButton
            {
                Text      = ar ? "\u0625\u063A\u0644\u0627\u0642" : "Close",
                Size      = new Size(90, 38),
                Location  = new Point(575, 390),
                Cursor    = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            ThemeConfig.ApplySecondaryButton(btnClose);
            this.Controls.Add(btnClose);
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
                lblStatus.Text = GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? $"\u0622\u062E\u0631 \u062A\u062D\u062F\u064A\u062B: {lastUpdate}" : $"Last rate update: {lastUpdate}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? $"\u0644\u0645 \u064A\u062A\u0645 \u0627\u0644\u062C\u0644\u0628: {ex.Message}" : "Could not load rates: " + ex.Message;
            }
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            bool ar = GenericInventorySystem.Helpers.LocalizationManager.IsArabic;
            btnRefresh.Enabled = false;
            btnRefresh.Text    = ar ? "\u062C\u0627\u0631\u064A \u0627\u0644\u062C\u0644\u0628..." : "Fetching...";
            lblStatus.ForeColor = ThemeConfig.SecondaryColor;
            lblStatus.Text     = ar ? "\u062C\u0627\u0631\u064A \u0627\u0644\u0627\u062A\u0635\u0627\u0644 \u0628\u062E\u062F\u0645\u0629 \u0627\u0644\u0623\u0633\u0639\u0627\u0631..." : "Connecting to exchange rate service...";

            var rates = await CurrencyService.FetchLiveRatesAsync();

            if (rates != null && rates.Count > 1)
            {
                CurrencyService.SaveRatesToDb(rates);
                LoadRates();
                lblStatus.ForeColor = ThemeConfig.SuccessColor;
                lblStatus.Text      = ar ? $"\u062A\u0645 \u0627\u0644\u062A\u062D\u062F\u064A\u062B: {DateTime.Now:g}" : $"Rates updated at {DateTime.Now:g}";
            }
            else
            {
                lblStatus.ForeColor = ThemeConfig.DangerColor;
                lblStatus.Text      = ar ? "\u062A\u0639\u0630\u0631 \u0627\u0644\u0627\u062A\u0635\u0627\u0644. \u064A\u062A\u0645 \u0627\u0633\u062A\u062E\u062F\u0627\u0645 \u0627\u0644\u0623\u0633\u0639\u0627\u0631 \u0627\u0644\u0645\u062E\u0632\u0646\u0629." : "Could not reach server. Using cached rates.";
            }

            btnRefresh.Enabled = true;
            btnRefresh.Text    = ar ? "\u062A\u062D\u062F\u064A\u062B \u0627\u0644\u0623\u0633\u0639\u0627\u0631" : "Refresh Live Rates";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Commit any in-progress edit
            dgvRates.EndEdit();
            if (dgvRates.BindingContext != null && dgvRates.DataSource != null)
                dgvRates.BindingContext[dgvRates.DataSource].EndCurrentEdit();

            var updated = new Dictionary<string, decimal>();
            foreach (DataGridViewRow row in dgvRates.Rows)
            {
                if (row.IsNewRow) continue;
                string code = row.Cells["code"].Value?.ToString();
                if (string.IsNullOrEmpty(code)) continue;
                if (decimal.TryParse(row.Cells["rate_vs_usd"].Value?.ToString(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.CurrentCulture, out decimal rate))
                {
                    updated[code] = rate;
                }
            }

            if (updated.Count > 0)
            {
                CurrencyService.SaveRatesToDb(updated);
                lblStatus.ForeColor = ThemeConfig.SuccessColor;
                lblStatus.Text      = GenericInventorySystem.Helpers.LocalizationManager.IsArabic ? $"\u062A\u0645 \u062D\u0641\u0638 \u0627\u0644\u0623\u0633\u0639\u0627\u0631 \u0627\u0644\u064A\u062F\u0648\u064A\u0629: {DateTime.Now:g}" : $"Manual rates saved at {DateTime.Now:g}";
            }
        }
    }
}
