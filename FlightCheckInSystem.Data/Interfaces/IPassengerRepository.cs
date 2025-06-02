// FlightCheckInSystem.Data/Interfaces/IPassengerRepository.cs
using FlightCheckInSystem.Core.Models;
using System.Threading.Tasks;

namespace FlightCheckInSystem.Data.Interfaces
{
    public interface IPassengerRepository
    {
        Task<Passenger> GetPassengerByIdAsync(int passengerId);
        Task<Passenger> GetPassengerByPassportAsync(string passportNumber);
        Task<Passenger> GetPassengerByPassportNumberAsync(string passportNumber);
        Task<Passenger> CreatePassengerAsync(Passenger passenger);
        Task<int> AddPassengerAsync(Passenger passenger);
    }
}