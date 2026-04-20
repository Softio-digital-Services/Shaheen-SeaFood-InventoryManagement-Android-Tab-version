using System;
using System.Drawing;
using System.Windows.Forms;
using GenericInventorySystem.Controls;
using GenericInventorySystem.Helpers;

namespace GenericInventorySystem.Forms
{
    public partial class TransactionEntryForm : BaseModalForm
    {
        public decimal Amount { get; private set; }
        public string Notes { get; private set; }

        private ModernTextBox txtAmount;
        private ModernTextBox txtNotes;
        private Label lblError;
        private Label lblPrompt;
        private Button btnSave;
        private Button btnCancel;

        public TransactionEntryForm(string title, string prompt, string initialValue = "0.00")
        {
            InitializeComponent();
            this.TitleText = title; // BaseModalForm Title
            lblPrompt.Text = prompt;
            txtAmount.Text = initialValue;
            ApplyTheme();
            GenericInventorySystem.Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            GenericInventorySystem.Helpers.LocalizationManager.ApplyRTL(this);
            Func<string, string> L = GenericInventorySystem.Helpers.LocalizationManager.GetString;

            if (txtAmount != null) txtAmount.LabelText = L("Tran_AmountLabel");
            if (txtNotes != null) txtNotes.LabelText = L("Tran_NotesLabel");
            
            if (btnCancel != null) btnCancel.Text = L("Tran_Cancel");
            if (btnSave != null) btnSave.Text = L("Tran_Confirm");
        }

        private void InitializeComponent()
        {
            this.Size = new Size(450, 540);
            
            lblPrompt = new Label();
            lblPrompt.Location = new Point(30, 60); // Below Header
            lblPrompt.AutoSize = true;
            lblPrompt.Font = ThemeConfig.SubHeaderFont; // Smaller than main header
            lblPrompt.ForeColor = ThemeConfig.SecondaryColor;
            this.ContentPanel.Controls.Add(lblPrompt);

            // Amount
            txtAmount = new ModernTextBox();
            txtAmount.LabelText = "Transaction Amount ($)";
            txtAmount.Location = new Point(30, 100);
            txtAmount.Width = 380;
            this.ContentPanel.Controls.Add(txtAmount);

            // Notes
            txtNotes = new ModernTextBox();
            txtNotes.LabelText = "Notes (Optional)";
            txtNotes.Location = new Point(30, 180);
            txtNotes.Width = 380;
            txtNotes.Multiline = true;
            txtNotes.Height = 120; // Taller for notes
            this.ContentPanel.Controls.Add(txtNotes);

            // Error Label
            lblError = new Label() 
            { 
                Location = new Point(30, 310), 
                AutoSize = true, 
                ForeColor = ThemeConfig.DangerColor, 
                Visible = false, 
                Font = ThemeConfig.StandardFont 
            };
            this.ContentPanel.Controls.Add(lblError);

            // Buttons
            btnSave = new ModernButton() { Text = "Confirm", Location = new Point(270, 420), Size = new Size(140, 40) };
            btnSave.Click += BtnSave_Click;

            btnCancel = new ModernButton() { Text = "Cancel", Location = new Point(120, 420), Size = new Size(140, 40) };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.ContentPanel.Controls.Add(btnSave);
            this.ContentPanel.Controls.Add(btnCancel);
            
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            // Background is White (BaseModalForm)
            ThemeConfig.ApplyPrimaryButton(btnSave);
            ThemeConfig.ApplySecondaryButton(btnCancel);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
             string clean = txtAmount.Text.Replace("$", "").Replace(",", ".").Trim();
             if(decimal.TryParse(clean, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal val) && val > 0)
             {
                 this.Amount = val;
                 this.Notes = txtNotes.Text.Trim();
                 this.DialogResult = DialogResult.OK;
                 this.Close();
             }
             else
             {
                 lblError.Text = Properties.Resources.Tran_ErrorInvalid;
                 lblError.Visible = true;
             }
        }
    }
}

