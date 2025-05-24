using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlightCheckInSystem.Server.Controllers
{
    // DTOs for Booking Controller
    public class PassengerSearchRequestDto // For finding or creating passenger
    {
        public string PassportNumber { get; set; }
        public string FirstName { get; set; } // Optional, for creating
        public string LastName { get; set; }  // Optional, for creating
    }

    public class BookingCreateRequestDto
    {
        public int PassengerId { get; set; }
        public int FlightId { get; set; }
    }

    public class BookingSearchByPassportFlightRequestDto
    {
        public string PassportNumber { get; set; }
        public int FlightId { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly ICheckInService _checkInService;
        private readonly IPassengerRepository _passengerRepository; // Direct repo access for simple find/create
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookingController> _logger;


        public BookingController(ICheckInService checkInService,
                                 IPassengerRepository passengerRepository,
                                 IBookingRepository bookingRepository,
                                 ILogger<BookingController> logger)
        {
            _checkInService = checkInService;
            _passengerRepository = passengerRepository;
            _bookingRepository = bookingRepository;
            _logger = logger;
        }

        // POST: api/booking/findorcreatepassenger
        [HttpPost("findorcreatepassenger")]
        public async Task<ActionResult<Passenger>> FindOrCreatePassenger([FromBody] PassengerSearchRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PassportNumber))
                return BadRequest("Passport number is required.");

            try
            {
                var passenger = await _passengerRepository.GetPassengerByPassportAsync(request.PassportNumber);
                if (passenger == null)
                {
                    if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                    {
                        // If passenger not found, and no name provided, indicate that names are needed to create
                        return NotFound(new { Message = "Passenger not found. Provide FirstName and LastName to create.", NeedsCreation = true });
                    }
                    passenger = new Passenger { PassportNumber = request.PassportNumber, FirstName = request.FirstName, LastName = request.LastName };
                    passenger.PassengerId = await _passengerRepository.AddPassengerAsync(passenger);
                    _logger.LogInformation($"Created new passenger ID {passenger.PassengerId} with passport {passenger.PassportNumber}");
                    return CreatedAtAction(nameof(FindOrCreatePassenger), new { id = passenger.PassengerId }, passenger); // Return 201 Created
                }
                _logger.LogInformation($"Found passenger ID {passenger.PassengerId} with passport {passenger.PassportNumber}");
                return Ok(passenger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FindOrCreatePassenger");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/booking/findbooking
        [HttpPost("findbooking")]
        public async Task<ActionResult<Booking>> FindBooking([FromBody] BookingSearchByPassportFlightRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PassportNumber) || request.FlightId <= 0)
                return BadRequest("Valid Passport number and FlightId are required.");
            try
            {
                // We need passengerId first
                var passenger = await _passengerRepository.GetPassengerByPassportAsync(request.PassportNumber);
                if (passenger == null)
                {
                    return NotFound(new { Message = "Passenger not found with this passport number." });
                }

                var booking = await _bookingRepository.GetBookingByPassengerAndFlightAsync(passenger.PassengerId, request.FlightId);
                if (booking == null)
                {
                    return NotFound(new { Message = "No booking found for this passenger on the specified flight." });
                }
                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding booking");
                return StatusCode(500, "Internal server error");
            }
        }


        // POST: api/booking
        [HttpPost]
        public async Task<ActionResult<Booking>> CreateBooking([FromBody] BookingCreateRequestDto request)
        {
            if (request.PassengerId <= 0 || request.FlightId <= 0)
                return BadRequest("Valid PassengerId and FlightId are required.");

            try
            {
                // Check if booking already exists to prevent duplicates (handled by DB unique constraint too)
                var existingBooking = await _bookingRepository.GetBookingByPassengerAndFlightAsync(request.PassengerId, request.FlightId);
                if (existingBooking != null)
                {
                    return Conflict(new { Message = "Booking already exists for this passenger on this flight.", ExistingBooking = existingBooking });
                }

                var newBooking = new Booking
                {
                    PassengerId = request.PassengerId,
                    FlightId = request.FlightId,
                    ReservationDate = DateTime.UtcNow,
                    IsCheckedIn = false,
                    SeatId = null
                };
                newBooking.BookingId = await _bookingRepository.AddBookingAsync(newBooking);
                _logger.LogInformation($"Created new booking ID {newBooking.BookingId}");

                // Fetch the full booking details to return
                var createdBookingDetails = await _bookingRepository.GetBookingByIdAsync(newBooking.BookingId);
                return CreatedAtAction(nameof(CreateBooking), new { id = createdBookingDetails.BookingId }, createdBookingDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}