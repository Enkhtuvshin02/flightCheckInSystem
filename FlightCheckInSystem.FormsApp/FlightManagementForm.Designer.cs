namespace FlightCheckInSystem.FormsApp
{
    partial class FlightManagementForm
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
            mainTableLayout = new TableLayoutPanel();
            leftPanel = new Panel();
            dgvFlights = new DataGridView();
            FlightIdColumn = new DataGridViewTextBoxColumn();
            FlightNumberColumn = new DataGridViewTextBoxColumn();
            DepartureAirportColumn = new DataGridViewTextBoxColumn();
            ArrivalAirportColumn = new DataGridViewTextBoxColumn();
            DepartureTimeColumn = new DataGridViewTextBoxColumn();
            ArrivalTimeColumn = new DataGridViewTextBoxColumn();
            StatusColumn = new DataGridViewTextBoxColumn();
            pnlToolbar = new Panel();
            btnRefresh = new Button();
            btnAddFlight = new Button();
            rightPanel = new Panel();
            grpFlightDetails = new GroupBox();
            tlpFlightDetails = new TableLayoutPanel();
            lblFlightNumber = new Label();
            txtFlightNumber = new TextBox();
            lblDepartureAirport = new Label();
            txtDepartureAirport = new TextBox();
            lblArrivalAirport = new Label();
            txtArrivalAirport = new TextBox();
            lblDepartureTime = new Label();
            dtpDepartureTime = new DateTimePicker();
            lblArrivalTime = new Label();
            dtpArrivalTime = new DateTimePicker();
            lblStatus = new Label();
            cmbStatus = new ComboBox();
            pnlInfo = new Panel();
            lblSeats = new Label();
            lblBookings = new Label();
            pnlSaveButton = new Panel();
            btnSave = new Button();
            statusStrip1 = new StatusStrip();
            lblStatusBar = new ToolStripStatusLabel();
            mainTableLayout.SuspendLayout();
            leftPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(dgvFlights)).BeginInit();
            pnlToolbar.SuspendLayout();
            rightPanel.SuspendLayout();
            grpFlightDetails.SuspendLayout();
            tlpFlightDetails.SuspendLayout();
            pnlInfo.SuspendLayout();
            pnlSaveButton.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();

            mainTableLayout.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainTableLayout.ColumnCount = 2;
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mainTableLayout.Controls.Add(leftPanel, 0, 0);
            mainTableLayout.Controls.Add(rightPanel, 1, 0);
            mainTableLayout.Location = new Point(20, 20);
            mainTableLayout.Name = "mainTableLayout";
            mainTableLayout.RowCount = 1;
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayout.Size = new Size(1160, 600);
            mainTableLayout.TabIndex = 0;

            leftPanel.Controls.Add(dgvFlights);
            leftPanel.Controls.Add(pnlToolbar);
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Location = new Point(3, 3);
            leftPanel.Name = "leftPanel";
            leftPanel.Size = new Size(690, 594);
            leftPanel.TabIndex = 0;

            dgvFlights.AllowUserToAddRows = false;
            dgvFlights.AllowUserToDeleteRows = false;
            dgvFlights.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFlights.BackgroundColor = Color.White;
            dgvFlights.BorderStyle = BorderStyle.None;
            dgvFlights.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvFlights.ColumnHeadersHeight = 40;
            dgvFlights.Columns.AddRange(new DataGridViewColumn[] {
                FlightIdColumn,
                FlightNumberColumn,
                DepartureAirportColumn,
                ArrivalAirportColumn,
                DepartureTimeColumn,
                ArrivalTimeColumn,
                StatusColumn});
            dgvFlights.Dock = DockStyle.Fill;
            dgvFlights.GridColor = Color.FromArgb(189, 195, 199);
            dgvFlights.Location = new Point(0, 60);
            dgvFlights.Name = "dgvFlights";
            dgvFlights.ReadOnly = true;
            dgvFlights.RowHeadersVisible = false;
            dgvFlights.RowTemplate.Height = 35;
            dgvFlights.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFlights.Size = new Size(690, 534);
            dgvFlights.TabIndex = 1;

            FlightIdColumn.DataPropertyName = "FlightId";
            FlightIdColumn.HeaderText = "ID";
            FlightIdColumn.Name = "FlightIdColumn";
            FlightIdColumn.ReadOnly = true;
            FlightIdColumn.FillWeight = 10F;

            FlightNumberColumn.DataPropertyName = "FlightNumber";
            FlightNumberColumn.HeaderText = "Нислэгийн №";
            FlightNumberColumn.Name = "FlightNumberColumn";
            FlightNumberColumn.ReadOnly = true;
            FlightNumberColumn.FillWeight = 15F;

            DepartureAirportColumn.DataPropertyName = "DepartureAirport";
            DepartureAirportColumn.HeaderText = "Хөдөлгөх";
            DepartureAirportColumn.Name = "DepartureAirportColumn";
            DepartureAirportColumn.ReadOnly = true;
            DepartureAirportColumn.FillWeight = 15F;

            ArrivalAirportColumn.DataPropertyName = "ArrivalAirport";
            ArrivalAirportColumn.HeaderText = "Ирэх";
            ArrivalAirportColumn.Name = "ArrivalAirportColumn";
            ArrivalAirportColumn.ReadOnly = true;
            ArrivalAirportColumn.FillWeight = 15F;

            DepartureTimeColumn.DataPropertyName = "DepartureTime";
            DepartureTimeColumn.HeaderText = "Хөдөлгөх цаг";
            DepartureTimeColumn.Name = "DepartureTimeColumn";
            DepartureTimeColumn.ReadOnly = true;
            DepartureTimeColumn.FillWeight = 20F;

            ArrivalTimeColumn.DataPropertyName = "ArrivalTime";
            ArrivalTimeColumn.HeaderText = "Ирэх цаг";
            ArrivalTimeColumn.Name = "ArrivalTimeColumn";
            ArrivalTimeColumn.ReadOnly = true;
            ArrivalTimeColumn.FillWeight = 20F;

            StatusColumn.DataPropertyName = "Status";
            StatusColumn.HeaderText = "Төлөв";
            StatusColumn.Name = "StatusColumn";
            StatusColumn.ReadOnly = true;
            StatusColumn.FillWeight = 15F;

            pnlToolbar.BackColor = Color.FromArgb(52, 73, 94);
            pnlToolbar.Controls.Add(btnRefresh);
            pnlToolbar.Controls.Add(btnAddFlight);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 0);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(690, 60);
            pnlToolbar.TabIndex = 0;

            btnAddFlight.BackColor = Color.FromArgb(39, 174, 96);
            btnAddFlight.FlatStyle = FlatStyle.Flat;
            btnAddFlight.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            btnAddFlight.ForeColor = Color.White;
            btnAddFlight.Location = new Point(15, 10);
            btnAddFlight.Name = "btnAddFlight";
            btnAddFlight.Size = new Size(140, 40);
            btnAddFlight.TabIndex = 0;
            btnAddFlight.Text = "Нислэг нэмэх";
            btnAddFlight.UseVisualStyleBackColor = false;
            btnAddFlight.Cursor = Cursors.Hand;

            btnRefresh.BackColor = Color.FromArgb(52, 152, 219);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Location = new Point(170, 10);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(120, 40);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "Шинэчлэх";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Cursor = Cursors.Hand;

            rightPanel.Controls.Add(grpFlightDetails);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Location = new Point(699, 3);
            rightPanel.Name = "rightPanel";
            rightPanel.Padding = new Padding(10, 0, 0, 0);
            rightPanel.Size = new Size(458, 594);
            rightPanel.TabIndex = 1;

            grpFlightDetails.Controls.Add(tlpFlightDetails);
            grpFlightDetails.Dock = DockStyle.Fill;
            grpFlightDetails.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            grpFlightDetails.ForeColor = Color.FromArgb(52, 73, 94);
            grpFlightDetails.Location = new Point(10, 0);
            grpFlightDetails.Name = "grpFlightDetails";
            grpFlightDetails.Padding = new Padding(15);
            grpFlightDetails.Size = new Size(448, 594);
            grpFlightDetails.TabIndex = 0;
            grpFlightDetails.TabStop = false;
            grpFlightDetails.Text = "Нислэгийн дэлгэрэнгүй";

            tlpFlightDetails.ColumnCount = 2;
            tlpFlightDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpFlightDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpFlightDetails.Controls.Add(lblFlightNumber, 0, 0);
            tlpFlightDetails.Controls.Add(txtFlightNumber, 1, 0);
            tlpFlightDetails.Controls.Add(lblDepartureAirport, 0, 1);
            tlpFlightDetails.Controls.Add(txtDepartureAirport, 1, 1);
            tlpFlightDetails.Controls.Add(lblArrivalAirport, 0, 2);
            tlpFlightDetails.Controls.Add(txtArrivalAirport, 1, 2);
            tlpFlightDetails.Controls.Add(lblDepartureTime, 0, 3);
            tlpFlightDetails.Controls.Add(dtpDepartureTime, 1, 3);
            tlpFlightDetails.Controls.Add(lblArrivalTime, 0, 4);
            tlpFlightDetails.Controls.Add(dtpArrivalTime, 1, 4);
            tlpFlightDetails.Controls.Add(lblStatus, 0, 5);
            tlpFlightDetails.Controls.Add(cmbStatus, 1, 5);
            tlpFlightDetails.Controls.Add(pnlInfo, 0, 6);
            tlpFlightDetails.Controls.Add(pnlSaveButton, 0, 7);
            tlpFlightDetails.Dock = DockStyle.Fill;
            tlpFlightDetails.Location = new Point(15, 42);
            tlpFlightDetails.Name = "tlpFlightDetails";
            tlpFlightDetails.RowCount = 8;
            tlpFlightDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tlpFlightDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tlpFlightDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tlpFlightDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tlpFlightDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tlpFlightDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tlpFlightDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tlpFlightDetails.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpFlightDetails.Size = new Size(418, 537);
            tlpFlightDetails.TabIndex = 0;

            lblFlightNumber.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lblFlightNumber.AutoSize = true;
            lblFlightNumber.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            lblFlightNumber.Location = new Point(3, 20);
            lblFlightNumber.Name = "lblFlightNumber";
            lblFlightNumber.Size = new Size(92, 23);
            lblFlightNumber.TabIndex = 0;
            lblFlightNumber.Text = "Нислэгийн №";

            txtFlightNumber.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtFlightNumber.Enabled = false;
            txtFlightNumber.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            txtFlightNumber.Location = new Point(170, 15);
            txtFlightNumber.Name = "txtFlightNumber";
            txtFlightNumber.Size = new Size(245, 32);
            txtFlightNumber.TabIndex = 1;

            lblDepartureAirport.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lblDepartureAirport.AutoSize = true;
            lblDepartureAirport.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            lblDepartureAirport.Location = new Point(3, 80);
            lblDepartureAirport.Name = "lblDepartureAirport";
            lblDepartureAirport.Size = new Size(116, 23);
            lblDepartureAirport.TabIndex = 2;
            lblDepartureAirport.Text = "Хөдөлгөх буудал";

            txtDepartureAirport.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtDepartureAirport.Enabled = false;
            txtDepartureAirport.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            txtDepartureAirport.Location = new Point(170, 75);
            txtDepartureAirport.Name = "txtDepartureAirport";
            txtDepartureAirport.Size = new Size(245, 32);
            txtDepartureAirport.TabIndex = 3;

            lblArrivalAirport.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lblArrivalAirport.AutoSize = true;
            lblArrivalAirport.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            lblArrivalAirport.Location = new Point(3, 140);
            lblArrivalAirport.Name = "lblArrivalAirport";
            lblArrivalAirport.Size = new Size(78, 23);
            lblArrivalAirport.TabIndex = 4;
            lblArrivalAirport.Text = "Ирэх буудал";

            txtArrivalAirport.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtArrivalAirport.Enabled = false;
            txtArrivalAirport.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            txtArrivalAirport.Location = new Point(170, 135);
            txtArrivalAirport.Name = "txtArrivalAirport";
            txtArrivalAirport.Size = new Size(245, 32);
            txtArrivalAirport.TabIndex = 5;

            lblDepartureTime.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lblDepartureTime.AutoSize = true;
            lblDepartureTime.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            lblDepartureTime.Location = new Point(3, 200);
            lblDepartureTime.Name = "lblDepartureTime";
            lblDepartureTime.Size = new Size(95, 23);
            lblDepartureTime.TabIndex = 6;
            lblDepartureTime.Text = "Хөдөлгөх цаг";

            dtpDepartureTime.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            dtpDepartureTime.CustomFormat = "yyyy-MM-dd HH:mm";
            dtpDepartureTime.Enabled = false;
            dtpDepartureTime.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            dtpDepartureTime.Format = DateTimePickerFormat.Custom;
            dtpDepartureTime.Location = new Point(170, 195);
            dtpDepartureTime.Name = "dtpDepartureTime";
            dtpDepartureTime.Size = new Size(245, 32);
            dtpDepartureTime.TabIndex = 7;

            lblArrivalTime.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lblArrivalTime.AutoSize = true;
            lblArrivalTime.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            lblArrivalTime.Location = new Point(3, 260);
            lblArrivalTime.Name = "lblArrivalTime";
            lblArrivalTime.Size = new Size(57, 23);
            lblArrivalTime.TabIndex = 8;
            lblArrivalTime.Text = "Ирэх цаг";

            dtpArrivalTime.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            dtpArrivalTime.CustomFormat = "yyyy-MM-dd HH:mm";
            dtpArrivalTime.Enabled = false;
            dtpArrivalTime.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            dtpArrivalTime.Format = DateTimePickerFormat.Custom;
            dtpArrivalTime.Location = new Point(170, 255);
            dtpArrivalTime.Name = "dtpArrivalTime";
            dtpArrivalTime.Size = new Size(245, 32);
            dtpArrivalTime.TabIndex = 9;

            lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            lblStatus.Location = new Point(3, 320);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(44, 23);
            lblStatus.TabIndex = 10;
            lblStatus.Text = "Төлөв";

            cmbStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Enabled = false;
            cmbStatus.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(170, 315);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(245, 33);
            cmbStatus.TabIndex = 11;

            pnlInfo.Dock = DockStyle.Fill;
            pnlInfo.Location = new Point(3, 363);
            pnlInfo.Name = "pnlInfo";
            pnlInfo.Size = new Size(412, 94);
            pnlInfo.TabIndex = 12;
            tlpFlightDetails.SetColumnSpan(pnlInfo, 2);

          

           

            tlpFlightDetails.SetColumnSpan(pnlSaveButton, 2);
            pnlSaveButton.Controls.Add(btnSave);
            pnlSaveButton.Dock = DockStyle.Fill;
            pnlSaveButton.Location = new Point(3, 463);
            pnlSaveButton.Name = "pnlSaveButton";
            pnlSaveButton.Size = new Size(412, 71);
            pnlSaveButton.TabIndex = 13;

            btnSave.BackColor = Color.FromArgb(39, 174, 96);
            btnSave.Enabled = false;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(10, 10);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(180, 50);
            btnSave.TabIndex = 0;
            btnSave.Text = "Өөрчлөлт хадгалах";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Cursor = Cursors.Hand;

            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatusBar });
            statusStrip1.Location = new Point(0, 640);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1200, 26);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            statusStrip1.BackColor = Color.FromArgb(189, 195, 199);

            lblStatusBar.Name = "lblStatusBar";
            lblStatusBar.Size = new Size(50, 20);
            lblStatusBar.Text = "Бэлэн";

            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1200, 666);
            Controls.Add(mainTableLayout);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FlightManagementForm";
            Text = "Нислэг удирдлага";
            mainTableLayout.ResumeLayout(false);
            leftPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(dgvFlights)).EndInit();
            pnlToolbar.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            grpFlightDetails.ResumeLayout(false);
            tlpFlightDetails.ResumeLayout(false);
            tlpFlightDetails.PerformLayout();
            pnlInfo.ResumeLayout(false);
            pnlInfo.PerformLayout();
            pnlSaveButton.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel mainTableLayout;
        private Panel leftPanel;
        private DataGridView dgvFlights;
        private DataGridViewTextBoxColumn FlightIdColumn;
        private DataGridViewTextBoxColumn FlightNumberColumn;
        private DataGridViewTextBoxColumn DepartureAirportColumn;
        private DataGridViewTextBoxColumn ArrivalAirportColumn;
        private DataGridViewTextBoxColumn DepartureTimeColumn;
        private DataGridViewTextBoxColumn ArrivalTimeColumn;
        private DataGridViewTextBoxColumn StatusColumn;
        private Panel pnlToolbar;
        private Button btnRefresh;
        private Button btnAddFlight;
        private Panel rightPanel;
        private GroupBox grpFlightDetails;
        private TableLayoutPanel tlpFlightDetails;
        private Label lblFlightNumber;
        private TextBox txtFlightNumber;
        private Label lblDepartureAirport;
        private TextBox txtDepartureAirport;
        private Label lblArrivalAirport;
        private TextBox txtArrivalAirport;
        private Label lblDepartureTime;
        private DateTimePicker dtpDepartureTime;
        private Label lblArrivalTime;
        private DateTimePicker dtpArrivalTime;
        private Label lblStatus;
        private ComboBox cmbStatus;
        private Panel pnlInfo;
        private Label lblSeats;
        private Label lblBookings;
        private Panel pnlSaveButton;
        private Button btnSave;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatusBar;
    }
}