using Microsoft.AspNetCore.SignalR;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Core.Enums;
using FlightCheckInSystem.Server.Hubs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FlightCheckInSystem.Server.Services
{
    public interface IFlightHubService
    {
        Task BroadcastFlightStatusUpdate(string flightNumber, FlightStatus newStatus);
        Task BroadcastNewFlight(Flight flight);
        Task BroadcastFlightUpdated(Flight flight);
    }

    public class FlightHubService : IFlightHubService
    {
        private readonly IHubContext<FlightHub> _hubContext;
        private readonly ILogger<FlightHubService> _logger;

        public FlightHubService(IHubContext<FlightHub> hubContext, ILogger<FlightHubService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task BroadcastFlightStatusUpdate(string flightNumber, FlightStatus newStatus)
        {
            try
            {
                _logger.LogInformation($"Broadcasting flight status update: {flightNumber} -> {newStatus}");
                await _hubContext.Clients.Group("FlightStatusBoard")
                    .SendAsync("FlightStatusUpdated", flightNumber, newStatus);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error broadcasting flight status update for {flightNumber}");
            }
        }

        public async Task BroadcastNewFlight(Flight flight)
        {
            try
            {
                _logger.LogInformation($"Broadcasting new flight created: {flight.FlightNumber}");
                await _hubContext.Clients.Group("FlightStatusBoard")
                    .SendAsync("NewFlightCreated", flight);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error broadcasting new flight {flight.FlightNumber}");
            }
        }

        public async Task BroadcastFlightUpdated(Flight flight)
        {
            try
            {
                _logger.LogInformation($"Broadcasting flight updated: {flight.FlightNumber}");
                await _hubContext.Clients.Group("FlightStatusBoard")
                    .SendAsync("FlightUpdated", flight);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error broadcasting flight update {flight.FlightNumber}");
            }
        }
    }
}