using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FlightCheckInSystem.Core.Models;
using System.Diagnostics;

namespace FlightCheckInSystem.FormsApp
{
    public partial class BookingSelectionForm : Form
    {
        private List<Booking> _bookings;
        private Booking _selectedBooking;
        private DataGridView _bookingsGrid;
        private Button _btnSelectForCheckIn;
        private Button _btnViewBoardingPass;
        private Button _btnCancel;
        private Label _lblInstruction;
        private TableLayoutPanel _mainLayout;
        private TableLayoutPanel _buttonPanel;

        public Booking SelectedBooking => _selectedBooking;
        public BookingAction SelectedAction { get; private set; }

        public BookingSelectionForm(List<Booking> bookings, string passengerName)
        {
            _bookings = bookings ?? throw new ArgumentNullException(nameof(bookings));
            InitializeComponent();
            LoadBookings();
            Debug.WriteLine($"[BookingSelectionForm] Initialized with {_bookings.Count} bookings for {passengerName}");
        }

      

        private void SetupGridColumns()
        {
            _bookingsGrid.Columns.Clear();

            // Configure column header style
            _bookingsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            _bookingsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _bookingsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            _bookingsGrid.ColumnHeadersHeight = 40;

            _bookingsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FlightNumber",
                HeaderText = "Нислэг",
                DataPropertyName = "FlightNumber",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _bookingsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Route",
                HeaderText = "Чиглэл",
                DataPropertyName = "Route",
                Width = 220,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });

            _bookingsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DepartureTime",
                HeaderText = "Хөдөлгөх цаг",
                DataPropertyName = "DepartureTime",
                Width = 150,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _bookingsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Төлөв",
                DataPropertyName = "Status",
                Width = 120,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _bookingsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SeatNumber",
                HeaderText = "Суудал",
                DataPropertyName = "SeatNumber",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _bookingsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CheckInStatus",
                HeaderText = "Бүртгэл",
                DataPropertyName = "CheckInStatus",
                Width = 120,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Set alternating row colors
            _bookingsGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            _bookingsGrid.DefaultCellStyle.BackColor = Color.White;
            _bookingsGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            _bookingsGrid.DefaultCellStyle.SelectionForeColor = Color.White;
        }
private void BookingsGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
{
    try
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            var row = _bookingsGrid.Rows[e.RowIndex];
            var checkInStatusCell = row.Cells["CheckInStatus"];
            
            if (checkInStatusCell != null && checkInStatusCell.Value != null)
            {
                var checkInStatus = checkInStatusCell.Value.ToString();

                if (checkInStatus == "Бүртгэгдсэн")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(39, 174, 96);
                    e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
                else if (checkInStatus == "Бүртгэгдээгүй")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(230, 126, 34);
                    e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"[BookingSelectionForm] Error in cell formatting: {ex.Message}");
    }
}

