using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace FlightCheckInSystem.Server.Controllers
{
    public class PassengerSearchRequestDto
    {
        public string PassportNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
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
    
    public class BookingResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
    
    public class PassengerNotFoundResponse
    {
        public string Message { get; set; }
        public bool NeedsCreation { get; set; }
    }
    
    public class BookingConflictResponse
    {
        public string Message { get; set; }
        public Booking ExistingBooking { get; set; }
    }

    [Route("api/bookings")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly ICheckInService _checkInService;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ILogger<BookingController> _logger;

        public BookingController(ICheckInService checkInService,
                               IPassengerRepository passengerRepository,
                               IBookingRepository bookingRepository,
                               IFlightRepository flightRepository,
                               ILogger<BookingController> logger)
        {
            _checkInService = checkInService ?? throw new ArgumentNullException(nameof(checkInService));
            _passengerRepository = passengerRepository ?? throw new ArgumentNullException(nameof(passengerRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _flightRepository = flightRepository ?? throw new ArgumentNullException(nameof(flightRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingResponse<List<Booking>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Booking>>> GetAllBookings()
        {
            try
            {
                var flights = await _flightRepository.GetAllFlightsAsync();
                var allBookings = new List<Booking>();
                
                foreach (var flight in flights)
                {
                    var flightBookings = await _bookingRepository.GetBookingsByFlightIdAsync(flight.FlightId);
                    allBookings.AddRange(flightBookings.ToList());
                }
                
                return Ok(new BookingResponse<List<Booking>>
                {
                    Success = true,
                    Data = allBookings,
                    Message = $"Retrieved {allBookings.Count} bookings"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving all bookings: {ex.Message}");
                return StatusCode(500, new BookingResponse<List<Booking>>
                {
                    Success = false,
                    Message = "Internal server error retrieving bookings"
                });
            }
        }

        [HttpGet("passport/{passportNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingResponse<List<Booking>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Booking>>> GetBookingsByPassport(string passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
            {
                return BadRequest(new BookingResponse<List<Booking>>
                {
                    Success = false,
                    Message = "Passport number is required"
                });
            }

            try
            {
                var passenger = await _passengerRepository.GetPassengerByPassportAsync(passportNumber);
                if (passenger == null)
                {
                    return NotFound(new BookingResponse<List<Booking>>
                    {
                        Success = false,
                        Message = $"No passenger found with passport number {passportNumber}"
                    });
                }


                var bookings = await _bookingRepository.GetBookingsByPassengerIdAsync(passenger.PassengerId);
                var bookingsList = bookings.ToList();
                return Ok(new BookingResponse<List<Booking>>
                {
                    Success = true,
                    Data = bookingsList,
                    Message = $"Retrieved {bookingsList.Count} bookings for passenger {passenger.FirstName} {passenger.LastName}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving bookings for passport {passportNumber}: {ex.Message}");
                return StatusCode(500, new BookingResponse<List<Booking>>
                {
                    Success = false,
                    Message = "Internal server error retrieving bookings"
                });
            }
        }

        [HttpPost("findorcreatepassenger")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingResponse<Passenger>))]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BookingResponse<Passenger>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(PassengerNotFoundResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Passenger>> FindOrCreatePassenger([FromBody] PassengerSearchRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new BookingResponse<Passenger>
                {
                    Success = false,
                    Message = "Request cannot be null"
                });
            }
            
            if (string.IsNullOrWhiteSpace(request.PassportNumber))
            {
                return BadRequest(new BookingResponse<Passenger>
                {
                    Success = false,
                    Message = "Passport number is required"
                });
            }

            try
            {
                var passenger = await _passengerRepository.GetPassengerByPassportAsync(request.PassportNumber);
                if (passenger == null)
                {
                    if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                    {
                        // If passenger not found, and no name provided, indicate that names are needed to create
                        return NotFound(new PassengerNotFoundResponse
                        {
                            Message = "Passenger not found. Provide FirstName and LastName to create.",
                            NeedsCreation = true
                        });
                    }
                    
                    passenger = new Passenger 
                    { 
                        PassportNumber = request.PassportNumber, 
                        FirstName = request.FirstName, 
                        LastName = request.LastName,
                        Email = request.Email,
                        Phone = request.Phone
                    };
                    
                    passenger.PassengerId = await _passengerRepository.AddPassengerAsync(passenger);
                    _logger.LogInformation($"Created new passenger ID {passenger.PassengerId} with passport {passenger.PassportNumber}");
                    
                    return CreatedAtAction(nameof(FindOrCreatePassenger), 
                        new { id = passenger.PassengerId }, 
                        new BookingResponse<Passenger>
                        {
                            Success = true,
                            Data = passenger,
                            Message = $"Passenger created with ID {passenger.PassengerId}"
                        });
                }
                
                _logger.LogInformation($"Found passenger ID {passenger.PassengerId} with passport {passenger.PassportNumber}");
                return Ok(new BookingResponse<Passenger>
                {
                    Success = true,
                    Data = passenger,
                    Message = "Passenger found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FindOrCreatePassenger");
                return StatusCode(500, new BookingResponse<Passenger>
                {
                    Success = false,
                    Message = "Internal server error processing passenger request"
                });
            }
        }

        // POST: api/booking/findbooking
        [HttpPost("findbooking")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingResponse<Booking>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Booking>> FindBooking([FromBody] BookingSearchByPassportFlightRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new BookingResponse<Booking>
                {
                    Success = false,
                    Message = "Request cannot be null"
                });
            }
            
            if (string.IsNullOrWhiteSpace(request.PassportNumber) || request.FlightId <= 0)
            {
                return BadRequest(new BookingResponse<Booking>
                {
                    Success = false,
                    Message = "Valid Passport number and FlightId are required"
                });
            }
            
            try
            {
                // We need passengerId first
                var passenger = await _passengerRepository.GetPassengerByPassportAsync(request.PassportNumber);
                if (passenger == null)
                {
                    return NotFound(new BookingResponse<Booking>
                    {
                        Success = false,
                        Message = "Passenger not found with this passport number"
                    });
                }

                var booking = await _bookingRepository.GetBookingByPassengerAndFlightAsync(passenger.PassengerId, request.FlightId);
                if (booking == null)
                {
                    return NotFound(new BookingResponse<Booking>
                    {
                        Success = false,
                        Message = "No booking found for this passenger on the specified flight"
                    });
                }
                
                return Ok(new BookingResponse<Booking>
                {
                    Success = true,
                    Data = booking,
                    Message = $"Booking found for passenger {passenger.FirstName} {passenger.LastName} on flight {booking.FlightId}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding booking");
                return StatusCode(500, new BookingResponse<Booking>
                {
                    Success = false,
                    Message = "Internal server error finding booking"
                });
            }
        }


        // POST: api/booking
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BookingResponse<Booking>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(BookingConflictResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Booking>> CreateBooking([FromBody] BookingCreateRequestDto request)
        {
            _logger.LogInformation($"CreateBooking called at {DateTime.UtcNow} with PassengerId={request?.PassengerId}, FlightId={request?.FlightId}");
            
            if (request == null)
            {
                _logger.LogWarning("CreateBooking received null request");
                return BadRequest(new BookingResponse<Booking>
                {
                    Success = false,
                    Message = "Request cannot be null"
                });
            }
            
            if (request.PassengerId <= 0 || request.FlightId <= 0)
            {
                _logger.LogWarning($"CreateBooking received invalid IDs: PassengerId={request.PassengerId}, FlightId={request.FlightId}");
                return BadRequest(new BookingResponse<Booking>
                {
                    Success = false,
                    Message = $"Valid PassengerId and FlightId are required. Received: PassengerId={request.PassengerId}, FlightId={request.FlightId}"
                });
            }

            try
            {
                // Verify passenger exists
                var passenger = await _passengerRepository.GetPassengerByIdAsync(request.PassengerId);
                if (passenger == null)
                {
                    _logger.LogWarning($"CreateBooking: Passenger with ID {request.PassengerId} not found");
                    return BadRequest(new BookingResponse<Booking>
                    {
                        Success = false,
                        Message = $"Passenger with ID {request.PassengerId} not found"
                    });
                }
                _logger.LogInformation($"CreateBooking: Found passenger {passenger.FirstName} {passenger.LastName} (ID: {passenger.PassengerId})");
                
                // Verify flight exists
                var flight = await _flightRepository.GetFlightByIdAsync(request.FlightId);
                if (flight == null)
                {
                    _logger.LogWarning($"CreateBooking: Flight with ID {request.FlightId} not found");
                    return BadRequest(new BookingResponse<Booking>
                    {
                        Success = false,
                        Message = $"Flight with ID {request.FlightId} not found"
                    });
                }
                _logger.LogInformation($"CreateBooking: Found flight {flight.FlightNumber} (ID: {flight.FlightId})");
                
                // Check if booking already exists to prevent duplicates (handled by DB unique constraint too)
                var existingBooking = await _bookingRepository.GetBookingByPassengerAndFlightAsync(request.PassengerId, request.FlightId);
                if (existingBooking != null)
                {
                    _logger.LogWarning($"CreateBooking: Booking already exists for passenger {request.PassengerId} on flight {request.FlightId} (Booking ID: {existingBooking.BookingId})");
                    return Conflict(new BookingConflictResponse
                    {
                        Message = $"Booking already exists for passenger {passenger.FirstName} {passenger.LastName} on flight {flight.FlightNumber}",
                        ExistingBooking = existingBooking
                    });
                }

                _logger.LogInformation($"CreateBooking: Creating new booking for passenger {passenger.FirstName} {passenger.LastName} on flight {flight.FlightNumber}");
                var newBooking = new Booking
                {
                    PassengerId = request.PassengerId,
                    FlightId = request.FlightId,
                    ReservationDate = DateTime.UtcNow,
                    IsCheckedIn = false,
                    SeatId = null
                };
                
                newBooking.BookingId = await _bookingRepository.AddBookingAsync(newBooking);
                _logger.LogInformation($"CreateBooking: Successfully created new booking ID {newBooking.BookingId} for passenger {passenger.PassengerId} on flight {flight.FlightId}");

                // Fetch the full booking details to return
                var createdBookingDetails = await _bookingRepository.GetBookingByIdAsync(newBooking.BookingId);
                if (createdBookingDetails == null)
                {
                    _logger.LogError($"CreateBooking: Could not retrieve newly created booking with ID {newBooking.BookingId}");
                    // Still return success since we know the booking was created
                    return CreatedAtAction(nameof(CreateBooking), 
                        new { id = newBooking.BookingId }, 
                        new BookingResponse<Booking>
                        {
                            Success = true,
                            Data = newBooking,
                            Message = $"Booking created successfully with ID {newBooking.BookingId}, but full details could not be retrieved"
                        });
                }
                
                return CreatedAtAction(nameof(CreateBooking), 
                    new { id = createdBookingDetails.BookingId }, 
                    new BookingResponse<Booking>
                    {
                        Success = true,
                        Data = createdBookingDetails,
                        Message = $"Booking created successfully with ID {createdBookingDetails.BookingId} for passenger {passenger.FirstName} {passenger.LastName} on flight {flight.FlightNumber}"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating booking for PassengerId={request.PassengerId}, FlightId={request.FlightId}");
                
                // Include more detailed error information in the response
                return StatusCode(500, new BookingResponse<Booking>
                {
                    Success = false,
                    Message = $"Internal server error creating booking: {ex.Message}"
                });
            }
        }
    }
}