using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightCheckInSystem.Business.Services
{
    public class BookingService : IBookingService
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ISeatRepository _seatRepository;

        public BookingService(
            IFlightRepository flightRepository,
            IPassengerRepository passengerRepository,
            IBookingRepository bookingRepository,
            ISeatRepository seatRepository)
        {
            _flightRepository = flightRepository ?? throw new ArgumentNullException(nameof(flightRepository));
            _passengerRepository = passengerRepository ?? throw new ArgumentNullException(nameof(passengerRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _seatRepository = seatRepository ?? throw new ArgumentNullException(nameof(seatRepository));
        }

        public async Task<IEnumerable<Flight>> GetAvailableFlightsAsync()
        {
            return await _flightRepository.GetAllFlightsAsync();
        }

        public async Task<(bool success, string message, Booking booking)> CreateBookingAsync(int passengerId, int flightId)
        {
                        var passenger = await _passengerRepository.GetPassengerByIdAsync(passengerId);
            if (passenger == null)
            {
                return (false, "Passenger not found", null);
            }

                        var flight = await _flightRepository.GetFlightByIdAsync(flightId);
            if (flight == null)
            {
                return (false, "Flight not found", null);
            }

                        var existingBooking = await _bookingRepository.GetBookingByPassengerAndFlightAsync(passengerId, flightId);
            if (existingBooking != null)
            {
                return (false, "Passenger already has a booking for this flight", null);
            }

                        var booking = new Booking
            {
                PassengerId = passengerId,
                FlightId = flightId,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };

            var bookingId = await _bookingRepository.AddBookingAsync(booking);
            booking.BookingId = bookingId;
            booking.Passenger = passenger;
            booking.Flight = flight;

            return (true, "Booking created successfully", booking);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByPassengerIdAsync(int passengerId)
        {
            return await _bookingRepository.GetBookingsByPassengerIdAsync(passengerId);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByFlightIdAsync(int flightId)
        {
            return await _bookingRepository.GetBookingsByFlightIdAsync(flightId);
        }

        public async Task<Booking> GetBookingByIdAsync(int bookingId)
        {
            return await _bookingRepository.GetBookingByIdAsync(bookingId);
        }

        public async Task<(bool success, string message)> CancelBookingAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return (false, "Booking not found");
            }

            if (booking.IsCheckedIn)
            {
                return (false, "Cannot cancel a booking that has already been checked in");
            }

            var result = await _bookingRepository.DeleteBookingAsync(bookingId);
            return result 
                ? (true, "Booking cancelled successfully") 
                : (false, "Failed to cancel booking");
        }

        public async Task<IEnumerable<Seat>> GetAvailableSeatsAsync(int flightId)
        {
                        var allSeats = await _seatRepository.GetSeatsByFlightIdAsync(flightId);
            
                        var bookings = await _bookingRepository.GetBookingsByFlightIdAsync(flightId);
            
                        var bookedSeatIds = new HashSet<int>(bookings.Where(b => b.SeatId.HasValue).Select(b => b.SeatId.Value));
            
                        return allSeats.Where(s => !bookedSeatIds.Contains(s.Id));
        }

        public async Task<(bool success, string message, Booking booking)> BookFlightAsync(Passenger passenger, Flight flight, Seat seat)
        {
                        if (passenger == null)
                return (false, "Passenger information is required", null);
                
            if (flight == null)
                return (false, "Flight information is required", null);
                
            if (seat == null)
                return (false, "Seat selection is required", null);

                        var existingPassenger = await _passengerRepository.GetPassengerByPassportNumberAsync(passenger.PassportNumber);
            int passengerId;
            
            if (existingPassenger == null)
            {
                                var newPassenger = await _passengerRepository.CreatePassengerAsync(passenger);
                if (newPassenger == null)
                    return (false, "Failed to create passenger record", null);
                    
                passengerId = newPassenger.Id;
            }
            else
            {
                passengerId = existingPassenger.Id;
            }
            
                        var availableSeats = await GetAvailableSeatsAsync(flight.Id);
            if (!availableSeats.Any(s => s.Id == seat.Id))
                return (false, "Selected seat is no longer available", null);
                
                        var booking = new Booking
            {
                PassengerId = passengerId,
                FlightId = flight.Id,
                SeatId = seat.Id,
                BookingReference = GenerateBookingReference(),
                BookingStatus = Core.Enums.BookingStatus.Confirmed,
                BookingDate = DateTime.Now,
                IsCheckedIn = false
            };
            
            var createdBooking = await _bookingRepository.CreateBookingAsync(booking);
            if (createdBooking == null)
                return (false, "Failed to create booking", null);
                
            return (true, "Booking created successfully", createdBooking);
        }
        
        private string GenerateBookingReference()
        {
                        var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}