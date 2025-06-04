using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlightCheckInSystem.Business.Services;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Data.Interfaces;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FlightCheckInSystem.Core.Enums;
using System.Linq;

namespace FlightCheckInSystem.Tests
{
    [TestClass]
    public class CheckInServiceTests
    {
        private TestBookingRepository _bookingRepo;
        private TestSeatRepository _seatRepo;
        private TestFlightRepository _flightRepo;
        private TestPassengerRepository _passengerRepo;
        private CheckInService _checkInService;

        [TestInitialize]
        public void Setup()
        {
            _bookingRepo = new TestBookingRepository();
            _seatRepo = new TestSeatRepository();
            _flightRepo = new TestFlightRepository();
            _passengerRepo = new TestPassengerRepository();

            _checkInService = new CheckInService(
                _bookingRepo,
                _seatRepo,
                _flightRepo,
                _passengerRepo
            );
        }

        [TestMethod]
        public async Task FindBookingForCheckIn_WithValidData_ReturnsBooking()
        {
            // Arrange
            string passportNumber = "AB123456";
            string flightNumber = "FL123";

            var passenger = new Passenger
            {
                PassengerId = 1,
                PassportNumber = passportNumber,
                FirstName = "John",
                LastName = "Doe"
            };
            await _passengerRepo.CreatePassengerAsync(passenger);

            var flight = new Flight
            {
                FlightId = 1,
                FlightNumber = flightNumber,
                Status = FlightStatus.CheckingIn,
                DepartureAirport = "ULN",
                ArrivalAirport = "ICN",
                DepartureTime = DateTime.UtcNow.AddHours(2),
                ArrivalTime = DateTime.UtcNow.AddHours(5)
            };
            await _flightRepo.AddFlightAsync(flight);

            var booking = new Booking
            {
                BookingId = 1,
                PassengerId = passenger.PassengerId,
                FlightId = flight.FlightId,
                IsCheckedIn = false,
                ReservationDate = DateTime.UtcNow
            };
            await _bookingRepo.CreateBookingAsync(booking);

            // Act
            var (resultBooking, message) = await _checkInService.FindBookingForCheckInAsync(passportNumber, flightNumber);

            // Assert
            Assert.IsNotNull(resultBooking);
            Assert.AreEqual(booking.BookingId, resultBooking.BookingId);
            Assert.IsTrue(message.Contains("Booking found and ready for check-in"));
        }

        [TestMethod]
        public async Task AssignSeatToBooking_WhenSuccessful_GeneratesBoardingPass()
        {
            // Arrange
            int bookingId = 1;
            int seatId = 1;
            int flightId = 1;
            int passengerId = 1;

            var passenger = new Passenger
            {
                PassengerId = passengerId,
                FirstName = "John",
                LastName = "Doe",
                PassportNumber = "AB123456"
            };
            await _passengerRepo.CreatePassengerAsync(passenger);

            var flight = new Flight
            {
                FlightId = flightId,
                FlightNumber = "FL123",
                DepartureAirport = "ULN",
                ArrivalAirport = "ICN",
                DepartureTime = DateTime.UtcNow.AddHours(2),
                ArrivalTime = DateTime.UtcNow.AddHours(5),
                Status = FlightStatus.CheckingIn
            };
            await _flightRepo.AddFlightAsync(flight);

            var seat = new Seat
            {
                SeatId = seatId,
                FlightId = flightId,
                IsBooked = false,
                SeatNumber = "12A",
                Class = "Economy",
                Price = 200.00m
            };
            await _seatRepo.AddSeatAsync(seat);

            var booking = new Booking
            {
                BookingId = bookingId,
                FlightId = flightId,
                PassengerId = passengerId,
                IsCheckedIn = false,
                ReservationDate = DateTime.UtcNow,
                Passenger = passenger,
                Flight = flight
            };
            await _bookingRepo.CreateBookingAsync(booking);

            // Act
            var (success, message, boardingPass) = await _checkInService.AssignSeatToBookingAsync(bookingId, seatId);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNotNull(boardingPass);
            Assert.AreEqual("12A", boardingPass.SeatNumber);
            Assert.AreEqual("John Doe", boardingPass.PassengerName);
            Assert.AreEqual("FL123", boardingPass.FlightNumber);
            Assert.AreEqual("ULN", boardingPass.DepartureAirport);
            Assert.AreEqual("ICN", boardingPass.ArrivalAirport);
        }

