using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlightCheckInSystem.Business.Services;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Core.Enums;
using FlightCheckInSystem.Data.Interfaces;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace FlightCheckInSystem.Tests
{
    [TestClass]
    public class BookingServiceTests
    {
        private TestFlightRepository _flightRepo;
        private TestPassengerRepository _passengerRepo;
        private TestBookingRepository _bookingRepo;
        private TestSeatRepository _seatRepo;
        private BookingService _bookingService;

        [TestInitialize]
        public void Setup()
        {
            _flightRepo = new TestFlightRepository();
            _passengerRepo = new TestPassengerRepository();
            _bookingRepo = new TestBookingRepository();
            _seatRepo = new TestSeatRepository();

            _bookingService = new BookingService(
                _flightRepo,
                _passengerRepo,
                _bookingRepo,
                _seatRepo
            );
        }

        [TestMethod]
        public async Task CreateBooking_WhenPassengerNotFound_ReturnsFailure()
        {
            // Arrange
            int passengerId = 999; // Non-existent passenger ID
            int flightId = 1;

            // Act
            var (success, message, booking) = await _bookingService.CreateBookingAsync(passengerId, flightId);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Passenger not found"));
            Assert.IsNull(booking);
        }

        [TestMethod]
        public async Task BookFlight_WhenSuccessful_CreatesBooking()
        {
            // Arrange
            var passenger = new Passenger
            {
                PassportNumber = "AB123456",
                FirstName = "John",
                LastName = "Doe"
            };

            var flight = new Flight
            {
                Id = 1,
                FlightNumber = "FL123"
            };

            var seat = new Seat
            {
                Id = 1,
                FlightId = 1,
                SeatNumber = "12A"
            };

            // Add test data
            await _passengerRepo.CreatePassengerAsync(passenger);
            await _flightRepo.AddFlightAsync(flight);
            await _seatRepo.AddSeatAsync(seat);

            // Act
            var (success, message, booking) = await _bookingService.BookFlightAsync(passenger, flight, seat);

            // Assert
            Assert.IsTrue(success);
            Assert.IsTrue(message.Contains("Booking created successfully"));
            Assert.IsNotNull(booking);
        }

        [TestMethod]
        public async Task GetBookingById_WhenBookingExists_ReturnsBooking()
        {
            // Arrange
            var booking = new Booking
            {
                BookingId = 1,
                PassengerId = 1,
                FlightId = 1,
                BookingReference = "TEST123"
            };
            await _bookingRepo.CreateBookingAsync(booking);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(booking.BookingId, result.BookingId);
            Assert.AreEqual(booking.BookingReference, result.BookingReference);
        }
    }

    // Simple in-memory test repositories
    public class TestFlightRepository : IFlightRepository
    {
        private readonly List<Flight> _flights = new List<Flight>();

        public async Task<List<Flight>> GetAllFlightsAsync()
        {
            return _flights;
        }

        public async Task<int> AddFlightAsync(Flight flight)
        {
            _flights.Add(flight);
            return flight.Id;
        }

        public async Task<Flight> GetFlightByIdAsync(int flightId)
        {
            return _flights.FirstOrDefault(f => f.Id == flightId);
        }

        public async Task<bool> UpdateFlightAsync(Flight flight)
        {
            var existingFlight = _flights.FirstOrDefault(f => f.Id == flight.Id);
            if (existingFlight != null)
            {
                _flights.Remove(existingFlight);
                _flights.Add(flight);
                return true;
            }
            return false;
        }

        public async Task CreateFlightWithSeatsAsync(Flight flight, int totalRows, char lastSeatLetterInRow)
        {
            _flights.Add(flight);
        }

        public async Task<bool> UpdateFlightStatusAsync(int flightId, FlightStatus newStatus)
        {
            var flight = _flights.FirstOrDefault(f => f.Id == flightId);
            if (flight != null)
            {
                flight.Status = newStatus;
                return true;
            }
            return false;
        }

        Task<IEnumerable<Flight>> IFlightRepository.GetAllFlightsAsync()
        {
            return Task.FromResult(_flights.AsEnumerable());
        }
    }

    public class TestPassengerRepository : IPassengerRepository
    {
        private readonly List<Passenger> _passengers = new List<Passenger>();
        private int _nextId = 1;

        public async Task<Passenger> GetPassengerByIdAsync(int id)
        {
            return _passengers.FirstOrDefault(p => p.Id == id);
        }

        public async Task<Passenger> GetPassengerByPassportAsync(string passportNumber)
        {
            return _passengers.FirstOrDefault(p => p.PassportNumber == passportNumber);
        }

        public async Task<Passenger> GetPassengerByPassportNumberAsync(string passportNumber)
        {
            return _passengers.FirstOrDefault(p => p.PassportNumber == passportNumber);
        }

        public async Task<Passenger> CreatePassengerAsync(Passenger passenger)
        {
            passenger.Id = _nextId++;
            _passengers.Add(passenger);
            return passenger;
        }

        public async Task<int> AddPassengerAsync(Passenger passenger)
        {
            passenger.Id = _nextId++;
            _passengers.Add(passenger);
            return passenger.Id;
        }
    }

    public class TestBookingRepository : IBookingRepository
    {
        private readonly List<Booking> _bookings = new List<Booking>();

        public async Task<Booking> GetBookingByIdAsync(int id)
        {
            return _bookings.FirstOrDefault(b => b.BookingId == id);
        }

        public async Task<Booking> GetBookingByPassengerAndFlightAsync(int passengerId, int flightId)
        {
            return _bookings.FirstOrDefault(b => b.PassengerId == passengerId && b.FlightId == flightId);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByFlightIdAsync(int flightId)
        {
            return _bookings.Where(b => b.FlightId == flightId);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByPassengerIdAsync(int passengerId)
        {
            return _bookings.Where(b => b.PassengerId == passengerId);
        }

        public async Task<int> AddBookingAsync(Booking booking)
        {
            _bookings.Add(booking);
            return booking.BookingId;
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            _bookings.Add(booking);
            return booking;
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            var existingBooking = _bookings.FirstOrDefault(b => b.BookingId == booking.BookingId);
            if (existingBooking != null)
            {
                _bookings.Remove(existingBooking);
                _bookings.Add(booking);
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            var booking = _bookings.FirstOrDefault(b => b.BookingId == id);
            if (booking != null)
            {
                _bookings.Remove(booking);
                return true;
            }
            return false;
        }

        public async Task<Booking> GetBookingBySeatIdAsync(int seatId)
        {
            return _bookings.FirstOrDefault(b => b.SeatId == seatId);
        }
    }

    public class TestSeatRepository : ISeatRepository
    {
        private readonly List<Seat> _seats = new List<Seat>();

        public async Task<Seat> GetSeatByIdAsync(int seatId)
        {
            return _seats.FirstOrDefault(s => s.Id == seatId);
        }

        public async Task<Seat> GetSeatByNumberAndFlightAsync(string seatNumber, int flightId)
        {
            return _seats.FirstOrDefault(s => s.SeatNumber == seatNumber && s.FlightId == flightId);
        }

        public async Task<IEnumerable<Seat>> GetSeatsByFlightIdAsync(int flightId)
        {
            return _seats.Where(s => s.FlightId == flightId);
        }

        public async Task<IEnumerable<Seat>> GetAvailableSeatsByFlightIdAsync(int flightId)
        {
            return _seats.Where(s => s.FlightId == flightId && !s.IsBooked);
        }

        public async Task<bool> UpdateSeatAsync(Seat seat)
        {
            var existingSeat = _seats.FirstOrDefault(s => s.Id == seat.Id);
            if (existingSeat != null)
            {
                _seats.Remove(existingSeat);
                _seats.Add(seat);
                return true;
            }
            return false;
        }

        public async Task<bool> BookSeatAsync(int seatId, int bookingId)
        {
            var seat = _seats.FirstOrDefault(s => s.Id == seatId);
            if (seat != null && !seat.IsBooked)
            {
                seat.IsBooked = true;
                seat.BookingId = bookingId;
                return true;
            }
            return false;
        }

        public async Task<bool> ReleaseSeatAsync(int seatId)
        {
            var seat = _seats.FirstOrDefault(s => s.Id == seatId);
            if (seat != null)
            {
                seat.IsBooked = false;
                seat.BookingId = null;
                return true;
            }
            return false;
        }

        public async Task<bool> BookSeatByNumberAsync(string seatNumber, int flightId, int bookingId)
        {
            var seat = _seats.FirstOrDefault(s => s.SeatNumber == seatNumber && s.FlightId == flightId);
            if (seat != null && !seat.IsBooked)
            {
                seat.IsBooked = true;
                seat.BookingId = bookingId;
                return true;
            }
            return false;
        }

        public async Task<int> AddSeatAsync(Seat seat)
        {
            _seats.Add(seat);
            return seat.Id;
        }

        public async Task<bool> UnbookSeatAsync(int seatId)
        {
            var seat = _seats.FirstOrDefault(s => s.Id == seatId);
            if (seat != null)
            {
                seat.IsBooked = false;
                return true;
            }
            return false;
        }
    }
}