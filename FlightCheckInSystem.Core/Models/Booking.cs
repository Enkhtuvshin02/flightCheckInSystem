using System;
using FlightCheckInSystem.Core.Enums;

namespace FlightCheckInSystem.Core.Models
{
    public class Booking
    {
        public int BookingId { get; set; }         public int PassengerId { get; set; }         public int FlightId { get; set; }         public int? SeatId { get; set; }         public string BookingReference { get; set; }         public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public bool IsCheckedIn { get; set; } = false;
        public DateTime? CheckInTime { get; set; }
        public string SeatNumber { get; set; } 
        public Passenger Passenger { get; set; }         public Flight Flight { get; set; }               public Seat Seat { get; set; }                   public override string ToString() => $"Booking ID: {BookingId} for Passenger ID: {PassengerId} on Flight ID: {FlightId} - {(IsCheckedIn ? "Checked-In" : "Pending")}";
    }
}