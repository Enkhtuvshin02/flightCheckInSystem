using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Core.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using System; 
namespace FlightCheckInSystem.Business.Interfaces
{
    public interface IFlightManagementService
    {
        Task<IEnumerable<Flight>> GetAllFlightsAsync();
        Task<Flight> GetFlightDetailsAsync(int flightId);         Task<(bool success, string message, Flight createdFlight)> CreateFlightWithSeatLayoutAsync(
            string flightNumber, string departureAirport, string arrivalAirport,
            DateTime departureTime, DateTime arrivalTime,
            int totalRows, char lastSeatLetterInRow);
        Task<IEnumerable<Passenger>> GetPassengersByFlightAsync(int flightId);
    }
}