        [TestMethod]
        public async Task AssignSeatToBooking_WhenSeatIsAlreadyTaken_HandlesRaceConditionGracefully()
        {
            // Arrange
            int bookingId = 1;
            int seatId = 1;
            int flightId = 1;
            int passengerId = 1;

            var passenger = new Passenger
            {
                PassengerId = passengerId,
                FirstName = "John",
                LastName = "Doe",
                PassportNumber = "AB123456"
            };
            await _passengerRepo.CreatePassengerAsync(passenger);

            var flight = new Flight
            {
                FlightId = flightId,
                FlightNumber = "FL123",
                DepartureAirport = "ULN",
                ArrivalAirport = "ICN",
                DepartureTime = DateTime.UtcNow.AddHours(2),
                ArrivalTime = DateTime.UtcNow.AddHours(5),
                Status = FlightStatus.CheckingIn
            };
            await _flightRepo.AddFlightAsync(flight);

            var booking = new Booking
            {
                BookingId = bookingId,
                FlightId = flightId,
                PassengerId = passengerId,
                IsCheckedIn = false,
                ReservationDate = DateTime.UtcNow,
                Passenger = passenger,
                Flight = flight
            };
            await _bookingRepo.CreateBookingAsync(booking);

            var seat = new Seat
            {
                SeatId = seatId,
                FlightId = flightId,
                IsBooked = true,  // Seat is already taken
                SeatNumber = "12A",
                Class = "Economy",
                Price = 200.00m
            };
            await _seatRepo.AddSeatAsync(seat);

            // Act
            var (success, message, boardingPass) = await _checkInService.AssignSeatToBookingAsync(bookingId, seatId);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Seat is no longer available") || message.Contains("Failed to book the seat"));
            Assert.IsNull(boardingPass);
        }

        [TestMethod]
        public async Task FindBookingForCheckIn_WhenFlightClosed_ReturnsAppropriateMessage()
        {
            // Arrange
            string passportNumber = "AB123456";
            string flightNumber = "FL123";

            var passenger = new Passenger
            {
                PassengerId = 1,
                PassportNumber = passportNumber,
                FirstName = "John",
                LastName = "Doe"
            };
            await _passengerRepo.CreatePassengerAsync(passenger);

            var flight = new Flight
            {
                FlightId = 1,
                FlightNumber = flightNumber,
                Status = FlightStatus.Departed, // Flight has departed
                DepartureAirport = "ULN",
                ArrivalAirport = "ICN",
                DepartureTime = DateTime.UtcNow.AddHours(-2), // Departed 2 hours ago
                ArrivalTime = DateTime.UtcNow.AddHours(1)
            };
            await _flightRepo.AddFlightAsync(flight);

            var booking = new Booking
            {
                BookingId = 1,
                PassengerId = passenger.PassengerId,
                FlightId = flight.FlightId,
                IsCheckedIn = false,
                ReservationDate = DateTime.UtcNow
            };
            await _bookingRepo.CreateBookingAsync(booking);

            // Act
            var (resultBooking, message) = await _checkInService.FindBookingForCheckInAsync(passportNumber, flightNumber);

            // Assert
            Assert.IsNotNull(resultBooking);
            Assert.IsTrue(message.Contains("not currently open for check-in"));
        }

        [TestMethod]
        public async Task GenerateBoardingPass_WhenBookingDataIsIncomplete_ReturnsNull()
        {
            // Arrange
            int bookingId = 1;
            var incompleteBooking = new Booking
            {
                BookingId = bookingId,
                IsCheckedIn = true,
                SeatId = 1,
                FlightId = 1,
                PassengerId = 1
                // Missing Passenger and Flight navigation properties
            };
            await _bookingRepo.CreateBookingAsync(incompleteBooking);

            // Act
            var boardingPass = await _checkInService.GenerateBoardingPassAsync(bookingId);

            // Assert
            Assert.IsNull(boardingPass);
        }

        [TestMethod]
        public async Task FindBookingForCheckIn_WhenPassengerNotFound_ReturnsNull()
        {
            // Arrange
            string passportNumber = "NOTFOUND";
            string flightNumber = "FL123";

            // Act
            var (resultBooking, message) = await _checkInService.FindBookingForCheckInAsync(passportNumber, flightNumber);

            // Assert
            Assert.IsNull(resultBooking);
            Assert.IsTrue(message.Contains("Passenger with this passport number not found"));
        }

        [TestMethod]
        public async Task FindBookingForCheckIn_WhenFlightNotFound_ReturnsNull()
        {
            // Arrange
            string passportNumber = "AB123456";
            string flightNumber = "NOTFOUND";

            var passenger = new Passenger
            {
                PassengerId = 1,
                PassportNumber = passportNumber,
                FirstName = "John",
                LastName = "Doe"
            };
            await _passengerRepo.CreatePassengerAsync(passenger);

            // Act
            var (resultBooking, message) = await _checkInService.FindBookingForCheckInAsync(passportNumber, flightNumber);

            // Assert
            Assert.IsNull(resultBooking);
            Assert.IsTrue(message.Contains("Flight with number NOTFOUND not found"));
        }

