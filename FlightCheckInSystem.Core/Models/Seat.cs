namespace FlightCheckInSystem.Core.Models
{
    public class Seat
    {
        public int SeatId { get; set; }
        public int FlightId { get; set; }
        public string SeatNumber { get; set; }
        public bool IsBooked { get; set; } = false;
        public string Class { get; set; }
        public decimal Price { get; set; }

        public Flight Flight { get; set; }
    }
}
