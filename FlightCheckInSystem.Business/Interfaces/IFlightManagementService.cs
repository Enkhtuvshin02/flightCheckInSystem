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
        Task<Flight> GetFlightDetailsAsync(int flightId);        
        Task<IEnumerable<Passenger>> GetPassengersByFlightAsync(int flightId);
    }
}