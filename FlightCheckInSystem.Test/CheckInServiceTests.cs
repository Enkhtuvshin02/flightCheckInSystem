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
        public async Task AssignSeatToBooking_WhenSeatIsAlreadyTaken_HandlesRaceConditionGracefully()
        {
            // Arrange
            int bookingId = 1;
            int seatId = 1;

            var booking = new Booking
            {
                BookingId = bookingId,
                FlightId = 1,
                IsCheckedIn = false
            };
            await _bookingRepo.CreateBookingAsync(booking);

            var seat = new Seat
            {
                Id = seatId,
                FlightId = 1,
                IsBooked = true  // Seat is already taken
            };
            await _seatRepo.AddSeatAsync(seat);

            // Act
            var (success, message, boardingPass) = await _checkInService.AssignSeatToBookingAsync(bookingId, seatId);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(message.Contains("Failed to book the seat"));
            Assert.IsNull(boardingPass);
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
                SeatId = 1
                // Missing Passenger and Flight data
            };
            await _bookingRepo.CreateBookingAsync(incompleteBooking);

            // Act
            var boardingPass = await _checkInService.GenerateBoardingPassAsync(bookingId);

            // Assert
            Assert.IsNull(boardingPass);
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
                Id = passengerId,
                FirstName = "John", 
                LastName = "Doe", 
                PassportNumber = "AB123456" 
            };
            await _passengerRepo.CreatePassengerAsync(passenger);

            var flight = new Flight
            {
                Id = flightId,
                FlightNumber = "FL123",
                DepartureAirport = "LAX",
                ArrivalAirport = "JFK",
                DepartureTime = DateTime.Now.AddHours(2)
            };
            await _flightRepo.AddFlightAsync(flight);

            var seat = new Seat
            {
                Id = seatId,
                FlightId = flightId,
                IsBooked = false,
                SeatNumber = "12A"
            };
            await _seatRepo.AddSeatAsync(seat);

            var booking = new Booking
            {
                BookingId = bookingId,
                FlightId = flightId,
                PassengerId = passengerId,
                IsCheckedIn = false,
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
        }

        [TestMethod]
        public async Task FindBookingForCheckIn_WithValidData_ReturnsBooking()
        {
            // Arrange
            string passportNumber = "AB123456";
            string flightNumber = "FL123";

            var passenger = new Passenger
            {
                Id = 1,
                PassportNumber = passportNumber
            };
            await _passengerRepo.CreatePassengerAsync(passenger);

            var flight = new Flight
            {
                Id = 1,
                FlightNumber = flightNumber,
                Status = FlightStatus.CheckingIn
            };
            await _flightRepo.AddFlightAsync(flight);

            var booking = new Booking
            {
                BookingId = 1,
                PassengerId = passenger.Id,
                FlightId = flight.Id,
                IsCheckedIn = false
            };
            await _bookingRepo.CreateBookingAsync(booking);

            // Act
            var (resultBooking, message) = await _checkInService.FindBookingForCheckInAsync(passportNumber, flightNumber);

            // Assert
            Assert.IsNotNull(resultBooking);
            Assert.AreEqual(booking.BookingId, resultBooking.BookingId);
            Assert.IsTrue(message.Contains("Booking found"));
        }

        [TestMethod]
        public async Task FindBookingForCheckIn_WhenFlightClosed_ReturnsAppropriateMessage()
        {
            // Arrange
            string passportNumber = "AB123456";
            string flightNumber = "FL123";

            var passenger = new Passenger
            {
                Id = 1,
                PassportNumber = passportNumber
            };
            await _passengerRepo.CreatePassengerAsync(passenger);

            var flight = new Flight
            {
                Id = 1,
                FlightNumber = flightNumber,
                Status = FlightStatus.Departed
            };
            await _flightRepo.AddFlightAsync(flight);

            var booking = new Booking
            {
                BookingId = 1,
                PassengerId = passenger.Id,
                FlightId = flight.Id,
                IsCheckedIn = false
            };
            await _bookingRepo.CreateBookingAsync(booking);

            // Act
            var (resultBooking, message) = await _checkInService.FindBookingForCheckInAsync(passportNumber, flightNumber);

            // Assert
            Assert.IsNotNull(resultBooking);
            Assert.IsTrue(message.Contains("not currently open for check-in"));
        }
    }

    // Note: The test repository classes (TestBookingRepository, TestSeatRepository, etc.) 
    // are already defined in BookingServiceTests.cs
} 