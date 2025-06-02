using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System;

namespace FlightCheckInSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CorsTestController : ControllerBase
    {
        private readonly ILogger<CorsTestController> _logger;

        public CorsTestController(ILogger<CorsTestController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [EnableCors("CorsPolicy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            _logger.LogInformation("CORS test endpoint called");

            var response = new
            {
                Success = true,
                Message = "CORS is working correctly",
                Timestamp = DateTime.Now,
                Headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            return Ok(response);
        }

        [HttpOptions]
        [EnableCors("CorsPolicy")]
        public IActionResult Options()
        {
            _logger.LogInformation("CORS preflight request received");

            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

            return Ok();
        }
    }
}