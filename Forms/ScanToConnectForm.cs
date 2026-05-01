using System;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using QRCoder;

namespace GenericInventorySystem.Forms
{
    /// <summary>
    /// Displays a QR code the tablet user can scan to open the Web POS instantly.
    /// Auto-detects the PC's current LAN IP — no manual configuration needed.
    /// </summary>
    public class ScanToConnectForm : Form
    {
        public ScanToConnectForm()
        {
            InitializeUI();
        }

        // ── Static helper — used by any form that wants the current server URL ──
        public static string GetServerUrl()
        {
            string ip = GetLocalIpAddress();
            return $"http://{ip}:5000";
        }

        public static string GetLocalIpAddress()
        {
            try
            {
                // Walk all active network interfaces; prefer Wi-Fi
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                    foreach (var addr in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == AddressFamily.InterNetwork
                            && !IPAddress.IsLoopback(addr.Address))
                        {
                            string ip = addr.Address.ToString();
                            if (!ip.StartsWith("169.254")) // Skip APIPA
                                return ip;
                        }
                    }
                }
            }
            catch { }
            return "localhost";
        }

        private void InitializeUI()
        {
            string url = GetServerUrl();

            // ── Form ────────────────────────────────────────────────────────────
            this.Text = "Scan to Connect — Web POS";
            this.Size = new Size(420, 540);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(15, 23, 42);

            // ── Title ───────────────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = "📱  Scan to Open on Tablet",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(0, 10, 0, 0)
            };

            // ── QR Code image ────────────────────────────────────────────────────
            var picBox = new PictureBox
            {
                Image = GenerateQrBitmap(url, 300),
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.White,
                Size = new Size(320, 320),
                Left = 50,
                Top = 60
            };
            // Rounded corners via paint
            picBox.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(25, 118, 210), 3);
                e.Graphics.DrawRectangle(pen, 1, 1, picBox.Width - 2, picBox.Height - 2);
            };

            // ── URL label ────────────────────────────────────────────────────────
            var lblUrl = new Label
            {
                Text = url,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(99, 179, 237),
                TextAlign = ContentAlignment.MiddleCenter,
                Left = 0,
                Top = 395,
                Width = 420,
                Height = 30,
                Cursor = Cursors.Hand
            };
            lblUrl.Click += (s, e) =>
            {
                Clipboard.SetText(url);
                lblUrl.Text = "✅ Copied!";
                var t = new System.Windows.Forms.Timer { Interval = 1500 };
                t.Tick += (_, __) => { lblUrl.Text = url; t.Stop(); };
                t.Start();
            };

            // ── Hint label ───────────────────────────────────────────────────────
            var lblHint = new Label
            {
                Text = "Open camera on tablet → point at QR code → tap the link\nOr click the URL above to copy it",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleCenter,
                Left = 10,
                Top = 432,
                Width = 400,
                Height = 45
            };

            // ── Close button ─────────────────────────────────────────────────────
            var btnClose = new Button
            {
                Text = "Close",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(25, 118, 210),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 36),
                Left = 150,
                Top = 483,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitle, picBox, lblUrl, lblHint, btnClose });
        }

        private static Bitmap GenerateQrBitmap(string url, int pixelsPerModule)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData     = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
            using var qrCode     = new QRCode(qrData);
            return qrCode.GetGraphic(pixelsPerModule / 21); // ~14px per module for 300px output
        }
    }
}
