namespace FlightCheckInSystem.FormsApp
{
    partial class BookingForm
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
            lblTitle = new Label();
            tableLayoutPanel = new TableLayoutPanel();
            grpPassengerInfo = new GroupBox();
            tlpPassenger = new TableLayoutPanel();
            lblPassportNumber = new Label();
            txtPassportNumber = new TextBox();
            lblFirstName = new Label();
            txtFirstName = new TextBox();
            lblLastName = new Label();
            txtLastName = new TextBox();
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblPhone = new Label();
            txtPhone = new TextBox();
            grpFlightInfo = new GroupBox();
            tlpFlight = new TableLayoutPanel();
            lblFlight = new Label();
            cmbFlight = new ComboBox();
            pnlButtons = new Panel();
            btnCreateBooking = new Button();
            btnCancel = new Button();
            lblBookingResult = new Label();
            tableLayoutPanel.SuspendLayout();
            grpPassengerInfo.SuspendLayout();
            tlpPassenger.SuspendLayout();
            grpFlightInfo.SuspendLayout();
            tlpFlight.SuspendLayout();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Location = new Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(234, 41);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Шинэ захиалга";
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.ColumnCount = 1;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(grpPassengerInfo, 0, 0);
            tableLayoutPanel.Controls.Add(grpFlightInfo, 0, 1);
            tableLayoutPanel.Location = new Point(20, 80);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 2;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            tableLayoutPanel.Size = new Size(760, 400);
            tableLayoutPanel.TabIndex = 1;
            // 
            // grpPassengerInfo
            // 
            grpPassengerInfo.Controls.Add(tlpPassenger);
            grpPassengerInfo.Dock = DockStyle.Fill;
            grpPassengerInfo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            grpPassengerInfo.ForeColor = Color.FromArgb(52, 73, 94);
            grpPassengerInfo.Location = new Point(3, 3);
            grpPassengerInfo.Name = "grpPassengerInfo";
            grpPassengerInfo.Padding = new Padding(15);
            grpPassengerInfo.Size = new Size(754, 274);
            grpPassengerInfo.TabIndex = 0;
            grpPassengerInfo.TabStop = false;
            grpPassengerInfo.Text = "Зорчигчийн мэдээлэл";
            // 
            // tlpPassenger
            // 
            tlpPassenger.ColumnCount = 4;
            tlpPassenger.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpPassenger.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpPassenger.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpPassenger.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpPassenger.Controls.Add(lblPassportNumber, 0, 0);
            tlpPassenger.Controls.Add(txtPassportNumber, 1, 0);
            tlpPassenger.Controls.Add(lblFirstName, 0, 1);
            tlpPassenger.Controls.Add(txtFirstName, 1, 1);
            tlpPassenger.Controls.Add(lblEmail, 0, 2);
            tlpPassenger.Controls.Add(txtEmail, 1, 2);
            tlpPassenger.Controls.Add(txtLastName, 3, 0);
            tlpPassenger.Controls.Add(lblLastName, 2, 0);
            tlpPassenger.Controls.Add(lblPhone, 2, 1);
            tlpPassenger.Controls.Add(txtPhone, 3, 1);
            tlpPassenger.Dock = DockStyle.Fill;
            tlpPassenger.Location = new Point(15, 42);
            tlpPassenger.Name = "tlpPassenger";
            tlpPassenger.RowCount = 3;
            tlpPassenger.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tlpPassenger.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tlpPassenger.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));
            tlpPassenger.Size = new Size(724, 217);
            tlpPassenger.TabIndex = 0;
            // 
            // lblPassportNumber
            // 
            lblPassportNumber.AutoSize = true;
            lblPassportNumber.Font = new Font("Segoe UI", 10F);
            lblPassportNumber.Location = new Point(3, 0);
            lblPassportNumber.Name = "lblPassportNumber";
            lblPassportNumber.Size = new Size(126, 23);
            lblPassportNumber.TabIndex = 0;
            lblPassportNumber.Text = "Паспортын №:";
            // 
            // txtPassportNumber
            // 
            txtPassportNumber.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPassportNumber.Font = new Font("Segoe UI", 11F);
            txtPassportNumber.Location = new Point(184, 3);
            txtPassportNumber.Name = "txtPassportNumber";
            txtPassportNumber.Size = new Size(175, 32);
            txtPassportNumber.TabIndex = 1;
            // 
            // lblFirstName
            // 
            lblFirstName.AutoSize = true;
            lblFirstName.Font = new Font("Segoe UI", 10F);
            lblFirstName.Location = new Point(3, 72);
            lblFirstName.Name = "lblFirstName";
            lblFirstName.Size = new Size(95, 23);
            lblFirstName.TabIndex = 2;
            lblFirstName.Text = "Эхний нэр:";
            // 
            // txtFirstName
            // 
            txtFirstName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFirstName.Font = new Font("Segoe UI", 11F);
            txtFirstName.Location = new Point(184, 75);
            txtFirstName.Name = "txtFirstName";
            txtFirstName.Size = new Size(175, 32);
            txtFirstName.TabIndex = 3;
            // 
            // lblLastName
            // 
            lblLastName.AutoSize = true;
            lblLastName.Font = new Font("Segoe UI", 10F);
            lblLastName.Location = new Point(365, 0);
            lblLastName.Name = "lblLastName";
            lblLastName.Size = new Size(86, 23);
            lblLastName.TabIndex = 4;
            lblLastName.Text = "Овог нэр:";
            // 
            // txtLastName
            // 
            txtLastName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtLastName.Font = new Font("Segoe UI", 11F);
            txtLastName.Location = new Point(546, 3);
            txtLastName.Name = "txtLastName";
            txtLastName.Size = new Size(175, 32);
            txtLastName.TabIndex = 5;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Font = new Font("Segoe UI", 10F);
            lblEmail.Location = new Point(3, 144);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(66, 23);
            lblEmail.TabIndex = 6;
            lblEmail.Text = "Имэйл:";
            // 
            // txtEmail
            // 
            txtEmail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtEmail.Font = new Font("Segoe UI", 11F);
            txtEmail.Location = new Point(184, 147);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(175, 32);
            txtEmail.TabIndex = 7;
            // 
            // lblPhone
            // 
            lblPhone.AutoSize = true;
            lblPhone.Font = new Font("Segoe UI", 10F);
            lblPhone.Location = new Point(365, 72);
            lblPhone.Name = "lblPhone";
            lblPhone.Size = new Size(94, 23);
            lblPhone.TabIndex = 8;
            lblPhone.Text = "Утасны №:";
            // 
            // txtPhone
            // 
            txtPhone.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPhone.Font = new Font("Segoe UI", 11F);
            txtPhone.Location = new Point(546, 75);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(175, 32);
            txtPhone.TabIndex = 9;
            // 
            // grpFlightInfo
            // 
            grpFlightInfo.Controls.Add(tlpFlight);
            grpFlightInfo.Dock = DockStyle.Fill;
            grpFlightInfo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            grpFlightInfo.ForeColor = Color.FromArgb(52, 73, 94);
            grpFlightInfo.Location = new Point(3, 283);
            grpFlightInfo.Name = "grpFlightInfo";
            grpFlightInfo.Padding = new Padding(15);
            grpFlightInfo.Size = new Size(754, 114);
            grpFlightInfo.TabIndex = 1;
            grpFlightInfo.TabStop = false;
            grpFlightInfo.Text = "Нислэгийн мэдээлэл";
            // 
            // tlpFlight
            // 
            tlpFlight.ColumnCount = 2;
            tlpFlight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpFlight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));
            tlpFlight.Controls.Add(lblFlight, 0, 0);
            tlpFlight.Controls.Add(cmbFlight, 1, 0);
            tlpFlight.Dock = DockStyle.Fill;
            tlpFlight.Location = new Point(15, 42);
            tlpFlight.Name = "tlpFlight";
            tlpFlight.RowCount = 1;
            tlpFlight.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpFlight.Size = new Size(724, 57);
            tlpFlight.TabIndex = 0;
            // 
            // lblFlight
            // 
            lblFlight.AutoSize = true;
            lblFlight.Font = new Font("Segoe UI", 10F);
            lblFlight.Location = new Point(3, 0);
            lblFlight.Name = "lblFlight";
            lblFlight.Size = new Size(126, 23);
            lblFlight.TabIndex = 0;
            lblFlight.Text = "Нислэг сонгох:";
            // 
            // cmbFlight
            // 
            cmbFlight.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbFlight.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFlight.Font = new Font("Segoe UI", 11F);
            cmbFlight.FormattingEnabled = true;
            cmbFlight.Location = new Point(184, 3);
            cmbFlight.Name = "cmbFlight";
            cmbFlight.Size = new Size(537, 33);
            cmbFlight.TabIndex = 1;
            // 
            // pnlButtons
            // 
            pnlButtons.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlButtons.Controls.Add(btnCreateBooking);
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Location = new Point(20, 500);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(760, 60);
            pnlButtons.TabIndex = 2;
            // 
            // btnCreateBooking
            // 
            btnCreateBooking.BackColor = Color.FromArgb(39, 174, 96);
            btnCreateBooking.Cursor = Cursors.Hand;
            btnCreateBooking.FlatStyle = FlatStyle.Flat;
            btnCreateBooking.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCreateBooking.ForeColor = Color.White;
            btnCreateBooking.Location = new Point(0, 10);
            btnCreateBooking.Name = "btnCreateBooking";
            btnCreateBooking.Size = new Size(180, 50);
            btnCreateBooking.TabIndex = 0;
            btnCreateBooking.Text = "Захиалга үүсгэх";
            btnCreateBooking.UseVisualStyleBackColor = false;
            btnCreateBooking.Click += btnCreateBooking_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(231, 76, 60);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(200, 10);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(120, 50);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Цуцлах";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // lblBookingResult
            // 
            lblBookingResult.AutoSize = true;
            lblBookingResult.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblBookingResult.ForeColor = Color.FromArgb(39, 174, 96);
            lblBookingResult.Location = new Point(20, 570);
            lblBookingResult.Name = "lblBookingResult";
            lblBookingResult.Size = new Size(0, 25);
            lblBookingResult.TabIndex = 3;
            // 
            // BookingForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 600);
            Controls.Add(lblBookingResult);
            Controls.Add(pnlButtons);
            Controls.Add(tableLayoutPanel);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.None;
            Name = "BookingForm";
            Text = "Нислэгийн захиалга";
            Load += BookingForm_Load;
            tableLayoutPanel.ResumeLayout(false);
            grpPassengerInfo.ResumeLayout(false);
            tlpPassenger.ResumeLayout(false);
            tlpPassenger.PerformLayout();
            grpFlightInfo.ResumeLayout(false);
            tlpFlight.ResumeLayout(false);
            tlpFlight.PerformLayout();
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private TableLayoutPanel tableLayoutPanel;
        private GroupBox grpPassengerInfo;
        private TableLayoutPanel tlpPassenger;
        private Label lblPassportNumber;
        private TextBox txtPassportNumber;
        private Label lblFirstName;
        private TextBox txtFirstName;
        private Label lblLastName;
        private TextBox txtLastName;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblPhone;
        private TextBox txtPhone;
        private GroupBox grpFlightInfo;
        private TableLayoutPanel tlpFlight;
        private Label lblFlight;
        private ComboBox cmbFlight;
        private Panel pnlButtons;
        private Button btnCreateBooking;
        private Button btnCancel;
        private Label lblBookingResult;
    }
}