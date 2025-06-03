using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.FormsApp.Services;
using Newtonsoft.Json;

namespace FlightCheckInSystem.FormsApp
{
    public partial class SeatSelectionForm : Form
    {
        private readonly SeatStatusSignalRService _signalRService;
        private readonly Flight _selectedFlight;
        private readonly string _passengerName;
        private readonly string _bookingReference;

        private List<Seat> _seats = new List<Seat>();
        private Dictionary<string, Button> _seatButtons = new Dictionary<string, Button>();
        private string _selectedSeat = null;

        private Panel _seatMapPanel;
        private Button _confirmButton;
        private Button _cancelButton;
        private Label _legendLabel;
        private Label _flightInfoLabel;

        public string SelectedSeat => _selectedSeat;

        public SeatSelectionForm(SeatStatusSignalRService signalRService, Flight flight, string passengerName, string bookingReference)
        {
            _signalRService = signalRService ?? throw new ArgumentNullException(nameof(signalRService));
            _selectedFlight = flight ?? throw new ArgumentNullException(nameof(flight));
            _passengerName = passengerName;
            _bookingReference = bookingReference;

            InitializeComponent();

            // Set flight info and title after initialization
            this.Text = $"Seat Selection - Flight {_selectedFlight.FlightNumber}";
            _flightInfoLabel.Text = $"Flight: {_selectedFlight.FlightNumber} | {_selectedFlight.DepartureAirport} → {_selectedFlight.ArrivalAirport}";

            SetupSignalREvents();
            LoadSeatsAsync();
        }

        private void SetupSignalREvents()
        {
            // Subscribe to seat status changes
            _signalRService.SeatBooked += OnSeatBooked;
            _signalRService.SeatReleased += OnSeatReleased;
            _signalRService.FlightSeatsReceived += OnFlightSeatsReceived;
            _signalRService.SeatReserved += OnSeatReserved;
            _signalRService.SeatReservationReleased += OnSeatReservationReleased;
            _signalRService.SeatReservationFailed += OnSeatReservationFailed;
        }

