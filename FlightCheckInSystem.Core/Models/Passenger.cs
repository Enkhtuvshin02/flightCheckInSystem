namespace FlightCheckInSystem.Core.Models
{
    public class Passenger
    {
        public int Id { get; set; }         public int PassengerId { get; set; }         public string PassportNumber { get; set; }         public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
                public override string ToString() => $"{FirstName} {LastName} ({PassportNumber})";
    }
}