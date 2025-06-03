namespace FlightCheckInSystem.Core.Models
{
    public class Seat
    {
        public int Id { get; set; }         public int SeatId { get; set; }         public int FlightId { get; set; }         public string SeatNumber { get; set; }         public bool IsBooked { get; set; } = false;         public int? BookingId { get; set; }         public Flight Flight { get; set; }         public override string ToString() => $"Seat {SeatNumber} (Flight ID: {FlightId}) - {(IsBooked ? "Booked" : "Available")}";
    }
}