private void LoadBookings()
{
    try
    {
        var displayBookings = _bookings.Select(b => new
        {
            BookingId = b.BookingId,
            FlightNumber = b.Flight?.FlightNumber ?? "N/A",
            Route = $"{b.Flight?.DepartureAirport ?? "N/A"} → {b.Flight?.ArrivalAirport ?? "N/A"}",
            DepartureTime = b.Flight?.DepartureTime.ToString("MM/dd HH:mm") ?? "N/A",
            Status = GetFlightStatusText(b.Flight?.Status),
            // Fixed: Use proper seat number access
            SeatNumber = b.IsCheckedIn ? (b.Seat?.SeatNumber ?? "N/A") : "Сонгоогүй",
            CheckInStatus = b.IsCheckedIn ? "Бүртгэгдсэн" : "Бүртгэгдээгүй",
            OriginalBooking = b
        }).ToList();

        _bookingsGrid.DataSource = displayBookings;

        if (displayBookings.Any())
        {
            _bookingsGrid.Rows[0].Selected = true;
        }

        Debug.WriteLine($"[BookingSelectionForm] Loaded {displayBookings.Count} bookings into grid");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"[BookingSelectionForm] Error loading bookings: {ex.Message}");
        MessageBox.Show($"Захиалгын мэдээлэл ачаалахад алдаа гарлаа: {ex.Message}", 
            "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
        private string GetFlightStatusText(FlightCheckInSystem.Core.Enums.FlightStatus? status)
        {
            if (!status.HasValue) return "N/A";

            return status.Value switch
            {
                FlightCheckInSystem.Core.Enums.FlightStatus.Scheduled => "Товлогдсон",
                FlightCheckInSystem.Core.Enums.FlightStatus.CheckingIn => "Бүртгэл нээлттэй",
                FlightCheckInSystem.Core.Enums.FlightStatus.Boarding => "Суух",
                FlightCheckInSystem.Core.Enums.FlightStatus.GateClosed => "Хаалга хаагдсан",
                FlightCheckInSystem.Core.Enums.FlightStatus.Departed => "Хөдөлсөн",
                FlightCheckInSystem.Core.Enums.FlightStatus.Delayed => "Хойшлогдсон",
                FlightCheckInSystem.Core.Enums.FlightStatus.Cancelled => "Цуцлагдсан",
                _ => status.ToString()
            };
        }

        private void BookingsGrid_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (_bookingsGrid.SelectedRows.Count > 0)
                {
                    var selectedRow = _bookingsGrid.SelectedRows[0];
                    var selectedData = selectedRow.DataBoundItem;

                    // Get the original booking from the anonymous object
                    var originalBookingProperty = selectedData.GetType().GetProperty("OriginalBooking");
                    _selectedBooking = originalBookingProperty?.GetValue(selectedData) as Booking;

                    Debug.WriteLine($"[BookingSelectionForm] Selected booking: {_selectedBooking?.BookingId}, IsCheckedIn: {_selectedBooking?.IsCheckedIn}");
                    UpdateButtonStates();
                }
                else
                {
                    _selectedBooking = null;
                    _btnSelectForCheckIn.Enabled = false;
                    _btnViewBoardingPass.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BookingSelectionForm] Error in selection changed: {ex.Message}");
            }
        }

        private void UpdateButtonStates()
        {
            try
            {
                if (_selectedBooking == null)
                {
                    _btnSelectForCheckIn.Enabled = false;
                    _btnViewBoardingPass.Enabled = false;
                    return;
                }

                Debug.WriteLine($"[BookingSelectionForm] UpdateButtonStates: Booking {_selectedBooking.BookingId}, IsCheckedIn: {_selectedBooking.IsCheckedIn}");

                // Enable seat selection for non-checked-in bookings
                _btnSelectForCheckIn.Enabled = !_selectedBooking.IsCheckedIn;

                // Enable boarding pass view for checked-in bookings
                _btnViewBoardingPass.Enabled = _selectedBooking.IsCheckedIn;

                // Update button text and appearance based on availability
                if (_selectedBooking.IsCheckedIn)
                {
                    _btnSelectForCheckIn.Text = "Аль хэдийн бүртгэгдсэн";
                    _btnSelectForCheckIn.BackColor = Color.FromArgb(149, 165, 166);
                    _btnViewBoardingPass.Text = "Суудлын тасалбар харах";
                    _btnViewBoardingPass.BackColor = Color.FromArgb(52, 152, 219);
                }
                else
                {
                    _btnSelectForCheckIn.Text = "Суудал сонгох";
                    _btnSelectForCheckIn.BackColor = Color.FromArgb(39, 174, 96);
                    _btnViewBoardingPass.Text = "Бүртгэгдээгүй";
                    _btnViewBoardingPass.BackColor = Color.FromArgb(149, 165, 166);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BookingSelectionForm] Error in UpdateButtonStates: {ex.Message}");
            }
        }

        private void BtnSelectForCheckIn_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedBooking != null && !_selectedBooking.IsCheckedIn)
                {
                    SelectedAction = BookingAction.SelectSeat;
                    this.DialogResult = DialogResult.OK;
                    Debug.WriteLine($"[BookingSelectionForm] User selected seat selection for booking {_selectedBooking.BookingId}");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Энэ захиалгын суудал сонгох боломжгүй байна.", "Анхааруулга",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BookingSelectionForm] Error in BtnSelectForCheckIn_Click: {ex.Message}");
                MessageBox.Show($"Алдаа гарлаа: {ex.Message}", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnViewBoardingPass_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedBooking != null && _selectedBooking.IsCheckedIn)
                {
                    SelectedAction = BookingAction.ViewBoardingPass;
                    this.DialogResult = DialogResult.OK;
                    Debug.WriteLine($"[BookingSelectionForm] User selected boarding pass view for booking {_selectedBooking.BookingId}");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Энэ захиалгын суудлын тасалбар харах боломжгүй байна.", "Анхааруулга",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BookingSelectionForm] Error in BtnViewBoardingPass_Click: {ex.Message}");
                MessageBox.Show($"Алдаа гарлаа: {ex.Message}", "Алдаа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.DialogResult = DialogResult.Cancel;
                Debug.WriteLine("[BookingSelectionForm] User cancelled booking selection");
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BookingSelectionForm] Error in BtnCancel_Click: {ex.Message}");
                this.Close(); // Force close even if error
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                // Clean up resources
                _bookingsGrid?.Dispose();
                _mainLayout?.Dispose();
                _buttonPanel?.Dispose();

                Debug.WriteLine("[BookingSelectionForm] Form closed and resources cleaned up");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BookingSelectionForm] Error during cleanup: {ex.Message}");
            }

            base.OnFormClosed(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Handle Escape key to cancel
            if (e.KeyCode == Keys.Escape)
            {
                BtnCancel_Click(null, null);
                e.Handled = true;
            }
            // Handle Enter key to select default action
            else if (e.KeyCode == Keys.Enter)
            {
                if (_btnSelectForCheckIn.Enabled)
                {
                    BtnSelectForCheckIn_Click(null, null);
                }
                else if (_btnViewBoardingPass.Enabled)
                {
                    BtnViewBoardingPass_Click(null, null);
                }
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
    }

    public enum BookingAction
    {
        SelectSeat,
        ViewBoardingPass
    }
}