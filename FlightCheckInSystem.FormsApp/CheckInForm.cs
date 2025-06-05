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
using System.Net.Http; // Added for HttpRequestException

namespace FlightCheckInSystem.FormsApp
{
    public partial class CheckInForm : Form
    {
        private readonly ApiService _apiService;
        private readonly SeatStatusSignalRService _seatStatusSignalRService;
        private readonly BoardingPassPrinter _boardingPassPrinter; // Keep the original printer
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
        private string _currentFlightNumberSubscription; // To track current flight subscription

        public CheckInForm(ApiService apiService, SeatStatusSignalRService seatStatusSignalRService)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _seatStatusSignalRService = seatStatusSignalRService ?? throw new ArgumentNullException(nameof(seatStatusSignalRService));
            _boardingPassPrinter = new BoardingPassPrinter(); // Use the actual printer
            _seatButtons = new Dictionary<string, Button>();

            SetupSignalREvents();
        }

        private void SetupSignalREvents()
        {
            _seatStatusSignalRService.SeatBooked += OnSeatBooked;
            _seatStatusSignalRService.SeatReleased += OnSeatReleased;
            _seatStatusSignalRService.FlightSeatsReceived += OnFlightSeatsReceived;
            _seatStatusSignalRService.SeatReserved += OnSeatReserved;
            _seatStatusSignalRService.SeatReservationReleased += OnSeatReservationReleased;
            _seatStatusSignalRService.SeatReservationFailed += OnSeatReservationFailed;
            _seatStatusSignalRService.RefreshSeatsForFlight += OnRefreshSeatsForFlight; // New event handler
        }

        private async void OnRefreshSeatsForFlight(string flightNumber)
        {
            if (_selectedBooking?.Flight?.FlightNumber == flightNumber)
            {
                Debug.WriteLine($"[CheckInForm] Received RefreshSeatsForFlight for {flightNumber}. Reloading seats.");
                // Suppress warning during programmatic refresh to avoid spamming user
                bool originalSuppress = _suppressSeatUnavailableWarning;
                _suppressSeatUnavailableWarning = true;
                await LoadSeatsForFlightAsync(_selectedBooking.FlightId);
                _suppressSeatUnavailableWarning = originalSuppress;
            }
        }

