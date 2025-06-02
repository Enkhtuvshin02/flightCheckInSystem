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
    public class CheckInRequestDto
    {
        public int BookingId { get; set; }
        public int SeatId { get; set; }
    }

    public class CheckInResponseDto
    {
        public BoardingPass boardingPass { get; set; }
        public string Message { get; set; }
    }

    [Route("api/checkin")]
    [ApiController]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _checkInService;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ILogger<CheckInController> _logger;

        public CheckInController(ICheckInService checkInService,
                               IPassengerRepository passengerRepository,
                               IBookingRepository bookingRepository,
                               IFlightRepository flightRepository,
                               ILogger<CheckInController> logger)
        {
            _checkInService = checkInService ?? throw new ArgumentNullException(nameof(checkInService));
            _passengerRepository = passengerRepository ?? throw new ArgumentNullException(nameof(passengerRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _flightRepository = flightRepository ?? throw new ArgumentNullException(nameof(flightRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> CheckInPassenger([FromBody] CheckInRequestDto request)
        {
            try
            {
                var (success, message, boardingPass) = await _checkInService.AssignSeatToBookingAsync(request.BookingId, request.SeatId);
                return Ok(new {
                    success,
                    message,
                    boardingPass
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in passenger.");
                return StatusCode(500, new { success = false, message = "Internal server error checking in passenger.", boardingPass = (object)null });
            }
        }
    }
}