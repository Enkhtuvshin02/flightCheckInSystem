using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.FormsApp.Services;
using System.Diagnostics;

namespace FlightCheckInSystem.FormsApp
{
    partial class BoardingPassDialog
    {
                private Panel pnlMain;
        private Label lblTitle;
        private TextBox txtBoardingPassInfo;
        private Button btnPrint;
        private Button btnPreview;
        private Button btnClose;
        private Button btnNewCheckIn;
        private Panel pnlButtons;
        private GroupBox grpBoardingPass;

        private void InitializeComponent()
        {
            this.SuspendLayout();

                        this.Text = "Boarding Pass";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = Color.White;

                        pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

                        lblTitle = new Label
            {
                Text = "✈ BOARDING PASS GENERATED ✈",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };

                        grpBoardingPass = new GroupBox
            {
                Text = "Boarding Pass Details",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 70),
                Size = new Size(540, 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

                        txtBoardingPassInfo = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10, FontStyle.Regular),
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.Black,
                Location = new Point(15, 25),
                Size = new Size(510, 260),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

                        pnlButtons = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
                Padding = new Padding(20, 10, 20, 10)
            };

                        btnPrint = new Button
            {
                Text = "🖨 Print Boarding Pass",
                Size = new Size(140, 40),
                Location = new Point(20, 10),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            btnPrint.FlatAppearance.BorderColor = Color.DarkGreen;
            btnPrint.Click += BtnPrint_Click;

                        btnPreview = new Button
            {
                Text = "👁 Print Preview",
                Size = new Size(120, 40),
                Location = new Point(170, 10),
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            btnPreview.FlatAppearance.BorderColor = Color.DarkBlue;
            btnPreview.Click += BtnPreview_Click;

                        btnNewCheckIn = new Button
            {
                Text = "➕ New Check-In",
                Size = new Size(120, 40),
                Location = new Point(300, 10),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            btnNewCheckIn.FlatAppearance.BorderColor = Color.Orange;
            btnNewCheckIn.Click += BtnNewCheckIn_Click;

                        btnClose = new Button
            {
                Text = "❌ Close",
                Size = new Size(80, 40),
                Location = new Point(430, 10),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            btnClose.FlatAppearance.BorderColor = Color.Gray;
            btnClose.Click += BtnClose_Click;

                        grpBoardingPass.Controls.Add(txtBoardingPassInfo);

                        pnlButtons.Controls.Add(btnPrint);
            pnlButtons.Controls.Add(btnPreview);
            pnlButtons.Controls.Add(btnNewCheckIn);
            pnlButtons.Controls.Add(btnClose);

                        pnlMain.Controls.Add(grpBoardingPass);
            pnlMain.Controls.Add(lblTitle);

                        this.Controls.Add(pnlMain);
            this.Controls.Add(pnlButtons);

            this.ResumeLayout(false);
        }

        private void LoadBoardingPassData()
        {
            if (_boardingPass == null)
            {
                txtBoardingPassInfo.Text = "Error: No boarding pass data available.";
                btnPrint.Enabled = false;
                btnPreview.Enabled = false;
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("                    ✈ BOARDING PASS ✈");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine();

            sb.AppendLine("PASSENGER INFORMATION:");
            sb.AppendLine($"  Name:         {_boardingPass.PassengerName.ToUpper()}");
            sb.AppendLine($"  Passport:     {_boardingPass.PassportNumber}");
            sb.AppendLine();

            sb.AppendLine("FLIGHT INFORMATION:");
            sb.AppendLine($"  Flight:       {_boardingPass.FlightNumber}");
            sb.AppendLine($"  From:         {_boardingPass.DepartureAirport}");
            sb.AppendLine($"  To:           {_boardingPass.ArrivalAirport}");
            sb.AppendLine($"  Seat:         {_boardingPass.SeatNumber}");
            sb.AppendLine();

            sb.AppendLine("TIMING INFORMATION:");
            sb.AppendLine($"  Departure:    {_boardingPass.DepartureTime:dddd, MMMM dd, yyyy}");
            sb.AppendLine($"                {_boardingPass.DepartureTime:HH:mm}");
            sb.AppendLine($"  Boarding:     {_boardingPass.BoardingTime:dddd, MMMM dd, yyyy}");
            sb.AppendLine($"                {_boardingPass.BoardingTime:HH:mm}");
            sb.AppendLine();

            sb.AppendLine("IMPORTANT NOTICES:");
            sb.AppendLine("  • Please arrive at the gate 30 minutes before boarding");
            sb.AppendLine("  • Valid photo ID and boarding pass required for boarding");
            sb.AppendLine("  • Check airport displays for any gate changes");
            sb.AppendLine("  • Follow airline baggage policies and restrictions");
            sb.AppendLine();

            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"Generated: {DateTime.Now:MMM dd, yyyy HH:mm}");
            sb.AppendLine("               Thank you for choosing our airline!");
            sb.AppendLine("                    Have a pleasant flight!");
            sb.AppendLine("═══════════════════════════════════════════════════════════");

            txtBoardingPassInfo.Text = sb.ToString();

            Debug.WriteLine($"[BoardingPassDialog] Loaded boarding pass data for {_boardingPass.PassengerName}");
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[BoardingPassDialog] Print button clicked");
            try
            {
                _printer.PrintBoardingPass(_boardingPass);
                MessageBox.Show("Boarding pass sent to printer successfully!",
                    "Print Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BoardingPassDialog] Error printing: {ex.Message}");
                MessageBox.Show($"Error printing boarding pass: {ex.Message}",
                    "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPreview_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[BoardingPassDialog] Preview button clicked");
            try
            {
                _printer.ShowPrintPreview(_boardingPass);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BoardingPassDialog] Error showing preview: {ex.Message}");
                MessageBox.Show($"Error showing print preview: {ex.Message}",
                    "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNewCheckIn_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[BoardingPassDialog] New Check-In button clicked");
            this.DialogResult = DialogResult.Retry;             this.Close();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[BoardingPassDialog] Close button clicked");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _printer?.Dispose();
            Debug.WriteLine("[BoardingPassDialog] Form closed and resources disposed");
        }

                public bool StartNewCheckIn => this.DialogResult == DialogResult.Retry;
    }
}