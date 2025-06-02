namespace FlightCheckInSystem.FormsApp
{
    partial class BookingForm
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.grpPassengerInfo = new System.Windows.Forms.GroupBox();
            this.txtPhone = new System.Windows.Forms.TextBox();
            this.lblPhone = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtLastName = new System.Windows.Forms.TextBox();
            this.lblLastName = new System.Windows.Forms.Label();
            this.txtFirstName = new System.Windows.Forms.TextBox();
            this.lblFirstName = new System.Windows.Forms.Label();
            this.txtPassportNumber = new System.Windows.Forms.TextBox();
            this.lblPassportNumber = new System.Windows.Forms.Label();
            this.grpFlightInfo = new System.Windows.Forms.GroupBox();
            this.cmbFlight = new System.Windows.Forms.ComboBox();
            this.lblFlight = new System.Windows.Forms.Label();
            this.btnCreateBooking = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblBookingResult = new System.Windows.Forms.Label();
            this.grpPassengerInfo.SuspendLayout();
            this.grpFlightInfo.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(233, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Create New Booking";
            //
            // grpPassengerInfo
            //
            this.grpPassengerInfo.Controls.Add(this.txtPhone);
            this.grpPassengerInfo.Controls.Add(this.lblPhone);
            this.grpPassengerInfo.Controls.Add(this.txtEmail);
            this.grpPassengerInfo.Controls.Add(this.lblEmail);
            this.grpPassengerInfo.Controls.Add(this.txtLastName);
            this.grpPassengerInfo.Controls.Add(this.lblLastName);
            this.grpPassengerInfo.Controls.Add(this.txtFirstName);
            this.grpPassengerInfo.Controls.Add(this.lblFirstName);
            this.grpPassengerInfo.Controls.Add(this.txtPassportNumber);
            this.grpPassengerInfo.Controls.Add(this.lblPassportNumber);
            this.grpPassengerInfo.Location = new System.Drawing.Point(12, 52);
            this.grpPassengerInfo.Name = "grpPassengerInfo";
            this.grpPassengerInfo.Size = new System.Drawing.Size(776, 180);
            this.grpPassengerInfo.TabIndex = 1;
            this.grpPassengerInfo.TabStop = false;
            this.grpPassengerInfo.Text = "Passenger Information";
            //
            // txtPhone
            //
            this.txtPhone.Location = new System.Drawing.Point(513, 83);
            this.txtPhone.Name = "txtPhone";
            this.txtPhone.Size = new System.Drawing.Size(237, 23);
            this.txtPhone.TabIndex = 9;
            //
            // lblPhone
            //
            this.lblPhone.AutoSize = true;
            this.lblPhone.Location = new System.Drawing.Point(413, 86);
            this.lblPhone.Name = "lblPhone";
            this.lblPhone.Size = new System.Drawing.Size(94, 15);
            this.lblPhone.TabIndex = 8;
            this.lblPhone.Text = "Phone Number:";
            //
            // txtEmail
            //
            this.txtEmail.Location = new System.Drawing.Point(513, 38);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(237, 23);
            this.txtEmail.TabIndex = 7;
            //
            // lblEmail
            //
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(413, 41);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(39, 15);
            this.lblEmail.TabIndex = 6;
            this.lblEmail.Text = "Email:";
            //
            // txtLastName
            //
            this.txtLastName.Location = new System.Drawing.Point(150, 128);
            this.txtLastName.Name = "txtLastName";
            this.txtLastName.Size = new System.Drawing.Size(237, 23);
            this.txtLastName.TabIndex = 5;
            //
            // lblLastName
            //
            this.lblLastName.AutoSize = true;
            this.lblLastName.Location = new System.Drawing.Point(20, 131);
            this.lblLastName.Name = "lblLastName";
            this.lblLastName.Size = new System.Drawing.Size(66, 15);
            this.lblLastName.TabIndex = 4;
            this.lblLastName.Text = "Last Name:";
            //
            // txtFirstName
            //
            this.txtFirstName.Location = new System.Drawing.Point(150, 83);
            this.txtFirstName.Name = "txtFirstName";
            this.txtFirstName.Size = new System.Drawing.Size(237, 23);
            this.txtFirstName.TabIndex = 3;
            //
            // lblFirstName
            //
            this.lblFirstName.AutoSize = true;
            this.lblFirstName.Location = new System.Drawing.Point(20, 86);
            this.lblFirstName.Name = "lblFirstName";
            this.lblFirstName.Size = new System.Drawing.Size(67, 15);
            this.lblFirstName.TabIndex = 2;
            this.lblFirstName.Text = "First Name:";
            //
            // txtPassportNumber
            //
            this.txtPassportNumber.Location = new System.Drawing.Point(150, 38);
            this.txtPassportNumber.Name = "txtPassportNumber";
            this.txtPassportNumber.Size = new System.Drawing.Size(237, 23);
            this.txtPassportNumber.TabIndex = 1;
            //
            // lblPassportNumber
            //
            this.lblPassportNumber.AutoSize = true;
            this.lblPassportNumber.Location = new System.Drawing.Point(20, 41);
            this.lblPassportNumber.Name = "lblPassportNumber";
            this.lblPassportNumber.Size = new System.Drawing.Size(106, 15);
            this.lblPassportNumber.TabIndex = 0;
            this.lblPassportNumber.Text = "Passport Number:";
            //
            // grpFlightInfo
            //
            this.grpFlightInfo.Controls.Add(this.cmbFlight);
            this.grpFlightInfo.Controls.Add(this.lblFlight);
            this.grpFlightInfo.Location = new System.Drawing.Point(12, 238);
            this.grpFlightInfo.Name = "grpFlightInfo";
            this.grpFlightInfo.Size = new System.Drawing.Size(776, 85);
            this.grpFlightInfo.TabIndex = 2;
            this.grpFlightInfo.TabStop = false;
            this.grpFlightInfo.Text = "Flight Information";
            //
            // cmbFlight
            //
            this.cmbFlight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFlight.FormattingEnabled = true;
            this.cmbFlight.Location = new System.Drawing.Point(150, 38);
            this.cmbFlight.Name = "cmbFlight";
            this.cmbFlight.Size = new System.Drawing.Size(600, 23);
            this.cmbFlight.TabIndex = 1;
            //
            // lblFlight
            //
            this.lblFlight.AutoSize = true;
            this.lblFlight.Location = new System.Drawing.Point(20, 41);
            this.lblFlight.Name = "lblFlight";
            this.lblFlight.Size = new System.Drawing.Size(83, 15);
            this.lblFlight.TabIndex = 0;
            this.lblFlight.Text = "Select a Flight:";
            //
            // btnCreateBooking
            //
            this.btnCreateBooking.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnCreateBooking.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreateBooking.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCreateBooking.ForeColor = System.Drawing.Color.White;
            this.btnCreateBooking.Location = new System.Drawing.Point(12, 389);
            this.btnCreateBooking.Name = "btnCreateBooking";
            this.btnCreateBooking.Size = new System.Drawing.Size(150, 35);
            this.btnCreateBooking.TabIndex = 3;
            this.btnCreateBooking.Text = "Create Booking";
            this.btnCreateBooking.UseVisualStyleBackColor = false;
            this.btnCreateBooking.Click += new System.EventHandler(this.btnCreateBooking_Click);
            //
            // btnCancel
            //
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(168, 389);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 35);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // lblBookingResult
            //
            this.lblBookingResult.AutoSize = true;
            this.lblBookingResult.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblBookingResult.ForeColor = System.Drawing.Color.Green;
            this.lblBookingResult.Location = new System.Drawing.Point(12, 339);
            this.lblBookingResult.Name = "lblBookingResult";
            this.lblBookingResult.Size = new System.Drawing.Size(0, 17);
            this.lblBookingResult.TabIndex = 5;
            //
            // BookingForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lblBookingResult);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCreateBooking);
            this.Controls.Add(this.grpFlightInfo);
            this.Controls.Add(this.grpPassengerInfo);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "BookingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Flight Booking";
            this.Load += new System.EventHandler(this.BookingForm_Load);
            this.grpPassengerInfo.ResumeLayout(false);
            this.grpPassengerInfo.PerformLayout();
            this.grpFlightInfo.ResumeLayout(false);
            this.grpFlightInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpPassengerInfo;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.Label lblPhone;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtLastName;
        private System.Windows.Forms.Label lblLastName;
        private System.Windows.Forms.TextBox txtFirstName;
        private System.Windows.Forms.Label lblFirstName;
        private System.Windows.Forms.TextBox txtPassportNumber;
        private System.Windows.Forms.Label lblPassportNumber;
        private System.Windows.Forms.GroupBox grpFlightInfo;
        private System.Windows.Forms.ComboBox cmbFlight;
        private System.Windows.Forms.Label lblFlight;
        private System.Windows.Forms.Button btnCreateBooking;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblBookingResult;
    }
}