// FlightCheckInSystem.Server/Sockets/WebSocketConnectionManager.cs
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace FlightCheckInSystem.Server.Sockets
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public string AddSocket(WebSocket socket)
        {
            string connectionId = Guid.NewGuid().ToString();
            _sockets.TryAdd(connectionId, socket);
            return connectionId;
        }

        public async Task RemoveSocketAsync(string id)
        {
            if (_sockets.TryRemove(id, out var socket))
            {
                if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                {
                    try
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by manager", CancellationToken.None);
                    }
                    catch (WebSocketException) { /* Ignore if already closing */ }
                    catch (ObjectDisposedException) { /* Ignore if disposed */ }
                }
            }
        }

        public WebSocket GetSocketById(string id)
        {
            return _sockets.TryGetValue(id, out var socket) ? socket : null;
        }

        public ConcurrentDictionary<string, WebSocket> GetAll()
        {
            return _sockets;
        }
    }
}