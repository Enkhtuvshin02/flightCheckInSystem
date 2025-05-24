// FlightCheckInSystem.Business/Interfaces/ICheckInService.cs
using FlightCheckInSystem.Core.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FlightCheckInSystem.Business.Interfaces
{
    public interface ICheckInService
    {
        Task<(Booking booking, string message)> FindBookingForCheckInAsync(string passportNumber, string flightNumber);
        Task<IEnumerable<Seat>> GetAvailableSeatsAsync(int flightId);
        Task<(bool success, string message, BoardingPass boardingPass)> AssignSeatToBookingAsync(int bookingId, int seatId);
        Task<BoardingPass> GenerateBoardingPassAsync(int bookingId);
    }
}