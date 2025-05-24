namespace FlightCheckInSystem.FormsApp
{
    partial class frmMainApplication
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
            panel1 = new Panel();
            panel2 = new Panel();
            bookingNavButton = new Button();
            checkingNavButton = new Button();
            managementNavButton = new Button();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(managementNavButton);
            panel1.Controls.Add(checkingNavButton);
            panel1.Controls.Add(bookingNavButton);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1371, 40);
            panel1.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 40);
            panel2.Name = "panel2";
            panel2.Size = new Size(1371, 897);
            panel2.TabIndex = 1;
            // 
            // bookingNavButton
            // 
            bookingNavButton.Location = new Point(54, 6);
            bookingNavButton.Name = "bookingNavButton";
            bookingNavButton.Size = new Size(94, 29);
            bookingNavButton.TabIndex = 0;
            bookingNavButton.Text = "booking";
            bookingNavButton.UseVisualStyleBackColor = true;
            // 
            // checkingNavButton
            // 
            checkingNavButton.Location = new Point(176, 5);
            checkingNavButton.Name = "checkingNavButton";
            checkingNavButton.Size = new Size(94, 29);
            checkingNavButton.TabIndex = 1;
            checkingNavButton.Text = "checking";
            checkingNavButton.UseVisualStyleBackColor = true;
            // 
            // managementNavButton
            // 
            managementNavButton.Location = new Point(293, 6);
            managementNavButton.Name = "managementNavButton";
            managementNavButton.Size = new Size(165, 29);
            managementNavButton.TabIndex = 2;
            managementNavButton.Text = "flight management";
            managementNavButton.UseVisualStyleBackColor = true;
            // 
            // frmMainApplication
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1371, 937);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "frmMainApplication";
            Text = "frmMainApplication";
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panel2;
        private Button managementNavButton;
        private Button checkingNavButton;
        private Button bookingNavButton;
    }
}