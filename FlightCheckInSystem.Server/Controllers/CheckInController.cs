using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FlightCheckInSystem.Server.Controllers
{
    public class CheckInRequest
    {
        public int BookingId { get; set; }
        public int SeatId { get; set; }
    }

    public class CheckInResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public BoardingPass BoardingPass { get; set; }
    }

    [Route("api/checkin")]
    [ApiController]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _checkInService;
        private readonly ILogger<CheckInController> _logger;

        public CheckInController(ICheckInService checkInService, ILogger<CheckInController> logger)
        {
            _checkInService = checkInService ?? throw new ArgumentNullException(nameof(checkInService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> CheckInPassenger([FromBody] CheckInRequest request)
        {
            if (request == null || request.BookingId <= 0 || request.SeatId <= 0)
            {
                return BadRequest(new CheckInResponse
                {
                    Success = false,
                    Message = "Valid BookingId and SeatId are required",
                    BoardingPass = null
                });
            }

            try
            {
                var (success, message, boardingPass) = await _checkInService.AssignSeatToBookingAsync(request.BookingId, request.SeatId);

                return Ok(new CheckInResponse
                {
                    Success = success,
                    Message = message,
                    BoardingPass = boardingPass
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in passenger");
                return StatusCode(500, new CheckInResponse
                {
                    Success = false,
                    Message = "Internal server error",
                    BoardingPass = null
                });
            }
        }
    }
}