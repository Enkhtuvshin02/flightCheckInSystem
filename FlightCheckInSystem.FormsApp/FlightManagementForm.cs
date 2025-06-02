using System; // Only keep essentials for flight management
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlightCheckInSystem.Core.Enums;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.FormsApp.Services;

namespace FlightCheckInSystem.FormsApp
{
    public partial class FlightManagementForm : Form
    {
        private readonly ApiService _apiService;
        private readonly SeatStatusSignalRService _seatStatusSignalRService;
        private List<Flight> _flights;
        private Flight _selectedFlight;

        public FlightManagementForm()
        {
            InitializeComponent();
            // TODO: Replace with your actual SignalR hub URL if different
            _seatStatusSignalRService = new SeatStatusSignalRService("https://localhost:5001/seathub");
            // Initialize API service
            _apiService = new ApiService();

            // Set up the status combo box with enum values
            cmbStatus.DataSource = Enum.GetValues(typeof(FlightStatus));
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;

            // Set up the flights data grid view
            dgvFlights.AutoGenerateColumns = false;
            dgvFlights.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFlights.MultiSelect = false;
            dgvFlights.ReadOnly = true;

            // Load flights when form loads
            this.Load += async (s, e) => await LoadFlightsAsync();

            // Wire up event handlers
            dgvFlights.SelectionChanged += DgvFlights_SelectionChanged;
            btnSave.Click += async (s, e) => await SaveFlightChangesAsync();
            btnRefresh.Click += async (s, e) => await LoadFlightsAsync();
            btnAddFlight.Click += BtnAddFlight_Click;
        }