        private void OnSeatReserved(string flightNumber, string seatNumber, string bookingReference)
        {
            if (_selectedBooking?.Flight?.FlightNumber != flightNumber)
                return;

            this.Invoke(() =>
            {
                var seat = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat != null && _seatButtons.ContainsKey(seatNumber))
                {
                    var button = _seatButtons[seatNumber];

                    if (bookingReference == _myBookingReference) // My reservation
                    {
                        Debug.WriteLine($"[CheckInForm] Seat {seatNumber} reserved by THIS CLIENT.");
                        _selectedSeat = seat; // Confirm our selection
                        lblSelectedSeat.Text = $"Сонгосон суудал: {_selectedSeat.SeatNumber}";
                        btnCheckIn.Enabled = true;
                        button.BackColor = Color.Blue; // My selected seat
                        button.Enabled = true;
                    }
                    else // Someone else's reservation
                    {
                        Debug.WriteLine($"[CheckInForm] Seat {seatNumber} reserved by ANOTHER CLIENT ({bookingReference}).");
                        button.BackColor = Color.Orange; // Temporarily unavailable
                        button.Enabled = false;

                        // If this was our selected seat, clear it and notify
                        if (_selectedSeat?.SeatNumber == seatNumber)
                        {
                            _selectedSeat = null;
                            lblSelectedSeat.Text = "Сонгосон суудал: (Өөр хэрэглэгч захиалсан)";
                            btnCheckIn.Enabled = false;

                            if (!_suppressSeatUnavailableWarning)
                            {
                                MessageBox.Show($"Суудал {seatNumber} өөр зорчигчоор захиалагдсан байна. Өөр суудал сонгоно уу.",
                                    "Суудал захиалагдсан", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    Debug.WriteLine($"[CheckInForm] Seat reservation released for {seatNumber}. It's now available.");

                    // If this was the seat we were trying to reserve, it's now available again
                    // but we clear our internal selection if it wasn't confirmed.
                    if (_selectedSeat?.SeatNumber == seatNumber)
                    {
                        _selectedSeat = null;
                        lblSelectedSeat.Text = "Сонгосон суудал: (Үгүй)";
                        btnCheckIn.Enabled = false;
                    }
                }
            });
        }

        private void OnSeatReservationFailed(string flightNumber, string seatNumber, string reason)
        {
            if (_selectedBooking?.Flight?.FlightNumber != flightNumber)
                return;

            this.Invoke(() =>
            {
                Debug.WriteLine($"[CheckInForm] Seat reservation FAILED for {seatNumber}: {reason}");
                MessageBox.Show($"Суудал {seatNumber} захиалж чадсангүй: {reason}", "Захиалга амжилтгүй боллоо",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Clear our selection since reservation failed
                if (_selectedSeat?.SeatNumber == seatNumber)
                {
                    _selectedSeat = null;
                    lblSelectedSeat.Text = "Сонгосон суудал: (Үгүй)";
                    btnCheckIn.Enabled = false;
                }

                // Restore the button to its original state (available if not booked)
                if (_seatButtons.ContainsKey(seatNumber))
                {
                    var button = _seatButtons[seatNumber];
                    var seatInList = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber);
                    if (seatInList != null && !seatInList.IsBooked)
                    {
                        button.BackColor = Color.Green;
                        button.Enabled = true;
                    }
                }
            });
        }

        private void OnSeatBooked(string flightNumber, string seatNumber, string bookingReference)
        {
            if (_selectedBooking?.Flight?.FlightNumber != flightNumber)
                return;

            this.Invoke(() =>
            {
                Debug.WriteLine($"[CheckInForm] Seat {seatNumber} BOOKED for flight {flightNumber} by {bookingReference}");
                var seat = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat != null)
                {
                    seat.IsBooked = true; // Update local data
                    if (_seatButtons.ContainsKey(seatNumber))
                    {
                        var button = _seatButtons[seatNumber];
                        button.BackColor = Color.Red;
                        button.Enabled = false;
                    }
                }

                // If this seat was selected by us but another booking confirmed it, clear our selection
                if (_selectedSeat?.SeatNumber == seatNumber && bookingReference != _myBookingReference)
                {
                    _selectedSeat = null;
                    lblSelectedSeat.Text = "Сонгосон суудал: (Өөр хэрэглэгч захиалсан)";
                    btnCheckIn.Enabled = false;
                    Debug.WriteLine($"[CheckInForm] Our selected seat {seatNumber} was booked by another client, clearing selection.");
                }
            });
        }

        private void OnSeatReleased(string flightNumber, string seatNumber)
        {
            if (_selectedBooking?.Flight?.FlightNumber != flightNumber)
                return;

            this.Invoke(() =>
            {
                Debug.WriteLine($"[CheckInForm] Seat {seatNumber} RELEASED for flight {flightNumber}");
                var seat = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber);
                if (seat != null)
                {
                    seat.IsBooked = false; // Update local data
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
            {
                Debug.WriteLine($"[CheckInForm] Received seat data for {flightNumber}, but current booking is for {_selectedBooking?.Flight?.FlightNumber}. Ignoring.");
                return;
            }

            this.Invoke(() =>
            {
                try
                {
                    var receivedSeats = JsonConvert.DeserializeObject<List<Seat>>(seatDataJson) ?? new List<Seat>();
                    Debug.WriteLine($"[CheckInForm] Deserialized {receivedSeats.Count} seats for flight {flightNumber} from SignalR.");
                    _seats = receivedSeats;
                    InitializeSeatPanel(_seats);
                    UpdateSeatDisplay(_selectedBooking.FlightId); // Update the display based on new data
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Суудлын мэдээлэл боловсруулахад алдаа гарлаа: {ex.Message}", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"[CheckInForm] Error parsing seat data from SignalR: {ex.Message}");
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
                _passengers = new List<Passenger>(); // Passengers are usually loaded with bookings or flights
                _seats = new List<Seat>();

                if (!_flights.Any())
                {
                    MessageBox.Show("Серверээс нислэг ачааллагдсангүй. Сервер/API-гаа шалгана уу.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Эхний мэдээлэл ачаалахад алдаа гарлаа: {ex.Message}", "Холболтын алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"[CheckInForm] Error loading initial data: {ex.Message}");
            }
        }

        private void InitializeSeatPanel(List<Seat> flightSeats)
        {
            pnlSeats.Controls.Clear();
            _seatButtons = new Dictionary<string, Button>();

            if (flightSeats == null || !flightSeats.Any())
            {
                Debug.WriteLine("[CheckInForm] No seats to initialize panel with.");
                return;
            }

            int buttonSize = 40, spacing = 10;
            int x = 20, y = 20;

            var groupedByRowNumber = flightSeats.GroupBy(s => {
                string numericPart = new string(s.SeatNumber.TakeWhile(char.IsDigit).ToArray());
                if (int.TryParse(numericPart, out int rowNumber))
                {
                    return rowNumber;
                }
                return 0; // Should not happen with valid seat numbers
            })
            .OrderBy(g => g.Key);

            foreach (var rowGroup in groupedByRowNumber)
            {
                int col = 0;
                foreach (var seat in rowGroup.OrderBy(s => s.SeatNumber)) // Order by full seat number (e.g., 10A, 10B, 10C)
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

                    // Add aisle space after C seats (adjust based on your seat layout logic)
                    if (seat.SeatNumber.EndsWith("C"))
                        col++;
                }
                y += buttonSize + spacing;
            }
            Debug.WriteLine($"[CheckInForm] Initialized seat panel with {flightSeats.Count} seats.");
        }

        private async Task LoadSeatsForFlightAsync(int flightId)
        {
            try
            {
                var flight = _flights?.FirstOrDefault(f => f.FlightId == flightId);
                if (flight == null)
                {
                    Debug.WriteLine($"[CheckInForm] Flight with ID {flightId} not found, cannot load seats.");
                    return;
                }

                // Unsubscribe from previous flight updates if any
                if (!string.IsNullOrEmpty(_currentFlightNumberSubscription) && _currentFlightNumberSubscription != flight.FlightNumber)
                {
                    await _seatStatusSignalRService.UnsubscribeFromFlightAsync(_currentFlightNumberSubscription);
                    Debug.WriteLine($"[CheckInForm] Unsubscribed from previous flight updates for {_currentFlightNumberSubscription}");
                }
                _currentFlightNumberSubscription = flight.FlightNumber;

                // Subscribe to real-time seat updates for this flight
                await _seatStatusSignalRService.SubscribeToFlightAsync(flight.FlightNumber);
                Debug.WriteLine($"[CheckInForm] Subscribed to flight updates for {flight.FlightNumber}");

                // Request current seat state via SignalR (this will trigger OnFlightSeatsReceived)
                await _seatStatusSignalRService.GetFlightSeatsAsync(flightId);
                Debug.WriteLine($"[CheckInForm] Requested seat data for flight {flightId} via SignalR.");

                // As a fallback or initial load, get seats from API as well
                var seatsFromServer = await _apiService.GetSeatsByFlightAsync(flightId);
                if (seatsFromServer != null)
                {
                    // Merge or replace with API data if necessary, SignalR usually provides more up-to-date state
                    _seats = seatsFromServer;
                    InitializeSeatPanel(_seats);
                    UpdateSeatDisplay(flightId);
                    Debug.WriteLine($"[CheckInForm] Loaded {seatsFromServer.Count} seats from API for flight {flightId}.");
                }
                else
                {
                    MessageBox.Show($"Серверээс нислэгийн {flightId} суудлын мэдээлэл ирсэнгүй.", "Суудал ачаалахад алдаа гарлаа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Debug.WriteLine($"[CheckInForm] No seats returned from API for flight {flightId}.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Серверээс суудал ачаалахад алдаа гарлаа: {ex.Message}.", "Суудал ачаалахад алдаа гарлаа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Debug.WriteLine($"[CheckInForm] Error loading seats from server: {ex.Message}");
            }
        }

        private void ResetForm()
        {
            txtPassportNumber.Clear();
            grpBookingDetails.Visible = false;
            grpSeatSelection.Visible = false;
            grpBoardingPass.Visible = false;
            btnCheckIn.Visible = false;
            btnCheckIn.Enabled = false; // Disable check-in button initially
            btnPrintBoardingPass.Visible = false; // Hide print button initially

            _selectedBooking = null;
            _selectedSeat = null;
            _myBookingReference = null;
            _isCheckingIn = false;
            lblSelectedSeat.Text = "Сонгосон суудал: (Үгүй)";
            pnlSeats.Controls.Clear();
            _seatButtons.Clear();
            _suppressSeatUnavailableWarning = false;
            txtPassportNumber.Focus();
            Debug.WriteLine("[CheckInForm] Form reset.");
        }

        private void ResetFormAfterBoardingPass()
        {
            // First, unsubscribe from the current flight's updates if any
            if (_selectedBooking?.Flight != null && !string.IsNullOrEmpty(_currentFlightNumberSubscription))
            {
                try
                {
                    _seatStatusSignalRService.UnsubscribeFromFlightAsync(_currentFlightNumberSubscription).Wait(); // Wait for unsubscribe
                    _currentFlightNumberSubscription = null;
                    Debug.WriteLine($"[CheckInForm] Unsubscribed from flight updates for {_selectedBooking.Flight.FlightNumber} after boarding pass.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CheckInForm] Error unsubscribing during reset after boarding pass: {ex.Message}");
                }
            }
            ResetForm();
            Debug.WriteLine("[CheckInForm] Form reset after boarding pass display.");
        }

        private void DisplayBookingDetails(Booking booking)
        {
            if (booking == null)
            {
                lblPassengerInfo.Text = "Зорчигч: Мэдээлэл алга";
                lblFlightInfo.Text = "Нислэг: Мэдээлэл алга";
                grpBookingDetails.Visible = false;
                grpSeatSelection.Visible = false;
                btnCheckIn.Visible = false;
                btnCheckIn.Enabled = false;
                btnPrintBoardingPass.Visible = false;
                return;
            }

            Passenger passenger = booking.Passenger;
            // Ensure passenger and flight are loaded into the booking object
            if (passenger == null && booking.PassengerId > 0)
            {
                passenger = _passengers?.FirstOrDefault(p => p.PassengerId == booking.PassengerId)
                    ?? _apiService.GetPassengersAsync().Result.FirstOrDefault(p => p.PassengerId == booking.PassengerId); // Fallback to API call
                booking.Passenger = passenger;
            }

            Flight flight = booking.Flight;
            if (flight == null && booking.FlightId > 0)
            {
                flight = _flights?.FirstOrDefault(f => f.FlightId == booking.FlightId)
                    ?? _apiService.GetFlightsAsync().Result.FirstOrDefault(f => f.FlightId == booking.FlightId); // Fallback to API call
                booking.Flight = flight;
            }

            if (passenger == null || flight == null)
            {
                lblPassengerInfo.Text = passenger != null ? $"Зорчигч: {passenger.FirstName} {passenger.LastName} (Паспорт: {passenger.PassportNumber})" : "Зорчигч: Ачаалахад алдаа гарлаа";
                lblFlightInfo.Text = flight != null ? $"Нислэг: {flight.FlightNumber} ({flight.DepartureAirport} -с {flight.ArrivalAirport}) - Хөдөлгөх: {flight.DepartureTime:g}" : "Нислэг: Ачаалахад алдаа гарлаа";
                grpBookingDetails.Visible = true;
                MessageBox.Show("Захиалгын дэлгэрэнгүй мэдээллийг бүрэн татаж чадсангүй. Зарим мэдээлэл дутуу байж болно.", "Дутуу мэдээлэл", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                grpSeatSelection.Visible = false;
                btnCheckIn.Visible = false;
                btnCheckIn.Enabled = false;
                btnPrintBoardingPass.Visible = false;
                return;
            }

            lblBookingRef.Text = $"Захиалгын код: {booking.BookingId}";
            lblPassengerInfo.Text = $"Зорчигч: {passenger.FirstName} {passenger.LastName} (Паспорт: {passenger.PassportNumber})";
            lblFlightInfo.Text = $"Нислэг: {flight.FlightNumber} ({flight.DepartureAirport} -с {flight.ArrivalAirport}) - Хөдөлгөх: {flight.DepartureTime:g}";

            grpBookingDetails.Visible = true;
            grpSeatSelection.Visible = true;
            btnCheckIn.Visible = true;
            btnCheckIn.Enabled = _selectedSeat != null && !_isCheckingIn; // Enable check-in only if seat selected and not already checking in
            btnPrintBoardingPass.Visible = booking.IsCheckedIn; // Show print button only if already checked in

            Debug.WriteLine($"[CheckInForm] Displayed booking details for Booking ID: {booking.BookingId}");
        }

        private void UpdateSeatDisplay(int flightId)
        {
            if (_seatButtons == null || !_seatButtons.Any())
            {
                Debug.WriteLine("[CheckInForm] No seat buttons to update display.");
                return;
            }

            var flightSeats = _seats?.Where(s => s.FlightId == flightId).ToList() ?? new List<Seat>();

            foreach (var seatNumKey in _seatButtons.Keys)
            {
                var button = _seatButtons[seatNumKey];
                var seatData = flightSeats.FirstOrDefault(s => s.SeatNumber == seatNumKey);

                bool isBooked = seatData?.IsBooked ?? false; // This 'isBooked' now includes temporary reservations from hub
                UpdateSeatButtonUI(button, seatNumKey, isBooked);
            }
            grpSeatSelection.Visible = true;
            Debug.WriteLine("[CheckInForm] Updated seat display.");
        }

        private void UpdateSeatButtonUI(Button button, string seatNumber, bool isBooked)
        {
            // This method should be called on the UI thread
            if (button.InvokeRequired)
            {
                button.Invoke(new Action(() => UpdateSeatButtonUI(button, seatNumber, isBooked)));
                return;
            }

            if (isBooked) // This includes both permanently booked and temporarily reserved by other clients
            {
                if (_selectedSeat?.SeatNumber == seatNumber && _selectedSeat.IsBooked == false) // Our current selection that just got booked by someone else
                {
                    button.BackColor = Color.Orange; // Show it's now taken
                    button.Enabled = false;
                }
                else
                {
                    button.BackColor = Color.Red; // Permanently booked or temporarily reserved by another
                    button.Enabled = false;
                }
            }
            else if (_selectedSeat != null && _selectedSeat.SeatNumber == seatNumber)
            {
                button.BackColor = Color.Blue; // Our selected seat
                button.Enabled = true;
            }
            else
            {
                button.BackColor = Color.Green; // Available seat
                button.Enabled = true;
            }
            button.ForeColor = Color.White;
        }

        private async void btnCancel_Click(object sender, EventArgs e)
        {
            // Release any active reservation if the form is being cancelled
            if (_selectedSeat != null && _selectedBooking != null && !_selectedBooking.IsCheckedIn)
            {
                try
                {
                    await _seatStatusSignalRService.ReleaseSeatReservationAsync(_selectedBooking.FlightId, _selectedSeat.SeatNumber);
                    Debug.WriteLine($"[CheckInForm] Cancel clicked. Released reservation for seat {_selectedSeat.SeatNumber}.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CheckInForm] Error releasing reservation on cancel: {ex.Message}");
                }
            }
            ResetForm();
        }

        private void ShowBoardingPassDialog(BoardingPass boardingPass)
        {
            if (boardingPass == null)
            {
                MessageBox.Show("Суудлын тасалбар үүсгэж чадсангүй. Шаардлагатай мэдээлэл дутуу байна.",
                    "Тасалбарын алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine("[CheckInForm] Attempted to show boarding pass dialog with null data.");
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
                    Debug.WriteLine("[CheckInForm] BoardingPassDialog closed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Суудлын тасалбар харуулахад алдаа гарлаа: {ex.Message}",
                    "Харуулах алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"[CheckInForm] Error displaying boarding pass dialog: {ex.Message}");
            }
        }

        private void btnPrintBoardingPass_Click(object sender, EventArgs e)
        {
            if (_selectedBooking == null || !_selectedBooking.IsCheckedIn)
            {
                MessageBox.Show("Хэвлэх боломжтой суудлын тасалбар алга байна.", "Хэвлэх алдаа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Ensure _selectedBooking.Flight and _selectedBooking.Passenger are populated
                var bookingForPrint = _bookings.FirstOrDefault(b => b.BookingId == _selectedBooking.BookingId);
                if (bookingForPrint != null)
                {
                    if (bookingForPrint.Flight == null)
                    {
                        bookingForPrint.Flight = _flights?.FirstOrDefault(f => f.FlightId == bookingForPrint.FlightId);
                    }
                    if (bookingForPrint.Passenger == null)
                    {
                        bookingForPrint.Passenger = _passengers?.FirstOrDefault(p => p.PassengerId == bookingForPrint.PassengerId)
                            ?? _apiService.GetPassengersAsync().Result.FirstOrDefault(p => p.PassengerId == bookingForPrint.PassengerId);
                    }
                }

                var boardingPass = CreateBoardingPassFromBooking(bookingForPrint, _selectedSeat);
                if (boardingPass != null)
                {
                    _boardingPassPrinter.PrintBoardingPass(boardingPass);
                }
                else
                {
                    MessageBox.Show("Хэвлэх суудлын тасалбар үүсгэж чадсангүй.", "Хэвлэх алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Суудлын тасалбар хэвлэхэд алдаа гарлаа: {ex.Message}", "Хэвлэх алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"[CheckInForm] Error printing boarding pass: {ex.Message}");
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
                    Debug.WriteLine($"[CheckInForm] Released reservation for seat {_selectedSeat.SeatNumber} during form close.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CheckInForm] Error releasing reservation during close: {ex.Message}");
                }
            }

            // Unsubscribe from SignalR events and flight updates
            try
            {
                _seatStatusSignalRService.SeatBooked -= OnSeatBooked;
                _seatStatusSignalRService.SeatReleased -= OnSeatReleased;
                _seatStatusSignalRService.FlightSeatsReceived -= OnFlightSeatsReceived;
                _seatStatusSignalRService.SeatReserved -= OnSeatReserved;
                _seatStatusSignalRService.SeatReservationReleased -= OnSeatReservationReleased;
                _seatStatusSignalRService.SeatReservationFailed -= OnSeatReservationFailed;
                _seatStatusSignalRService.RefreshSeatsForFlight -= OnRefreshSeatsForFlight;

                if (!string.IsNullOrEmpty(_currentFlightNumberSubscription))
                {
                    await _seatStatusSignalRService.UnsubscribeFromFlightAsync(_currentFlightNumberSubscription);
                    Debug.WriteLine($"[CheckInForm] Unsubscribed from flight updates for {_currentFlightNumberSubscription} during form close.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error during SignalR cleanup on form close: {ex.Message}");
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
                Debug.WriteLine("[CheckInForm] Resources disposed successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error during resource dispose on form close: {ex.Message}");
            }
            base.OnFormClosed(e);
        }

        private async void btnCheckIn_Click(object sender, EventArgs e)
        {
            if (_selectedBooking == null || _selectedSeat == null)
            {
                MessageBox.Show("Захиалга болон суудал сонгоно уу.", "Check-In алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_isCheckingIn)
            {
                MessageBox.Show("Check-in явагдаж байна. Түр хүлээнэ үү.", "Check-in явагдаж байна", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _isCheckingIn = true;
            btnCheckIn.Enabled = false;
            btnCheckIn.Text = "Бүртгэж байна...";
            Debug.WriteLine($"[CheckInForm] Starting check-in for booking {_selectedBooking.BookingId} with seat {_selectedSeat.SeatId}");

            try
            {
                // Get the most current seat status from the API
                var currentSeats = await _apiService.GetSeatsByFlightAsync(_selectedBooking.FlightId);
                var seatToCheck = currentSeats?.FirstOrDefault(s => s.SeatNumber == _selectedSeat.SeatNumber);

                if (seatToCheck == null)
                {
                    MessageBox.Show("Сонгосон суудлын мэдээлэл олдсонгүй.", "Суудлын алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    await LoadSeatsForFlightAsync(_selectedBooking.FlightId); // Refresh seat display
                    return;
                }

                if (seatToCheck.IsBooked)
                {
                    MessageBox.Show("Энэ суудал аль хэдийн эзэгдсэн байна. Өөр суудал сонгоно уу.", "Суудал эзэгдсэн", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    await LoadSeatsForFlightAsync(_selectedBooking.FlightId); // Refresh seat display
                    return;
                }

                // If seat is temporarily reserved by another client, it will be reflected in IsBooked=true from the SignalR's GetFlightSeatsAsync response
                // but the API's GetSeatsByFlightAsync will only show IsBooked=true if it's permanently booked.
                // We need to ensure we're not trying to book a seat that SignalR marked as orange (reserved by another).
                // The `ApplyReservationsToSeats` in FlightHub already marks them as booked=true, so `seatToCheck.IsBooked` should reflect temporary reservations.
                // Let's rely on the SignalR's OnFlightSeatsReceived for real-time temporary state.
                // If this point is reached, it means `seatToCheck.IsBooked` from API is false, which is good.
                // The SignalR hub will handle the race condition for temporary reservations.

                // Use the correct seat ID from the database/API fresh data
                int seatIdToUse = seatToCheck.SeatId;
                Debug.WriteLine($"[CheckInForm] Using seat ID {seatIdToUse} for seat number {_selectedSeat.SeatNumber} (fetched from API)");

                // Perform check-in via API
                var response = await _apiService.CheckInAsync(_selectedBooking.BookingId, seatIdToUse);

                if (response != null && response.Success)
                {
                    _selectedBooking.IsCheckedIn = true;
                    // Update booking object with confirmed seat details
                    _selectedBooking.Seat = new Core.Models.Seat
                    {
                        SeatNumber = _selectedSeat.SeatNumber,
                        SeatId = seatIdToUse,
                        FlightId = _selectedBooking.FlightId,
                        Class = seatToCheck.Class, // Populate other properties if available
                        Price = seatToCheck.Price
                    };
                    _selectedBooking.SeatId = seatIdToUse;
                    _selectedBooking.CheckInTime = DateTime.UtcNow;

                    // Update the seat in our local SignalR received list to reflect permanent booking
                    var actualSeatInList = _seats?.FirstOrDefault(s => s.SeatNumber == _selectedSeat.SeatNumber && s.FlightId == _selectedBooking.FlightId);
                    if (actualSeatInList != null)
                    {
                        actualSeatInList.IsBooked = true;
                        Debug.WriteLine($"[CheckInForm] Locally marked seat {actualSeatInList.SeatNumber} as booked (IsBooked = true).");
                    }

                    // Notify SignalR hub to confirm the booking (this will broadcast SeatBooked)
                    var flight = _flights?.FirstOrDefault(f => f.FlightId == _selectedBooking.FlightId);
                    if (flight != null)
                    {
                        await _seatStatusSignalRService.ConfirmSeatBookingAsync(flight.FlightNumber, _selectedSeat.SeatNumber, _myBookingReference);
                        Debug.WriteLine($"[CheckInForm] Sent ConfirmSeatBookingAsync to hub for flight {flight.FlightNumber}, seat {_selectedSeat.SeatNumber}.");
                    }

                    // Create boarding pass
                    BoardingPass boardingPass;
                    if (response.BoardingPass != null)
                    {
                        boardingPass = response.BoardingPass;
                        Debug.WriteLine("[CheckInForm] Using boarding pass from API response.");
                    }
                    else
                    {
                        // Ensure all necessary data for boarding pass is available in _selectedBooking and _selectedSeat
                        // This might require re-fetching full booking/passenger/flight details if they are not complete
                        var completeBooking = _bookings.FirstOrDefault(b => b.BookingId == _selectedBooking.BookingId);
                        if (completeBooking == null)
                        {
                            completeBooking = await _apiService.GetBookingByIdAsync(_selectedBooking.BookingId);
                        }

                        // Ensure passenger and flight objects are populated in the completeBooking
                        if (completeBooking?.Passenger == null && completeBooking?.PassengerId > 0)
                        {
                            completeBooking.Passenger = _passengers?.FirstOrDefault(p => p.PassengerId == completeBooking.PassengerId)
                                ?? await _apiService.GetPassengersAsync().ContinueWith(t => t.Result.FirstOrDefault(p => p.PassengerId == completeBooking.PassengerId));
                        }
                        if (completeBooking?.Flight == null && completeBooking?.FlightId > 0)
                        {
                            completeBooking.Flight = _flights?.FirstOrDefault(f => f.FlightId == completeBooking.FlightId)
                                ?? await _apiService.GetFlightsAsync().ContinueWith(t => t.Result.FirstOrDefault(f => f.FlightId == completeBooking.FlightId));
                        }

                        boardingPass = CreateBoardingPassFromBooking(completeBooking, _selectedSeat);
                        Debug.WriteLine("[CheckInForm] Created boarding pass from local booking data.");
                    }

                    if (boardingPass == null)
                    {
                        MessageBox.Show("Суудлын тасалбар үүсгэж чадсангүй. Дахин оролдоно уу.", "Тасалбарын алдаа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    MessageBox.Show(response.Message ?? "Check-in амжилттай боллоо!", "Амжилттай", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Update seat button UI immediately to red (booked)
                    _suppressSeatUnavailableWarning = true; // Suppress warning during this programmatic update
                    if (_seatButtons.ContainsKey(_selectedSeat.SeatNumber))
                    {
                        UpdateSeatButtonUI(_seatButtons[_selectedSeat.SeatNumber], _selectedSeat.SeatNumber, true);
                    }
                    _suppressSeatUnavailableWarning = false;

                    // Show boarding pass dialog
                    ShowBoardingPassDialog(boardingPass);
                    ResetFormAfterBoardingPass();
                }
                else
                {
                    string errorMessage = response?.Message ?? "Check-in амжилтгүй боллоо. Дахин оролдоно уу.";
                    MessageBox.Show(errorMessage, "Check-In амжилтгүй", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Release our reservation since check-in failed
                    if (_selectedSeat != null)
                    {
                        await _seatStatusSignalRService.ReleaseSeatReservationAsync(_selectedBooking.FlightId, _selectedSeat.SeatNumber);
                        Debug.WriteLine($"[CheckInForm] Check-in failed. Released reservation for seat {_selectedSeat.SeatNumber}.");
                    }

                    // Reload seats to get current status and clear any stale UI state
                    await LoadSeatsForFlightAsync(_selectedBooking.FlightId);
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"[CheckInForm] HTTP error during check-in: {httpEx.Message}");
                MessageBox.Show($"Серверт холбогдоход алдаа гарлаа: {httpEx.Message}", "Холболтын алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Release reservation on error
                if (_selectedSeat != null)
                {
                    await _seatStatusSignalRService.ReleaseSeatReservationAsync(_selectedBooking.FlightId, _selectedSeat.SeatNumber);
                    Debug.WriteLine($"[CheckInForm] HTTP error. Released reservation for seat {_selectedSeat.SeatNumber}.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error during check-in: {ex.Message}");
                MessageBox.Show($"Check-in хийхэд алдаа гарлаа: {ex.Message}", "Check-In алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Release reservation on error
                if (_selectedSeat != null)
                {
                    await _seatStatusSignalRService.ReleaseSeatReservationAsync(_selectedBooking.FlightId, _selectedSeat.SeatNumber);
                    Debug.WriteLine($"[CheckInForm] General error. Released reservation for seat {_selectedSeat.SeatNumber}.");
                }
            }
            finally
            {
                _isCheckingIn = false;
                btnCheckIn.Text = "Бүртгэх";
                // Only enable button if we have valid selection and not checked in
                btnCheckIn.Enabled = (_selectedSeat != null && _selectedBooking != null && !_selectedBooking.IsCheckedIn);
            }
        }

        private async void SeatButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string seatNumber = button.Tag?.ToString();

            if (string.IsNullOrEmpty(seatNumber))
            {
                MessageBox.Show("Суудлын мэдээлэл олдсонгүй.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_selectedBooking == null)
            {
                MessageBox.Show("Эхлээд захиалга хайж сонгоно уу.", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_isCheckingIn)
            {
                MessageBox.Show("Check-in явагдаж байна. Түр хүлээнэ үү.", "Check-in явагдаж байна", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Find the seat data from our local _seats list (which is updated by SignalR)
            var seatData = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber && s.FlightId == _selectedBooking.FlightId);
            if (seatData == null)
            {
                MessageBox.Show("Суудлын мэдээлэл олдсонгүй. Суудлын зургийг дахин ачааллана уу.", "Суудлын алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await LoadSeatsForFlightAsync(_selectedBooking.FlightId); // Reload seats
                return;
            }

            if (seatData.IsBooked) // This includes if it's temporarily reserved by another client
            {
                MessageBox.Show("Энэ суудал аль хэдийн эзэгдсэн байна. Өөр суудал сонгоно уу.", "Суудал эзэгдсэн", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // If a seat was previously selected and is different from the new one, release the old reservation
                if (_selectedSeat != null && _selectedSeat.SeatNumber != seatNumber)
                {
                    Debug.WriteLine($"[CheckInForm] Releasing previous reservation for {_selectedSeat.SeatNumber}.");
                    await _seatStatusSignalRService.ReleaseSeatReservationAsync(_selectedBooking.FlightId, _selectedSeat.SeatNumber);
                    // Reset the UI of the previously selected seat
                    if (_seatButtons.ContainsKey(_selectedSeat.SeatNumber))
                    {
                        var prevButton = _seatButtons[_selectedSeat.SeatNumber];
                        var prevSeat = _seats?.FirstOrDefault(s => s.SeatNumber == _selectedSeat.SeatNumber);
                        if (prevSeat != null && !prevSeat.IsBooked) // Only if it's not permanently booked
                        {
                            UpdateSeatButtonUI(prevButton, _selectedSeat.SeatNumber, false); // Make it green (available)
                        }
                    }
                }

                // Try to reserve the new seat
                Debug.WriteLine($"[CheckInForm] Attempting to reserve seat {seatNumber} for booking {_myBookingReference}.");
                await _seatStatusSignalRService.ReserveSeatAsync(_selectedBooking.FlightId, seatNumber, _myBookingReference);

                // Temporarily mark as selected (blue) in UI, SignalR will confirm or fail
                button.BackColor = Color.Blue;
                lblSelectedSeat.Text = $"Сонгосон суудал: {seatNumber} (Захиалж байна...)";
                btnCheckIn.Enabled = false; // Disable check-in until reservation is confirmed

                // Note: _selectedSeat is updated in OnSeatReserved when OUR reservation is confirmed.
                // This means the button will turn blue and enable check-in after the SignalR callback.
                // If SignalR fails, OnSeatReservationFailed will revert the UI and selection.
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error selecting seat {seatNumber}: {ex.Message}");
                MessageBox.Show($"Суудал захиалахад алдаа гарлаа: {ex.Message}", "Захиалгын алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Revert UI if an error occurs before SignalR can respond
                if (_seatButtons.ContainsKey(seatNumber))
                {
                    var seatInList = _seats?.FirstOrDefault(s => s.SeatNumber == seatNumber);
                    if (seatInList != null && !seatInList.IsBooked)
                    {
                        UpdateSeatButtonUI(button, seatNumber, false);
                    }
                }
                _selectedSeat = null;
                lblSelectedSeat.Text = "Сонгосон суудал: (Үгүй)";
                btnCheckIn.Enabled = false;
            }
        }

        private BoardingPass CreateBoardingPassFromBooking(Booking booking, Core.Models.Seat seat)
        {
            if (booking?.Passenger == null || booking?.Flight == null)
            {
                Debug.WriteLine("[CheckInForm] Cannot create boarding pass - missing booking, passenger, or flight data.");
                return null;
            }

            try
            {
                // Use the seat passed in, which should be the _selectedSeat after successful check-in
                // or the seat associated with the booking if already checked in.
                string finalSeatNumber = seat?.SeatNumber ?? booking.Seat?.SeatNumber ?? "N/A";

                var boardingPass = new BoardingPass
                {
                    PassengerName = $"{booking.Passenger.FirstName} {booking.Passenger.LastName}",
                    PassportNumber = booking.Passenger.PassportNumber,
                    FlightNumber = booking.Flight.FlightNumber,
                    DepartureAirport = booking.Flight.DepartureAirport,
                    ArrivalAirport = booking.Flight.ArrivalAirport,
                    DepartureTime = booking.Flight.DepartureTime,
                    SeatNumber = finalSeatNumber,
                    BoardingTime = booking.Flight.DepartureTime.AddMinutes(-45)
                };

                Debug.WriteLine($"[CheckInForm] Created boarding pass for {boardingPass.PassengerName}, seat {boardingPass.SeatNumber}.");
                return boardingPass;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error creating boarding pass: {ex.Message}");
                return null;
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassportNumber.Text))
            {
                MessageBox.Show("Паспортын дугаар оруулна уу.", "Хоосон утга", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassportNumber.Focus();
                return;
            }

            string passportNumberToSearch = txtPassportNumber.Text.Trim();
            btnSearch.Enabled = false;
            btnSearch.Text = "Хайж байна...";

            // Release any existing reservation before searching for a new passenger
            if (_selectedSeat != null && _selectedBooking != null && !_selectedBooking.IsCheckedIn)
            {
                try
                {
                    await _seatStatusSignalRService.ReleaseSeatReservationAsync(_selectedBooking.FlightId, _selectedSeat.SeatNumber);
                    Debug.WriteLine($"[CheckInForm] Released existing reservation for seat {_selectedSeat.SeatNumber} before new search.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CheckInForm] Error releasing reservation during search initiation: {ex.Message}");
                }
            }
            ResetForm(); // Reset form state before displaying new search results

            try
            {
                Debug.WriteLine($"[CheckInForm] Searching for passport: {passportNumberToSearch}");

                var bookingsFromServer = await _apiService.GetBookingsByPassportAsync(passportNumberToSearch);

                if (bookingsFromServer != null && bookingsFromServer.Any())
                {
                    // Update local _bookings list with potentially more complete data
                    foreach (var b in bookingsFromServer)
                    {
                        var existingBooking = _bookings.FirstOrDefault(eb => eb.BookingId == b.BookingId);
                        if (existingBooking != null)
                        {
                            existingBooking.Passenger = b.Passenger;
                            existingBooking.Flight = b.Flight;
                            existingBooking.IsCheckedIn = b.IsCheckedIn;
                            existingBooking.SeatId = b.SeatId;
                            existingBooking.Seat = b.Seat; // Ensure Seat object is also updated
                            existingBooking.CheckInTime = b.CheckInTime;
                        }
                        else
                        {
                            _bookings.Add(b);
                        }
                    }

                    // Check if already checked in
                    var alreadyCheckedInBooking = bookingsFromServer.FirstOrDefault(b => b.IsCheckedIn);
                    if (alreadyCheckedInBooking != null)
                    {
                        Debug.WriteLine($"[CheckInForm] Found already checked-in booking: {alreadyCheckedInBooking.BookingId}");
                        _selectedBooking = alreadyCheckedInBooking;

                        // Ensure _selectedBooking has full passenger and flight details
                        if (_selectedBooking.Passenger == null && _selectedBooking.PassengerId > 0)
                        {
                            _selectedBooking.Passenger = _passengers?.FirstOrDefault(p => p.PassengerId == _selectedBooking.PassengerId)
                                ?? await _apiService.GetPassengersAsync().ContinueWith(t => t.Result.FirstOrDefault(p => p.PassengerId == _selectedBooking.PassengerId));
                        }
                        if (_selectedBooking.Flight == null && _selectedBooking.FlightId > 0)
                        {
                            _selectedBooking.Flight = _flights?.FirstOrDefault(f => f.FlightId == _selectedBooking.FlightId)
                                ?? await _apiService.GetFlightsAsync().ContinueWith(t => t.Result.FirstOrDefault(f => f.FlightId == _selectedBooking.FlightId));
                        }

                        // Load seat information for the checked-in seat
                        if (_selectedBooking.SeatId.HasValue && _selectedBooking.SeatId > 0)
                        {
                            _seats = await _apiService.GetSeatsByFlightAsync(_selectedBooking.FlightId); // Get all seats for the flight
                            _selectedSeat = _seats?.FirstOrDefault(s => s.SeatId == _selectedBooking.SeatId);
                        }
                        else if (!string.IsNullOrEmpty(_selectedBooking.Seat?.SeatNumber))
                        {
                            _seats = await _apiService.GetSeatsByFlightAsync(_selectedBooking.FlightId); // Get all seats for the flight
                            _selectedSeat = _seats?.FirstOrDefault(s => s.SeatNumber == _selectedBooking.Seat.SeatNumber);
                        }
                        else
                        {
                            Debug.WriteLine("[CheckInForm] Checked-in booking has no seat details. Boarding pass might be incomplete.");
                        }

                        var boardingPass = CreateBoardingPassFromBooking(_selectedBooking, _selectedSeat);
                        if (boardingPass != null)
                        {
                            ShowBoardingPassDialog(boardingPass);
                        }
                        else
                        {
                            MessageBox.Show("Суудлын тасалбар үүсгэж чадсангүй.", "Тасалбарын алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        ResetFormAfterBoardingPass(); // Reset form to clear sensitive data and UI elements
                        return;
                    }

                    // Find active bookings (not checked in)
                    var activeBookings = bookingsFromServer.Where(b => !b.IsCheckedIn).ToList();
                    if (!activeBookings.Any())
                    {
                        MessageBox.Show("Энэ зорчигчийн идэвхтэй захиалга олдсонгүй.", "Захиалга олдсонгүй", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetForm();
                        return;
                    }

                    _selectedBooking = activeBookings.First(); // Assume one active booking for simplicity
                    _myBookingReference = _selectedBooking.BookingId.ToString();

                    // Ensure _selectedBooking has full passenger and flight details
                    if (_selectedBooking.Passenger == null && _selectedBooking.PassengerId > 0)
                    {
                        _selectedBooking.Passenger = _passengers?.FirstOrDefault(p => p.PassengerId == _selectedBooking.PassengerId)
                            ?? await _apiService.GetPassengersAsync().ContinueWith(t => t.Result.FirstOrDefault(p => p.PassengerId == _selectedBooking.PassengerId));
                    }
                    if (_selectedBooking.Flight == null && _selectedBooking.FlightId > 0)
                    {
                        _selectedBooking.Flight = _flights?.FirstOrDefault(f => f.FlightId == _selectedBooking.FlightId)
                            ?? await _apiService.GetFlightsAsync().ContinueWith(t => t.Result.FirstOrDefault(f => f.FlightId == _selectedBooking.FlightId));
                    }


                    Debug.WriteLine($"[CheckInForm] Found active booking: {_selectedBooking.BookingId} for flight {_selectedBooking.FlightId}");

                    DisplayBookingDetails(_selectedBooking);
                    await LoadSeatsForFlightAsync(_selectedBooking.FlightId);
                }
                else
                {
                    MessageBox.Show("Энэ паспортын дугаартай захиалга олдсонгүй.", "Захиалга олдсонгүй", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetForm();
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"[CheckInForm] HTTP error during search: {httpEx.Message}");
                MessageBox.Show($"Серверт холбогдоход алдаа гарлаа: {httpEx.Message}", "Холболтын алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetForm();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInForm] Error searching bookings: {ex.Message}");
                MessageBox.Show($"Захиалга хайхад алдаа гарлаа: {ex.Message}", "Хайлтын алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetForm();
            }
            finally
            {
                btnSearch.Enabled = true;
                btnSearch.Text = "Хайх";
            }
        }
    }
}