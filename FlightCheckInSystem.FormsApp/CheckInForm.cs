using System;
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
using Newtonsoft.Json;

namespace FlightCheckInSystem.FormsApp
{
    public partial class CheckInForm : Form
    {
        private readonly ApiService _apiService;
        private readonly SeatStatusSignalRService _seatStatusSignalRService;
        private readonly BoardingPassPrinter _boardingPassPrinter;

        private List<Flight> _flights;
        private List<Passenger> _passengers;
        private List<Booking> _bookings;
        private List<Seat> _seats;

        private Booking _selectedBooking;
        private Seat _selectedSeat;
        private Dictionary<string, Button> _seatButtons;

        private bool _suppressSeatUnavailableWarning = false;
        private string _myBookingReference;
        private bool _isCheckingIn = false;

        public CheckInForm(ApiService apiService, SeatStatusSignalRService seatStatusSignalRService)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _seatStatusSignalRService = seatStatusSignalRService ?? throw new ArgumentNullException(nameof(seatStatusSignalRService));
            _boardingPassPrinter = new BoardingPassPrinter();
            _seatButtons = new Dictionary<string, Button>();

            SetupSignalREvents();
        }

        private void SetupSignalREvents()
        {
            // Subscribe to seat status changes
            _seatStatusSignalRService.SeatBooked += OnSeatBooked;
            _seatStatusSignalRService.SeatReleased += OnSeatReleased;
            _seatStatusSignalRService.FlightSeatsReceived += OnFlightSeatsReceived;
            _seatStatusSignalRService.SeatReserved += OnSeatReserved;
            _seatStatusSignalRService.SeatReservationReleased += OnSeatReservationReleased;
            _seatStatusSignalRService.SeatReservationFailed += OnSeatReservationFailed;
        }

