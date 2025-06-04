namespace FlightCheckInSystem.FormsApp
{
    partial class BookingSelectionForm
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
            this.SuspendLayout();

            // Form properties
            this.Size = new Size(900, 600);
            this.Text = "Нислэгийн захиалга сонгох";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(236, 240, 241);

            // Main layout
            _mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(236, 240, 241)
            };
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));

            // Instruction label
            _lblInstruction = new Label
            {
                Text = "Та олон нислэгийн захиалгатай байна. Сонгож, үйлдэл хийнэ үү:",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            // Data grid view
            _bookingsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = false,
                AllowUserToResizeRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                RowTemplate = { Height = 35 }
            };

            // Set up grid columns
            SetupGridColumns();

            // Button panel
            _buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 4,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(236, 240, 241)
            };
            _buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Spacer
            _buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            _buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            _buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

            // Buttons
            _btnSelectForCheckIn = new Button
            {
                Text = "Суудал сонгох",
                Size = new Size(150, 50),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand
            };
            _btnSelectForCheckIn.FlatAppearance.BorderSize = 0;
            _btnSelectForCheckIn.Click += BtnSelectForCheckIn_Click;

            _btnViewBoardingPass = new Button
            {
                Text = "Суудлын тасалбар харах",
                Size = new Size(190, 50),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand
            };
            _btnViewBoardingPass.FlatAppearance.BorderSize = 0;
            _btnViewBoardingPass.Click += BtnViewBoardingPass_Click;

            _btnCancel = new Button
            {
                Text = "Цуцлах",
                Size = new Size(110, 50),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += BtnCancel_Click;

            // Add controls to button panel
            _buttonPanel.Controls.Add(new Panel(), 0, 0); // Spacer
            _buttonPanel.Controls.Add(_btnSelectForCheckIn, 1, 0);
            _buttonPanel.Controls.Add(_btnViewBoardingPass, 2, 0);
            _buttonPanel.Controls.Add(_btnCancel, 3, 0);

            // Add controls to main layout
            _mainLayout.Controls.Add(_lblInstruction, 0, 0);
            _mainLayout.Controls.Add(_bookingsGrid, 0, 1);
            _mainLayout.Controls.Add(_buttonPanel, 0, 2);

            this.Controls.Add(_mainLayout);

            // Event handlers
            _bookingsGrid.SelectionChanged += BookingsGrid_SelectionChanged;
            _bookingsGrid.CellFormatting += BookingsGrid_CellFormatting;

            this.ResumeLayout(false);
        }
        #endregion
    }
}