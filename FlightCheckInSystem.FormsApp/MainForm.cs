using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlightCheckInSystem.FormsApp.Services;
using System.Diagnostics;

namespace FlightCheckInSystem.FormsApp
{
    public partial class MainForm : Form
    {
        private readonly ApiService _apiService;
        private readonly SeatStatusSignalRService _signalRService;

        // Navigation buttons
        private Button _bookingButton;
        private Button _checkInButton;
        private Button _flightManagementButton;

        // Content panel for forms
        private Panel _contentPanel;
        private Form _currentChildForm;

        public MainForm(ApiService apiService, SeatStatusSignalRService signalRService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _signalRService = signalRService ?? throw new ArgumentNullException(nameof(signalRService));

            InitializeComponent();
        }

      

        private void BookingButton_Click(object sender, EventArgs e)
        {
            LoadBookingForm();
            UpdateButtonStates(_bookingButton);
        }

        private void CheckInButton_Click(object sender, EventArgs e)
        {
            LoadCheckInForm();
            UpdateButtonStates(_checkInButton);
        }

        private void FlightManagementButton_Click(object sender, EventArgs e)
        {
            LoadFlightManagementForm();
            UpdateButtonStates(_flightManagementButton);
        }

        private void LoadBookingForm()
        {
            try
            {
                CloseCurrentForm();

                // BookingForm only needs ApiService - no seat selection here
                var bookingForm = new BookingForm(_apiService)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };

                _contentPanel.Controls.Add(bookingForm);
                bookingForm.Show();
                _currentChildForm = bookingForm;

                Debug.WriteLine("[MainForm] Booking form loaded successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading booking form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"[MainForm] Error loading booking form: {ex.Message}");
            }
        }

        private void LoadCheckInForm()
        {
            try
            {
                CloseCurrentForm();

                // CheckInForm needs both services for seat selection with SignalR
                var checkInForm = new CheckInForm(_apiService, _signalRService)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };

                _contentPanel.Controls.Add(checkInForm);
                checkInForm.Show();
                _currentChildForm = checkInForm;

                Debug.WriteLine("[MainForm] Check-in form loaded successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading check-in form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"[MainForm] Error loading check-in form: {ex.Message}");
            }
        }

        private void LoadFlightManagementForm()
        {
            try
            {
                CloseCurrentForm();

                // FlightManagementForm only needs ApiService
                var flightManagementForm = new FlightManagementForm(_apiService)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };

                _contentPanel.Controls.Add(flightManagementForm);
                flightManagementForm.Show();
                _currentChildForm = flightManagementForm;

                Debug.WriteLine("[MainForm] Flight management form loaded successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading flight management form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"[MainForm] Error loading flight management form: {ex.Message}");
            }
        }

        private void CloseCurrentForm()
        {
            if (_currentChildForm != null)
            {
                _contentPanel.Controls.Remove(_currentChildForm);
                _currentChildForm.Close();
                _currentChildForm.Dispose();
                _currentChildForm = null;
            }
        }

        private void UpdateButtonStates(Button activeButton)
        {
            // Reset all buttons
            _bookingButton.BackColor = Color.FromArgb(39, 174, 96);
            _checkInButton.BackColor = Color.FromArgb(52, 152, 219);
            _flightManagementButton.BackColor = Color.FromArgb(155, 89, 182);

            // Highlight active button
            activeButton.BackColor = Color.FromArgb(230, 126, 34);
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                CloseCurrentForm();

                // Stop SignalR connection gracefully
                if (_signalRService != null)
                {
                    await _signalRService.StopAsync();
                    _signalRService.Dispose();
                }

                Debug.WriteLine("[MainForm] Application shutdown completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainForm] Error during cleanup: {ex.Message}");
            }

            base.OnFormClosing(e);
        }
    }
}