using System;

namespace FlightCheckInSystem.Core.Models
{
    public class Booking
    {
        public int BookingId { get; set; } // Primary Key
        public int PassengerId { get; set; } // Foreign Key to Passenger
        public int FlightId { get; set; } // Foreign Key to Flight
        public int? SeatId { get; set; } // Foreign Key to Seat (nullable, assigned at check-in)
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public bool IsCheckedIn { get; set; } = false;
        public DateTime? CheckInTime { get; set; }

        public Passenger Passenger { get; set; } // Navigation property
        public Flight Flight { get; set; }       // Navigation property
        public Seat Seat { get; set; }           // Navigation property (can be null)
        public override string ToString() => $"Booking ID: {BookingId} for Passenger ID: {PassengerId} on Flight ID: {FlightId} - {(IsCheckedIn ? "Checked-In" : "Pending")}";
    }
}