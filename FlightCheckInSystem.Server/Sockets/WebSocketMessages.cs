// FlightCheckInSystem.Server/Sockets/WebSocketMessages.cs
using FlightCheckInSystem.Core.Models; // For BoardingPass etc.

namespace FlightCheckInSystem.Server.Sockets
{
    // Generic message structure
    public class ClientSocketMessage
    {
        public string Type { get; set; }
        public string Payload { get; set; } // JSON string for specific payload
    }

    public class ServerSocketMessage
    {
        public string Type { get; set; }
        public object Payload { get; set; } // Can be any object, will be serialized to JSON
        public string ClientId { get; set; } // Optional: For messages targeted at a specific client
    }

    // --- Example Payloads for Client-to-Server ---
    public class AssignSeatRequestPayload
    {
        public int BookingId { get; set; }
        public int SeatId { get; set; }
    }

    public class GetFlightSeatsRequestPayload
    {
        public int FlightId { get; set; }
    }


    // --- Example Payloads for Server-to-Client ---
    public class AssignSeatResponsePayload
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public BoardingPass BoardingPass { get; set; }
        public int FlightId { get; set; } // To know which flight's UI to update
        public int SeatId { get; set; } // The seat that was processed
        public string SeatNumber { get; set; } // The seat number
        public bool IsNowBooked { get; set; } // True if booked, false if (e.g.) assignment failed and it's still available
    }

    public class FlightSeatsResponsePayload
    {
        public int FlightId { get; set; }
        public IEnumerable<Seat> Seats { get; set; }
    }

    public class ErrorResponsePayload
    {
        public string ErrorMessage { get; set; }
        public string OriginalRequestType { get; set; }
    }

    public class ClientIdAssignedPayload
    {
        public string ClientId { get; set; }
    }

    // For broadcasting seat updates
    public class SeatStatusUpdatePayload
    {
        public int FlightId { get; set; }
        public int SeatId { get; set; }
        public string SeatNumber { get; set; }
        public bool IsBooked { get; set; }
        public string BookedByClientId { get; set; } // ID of the client who booked it
    }

    // For broadcasting flight status updates
    public class FlightStatusUpdatePayload
    {
        public int FlightId { get; set; }
        public string NewStatus { get; set; }
        public string FlightNumber { get; set; }
    }
}