        private void OnSeatReserved(string flightNumber, string seatNumber, string bookingReference)
        {
            if (_selectedBooking?.Flight?.FlightNumber != flightNumber)
                return;

            bool isMyReservation = bookingReference == _myBookingReference;

            this.Invoke(() =>
            {
                var seat = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat != null && _seatButtons.ContainsKey(seatNumber))
                {
                    var button = _seatButtons[seatNumber];

                    if (isMyReservation)
                    {
                        // Our reservation - keep it blue (selected)
                        button.BackColor = Color.Blue;
                        button.Enabled = true;
                        Debug.WriteLine($"[CheckInForm] Seat {seatNumber} reserved by us");
                    }
                    else
                    {
                        // Someone else's reservation - show as temporarily unavailable
                        button.BackColor = Color.Orange;
                        button.Enabled = false;

                        // If this was our selected seat, clear it
                        if (_selectedSeat?.SeatNumber == seatNumber)
                        {
                            _selectedSeat = null;
                            lblSelectedSeat.Text = "Selected Seat: (Reserved by another)";
                            btnCheckIn.Enabled = false;

                            if (!_suppressSeatUnavailableWarning)
                            {
                                MessageBox.Show($"Seat {seatNumber} has been reserved by another passenger. Please select a different seat.",
                                    "Seat Reserved", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
            });
        }

        private void OnSeatReservationReleased(string flightNumber, string seatNumber)
        {
            if (_selectedBooking?.Flight?.FlightNumber != flightNumber)
                return;

            this.Invoke(() =>
            {
                var seat = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat != null && !seat.IsBooked && _seatButtons.ContainsKey(seatNumber))
                {
                    // Seat reservation released - make it available again
                    var button = _seatButtons[seatNumber];
                    button.BackColor = Color.Green;
                    button.Enabled = true;
                }
            });
        }

        private void OnSeatReservationFailed(string flightNumber, string seatNumber, string reason)
        {
            if (_selectedBooking?.Flight?.FlightNumber != flightNumber)
                return;

            this.Invoke(() =>
            {
                MessageBox.Show($"Could not reserve seat {seatNumber}: {reason}", "Reservation Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Clear our selection since reservation failed
                if (_selectedSeat?.SeatNumber == seatNumber)
                {
                    _selectedSeat = null;
                    lblSelectedSeat.Text = "Selected Seat: (None)";
                    btnCheckIn.Enabled = false;
                }
            });
        }

        private void OnSeatBooked(string flightNumber, string seatNumber, string bookingReference)
        {
            if (_selectedBooking?.Flight?.FlightNumber != flightNumber)
                return;

            this.Invoke(() =>
            {
                var seat = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat != null)
                {
                    seat.IsBooked = true;
                    if (_seatButtons.ContainsKey(seatNumber))
                    {
                        var button = _seatButtons[seatNumber];
                        button.BackColor = Color.Red;
                        button.Enabled = false;
                    }
                }

                // Clear selection if this seat was selected by someone else
                if (_selectedSeat?.SeatNumber == seatNumber && bookingReference != _myBookingReference)
                {
                    _selectedSeat = null;
                    lblSelectedSeat.Text = "Selected Seat: (Booked by another)";
                    btnCheckIn.Enabled = false;
                }
            });
        }

        private void OnSeatReleased(string flightNumber, string seatNumber)
        {
            if (_selectedBooking?.Flight?.FlightNumber != flightNumber)
                return;

            this.Invoke(() =>
            {
                var seat = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat != null)
                {
                    seat.IsBooked = false;
                    if (_seatButtons.ContainsKey(seatNumber))
                    {
                        var button = _seatButtons[seatNumber];
                        button.BackColor = Color.Green;
                        button.Enabled = true;
                    }
                }
            });
        }

        private void OnFlightSeatsReceived(string flightNumber, string seatDataJson)
        {
            if (_selectedBooking?.Flight?.FlightNumber != flightNumber)
                return;

            this.Invoke(() =>
            {
                try
                {
                    var receivedSeats = JsonConvert.DeserializeObject<List<Seat>>(seatDataJson) ?? new List<Seat>();
                    _seats = receivedSeats;
                    InitializeSeatPanel(_seats);
                    UpdateSeatDisplay(_selectedBooking.FlightId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error parsing seat data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        private async void CheckInForm_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
            ResetForm();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var flightsTask = _apiService.GetFlightsAsync();
                var bookingsTask = _apiService.GetBookingsAsync();

                _flights = await flightsTask ?? new List<Flight>();
                _bookings = await bookingsTask ?? new List<Booking>();
                _passengers = new List<Passenger>();
                _seats = new List<Seat>();

                if (!_flights.Any())
                {
                    MessageBox.Show("No flights loaded from server. Please check your server/API.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading initial data: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeSeatPanel(List<Seat> flightSeats)
        {
            pnlSeats.Controls.Clear();
            _seatButtons = new Dictionary<string, Button>();

            if (_seats == null || !_seats.Any())
            {
                return;
            }

            int buttonSize = 40, spacing = 10;
            int x = 20, y = 20;

            var groupedByRowNumber = _seats.GroupBy(s => {
                string numericPart = new string(s.SeatNumber.TakeWhile(char.IsDigit).ToArray());
                if (int.TryParse(numericPart, out int rowNumber))
                {
                    return rowNumber;
                }
                return 0;
            })
            .OrderBy(g => g.Key);

            foreach (var rowGroup in groupedByRowNumber)
            {
                int col = 0;
                foreach (var seat in rowGroup.OrderBy(s => s.SeatId))
                {
                    var button = new Button
                    {
                        Text = seat.SeatNumber,
                        Size = new Size(buttonSize, buttonSize),
                        Location = new Point(x + col * (buttonSize + spacing), y),
                        Tag = seat.SeatNumber,
                        BackColor = seat.IsBooked ? Color.Red : Color.Green,
                        ForeColor = Color.White,
                        Enabled = !seat.IsBooked,
                        FlatStyle = FlatStyle.Flat
                    };
                    button.Click += SeatButton_Click;
                    pnlSeats.Controls.Add(button);
                    _seatButtons[seat.SeatNumber] = button;
                    col++;

                    // Add aisle space after C seats
                    if (seat.SeatNumber.EndsWith("C"))
                        col++;
                }
                y += buttonSize + spacing;
            }
        }

        private async Task LoadSeatsForFlightAsync(int flightId)
        {
            try
            {
                // Subscribe to real-time seat updates for this flight
                var flight = _flights?.FirstOrDefault(f => f.FlightId == flightId);
                if (flight != null)
                {
                    await _seatStatusSignalRService.SubscribeToFlightAsync(flight.FlightNumber);
                    await _seatStatusSignalRService.GetFlightSeatsAsync(flightId);
                }

                // Also get seats from API as fallback
                var seatsFromServer = await _apiService.GetSeatsByFlightAsync(flightId);
                if (seatsFromServer != null)
                {
                    _seats = seatsFromServer;
                    InitializeSeatPanel(_seats);
                    UpdateSeatDisplay(flightId);
                    return;
                }
                else
                {
                    MessageBox.Show($"No seats returned from server for flight {flightId}.", "Seat Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading seats from server: {ex.Message}.", "Seat Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ResetForm()
        {
            txtPassportNumber.Clear();
            grpBookingDetails.Visible = false;
            grpSeatSelection.Visible = false;
            grpBoardingPass.Visible = false;
            btnCheckIn.Visible = false;
            _selectedBooking = null;
            _selectedSeat = null;
            _myBookingReference = null;
            _isCheckingIn = false;
            lblSelectedSeat.Text = "Selected Seat: (None)";
            pnlSeats.Controls.Clear();
            _seatButtons.Clear();
            _suppressSeatUnavailableWarning = false;
            txtPassportNumber.Focus();
        }

        private void ResetFormAfterBoardingPass()
        {
            txtPassportNumber.Clear();
            grpBookingDetails.Visible = false;
            grpSeatSelection.Visible = false;
            grpBoardingPass.Visible = false;
            btnCheckIn.Visible = false;

            _selectedBooking = null;
            _selectedSeat = null;
            _myBookingReference = null;
            _isCheckingIn = false;
            lblSelectedSeat.Text = "Selected Seat: (None)";

            pnlSeats.Controls.Clear();
            _seatButtons.Clear();

            _suppressSeatUnavailableWarning = false;

            txtPassportNumber.Focus();
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassportNumber.Text))
            {
                MessageBox.Show("Please enter a passport number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string passportNumberToSearch = txtPassportNumber.Text.Trim();

            try
            {
                var bookingsFromServer = await _apiService.GetBookingsByPassportAsync(passportNumberToSearch);
                if (bookingsFromServer != null && bookingsFromServer.Any())
                {
                    var alreadyCheckedIn = bookingsFromServer.FirstOrDefault(b => b.IsCheckedIn);
                    if (alreadyCheckedIn != null)
                    {
                        _selectedBooking = alreadyCheckedIn;
                        if (_selectedBooking.SeatId > 0)
                        {
                            if (_seats == null || !_seats.Any() || !_seats.Any(s => s.SeatId == _selectedBooking.SeatId))
                            {
                                _seats = await _apiService.GetSeatsByFlightAsync(_selectedBooking.FlightId);
                            }
                            _selectedSeat = _seats.FirstOrDefault(s => s.SeatId == _selectedBooking.SeatId);
                        }
                        else if (!string.IsNullOrEmpty(_selectedBooking.Seat.SeatNumber))
                        {
                            if (_seats == null || !_seats.Any())
                            {
                                _seats = await _apiService.GetSeatsByFlightAsync(_selectedBooking.FlightId);
                            }
                            _selectedSeat = _seats.FirstOrDefault(s => s.SeatNumber == _selectedBooking.Seat.SeatNumber);
                        }
                        else
                        {
                            _selectedSeat = null;
                        }

                        var boardingPass = CreateBoardingPassFromBooking(_selectedBooking, _selectedSeat);
                        ShowBoardingPassDialog(boardingPass);

                        ResetFormAfterBoardingPass();
                        return;
                    }

                    var activeBookings = bookingsFromServer.Where(b => !b.IsCheckedIn).ToList();
                    if (!activeBookings.Any())
                    {
                        MessageBox.Show("No active (not checked-in) bookings found for this passenger on the server.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetForm();
                        return;
                    }

                    _selectedBooking = activeBookings.First();
                    // Set our booking reference for tracking
                    _myBookingReference = _selectedBooking.BookingId.ToString();
                    DisplayBookingDetails(_selectedBooking);

                    await LoadSeatsForFlightAsync(_selectedBooking.FlightId);
                    return;
                }
                else
                {
                    MessageBox.Show("No bookings found for this passport number on the server.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching bookings on server: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetForm();
            }
        }

        private void DisplayBookingDetails(Booking booking)
        {
            if (booking == null)
            {
                DisplayBookingDetails(null, null, null);
                grpSeatSelection.Visible = false;
                btnCheckIn.Visible = false;
                return;
            }

            Passenger passenger = booking.Passenger;
            if (passenger == null && booking.PassengerId > 0 && _passengers != null)
            {
                passenger = _passengers.FirstOrDefault(p => p.PassengerId == booking.PassengerId);
            }

            Flight flight = booking.Flight;
            if (flight == null && booking.FlightId > 0 && _flights != null)
            {
                flight = _flights.FirstOrDefault(f => f.FlightId == booking.FlightId);
            }

            DisplayBookingDetails(booking, passenger, flight);

            if (grpBookingDetails.Visible && passenger != null && flight != null)
            {
                grpSeatSelection.Visible = true;
                btnCheckIn.Visible = true;
                btnCheckIn.Enabled = false;
            }
            else
            {
                grpSeatSelection.Visible = false;
                btnCheckIn.Visible = false;
            }
        }

        private void DisplayBookingDetails(Booking booking, Passenger passenger, Flight flight)
        {
            if (booking == null || passenger == null || flight == null)
            {
                lblPassengerInfo.Text = passenger != null ? $"Passenger: {passenger.FirstName} {passenger.LastName} (Passport: {passenger.PassportNumber})" : "Passenger: Error loading data";
                lblFlightInfo.Text = flight != null ? $"Flight: {flight.FlightNumber} ({flight.DepartureAirport} to {flight.ArrivalAirport}) - Departs: {flight.DepartureTime:g}" : "Flight: Error loading data";

                if (booking == null && passenger == null && flight == null)
                {
                    grpBookingDetails.Visible = false;
                }
                else
                {
                    grpBookingDetails.Visible = true;
                    MessageBox.Show("Could not retrieve complete booking details. Some information may be missing.", "Partial Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            lblPassengerInfo.Text = $"Passenger: {passenger.FirstName} {passenger.LastName} (Passport: {passenger.PassportNumber})";
            lblFlightInfo.Text = $"Flight: {flight.FlightNumber} ({flight.DepartureAirport} to {flight.ArrivalAirport}) - Departs: {flight.DepartureTime:g}";

            grpBookingDetails.Visible = true;
        }

        private void UpdateSeatDisplay(int flightId)
        {
            if (_seatButtons == null || !_seatButtons.Any())
            {
                return;
            }

            var flightSeats = _seats?.Where(s => s.FlightId == flightId).ToList() ?? new List<Seat>();

            foreach (var seatNumKey in _seatButtons.Keys)
            {
                var button = _seatButtons[seatNumKey];
                var seatData = flightSeats.FirstOrDefault(s => s.SeatNumber == seatNumKey);

                bool isBooked = seatData?.IsBooked ?? false;
                UpdateSeatButton(button, seatNumKey, isBooked);
            }
            grpSeatSelection.Visible = true;
        }

        private void UpdateSeatButton(Button button, string seatNumber, bool isBooked)
        {
            if (button.InvokeRequired)
            {
                button.Invoke(new Action(() => UpdateSeatButtonUI(button, seatNumber, isBooked)));
            }
            else
            {
                UpdateSeatButtonUI(button, seatNumber, isBooked);
            }
        }

        private void UpdateSeatButtonUI(Button button, string seatNumber, bool isBooked)
        {
            if (isBooked)
            {
                button.BackColor = Color.Red;
                button.Enabled = false;
            }
            else if (_selectedSeat != null && _selectedSeat.SeatNumber == seatNumber)
            {
                button.BackColor = Color.Blue; // Selected seat
                button.Enabled = true;
            }
            else
            {
                button.BackColor = Color.Green; // Available seat
                button.Enabled = true;
            }
            button.ForeColor = Color.White;
        }

        private async void SeatButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string seatNumber = button.Tag.ToString();

            if (_selectedBooking == null)
            {
                MessageBox.Show("Please search for and select a booking first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_isCheckingIn)
            {
                MessageBox.Show("Check-in is already in progress. Please wait.", "Check-in In Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var seatData = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber && s.FlightId == _selectedBooking.FlightId);
            if (seatData == null)
            {
                MessageBox.Show("Seat data could not be found for this seat. Please reload seat map or try again.", "Seat Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (seatData.IsBooked)
            {
                MessageBox.Show("This seat is already booked. Please select another seat.", "Seat Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Release previous reservation if any
            if (_selectedSeat != null && _selectedSeat.SeatNumber != seatNumber)
            {
                await _seatStatusSignalRService.ReleaseSeatReservationAsync(_selectedBooking.FlightId, _selectedSeat.SeatNumber);
            }

            // Try to reserve the new seat
            try
            {
                await _seatStatusSignalRService.ReserveSeatAsync(_selectedBooking.FlightId, seatNumber, _myBookingReference);

                // Set selection immediately (will be confirmed or rejected by SignalR events)
                _selectedSeat = seatData;
                lblSelectedSeat.Text = $"Selected Seat: {_selectedSeat.SeatNumber}";
                btnCheckIn.Enabled = true;

                // Update UI immediately
                UpdateSeatButtonUI(button, seatNumber, false);
                button.BackColor = Color.Blue; // Show as selected
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reserving seat: {ex.Message}", "Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnCheckIn_Click(object sender, EventArgs e)
        {
            if (_selectedBooking == null || _selectedSeat == null)
            {
                MessageBox.Show("Please select a booking and a seat.", "Check-In Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_isCheckingIn)
            {
                MessageBox.Show("Check-in is already in progress. Please wait.", "Check-in In Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _isCheckingIn = true;
                btnCheckIn.Enabled = false;

                // Perform check-in via API (this will handle the actual seat booking in database)
                var response = await _apiService.CheckInAsync(_selectedBooking.BookingId, _selectedSeat.SeatId != 0 ? _selectedSeat.SeatId : _selectedSeat.SeatId);
                if (response != null && response.Success)
                {
                    _selectedBooking.IsCheckedIn = true;
                    _selectedBooking.Seat.SeatNumber = _selectedSeat.SeatNumber;
                    _selectedBooking.SeatId = _selectedSeat.SeatId != 0 ? _selectedSeat.SeatId : _selectedSeat.SeatId;

                    var actualSeatInList = _seats?.FirstOrDefault(s => s.FlightId == _selectedBooking.FlightId && s.SeatNumber == _selectedSeat.SeatNumber);
                    if (actualSeatInList != null) actualSeatInList.IsBooked = true;

                    // Notify SignalR hub to confirm the booking (this will broadcast to other clients)
                    var flight = _flights?.FirstOrDefault(f => f.FlightId == _selectedBooking.FlightId);
                    if (flight != null)
                    {
                        await _seatStatusSignalRService.ConfirmSeatBookingAsync(flight.FlightNumber, _selectedSeat.SeatNumber, _myBookingReference);
                    }

                    BoardingPass boardingPass;
                    if (response.BoardingPass != null)
                    {
                        boardingPass = response.BoardingPass;
                    }
                    else
                    {
                        boardingPass = CreateBoardingPassFromBooking(_selectedBooking, _selectedSeat);
                    }

                    MessageBox.Show(response.Message ?? "Check-in successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    _suppressSeatUnavailableWarning = true;
                    if (_seatButtons.ContainsKey(_selectedSeat.SeatNumber))
                    {
                        UpdateSeatButton(_seatButtons[_selectedSeat.SeatNumber], _selectedSeat.SeatNumber, true);
                    }

                    ShowBoardingPassDialog(boardingPass);
                    ResetFormAfterBoardingPass();
                }
                else
                {
                    MessageBox.Show(response?.Message ?? "Check-in failed. Please try again.", "Check-In Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Release our reservation since check-in failed
                    await _seatStatusSignalRService.ReleaseSeatReservationAsync(_selectedBooking.FlightId, _selectedSeat.SeatNumber);
                    if (_selectedBooking != null) await LoadSeatsForFlightAsync(_selectedBooking.FlightId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during check-in: {ex.Message}", "Check-In Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Release our reservation on error
                if (_selectedSeat != null)
                {
                    await _seatStatusSignalRService.ReleaseSeatReservationAsync(_selectedBooking.FlightId, _selectedSeat.SeatNumber);
                }
                if (_selectedBooking != null) await LoadSeatsForFlightAsync(_selectedBooking.FlightId);
            }
            finally
            {
                _isCheckingIn = false;
                if (!_selectedBooking?.IsCheckedIn == true)
                {
                    btnCheckIn.Enabled = _selectedSeat != null;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        private BoardingPass CreateBoardingPassFromBooking(Booking booking, Seat seat)
        {
            if (booking?.Passenger == null || booking?.Flight == null)
            {
                Debug.WriteLine("[CheckInForm] Cannot create boarding pass - missing booking, passenger, or flight data");
                return null;
            }

            return new BoardingPass
            {
                PassengerName = $"{booking.Passenger.FirstName} {booking.Passenger.LastName}",
                PassportNumber = booking.Passenger.PassportNumber,
                FlightNumber = booking.Flight.FlightNumber,
                DepartureAirport = booking.Flight.DepartureAirport,
                ArrivalAirport = booking.Flight.ArrivalAirport,
                DepartureTime = booking.Flight.DepartureTime,
                SeatNumber = seat?.SeatNumber ?? booking.Seat.SeatNumber ?? "TBD",
                BoardingTime = booking.Flight.DepartureTime.AddMinutes(-45)
            };
        }

        private void ShowBoardingPassDialog(BoardingPass boardingPass)
        {
            if (boardingPass == null)
            {
                MessageBox.Show("Unable to generate boarding pass. Missing required information.",
                    "Boarding Pass Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var dialog = new BoardingPassDialog(boardingPass))
                {
                    DialogResult result = dialog.ShowDialog(this);

                    if (dialog.StartNewCheckIn)
                    {
                        txtPassportNumber.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying boarding pass: {ex.Message}",
                    "Display Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrintBoardingPass_Click(object sender, EventArgs e)
        {
            if (_selectedBooking == null || !_selectedBooking.IsCheckedIn)
            {
                MessageBox.Show("No valid boarding pass to print.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var boardingPass = CreateBoardingPassFromBooking(_selectedBooking, _selectedSeat);
                if (boardingPass != null)
                {
                    _boardingPassPrinter.PrintBoardingPass(boardingPass);
                }
                else
                {
                    MessageBox.Show("Unable to generate boarding pass for printing.", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing boarding pass: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtPassportNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                if (btnSearch.Enabled && !string.IsNullOrWhiteSpace(txtPassportNumber.Text))
                {
                    btnSearch_Click(btnSearch, EventArgs.Empty);
                }
            }
        }

        protected override async void OnFormClosed(FormClosedEventArgs e)
        {
            // Release any seat reservations before closing
            if (_selectedSeat != null && _selectedBooking != null && !_selectedBooking.IsCheckedIn)
            {
                try
                {
                    await _seatStatusSignalRService.ReleaseSeatReservationAsync(_selectedBooking.FlightId, _selectedSeat.SeatNumber);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CheckInForm] Error releasing reservation during close: {ex.Message}");
                }
            }

            // Unsubscribe from SignalR events
            try
            {
                _seatStatusSignalRService.SeatBooked -= OnSeatBooked;
                _seatStatusSignalRService.SeatReleased -= OnSeatReleased;
                _seatStatusSignalRService.FlightSeatsReceived -= OnFlightSeatsReceived;
                _seatStatusSignalRService.SeatReserved -= OnSeatReserved;
                _seatStatusSignalRService.SeatReservationReleased -= OnSeatReservationReleased;
                _seatStatusSignalRService.SeatReservationFailed -= OnSeatReservationFailed;

                // Unsubscribe from flight updates
                if (_selectedBooking?.Flight != null)
                {
                    await _seatStatusSignalRService.UnsubscribeFromFlightAsync(_selectedBooking.Flight.FlightNumber);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error during cleanup: {ex.Message}");
            }

            try
            {
                _boardingPassPrinter?.Dispose();

                if (_seatButtons != null)
                {
                    _seatButtons.Clear();
                    _seatButtons = null;
                }

                _flights?.Clear();
                _passengers?.Clear();
                _bookings?.Clear();
                _seats?.Clear();

                Debug.WriteLine("[CheckInForm] Dispose completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error during dispose: {ex.Message}");
            }

            base.OnFormClosed(e);
        }
    }
}