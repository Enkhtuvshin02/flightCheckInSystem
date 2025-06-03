using System;
using System.Drawing;
using System.Windows.Forms;
using FlightCheckInSystem.FormsApp.Services;

namespace FlightCheckInSystem.FormsApp
{
    public partial class MainForm : Form
    {
        private Form _currentForm;
        private Button _activeButton;
        private Color _defaultButtonColor;
        private Color _activeButtonColor = Color.FromArgb(0, 90, 158);
        private readonly ApiService _apiService;

        public MainForm(ApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            InitializeComponent();

            btnBooking.Text = "Booking";
            btnCheckIn.Text = "Check-In";
            btnFlightManagement.Text = "Flight Management";

            _defaultButtonColor = btnBooking.BackColor;

            btnBooking.Click += BtnBooking_Click;
            btnCheckIn.Click += BtnCheckIn_Click;
            btnFlightManagement.Click += BtnFlightManagement_Click;
            
            BtnBooking_Click(btnBooking, EventArgs.Empty);
        }

        private void BtnBooking_Click(object sender, EventArgs e)
        {            
            ActivateButton(sender as Button);
            ShowForm(new BookingForm(_apiService));
            lblStatus.Text = "Booking management mode active";
        }

        private void BtnCheckIn_Click(object sender, EventArgs e)
        {            
            ActivateButton(sender as Button);
            ShowForm(new CheckInForm(_apiService));
            lblStatus.Text = "Check-in management mode active";
        }

        private void BtnFlightManagement_Click(object sender, EventArgs e)
        {            
            ActivateButton(sender as Button);
            ShowForm(new FlightManagementForm(_apiService));
            lblStatus.Text = "Flight management mode active";
        }
        
        private void ActivateButton(Button button)
        {
            if (_activeButton != null)
            {
                _activeButton.BackColor = _defaultButtonColor;
            }
            
            _activeButton = button;
            _activeButton.BackColor = _activeButtonColor;
        }

        private void ShowForm(Form form)
        {            
            if (_currentForm != null)
            {
                _currentForm.Close();
                _currentForm.Dispose();
            }

            _currentForm = form;
            _currentForm.TopLevel = false;
            _currentForm.FormBorderStyle = FormBorderStyle.None;
            _currentForm.Dock = DockStyle.Fill;

            pnlMain.Controls.Clear();
            pnlMain.Controls.Add(_currentForm);
            _currentForm.Show();
        }
    }
}