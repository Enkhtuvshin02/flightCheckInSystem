using System; // Only keep essentials for booking creation
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Core.Enums;
using FlightCheckInSystem.FormsApp.Services;
using System.Diagnostics;

namespace FlightCheckInSystem.FormsApp
{
    public partial class BookingForm : Form
    {
        // Server connectivity services
        private readonly ApiService _apiService;
        
        // Data for flights
        private List<Flight> _flights;
        
        // Data for passengers
        private List<Passenger> _passengers;
        
        // Data for bookings
        private List<Booking> _bookings;
        
        // Dictionary to map dropdown items to Flight objects
        private Dictionary<int, Flight> _flightDictionary = new Dictionary<int, Flight>();

        public BookingForm()
        {
            InitializeComponent();
            
            // Initialize API service
            _apiService = new ApiService();
            
            // Load data and populate UI
            LoadDataAsync();
        }
        
        private async void LoadDataAsync()
        {
            try
            {
                // Clear existing data
                _flights = new List<Flight>();
                _flightDictionary.Clear();
                
                // Load flights from server
                var flights = await _apiService.GetFlightsAsync();
                if (flights != null && flights.Count > 0)
                {
                    _flights = flights;
                    PopulateFlightDropdown();
                }
                else
                {
                    MessageBox.Show("No flights were found on the server. Please check server connection.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data from server: {ex.Message}\n\nPlease check that the server is running and try again.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateFlightDropdown()
        {
            cmbFlight.Items.Clear();
            _flightDictionary.Clear();
            
            for (int i = 0; i < _flights.Count; i++)
            {
                var flight = _flights[i];
                cmbFlight.Items.Add($"{flight.FlightNumber} - {flight.DepartureAirport} to {flight.ArrivalAirport} - {flight.DepartureTime:yyyy-MM-dd HH:mm}");
                _flightDictionary[i] = flight;
            }

            if (cmbFlight.Items.Count > 0)
            {
                cmbFlight.SelectedIndex = 0;
            }
        }

        private async void btnCreateBooking_Click(object sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtPassportNumber.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                cmbFlight.SelectedIndex < 0)
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // Get selected flight from dictionary using the selected index
            if (!_flightDictionary.TryGetValue(cmbFlight.SelectedIndex, out Flight selectedFlight))
            {
                MessageBox.Show("Could not retrieve selected flight. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            try
            {
                // Create booking via API
                var booking = await _apiService.CreateBookingAsync(
                    selectedFlight.FlightNumber,
                    txtPassportNumber.Text,
                    txtFirstName.Text,
                    txtLastName.Text,
                    txtEmail.Text,
                    txtPhone.Text);
                    
                if (booking != null)
                {
                    // Show success message with booking details
                    string message = $"Booking created successfully!\n\n";
                    message += $"Booking ID: {booking.BookingId}\n";
                    message += $"Passenger: {txtFirstName.Text} {txtLastName.Text}\n";
                    message += $"Passport: {txtPassportNumber.Text}\n";
                    message += $"Flight: {selectedFlight.FlightNumber}\n";
                    message += $"Route: {selectedFlight.DepartureAirport} to {selectedFlight.ArrivalAirport}\n";
                    message += $"Departure: {selectedFlight.DepartureTime:yyyy-MM-dd HH:mm}";
                    
                    MessageBox.Show(message, "Booking Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Reset form
                    ClearFormFields();
                }
                else
                {
                    MessageBox.Show("Failed to create booking. Please try again.", "Booking Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating booking: {ex.Message}\n\nPlease check that the server is running and try again.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BookingForm_Load(object sender, EventArgs e)
        {
            // Initialize data when form loads
            LoadDataAsync();
        }

        private void ClearFormFields()
        {
            txtPassportNumber.Clear();
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            
            if (cmbFlight.Items.Count > 0)
            {
                cmbFlight.SelectedIndex = 0;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