        [TestMethod]
        public async Task AssignSeatToBooking_WhenBookingNotFound_ReturnsFailure()
        {
            // Arrange
            int bookingId = 999; // Non-existent booking
            int seatId = 1;

            // Act
            var (success, message, boardingPass) = await _checkInService.AssignSeatToBookingAsync(bookingId, seatId);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Booking not found"));
            Assert.IsNull(boardingPass);
        }

        [TestMethod]
        public async Task AssignSeatToBooking_WhenSeatNotFound_ReturnsFailure()
        {
            // Arrange
            int bookingId = 1;
            int seatId = 999; // Non-existent seat

            var booking = new Booking
            {
                BookingId = bookingId,
                FlightId = 1,
                PassengerId = 1,
                IsCheckedIn = false,
                ReservationDate = DateTime.UtcNow
            };
            await _bookingRepo.CreateBookingAsync(booking);

            // Act
            var (success, message, boardingPass) = await _checkInService.AssignSeatToBookingAsync(bookingId, seatId);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Seat not found"));
            Assert.IsNull(boardingPass);
        }
    }

    // Corrected Test Repository Classes
    public class TestFlightRepository : IFlightRepository
    {
        private readonly List<Flight> _flights = new List<Flight>();
        private int _nextId = 1;

        public async Task<IEnumerable<Flight>> GetAllFlightsAsync()
        {
            return _flights;
        }

        public async Task<int> AddFlightAsync(Flight flight)
        {
            if (flight.FlightId == 0)
                flight.FlightId = _nextId++;
            _flights.Add(flight);
            return flight.FlightId;
        }

        public async Task<Flight> GetFlightByIdAsync(int flightId)
        {
            return _flights.FirstOrDefault(f => f.FlightId == flightId);
        }

        public async Task<bool> UpdateFlightAsync(Flight flight)
        {
            var existingFlight = _flights.FirstOrDefault(f => f.FlightId == flight.FlightId);
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
            if (flight.FlightId == 0)
                flight.FlightId = _nextId++;
            _flights.Add(flight);
        }

        public async Task<bool> UpdateFlightStatusAsync(int flightId, FlightStatus newStatus)
        {
            var flight = _flights.FirstOrDefault(f => f.FlightId == flightId);
            if (flight != null)
            {
                flight.Status = newStatus;
                return true;
            }
            return false;
        }
    }

    public class TestPassengerRepository : IPassengerRepository
    {
        private readonly List<Passenger> _passengers = new List<Passenger>();
        private int _nextId = 1;

        public async Task<Passenger> GetPassengerByIdAsync(int passengerId)
        {
            return _passengers.FirstOrDefault(p => p.PassengerId == passengerId);
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
            if (passenger.PassengerId == 0)
                passenger.PassengerId = _nextId++;
            _passengers.Add(passenger);
            return passenger;
        }

        public async Task<int> AddPassengerAsync(Passenger passenger)
        {
            if (passenger.PassengerId == 0)
                passenger.PassengerId = _nextId++;
            _passengers.Add(passenger);
            return passenger.PassengerId;
        }
    }

    public class TestBookingRepository : IBookingRepository
    {
        private readonly List<Booking> _bookings = new List<Booking>();
        private int _nextId = 1;

        public async Task<Booking> GetBookingByIdAsync(int bookingId)
        {
            return _bookings.FirstOrDefault(b => b.BookingId == bookingId);
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
            if (booking.BookingId == 0)
                booking.BookingId = _nextId++;
            _bookings.Add(booking);
            return booking.BookingId;
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            if (booking.BookingId == 0)
                booking.BookingId = _nextId++;
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

        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            var booking = _bookings.FirstOrDefault(b => b.BookingId == bookingId);
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
        private int _nextId = 1;

        public async Task<Seat> GetSeatByIdAsync(int seatId)
        {
            return _seats.FirstOrDefault(s => s.SeatId == seatId);
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
            var existingSeat = _seats.FirstOrDefault(s => s.SeatId == seat.SeatId);
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
            var seat = _seats.FirstOrDefault(s => s.SeatId == seatId);
            if (seat != null && !seat.IsBooked)
            {
                seat.IsBooked = true;
                return true;
            }
            return false;
        }

        public async Task<bool> ReleaseSeatAsync(int seatId)
        {
            var seat = _seats.FirstOrDefault(s => s.SeatId == seatId);
            if (seat != null)
            {
                seat.IsBooked = false;
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
                return true;
            }
            return false;
        }

        public async Task<int> AddSeatAsync(Seat seat)
        {
            if (seat.SeatId == 0)
                seat.SeatId = _nextId++;
            _seats.Add(seat);
            return seat.SeatId;
        }

        public async Task<bool> UnbookSeatAsync(int seatId)
        {
            var seat = _seats.FirstOrDefault(s => s.SeatId == seatId);
            if (seat != null)
            {
                seat.IsBooked = false;
                return true;
            }
            return false;
        }
    }
}