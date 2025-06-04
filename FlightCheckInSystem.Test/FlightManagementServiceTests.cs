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
    public class FlightManagementServiceTests
    {
        private TestFlightRepository _flightRepo;
        private TestSeatRepository _seatRepo;
        private TestBookingRepository _bookingRepo;
        private FlightManagementService _flightManagementService;

        [TestInitialize]
        public void Setup()
        {
            _flightRepo = new TestFlightRepository();
            _seatRepo = new TestSeatRepository();
            _bookingRepo = new TestBookingRepository();

            _flightManagementService = new FlightManagementService(
                _flightRepo,
                _seatRepo,
                _bookingRepo
            );
        }

        [TestMethod]
        public async Task GetAllFlights_WhenFlightsExist_ReturnsAllFlights()
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
            var result = await _flightManagementService.GetAllFlightsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(f => f.FlightNumber == "FL123"));
            Assert.IsTrue(result.Any(f => f.FlightNumber == "FL456"));
        }

        [TestMethod]
        public async Task GetAllFlights_WhenNoFlights_ReturnsEmptyCollection()
        {
            // Act
            var result = await _flightManagementService.GetAllFlightsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetFlightDetails_WhenFlightExists_ReturnsFlightWithSeatsAndBookings()
        {
            // Arrange
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
            await _flightRepo.AddFlightAsync(flight);

            var seat = new Seat
            {
                SeatId = 1,
                FlightId = flight.FlightId,
                SeatNumber = "12A",
                IsBooked = false,
                Class = "Economy",
                Price = 200.00m
            };
            await _seatRepo.AddSeatAsync(seat);

            var booking = new Booking
            {
                BookingId = 1,
                FlightId = flight.FlightId,
                PassengerId = 1,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };
            await _bookingRepo.CreateBookingAsync(booking);

            // Act
            var result = await _flightManagementService.GetFlightDetailsAsync(flight.FlightId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(flight.FlightId, result.FlightId);
            Assert.AreEqual(flight.FlightNumber, result.FlightNumber);
            Assert.IsNotNull(result.Seats);
            Assert.AreEqual(1, result.Seats.Count);
            Assert.IsNotNull(result.Bookings);
            Assert.AreEqual(1, result.Bookings.Count);
        }

        [TestMethod]
        public async Task GetFlightDetails_WhenFlightNotExists_ReturnsNull()
        {
            // Arrange
            int nonExistentFlightId = 999;

            // Act
            var result = await _flightManagementService.GetFlightDetailsAsync(nonExistentFlightId);

            // Assert
            Assert.IsNull(result);
        }

       
      



        [TestMethod]
        public async Task GetPassengersByFlight_WhenBookingsExist_ReturnsDistinctPassengers()
        {
            // Arrange
            int flightId = 1;

            var passenger1 = new Passenger
            {
                PassengerId = 1,
                PassportNumber = "AB123456",
                FirstName = "John",
                LastName = "Doe"
            };

            var passenger2 = new Passenger
            {
                PassengerId = 2,
                PassportNumber = "CD789012",
                FirstName = "Jane",
                LastName = "Smith"
            };

            var booking1 = new Booking
            {
                BookingId = 1,
                FlightId = flightId,
                PassengerId = passenger1.PassengerId,
                Passenger = passenger1,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };

            var booking2 = new Booking
            {
                BookingId = 2,
                FlightId = flightId,
                PassengerId = passenger2.PassengerId,
                Passenger = passenger2,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };

            // Duplicate booking for same passenger (should still return distinct passengers)
            var booking3 = new Booking
            {
                BookingId = 3,
                FlightId = flightId,
                PassengerId = passenger1.PassengerId,
                Passenger = passenger1,
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };

            await _bookingRepo.CreateBookingAsync(booking1);
            await _bookingRepo.CreateBookingAsync(booking2);
            await _bookingRepo.CreateBookingAsync(booking3);

            // Act
            var result = await _flightManagementService.GetPassengersByFlightAsync(flightId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count()); // Should return distinct passengers only
            Assert.IsTrue(result.Any(p => p.PassportNumber == "AB123456"));
            Assert.IsTrue(result.Any(p => p.PassportNumber == "CD789012"));
        }

        [TestMethod]
        public async Task GetPassengersByFlight_WhenNoBookings_ReturnsEmptyCollection()
        {
            // Arrange
            int flightId = 999; // Flight with no bookings

            // Act
            var result = await _flightManagementService.GetPassengersByFlightAsync(flightId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetPassengersByFlight_WhenBookingsWithoutPassengers_ReturnsEmptyCollection()
        {
            // Arrange
            int flightId = 1;

            var booking = new Booking
            {
                BookingId = 1,
                FlightId = flightId,
                PassengerId = 1,
                Passenger = null, // No passenger data
                ReservationDate = DateTime.UtcNow,
                IsCheckedIn = false
            };

            await _bookingRepo.CreateBookingAsync(booking);

            // Act
            var result = await _flightManagementService.GetPassengersByFlightAsync(flightId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count()); // Should filter out bookings without passenger data
        }
    }
}