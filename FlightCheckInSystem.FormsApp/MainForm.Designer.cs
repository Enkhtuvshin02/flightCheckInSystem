namespace FlightCheckInSystem.FormsApp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlNavigation = new Panel();
            btnFlightManagement = new Button();
            btnCheckIn = new Button();
            btnBooking = new Button();
            pnlMain = new Panel();
            lblTitle = new Label();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            pnlNavigation.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // pnlNavigation
            //
            pnlNavigation.BackColor = Color.FromArgb(0, 122, 204);
            pnlNavigation.Controls.Add(btnFlightManagement);
            pnlNavigation.Controls.Add(btnCheckIn);
            pnlNavigation.Controls.Add(btnBooking);
            pnlNavigation.Controls.Add(lblTitle);
            pnlNavigation.Dock = DockStyle.Top;
            pnlNavigation.Location = new Point(0, 0);
            pnlNavigation.Name = "pnlNavigation";
            pnlNavigation.Size = new Size(1024, 60);
            pnlNavigation.TabIndex = 0;
            //
            // pnlMain
            //
            pnlMain.BackColor = Color.White;
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 60);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(1024, 578);
            pnlMain.TabIndex = 1;
            //
            // btnBooking
            //
            btnBooking.BackColor = Color.FromArgb(0, 122, 204);
            btnBooking.FlatAppearance.BorderSize = 0;
            btnBooking.FlatStyle = FlatStyle.Flat;
            btnBooking.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnBooking.ForeColor = Color.White;
            btnBooking.Location = new Point(200, 10);
            btnBooking.Name = "btnBooking";
            btnBooking.Size = new Size(150, 40);
            btnBooking.TabIndex = 0;
            btnBooking.Text = "Booking";
            btnBooking.UseVisualStyleBackColor = false;
            //
            // btnCheckIn
            //
            btnCheckIn.BackColor = Color.FromArgb(0, 122, 204);
            btnCheckIn.FlatAppearance.BorderSize = 0;
            btnCheckIn.FlatStyle = FlatStyle.Flat;
            btnCheckIn.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnCheckIn.ForeColor = Color.White;
            btnCheckIn.Location = new Point(360, 10);
            btnCheckIn.Name = "btnCheckIn";
            btnCheckIn.Size = new Size(150, 40);
            btnCheckIn.TabIndex = 1;
            btnCheckIn.Text = "Check-In";
            btnCheckIn.UseVisualStyleBackColor = false;
            //
            // btnFlightManagement
            //
            btnFlightManagement.BackColor = Color.FromArgb(0, 122, 204);
            btnFlightManagement.FlatAppearance.BorderSize = 0;
            btnFlightManagement.FlatStyle = FlatStyle.Flat;
            btnFlightManagement.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnFlightManagement.ForeColor = Color.White;
            btnFlightManagement.Location = new Point(520, 10);
            btnFlightManagement.Name = "btnFlightManagement";
            btnFlightManagement.Size = new Size(180, 40);
            btnFlightManagement.TabIndex = 2;
            btnFlightManagement.Text = "Flight Management";
            btnFlightManagement.UseVisualStyleBackColor = false;
            
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(170, 28);
            lblTitle.TabIndex = 3;
            lblTitle.Text = "Flight Check-In System";
            
            // statusStrip
            //
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new Point(0, 638);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1024, 26);
            statusStrip.TabIndex = 2;
            statusStrip.Text = "statusStrip";
            
            // lblStatus
            //
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(50, 20);
            lblStatus.Text = "Ready";
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1024, 664);
            Controls.Add(pnlMain);
            Controls.Add(pnlNavigation);
            Controls.Add(statusStrip);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Flight Check-In System";
            pnlNavigation.ResumeLayout(false);
            pnlNavigation.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
