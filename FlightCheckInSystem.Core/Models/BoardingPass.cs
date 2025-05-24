// FlightCheckInSystem.Core/Models/BoardingPass.cs
using System;

namespace FlightCheckInSystem.Core.Models
{
    public class BoardingPass
    {
        public string PassengerName { get; set; }
        public string PassportNumber { get; set; }
        public string FlightNumber { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public string SeatNumber { get; set; }
        public DateTime BoardingTime { get; set; } // Typically some time before departure
    }
}