        private async void LoadSeatsAsync()
        {
            try
            {
                // Subscribe to flight updates
                await _signalRService.SubscribeToFlightAsync(_selectedFlight.FlightNumber);

                // Request current seat data
                await _signalRService.GetFlightSeatsAsync(_selectedFlight.FlightId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading seats: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSeatReserved(string flightNumber, string seatNumber, string bookingReference)
        {
            if (flightNumber != _selectedFlight.FlightNumber)
                return;

            bool isMyReservation = bookingReference == _bookingReference;

            this.Invoke(() =>
            {
                var seat = _seats.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat != null && _seatButtons.ContainsKey(seatNumber))
                {
                    var button = _seatButtons[seatNumber];

                    if (isMyReservation)
                    {
                        // Our reservation - keep it selected (yellow)
                        button.BackColor = Color.FromArgb(241, 196, 15); // Yellow
                        button.ForeColor = Color.Black;
                        button.Enabled = true;
                    }
                    else
                    {
                        // Someone else's reservation - show as temporarily unavailable
                        button.BackColor = Color.Orange;
                        button.ForeColor = Color.White;
                        button.Enabled = false;

                        // If this was our selected seat, clear it
                        if (_selectedSeat == seatNumber)
                        {
                            _selectedSeat = null;
                            _confirmButton.Enabled = false;
                            MessageBox.Show($"Seat {seatNumber} has been reserved by another passenger. Please select a different seat.",
                                "Seat Reserved", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            });
        }

        private void OnSeatReservationReleased(string flightNumber, string seatNumber)
        {
            if (flightNumber != _selectedFlight.FlightNumber)
                return;

            this.Invoke(() =>
            {
                var seat = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat != null && !seat.IsBooked && _seatButtons.ContainsKey(seatNumber))
                {
                    // Seat reservation released - make it available again
                    var button = _seatButtons[seatNumber];
                    button.BackColor = Color.FromArgb(39, 174, 96); // Green
                    button.ForeColor = Color.White;
                    button.Enabled = true;
                }
            });
        }

        private void OnSeatReservationFailed(string flightNumber, string seatNumber, string reason)
        {
            if (flightNumber != _selectedFlight.FlightNumber)
                return;

            this.Invoke(() =>
            {
                MessageBox.Show($"Could not reserve seat {seatNumber}: {reason}", "Reservation Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Clear our selection since reservation failed
                if (_selectedSeat == seatNumber)
                {
                    _selectedSeat = null;
                    _confirmButton.Enabled = false;
                }
            });
        }

        private void OnFlightSeatsReceived(string flightNumber, string seatDataJson)
        {
            if (flightNumber != _selectedFlight.FlightNumber)
                return;

            this.Invoke(() =>
            {
                try
                {
                    _seats = JsonConvert.DeserializeObject<List<Seat>>(seatDataJson) ?? new List<Seat>();
                    CreateSeatMap();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error parsing seat data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        private void OnSeatBooked(string flightNumber, string seatNumber, string bookingReference)
        {
            if (flightNumber != _selectedFlight.FlightNumber)
                return;

            this.Invoke(() =>
            {
                var seat = _seats.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat != null)
                {
                    seat.IsBooked = true;
                    UpdateSeatButton(seat);
                }

                // If the booked seat was our selection, clear it (unless it's our own booking)
                if (_selectedSeat == seatNumber && bookingReference != _bookingReference)
                {
                    _selectedSeat = null;
                    _confirmButton.Enabled = false;
                }
            });
        }

        private void OnSeatReleased(string flightNumber, string seatNumber)
        {
            if (flightNumber != _selectedFlight.FlightNumber)
                return;

            this.Invoke(() =>
            {
                var seat = _seats.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat != null)
                {
                    seat.IsBooked = false;
                    UpdateSeatButton(seat);
                }
            });
        }

        private void CreateSeatMap()
        {
            _seatMapPanel.Controls.Clear();
            _seatButtons.Clear();

            if (!_seats.Any())
            {
                var noSeatsLabel = new Label
                {
                    Text = "No seats available for this flight",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.Gray,
                    Location = new Point(300, 150),
                    Size = new Size(300, 30)
                };
                _seatMapPanel.Controls.Add(noSeatsLabel);
                return;
            }

            // Group seats by row
            var seatsByRow = _seats.GroupBy(s => GetRowFromSeatNumber(s.SeatNumber))
                                  .OrderBy(g => g.Key)
                                  .ToList();

            int y = 20;
            foreach (var rowGroup in seatsByRow)
            {
                // Row label
                var rowLabel = new Label
                {
                    Text = $"Row {rowGroup.Key}",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Location = new Point(20, y + 5),
                    Size = new Size(50, 25)
                };
                _seatMapPanel.Controls.Add(rowLabel);

                // Seats in row
                var seatsInRow = rowGroup.OrderBy(s => s.SeatNumber).ToList();
                int x = 80;

                foreach (var seat in seatsInRow)
                {
                    var seatButton = new Button
                    {
                        Text = seat.SeatNumber,
                        Size = new Size(40, 40),
                        Location = new Point(x, y),
                        Font = new Font("Segoe UI", 8, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        Tag = seat
                    };

                    UpdateSeatButton(seat, seatButton);
                    seatButton.Click += SeatButton_Click;

                    _seatMapPanel.Controls.Add(seatButton);
                    _seatButtons[seat.SeatNumber] = seatButton;

                    x += 50;

                    // Add aisle space after C and before D (common airplane layout)
                    if (seat.SeatNumber.EndsWith("C"))
                        x += 20;
                }

                y += 60;
            }
        }

        private void UpdateSeatButton(Seat seat, Button button = null)
        {
            if (button == null && !_seatButtons.TryGetValue(seat.SeatNumber, out button))
                return;

            if (seat.IsBooked)
            {
                button.BackColor = Color.FromArgb(231, 76, 60); // Red
                button.ForeColor = Color.White;
                button.Enabled = false;
            }
            else if (_selectedSeat == seat.SeatNumber)
            {
                button.BackColor = Color.FromArgb(241, 196, 15); // Yellow
                button.ForeColor = Color.Black;
                button.Enabled = true;
            }
            else
            {
                button.BackColor = Color.FromArgb(39, 174, 96); // Green
                button.ForeColor = Color.White;
                button.Enabled = true;
            }
        }

        private async void SeatButton_Click(object sender, EventArgs e)
        {
            if (!(sender is Button button) || !(button.Tag is Seat seat))
                return;

            if (seat.IsBooked)
                return;

            // Release previous reservation if any
            if (!string.IsNullOrEmpty(_selectedSeat) && _selectedSeat != seat.SeatNumber)
            {
                await _signalRService.ReleaseSeatReservationAsync(_selectedFlight.FlightId, _selectedSeat);
            }

            // Try to reserve the new seat
            try
            {
                await _signalRService.ReserveSeatAsync(_selectedFlight.FlightId, seat.SeatNumber, _bookingReference);

                // Set selection immediately (will be confirmed or rejected by SignalR events)
                _selectedSeat = seat.SeatNumber;
                UpdateSeatButton(seat);
                _confirmButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reserving seat: {ex.Message}", "Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ConfirmButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedSeat))
                return;

            try
            {
                // Note: This form should NOT actually book the seat permanently
                // It should only confirm the reservation. The actual booking
                // should happen via the API in the check-in process.

                // For now, we'll just close the dialog and return the selected seat
                // The calling code should handle the actual booking

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error confirming seat selection: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetRowFromSeatNumber(string seatNumber)
        {
            // Extract row number from seat number (e.g., "12A" -> 12)
            var rowStr = new string(seatNumber.TakeWhile(char.IsDigit).ToArray());
            return int.TryParse(rowStr, out int row) ? row : 0;
        }

        protected override async void OnFormClosed(FormClosedEventArgs e)
        {
            // Release any seat reservations before closing if we're not confirming the selection
            if (this.DialogResult != DialogResult.OK && !string.IsNullOrEmpty(_selectedSeat))
            {
                try
                {
                    await _signalRService.ReleaseSeatReservationAsync(_selectedFlight.FlightId, _selectedSeat);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SeatSelectionForm] Error releasing reservation during close: {ex.Message}");
                }
            }

            // Unsubscribe from SignalR events
            _signalRService.SeatBooked -= OnSeatBooked;
            _signalRService.SeatReleased -= OnSeatReleased;
            _signalRService.FlightSeatsReceived -= OnFlightSeatsReceived;
            _signalRService.SeatReserved -= OnSeatReserved;
            _signalRService.SeatReservationReleased -= OnSeatReservationReleased;
            _signalRService.SeatReservationFailed -= OnSeatReservationFailed;

            // Unsubscribe from flight updates
            try
            {
                await _signalRService.UnsubscribeFromFlightAsync(_selectedFlight.FlightNumber);
            }
            catch { /* Ignore errors during cleanup */ }

            base.OnFormClosed(e);
        }
    }
}