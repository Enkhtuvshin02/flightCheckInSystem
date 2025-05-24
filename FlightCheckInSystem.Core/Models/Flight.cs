// FlightCheckInSystem.Core/Models/Flight.cs
using FlightCheckInSystem.Core.Enums;
using System;
using System.Collections.Generic;

namespace FlightCheckInSystem.Core.Models
{
    public class Flight
    {
        public int FlightId { get; set; } // Primary Key
        public string FlightNumber { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public FlightStatus Status { get; set; } = FlightStatus.CheckingIn;
        public List<Seat> Seats { get; set; } = new List<Seat>(); // All seats for this flight
        public List<Booking> Bookings { get; set; } = new List<Booking>(); // All bookings for this flight
        public override string ToString() => $"{FlightNumber} ({DepartureAirport} to {ArrivalAirport}) - {Status}";
    }
}