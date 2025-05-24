namespace FlightCheckInSystem.FormsApp
{
    partial class bookingPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblBookingPageTitle = new Label();
            lblFlightSelectionTitle = new Label();
            panel1 = new Panel();
            txtFlightDetails = new TextBox();
            lblDetails = new Label();
            btnRefresh = new Button();
            cmbFlights = new ComboBox();
            lblPassengerTitle = new Label();
            pnlPassengerInfo = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblBookingPageTitle
            // 
            lblBookingPageTitle.AutoSize = true;
            lblBookingPageTitle.Dock = DockStyle.Top;
            lblBookingPageTitle.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblBookingPageTitle.Location = new Point(0, 0);
            lblBookingPageTitle.Name = "lblBookingPageTitle";
            lblBookingPageTitle.Size = new Size(218, 31);
            lblBookingPageTitle.TabIndex = 0;
            lblBookingPageTitle.Text = "Create new booking";
            // 
            // lblFlightSelectionTitle
            // 
            lblFlightSelectionTitle.AutoSize = true;
            lblFlightSelectionTitle.Dock = DockStyle.Top;
            lblFlightSelectionTitle.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblFlightSelectionTitle.Location = new Point(0, 31);
            lblFlightSelectionTitle.Name = "lblFlightSelectionTitle";
            lblFlightSelectionTitle.Size = new Size(102, 23);
            lblFlightSelectionTitle.TabIndex = 1;
            lblFlightSelectionTitle.Text = "Select Flight";
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(txtFlightDetails);
            panel1.Controls.Add(lblDetails);
            panel1.Controls.Add(btnRefresh);
            panel1.Controls.Add(cmbFlights);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 54);
            panel1.Name = "panel1";
            panel1.Size = new Size(970, 100);
            panel1.TabIndex = 2;
            // 
            // txtFlightDetails
            // 
            txtFlightDetails.ImeMode = ImeMode.NoControl;
            txtFlightDetails.Location = new Point(415, 28);
            txtFlightDetails.Multiline = true;
            txtFlightDetails.Name = "txtFlightDetails";
            txtFlightDetails.ReadOnly = true;
            txtFlightDetails.ScrollBars = ScrollBars.Vertical;
            txtFlightDetails.Size = new Size(505, 38);
            txtFlightDetails.TabIndex = 4;
            // 
            // lblDetails
            // 
            lblDetails.AutoSize = true;
            lblDetails.Location = new Point(318, 32);
            lblDetails.Name = "lblDetails";
            lblDetails.Size = new Size(62, 20);
            lblDetails.TabIndex = 3;
            lblDetails.Text = "Details :";
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(192, 28);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(94, 29);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // cmbFlights
            // 
            cmbFlights.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFlights.FormattingEnabled = true;
            cmbFlights.Location = new Point(17, 29);
            cmbFlights.Name = "cmbFlights";
            cmbFlights.Size = new Size(151, 28);
            cmbFlights.TabIndex = 1;
            // 
            // lblPassengerTitle
            // 
            lblPassengerTitle.AutoSize = true;
            lblPassengerTitle.Dock = DockStyle.Top;
            lblPassengerTitle.Location = new Point(0, 154);
            lblPassengerTitle.Name = "lblPassengerTitle";
            lblPassengerTitle.Size = new Size(156, 20);
            lblPassengerTitle.TabIndex = 3;
            lblPassengerTitle.Text = "Passenger information";
            // 
            // pnlPassengerInfo
            // 
            pnlPassengerInfo.Dock = DockStyle.Top;
            pnlPassengerInfo.Location = new Point(0, 174);
            pnlPassengerInfo.Name = "pnlPassengerInfo";
            pnlPassengerInfo.Size = new Size(970, 125);
            pnlPassengerInfo.TabIndex = 4;


            //
            //info comonenets
            //
            // 

            lblFirstName.Size = new Size(100, 20);
            lblFirstName.Text=$`First name:`;
            // bookingPage
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlPassengerInfo);
            Controls.Add(lblPassengerTitle);
            Controls.Add(panel1);
            Controls.Add(lblFlightSelectionTitle);
            Controls.Add(lblBookingPageTitle);
            Name = "bookingPage";
            Size = new Size(970, 701);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblBookingPageTitle;
        private Label lblFlightSelectionTitle;
        private Panel panel1;
        private ComboBox cmbFlights;
        private TextBox txtFlightDetails;
        private Label lblDetails;
        private Button btnRefresh;
        private Label lblPassengerTitle;
        private Panel pnlPassengerInfo;
        private Label lblPassportNo;
        private TextBox txtPassportNo;
        private Button bntFind;
        private Label lblFirstName;
        private TextBox txtFirstName;
        private Label lblLastName;
        private TextBox txtLastName;


    }
}
