namespace FlightCheckInSystem.FormsApp
{
    partial class SeatSelectionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            // Flight info label - will be set later in constructor
            _flightInfoLabel = new Label
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(20, 20),
                Size = new Size(750, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(_flightInfoLabel);

            // Legend
            _legendLabel = new Label
            {
                Text = "🟢 Available  🔴 Booked  🟡 Selected",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 60),
                Size = new Size(300, 25)
            };
            this.Controls.Add(_legendLabel);

            // Seat map panel
            _seatMapPanel = new Panel
            {
                Location = new Point(20, 100),
                Size = new Size(740, 400),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke,
                AutoScroll = true
            };
            this.Controls.Add(_seatMapPanel);

            // Buttons
            _confirmButton = new Button
            {
                Text = "Confirm Selection",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(150, 40),
                Location = new Point(500, 520),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _confirmButton.Click += ConfirmButton_Click;
            this.Controls.Add(_confirmButton);

            _cancelButton = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(100, 40),
                Location = new Point(670, 520),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(_cancelButton);
        }

        #endregion
    }
}