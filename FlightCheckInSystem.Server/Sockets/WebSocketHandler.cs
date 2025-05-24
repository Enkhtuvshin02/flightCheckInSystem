// FlightCheckInSystem.Server/Sockets/WebSocketHandler.cs
using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Core.Enums;
using FlightCheckInSystem.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlightCheckInSystem.Server.Sockets
{
    public class WebSocketHandler
    {
        private readonly WebSocket _webSocket;
        private readonly IServiceProvider _serviceProvider; // To resolve scoped services
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private string _connectionId;

        public WebSocketHandler(WebSocket webSocket, IServiceProvider serviceProvider, WebSocketConnectionManager connectionManager, ILogger logger)
        {
            _webSocket = webSocket;
            _serviceProvider = serviceProvider;
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public async Task HandleConnectionAsync()
        {
            _connectionId = _connectionManager.AddSocket(_webSocket);
            _logger.LogInformation($"WebSocket Handler: Connection {_connectionId} established.");

            // Send Client ID to the connected client
            var clientIdMsg = new ServerSocketMessage { Type = "CLIENT_ID_ASSIGNED", Payload = new ClientIdAssignedPayload { ClientId = _connectionId } };
            await SendMessageAsync(clientIdMsg);

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = null;

            try
            {
                do
                {
                    Array.Clear(buffer, 0, buffer.Length); // Clear buffer before read
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text && !result.CloseStatus.HasValue)
                    {
                        var messageString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation($"WebSocket Handler: Received from {_connectionId}: {messageString}");
                        await ProcessClientMessageAsync(messageString);
                    }
                } while (!result.CloseStatus.HasValue);

                _logger.LogInformation($"WebSocket Handler: Connection {_connectionId} closing. Status: {result.CloseStatus}");
            }
            catch (WebSocketException wsEx) when (wsEx.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely || wsEx.WebSocketErrorCode == WebSocketError.InvalidState)
            {
                _logger.LogWarning($"WebSocket Handler: Connection {_connectionId} closed prematurely or in invalid state: {wsEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"WebSocket Handler: Error with connection {_connectionId}.");
            }
            finally
            {
                await _connectionManager.RemoveSocketAsync(_connectionId);
                _logger.LogInformation($"WebSocket Handler: Connection {_connectionId} removed.");
            }
        }

        private async Task ProcessClientMessageAsync(string messageString)
        {
            ClientSocketMessage clientMessage;
            try
            {
                clientMessage = JsonConvert.DeserializeObject<ClientSocketMessage>(messageString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"WebSocket Handler: Invalid JSON received from {_connectionId}: {messageString}");
                await SendMessageAsync(new ServerSocketMessage { Type = "ERROR", Payload = new ErrorResponsePayload { ErrorMessage = "Invalid JSON format." } });
                return;
            }

            if (clientMessage == null || string.IsNullOrEmpty(clientMessage.Type))
            {
                await SendMessageAsync(new ServerSocketMessage { Type = "ERROR", Payload = new ErrorResponsePayload { ErrorMessage = "Message type cannot be empty." } });
                return;
            }

            // Use a new scope for each message to correctly resolve scoped services
            using (var scope = _serviceProvider.CreateScope())
            {
                var checkInService = scope.ServiceProvider.GetRequiredService<ICheckInService>();
                var flightService = scope.ServiceProvider.GetRequiredService<IFlightManagementService>();
                var seatRepository = scope.ServiceProvider.GetRequiredService<ISeatRepository>(); // For seat details
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>(); // For flight details

                try
                {
                    switch (clientMessage.Type.ToUpper())
                    {
                        case "ASSIGN_SEAT_REQUEST":
                            var assignPayload = JsonConvert.DeserializeObject<AssignSeatRequestPayload>(clientMessage.Payload);
                            var (success, msg, boardingPass) = await checkInService.AssignSeatToBookingAsync(assignPayload.BookingId, assignPayload.SeatId);

                            var bookingDetails = await bookingRepository.GetBookingByIdAsync(assignPayload.BookingId);
                            var seatDetails = await seatRepository.GetSeatByIdAsync(assignPayload.SeatId);

                            var responsePayload = new AssignSeatResponsePayload
                            {
                                Success = success,
                                Message = msg,
                                BoardingPass = boardingPass, // boardingPass is already populated from checkInService
                                FlightId = bookingDetails?.FlightId ?? 0,
                                SeatId = assignPayload.SeatId,
                                SeatNumber = seatDetails?.SeatNumber,
                                IsNowBooked = success
                            };
                            await SendMessageAsync(new ServerSocketMessage { Type = "ASSIGN_SEAT_RESPONSE", Payload = responsePayload });

                            if (success)
                            {
                                var seatUpdate = new SeatStatusUpdatePayload
                                {
                                    FlightId = responsePayload.FlightId,
                                    SeatId = responsePayload.SeatId,
                                    SeatNumber = responsePayload.SeatNumber,
                                    IsBooked = true,
                                    BookedByClientId = _connectionId
                                };
                                await BroadcastMessageAsync(new ServerSocketMessage { Type = "SEAT_STATUS_UPDATE", Payload = seatUpdate }, excludeSelf: true);
                            }
                            break;

                        case "GET_FLIGHT_SEATS_REQUEST": // For WinForms to get initial seat layout/status
                            var getSeatsPayload = JsonConvert.DeserializeObject<GetFlightSeatsRequestPayload>(clientMessage.Payload);
                            var seats = await seatRepository.GetSeatsByFlightIdAsync(getSeatsPayload.FlightId);
                            await SendMessageAsync(new ServerSocketMessage { Type = "FLIGHT_SEATS_RESPONSE", Payload = new FlightSeatsResponsePayload { FlightId = getSeatsPayload.FlightId, Seats = seats } });
                            break;

                        // Add a case for Flight Status Update initiated by an agent
                        case "UPDATE_FLIGHT_STATUS_REQUEST":
                            // Payload: { "FlightId": 1, "NewStatusString": "Boarding" }
                            var updateFlightStatusReq = JsonConvert.DeserializeObject<dynamic>(clientMessage.Payload); // Use dynamic for simplicity here
                            int flightIdToUpdate = (int)updateFlightStatusReq.FlightId;
                            string newStatusStr = (string)updateFlightStatusReq.NewStatusString;

                            if (Enum.TryParse<FlightStatus>(newStatusStr, true, out FlightStatus newFlightStatus))
                            {
                                bool updated = await flightService.UpdateFlightStatusAsync(flightIdToUpdate, newFlightStatus);
                                if (updated)
                                {
                                    var flightDetails = await flightService.GetFlightDetailsAsync(flightIdToUpdate);
                                    var flightStatusUpdateBroadcast = new FlightStatusUpdatePayload
                                    {
                                        FlightId = flightIdToUpdate,
                                        FlightNumber = flightDetails?.FlightNumber,
                                        NewStatus = newFlightStatus.ToString()
                                    };
                                    // Send confirmation to originating client
                                    await SendMessageAsync(new ServerSocketMessage { Type = "FLIGHT_STATUS_UPDATE_CONFIRMED", Payload = flightStatusUpdateBroadcast });
                                    // Broadcast to all (including display boards via their own WebSocket or SignalR connection in future)
                                    await BroadcastMessageAsync(new ServerSocketMessage { Type = "FLIGHT_STATUS_CHANGED_BROADCAST", Payload = flightStatusUpdateBroadcast }, excludeSelf: false); // broadcast to self too for UI consistency
                                }
                                else
                                {
                                    await SendMessageAsync(new ServerSocketMessage { Type = "ERROR", Payload = new ErrorResponsePayload { ErrorMessage = "Failed to update flight status.", OriginalRequestType = clientMessage.Type } });
                                }
                            }
                            else
                            {
                                await SendMessageAsync(new ServerSocketMessage { Type = "ERROR", Payload = new ErrorResponsePayload { ErrorMessage = "Invalid flight status provided.", OriginalRequestType = clientMessage.Type } });
                            }
                            break;

                        default:
                            _logger.LogWarning($"WebSocket Handler: Unknown message type '{clientMessage.Type}' from {_connectionId}.");
                            await SendMessageAsync(new ServerSocketMessage { Type = "ERROR", Payload = new ErrorResponsePayload { ErrorMessage = $"Unknown message type: {clientMessage.Type}", OriginalRequestType = clientMessage.Type } });
                            break;
                    }
                }
                catch (JsonSerializationException jsonEx)
                {
                    _logger.LogError(jsonEx, $"WebSocket Handler: Error deserializing payload for type '{clientMessage.Type}' from {_connectionId}. Payload: {clientMessage.Payload}");
                    await SendMessageAsync(new ServerSocketMessage { Type = "ERROR", Payload = new ErrorResponsePayload { ErrorMessage = "Invalid payload for message type: " + clientMessage.Type, OriginalRequestType = clientMessage.Type } });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"WebSocket Handler: Error processing message type '{clientMessage.Type}' from {_connectionId}.");
                    await SendMessageAsync(new ServerSocketMessage { Type = "ERROR", Payload = new ErrorResponsePayload { ErrorMessage = "Server error processing your request for type: " + clientMessage.Type, OriginalRequestType = clientMessage.Type } });
                }
            }
        }

        private async Task SendMessageAsync(ServerSocketMessage message)
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                var messageString = JsonConvert.SerializeObject(message);
                var buffer = Encoding.UTF8.GetBytes(messageString);
                await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                _logger.LogInformation($"WebSocket Handler: Sent to {_connectionId}: {messageString}");
            }
        }

        private async Task BroadcastMessageAsync(ServerSocketMessage message, bool excludeSelf = false)
        {
            _logger.LogInformation($"WebSocket Handler: Broadcasting Type '{message.Type}'. ExcludeSelf: {excludeSelf}");
            foreach (var entry in _connectionManager.GetAll())
            {
                if (excludeSelf && entry.Key == _connectionId) continue;

                WebSocket socket = entry.Value;
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        var messageString = JsonConvert.SerializeObject(message);
                        var buffer = Encoding.UTF8.GetBytes(messageString);
                        await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"WebSocket Handler: Error broadcasting to connection {entry.Key}.");
                        // Optionally remove problematic socket from manager
                    }
                }
            }
        }
    }
}