        private async Task LoadFlightsAsync()
        {
            try
            {
                // Show loading indicator
                lblStatus.Text = "Loading flights...";
                this.Cursor = Cursors.WaitCursor;

                // Get all flights using API service
                _flights = await _apiService.GetFlightsAsync();
                dgvFlights.DataSource = null;
                dgvFlights.DataSource = _flights;

                // Clear selection
                ClearFlightDetails();

                lblStatus.Text = $"{_flights.Count} flights loaded successfully.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading flights: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error loading flights.";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void DgvFlights_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvFlights.SelectedRows.Count > 0 && dgvFlights.SelectedRows[0].DataBoundItem is Flight selectedFlight)
            {
                try
                {
                    lblStatus.Text = "Loading flight details...";
                    this.Cursor = Cursors.WaitCursor;

                    // Load full flight details using API service
                    _selectedFlight = await _apiService.GetFlightByIdAsync(selectedFlight.FlightId);
                    
                    if (_selectedFlight == null)
                    {
                        MessageBox.Show("Could not load flight details. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Display flight details
                    txtFlightNumber.Text = _selectedFlight.FlightNumber;
                    txtDepartureAirport.Text = _selectedFlight.DepartureAirport;
                    txtArrivalAirport.Text = _selectedFlight.ArrivalAirport;
                    dtpDepartureTime.Value = _selectedFlight.DepartureTime;
                    dtpArrivalTime.Value = _selectedFlight.ArrivalTime;
                    cmbStatus.SelectedItem = _selectedFlight.Status;

                    // Get seats for the flight (seat data will be received via FlightSeatsReceived event)
                    await _seatStatusSignalRService.GetFlightSeatsAsync(_selectedFlight.FlightId);
                    // NOTE: Update the UI in the FlightSeatsReceived event handler.

                    // Enable edit controls
                    EnableFlightDetailsControls(true);

                    lblStatus.Text = "Flight details loaded.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading flight details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Error loading flight details.";
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
            else
            {
                ClearFlightDetails();
            }
        }

        private async Task SaveFlightChangesAsync()
        {
            try
            {
                lblStatus.Text = "Saving changes...";
                this.Cursor = Cursors.WaitCursor;
                
                if (_selectedFlight == null)
                {
                    // Creating a new flight
                    var newFlight = new Flight
                    {
                        FlightNumber = txtFlightNumber.Text,
                        DepartureAirport = txtDepartureAirport.Text,
                        ArrivalAirport = txtArrivalAirport.Text,
                        DepartureTime = dtpDepartureTime.Value,
                        ArrivalTime = dtpArrivalTime.Value,
                        Status = (FlightStatus)cmbStatus.SelectedItem
                    };
                    
                    // Create the flight using API service
                    var createdFlight = await _apiService.CreateFlightAsync(newFlight);
                    
                    if (createdFlight != null)
                    {
                        lblStatus.Text = "New flight created successfully.";
                        await LoadFlightsAsync(); // Refresh the list
                    }
                    else
                    {
                        throw new Exception("Failed to create new flight.");
                    }
                }
                else
                {
                    // Check if only the status has changed
                    var newStatus = (FlightStatus)cmbStatus.SelectedItem;
                    if (newStatus != _selectedFlight.Status)
                    {
                        // Update just the status using API service
                        bool success = await _apiService.UpdateFlightStatusAsync(_selectedFlight.FlightId, newStatus);
                        if (success)
                        {
                            _selectedFlight.Status = newStatus;
                            lblStatus.Text = "Flight status updated successfully.";
                        }
                        else
                        {
                            throw new Exception("Failed to update flight status.");
                        }
                    }
                    else
                    {
                        // Update all flight details
                        _selectedFlight.FlightNumber = txtFlightNumber.Text;
                        _selectedFlight.DepartureAirport = txtDepartureAirport.Text;
                        _selectedFlight.ArrivalAirport = txtArrivalAirport.Text;
                        _selectedFlight.DepartureTime = dtpDepartureTime.Value;
                        _selectedFlight.ArrivalTime = dtpArrivalTime.Value;
                        _selectedFlight.Status = newStatus;

                        // Update the flight using API service
                        bool success = await _apiService.UpdateFlightAsync(_selectedFlight);

                        if (success)
                        {
                            lblStatus.Text = "Flight updated successfully.";
                        }
                        else
                        {
                            throw new Exception("Failed to update flight details.");
                        }
                    }
                }

                // Refresh the flights list
                await LoadFlightsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error saving changes.";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void BtnAddFlight_Click(object sender, EventArgs e)
        {
            // Clear the current selection
            dgvFlights.ClearSelection();
            ClearFlightDetails();

            // Enable the controls for adding a new flight
            EnableFlightDetailsControls(true);
            txtFlightNumber.Focus();

            // Set default values
            dtpDepartureTime.Value = DateTime.Now.AddDays(1);
            dtpArrivalTime.Value = DateTime.Now.AddDays(1).AddHours(2);
            cmbStatus.SelectedItem = FlightStatus.Scheduled;

            // Set flag for adding new flight
            _selectedFlight = null;
            lblStatus.Text = "Adding new flight. Fill in details and click Save.";
        }

        private void ClearFlightDetails()
        {
            txtFlightNumber.Text = string.Empty;
            txtDepartureAirport.Text = string.Empty;
            txtArrivalAirport.Text = string.Empty;
            dtpDepartureTime.Value = DateTime.Now;
            dtpArrivalTime.Value = DateTime.Now.AddHours(2);
            cmbStatus.SelectedIndex = -1;
            lblSeats.Text = "Seats: N/A";
            lblBookings.Text = "Bookings: N/A";

            EnableFlightDetailsControls(false);
            _selectedFlight = null;
        }

        private void EnableFlightDetailsControls(bool enable)
        {
            txtFlightNumber.Enabled = enable;
            txtDepartureAirport.Enabled = enable;
            txtArrivalAirport.Enabled = enable;
            dtpDepartureTime.Enabled = enable;
            dtpArrivalTime.Enabled = enable;
            cmbStatus.Enabled = enable;
            btnSave.Enabled = enable;
        }
    }
}
