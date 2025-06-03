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

        public CheckInForm(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _seatStatusSignalRService = new SeatStatusSignalRService("https://localhost:5001");
            _boardingPassPrinter = new BoardingPassPrinter();
            _seatButtons = new Dictionary<string, Button>();
        }

        private async void CheckInForm_Load(object sender, EventArgs e)
        {
            Debug.WriteLine("[CheckInForm] Form_Load event triggered.");
            await LoadDataAsync();
            ResetForm();         }

        private async Task LoadDataAsync()
        {
            Debug.WriteLine("[CheckInForm] LoadDataAsync started.");
            try
            {
                Debug.WriteLine("[CheckInForm] Attempting to load flights and bookings from server.");
                var flightsTask = _apiService.GetFlightsAsync();
                var bookingsTask = _apiService.GetBookingsAsync();

                _flights = await flightsTask ?? new List<Flight>();
                _bookings = await bookingsTask ?? new List<Booking>();

                                                _passengers = new List<Passenger>();
                _seats = new List<Seat>(); 
                if (_flights.Any())
                {
                    Debug.WriteLine($"[CheckInForm] Loaded {_flights.Count} flights from server.");
                                    }
                else
                {
                    Debug.WriteLine("[CheckInForm] No flights loaded from server or server data is empty.");
                    MessageBox.Show("No flights loaded from server. Please check your server/API.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error loading initial data: {ex.Message}.");
                MessageBox.Show($"Error loading initial data: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Debug.WriteLine("[CheckInForm] LoadDataAsync finished.");
        }

        private void InitializeSeatPanel(List<Seat> flightSeats)
        {
            Debug.WriteLine("[CheckInForm] InitializeSeatPanel started.");
            pnlSeats.Controls.Clear();
            _seatButtons = new Dictionary<string, Button>();

            if (_seats == null || !_seats.Any())
            {
                Debug.WriteLine("[CheckInForm] No seats to display.");
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
                return 0;             })
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
                        Enabled = !seat.IsBooked
                    };
                    button.Click += SeatButton_Click;
                    pnlSeats.Controls.Add(button);
                    _seatButtons[seat.SeatNumber] = button;
                    col++;
                }
                y += buttonSize + spacing;
            }
            Debug.WriteLine("[CheckInForm] InitializeSeatPanel finished.");
        }

        private void ResetForm()
        {
            Debug.WriteLine("[CheckInForm] ResetForm called.");
            txtPassportNumber.Clear();
            grpBookingDetails.Visible = false;
            grpSeatSelection.Visible = false;
            grpBoardingPass.Visible = false;
            btnCheckIn.Visible = false;
            _selectedBooking = null;
            _selectedSeat = null;
            lblSelectedSeat.Text = "Selected Seat: (None)";
                        pnlSeats.Controls.Clear();
            _seatButtons.Clear();
            txtPassportNumber.Focus();         }

        private void ResetFormAfterBoardingPass()
        {
            Debug.WriteLine("[CheckInForm] ResetFormAfterBoardingPass called.");
            
                        txtPassportNumber.Clear();
            grpBookingDetails.Visible = false;
            grpSeatSelection.Visible = false;
            grpBoardingPass.Visible = false;
            btnCheckIn.Visible = false;
            
                        _selectedBooking = null;
            _selectedSeat = null;
            lblSelectedSeat.Text = "Selected Seat: (None)";
            
                        pnlSeats.Controls.Clear();
            _seatButtons.Clear();
            
                        _suppressSeatUnavailableWarning = false;
            
                        txtPassportNumber.Focus();
            
            Debug.WriteLine("[CheckInForm] Form reset completed, ready for next passenger.");
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[CheckInForm] btnSearch_Click triggered.");
            if (string.IsNullOrWhiteSpace(txtPassportNumber.Text))
            {
                MessageBox.Show("Please enter a passport number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Debug.WriteLine($"[CheckInForm] Passport number to search: {txtPassportNumber.Text.Trim()}");

            string passportNumberToSearch = txtPassportNumber.Text.Trim();
            
            try
            {
                Debug.WriteLine($"[CheckInForm] Searching bookings for passport '{passportNumberToSearch}' on server.");
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
                        else if (!string.IsNullOrEmpty(_selectedBooking.SeatNumber))
                        {
                            if (_seats == null || !_seats.Any())
                            {
                                _seats = await _apiService.GetSeatsByFlightAsync(_selectedBooking.FlightId);
                            }
                            _selectedSeat = _seats.FirstOrDefault(s => s.SeatNumber == _selectedBooking.SeatNumber);
                        }
                        else
                        {
                            _selectedSeat = null;
                        }
                        
                        Debug.WriteLine($"[CheckInForm] Booking already checked in. Showing boarding pass for booking ID: {_selectedBooking.BookingId}.");
                        
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
                    _selectedBooking = activeBookings.First();                     Debug.WriteLine($"[CheckInForm] Found booking {_selectedBooking.BookingReference} on server.");
                    DisplayBookingDetails(_selectedBooking);

                                                            await LoadSeatsForFlightAsync(_selectedBooking.FlightId);

                                        Flight flightForSignalR = _selectedBooking.Flight;
                    if (flightForSignalR == null && _selectedBooking.FlightId > 0 && _flights != null)
                    {
                        flightForSignalR = _flights.FirstOrDefault(f => f.FlightId == _selectedBooking.FlightId);
                    }
                    if (flightForSignalR != null)
                    {
                        Debug.WriteLine($"[CheckInForm] Subscribed to SignalR updates for flight {flightForSignalR.FlightNumber}.");
                    }
                    else
                    {
                        Debug.WriteLine($"[CheckInForm] Could not determine flight number for SignalR subscription for booking ID: {_selectedBooking.BookingId}.");
                    }
                    return;                 }
                else
                {
                    Debug.WriteLine($"[CheckInForm] No bookings found for passport '{passportNumberToSearch}' on server.");
                    MessageBox.Show("No bookings found for this passport number on the server.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetForm();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error searching bookings on server: {ex.Message}.");
                MessageBox.Show($"Error searching bookings on server: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetForm();
            }
        }

        private void DisplayBookingDetails(Booking booking)
        {
            Debug.WriteLine($"[CheckInForm] DisplayBookingDetails (single arg) for booking '{booking?.BookingReference}'.");
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
                btnCheckIn.Enabled = false;             }
            else
            {
                grpSeatSelection.Visible = false;
                btnCheckIn.Visible = false;
            }
        }

        private void DisplayBookingDetails(Booking booking, Passenger passenger, Flight flight)
        {
            Debug.WriteLine($"[CheckInForm] DisplayBookingDetails for Booking ID: {booking?.BookingId}");

            if (booking == null || passenger == null || flight == null)
            {
                Debug.WriteLine("[CheckInForm] Booking, Passenger or Flight data is null in DisplayBookingDetails (3-arg).");
                lblBookingRef.Text = booking != null ? $"Booking Reference: {booking.BookingReference}" : "Booking Reference: Error";
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

            lblBookingRef.Text = $"Booking Reference: {booking.BookingReference}";
            lblPassengerInfo.Text = $"Passenger: {passenger.FirstName} {passenger.LastName} (Passport: {passenger.PassportNumber})";
            lblFlightInfo.Text = $"Flight: {flight.FlightNumber} ({flight.DepartureAirport} to {flight.ArrivalAirport}) - Departs: {flight.DepartureTime:g}";

            grpBookingDetails.Visible = true;
            Debug.WriteLine("[CheckInForm] Booking details displayed using consolidated labels.");
        }

        private async Task LoadSeatsForFlightAsync(int flightId)
        {
            Debug.WriteLine($"[CheckInForm] LoadSeatsForFlightAsync for flight ID {flightId}.");
            try
            {
                Debug.WriteLine($"[CheckInForm] Attempting to load seats for flight {flightId} from server.");
                var seatsFromServer = await _apiService.GetSeatsByFlightAsync(flightId);
                if (seatsFromServer != null)
                {
                    _seats = seatsFromServer;                     Debug.WriteLine($"[CheckInForm] Loaded {seatsFromServer.Count} seats from server for flight {flightId}.");
                    InitializeSeatPanel(_seats);                                         UpdateSeatDisplay(flightId);                     return;
                }
                else
                {
                    Debug.WriteLine($"[CheckInForm] No seats returned from server for flight {flightId}.");
                    MessageBox.Show($"No seats returned from server for flight {flightId}.", "Seat Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error loading seats from server for flight {flightId}: {ex.Message}.");
                MessageBox.Show($"Error loading seats from server: {ex.Message}.", "Seat Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateSeatDisplay(int flightId)
        {
            Debug.WriteLine($"[CheckInForm] UpdateSeatDisplay for flight ID {flightId}.");
                                    if (_seatButtons == null || !_seatButtons.Any())
            {
                Debug.WriteLine("[CheckInForm] _seatButtons is empty or null, cannot update seat display.");
                return;
            }

                                                                        var flightSeats = _seats?.Where(s => s.FlightId == flightId).ToList() ?? new List<Seat>();
            Debug.WriteLine($"[CheckInForm] Found {flightSeats.Count} seat records for flight ID {flightId} in current _seats list for update.");

            foreach (var seatNumKey in _seatButtons.Keys)
            {
                var button = _seatButtons[seatNumKey];
                var seatData = flightSeats.FirstOrDefault(s => s.SeatNumber == seatNumKey);

                bool isBooked = seatData?.IsBooked ?? false;
                UpdateSeatButton(button, seatNumKey, isBooked);
            }
            grpSeatSelection.Visible = true;
            Debug.WriteLine($"[CheckInForm] Seat display updated for flight ID {flightId}.");
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
                if (_selectedSeat != null && _selectedSeat.SeatNumber == seatNumber)
                {
                    Debug.WriteLine($"[CheckInForm] Seat {seatNumber} was selected but is now booked by another. Deselecting.");
                    _selectedSeat = null;
                    lblSelectedSeat.Text = "Selected Seat: (Taken)";
                    btnCheckIn.Enabled = false;
                                        if (!_suppressSeatUnavailableWarning)
                    {
                        MessageBox.Show($"Seat {seatNumber} has just been booked by another passenger. Please select a different seat.", "Seat Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    _suppressSeatUnavailableWarning = false;                 }
            }
            else
            {
                button.BackColor = (_selectedSeat != null && _selectedSeat.SeatNumber == seatNumber) ? Color.Blue : Color.Green;
                button.Enabled = true;
            }
            button.ForeColor = Color.White;
        }

        private void SeatButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string seatNumber = button.Tag.ToString();
            Debug.WriteLine($"[CheckInForm] SeatButton_Click for seat {seatNumber}.");

            if (_selectedBooking == null)
            {
                MessageBox.Show("Please search for and select a booking first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (seatData.SeatId == 0 && seatData.Id == 0)
            {
                MessageBox.Show("Selected seat does not have a valid Seat ID. Cannot proceed with check-in. Please reload seats.", "Seat Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

                        string previousSeatNumber = _selectedSeat?.SeatNumber;
            if (!string.IsNullOrEmpty(previousSeatNumber) && _seatButtons.ContainsKey(previousSeatNumber))
            {
                var previousButton = _seatButtons[previousSeatNumber];
                                var prevSeatData = _seats.FirstOrDefault(s => s.SeatNumber == previousSeatNumber && s.FlightId == _selectedBooking.FlightId);
                previousButton.BackColor = (prevSeatData != null && prevSeatData.IsBooked) ? Color.Red : Color.Green;
                previousButton.Enabled = (prevSeatData != null && !prevSeatData.IsBooked);
            }

                        _selectedSeat = seatData;
            lblSelectedSeat.Text = $"Selected Seat: {_selectedSeat.SeatNumber}";
            button.BackColor = Color.Blue;
            button.Enabled = true;
            btnCheckIn.Enabled = true;
            Debug.WriteLine($"[CheckInForm] Seat {_selectedSeat.SeatNumber} selected.");
        }

        private async void btnCheckIn_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[CheckInForm] btnCheckIn_Click triggered.");
            if (_selectedBooking == null || _selectedSeat == null)
            {
                MessageBox.Show("Please select a booking and a seat.", "Check-In Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Debug.WriteLine($"[CheckInForm] Attempting server check-in for booking {_selectedBooking.BookingId}, seat {_selectedSeat.SeatNumber}.");
                var response = await _apiService.CheckInAsync(_selectedBooking.BookingId, _selectedSeat.SeatId != 0 ? _selectedSeat.SeatId : _selectedSeat.Id);
                if (response != null && response.Success)
                {
                                        _selectedBooking.IsCheckedIn = true;
                    _selectedBooking.SeatNumber = _selectedSeat.SeatNumber;
                    _selectedBooking.SeatId = _selectedSeat.SeatId != 0 ? _selectedSeat.SeatId : _selectedSeat.Id;

                    var actualSeatInList = _seats?.FirstOrDefault(s => s.FlightId == _selectedBooking.FlightId && s.SeatNumber == _selectedSeat.SeatNumber);
                    if (actualSeatInList != null) actualSeatInList.IsBooked = true;

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
                    MessageBox.Show(response?.Message ?? "Check-in failed. The seat might have been taken or an error occurred.", "Check-In Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (_selectedBooking != null) await LoadSeatsForFlightAsync(_selectedBooking.FlightId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error during check-in: {ex.Message}");
                MessageBox.Show($"Error during check-in: {ex.Message}", "Check-In Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (_selectedBooking != null) await LoadSeatsForFlightAsync(_selectedBooking.FlightId);
            }
        }

        private async void btnCancel_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[CheckInForm] btnCancel_Click triggered.");
            ResetForm();
            Debug.WriteLine("[CheckInForm] Form reset and cancel operation completed.");
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
                SeatNumber = seat?.SeatNumber ?? booking.SeatNumber ?? "TBD",
                BoardingTime = booking.Flight.DepartureTime.AddMinutes(-45)             };
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
                Debug.WriteLine($"[CheckInForm] Showing boarding pass dialog for {boardingPass.PassengerName}");
                using (var dialog = new BoardingPassDialog(boardingPass))
                {
                    DialogResult result = dialog.ShowDialog(this);
                    
                    if (dialog.StartNewCheckIn)
                    {
                                                Debug.WriteLine("[CheckInForm] User requested new check-in from boarding pass dialog");
                                                txtPassportNumber.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error showing boarding pass dialog: {ex.Message}");
                MessageBox.Show($"Error displaying boarding pass: {ex.Message}", 
                    "Display Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

                private void DisplayBoardingPass(BoardingPass boardingPass = null)
        {
            Debug.WriteLine("[CheckInForm] DisplayBoardingPass called - redirecting to dialog.");
            
                        if (boardingPass != null)
            {
                ShowBoardingPassDialog(boardingPass);
                return;
            }
            
                        if (_selectedBooking != null)
            {
                var generatedBoardingPass = CreateBoardingPassFromBooking(_selectedBooking, _selectedSeat);
                ShowBoardingPassDialog(generatedBoardingPass);
            }
            else
            {
                MessageBox.Show("Cannot display boarding pass. No booking information available.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

                private void btnPrintBoardingPass_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[CheckInForm] btnPrintBoardingPass_Click triggered.");
            
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
                Debug.WriteLine($"[CheckInForm] Error printing boarding pass: {ex.Message}");
                MessageBox.Show($"Error printing boarding pass: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

                protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
                        if (keyData == Keys.F1)
            {
                txtPassportNumber.Focus();
                txtPassportNumber.SelectAll();
                return true;
            }
                        else if (keyData == Keys.F2 && btnSearch.Enabled)
            {
                btnSearch_Click(btnSearch, EventArgs.Empty);
                return true;
            }
                        else if (keyData == Keys.F3 && btnCheckIn.Enabled)
            {
                btnCheckIn_Click(btnCheckIn, EventArgs.Empty);
                return true;
            }
                        else if (keyData == Keys.Escape)
            {
                btnCancel_Click(btnCancel, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

                private void HandleApiError(string operation, Exception ex)
        {
            Debug.WriteLine($"[CheckInForm] Error in {operation}: {ex.Message}");
            
            string userMessage = ex.Message;
            if (ex is HttpRequestException)
            {
                userMessage = "Connection error. Please check your network connection and try again.";
            }
            else if (ex is TaskCanceledException)
            {
                userMessage = "Request timed out. The server might be busy. Please try again.";
            }
            
            MessageBox.Show($"Error during {operation}: {userMessage}", 
                "Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

                public async Task RefreshSeatDisplayAsync()
        {
            if (_selectedBooking != null && _selectedBooking.FlightId > 0)
            {
                Debug.WriteLine($"[CheckInForm] Refreshing seat display for flight {_selectedBooking.FlightId}");
                await LoadSeatsForFlightAsync(_selectedBooking.FlightId);
            }
        }

                private void OnSeatStatusChanged(int flightId, string seatNumber, bool isBooked)
        {
            if (_selectedBooking?.FlightId == flightId && _seatButtons.ContainsKey(seatNumber))
            {
                Debug.WriteLine($"[CheckInForm] SignalR update: Seat {seatNumber} booking status changed to {isBooked}");
                
                                var seat = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber && s.FlightId == flightId);
                if (seat != null)
                {
                    seat.IsBooked = isBooked;
                }
                
                                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => UpdateSeatButton(_seatButtons[seatNumber], seatNumber, isBooked)));
                }
                else
                {
                    UpdateSeatButton(_seatButtons[seatNumber], seatNumber, isBooked);
                }
            }
        }

                private bool IsValidPassportNumber(string passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
                return false;

                        passportNumber = passportNumber.Trim().ToUpper();

                                    if (passportNumber.Length < 6 || passportNumber.Length > 12)
                return false;

                        return passportNumber.All(c => char.IsLetterOrDigit(c));
        }

                private string FormatPassportNumber(string passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
                return string.Empty;

            return passportNumber.Trim().ToUpper();
        }

                private bool CanCloseForm()
        {
                        if (_selectedBooking != null && !_selectedBooking.IsCheckedIn && _selectedSeat != null)
            {
                var result = MessageBox.Show(
                    "There is an ongoing check-in process. Are you sure you want to close the form?",
                    "Confirm Close",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                return result == DialogResult.Yes;
            }

            return true;
        }

                private void CheckInForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Debug.WriteLine("[CheckInForm] FormClosing event triggered.");
            
                        if (!CanCloseForm())
            {
                e.Cancel = true;
                return;
            }

            try
            {
                                _boardingPassPrinter?.Dispose();
                
                                if (_seatStatusSignalRService != null)
                {
                                        Debug.WriteLine("[CheckInForm] Cleaning up SignalR connections");
                }
                
                                if (_apiService is IDisposable disposableApiService)
                {
                    disposableApiService.Dispose();
                }
                
                Debug.WriteLine("[CheckInForm] Form cleanup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error during form cleanup: {ex.Message}");
            }
        }

                protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
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
            }
            
            base.Dispose(disposing);
        }

                public async Task SearchBookingAsync(string passportNumber)
        {
            if (!string.IsNullOrWhiteSpace(passportNumber))
            {
                txtPassportNumber.Text = passportNumber;
                await Task.Run(() => btnSearch_Click(btnSearch, EventArgs.Empty));
            }
        }

                public string GetCurrentBookingStatus()
        {
            if (_selectedBooking == null)
                return "No booking selected";

            if (_selectedBooking.IsCheckedIn)
                return $"Checked in - Seat: {_selectedBooking.SeatNumber}";

            if (_selectedSeat != null)
                return $"Seat selected: {_selectedSeat.SeatNumber} - Ready for check-in";

            return "Booking found - Seat selection required";
        }

                public void ClearAllData()
        {
            Debug.WriteLine("[CheckInForm] ClearAllData called - performing complete reset");
            
            _flights?.Clear();
            _passengers?.Clear();
            _bookings?.Clear();
            _seats?.Clear();
            
            ResetForm();
            
            Debug.WriteLine("[CheckInForm] All data cleared successfully");
        }

                private void SetFormControlsEnabled(bool enabled)
        {
            txtPassportNumber.Enabled = enabled;
            btnSearch.Enabled = enabled;
            btnCancel.Enabled = enabled;
            
                        if (!enabled)
            {
                btnCheckIn.Enabled = false;
            }

                        pnlSeats.Enabled = enabled;
        }

                private void ShowLoadingState(string message = "Processing...")
        {
            SetFormControlsEnabled(false);
            this.Cursor = Cursors.WaitCursor;
                        Debug.WriteLine($"[CheckInForm] Loading state: {message}");
        }

                private void HideLoadingState()
        {
            SetFormControlsEnabled(true);
            this.Cursor = Cursors.Default;
            Debug.WriteLine("[CheckInForm] Loading state cleared");
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

                private void UpdateUIState()
        {
                        btnSearch.Enabled = !string.IsNullOrWhiteSpace(txtPassportNumber.Text);
            btnCheckIn.Enabled = _selectedBooking != null && _selectedSeat != null && !_selectedBooking.IsCheckedIn;
            
                        grpBookingDetails.Visible = _selectedBooking != null;
            grpSeatSelection.Visible = _selectedBooking != null && !_selectedBooking.IsCheckedIn;
            grpBoardingPass.Visible = _selectedBooking != null && _selectedBooking.IsCheckedIn;
            
            Debug.WriteLine($"[CheckInForm] UI state updated - Booking: {_selectedBooking != null}, Seat: {_selectedSeat != null}");
        }
    }
}