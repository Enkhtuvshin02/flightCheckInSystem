using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace FlightCheckInSystem.FormsApp.Services
{
    public class SeatStatusSignalRService : IDisposable
    {
        private readonly HubConnection _connection;
        private bool _disposed = false;

        // Events for real-time updates
        public event Action<string, string, string> SeatBooked;
        public event Action<string, string> SeatReleased;
        public event Action<string, string> FlightSeatsReceived;

        // New events for seat reservations
        public event Action<string, string, string> SeatReserved;
        public event Action<string, string> SeatReservationReleased;
        public event Action<string, string, string> SeatReservationFailed;
        // Inside SeatStatusSignalRService.cs (example, not provided in files)
        public event Action<string> RefreshSeatsForFlight;

     
        public SeatStatusSignalRService(string hubUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            // Handle seat booked notifications (permanent bookings)
            _connection.On<string, string, string>("SeatBooked", (flightNumber, seatNumber, bookingReference) =>
            {
                Debug.WriteLine($"[SignalR] SeatBooked: {flightNumber} {seatNumber} {bookingReference}");
                SeatBooked?.Invoke(flightNumber, seatNumber, bookingReference);
            });

            // Handle seat released notifications
            _connection.On<string, string>("SeatReleased", (flightNumber, seatNumber) =>
            {
                Debug.WriteLine($"[SignalR] SeatReleased: {flightNumber} {seatNumber}");
                SeatReleased?.Invoke(flightNumber, seatNumber);
            });

            // Handle flight seats data
            _connection.On<string, string>("ReceiveFlightSeats", (flightNumber, seatDataJson) =>
            {
                Debug.WriteLine($"[SignalR] ReceiveFlightSeats: {flightNumber} - Data length: {seatDataJson?.Length ?? 0}");
                FlightSeatsReceived?.Invoke(flightNumber, seatDataJson);
            });

            // Handle seat reserved notifications (temporary reservations)
            _connection.On<string, string, string>("SeatReserved", (flightNumber, seatNumber, bookingReference) =>
            {
                Debug.WriteLine($"[SignalR] SeatReserved: {flightNumber} {seatNumber} {bookingReference}");
                SeatReserved?.Invoke(flightNumber, seatNumber, bookingReference);
            });

            // Handle seat reservation released notifications
            _connection.On<string, string>("SeatReservationReleased", (flightNumber, seatNumber) =>
            {
                Debug.WriteLine($"[SignalR] SeatReservationReleased: {flightNumber} {seatNumber}");
                SeatReservationReleased?.Invoke(flightNumber, seatNumber);
            });

            // Handle seat reservation failed notifications
            _connection.On<string, string, string>("SeatReservationFailed", (flightNumber, seatNumber, reason) =>
            {
                Debug.WriteLine($"[SignalR] SeatReservationFailed: {flightNumber} {seatNumber} {reason}");
                SeatReservationFailed?.Invoke(flightNumber, seatNumber, reason);
            });

            // Handle subscription confirmations
            _connection.On<string>("SubscriptionConfirmed", (target) =>
            {
                Debug.WriteLine($"[SignalR] SubscriptionConfirmed: {target}");
            });

            // Handle connection state changes
            _connection.Closed += async (exception) =>
            {
                Debug.WriteLine($"[SignalR] Connection closed: {exception?.Message ?? "No error"}");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                try
                {
                    await _connection.StartAsync();
                    Debug.WriteLine("[SignalR] Reconnected successfully");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SignalR] Reconnection failed: {ex.Message}");
                }
            };

            _connection.Reconnecting += (exception) =>
            {
                Debug.WriteLine($"[SignalR] Reconnecting: {exception?.Message ?? "Connection lost"}");
                return Task.CompletedTask;
            };

            _connection.Reconnected += (connectionId) =>
            {
                Debug.WriteLine($"[SignalR] Reconnected with ID: {connectionId}");
                return Task.CompletedTask;
            };
        }

        public async Task StartAsync()
        {
            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                    Debug.WriteLine("[SignalR] Connection started successfully.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignalR] Failed to start connection: {ex.Message}");
                throw;
            }
        }

        public async Task StopAsync()
        {
            try
            {
                if (_connection.State != HubConnectionState.Disconnected)
                {
                    await _connection.StopAsync();
                    Debug.WriteLine("[SignalR] Connection stopped.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignalR] Error stopping connection: {ex.Message}");
            }
        }

        public async Task SubscribeToFlightAsync(string flightNumber)
        {
            try
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    await _connection.InvokeAsync("SubscribeToFlightUpdates", flightNumber);
                    Debug.WriteLine($"[SignalR] Subscribed to flight {flightNumber}");
                }
                else
                {
                    Debug.WriteLine($"[SignalR] Cannot subscribe - connection state: {_connection.State}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignalR] Error subscribing to flight {flightNumber}: {ex.Message}");
                throw;
            }
        }

        public async Task GetFlightSeatsAsync(int flightId)
        {
            try
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    await _connection.InvokeAsync("GetFlightSeatsAsync", flightId);
                    Debug.WriteLine($"[SignalR] Requested seat data for flight {flightId}");
                }
                else
                {
                    Debug.WriteLine($"[SignalR] Cannot get flight seats - connection state: {_connection.State}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignalR] Error getting flight seats for {flightId}: {ex.Message}");
                throw;
            }
        }

        // Reserve a seat temporarily (new method)
        public async Task ReserveSeatAsync(int flightId, string seatNumber, string bookingReference)
        {
            try
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    await _connection.InvokeAsync("ReserveSeatAsync", flightId, seatNumber, bookingReference);
                    Debug.WriteLine($"[SignalR] Reserved seat {seatNumber} for flight {flightId} with booking {bookingReference}");
                }
                else
                {
                    Debug.WriteLine($"[SignalR] Cannot reserve seat - connection state: {_connection.State}");
                    throw new InvalidOperationException("SignalR connection is not active");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignalR] Error reserving seat {seatNumber} for flight {flightId}: {ex.Message}");
                throw;
            }
        }

        // Release a seat reservation (new method)
        public async Task ReleaseSeatReservationAsync(int flightId, string seatNumber)
        {
            try
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    await _connection.InvokeAsync("ReleaseSeatReservationAsync", flightId, seatNumber);
                    Debug.WriteLine($"[SignalR] Released seat reservation {seatNumber} for flight {flightId}");
                }
                else
                {
                    Debug.WriteLine($"[SignalR] Cannot release seat reservation - connection state: {_connection.State}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignalR] Error releasing seat reservation {seatNumber} for flight {flightId}: {ex.Message}");
                // Don't throw here as this might be called during cleanup
            }
        }

        // Confirm a seat booking after successful API check-in (new method)
        public async Task ConfirmSeatBookingAsync(string flightNumber, string seatNumber, string bookingReference)
        {
            try
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    await _connection.InvokeAsync("ConfirmSeatBookingAsync", flightNumber, seatNumber, bookingReference);
                    Debug.WriteLine($"[SignalR] Confirmed seat booking {seatNumber} for flight {flightNumber}");
                }
                else
                {
                    Debug.WriteLine($"[SignalR] Cannot confirm seat booking - connection state: {_connection.State}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignalR] Error confirming seat booking {seatNumber} for flight {flightNumber}: {ex.Message}");
                // Don't throw here as the check-in was already successful
            }
        }

        public async Task UnsubscribeFromFlightAsync(string flightNumber)
        {
            try
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    await _connection.InvokeAsync("UnsubscribeFromFlightUpdates", flightNumber);
                    Debug.WriteLine($"[SignalR] Unsubscribed from flight {flightNumber}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignalR] Error unsubscribing from flight {flightNumber}: {ex.Message}");
                // Don't throw here as this might be called during cleanup
            }
        }

        public HubConnectionState ConnectionState => _connection.State;

        public bool IsConnected => _connection.State == HubConnectionState.Connected;

        public async Task<bool> EnsureConnectedAsync()
        {
            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await StartAsync();
                }
                return _connection.State == HubConnectionState.Connected;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _connection?.DisposeAsync().AsTask().Wait(TimeSpan.FromSeconds(5));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SignalR] Error disposing connection: {ex.Message}");
                }
                _disposed = true;
            }
        }
    }
}