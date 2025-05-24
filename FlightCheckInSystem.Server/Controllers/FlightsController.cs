using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlightCheckInSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightManagementService _flightService;
        private readonly ILogger<FlightsController> _logger;

        public FlightsController(IFlightManagementService flightService, ILogger<FlightsController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        // GET: api/flights
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlights()
        {
            try
            {
                var flights = await _flightService.GetAllFlightsAsync();
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flights.");
                return StatusCode(500, "Internal server error retrieving flights.");
            }
        }
    }
}