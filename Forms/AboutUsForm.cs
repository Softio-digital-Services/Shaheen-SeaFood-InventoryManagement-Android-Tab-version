using System;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Controls;

namespace InventorySystem.Forms
{
    public class AboutUsForm : BaseModalForm
    {
        public AboutUsForm()
        {
            InitializeAboutLayout();
            this.TitleText = LocalizationManager.IsArabic ? "عن البرنامج" : "About Us";
            this.Size = new Size(500, 580);
        }

        private void InitializeAboutLayout()
        {
            Panel container = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            
            // Logo
            PictureBox pbLogo = new PictureBox {
                Size = new Size(120, 120),
                Location = new Point(160, 20),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            try { 
                string logoPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "softio_logo.png");
                if (System.IO.File.Exists(logoPath)) pbLogo.Image = Image.FromFile(logoPath);
            } catch { }
            container.Controls.Add(pbLogo);

            // App Name
            Label lblName = new Label {
                Text = "InventorySystem",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ThemeConfig.PrimaryColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(440, 35),
                Location = new Point(0, 150)
            };
            container.Controls.Add(lblName);

            // Version
            Label lblVer = new Label {
                Text = "Version 1.0.2 Platinum",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = ThemeConfig.MutedTextColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(440, 25),
                Location = new Point(0, 190)
            };
            container.Controls.Add(lblVer);

            // Description
            Label lblDesc = new Label {
                Text = LocalizationManager.IsArabic 
                    ? "نظام متكامل لإدارة المخازن والمبيعات، مصمم خصيصاً لتلبية احتياجات الشركات الصغيرة والمتوسطة. يتميز بواجهة عصرية ودعم كامل للغة العربية."
                    : "A comprehensive Inventory and Sales Management System designed to meet the needs of SMBs. Features modern UI, real-time sync, and multi-language support.",
                Font = new Font("Segoe UI", 10.5f),
                ForeColor = ThemeConfig.TextColorDark,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(380, 80),
                Location = new Point(30, 230)
            };
            container.Controls.Add(lblDesc);

            // Divider
            Panel divider = new Panel {
                Size = new Size(300, 1),
                BackColor = Color.FromArgb(230, 230, 230),
                Location = new Point(70, 320)
            };
            container.Controls.Add(divider);

            // Developer Info
            Label lblDev = new Label {
                Text = LocalizationManager.IsArabic ? "تطوير بواسطة Softio Digital Transformation" : "Developed by Softio Digital Transformation",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ThemeConfig.TextColorDark,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(440, 25),
                Location = new Point(0, 340)
            };
            container.Controls.Add(lblDev);

            // Links/Contact
            Button btnContact = new ModernButton {
                Text = "  " + (LocalizationManager.IsArabic ? "تواصل معنا" : "Contact Support"),
                Size = new Size(200, 45),
                Location = new Point(150, 385),
                Cursor = Cursors.Hand,
                Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("contact_us"), Color.White),
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(10, 0, 0, 0)
            };
            ThemeConfig.ApplyPrimaryButton(btnContact);
            btnContact.Click += (s, e) => {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("mailto:softioservices@gmail.com") { UseShellExecute = true }); }
                catch { MessageHelper.ShowInfo("Contact us at: softioservices@gmail.com"); }
            };
            container.Controls.Add(btnContact);

            Label lblCopyright = new Label {
                Text = "© 2026 Softio Services. All Rights Reserved.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeConfig.MutedTextColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(440, 20),
                Location = new Point(0, 450)
            };
            container.Controls.Add(lblCopyright);

            this.ContentPanel.Controls.Add(container);
        }
    }
}
