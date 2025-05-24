// FlightCheckInSystem.Data/Interfaces/IFlightRepository.cs
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightCheckInSystem.Data.Interfaces
{
    public interface IFlightRepository
    {
        Task<Flight> GetFlightByIdAsync(int flightId);
        Task<IEnumerable<Flight>> GetAllFlightsAsync();
        Task<int> AddFlightAsync(Flight flight);
        Task<bool> UpdateFlightAsync(Flight flight);
        Task<bool> UpdateFlightStatusAsync(int flightId, FlightStatus newStatus);
        Task<bool> DeleteFlightAsync(int flightId);
        Task CreateFlightWithSeatsAsync(Flight flight, int totalRows, char lastSeatLetterInRow);
    }
}