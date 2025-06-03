using FlightCheckInSystem.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightCheckInSystem.Data.Interfaces
{
    public interface ISeatRepository
    {
        Task<Seat> GetSeatByIdAsync(int seatId);
        Task<Seat> GetSeatByNumberAndFlightAsync(string seatNumber, int flightId);
        Task<IEnumerable<Seat>> GetSeatsByFlightIdAsync(int flightId);
        Task<IEnumerable<Seat>> GetAvailableSeatsByFlightIdAsync(int flightId);
        Task<bool> UpdateSeatAsync(Seat seat);
        Task<bool> BookSeatAsync(int seatId, int bookingId);
        Task<bool> ReleaseSeatAsync(int seatId);
        Task<bool> BookSeatByNumberAsync(string seatNumber, int flightId, int bookingId);
        Task<int> AddSeatAsync(Seat seat);
        Task<bool> UnbookSeatAsync(int seatId);
    }
}