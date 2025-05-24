// FlightCheckInSystem.Core/Models/Seat.cs
namespace FlightCheckInSystem.Core.Models
{
    public class Seat
    {
        public int SeatId { get; set; } // Primary Key
        public int FlightId { get; set; } // Foreign Key to Flight
        public string SeatNumber { get; set; } // e.g., "A1", "12B"
        public bool IsBooked { get; set; } = false; // True if this seat is assigned to a passenger
        public Flight Flight { get; set; } // Navigation property
        public override string ToString() => $"Seat {SeatNumber} (Flight ID: {FlightId}) - {(IsBooked ? "Booked" : "Available")}";
    }
}