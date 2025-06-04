using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SimpleRaceConditionTest
{
    class Program
    {
        private static readonly string ApiBaseUrl = "http://localhost:5001";
        private static readonly HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Race Condition Test");
            Console.WriteLine("============================================");

            try
            {
                await CheckServer();
                await CreateTestBookings();
                await TestSeatRaceCondition();

                Console.WriteLine("\nTest finished. Press any key to exit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.ReadKey();
        }

        static async Task CheckServer()
        {
            Console.WriteLine("Checking server connection...");
            var response = await httpClient.GetAsync($"{ApiBaseUrl}/health");
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Server is running.");
            }
            else
            {
                throw new Exception("Could not connect to the server.");
            }
        }

        static async Task CreateTestBookings()
        {
            Console.WriteLine("\nCreating test bookings...");

            var passengers = new[]
            {
                new { Passport = "RACE001", FirstName = "Alice", LastName = "Test" },
                new { Passport = "RACE002", FirstName = "Bob", LastName = "Test" },
                new { Passport = "RACE003", FirstName = "Charlie", LastName = "Test" }
            };

            foreach (var p in passengers)
            {
                try
                {
                    await CreateBooking(p.Passport, p.FirstName, p.LastName);
                    Console.WriteLine($"Booking created for {p.FirstName}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error for {p.FirstName}: {ex.Message}");
                }
            }
        }

        static async Task CreateBooking(string passport, string firstName, string lastName)
        {
            // Зорчигч бүртгэх
            var passengerRequest = new
            {
                PassportNumber = passport,
                FirstName = firstName,
                LastName = lastName,
                Email = $"{firstName.ToLower()}@test.com",
                Phone = $"+976{Random.Shared.Next(10000000, 99999999)}"
            };
            var passengerResponse = await httpClient.PostAsJsonAsync($"{ApiBaseUrl}/api/bookings/findorcreatepassenger", passengerRequest);
            if (!passengerResponse.IsSuccessStatusCode)
            {
                var errorContent = await passengerResponse.Content.ReadAsStringAsync();
                throw new Exception($"Passenger creation failed: {passengerResponse.StatusCode}");
            }
            var passengerContent = await passengerResponse.Content.ReadAsStringAsync();
            var passengerResult = JsonSerializer.Deserialize<JsonElement>(passengerContent);

            if (!passengerResult.GetProperty("success").GetBoolean())
            {
                throw new Exception("Passenger creation failed");
            }
            var dataElement = passengerResult.GetProperty("data");
            int passengerId = GetPropertyValue(dataElement, new[] { "passengerId", "PassengerId", "id" });

            // Нислэг олох
            var flightResponse = await httpClient.GetAsync($"{ApiBaseUrl}/api/flights/number/OM201");
            if (!flightResponse.IsSuccessStatusCode)
            {
                throw new Exception("Flight OM201 not found");
            }
            var flightContent = await flightResponse.Content.ReadAsStringAsync();
            var flightResult = JsonSerializer.Deserialize<JsonElement>(flightContent);

            if (!flightResult.GetProperty("success").GetBoolean())
            {
                throw new Exception("Flight OM201 not found");
            }
            var flightDataElement = flightResult.GetProperty("data");
            int flightId = GetPropertyValue(flightDataElement, new[] { "flightId", "FlightId", "id" });

            // Захиалга үүсгэх
            var bookingRequest = new { PassengerId = passengerId, FlightId = flightId };
            var bookingResponse = await httpClient.PostAsJsonAsync($"{ApiBaseUrl}/api/bookings", bookingRequest);

            if (bookingResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return; // Захиалга аль хэдийн байгаа
            }

            if (!bookingResponse.IsSuccessStatusCode)
            {
                var bookingErrorContent = await bookingResponse.Content.ReadAsStringAsync();
                throw new Exception($"Booking creation failed: {bookingResponse.StatusCode}");
            }
        }

        static async Task TestSeatRaceCondition()
        {
            Console.WriteLine("\nStarting seat race condition test (seat 1A)...");
            Console.WriteLine("3 users will attempt to check-in to seat 1A simultaneously.");

            var users = new[]
            {
                new RaceTestUser("RACE001", "User1"),
                new RaceTestUser("RACE002", "User2"),
                new RaceTestUser("RACE003", "User3")
            };

            // SignalR холболт
            foreach (var user in users)
            {
                await user.Connect();
            }

            Console.WriteLine("\nStarting simultaneous check-ins...");

            var tasks = new List<Task>();
            foreach (var user in users)
            {
                tasks.Add(user.TryCheckIn("2A"));
            }
            await Task.WhenAll(tasks);

            await Task.Delay(2000); // SignalR мессеж хүлээх

            Console.WriteLine("\nFinal results:");
            int successCount = 0;
            RaceTestUser successfulUser = null;

            foreach (var user in users)
            {
                if (user.CheckInSuccessful)
                {
                    successCount++;
                    successfulUser = user;
                    Console.WriteLine($"{user.Name}: Successfully checked in - {user.AssignedSeat}");
                }
                else
                {
                    Console.WriteLine($"{user.Name}: Check-in failed - {user.ErrorMessage}");
                }
            }

            // Амжилттай бүртгэгдсэн хэрэглэгчийн мэдээллийг дэлгэрэнгүй харуулах
            if (successfulUser != null)
            {
                await ShowDatabaseChanges(successfulUser);
            }

            Console.WriteLine($"\nRace condition test results:");
            if (successCount == 1)
            {
                Console.WriteLine("SUCCESS: Only 1 user got the seat (race condition handled correctly)");
            }
            else if (successCount == 0)
            {
                Console.WriteLine("WARNING: No one got the seat (system issue)");
            }
            else
            {
                Console.WriteLine($"ERROR: {successCount} users got the same seat (RACE CONDITION DETECTED!)");
            }

            // Цэвэрлэх
            foreach (var user in users)
            {
                await user.Disconnect();
            }
        }

        // Амжилттай бүртгэгдсэн хэрэглэгчийн өгөгдлийн сангийн өөрчлөлтийг харуулах
        static async Task ShowDatabaseChanges(RaceTestUser user)
        {
            Console.WriteLine($"\nDatabase entries for {user.Name}:");
            Console.WriteLine("==========================================");

            try
            {
                // Зорчигчийн мэдээлэл
                var passengerResponse = await httpClient.GetAsync($"{ApiBaseUrl}/api/bookings/passport/{user.PassportNumber}");
                if (passengerResponse.IsSuccessStatusCode)
                {
                    var content = await passengerResponse.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(content);
                    var bookings = result.GetProperty("data").EnumerateArray();

                    foreach (var booking in bookings)
                    {
                        if (TryGetPropertyValue(booking, new[] { "isCheckedIn", "IsCheckedIn" }) == 1)
                        {
                            Console.WriteLine("BOOKING record:");
                            Console.WriteLine($"  BookingId: {GetPropertyValue(booking, new[] { "bookingId", "BookingId" })}");
                            Console.WriteLine($"  PassengerId: {GetPropertyValue(booking, new[] { "passengerId", "PassengerId" })}");
                            Console.WriteLine($"  FlightId: {GetPropertyValue(booking, new[] { "flightId", "FlightId" })}");
                            Console.WriteLine($"  SeatId: {TryGetPropertyValue(booking, new[] { "seatId", "SeatId" })}");
                            Console.WriteLine($"  IsCheckedIn: true");
                            Console.WriteLine($"  CheckInTime: {TryGetPropertyString(booking, new[] { "checkInTime", "CheckInTime" })}");

                            if (booking.TryGetProperty("passenger", out var passenger))
                            {
                                Console.WriteLine("PASSENGER info:");
                                Console.WriteLine($"  Name: {TryGetPropertyString(passenger, new[] { "firstName", "FirstName" })} {TryGetPropertyString(passenger, new[] { "lastName", "LastName" })}");
                                Console.WriteLine($"  PassportNumber: {TryGetPropertyString(passenger, new[] { "passportNumber", "PassportNumber" })}");
                            }
                            break;
                        }
                    }
                }

                // Суудлын мэдээлэл
                var seatResponse = await httpClient.GetAsync($"{ApiBaseUrl}/api/flights/1/seats");
                if (seatResponse.IsSuccessStatusCode)
                {
                    var content = await seatResponse.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(content);
                    var seats = result.GetProperty("data").EnumerateArray();

                    foreach (var seat in seats)
                    {
                        var seatNumber = TryGetPropertyString(seat, new[] { "seatNumber", "SeatNumber" });
                        if (seatNumber == user.AssignedSeat)
                        {
                            Console.WriteLine("SEAT info:");
                            Console.WriteLine($"  SeatId: {GetPropertyValue(seat, new[] { "seatId", "SeatId", "id" })}");
                            Console.WriteLine($"  SeatNumber: {seatNumber}");
                            Console.WriteLine($"  IsBooked: {TryGetPropertyValue(seat, new[] { "isBooked", "IsBooked" }) == 1}");
                            Console.WriteLine($"  Class: {TryGetPropertyString(seat, new[] { "class", "Class" })}");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving information: {ex.Message}");
            }
        }

        // JSON property утга авах helper функц
        static int GetPropertyValue(JsonElement element, string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                if (element.TryGetProperty(name, out var prop))
                {
                    return prop.GetInt32();
                }
            }
            throw new Exception($"Property not found: {string.Join(", ", possibleNames)}");
        }

        static int TryGetPropertyValue(JsonElement element, string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                if (element.TryGetProperty(name, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.True) return 1;
                    if (prop.ValueKind == JsonValueKind.False) return 0;
                    return prop.GetInt32();
                }
            }
            return 0;
        }

        static string TryGetPropertyString(JsonElement element, string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                if (element.TryGetProperty(name, out var prop))
                {
                    return prop.GetString() ?? "";
                }
            }
            return "";
        }
    }

    class RaceTestUser
    {
        public string PassportNumber { get; }
        public string Name { get; }
        public bool CheckInSuccessful { get; private set; }
        public string AssignedSeat { get; private set; }
        public string ErrorMessage { get; private set; }

        private HubConnection signalRConnection;
        private readonly HttpClient httpClient = new HttpClient();

        public RaceTestUser(string passportNumber, string name)
        {
            PassportNumber = passportNumber;
            Name = name;
        }

        public async Task Connect()
        {
            signalRConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5001/flighthub")
                .Build();

            signalRConnection.On<string, string, string>("SeatReserved", (flight, seat, booking) =>
            {
                if (booking == PassportNumber)
                {
                    Console.WriteLine($"{Name}: Seat {seat} reserved");
                }
            });

            signalRConnection.On<string, string, string>("SeatBooked", (flight, seat, booking) =>
            {
                if (booking.Contains(PassportNumber))
                {
                    Console.WriteLine($"{Name}: Seat {seat} permanently booked");
                }
            });

            signalRConnection.On<string, string, string>("SeatReservationFailed", (flight, seat, reason) =>
            {
                Console.WriteLine($"{Name}: Seat {seat} reservation failed - {reason}");
            });

            await signalRConnection.StartAsync();
            await signalRConnection.InvokeAsync("SubscribeToFlightUpdates", "OM201");

            Console.WriteLine($"{Name}: Connected to SignalR");
        }

        public async Task TryCheckIn(string targetSeat)
        {
            try
            {
                Console.WriteLine($"{Name}: Starting check-in for seat {targetSeat}");
                var bookingId = await GetBookingId();
                if (bookingId == 0)
                {
                    ErrorMessage = "No booking found";
                    return;
                }

                var seatId = await GetSeatId(targetSeat);
                if (seatId == 0)
                {
                    ErrorMessage = $"Seat {targetSeat} not found";
                    return;
                }

                // SignalR арқылы суудал захиалах
                await signalRConnection.InvokeAsync("ReserveSeatAsync", 1, targetSeat, PassportNumber);
                await Task.Delay(100);

                // API арқылы бүртгэл хийх
                var checkInRequest = new { BookingId = bookingId, SeatId = seatId };
                var response = await httpClient.PostAsJsonAsync("http://localhost:5001/api/checkin", checkInRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var success = result.GetProperty("success").GetBoolean();

                    if (success)
                    {
                        CheckInSuccessful = true;
                        AssignedSeat = targetSeat;
                        Console.WriteLine($"{Name}: Check-in successful");

                        await signalRConnection.InvokeAsync("ConfirmSeatBookingAsync", "OM201", targetSeat, PassportNumber);
                    }
                    else
                    {
                        var message = result.GetProperty("message").GetString();
                        ErrorMessage = message;
                        Console.WriteLine($"{Name}: Check-in failed - {message}");
                    }
                }
                else
                {
                    ErrorMessage = $"HTTP {response.StatusCode}";
                    Console.WriteLine($"{Name}: HTTP error - {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Console.WriteLine($"{Name}: Exception - {ex.Message}");
            }
        }

        private async Task<int> GetBookingId()
        {
            try
            {
                var response = await httpClient.GetAsync($"http://localhost:5001/api/bookings/passport/{PassportNumber}");
                if (!response.IsSuccessStatusCode) return 0;

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                if (!result.GetProperty("success").GetBoolean()) return 0;

                var bookingsArray = result.GetProperty("data");
                foreach (var booking in bookingsArray.EnumerateArray())
                {
                    var isCheckedIn = TryGetBoolProperty(booking, new[] { "isCheckedIn", "IsCheckedIn" });
                    if (!isCheckedIn)
                    {
                        return GetIntProperty(booking, new[] { "bookingId", "BookingId" });
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> GetSeatId(string seatNumber)
        {
            try
            {
                var response = await httpClient.GetAsync("http://localhost:5001/api/flights/1/seats");
                if (!response.IsSuccessStatusCode) return 0;

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                if (!result.GetProperty("success").GetBoolean()) return 0;

                var seatsArray = result.GetProperty("data");
                foreach (var seat in seatsArray.EnumerateArray())
                {
                    var currentSeatNumber = GetStringProperty(seat, new[] { "seatNumber", "SeatNumber" });
                    if (currentSeatNumber == seatNumber)
                    {
                        return GetIntProperty(seat, new[] { "seatId", "SeatId", "id" });
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        // Helper функцууд
        private int GetIntProperty(JsonElement element, string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                if (element.TryGetProperty(name, out var prop))
                {
                    return prop.GetInt32();
                }
            }
            return 0;
        }

        private string GetStringProperty(JsonElement element, string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                if (element.TryGetProperty(name, out var prop))
                {
                    return prop.GetString() ?? "";
                }
            }
            return "";
        }

        private bool TryGetBoolProperty(JsonElement element, string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                if (element.TryGetProperty(name, out var prop))
                {
                    return prop.GetBoolean();
                }
            }
            return false;
        }

        public async Task Disconnect()
        {
            if (signalRConnection != null)
            {
                await signalRConnection.DisposeAsync();
                Console.WriteLine($"{Name}: Disconnected");
            }
        }
    }
}