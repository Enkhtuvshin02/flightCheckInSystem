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
using System.Diagnostics; // Added for Debug.WriteLine

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
        private List<Seat> _seats; // This will hold seats for the *currently selected* flight

        private Booking _selectedBooking;
        private Seat _selectedSeat;
        private Dictionary<string, Button> _seatButtons;

        // Flag to suppress seat unavailable warning after successful check-in by current user
        private bool _suppressSeatUnavailableWarning = false;

        public CheckInForm()
        {
            InitializeComponent();
            _apiService = new ApiService();
            // TODO: Replace with your actual SignalR hub URL if different
            _seatStatusSignalRService = new SeatStatusSignalRService("https://localhost:5001/seathub");
            _boardingPassPrinter = new BoardingPassPrinter();
            // Initialize _seatButtons here to prevent NullReferenceException before any seats are loaded
            _seatButtons = new Dictionary<string, Button>();
            // Initial data load for flights and bookings can be triggered by Load event.
            // DO NOT CALL InitializeSeatPanel() here, as _seats is not yet populated with flight-specific data.
        }

        private async void CheckInForm_Load(object sender, EventArgs e)
        {
            Debug.WriteLine("[CheckInForm] Form_Load event triggered.");
            await LoadDataAsync();
            ResetForm(); // Call ResetForm to set initial UI state correctly
        }

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

                // Passengers are typically loaded with bookings or on demand.
                // For simplicity, we'll keep _passengers as a placeholder.
                _passengers = new List<Passenger>();
                _seats = new List<Seat>(); // Initialize _seats as empty initially; it will be populated per-flight.

                if (_flights.Any())
                {
                    Debug.WriteLine($"[CheckInForm] Loaded {_flights.Count} flights from server.");
                    // No call to InitializeSeatPanel() here. It will be called when a flight is selected.
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

            // Group seats by row letter for layout
            // Order by the character of the seat number (e.g., '1' from "1A", '2' from "2A")
            // Assuming SeatNumber format "RowNumberLetter" (e.g., "1A", "1B", "2A", "2B")
            // We want to group by the row number and then order by the seat letter.
            // However, your data has "1A", "1B", "2A", "2B"
            // Let's group by the numeric part first for rows, then by the letter.
            // The previous grouping by s.SeatNumber[0] was for 'A' from 'A1' format.
            // Given your API response: "1A", "1B", "2A", "2B", it looks like Row is numeric and seat is alpha.

            // Let's refine the grouping and ordering based on your provided data format ("1A", "1B", "2A", "2B"):
            // Group by the numeric part (the row number)
            var groupedByRowNumber = _seats.GroupBy(s => {
                // Extract the numeric part of the seat number (e.g., "1" from "1A")
                string numericPart = new string(s.SeatNumber.TakeWhile(char.IsDigit).ToArray());
                if (int.TryParse(numericPart, out int rowNumber))
                {
                    return rowNumber;
                }
                return 0; // Fallback for malformed seat numbers
            })
                                            .OrderBy(g => g.Key); // Order rows by row number

            foreach (var rowGroup in groupedByRowNumber)
            {
                int col = 0;
                // Order seats within each row by their seatId (which is a reliable integer)
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
            // Clear seat buttons when resetting the form (no flight selected)
            pnlSeats.Controls.Clear();
            _seatButtons.Clear();
            txtPassportNumber.Focus(); // Focus on passport field for next passenger
        }

        private void ResetFormAfterBoardingPass()
        {
            Debug.WriteLine("[CheckInForm] ResetFormAfterBoardingPass called.");
            
            // Clear all form data
            txtPassportNumber.Clear();
            grpBookingDetails.Visible = false;
            grpSeatSelection.Visible = false;
            grpBoardingPass.Visible = false;
            btnCheckIn.Visible = false;
            
            // Clear selections
            _selectedBooking = null;
            _selectedSeat = null;
            lblSelectedSeat.Text = "Selected Seat: (None)";
            
            // Clear seat buttons
            pnlSeats.Controls.Clear();
            _seatButtons.Clear();
            
            // Suppress seat unavailable warning since we're resetting
            _suppressSeatUnavailableWarning = false;
            
            // Focus on passport number field for next passenger
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
                    // If any booking is already checked in, show boarding pass immediately
                    var alreadyCheckedIn = bookingsFromServer.FirstOrDefault(b => b.IsCheckedIn);
                    if (alreadyCheckedIn != null)
                    {
                        _selectedBooking = alreadyCheckedIn;
                        // Try to resolve the seat for the checked-in booking
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
                        
                        // Create boarding pass and show dialog
                        var boardingPass = CreateBoardingPassFromBooking(_selectedBooking, _selectedSeat);
                        ShowBoardingPassDialog(boardingPass);
                        
                        // Reset form for next passenger
                        ResetFormAfterBoardingPass();
                        return;
                    }

                    // Otherwise, proceed with normal check-in flow for not-checked-in bookings
                    var activeBookings = bookingsFromServer.Where(b => !b.IsCheckedIn).ToList();
                    if (!activeBookings.Any())
                    {
                        MessageBox.Show("No active (not checked-in) bookings found for this passenger on the server.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetForm();
                        return;
                    }
                    _selectedBooking = activeBookings.First(); // Assuming one active booking for simplicity
                    Debug.WriteLine($"[CheckInForm] Found booking {_selectedBooking.BookingReference} on server.");
                    DisplayBookingDetails(_selectedBooking);

                    // AFTER booking details are displayed and _selectedBooking is set,
                    // load seats for this specific flight and initialize/update the panel.
                    await LoadSeatsForFlightAsync(_selectedBooking.FlightId);

                    // Connect and subscribe to SignalR for this flight
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
                    return; // Exit after successful server search
                }
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
                btnCheckIn.Enabled = false; // Enabled only after a seat is selected
            }
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
                    _seats = seatsFromServer; // Update local cache of seats for this flight
                    Debug.WriteLine($"[CheckInForm] Loaded {seatsFromServer.Count} seats from server for flight {flightId}.");
                    InitializeSeatPanel(_seats); // Call InitializeSeatPanel with the loaded seats
                    // Now that buttons are created, update their state
                    UpdateSeatDisplay(flightId); // This method should just update button colors/enabled states
                    return;
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
            // InitializeSeatPanel should have already been called with the correct seats,
            // so we should have a populated _seatButtons dictionary.
            if (_seatButtons == null || !_seatButtons.Any())
            {
                Debug.WriteLine("[CheckInForm] _seatButtons is empty or null, cannot update seat display.");
                return;
            }

            // Ensure _seats contains the relevant data for the current flight.
            // This is crucial if _seats is meant to hold *all* seats across flights
            // fetched initially, or if it's updated with the current flight's seats.
            // Given LoadSeatsForFlightAsync populates _seats with current flight's data,
            // we can directly use _seats here.
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
                    // Only show the warning if not just checked in by the current user
                    if (!_suppressSeatUnavailableWarning)
                    {
                        MessageBox.Show($"Seat {seatNumber} has just been booked by another passenger. Please select a different seat.", "Seat Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    _suppressSeatUnavailableWarning = false; // Always reset after use
                }
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

            // Deselect previous seat (UI update)
            string previousSeatNumber = _selectedSeat?.SeatNumber;
            if (!string.IsNullOrEmpty(previousSeatNumber) && _seatButtons.ContainsKey(previousSeatNumber))
            {
                var previousButton = _seatButtons[previousSeatNumber];
                // Restore its original color (green if not booked, red if booked by someone else)
                var prevSeatData = _seats.FirstOrDefault(s => s.SeatNumber == previousSeatNumber && s.FlightId == _selectedBooking.FlightId);
                previousButton.BackColor = (prevSeatData != null && prevSeatData.IsBooked) ? Color.Red : Color.Green;
                previousButton.Enabled = (prevSeatData != null && !prevSeatData.IsBooked);
            }

            // Select new seat
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
                    // Update local data
                    _selectedBooking.IsCheckedIn = true;
                    _selectedBooking.SeatNumber = _selectedSeat.SeatNumber;
                    _selectedBooking.SeatId = _selectedSeat.SeatId != 0 ? _selectedSeat.SeatId : _selectedSeat.Id;

                    var actualSeatInList = _seats?.FirstOrDefault(s => s.FlightId == _selectedBooking.FlightId && s.SeatNumber == _selectedSeat.SeatNumber);
                    if (actualSeatInList != null) actualSeatInList.IsBooked = true;

                    // Create boarding pass
                    BoardingPass boardingPass;
                    if (response.BoardingPass != null)
                    {
                        boardingPass = response.BoardingPass;
                    }
                    else
                    {
                        boardingPass = CreateBoardingPassFromBooking(_selectedBooking, _selectedSeat);
                    }

                    // Show success message
                    MessageBox.Show(response.Message ?? "Check-in successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Suppress seat unavailable warning for this seat update
                    _suppressSeatUnavailableWarning = true;
                    if (_seatButtons.ContainsKey(_selectedSeat.SeatNumber))
                    {
                        UpdateSeatButton(_seatButtons[_selectedSeat.SeatNumber], _selectedSeat.SeatNumber, true);
                    }

                    // Show boarding pass dialog
                    ShowBoardingPassDialog(boardingPass);

                    // Reset form for next passenger
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

        // Helper method to create boarding pass from booking data
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
                BoardingTime = booking.Flight.DepartureTime.AddMinutes(-45) // 45 minutes before departure
            };
        }

        // Method to show boarding pass dialog
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
                        // User clicked "New Check-In" button
                        Debug.WriteLine("[CheckInForm] User requested new check-in from boarding pass dialog");
                        // Form is already reset, just focus on passport field
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

        // Updated DisplayBoardingPass method (legacy support)
        private void DisplayBoardingPass(BoardingPass boardingPass = null)
        {
            Debug.WriteLine("[CheckInForm] DisplayBoardingPass called - redirecting to dialog.");
            
            // If called with a boarding pass from API, show it
            if (boardingPass != null)
            {
                ShowBoardingPassDialog(boardingPass);
                return;
            }
            
            // Legacy fallback - create boarding pass from current booking
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

        // Updated btnPrintBoardingPass_Click method:
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

        // Event handlers for keyboard shortcuts (optional)
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // F1 - Focus on passport number
            if (keyData == Keys.F1)
            {
                txtPassportNumber.Focus();
                txtPassportNumber.SelectAll();
                return true;
            }
            // F2 - Search booking
            else if (keyData == Keys.F2 && btnSearch.Enabled)
            {
                btnSearch_Click(btnSearch, EventArgs.Empty);
                return true;
            }
            // F3 - Check-in (if enabled)
            else if (keyData == Keys.F3 && btnCheckIn.Enabled)
            {
                btnCheckIn_Click(btnCheckIn, EventArgs.Empty);
                return true;
            }
            // Escape - Cancel/Reset
            else if (keyData == Keys.Escape)
            {
                btnCancel_Click(btnCancel, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Additional helper method for error handling
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

        // Method to refresh seat display (can be called from SignalR updates)
        public async Task RefreshSeatDisplayAsync()
        {
            if (_selectedBooking != null && _selectedBooking.FlightId > 0)
            {
                Debug.WriteLine($"[CheckInForm] Refreshing seat display for flight {_selectedBooking.FlightId}");
                await LoadSeatsForFlightAsync(_selectedBooking.FlightId);
            }
        }

        // Method to handle SignalR seat updates (if implemented)
        private void OnSeatStatusChanged(int flightId, string seatNumber, bool isBooked)
        {
            if (_selectedBooking?.FlightId == flightId && _seatButtons.ContainsKey(seatNumber))
            {
                Debug.WriteLine($"[CheckInForm] SignalR update: Seat {seatNumber} booking status changed to {isBooked}");
                
                // Update the seat in our local cache
                var seat = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber && s.FlightId == flightId);
                if (seat != null)
                {
                    seat.IsBooked = isBooked;
                }
                
                // Update the UI
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

        // Method to validate passport number format (optional enhancement)
        private bool IsValidPassportNumber(string passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
                return false;

            // Remove whitespace and convert to uppercase
            passportNumber = passportNumber.Trim().ToUpper();

            // Basic validation - adjust according to your requirements
            // This is a simple example - real passport validation would be more complex
            if (passportNumber.Length < 6 || passportNumber.Length > 12)
                return false;

            // Check for alphanumeric characters only
            return passportNumber.All(c => char.IsLetterOrDigit(c));
        }

        // Method to format passport number consistently
        private string FormatPassportNumber(string passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
                return string.Empty;

            return passportNumber.Trim().ToUpper();
        }

        // Method to check if form can be closed safely
        private bool CanCloseForm()
        {
            // Check if there's an ongoing check-in process
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

        // Method to handle form closing with validation
        private void CheckInForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Debug.WriteLine("[CheckInForm] FormClosing event triggered.");
            
            // Check if it's safe to close
            if (!CanCloseForm())
            {
                e.Cancel = true;
                return;
            }

            try
            {
                // Dispose of the boarding pass printer
                _boardingPassPrinter?.Dispose();
                
                // Disconnect SignalR if connected
                if (_seatStatusSignalRService != null)
                {
                    // Add any cleanup for SignalR service if needed
                    Debug.WriteLine("[CheckInForm] Cleaning up SignalR connections");
                }
                
                // Dispose of API service if it implements IDisposable
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

        // Override Dispose to ensure proper cleanup
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _boardingPassPrinter?.Dispose();
                    
                    // Dispose of seat buttons dictionary
                    if (_seatButtons != null)
                    {
                        _seatButtons.Clear();
                        _seatButtons = null;
                    }
                    
                    // Clear collections
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

        // Public method to programmatically trigger a search (useful for testing or integration)
        public async Task SearchBookingAsync(string passportNumber)
        {
            if (!string.IsNullOrWhiteSpace(passportNumber))
            {
                txtPassportNumber.Text = passportNumber;
                await Task.Run(() => btnSearch_Click(btnSearch, EventArgs.Empty));
            }
        }

        // Public method to get current booking status (useful for monitoring)
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

        // Public method to clear all data (useful for reset scenarios)
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

        // Method to enable/disable form controls during operations
        private void SetFormControlsEnabled(bool enabled)
        {
            txtPassportNumber.Enabled = enabled;
            btnSearch.Enabled = enabled;
            btnCancel.Enabled = enabled;
            
            // Don't disable check-in button if it's supposed to be enabled
            if (!enabled)
            {
                btnCheckIn.Enabled = false;
            }

            // Disable seat selection panel during operations
            pnlSeats.Enabled = enabled;
        }

        // Method to show loading state
        private void ShowLoadingState(string message = "Processing...")
        {
            SetFormControlsEnabled(false);
            this.Cursor = Cursors.WaitCursor;
            // You could add a status label here if your form has one
            Debug.WriteLine($"[CheckInForm] Loading state: {message}");
        }

        // Method to hide loading state
        private void HideLoadingState()
        {
            SetFormControlsEnabled(true);
            this.Cursor = Cursors.Default;
            Debug.WriteLine("[CheckInForm] Loading state cleared");
        }

        // Event handler for Enter key in passport number textbox
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

        // Method to update UI based on current state
        private void UpdateUIState()
        {
            // Update button states based on current selections
            btnSearch.Enabled = !string.IsNullOrWhiteSpace(txtPassportNumber.Text);
            btnCheckIn.Enabled = _selectedBooking != null && _selectedSeat != null && !_selectedBooking.IsCheckedIn;
            
            // Update group box visibility
            grpBookingDetails.Visible = _selectedBooking != null;
            grpSeatSelection.Visible = _selectedBooking != null && !_selectedBooking.IsCheckedIn;
            grpBoardingPass.Visible = _selectedBooking != null && _selectedBooking.IsCheckedIn;
            
            Debug.WriteLine($"[CheckInForm] UI state updated - Booking: {_selectedBooking != null}, Seat: {_selectedSeat != null}");
        }
    }
}