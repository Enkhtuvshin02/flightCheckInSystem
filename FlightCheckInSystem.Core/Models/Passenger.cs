// FlightCheckInSystem.Core/Models/Passenger.cs
namespace FlightCheckInSystem.Core.Models
{
    public class Passenger
    {
        public int PassengerId { get; set; } // Primary Key
        public string PassportNumber { get; set; } // Unique
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // Add other relevant passenger details if needed
        public override string ToString() => $"{FirstName} {LastName} ({PassportNumber})";
    }
}