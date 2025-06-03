using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Core.Enums;
using FlightCheckInSystem.Data.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FlightCheckInSystem.Business.Services
{
    public class FlightManagementService : IFlightManagementService
    {
        private readonly IFlightRepository _flightRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IBookingRepository _bookingRepository;

        public FlightManagementService(
            IFlightRepository flightRepository,
            ISeatRepository seatRepository,
            IBookingRepository bookingRepository)
        {
            _flightRepository = flightRepository;
            _seatRepository = seatRepository;
            _bookingRepository = bookingRepository;
        }
        
      

        public async Task<IEnumerable<Flight>> GetAllFlightsAsync()
        {
            return await _flightRepository.GetAllFlightsAsync();
        }

        public async Task<Flight> GetFlightDetailsAsync(int flightId)
        {
            var flight = await _flightRepository.GetFlightByIdAsync(flightId);
            if (flight != null)
            {
                flight.Seats = (await _seatRepository.GetSeatsByFlightIdAsync(flightId)).ToList();
                flight.Bookings = (await _bookingRepository.GetBookingsByFlightIdAsync(flightId)).ToList();
            }
            return flight;
        }

        public async Task<(bool success, string message, Flight createdFlight)> CreateFlightWithSeatLayoutAsync(
            string flightNumber, string departureAirport, string arrivalAirport,
            DateTime departureTime, DateTime arrivalTime,
            int totalRows, char lastSeatLetterInRow)
        {
            if (string.IsNullOrWhiteSpace(flightNumber) || string.IsNullOrWhiteSpace(departureAirport) || string.IsNullOrWhiteSpace(arrivalAirport))
            {
                return (false, "Flight details cannot be empty.", null);
            }
            if (departureTime >= arrivalTime)
            {
                return (false, "Departure time must be before arrival time.", null);
            }
            if (totalRows <= 0 || lastSeatLetterInRow < 'A')
            {
                return (false, "Invalid seat layout parameters.", null);
            }

            var flight = new Flight
            {
                FlightNumber = flightNumber,
                DepartureAirport = departureAirport,
                ArrivalAirport = arrivalAirport,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                Status = FlightStatus.Scheduled
            };

            try
            {
                await _flightRepository.CreateFlightWithSeatsAsync(flight, totalRows, lastSeatLetterInRow);
                                return (true, "Flight and seat layout created successfully.", flight);
            }
            catch (Exception ex)
            {
                                return (false, $"Failed to create flight: {ex.Message}", null);
            }
        }

        public async Task<IEnumerable<Passenger>> GetPassengersByFlightAsync(int flightId)
        {
            var bookings = await _bookingRepository.GetBookingsByFlightIdAsync(flightId);
                        return bookings.Where(b => b.Passenger != null).Select(b => b.Passenger).Distinct();
        }
    }
}