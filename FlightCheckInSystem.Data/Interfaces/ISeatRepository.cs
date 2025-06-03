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
        Task<bool> BookSeatAsync(int seatId, int bookingId);         Task<bool> UnbookSeatAsync(int seatId);         Task<Seat> GetSeatByFlightAndNumberAsync(int flightId, string seatNumber);
    }
}