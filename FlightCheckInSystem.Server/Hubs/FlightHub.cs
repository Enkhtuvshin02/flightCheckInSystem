using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Core.Enums;
using FlightCheckInSystem.Data.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FlightCheckInSystem.Server.Hubs
{
    public class FlightHub : Hub
    {
        private static readonly Dictionary<string, List<Seat>> FlightSeats = new Dictionary<string, List<Seat>>();

        // Track temporary seat reservations (not permanent bookings)
        private static readonly Dictionary<string, Dictionary<string, SeatReservation>> SeatReservations =
            new Dictionary<string, Dictionary<string, SeatReservation>>();

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

            // Clean up any reservations held by this client
            await CleanupClientReservations(Context.ConnectionId);

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

        public async Task SubscribeToFlightUpdates(string flightNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Flight_{flightNumber}");
            _logger.LogInformation($"Client {Context.ConnectionId} subscribed to updates for flight {flightNumber}");
            await Clients.Caller.SendAsync("SubscriptionConfirmed", flightNumber);
        }

        public async Task UnsubscribeFromFlightUpdates(string flightNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Flight_{flightNumber}");
            _logger.LogInformation($"Client {Context.ConnectionId} unsubscribed from updates for flight {flightNumber}");
        }

        public async Task GetFlightSeatsAsync(int flightId)
        {
            try
            {
                _logger.LogInformation($"Getting seats for flight ID {flightId}");

                var flight = await _flightRepository.GetFlightByIdAsync(flightId);
                if (flight == null)
                {
                    _logger.LogWarning($"Flight with ID {flightId} not found");
                    await Clients.Caller.SendAsync("ReceiveFlightSeats", "", "[]");
                    return;
                }

                var seats = await _seatRepository.GetSeatsByFlightIdAsync(flightId);
                var seatList = seats?.ToList() ?? new List<Seat>();

                // Update local cache
                FlightSeats[flight.FlightNumber] = seatList;

                // Apply temporary reservations to the seat data
                var seatsWithReservations = ApplyReservationsToSeats(flight.FlightNumber, seatList);

                var seatDataJson = JsonConvert.SerializeObject(seatsWithReservations);
                await Clients.Caller.SendAsync("ReceiveFlightSeats", flight.FlightNumber, seatDataJson);

                _logger.LogInformation($"Sent {seatList.Count} seats for flight {flight.FlightNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting seats for flight ID {flightId}");
                await Clients.Caller.SendAsync("ReceiveFlightSeats", "", "[]");
            }
        }

        // Reserve a seat temporarily (not permanent booking)
        public async Task ReserveSeatAsync(int flightId, string seatNumber, string bookingReference)
        {
            try
            {
                _logger.LogInformation($"Reserving seat {seatNumber} for flight ID {flightId} with booking {bookingReference}");

                var flight = await _flightRepository.GetFlightByIdAsync(flightId);
                if (flight == null)
                {
                    _logger.LogWarning($"Flight with ID {flightId} not found");
                    return;
                }

                // Check if seat is already permanently booked
                var seat = await _seatRepository.GetSeatByNumberAndFlightAsync(seatNumber, flightId);
                if (seat != null && seat.IsBooked)
                {
                    _logger.LogWarning($"Seat {seatNumber} is already permanently booked for flight {flight.FlightNumber}");
                    await Clients.Caller.SendAsync("SeatReservationFailed", flight.FlightNumber, seatNumber, "Seat is already booked");
                    return;
                }

                // Initialize reservations for this flight if needed
                if (!SeatReservations.ContainsKey(flight.FlightNumber))
                {
                    SeatReservations[flight.FlightNumber] = new Dictionary<string, SeatReservation>();
                }

                // Check if seat is already reserved by someone else
                if (SeatReservations[flight.FlightNumber].ContainsKey(seatNumber))
                {
                    var existingReservation = SeatReservations[flight.FlightNumber][seatNumber];
                    if (existingReservation.BookingReference != bookingReference)
                    {
                        _logger.LogWarning($"Seat {seatNumber} is already reserved by booking {existingReservation.BookingReference}");
                        await Clients.Caller.SendAsync("SeatReservationFailed", flight.FlightNumber, seatNumber, "Seat is already reserved");
                        return;
                    }
                }

                // Create or update reservation
                SeatReservations[flight.FlightNumber][seatNumber] = new SeatReservation
                {
                    ConnectionId = Context.ConnectionId,
                    BookingReference = bookingReference,
                    ReservedAt = DateTime.UtcNow
                };

                // Notify all subscribers about the reservation
                await Clients.Group($"Flight_{flight.FlightNumber}")
                    .SendAsync("SeatReserved", flight.FlightNumber, seatNumber, bookingReference);

                _logger.LogInformation($"Seat {seatNumber} reserved for flight {flight.FlightNumber} by booking {bookingReference}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reserving seat {seatNumber} for flight ID {flightId}");
            }
        }

        // Release a seat reservation
        public async Task ReleaseSeatReservationAsync(int flightId, string seatNumber)
        {
            try
            {
                var flight = await _flightRepository.GetFlightByIdAsync(flightId);
                if (flight == null) return;

                if (SeatReservations.ContainsKey(flight.FlightNumber) &&
                    SeatReservations[flight.FlightNumber].ContainsKey(seatNumber))
                {
                    SeatReservations[flight.FlightNumber].Remove(seatNumber);

                    // Notify all subscribers about the released reservation
                    await Clients.Group($"Flight_{flight.FlightNumber}")
                        .SendAsync("SeatReservationReleased", flight.FlightNumber, seatNumber);

                    _logger.LogInformation($"Seat reservation released for {seatNumber} on flight {flight.FlightNumber}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error releasing seat reservation {seatNumber} for flight ID {flightId}");
            }
        }

        // This method is called by the API after successful check-in to confirm the booking
        public async Task ConfirmSeatBookingAsync(string flightNumber, string seatNumber, string bookingReference)
        {
            try
            {
                _logger.LogInformation($"Confirming seat booking for {seatNumber} on flight {flightNumber}");

                // Remove the reservation
                if (SeatReservations.ContainsKey(flightNumber) &&
                    SeatReservations[flightNumber].ContainsKey(seatNumber))
                {
                    SeatReservations[flightNumber].Remove(seatNumber);
                }

                // Notify all subscribers about the confirmed booking
                await Clients.Group($"Flight_{flightNumber}")
                    .SendAsync("SeatBooked", flightNumber, seatNumber, bookingReference);

                _logger.LogInformation($"Seat {seatNumber} booking confirmed for flight {flightNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error confirming seat booking {seatNumber} for flight {flightNumber}");
            }
        }

        private async Task CleanupClientReservations(string connectionId)
        {
            try
            {
                foreach (var flightReservations in SeatReservations.Values)
                {
                    var reservationsToRemove = flightReservations
                        .Where(kvp => kvp.Value.ConnectionId == connectionId)
                        .ToList();

                    foreach (var reservation in reservationsToRemove)
                    {
                        flightReservations.Remove(reservation.Key);
                        _logger.LogInformation($"Cleaned up reservation for seat {reservation.Key} from disconnected client {connectionId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cleaning up reservations for client {connectionId}");
            }
        }

        private List<Seat> ApplyReservationsToSeats(string flightNumber, List<Seat> seats)
        {
            if (!SeatReservations.ContainsKey(flightNumber))
                return seats;

            var reservations = SeatReservations[flightNumber];
            var result = new List<Seat>();

            foreach (var seat in seats)
            {
                var seatCopy = new Seat
                {
                    SeatId = seat.SeatId,
                    FlightId = seat.FlightId,
                    SeatNumber = seat.SeatNumber,
                    IsBooked = seat.IsBooked, // Keep original booking status
                    Class = seat.Class,
                    Price = seat.Price
                };

                // If seat is reserved (but not permanently booked), mark it as temporarily unavailable
                if (!seat.IsBooked && reservations.ContainsKey(seat.SeatNumber))
                {
                    seatCopy.IsBooked = true; // Temporarily show as booked for UI purposes
                }

                result.Add(seatCopy);
            }

            return result;
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
                    FlightSeats[flightNumber] = new List<Seat>();
                    return;
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
                                FlightId = flight.FlightId,
                                SeatNumber = seatNumber,
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

    public class SeatReservation
    {
        public string ConnectionId { get; set; }
        public string BookingReference { get; set; }
        public DateTime ReservedAt { get; set; }
    }
}