// FlightCheckInSystem.Core/Enums/FlightStatus.cs
namespace FlightCheckInSystem.Core.Enums
{
    public enum FlightStatus
    {
        Scheduled,    // Нислэг хуваарьлагдсан
        CheckingIn,   // Бүртгэж байна
        Boarding,     // Онгоцонд сууж байна
        GateClosed,   // Хаалга хаагдсан
        Departed,     // Ниссэн
        Delayed,      // Хойшилсон
        Cancelled     // Цуцалсан
    }
}