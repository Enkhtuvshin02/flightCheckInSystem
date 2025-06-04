using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Data.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        public async Task<IEnumerable<Passenger>> GetPassengersByFlightAsync(int flightId)
        {
            var bookings = await _bookingRepository.GetBookingsByFlightIdAsync(flightId);
            return bookings.Where(b => b.Passenger != null).Select(b => b.Passenger).Distinct();
        }
    }
}