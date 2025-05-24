// FlightCheckInSystem.Business/Services/CheckInService.cs
using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Core.Enums;
using FlightCheckInSystem.Data.Interfaces; // Ensure this using directive is present
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq; // For FirstOrDefault

namespace FlightCheckInSystem.Business.Services
{
    public class CheckInService : ICheckInService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IPassengerRepository _passengerRepository;
        private static readonly object _seatAssignmentLock = new object();

        public CheckInService(
            IBookingRepository bookingRepository,
            ISeatRepository seatRepository,
            IFlightRepository flightRepository,
            IPassengerRepository passengerRepository)
        {
            _bookingRepository = bookingRepository;
            _seatRepository = seatRepository;
            _flightRepository = flightRepository;
            _passengerRepository = passengerRepository;
        }

        public async Task<(Booking booking, string message)> FindBookingForCheckInAsync(string passportNumber, string flightNumber)
        {
            var passenger = await _passengerRepository.GetPassengerByPassportAsync(passportNumber);
            if (passenger == null)
            {
                return (null, "Passenger with this passport number not found.");
            }

            // Find the flight by flight number - requires iterating or a specific repo method
            // For simplicity, assuming flightNumber is unique enough for now, or add a method to IFlightRepository
            var allFlights = await _flightRepository.GetAllFlightsAsync();
            var flight = allFlights.FirstOrDefault(f => f.FlightNumber.Equals(flightNumber, StringComparison.OrdinalIgnoreCase));

            if (flight == null)
            {
                return (null, $"Flight with number {flightNumber} not found.");
            }

            var booking = await _bookingRepository.GetBookingByPassengerAndFlightAsync(passenger.PassengerId, flight.FlightId);
            if (booking == null)
            {
                return (null, "No booking found for this passenger on the specified flight.");
            }
            if (booking.IsCheckedIn)
            {
                // Optionally regenerate boarding pass if already checked in
                return (booking, "Passenger is already checked in.");
            }
            if (flight.Status != FlightStatus.CheckingIn && flight.Status != FlightStatus.Scheduled && flight.Status != FlightStatus.Boarding && flight.Status != FlightStatus.Delayed)
            {
                return (booking, $"Flight {flight.FlightNumber} is not currently open for check-in. Status: {flight.Status}");
            }

            return (booking, "Booking found and ready for check-in.");
        }

        public async Task<IEnumerable<Seat>> GetAvailableSeatsAsync(int flightId)
        {
            return await _seatRepository.GetAvailableSeatsByFlightIdAsync(flightId);
        }

        public async Task<(bool success, string message, BoardingPass boardingPass)> AssignSeatToBookingAsync(int bookingId, int seatId)
        {
            // Critical section for assigning a seat to prevent race conditions
            bool seatSuccessfullyBooked = false;
            bool bookingSuccessfullyUpdated = false;
            BoardingPass generatedBoardingPass = null;

            // Fetch booking and seat details outside the lock if possible, to minimize lock duration
            // However, their state (IsBooked, IsCheckedIn) must be re-verified inside the lock
            Booking booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            if (booking == null) return (false, "Booking not found.", null);
            if (booking.IsCheckedIn) return (false, "Passenger is already checked in for this booking.", await GenerateBoardingPassAsync(bookingId));

            Seat seat = await _seatRepository.GetSeatByIdAsync(seatId);
            if (seat == null) return (false, "Seat not found.", null);
            if (seat.FlightId != booking.FlightId) return (false, "Selected seat does not belong to the booked flight.", null);


            lock (_seatAssignmentLock)
            {
                // Re-verify seat availability and booking status inside the lock
                var currentSeatState = Task.Run(async () => await _seatRepository.GetSeatByIdAsync(seatId)).Result;
                if (currentSeatState == null || currentSeatState.IsBooked)
                {
                    return (false, "Seat is no longer available or has been booked by another agent.", null);
                }

                var currentBookingState = Task.Run(async () => await _bookingRepository.GetBookingByIdAsync(bookingId)).Result;
                if (currentBookingState == null || currentBookingState.IsCheckedIn)
                {
                    // This check is somewhat redundant if the initial check passed, but good for safety
                    return (false, "Booking state changed; passenger might be already checked in.", null);
                }

                // Attempt to book the seat
                seatSuccessfullyBooked = Task.Run(async () => await _seatRepository.BookSeatAsync(seatId, bookingId)).Result;
                if (!seatSuccessfullyBooked)
                {
                    return (false, "Failed to book the seat. It might have been taken concurrently.", null);
                }

                // If seat booking was successful, update the booking record
                currentBookingState.SeatId = seatId;
                currentBookingState.IsCheckedIn = true;
                currentBookingState.CheckInTime = DateTime.UtcNow;
                bookingSuccessfullyUpdated = Task.Run(async () => await _bookingRepository.UpdateBookingAsync(currentBookingState)).Result;

                if (!bookingSuccessfullyUpdated)
                {
                    // Rollback: Unbook the seat if booking update failed
                    Task.Run(async () => await _seatRepository.UnbookSeatAsync(seatId)).Wait(); // Best effort rollback
                    return (false, "Seat was booked, but failed to update booking details. Please try again.", null);
                }

                // Generate boarding pass information
                generatedBoardingPass = Task.Run(async () => await GenerateBoardingPassAsync(bookingId)).Result; // Pass the original bookingId
            } // End lock

            if (seatSuccessfullyBooked && bookingSuccessfullyUpdated)
            {
                return (true, "Check-in successful. Seat assigned.", generatedBoardingPass);
            }
            // This path should ideally not be reached if logic inside lock is correct
            return (false, "An unexpected error occurred during seat assignment.", null);
        }


        public async Task<BoardingPass> GenerateBoardingPassAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            if (booking == null || !booking.IsCheckedIn || !booking.SeatId.HasValue || booking.Passenger == null || booking.Flight == null || booking.Seat == null)
            {
                // If any related entity is null (should be loaded by GetBookingByIdAsync), something is wrong
                return null;
            }

            return new BoardingPass
            {
                PassengerName = $"{booking.Passenger.FirstName} {booking.Passenger.LastName}",
                PassportNumber = booking.Passenger.PassportNumber,
                FlightNumber = booking.Flight.FlightNumber,
                DepartureAirport = booking.Flight.DepartureAirport,
                ArrivalAirport = booking.Flight.ArrivalAirport,
                DepartureTime = booking.Flight.DepartureTime,
                SeatNumber = booking.Seat.SeatNumber,
                BoardingTime = booking.Flight.DepartureTime.AddMinutes(-45), // Example: 45 mins before departure
            };
        }
    }
}