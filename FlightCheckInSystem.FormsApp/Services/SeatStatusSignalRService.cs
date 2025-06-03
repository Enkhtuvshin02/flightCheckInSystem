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

        public event Action<string, string, string> SeatBooked;         public event Action<string, string> SeatReleased; 
                public event Action<string, string> FlightSeatsReceived; 
        public SeatStatusSignalRService(string hubUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string, string, string>("SeatBooked", (flightNumber, seatNumber, bookingReference) =>
            {
                Debug.WriteLine($"[SignalR] SeatBooked: {flightNumber} {seatNumber} {bookingReference}");
                SeatBooked?.Invoke(flightNumber, seatNumber, bookingReference);
            });

            _connection.On<string, string>("SeatReleased", (flightNumber, seatNumber) =>
            {
                Debug.WriteLine($"[SignalR] SeatReleased: {flightNumber} {seatNumber}");
                SeatReleased?.Invoke(flightNumber, seatNumber);
            });

                        _connection.On<string, string>("ReceiveFlightSeats", (flightNumber, seatDataJson) =>
            {
                Debug.WriteLine($"[SignalR] ReceiveFlightSeats: {flightNumber} {seatDataJson}");
                FlightSeatsReceived?.Invoke(flightNumber, seatDataJson);
            });
        }

        public async Task StartAsync()
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                Debug.WriteLine("[SignalR] Connection started.");
            }
        }

        public async Task StopAsync()
        {
            if (_connection.State != HubConnectionState.Disconnected)
            {
                await _connection.StopAsync();
                Debug.WriteLine("[SignalR] Connection stopped.");
            }
        }

        public async Task SubscribeToFlightAsync(string flightNumber)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("SubscribeToFlightUpdates", flightNumber);
                Debug.WriteLine($"[SignalR] Subscribed to flight {flightNumber}");
            }
        }

                public async Task GetFlightSeatsAsync(int flightId)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("GetFlightSeatsAsync", flightId);
                Debug.WriteLine($"[SignalR] Requested seat data for flight {flightId}");
            }
        }

        public async Task UnsubscribeFromFlightAsync(string flightNumber)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("UnsubscribeFromFlightUpdates", flightNumber);
                Debug.WriteLine($"[SignalR] Unsubscribed from flight {flightNumber}");
            }
        }


        public HubConnectionState ConnectionState => _connection.State;


        public void Dispose()
        {
            if (!_disposed)
            {
                _connection.DisposeAsync().AsTask().Wait();
                _disposed = true;
            }
        }
    }
}