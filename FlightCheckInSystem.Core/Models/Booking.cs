using System;
using FlightCheckInSystem.Core.Enums;

namespace FlightCheckInSystem.Core.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int PassengerId { get; set; }
        public int FlightId { get; set; }
        public int? SeatId { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public bool IsCheckedIn { get; set; } = false;
        public DateTime? CheckInTime { get; set; }

        public Passenger Passenger { get; set; }
        public Flight Flight { get; set; }
        public Seat Seat { get; set; }
    }
}