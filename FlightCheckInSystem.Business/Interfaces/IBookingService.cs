// FlightCheckInSystem.Business/Interfaces/IBookingService.cs
using FlightCheckInSystem.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightCheckInSystem.Business.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<Flight>> GetAvailableFlightsAsync();
        Task<(bool success, string message, Booking booking)> CreateBookingAsync(int passengerId, int flightId);
        Task<IEnumerable<Booking>> GetBookingsByPassengerIdAsync(int passengerId);
        Task<IEnumerable<Booking>> GetBookingsByFlightIdAsync(int flightId);
        Task<Booking> GetBookingByIdAsync(int bookingId);
        Task<(bool success, string message)> CancelBookingAsync(int bookingId);
        Task<IEnumerable<Seat>> GetAvailableSeatsAsync(int flightId);
        Task<(bool success, string message, Booking booking)> BookFlightAsync(Passenger passenger, Flight flight, Seat seat);
    }
}
