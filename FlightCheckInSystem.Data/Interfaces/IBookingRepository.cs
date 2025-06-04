using FlightCheckInSystem.Core.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FlightCheckInSystem.Data.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking> GetBookingByIdAsync(int bookingId);
        Task<Booking> GetBookingByPassengerAndFlightAsync(int passengerId, int flightId);
        Task<IEnumerable<Booking>> GetBookingsByFlightIdAsync(int flightId);
        Task<IEnumerable<Booking>> GetBookingsByPassengerIdAsync(int passengerId);
        Task<int> AddBookingAsync(Booking booking);
        Task<bool> UpdateBookingAsync(Booking booking);         Task<bool> DeleteBookingAsync(int bookingId);
    }
}