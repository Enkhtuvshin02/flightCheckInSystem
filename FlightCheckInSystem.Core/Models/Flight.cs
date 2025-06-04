using FlightCheckInSystem.Core.Enums;
using System;
using System.Collections.Generic;

namespace FlightCheckInSystem.Core.Models
{
    public class Flight
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public FlightStatus Status { get; set; } = FlightStatus.Scheduled;

        public List<Seat> Seats { get; set; } = new List<Seat>();
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}