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

            // Add a valid flight
            var flight = new Flight
            {
                FlightId = flightId,
                FlightNumber = "FL123",
                DepartureAirport = "ULN",
                ArrivalAirport = "ICN",
                DepartureTime = DateTime.UtcNow.AddHours(2),
                ArrivalTime = DateTime.UtcNow.AddHours(5),
                Status = FlightStatus.Scheduled
            };
            await _flightRepo.AddFlightAsync(flight);

            // Act
            var (success, message, booking) = await _bookingService.CreateBookingAsync(passengerId, flightId);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Passenger not found"));
            Assert.IsNull(booking);
        }

        [TestMethod]
        public async Task CreateBooking_WhenFlightNotFound_ReturnsFailure()
        {
            // Arrange
            int passengerId = 1;
            int flightId = 999; // Non-existent flight ID

            // Add a valid passenger
            var passenger = new Passenger
            {
                PassengerId = passengerId,
                PassportNumber = "AB123456",
                FirstName = "John",
                LastName = "Doe"
            };
            await _passengerRepo.CreatePassengerAsync(passenger);

            // Act
            var (success, message, booking) = await _bookingService.CreateBookingAsync(passengerId, flightId);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Flight not found"));
            Assert.IsNull(booking);
        }

        [TestMethod]
        public async Task CreateBooking_WhenSuccessful_ReturnsBooking()
        {
            // Arrange
            int passengerId = 1;
            int flightId = 1;

            var passenger = new Passenger
            {
                PassengerId = passengerId,
                PassportNumber = "AB123456",
                FirstName = "John",
                LastName = "Doe"
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
                Status = FlightStatus.Scheduled
            };
            await _flightRepo.AddFlightAsync(flight);

            // Act
            var (success, message, booking) = await _bookingService.CreateBookingAsync(passengerId, flightId);

            // Assert
            Assert.IsTrue(success);
            Assert.IsTrue(message.Contains("Booking created successfully"));
            Assert.IsNotNull(booking);
            Assert.AreEqual(passengerId, booking.PassengerId);
            Assert.AreEqual(flightId, booking.FlightId);
            Assert.IsFalse(booking.IsCheckedIn);
        }

        [TestMethod]
        public async Task CreateBooking_WhenBookingAlreadyExists_ReturnsFailure()
        {
            // Arrange
            int passengerId = 1;
            int flightId = 1;

            var passenger = new Passenger
            {
                PassengerId = passengerId,
                PassportNumber = "AB123456",
                FirstName = "John",
                LastName = "Doe"
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
                Status = FlightStatus.Scheduled
            };
            await _flightRepo.AddFlightAsync(flight);

            // Create an existing booking
            var existingBooking = new Booking
            {
                BookingId = 1,
                PassengerId = passengerId,
                FlightId = flightId,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };
            await _bookingRepo.CreateBookingAsync(existingBooking);

            // Act
            var (success, message, booking) = await _bookingService.CreateBookingAsync(passengerId, flightId);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Passenger already has a booking for this flight"));
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
                FlightId = 1,
                FlightNumber = "FL123",
                DepartureAirport = "ULN",
                ArrivalAirport = "ICN",
                DepartureTime = DateTime.UtcNow.AddHours(2),
                ArrivalTime = DateTime.UtcNow.AddHours(5),
                Status = FlightStatus.Scheduled
            };

            var seat = new Seat
            {
                SeatId = 1,
                FlightId = 1,
                SeatNumber = "12A",
                IsBooked = false,
                Class = "Economy",
                Price = 200.00m
            };

            // Add test data
            await _flightRepo.AddFlightAsync(flight);
            await _seatRepo.AddSeatAsync(seat);

            // Act
            var (success, message, booking) = await _bookingService.BookFlightAsync(passenger, flight, seat);

            // Assert
            Assert.IsTrue(success);
            Assert.IsTrue(message.Contains("Booking created successfully"));
            Assert.IsNotNull(booking);
            Assert.AreEqual(seat.SeatId, booking.SeatId);
            Assert.AreEqual(flight.FlightId, booking.FlightId);
            Assert.IsFalse(booking.IsCheckedIn);
        }

        [TestMethod]
        public async Task BookFlight_WhenPassengerIsNull_ReturnsFailure()
        {
            // Arrange
            Passenger passenger = null;
            var flight = new Flight { FlightId = 1 };
            var seat = new Seat { SeatId = 1 };

            // Act
            var (success, message, booking) = await _bookingService.BookFlightAsync(passenger, flight, seat);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Passenger information is required"));
            Assert.IsNull(booking);
        }

        [TestMethod]
        public async Task BookFlight_WhenFlightIsNull_ReturnsFailure()
        {
            // Arrange
            var passenger = new Passenger { PassportNumber = "AB123456", FirstName = "John", LastName = "Doe" };
            Flight flight = null;
            var seat = new Seat { SeatId = 1 };

            // Act
            var (success, message, booking) = await _bookingService.BookFlightAsync(passenger, flight, seat);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Flight information is required"));
            Assert.IsNull(booking);
        }

        [TestMethod]
        public async Task BookFlight_WhenSeatIsNull_ReturnsFailure()
        {
            // Arrange
            var passenger = new Passenger { PassportNumber = "AB123456", FirstName = "John", LastName = "Doe" };
            var flight = new Flight { FlightId = 1 };
            Seat seat = null;

            // Act
            var (success, message, booking) = await _bookingService.BookFlightAsync(passenger, flight, seat);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Seat selection is required"));
            Assert.IsNull(booking);
        }

        [TestMethod]
        public async Task BookFlight_WhenSeatNotAvailable_ReturnsFailure()
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
                FlightId = 1,
                FlightNumber = "FL123",
                DepartureAirport = "ULN",
                ArrivalAirport = "ICN",
                DepartureTime = DateTime.UtcNow.AddHours(2),
                ArrivalTime = DateTime.UtcNow.AddHours(5),
                Status = FlightStatus.Scheduled
            };

            var seat = new Seat
            {
                SeatId = 1,
                FlightId = 1,
                SeatNumber = "12A",
                IsBooked = true, // Seat is already booked
                Class = "Economy",
                Price = 200.00m
            };

            await _flightRepo.AddFlightAsync(flight);
            await _seatRepo.AddSeatAsync(seat);

            // Act
            var (success, message, booking) = await _bookingService.BookFlightAsync(passenger, flight, seat);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Selected seat is no longer available"));
            Assert.IsNull(booking);
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
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };
            await _bookingRepo.CreateBookingAsync(booking);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(booking.BookingId, result.BookingId);
            Assert.AreEqual(booking.PassengerId, result.PassengerId);
            Assert.AreEqual(booking.FlightId, result.FlightId);
        }

        [TestMethod]
        public async Task GetBookingById_WhenBookingNotExists_ReturnsNull()
        {
            // Arrange
            int nonExistentBookingId = 999;

            // Act
            var result = await _bookingService.GetBookingByIdAsync(nonExistentBookingId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAvailableFlights_ReturnsAllFlights()
        {
            // Arrange
            var flight1 = new Flight
            {
                FlightId = 1,
                FlightNumber = "FL123",
                DepartureAirport = "ULN",
                ArrivalAirport = "ICN",
                DepartureTime = DateTime.UtcNow.AddHours(2),
                ArrivalTime = DateTime.UtcNow.AddHours(5),
                Status = FlightStatus.Scheduled
            };

            var flight2 = new Flight
            {
                FlightId = 2,
                FlightNumber = "FL456",
                DepartureAirport = "ICN",
                ArrivalAirport = "ULN",
                DepartureTime = DateTime.UtcNow.AddHours(8),
                ArrivalTime = DateTime.UtcNow.AddHours(11),
                Status = FlightStatus.Scheduled
            };

            await _flightRepo.AddFlightAsync(flight1);
            await _flightRepo.AddFlightAsync(flight2);

            // Act
            var result = await _bookingService.GetAvailableFlightsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(f => f.FlightNumber == "FL123"));
            Assert.IsTrue(result.Any(f => f.FlightNumber == "FL456"));
        }

        [TestMethod]
        public async Task GetAvailableSeats_ReturnsUnbookedSeats()
        {
            // Arrange
            int flightId = 1;

            var seat1 = new Seat
            {
                SeatId = 1,
                FlightId = flightId,
                SeatNumber = "12A",
                IsBooked = false,
                Class = "Economy",
                Price = 200.00m
            };

            var seat2 = new Seat
            {
                SeatId = 2,
                FlightId = flightId,
                SeatNumber = "12B",
                IsBooked = true, // This seat is booked
                Class = "Economy",
                Price = 200.00m
            };

            var seat3 = new Seat
            {
                SeatId = 3,
                FlightId = flightId,
                SeatNumber = "12C",
                IsBooked = false,
                Class = "Economy",
                Price = 200.00m
            };

            await _seatRepo.AddSeatAsync(seat1);
            await _seatRepo.AddSeatAsync(seat2);
            await _seatRepo.AddSeatAsync(seat3);

            // Act
            var result = await _bookingService.GetAvailableSeatsAsync(flightId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count()); // Only 2 unbooked seats
            Assert.IsTrue(result.Any(s => s.SeatNumber == "12A"));
            Assert.IsTrue(result.Any(s => s.SeatNumber == "12C"));
            Assert.IsFalse(result.Any(s => s.SeatNumber == "12B")); // Booked seat should not be included
        }

        [TestMethod]
        public async Task CancelBooking_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            var booking = new Booking
            {
                BookingId = 1,
                PassengerId = 1,
                FlightId = 1,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };
            await _bookingRepo.CreateBookingAsync(booking);

            // Act
            var (success, message) = await _bookingService.CancelBookingAsync(1);

            // Assert
            Assert.IsTrue(success);
            Assert.IsTrue(message.Contains("Booking cancelled successfully"));

            // Verify booking is actually deleted
            var deletedBooking = await _bookingService.GetBookingByIdAsync(1);
            Assert.IsNull(deletedBooking);
        }

        [TestMethod]
        public async Task CancelBooking_WhenBookingNotFound_ReturnsFalse()
        {
            // Arrange
            int nonExistentBookingId = 999;

            // Act
            var (success, message) = await _bookingService.CancelBookingAsync(nonExistentBookingId);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Booking not found"));
        }

        [TestMethod]
        public async Task CancelBooking_WhenAlreadyCheckedIn_ReturnsFalse()
        {
            // Arrange
            var booking = new Booking
            {
                BookingId = 1,
                PassengerId = 1,
                FlightId = 1,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = true // Already checked in
            };
            await _bookingRepo.CreateBookingAsync(booking);

            // Act
            var (success, message) = await _bookingService.CancelBookingAsync(1);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Cannot cancel a booking that has already been checked in"));
        }

        [TestMethod]
        public async Task GetBookingsByPassengerId_ReturnsCorrectBookings()
        {
            // Arrange
            int passengerId = 1;

            var booking1 = new Booking
            {
                BookingId = 1,
                PassengerId = passengerId,
                FlightId = 1,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };

            var booking2 = new Booking
            {
                BookingId = 2,
                PassengerId = passengerId,
                FlightId = 2,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };

            var booking3 = new Booking
            {
                BookingId = 3,
                PassengerId = 2, // Different passenger
                FlightId = 1,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };

            await _bookingRepo.CreateBookingAsync(booking1);
            await _bookingRepo.CreateBookingAsync(booking2);
            await _bookingRepo.CreateBookingAsync(booking3);

            // Act
            var result = await _bookingService.GetBookingsByPassengerIdAsync(passengerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count()); // Only bookings for passenger 1
            Assert.IsTrue(result.All(b => b.PassengerId == passengerId));
        }

        [TestMethod]
        public async Task GetBookingsByFlightId_ReturnsCorrectBookings()
        {
            // Arrange
            int flightId = 1;

            var booking1 = new Booking
            {
                BookingId = 1,
                PassengerId = 1,
                FlightId = flightId,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };

            var booking2 = new Booking
            {
                BookingId = 2,
                PassengerId = 2,
                FlightId = flightId,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };

            var booking3 = new Booking
            {
                BookingId = 3,
                PassengerId = 1,
                FlightId = 2, // Different flight
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };

            await _bookingRepo.CreateBookingAsync(booking1);
            await _bookingRepo.CreateBookingAsync(booking2);
            await _bookingRepo.CreateBookingAsync(booking3);

            // Act
            var result = await _bookingService.GetBookingsByFlightIdAsync(flightId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count()); // Only bookings for flight 1
            Assert.IsTrue(result.All(b => b.FlightId == flightId));
        }
    }

}