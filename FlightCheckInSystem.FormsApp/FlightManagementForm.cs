using System;
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
        private List<Flight> _flights;
        private Flight _selectedFlight;

        public FlightManagementForm(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            cmbStatus.DataSource = Enum.GetValues(typeof(FlightStatus));
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;

            dgvFlights.AutoGenerateColumns = false;
            dgvFlights.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFlights.MultiSelect = false;
            dgvFlights.ReadOnly = true;

            this.Load += async (s, e) => await LoadFlightsAsync();

            dgvFlights.SelectionChanged += DgvFlights_SelectionChanged;
            btnSave.Click += async (s, e) => await SaveFlightChangesAsync();
            btnRefresh.Click += async (s, e) => await LoadFlightsAsync();
            btnAddFlight.Click += BtnAddFlight_Click;
        }

        private async Task LoadFlightsAsync()
        {
            try
            {
                lblStatusBar.Text = "Loading flights...";
                this.Cursor = Cursors.WaitCursor;

                _flights = await _apiService.GetFlightsAsync();
                if (_flights != null && _flights.Any())
                {
                    dgvFlights.DataSource = _flights;
                    lblStatusBar.Text = $"Loaded {_flights.Count} flights.";
                }
                else
                {
                    lblStatusBar.Text = "No flights found.";
                }
            }
            catch (Exception ex)
            {
                lblStatusBar.Text = "Error loading flights.";
                MessageBox.Show($"Error loading flights: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    lblStatusBar.Text = "Loading flight details...";
                    this.Cursor = Cursors.WaitCursor;

                    _selectedFlight = await _apiService.GetFlightByIdAsync(selectedFlight.FlightId);

                    if (_selectedFlight == null)
                    {
                        MessageBox.Show("Could not load flight details. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    txtFlightNumber.Text = _selectedFlight.FlightNumber;
                    txtDepartureAirport.Text = _selectedFlight.DepartureAirport;
                    txtArrivalAirport.Text = _selectedFlight.ArrivalAirport;
                    dtpDepartureTime.Value = _selectedFlight.DepartureTime;
                    dtpArrivalTime.Value = _selectedFlight.ArrivalTime;
                    cmbStatus.SelectedItem = _selectedFlight.Status;

                    EnableFlightDetailsControls(true);

                    lblStatusBar.Text = "Flight details loaded.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading flight details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatusBar.Text = "Error loading flight details.";
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
                lblStatusBar.Text = "Saving changes...";
                this.Cursor = Cursors.WaitCursor;

                if (_selectedFlight == null)
                {
                    var newFlight = new Flight
                    {
                        FlightNumber = txtFlightNumber.Text,
                        DepartureAirport = txtDepartureAirport.Text,
                        ArrivalAirport = txtArrivalAirport.Text,
                        DepartureTime = dtpDepartureTime.Value,
                        ArrivalTime = dtpArrivalTime.Value,
                        Status = (FlightStatus)cmbStatus.SelectedItem
                    };

                    var createdFlight = await _apiService.CreateFlightAsync(newFlight);

                    if (createdFlight != null)
                    {
                        lblStatusBar.Text = "New flight created successfully.";
                        await LoadFlightsAsync();
                    }
                    else
                    {
                        throw new Exception("Failed to create new flight.");
                    }
                }
                else
                {
                    var newStatus = (FlightStatus)cmbStatus.SelectedItem;
                    if (newStatus != _selectedFlight.Status)
                    {
                        bool success = await _apiService.UpdateFlightStatusAsync(_selectedFlight.FlightId, newStatus);
                        if (success)
                        {
                            _selectedFlight.Status = newStatus;
                            lblStatusBar.Text = "Flight status updated successfully.";
                        }
                        else
                        {
                            throw new Exception("Failed to update flight status.");
                        }
                    }
                    else
                    {
                        _selectedFlight.FlightNumber = txtFlightNumber.Text;
                        _selectedFlight.DepartureAirport = txtDepartureAirport.Text;
                        _selectedFlight.ArrivalAirport = txtArrivalAirport.Text;
                        _selectedFlight.DepartureTime = dtpDepartureTime.Value;
                        _selectedFlight.ArrivalTime = dtpArrivalTime.Value;
                        _selectedFlight.Status = newStatus;

                        bool success = await _apiService.UpdateFlightAsync(_selectedFlight);

                        if (success)
                        {
                            lblStatusBar.Text = "Flight updated successfully.";
                        }
                        else
                        {
                            throw new Exception("Failed to update flight details.");
                        }
                    }
                }

                await LoadFlightsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatusBar.Text = "Error saving changes.";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void BtnAddFlight_Click(object sender, EventArgs e)
        {
            dgvFlights.ClearSelection();
            ClearFlightDetails();

            EnableFlightDetailsControls(true);
            txtFlightNumber.Focus();

            dtpDepartureTime.Value = DateTime.Now.AddDays(1);
            dtpArrivalTime.Value = DateTime.Now.AddDays(1).AddHours(2);
            cmbStatus.SelectedItem = FlightStatus.Scheduled;

            _selectedFlight = null;
            lblStatusBar.Text = "Adding new flight. Fill in details and click Save.";
        }

        private void ClearFlightDetails()
        {
            txtFlightNumber.Text = string.Empty;
            txtDepartureAirport.Text = string.Empty;
            txtArrivalAirport.Text = string.Empty;
            dtpDepartureTime.Value = DateTime.Now;
            dtpArrivalTime.Value = DateTime.Now.AddHours(2);
            cmbStatus.SelectedIndex = -1;

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