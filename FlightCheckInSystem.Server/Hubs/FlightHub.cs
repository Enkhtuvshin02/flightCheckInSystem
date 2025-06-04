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
        // Түр зуурын суудлын захиалга
        private static readonly Dictionary<string, Dictionary<string, SeatReservation>> SeatReservations =
            new Dictionary<string, Dictionary<string, SeatReservation>>();

        private readonly ISeatRepository _seatRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ILogger<FlightHub> _logger;

        public FlightHub(ISeatRepository seatRepository, IFlightRepository flightRepository, ILogger<FlightHub> logger)
        {
            _seatRepository = seatRepository;
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
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await CleanupClientReservations(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        // Subscribe to flight updates for seat selection (existing functionality)
        public async Task SubscribeToFlightUpdates(string flightNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Flight_{flightNumber}");
            _logger.LogInformation($"Client {Context.ConnectionId} subscribed to flight {flightNumber}");
            await Clients.Caller.SendAsync("SubscriptionConfirmed", flightNumber);
        }

        public async Task UnsubscribeFromFlightUpdates(string flightNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Flight_{flightNumber}");
            _logger.LogInformation($"Client {Context.ConnectionId} unsubscribed from flight {flightNumber}");
        }

        // NEW: Subscribe to flight status board updates
        public async Task SubscribeToFlightStatusBoard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "FlightStatusBoard");
            _logger.LogInformation($"Client {Context.ConnectionId} subscribed to flight status board");
            await Clients.Caller.SendAsync("SubscriptionConfirmed", "FlightStatusBoard");
        }

        public async Task UnsubscribeFromFlightStatusBoard()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "FlightStatusBoard");
            _logger.LogInformation($"Client {Context.ConnectionId} unsubscribed from flight status board");
        }

        // NEW: Ping method for testing connection
        public async Task<string> Ping()
        {
            var response = $"Pong from server at {DateTime.UtcNow:HH:mm:ss}";
            _logger.LogInformation($"Ping received from {Context.ConnectionId}, responding with: {response}");
            return response;
        }

        // NEW: Broadcast flight status update
        public async Task BroadcastFlightStatusUpdate(string flightNumber, FlightStatus newStatus)
        {
            _logger.LogInformation($"Broadcasting flight status update: {flightNumber} -> {newStatus}");
            await Clients.Group("FlightStatusBoard").SendAsync("FlightStatusUpdated", flightNumber, newStatus);
        }

        // NEW: Broadcast new flight created
        public async Task BroadcastNewFlight(Flight flight)
        {
            _logger.LogInformation($"Broadcasting new flight created: {flight.FlightNumber}");
            await Clients.Group("FlightStatusBoard").SendAsync("NewFlightCreated", flight);
        }

        // NEW: Broadcast flight updated
        public async Task BroadcastFlightUpdated(Flight flight)
        {
            _logger.LogInformation($"Broadcasting flight updated: {flight.FlightNumber}");
            await Clients.Group("FlightStatusBoard").SendAsync("FlightUpdated", flight);
        }

        public async Task GetFlightSeatsAsync(int flightId)
        {
            try
            {
                _logger.LogInformation($"Getting seats for flight ID {flightId}");

                var flight = await _flightRepository.GetFlightByIdAsync(flightId);
                if (flight == null)
                {
                    await Clients.Caller.SendAsync("ReceiveFlightSeats", "", "[]");
                    return;
                }

                var seats = await _seatRepository.GetSeatsByFlightIdAsync(flightId);
                var seatList = seats?.ToList() ?? new List<Seat>();

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

        public async Task ReserveSeatAsync(int flightId, string seatNumber, string bookingReference)
        {
            try
            {
                var flight = await _flightRepository.GetFlightByIdAsync(flightId);
                if (flight == null) return;

                // Суудал аль хэдийн эзэгдсэн эсэхийг шалгах
                var seat = await _seatRepository.GetSeatByNumberAndFlightAsync(seatNumber, flightId);
                if (seat != null && seat.IsBooked)
                {
                    await Clients.Caller.SendAsync("SeatReservationFailed", flight.FlightNumber, seatNumber, "Seat is already booked");
                    return;
                }

                // Захиалгын бүртгэл эхлүүлэх
                if (!SeatReservations.ContainsKey(flight.FlightNumber))
                {
                    SeatReservations[flight.FlightNumber] = new Dictionary<string, SeatReservation>();
                }

                // Өөр хэрэглэгчээр захиалагдсан эсэхийг шалгах
                if (SeatReservations[flight.FlightNumber].ContainsKey(seatNumber))
                {
                    var existingReservation = SeatReservations[flight.FlightNumber][seatNumber];
                    if (existingReservation.BookingReference != bookingReference)
                    {
                        await Clients.Caller.SendAsync("SeatReservationFailed", flight.FlightNumber, seatNumber, "Seat is already reserved");
                        return;
                    }
                }

                // Захиалга үүсгэх эсвэл шинэчлэх
                SeatReservations[flight.FlightNumber][seatNumber] = new SeatReservation
                {
                    ConnectionId = Context.ConnectionId,
                    BookingReference = bookingReference,
                    ReservedAt = DateTime.UtcNow
                };

                // Бүх захиалагчдад мэдэгдэх
                await Clients.Group($"Flight_{flight.FlightNumber}")
                    .SendAsync("SeatReserved", flight.FlightNumber, seatNumber, bookingReference);

                _logger.LogInformation($"Seat {seatNumber} reserved for flight {flight.FlightNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reserving seat {seatNumber} for flight ID {flightId}");
            }
        }

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

        public async Task ConfirmSeatBookingAsync(string flightNumber, string seatNumber, string bookingReference)
        {
            try
            {
                // Захиалгыг устгах
                if (SeatReservations.ContainsKey(flightNumber) &&
                    SeatReservations[flightNumber].ContainsKey(seatNumber))
                {
                    SeatReservations[flightNumber].Remove(seatNumber);
                }

                // Баталгаажсан захиалгыг мэдэгдэх
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
                        _logger.LogInformation($"Cleaned up reservation for seat {reservation.Key}");
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
                    IsBooked = seat.IsBooked,
                    Class = seat.Class,
                    Price = seat.Price
                };

                // Захиалагдсан боловч баталгаажаагүй суудлыг түр хаагдсан гэж харуулах
                if (!seat.IsBooked && reservations.ContainsKey(seat.SeatNumber))
                {
                    seatCopy.IsBooked = true;
                }

                result.Add(seatCopy);
            }

            return result;
        }
    }

    public class SeatReservation
    {
        public string ConnectionId { get; set; }
        public string BookingReference { get; set; }
        public DateTime ReservedAt { get; set; }
    }
}