using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightCheckInSystem.Core.Models;
using FlightCheckInSystem.Core.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Text;

namespace FlightCheckInSystem.FormsApp.Services
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }

    public class BookingResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }

    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:5001";
        private readonly string _fallbackBaseUrl = "http://localhost:5000";
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _useHttps = true;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };

            TestServerConnectionAsync().ConfigureAwait(false);

            Debug.WriteLine($"[ApiService] Initialized with base URL: {_baseUrl}");
        }

        private async Task TestServerConnectionAsync()
        {
            try
            {
                string currentUrl = GetCurrentBaseUrl();
                Debug.WriteLine($"[ApiService] Testing connection to {currentUrl}");

                if (!await TestConnectionAsync(currentUrl))
                {
                    Debug.WriteLine($"[ApiService] Failed to connect to {currentUrl}.");

                                        _useHttps = false;
                    string fallbackUrl = GetCurrentBaseUrl();
                    Debug.WriteLine($"[ApiService] Switching to fallback URL: {fallbackUrl}");

                    if (!await TestConnectionAsync(fallbackUrl))
                    {
                        Debug.WriteLine($"[ApiService] Error connecting to fallback URL: {fallbackUrl}");
                                                _useHttps = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error connecting to primary URL: {ex.Message}");
            }
        }

        private async Task<bool> TestConnectionAsync(string baseUrl)
        {
            try
            {
                Debug.WriteLine($"[ApiService] Testing connection to {baseUrl}");

                var response = await _httpClient.GetAsync($"{baseUrl}/flights");
                Debug.WriteLine($"[ApiService] API endpoint test to {baseUrl}/flights returned: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Connection test to {baseUrl} failed: {ex.Message}");
                return false;
            }
        }

                private string GetCurrentBaseUrl()
        {
            return _useHttps ? _baseUrl : _fallbackBaseUrl;
        }

                private void LogApiCall(string method, string endpoint, string details = null)
        {
            string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] API Call: {method} {endpoint}";
            if (!string.IsNullOrEmpty(details))
            {
                message += $" | {details}";
            }
            Debug.WriteLine(message);
            Console.WriteLine(message);
        }

                private void LogApiResponse<T>(string endpoint, ApiResponse<T> response, Exception ex = null)
        {
            string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] API Response from {endpoint}";
            if (response != null)
            {
                message += $" | Success: {response.Success} | Message: {response.Message}";
                if (response.Data != null)
                {
                    if (typeof(T) == typeof(List<Flight>))
                    {
                        var flights = response.Data as List<Flight>;
                        message += $" | Flights count: {flights?.Count ?? 0}";
                    }
                    else if (typeof(T) == typeof(List<Booking>))
                    {
                        var bookings = response.Data as List<Booking>;
                        message += $" | Bookings count: {bookings?.Count ?? 0}";
                    }
                    else if (typeof(T) == typeof(List<Seat>))
                    {
                        var seats = response.Data as List<Seat>;
                        message += $" | Seats count: {seats?.Count ?? 0}";
                    }
                    else if (typeof(T) == typeof(Flight))
                    {
                        var flight = response.Data as Flight;
                        message += $" | Flight: {flight?.FlightNumber}";
                    }
                    else if (typeof(T) == typeof(Booking))
                    {
                        var booking = response.Data as Booking;
                        message += $" | Booking ID: {booking?.BookingId}";
                    }
                }
            }
            else if (ex != null)
            {
                message += $" | Exception: {ex.Message}";
                if (ex is HttpRequestException httpEx)
                {
                    message += $" | Status: {httpEx.StatusCode}";
                }
            }
            Debug.WriteLine(message);
            Console.WriteLine(message);
        }

                public async Task<List<Flight>> GetFlightsAsync()
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights";

            try
            {
                LogApiCall("GET", endpoint);

                var response = await _httpClient.GetAsync(endpoint);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Flight>>>(responseContent, _jsonOptions);
                LogApiResponse(endpoint, apiResponse);

                if (apiResponse != null && apiResponse.Success)
                {
                    Debug.WriteLine($"[ApiService] Successfully retrieved {apiResponse.Data?.Count ?? 0} flights");
                    return apiResponse.Data;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return new List<Flight>();
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ApiService] HTTP request error getting flights: {ex.Message}");
                Debug.WriteLine($"[ApiService] Status code: {ex.StatusCode}, Inner exception: {ex.InnerException?.Message}");
                LogApiResponse<List<Flight>>(endpoint, null, ex);

                                if (_useHttps)
                {
                    Debug.WriteLine($"[ApiService] Trying fallback URL for GetFlightsAsync");
                    _useHttps = false;

                                        bool fallbackAvailable = await TestConnectionAsync(GetCurrentBaseUrl());
                    if (fallbackAvailable)
                    {
                        Debug.WriteLine($"[ApiService] Fallback URL is available, retrying flights retrieval");
                        try
                        {
                            var result = await GetFlightsAsync();
                            return result;
                        }
                        catch (Exception fallbackEx)
                        {
                            Debug.WriteLine($"[ApiService] Fallback URL also failed: {fallbackEx.Message}");
                            _useHttps = true;                             return new List<Flight>();
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"[ApiService] Fallback URL is not available");
                        _useHttps = true;                         return new List<Flight>();
                    }
                }

                return new List<Flight>();
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"[ApiService] JSON deserialization error: {ex.Message}");
                LogApiResponse<List<Flight>>(endpoint, null, ex);
                return new List<Flight>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error getting flights: {ex.Message}");
                Debug.WriteLine($"[ApiService] Exception type: {ex.GetType().Name}");
                Debug.WriteLine($"[ApiService] Stack trace: {ex.StackTrace}");
                LogApiResponse<List<Flight>>(endpoint, null, ex);

                                if (ex is System.Net.Sockets.SocketException socketEx)
                {
                    Debug.WriteLine($"[ApiService] Socket error code: {socketEx.ErrorCode}, Native error code: {socketEx.NativeErrorCode}");
                    Debug.WriteLine($"[ApiService] Socket type: {socketEx.SocketErrorCode}");
                }
                else if (ex is TaskCanceledException)
                {
                    Debug.WriteLine($"[ApiService] Request timed out. The server might be overloaded or the network connection is unstable.");
                }


                return new List<Flight>();
            }
        }

        public async Task<Flight> GetFlightByIdAsync(int flightId)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights/{flightId}";
            try
            {
                LogApiCall("GET", endpoint);
                var response = await _httpClient.GetAsync(endpoint);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<Flight>>(responseContent, _jsonOptions);
                LogApiResponse(endpoint, apiResponse);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ApiService] HTTP request error getting flight: {ex.Message}");
                Debug.WriteLine($"[ApiService] Status code: {ex.StatusCode}, Inner exception: {ex.InnerException?.Message}");
                LogApiResponse<Flight>(endpoint, null, ex);

                                if (_useHttps)
                {
                    Debug.WriteLine($"[ApiService] Trying fallback URL for GetFlightByIdAsync");
                    _useHttps = false;
                    try
                    {
                        var result = await GetFlightByIdAsync(flightId);
                        return result;
                    }
                    catch
                    {
                        _useHttps = true;                         return null;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error getting flight: {ex.Message}");
                LogApiResponse<Flight>(endpoint, null, ex);
                return null;
            }
        }

        public async Task<Flight> GetFlightByNumberAsync(string flightNumber)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights/number/{flightNumber}";
            try
            {
                LogApiCall("GET", endpoint);

                var response = await _httpClient.GetAsync(endpoint);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<Flight>>(responseContent, _jsonOptions);
                LogApiResponse(endpoint, apiResponse);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error getting flight by number: {ex.Message}");
                LogApiResponse<Flight>(endpoint, null, ex);
                return null;
            }
        }

        public async Task<List<Seat>> GetAvailableSeatsAsync(int flightId)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights/{flightId}/availableseats";
            try
            {
                LogApiCall("GET", endpoint);

                var response = await _httpClient.GetAsync(endpoint);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Seat>>>(responseContent, _jsonOptions);
                LogApiResponse(endpoint, apiResponse);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return new List<Seat>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error getting available seats: {ex.Message}");
                LogApiResponse<List<Seat>>(endpoint, null, ex);
                return new List<Seat>();
            }
        }

                                        public async Task<List<Seat>> GetSeatsByFlightAsync(int flightId)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights/{flightId}/seats";
            try
            {
                                                                                                

                                                                                                                                                                                                                                                                                                                                                                                                                                
                                LogApiCall("GET", endpoint);
                var response = await _httpClient.GetAsync(endpoint);
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Seat>>>(responseContent, _jsonOptions);
                LogApiResponse(endpoint, apiResponse);
                if (apiResponse != null && apiResponse.Success)
                {
                    Debug.WriteLine($"[ApiService] Successfully retrieved {apiResponse.Data?.Count ?? 0} seats for flight {flightId}");
                    return apiResponse.Data ?? new List<Seat>();
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return new List<Seat>();
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ApiService] HTTP request error getting flight seats: {ex.Message}");
                LogApiResponse<List<Seat>>(endpoint, null, ex);
                return new List<Seat>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error getting flight seats: {ex.Message}");
                LogApiResponse<List<Seat>>(endpoint, null, ex);
                return new List<Seat>();
            }
        }

                

        public async Task<List<Booking>> GetBookingsAsync()
        {
            string endpoint = $"{GetCurrentBaseUrl()}/bookings";
            try
            {
                LogApiCall("GET", endpoint);

                var response = await _httpClient.GetAsync(endpoint);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Booking>>>(responseContent, _jsonOptions);
                LogApiResponse(endpoint, apiResponse);

                if (apiResponse != null && apiResponse.Success)
                {
                    Debug.WriteLine($"[ApiService] Successfully retrieved {apiResponse.Data?.Count ?? 0} bookings");
                    return apiResponse.Data;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return new List<Booking>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error getting bookings: {ex.Message}");
                LogApiResponse<List<Booking>>(endpoint, null, ex);
                return new List<Booking>();
            }
        }

        public async Task<Booking> GetBookingByIdAsync(int bookingId)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/bookings/{bookingId}";
            try
            {
                LogApiCall("GET", endpoint);

                var response = await _httpClient.GetAsync(endpoint);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<Booking>>(responseContent, _jsonOptions);
                LogApiResponse(endpoint, apiResponse);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error getting booking: {ex.Message}");
                LogApiResponse<Booking>(endpoint, null, ex);
                return null;
            }
        }

        public async Task<List<Booking>> GetBookingsByPassportAsync(string passportNumber)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/bookings/passport/{passportNumber}";
            try
            {
                LogApiCall("GET", endpoint);

                var response = await _httpClient.GetAsync(endpoint);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Booking>>>(responseContent, _jsonOptions);
                LogApiResponse(endpoint, apiResponse);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return new List<Booking>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error getting bookings by passport: {ex.Message}");
                LogApiResponse<List<Booking>>(endpoint, null, ex);
                return new List<Booking>();
            }
        }

                public async Task<Passenger> FindOrCreatePassengerAsync(string passportNumber, string firstName, string lastName, string email = null, string phone = null)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/bookings/findorcreatepassenger";
            try
            {
                LogApiCall("POST", endpoint);

                var passengerRequest = new
                {
                    PassportNumber = passportNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = phone
                };

                                string requestJson = JsonSerializer.Serialize(passengerRequest, _jsonOptions);
                Debug.WriteLine($"[ApiService] Passenger request payload: {requestJson}");

                var response = await _httpClient.PostAsJsonAsync(endpoint, passengerRequest);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<Passenger>>(responseContent, _jsonOptions);
                LogApiResponse<Passenger>(endpoint, apiResponse, ex: null);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error finding or creating passenger: {ex.Message}");
                LogApiResponse<Passenger>(endpoint, null, ex);
                return null;
            }
        }

        public async Task<Booking> FindBookingAsync(string passportNumber, int flightId)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/bookings/findbooking";
            try
            {
                LogApiCall("POST", endpoint);

                var bookingRequest = new
                {
                    PassportNumber = passportNumber,
                    FlightId = flightId
                };

                                string requestJson = JsonSerializer.Serialize(bookingRequest, _jsonOptions);
                Debug.WriteLine($"[ApiService] Booking request payload: {requestJson}");

                var response = await _httpClient.PostAsJsonAsync(endpoint, bookingRequest);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<Booking>>(responseContent, _jsonOptions);
                LogApiResponse(endpoint, apiResponse);

                if (apiResponse != null && apiResponse.Success)
                {
                    return apiResponse.Data;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error finding booking: {ex.Message}");
                LogApiResponse<Booking>(endpoint, null, ex);
                return null;
            }
        }

        public async Task<Booking> CreateBookingAsync(string flightNumber, string passportNumber, string firstName, string lastName, string email = null, string phone = null)
        {
            try
            {
                                Debug.WriteLine($"[ApiService] Finding or creating passenger with passport {passportNumber}");
                var passenger = await FindOrCreatePassengerAsync(passportNumber, firstName, lastName, email, phone);
                if (passenger == null)
                {
                    Debug.WriteLine("[ApiService] Failed to find or create passenger");
                    return null;
                }
                Debug.WriteLine($"[ApiService] Using passenger with ID {passenger.PassengerId}");

                                Debug.WriteLine($"[ApiService] Finding flight with number {flightNumber}");
                var flight = await GetFlightByNumberAsync(flightNumber);
                if (flight == null)
                {
                    Debug.WriteLine($"[ApiService] Flight {flightNumber} not found");
                    return null;
                }
                Debug.WriteLine($"[ApiService] Using flight with ID {flight.FlightId}");

                                Debug.WriteLine($"[ApiService] Checking if booking already exists for passenger {passenger.PassengerId} on flight {flight.FlightId}");
                var existingBooking = await FindBookingAsync(passportNumber, flight.FlightId);
                if (existingBooking != null)
                {
                    Debug.WriteLine($"[ApiService] Booking already exists for passenger {passenger.PassengerId} on flight {flight.FlightId}");
                    return existingBooking;
                }

                                var bookingRequest = new
                {
                    PassengerId = passenger.PassengerId,
                    FlightId = flight.FlightId,
                    ReservationDate = DateTime.Now
                };

                string endpoint = $"{GetCurrentBaseUrl()}/bookings";
                LogApiCall("POST", endpoint, $"Creating booking for {firstName} {lastName} on flight {flightNumber}");

                                string requestJson = JsonSerializer.Serialize(bookingRequest, _jsonOptions);
                Debug.WriteLine($"[ApiService] Booking request payload: {requestJson}");

                                Debug.WriteLine($"[ApiService] Sending POST request to {endpoint}");
                var response = await _httpClient.PostAsJsonAsync(endpoint, bookingRequest);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<Booking>>(responseContent, _jsonOptions);

                if (apiResponse != null && apiResponse.Success)
                {
                    Debug.WriteLine($"[ApiService] Successfully created booking with ID {apiResponse.Data?.BookingId}");
                    return apiResponse.Data;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ApiService] HTTP request error creating booking: {ex.Message}");
                Debug.WriteLine($"[ApiService] Status code: {ex.StatusCode}, Inner exception: {ex.InnerException?.Message}");

                                if (_useHttps)
                {
                    Debug.WriteLine($"[ApiService] Trying fallback URL for CreateBookingAsync");
                    _useHttps = false;

                                        bool fallbackAvailable = await TestConnectionAsync(GetCurrentBaseUrl());
                    if (fallbackAvailable)
                    {
                        Debug.WriteLine($"[ApiService] Fallback URL is available, retrying booking creation");
                        try
                        {
                            var result = await CreateBookingAsync(flightNumber, passportNumber, firstName, lastName, email, phone);
                            return result;
                        }
                        catch (Exception fallbackEx)
                        {
                            Debug.WriteLine($"[ApiService] Fallback URL also failed: {fallbackEx.Message}");
                            _useHttps = true;                             return null;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"[ApiService] Fallback URL is not available");
                        _useHttps = true;                         return null;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error creating booking: {ex.Message}");
                Debug.WriteLine($"[ApiService] Exception type: {ex.GetType().Name}");
                Debug.WriteLine($"[ApiService] Stack trace: {ex.StackTrace}");

                                if (ex is System.Net.Sockets.SocketException socketEx)
                {
                    Debug.WriteLine($"[ApiService] Socket error code: {socketEx.ErrorCode}, Native error code: {socketEx.NativeErrorCode}");
                    Debug.WriteLine($"[ApiService] Socket type: {socketEx.SocketErrorCode}");
                }
                else if (ex is TaskCanceledException)
                {
                    Debug.WriteLine($"[ApiService] Request timed out. The server might be overloaded or the network connection is unstable.");
                }


                return null;
            }
        } 
        public class CheckInApiResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public BoardingPass BoardingPass { get; set; }
        }

        public async Task<CheckInApiResponse> CheckInAsync(int bookingId, int seatId)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/checkin";
            try
            {
                LogApiCall("POST", endpoint, $"Checking in booking {bookingId} with seat {seatId}");
                var checkInRequest = new { BookingId = bookingId, SeatId = seatId };
                string requestJson = JsonSerializer.Serialize(checkInRequest, _jsonOptions);
                Debug.WriteLine($"[ApiService] Check-in request payload: {requestJson}");
                Debug.WriteLine($"[ApiService] Sending POST request to {endpoint}");
                var response = await _httpClient.PostAsJsonAsync(endpoint, checkInRequest);
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");
                var apiResponse = JsonSerializer.Deserialize<CheckInApiResponse>(responseContent, _jsonOptions);
                if (apiResponse != null && apiResponse.Success)
                {
                    Debug.WriteLine($"[ApiService] Successfully checked in booking {bookingId} with seat {seatId}");
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                }
                return apiResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error checking in: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[ApiService] Inner exception: {ex.InnerException.Message}");
                }
                return new CheckInApiResponse { Success = false, Message = ex.Message, BoardingPass = null };
            }
        }

                private void HandleHttpError(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP error: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        #region Flight Management Methods

                                public async Task<Flight> CreateFlightAsync(Flight flight)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights";
            try
            {
                LogApiCall("POST", endpoint, $"Flight: {flight.FlightNumber}");

                var response = await _httpClient.PostAsJsonAsync(endpoint, flight);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<Flight>>(responseContent, _jsonOptions);
                LogApiResponse(endpoint, apiResponse);

                if (apiResponse != null && apiResponse.Success)
                {
                    Debug.WriteLine($"[ApiService] Successfully created flight {apiResponse.Data?.FlightNumber}");
                    return apiResponse.Data;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ApiService] HTTP request error creating flight: {ex.Message}");
                LogApiResponse<Flight>(endpoint, null, ex);
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error creating flight: {ex.Message}");
                LogApiResponse<Flight>(endpoint, null, ex);
                return null;
            }
        }

                                public async Task<bool> UpdateFlightStatusAsync(int flightId, FlightStatus status)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights/{flightId}/status";
            try
            {
                LogApiCall("PUT", endpoint, $"Status: {status}");

                var statusRequest = new { Status = status };
                var response = await _httpClient.PutAsJsonAsync(endpoint, statusRequest);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<Flight?>>(responseContent, _jsonOptions);
                LogApiResponse<Flight?>(endpoint, apiResponse);

                if (apiResponse != null && apiResponse.Success)
                {
                    Debug.WriteLine($"[ApiService] Successfully updated flight status");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return false;
                }
                            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ApiService] HTTP request error updating flight status: {ex.Message}");
                LogApiResponse<Flight?>(endpoint, null, ex);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error updating flight status: {ex.Message}");
                LogApiResponse<Flight?>(endpoint, null, ex);
                return false;
            }
        }

                                public async Task<bool> UpdateFlightAsync(Flight flight)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights/{flight.FlightId}";
            try
            {
                                                
                LogApiCall("PUT", endpoint, $"Flight: {flight.FlightNumber}");

                var response = await _httpClient.PutAsJsonAsync(endpoint, flight);

                                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<bool>>(responseContent, _jsonOptions);
                LogApiResponse(endpoint, apiResponse);

                if (apiResponse != null && apiResponse.Success)
                {
                    Debug.WriteLine($"[ApiService] Successfully updated flight details");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ApiService] HTTP request error updating flight: {ex.Message}");
                LogApiResponse<bool>(endpoint, null, ex);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error updating flight: {ex.Message}");
                LogApiResponse<bool>(endpoint, null, ex);
                return false;
            }
        }
        #endregion
    }
}