using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Core.Enums;
using FlightCheckInSystem.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlightCheckInSystem.Server.Hubs
{
    public class FlightHub : Hub
    {
        private static readonly Dictionary<string, List<Seat>> FlightSeats = new Dictionary<string, List<Seat>>();
        private readonly ISeatRepository _seatRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ILogger<FlightHub> _logger;

        public FlightHub(ISeatRepository seatRepository, IBookingRepository bookingRepository, IFlightRepository flightRepository, ILogger<FlightHub> logger)
        {
            _seatRepository = seatRepository;
            _bookingRepository = bookingRepository;
            _flightRepository = flightRepository;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}. Reason: {exception?.Message ?? "No reason provided"}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SubscribeToFlightStatusBoard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "FlightStatusBoard");
            _logger.LogInformation($"Client {Context.ConnectionId} subscribed to flight status board");
            await Clients.Caller.SendAsync("SubscriptionConfirmed", "FlightStatusBoard");
        }

        public async Task BroadcastFlightStatusUpdate(string flightNumber, FlightStatus newStatus)
        {
            _logger.LogInformation($"Broadcasting flight status update: {flightNumber} - {newStatus}");
            await Clients.Group("FlightStatusBoard").SendAsync("FlightStatusUpdated", flightNumber, newStatus);
            _logger.LogInformation($"Broadcast complete for flight status update: {flightNumber} - {newStatus}");
        }

        public async Task<List<Seat>> GetAvailableSeats(string flightNumber)
        {
            try
            {
                _logger.LogInformation($"Getting available seats for flight {flightNumber}");

                if (!FlightSeats.ContainsKey(flightNumber))
                {
                    await InitializeSeatsForFlight(flightNumber);
                }

                var availableSeats = FlightSeats[flightNumber].FindAll(s => !s.IsBooked);
                _logger.LogInformation($"Found {availableSeats.Count} available seats for flight {flightNumber}");
                return availableSeats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting available seats for flight {flightNumber}");
                return new List<Seat>();
            }
        }

        public async Task<bool> BookSeat(string flightNumber, string seatNumber, string bookingReference)
        {
            try
            {
                _logger.LogInformation($"Attempting to book seat {seatNumber} on flight {flightNumber} with reference {bookingReference}");

                if (!FlightSeats.ContainsKey(flightNumber))
                {
                    await InitializeSeatsForFlight(flightNumber);
                }

                var seats = FlightSeats[flightNumber];
                var seat = seats.Find(s => s.SeatNumber == seatNumber && !s.IsBooked);

                if (seat == null)
                {
                    _logger.LogWarning($"Seat {seatNumber} on flight {flightNumber} not available for booking");
                    return false;
                }

                seat.IsBooked = true;

                var allBookings = await _bookingRepository.GetBookingsByFlightIdAsync(seat.FlightId);
                var booking = allBookings.FirstOrDefault(b => b.BookingReference == bookingReference);

                if (booking != null)
                {
                    booking.SeatId = seat.SeatId;
                    booking.IsCheckedIn = true;
                    await _bookingRepository.UpdateBookingAsync(booking);
                    _logger.LogInformation($"Updated booking {bookingReference} with seat {seatNumber}");
                }
                else
                {
                    _logger.LogWarning($"Booking with reference {bookingReference} not found for flight ID {seat.FlightId}");
                }

                await Clients.Group(flightNumber).SendAsync("SeatBooked", flightNumber, seatNumber, bookingReference);
                _logger.LogInformation($"Seat {seatNumber} on flight {flightNumber} booked with reference {bookingReference}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error booking seat {seatNumber} on flight {flightNumber}");
                return false;
            }
        }

        public async Task<bool> ReleaseSeat(string flightNumber, string seatNumber)
        {
            try
            {
                _logger.LogInformation($"Attempting to release seat {seatNumber} on flight {flightNumber}");

                if (!FlightSeats.ContainsKey(flightNumber))
                {
                    await InitializeSeatsForFlight(flightNumber);
                }

                var seats = FlightSeats[flightNumber];
                var seat = seats.Find(s => s.SeatNumber == seatNumber);

                if (seat == null)
                {
                    _logger.LogWarning($"Seat {seatNumber} on flight {flightNumber} not found for release");
                    return false;
                }

                seat.IsBooked = false;
                await Clients.Group(flightNumber).SendAsync("SeatReleased", flightNumber, seatNumber);
                _logger.LogInformation($"Seat {seatNumber} on flight {flightNumber} released");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error releasing seat {seatNumber} on flight {flightNumber}");
                return false;
            }
        }

        public async Task SubscribeToFlightUpdates(string flightNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, flightNumber);
            _logger.LogInformation($"Client {Context.ConnectionId} subscribed to updates for flight {flightNumber}");
            await Clients.Caller.SendAsync("SubscriptionConfirmed", flightNumber);
        }

        public async Task UnsubscribeFromFlightUpdates(string flightNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, flightNumber);
            _logger.LogInformation($"Client {Context.ConnectionId} unsubscribed from updates for flight {flightNumber}");
        }

        public string Ping()
        {
            _logger.LogInformation($"Ping received from client {Context.ConnectionId}");
            return $"Pong! Server time: {DateTime.Now}";
        }

                private async Task InitializeSeatsForFlight(string flightNumber)
        {
            try
            {
                _logger.LogInformation($"Initializing seats for flight {flightNumber}");

                                                var flights = await _flightRepository.GetAllFlightsAsync();
                var flight = flights.FirstOrDefault(f => f.FlightNumber == flightNumber);

                if (flight == null)
                {
                    _logger.LogWarning($"Flight {flightNumber} not found");
                    FlightSeats[flightNumber] = new List<Seat>();                     return;
                }

                var dbSeats = await _seatRepository.GetSeatsByFlightIdAsync(flight.FlightId);

                if (dbSeats != null && dbSeats.Count() > 0)
                {
                    FlightSeats[flightNumber] = dbSeats.ToList();
                    _logger.LogInformation($"Initialized {dbSeats.Count()} seats for flight {flightNumber} from database");
                }
                else
                {
                                        var seats = new List<Seat>();
                    int seatId = 1;

                                        for (int row = 0; row < 6; row++)
                    {
                        for (int col = 1; col <= 5; col++)
                        {
                            char rowChar = (char)('A' + row);
                            string seatNumber = $"{rowChar}{col}";

                            seats.Add(new Seat
                            {
                                SeatId = seatId++,
                                FlightId = flight.FlightId,                                 SeatNumber = seatNumber,
                                IsBooked = false
                            });
                        }
                    }

                    FlightSeats[flightNumber] = seats;
                    _logger.LogInformation($"Created default 30 seats for flight {flightNumber}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error initializing seats for flight {flightNumber}");
                                FlightSeats[flightNumber] = new List<Seat>();
            }
        }
    }
}