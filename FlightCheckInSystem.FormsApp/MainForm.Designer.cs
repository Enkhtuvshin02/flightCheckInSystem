namespace FlightCheckInSystem.FormsApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.Size = new Size(1200, 800);
            this.Text = "Нислэгийн бүртгэлийн систем";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(52, 73, 94);

            // Create navigation panel
            var navPanel = new Panel
            {
                Height = 80,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(44, 62, 80)
            };

            // Title label
            var titleLabel = new Label
            {
                Text = "Нислэгийн бүртгэлийн систем",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                Size = new Size(400, 40)
            };
            navPanel.Controls.Add(titleLabel);

            // Navigation buttons
            _bookingButton = new Button
            {
                Text = "Захиалга",
                Size = new Size(120, 50),
                Location = new Point(450, 15),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _bookingButton.Click += BookingButton_Click;
            navPanel.Controls.Add(_bookingButton);

            _checkInButton = new Button
            {
                Text = "Бүртгэл",
                Size = new Size(120, 50),
                Location = new Point(580, 15),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _checkInButton.Click += CheckInButton_Click;
            navPanel.Controls.Add(_checkInButton);

            _flightManagementButton = new Button
            {
                Text = "Нислэг удирдлага",
                Size = new Size(140, 50),
                Location = new Point(710, 15),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _flightManagementButton.Click += FlightManagementButton_Click;
            navPanel.Controls.Add(_flightManagementButton);

            // Content panel
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            this.Controls.Add(_contentPanel);
            this.Controls.Add(navPanel);

            // Load booking form by default
            LoadBookingForm();
        }
        #endregion

        private Panel pnlNavigation;
        private Button btnFlightManagement;
        private Button btnCheckIn;
        private Button btnBooking;
        private Panel pnlMain;
        private Label lblTitle;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
    }
}