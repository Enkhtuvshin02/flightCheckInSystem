namespace FlightCheckInSystem.FormsApp
{
    partial class CheckInForm
    {
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblTitle = new Label();
            mainTableLayout = new TableLayoutPanel();
            grpSearch = new GroupBox();
            tlpSearch = new TableLayoutPanel();
            lblPassportNumber = new Label();
            txtPassportNumber = new TextBox();
            btnSearch = new Button();
            leftPanel = new TableLayoutPanel();
            grpBookingDetails = new GroupBox();
            tlpBookingDetails = new TableLayoutPanel();
            lblBookingRef = new Label();
            lblPassengerInfo = new Label();
            lblFlightInfo = new Label();
            grpBoardingPass = new GroupBox();
            tlpBoardingPass = new TableLayoutPanel();
            lblBPName = new Label();
            lblBPFlight = new Label();
            lblBPSeat = new Label();
            lblBPGate = new Label();
            lblBPDeparture = new Label();
            lblBPArrival = new Label();
            lblBPBoardingTime = new Label();
            rightPanel = new Panel();
            grpSeatSelection = new GroupBox();
            lblSelectedSeat = new Label();
            pnlSeats = new Panel();
            pnlButtons = new Panel();
            btnCheckIn = new Button();
            btnCancel = new Button();
            btnPrintBoardingPass = new Button();
            mainTableLayout.SuspendLayout();
            grpSearch.SuspendLayout();
            tlpSearch.SuspendLayout();
            leftPanel.SuspendLayout();
            grpBookingDetails.SuspendLayout();
            tlpBookingDetails.SuspendLayout();
            grpBoardingPass.SuspendLayout();
            tlpBoardingPass.SuspendLayout();
            rightPanel.SuspendLayout();
            grpSeatSelection.SuspendLayout();
            pnlButtons.SuspendLayout();
            SuspendLayout();

            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Location = new Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(150, 41);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Нислэгт бүртгэх";

            mainTableLayout.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainTableLayout.ColumnCount = 1;
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTableLayout.Controls.Add(grpSearch, 0, 0);
            mainTableLayout.Controls.Add(leftPanel, 0, 1);
            mainTableLayout.Location = new Point(20, 80);
            mainTableLayout.Name = "mainTableLayout";
            mainTableLayout.RowCount = 2;
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayout.Size = new Size(910, 520);
            mainTableLayout.TabIndex = 1;

            grpSearch.Controls.Add(tlpSearch);
            grpSearch.Dock = DockStyle.Fill;
            grpSearch.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            grpSearch.ForeColor = Color.FromArgb(52, 73, 94);
            grpSearch.Location = new Point(3, 3);
            grpSearch.Name = "grpSearch";
            grpSearch.Padding = new Padding(15);
            grpSearch.Size = new Size(904, 114);
            grpSearch.TabIndex = 0;
            grpSearch.TabStop = false;
            grpSearch.Text = "Захиалга хайх";

            tlpSearch.ColumnCount = 3;
            tlpSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpSearch.Controls.Add(lblPassportNumber, 0, 0);
            tlpSearch.Controls.Add(txtPassportNumber, 1, 0);
            tlpSearch.Controls.Add(btnSearch, 2, 0);
            tlpSearch.Dock = DockStyle.Fill;
            tlpSearch.Location = new Point(15, 42);
            tlpSearch.Name = "tlpSearch";
            tlpSearch.RowCount = 1;
            tlpSearch.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpSearch.Size = new Size(874, 57);
            tlpSearch.TabIndex = 0;

            lblPassportNumber.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lblPassportNumber.AutoSize = true;
            lblPassportNumber.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            lblPassportNumber.Location = new Point(3, 15);
            lblPassportNumber.Name = "lblPassportNumber";
            lblPassportNumber.Size = new Size(118, 25);
            lblPassportNumber.TabIndex = 0;
            lblPassportNumber.Text = "Паспортын №:";

            txtPassportNumber.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtPassportNumber.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtPassportNumber.Location = new Point(265, 10);
            txtPassportNumber.Name = "txtPassportNumber";
            txtPassportNumber.Size = new Size(431, 34);
            txtPassportNumber.TabIndex = 1;
            txtPassportNumber.KeyPress += txtPassportNumber_KeyPress;

            btnSearch.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            btnSearch.BackColor = Color.FromArgb(52, 152, 219);
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            btnSearch.ForeColor = Color.White;
            btnSearch.Location = new Point(705, 5);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(166, 45);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "Хайх";
            btnSearch.UseVisualStyleBackColor = false;
            btnSearch.Cursor = Cursors.Hand;
            btnSearch.Click += btnSearch_Click;

            leftPanel.ColumnCount = 2;
            leftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            leftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            leftPanel.Controls.Add(grpBookingDetails, 0, 0);
            leftPanel.Controls.Add(rightPanel, 1, 0);
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Location = new Point(3, 123);
            leftPanel.Name = "leftPanel";
            leftPanel.RowCount = 1;
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            leftPanel.Size = new Size(904, 394);
            leftPanel.TabIndex = 1;

            grpBookingDetails.Controls.Add(tlpBookingDetails);
            grpBookingDetails.Dock = DockStyle.Fill;
            grpBookingDetails.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            grpBookingDetails.ForeColor = Color.FromArgb(52, 73, 94);
            grpBookingDetails.Location = new Point(3, 3);
            grpBookingDetails.Name = "grpBookingDetails";
            grpBookingDetails.Padding = new Padding(10);
            grpBookingDetails.Size = new Size(446, 388);
            grpBookingDetails.TabIndex = 0;
            grpBookingDetails.TabStop = false;
            grpBookingDetails.Text = "Захиалгын дэлгэрэнгүй";
            grpBookingDetails.Visible = false;

            tlpBookingDetails.ColumnCount = 1;
            tlpBookingDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpBookingDetails.Controls.Add(lblBookingRef, 0, 0);
            tlpBookingDetails.Controls.Add(lblPassengerInfo, 0, 1);
            tlpBookingDetails.Controls.Add(lblFlightInfo, 0, 2);
            tlpBookingDetails.Controls.Add(grpBoardingPass, 0, 3);
            tlpBookingDetails.Dock = DockStyle.Fill;
            tlpBookingDetails.Location = new Point(10, 35);
            tlpBookingDetails.Name = "tlpBookingDetails";
            tlpBookingDetails.RowCount = 4;
            tlpBookingDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tlpBookingDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tlpBookingDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tlpBookingDetails.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpBookingDetails.Size = new Size(426, 343);
            tlpBookingDetails.TabIndex = 0;

            lblBookingRef.AutoSize = true;
            lblBookingRef.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            lblBookingRef.Location = new Point(3, 15);
            lblBookingRef.Name = "lblBookingRef";
            lblBookingRef.Size = new Size(120, 23);
            lblBookingRef.TabIndex = 0;
            lblBookingRef.Text = "Захиалгын код:";

            lblPassengerInfo.AutoSize = true;
            lblPassengerInfo.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            lblPassengerInfo.Location = new Point(3, 75);
            lblPassengerInfo.Name = "lblPassengerInfo";
            lblPassengerInfo.Size = new Size(150, 23);
            lblPassengerInfo.TabIndex = 1;
            lblPassengerInfo.Text = "Зорчигчийн мэдээлэл:";

            lblFlightInfo.AutoSize = true;
            lblFlightInfo.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            lblFlightInfo.Location = new Point(3, 135);
            lblFlightInfo.Name = "lblFlightInfo";
            lblFlightInfo.Size = new Size(130, 23);
            lblFlightInfo.TabIndex = 2;
            lblFlightInfo.Text = "Нислэгийн мэдээлэл:";

            grpBoardingPass.Controls.Add(tlpBoardingPass);
            grpBoardingPass.Dock = DockStyle.Fill;
            grpBoardingPass.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            grpBoardingPass.Location = new Point(3, 183);
            grpBoardingPass.Name = "grpBoardingPass";
            grpBoardingPass.Padding = new Padding(5);
            grpBoardingPass.Size = new Size(420, 157);
            grpBoardingPass.TabIndex = 3;
            grpBoardingPass.TabStop = false;
            grpBoardingPass.Text = "Суудлын тасалбар";
            grpBoardingPass.Visible = false;

            tlpBoardingPass.ColumnCount = 1;
            tlpBoardingPass.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpBoardingPass.Controls.Add(lblBPName, 0, 0);
            tlpBoardingPass.Controls.Add(lblBPFlight, 0, 1);
            tlpBoardingPass.Controls.Add(lblBPSeat, 0, 2);
            tlpBoardingPass.Controls.Add(lblBPGate, 0, 3);
            tlpBoardingPass.Controls.Add(lblBPDeparture, 0, 4);
            tlpBoardingPass.Controls.Add(lblBPArrival, 0, 5);
            tlpBoardingPass.Controls.Add(lblBPBoardingTime, 0, 6);
            tlpBoardingPass.Dock = DockStyle.Fill;
            tlpBoardingPass.Location = new Point(5, 28);
            tlpBoardingPass.Name = "tlpBoardingPass";
            tlpBoardingPass.RowCount = 7;
            tlpBoardingPass.RowStyles.Add(new RowStyle(SizeType.Percent, 14.29F));
            tlpBoardingPass.RowStyles.Add(new RowStyle(SizeType.Percent, 14.29F));
            tlpBoardingPass.RowStyles.Add(new RowStyle(SizeType.Percent, 14.29F));
            tlpBoardingPass.RowStyles.Add(new RowStyle(SizeType.Percent, 14.29F));
            tlpBoardingPass.RowStyles.Add(new RowStyle(SizeType.Percent, 14.29F));
            tlpBoardingPass.RowStyles.Add(new RowStyle(SizeType.Percent, 14.29F));
            tlpBoardingPass.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28F));
            tlpBoardingPass.Size = new Size(410, 124);
            tlpBoardingPass.TabIndex = 0;

            lblBPName.AutoSize = true;
            lblBPName.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblBPName.Location = new Point(3, 0);
            lblBPName.Name = "lblBPName";
            lblBPName.Size = new Size(50, 17);
            lblBPName.TabIndex = 0;
            lblBPName.Text = "Нэр: ---";

            lblBPFlight.AutoSize = true;
            lblBPFlight.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblBPFlight.Location = new Point(3, 17);
            lblBPFlight.Name = "lblBPFlight";
            lblBPFlight.Size = new Size(67, 17);
            lblBPFlight.TabIndex = 1;
            lblBPFlight.Text = "Нислэг: ---";

            lblBPSeat.AutoSize = true;
            lblBPSeat.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblBPSeat.Location = new Point(3, 34);
            lblBPSeat.Name = "lblBPSeat";
            lblBPSeat.Size = new Size(66, 17);
            lblBPSeat.TabIndex = 2;
            lblBPSeat.Text = "Суудал: ---";

            lblBPGate.AutoSize = true;
            lblBPGate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblBPGate.Location = new Point(3, 51);
            lblBPGate.Name = "lblBPGate";
            lblBPGate.Size = new Size(63, 17);
            lblBPGate.TabIndex = 3;
            lblBPGate.Text = "Хаалга: ---";

            lblBPDeparture.AutoSize = true;
            lblBPDeparture.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblBPDeparture.Location = new Point(3, 68);
            lblBPDeparture.Name = "lblBPDeparture";
            lblBPDeparture.Size = new Size(73, 17);
            lblBPDeparture.TabIndex = 4;
            lblBPDeparture.Text = "Хөдөлгөх: ---";

            lblBPArrival.AutoSize = true;
            lblBPArrival.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblBPArrival.Location = new Point(3, 85);
            lblBPArrival.Name = "lblBPArrival";
            lblBPArrival.Size = new Size(62, 17);
            lblBPArrival.TabIndex = 5;
            lblBPArrival.Text = "Ирэх: ---";

            lblBPBoardingTime.AutoSize = true;
            lblBPBoardingTime.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblBPBoardingTime.Location = new Point(3, 102);
            lblBPBoardingTime.Name = "lblBPBoardingTime";
            lblBPBoardingTime.Size = new Size(70, 17);
            lblBPBoardingTime.TabIndex = 6;
            lblBPBoardingTime.Text = "Суух: ---";

            rightPanel.Controls.Add(grpSeatSelection);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Location = new Point(455, 3);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(446, 388);
            rightPanel.TabIndex = 1;

            grpSeatSelection.Controls.Add(lblSelectedSeat);
            grpSeatSelection.Controls.Add(pnlSeats);
            grpSeatSelection.Dock = DockStyle.Fill;
            grpSeatSelection.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            grpSeatSelection.ForeColor = Color.FromArgb(52, 73, 94);
            grpSeatSelection.Location = new Point(0, 0);
            grpSeatSelection.Name = "grpSeatSelection";
            grpSeatSelection.Padding = new Padding(10);
            grpSeatSelection.Size = new Size(446, 388);
            grpSeatSelection.TabIndex = 0;
            grpSeatSelection.TabStop = false;
            grpSeatSelection.Text = "Суудал сонгох";
            grpSeatSelection.Visible = false;

            lblSelectedSeat.AutoSize = true;
            lblSelectedSeat.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblSelectedSeat.ForeColor = Color.FromArgb(39, 174, 96);
            lblSelectedSeat.Location = new Point(15, 35);
            lblSelectedSeat.Name = "lblSelectedSeat";
            lblSelectedSeat.Size = new Size(170, 23);
            lblSelectedSeat.TabIndex = 0;
            lblSelectedSeat.Text = "Сонгосон суудал: (Үгүй)";

            pnlSeats.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlSeats.BorderStyle = BorderStyle.FixedSingle;
            pnlSeats.Location = new Point(15, 70);
            pnlSeats.Name = "pnlSeats";
            pnlSeats.Size = new Size(416, 300);
            pnlSeats.TabIndex = 1;
            pnlSeats.AutoScroll = true;

            pnlButtons.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlButtons.Controls.Add(btnCheckIn);
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnPrintBoardingPass);
            pnlButtons.Location = new Point(20, 620);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(910, 60);
            pnlButtons.TabIndex = 2;

            btnCheckIn.BackColor = Color.FromArgb(39, 174, 96);
            btnCheckIn.FlatStyle = FlatStyle.Flat;
            btnCheckIn.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            btnCheckIn.ForeColor = Color.White;
            btnCheckIn.Location = new Point(700, 5);
            btnCheckIn.Name = "btnCheckIn";
            btnCheckIn.Size = new Size(120, 50);
            btnCheckIn.TabIndex = 0;
            btnCheckIn.Text = "Бүртгэх";
            btnCheckIn.UseVisualStyleBackColor = false;
            btnCheckIn.Visible = false;
            btnCheckIn.Cursor = Cursors.Hand;
            btnCheckIn.Click += btnCheckIn_Click;

            btnCancel.BackColor = Color.FromArgb(231, 76, 60);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(400, 5);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(120, 50);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Цуцлах";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += btnCancel_Click;

            btnPrintBoardingPass.BackColor = Color.FromArgb(52, 152, 219);
            btnPrintBoardingPass.FlatStyle = FlatStyle.Flat;
            btnPrintBoardingPass.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            btnPrintBoardingPass.ForeColor = Color.White;
            btnPrintBoardingPass.Location = new Point(530, 5);
            btnPrintBoardingPass.Name = "btnPrintBoardingPass";
            btnPrintBoardingPass.Size = new Size(160, 50);
            btnPrintBoardingPass.TabIndex = 2;
            btnPrintBoardingPass.Text = "Тасалбар хэвлэх";
            btnPrintBoardingPass.UseVisualStyleBackColor = false;
            btnPrintBoardingPass.Cursor = Cursors.Hand;
            btnPrintBoardingPass.Click += btnPrintBoardingPass_Click;
            this.grpBookingDetails = new System.Windows.Forms.GroupBox();
            this.lblBookingRef = new System.Windows.Forms.Label();
            this.lblPassengerInfo = new System.Windows.Forms.Label();
            this.lblFlightInfo = new System.Windows.Forms.Label();
            this.grpSeatSelection = new System.Windows.Forms.GroupBox();
            this.pnlSeats = new System.Windows.Forms.Panel();
            this.lblSelectedSeat = new System.Windows.Forms.Label();
            this.grpBoardingPass = new System.Windows.Forms.GroupBox();
            this.btnCheckIn = new System.Windows.Forms.Button();
            this.btnPrintBoardingPass = new System.Windows.Forms.Button();

            // Configure grpBookingDetails
            this.grpBookingDetails.Text = "Захиалгын мэдээлэл";
            this.grpBookingDetails.Visible = false;

            // Configure grpSeatSelection
            this.grpSeatSelection.Text = "Суудал сонгох";
            this.grpSeatSelection.Visible = false;

            // Configure grpBoardingPass
            this.grpBoardingPass.Text = "Суудлын тасалбар";
            this.grpBoardingPass.Visible = false;

            // Configure lblSelectedSeat
            this.lblSelectedSeat.Text = "Selected Seat: (None)";

            // Configure buttons
            this.btnCheckIn.Text = "Бүртгүүлэх";
            this.btnCheckIn.Visible = false;
            this.btnCheckIn.Enabled = false;

            this.btnPrintBoardingPass.Text = "Тасалбар хэвлэх";
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(950, 700);
            Controls.Add(pnlButtons);
            Controls.Add(mainTableLayout);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.None;
            Name = "CheckInForm";
            Text = "Нислэгт бүртгэх";
            Load += CheckInForm_Load;
            mainTableLayout.ResumeLayout(false);
            grpSearch.ResumeLayout(false);
            tlpSearch.ResumeLayout(false);
            tlpSearch.PerformLayout();
            leftPanel.ResumeLayout(false);
            grpBookingDetails.ResumeLayout(false);
            tlpBookingDetails.ResumeLayout(false);
            tlpBookingDetails.PerformLayout();
            grpBoardingPass.ResumeLayout(false);
            tlpBoardingPass.ResumeLayout(false);
            tlpBoardingPass.PerformLayout();
            rightPanel.ResumeLayout(false);
            grpSeatSelection.ResumeLayout(false);
            grpSeatSelection.PerformLayout();
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.GroupBox grpBookingDetails;
        private System.Windows.Forms.Label lblBookingRef;
        private System.Windows.Forms.Label lblPassengerInfo;
        private System.Windows.Forms.Label lblFlightInfo;
        private System.Windows.Forms.GroupBox grpSeatSelection;
        private System.Windows.Forms.Panel pnlSeats;
        private System.Windows.Forms.Label lblSelectedSeat;
        private System.Windows.Forms.GroupBox grpBoardingPass;
        private System.Windows.Forms.Button btnCheckIn;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnPrintBoardingPass;
        private System.Windows.Forms.TextBox txtPassportNumber;
        private Label lblTitle;
        private TableLayoutPanel mainTableLayout;
        private GroupBox grpSearch;
        private TableLayoutPanel tlpSearch;
        private Label lblPassportNumber;
        private TableLayoutPanel leftPanel;
        private TableLayoutPanel tlpBookingDetails;
        private TableLayoutPanel tlpBoardingPass;
        private Label lblBPName;
        private Label lblBPFlight;
        private Label lblBPSeat;
        private Label lblBPGate;
        private Label lblBPDeparture;
        private Label lblBPArrival;
        private Label lblBPBoardingTime;
        private Panel rightPanel;
        private Panel pnlButtons;
    }
}