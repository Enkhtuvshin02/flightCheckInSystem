using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Core.Enums;
using FlightCheckInSystem.Data.Interfaces;
using FlightCheckInSystem.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightCheckInSystem.Server.Controllers
{
    public class FlightStatusUpdateRequest
    {
        public FlightStatus Status { get; set; }
    }
    
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightManagementService _flightService;
        private readonly ISeatRepository _seatRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ILogger<FlightsController> _logger;
        private readonly IHubContext<FlightHub> _hubContext;

        public FlightsController(
            IFlightManagementService flightService, 
            ISeatRepository seatRepository,
            IFlightRepository flightRepository,
            IHubContext<FlightHub> hubContext,
            ILogger<FlightsController> logger)
        {
            _flightService = flightService ?? throw new ArgumentNullException(nameof(flightService));
            _seatRepository = seatRepository ?? throw new ArgumentNullException(nameof(seatRepository));
            _flightRepository = flightRepository ?? throw new ArgumentNullException(nameof(flightRepository));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Flight>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlights()
        {
            try
            {
                var flights = await _flightService.GetAllFlightsAsync();
                return Ok(new ApiResponse<IEnumerable<Flight>> 
                { 
                    Success = true, 
                    Data = flights, 
                    Message = "Flights retrieved successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flights.");
                return StatusCode(500, new ApiResponse<IEnumerable<Flight>> 
                { 
                    Success = false, 
                    Message = "Internal server error retrieving flights." 
                });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Flight))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Flight>> GetFlight(int id)
        {
            try
            {
                var flight = await _flightService.GetFlightDetailsAsync(id);
                if (flight == null)
                {
                    return NotFound(new ApiResponse<Flight> 
                    { 
                        Success = false, 
                        Message = $"Flight with ID {id} not found" 
                    });
                }
                return Ok(new ApiResponse<Flight> 
                { 
                    Success = true, 
                    Data = flight, 
                    Message = "Flight retrieved successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving flight with ID {id}.");
                return StatusCode(500, new ApiResponse<Flight> 
                { 
                    Success = false, 
                    Message = "Internal server error retrieving flight." 
                });
            }
        }

        [HttpGet("number/{flightNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Flight))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Flight>> GetFlightByNumber(string flightNumber)
        {
            if (string.IsNullOrWhiteSpace(flightNumber))
            {
                return BadRequest(new ApiResponse<Flight> 
                { 
                    Success = false, 
                    Message = "Flight number cannot be empty" 
                });
            }
            
            try
            {
                var flights = await _flightService.GetAllFlightsAsync();
                var flight = flights.FirstOrDefault(f => 
                    f.FlightNumber.Equals(flightNumber, StringComparison.OrdinalIgnoreCase));
                
                if (flight == null)
                {
                    return NotFound(new ApiResponse<Flight> 
                    { 
                        Success = false, 
                        Message = $"Flight with number {flightNumber} not found" 
                    });
                }
                
                return Ok(new ApiResponse<Flight> 
                { 
                    Success = true, 
                    Data = flight, 
                    Message = "Flight retrieved successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving flight with number {flightNumber}.");
                return StatusCode(500, new ApiResponse<Flight> 
                { 
                    Success = false, 
                    Message = "Internal server error retrieving flight." 
                });
            }
        }

        [HttpGet("{id}/seats")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Seat>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Seat>>> GetFlightSeats(int id)
        {
            try
            {
                var flight = await _flightRepository.GetFlightByIdAsync(id);
                if (flight == null)
                {
                    return NotFound(new ApiResponse<IEnumerable<Seat>> 
                    { 
                        Success = false, 
                        Message = $"Flight with ID {id} not found" 
                    });
                }

                var seats = await _seatRepository.GetSeatsByFlightIdAsync(id);
                return Ok(new ApiResponse<IEnumerable<Seat>> 
                { 
                    Success = true, 
                    Data = seats, 
                    Message = $"Seats for flight {flight.FlightNumber} retrieved successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving seats for flight ID {id}.");
                return StatusCode(500, new ApiResponse<IEnumerable<Seat>> 
                { 
                    Success = false, 
                    Message = "Internal server error retrieving flight seats." 
                });
            }
        }

                [HttpGet("{id}/availableseats")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Seat>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Seat>>> GetAvailableSeats(int id)
        {
            _logger.LogInformation($"GetAvailableSeats called for flight ID {id} at {DateTime.UtcNow}.");
            try
            {
                                if (id <= 0)
                {
                    _logger.LogWarning($"Invalid flight ID: {id}");
                    return BadRequest(new ApiResponse<IEnumerable<Seat>> { Success = false, Message = $"Invalid flight ID: {id}" });
                }
                
                                var flight = await _flightRepository.GetFlightByIdAsync(id);
                if (flight == null)
                {
                    _logger.LogWarning($"Flight with ID {id} not found");
                    return NotFound(new ApiResponse<IEnumerable<Seat>> { Success = false, Message = $"Flight with ID {id} not found" });
                }
                
                _logger.LogInformation($"Found flight {flight.FlightNumber} (ID: {id}). Retrieving seats...");
                
                                var seats = await _seatRepository.GetSeatsByFlightIdAsync(id);
                var availableSeats = seats.Where(s => !s.IsBooked).ToList();
                
                _logger.LogInformation($"Found {availableSeats.Count} available seats out of {seats.Count()} total seats for flight {flight.FlightNumber}.");
                
                                return Ok(new ApiResponse<IEnumerable<Seat>> { 
                    Success = true, 
                    Data = availableSeats, 
                    Message = $"Available seats for flight {flight.FlightNumber} retrieved successfully. {availableSeats.Count} seats available."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving available seats for flight ID {id}.");
                return StatusCode(500, new ApiResponse<IEnumerable<Seat>> { 
                    Success = false, 
                    Message = $"Internal server error retrieving available seats: {ex.Message}"
                });
            }
        }

                [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<Flight>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Flight>> CreateFlight([FromBody] Flight flight)
        {
            _logger.LogInformation($"CreateFlight called at {DateTime.UtcNow}");
            
            if (flight == null)
            {
                _logger.LogWarning("CreateFlight received null flight");
                return BadRequest(new ApiResponse<Flight>
                {
                    Success = false,
                    Message = "Flight data cannot be null"
                });
            }
            
            if (string.IsNullOrWhiteSpace(flight.FlightNumber) ||
                string.IsNullOrWhiteSpace(flight.DepartureAirport) ||
                string.IsNullOrWhiteSpace(flight.ArrivalAirport))
            {
                _logger.LogWarning("CreateFlight received invalid flight data");
                return BadRequest(new ApiResponse<Flight>
                {
                    Success = false,
                    Message = "Flight number, departure airport, and arrival airport are required"
                });
            }
            
            try
            {
                // Set default status if not valid
                if (!Enum.IsDefined(typeof(FlightStatus), flight.Status))
                {
                    flight.Status = FlightStatus.Scheduled;
                }
                
                // Create flight with seats (20 rows, seats A through F)
                await _flightRepository.CreateFlightWithSeatsAsync(flight, 20, 'F');
                
                _logger.LogInformation($"Successfully created flight {flight.FlightNumber} with ID {flight.FlightId}");
                
                return CreatedAtAction(nameof(GetFlight), 
                    new { id = flight.FlightId }, 
                    new ApiResponse<Flight>
                    {
                        Success = true,
                        Data = flight,
                        Message = $"Flight {flight.FlightNumber} created successfully with ID {flight.FlightId}"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating flight: {ex.Message}");
                return StatusCode(500, new ApiResponse<Flight>
                {
                    Success = false,
                    Message = $"Internal server error creating flight: {ex.Message}"
                });
            }
        }

                [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Flight))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Flight>> UpdateFlightStatus(int id, FlightStatusUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<Flight> 
                { 
                    Success = false, 
                    Message = "Status update request cannot be null" 
                });
            }
            
            try
            {
                var flight = await _flightRepository.GetFlightByIdAsync(id);
                if (flight == null)
                {
                    return NotFound(new ApiResponse<Flight> 
                    { 
                        Success = false, 
                        Message = $"Flight with ID {id} not found" 
                    });
                }

                                if (!Enum.IsDefined(typeof(FlightStatus), request.Status))
                {
                    return BadRequest(new ApiResponse<Flight> 
                    { 
                        Success = false, 
                        Message = $"Invalid flight status: {request.Status}" 
                    });
                }

                flight.Status = request.Status;
                await _flightRepository.UpdateFlightAsync(flight);
                
                                await _hubContext.Clients.Group("FlightStatusBoard").SendAsync("FlightStatusUpdated", flight.FlightNumber, request.Status);
                _logger.LogInformation($"Broadcasted status update for flight {flight.FlightNumber} to {request.Status}");
                
                return Ok(new ApiResponse<Flight> 
                { 
                    Success = true, 
                    Data = flight, 
                    Message = $"Flight {flight.FlightNumber} status updated to {request.Status}" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for flight ID {id}.");
                return StatusCode(500, new ApiResponse<Flight> 
                { 
                    Success = false, 
                    Message = "Internal server error updating flight status." 
                });
            }
        }
    }
}