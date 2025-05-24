// FlightCheckInSystem.Data/Interfaces/ISeatRepository.cs
using FlightCheckInSystem.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightCheckInSystem.Data.Interfaces
{
    public interface ISeatRepository
    {
        Task<Seat> GetSeatByIdAsync(int seatId);
        Task<IEnumerable<Seat>> GetSeatsByFlightIdAsync(int flightId);
        Task<IEnumerable<Seat>> GetAvailableSeatsByFlightIdAsync(int flightId);
        Task<bool> BookSeatAsync(int seatId, int bookingId); // Modified to link seat to booking implicitly or explicitly
        Task<bool> UnbookSeatAsync(int seatId); // For rollbacks or cancellations
        Task<Seat> GetSeatByFlightAndNumberAsync(int flightId, string seatNumber);
    }
}