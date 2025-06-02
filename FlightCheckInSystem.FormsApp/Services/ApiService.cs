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
        private readonly string _baseUrl = "https://localhost:7106/api";
        private readonly string _fallbackBaseUrl = "http://localhost:5001/api";
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

                    // Try fallback URL
                    _useHttps = false;
                    string fallbackUrl = GetCurrentBaseUrl();
                    Debug.WriteLine($"[ApiService] Switching to fallback URL: {fallbackUrl}");

                    if (!await TestConnectionAsync(fallbackUrl))
                    {
                        Debug.WriteLine($"[ApiService] Error connecting to fallback URL: {fallbackUrl}");
                        // Reset to HTTPS as default even if both fail
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

        // Get the current base URL based on the _useHttps flag
        private string GetCurrentBaseUrl()
        {
            return _useHttps ? _baseUrl : _fallbackBaseUrl;
        }

        // Helper method to log API calls
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

        // Helper method to log API responses
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

        // Flight-related methods
        public async Task<List<Flight>> GetFlightsAsync()
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights";

            try
            {
                LogApiCall("GET", endpoint);

                var response = await _httpClient.GetAsync(endpoint);

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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

                // Try the fallback URL if we haven't already
                if (_useHttps)
                {
                    Debug.WriteLine($"[ApiService] Trying fallback URL for GetFlightsAsync");
                    _useHttps = false;

                    // Test connection to fallback URL before attempting retry
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
                            _useHttps = true; // Reset to default
                            return new List<Flight>();
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"[ApiService] Fallback URL is not available");
                        _useHttps = true; // Reset to default
                        return new List<Flight>();
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

                // Log detailed diagnostics for network-related exceptions
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

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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

                // Try the fallback URL if we haven't already
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
                        _useHttps = true; // Reset to default
                        return null;
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

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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

        /// <summary>
        /// Get all seats for a flight
        /// </summary>
        // Removed the duplicate GetSeatsByFlightAsync method. Keep only the one with retry logic.
        public async Task<List<Seat>> GetSeatsByFlightAsync(int flightId)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights/{flightId}/seats";
            try
            {
                // Removed RetryHelper if it's not defined or you don't need it for this simple example.
                // If you have a RetryHelper, ensure it's in a using directive or accessible.
                // For now, let's make it a direct call to simplify compilation.
                // If you want to use RetryHelper, you need to define it.
                // Example of a basic RetryHelper (you'd put this in a separate file or a utility class):
                

                // Assuming you have a RetryHelper, this is how you'd use it:
                // return await RetryHelper.ExecuteWithRetryAsync<List<Seat>>(
                //     async () =>
                //     {
                //         LogApiCall("GET", endpoint);
                //         var response = await _httpClient.GetAsync(endpoint);
                //         Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");
                //         response.EnsureSuccessStatusCode();
                //         string responseContent = await response.Content.ReadAsStringAsync();
                //         Debug.WriteLine($"[ApiService] Response content: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");
                //         var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Seat>>>(responseContent, _jsonOptions);
                //         LogApiResponse(endpoint, apiResponse);
                //         if (apiResponse != null && apiResponse.Success)
                //         {
                //             Debug.WriteLine($"[ApiService] Successfully retrieved {apiResponse.Data?.Count ?? 0} seats for flight {flightId}");
                //             return apiResponse.Data ?? new List<Seat>();
                //         }
                //         else
                //         {
                //             Debug.WriteLine($"[ApiService] API returned unsuccessful response: {apiResponse?.Message}");
                //             return new List<Seat>();
                //         }
                //     },
                //     retryCount: 3,
                //     retryDelayMs: 500,
                //     operationName: "GetSeatsByFlightAsync");

                // Direct call without RetryHelper for now to resolve compilation errors if RetryHelper is missing
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

        // Removed the duplicate method GetFlightSeatsAsync, as GetSeatsByFlightAsync (above) serves the same purpose.
        // If you intended GetFlightSeatsAsync to be different, please clarify.


        public async Task<List<Booking>> GetBookingsAsync()
        {
            string endpoint = $"{GetCurrentBaseUrl()}/bookings";
            try
            {
                LogApiCall("GET", endpoint);

                var response = await _httpClient.GetAsync(endpoint);

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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

        // Method to find or create a passenger
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

                // Log the request payload
                string requestJson = JsonSerializer.Serialize(passengerRequest, _jsonOptions);
                Debug.WriteLine($"[ApiService] Passenger request payload: {requestJson}");

                var response = await _httpClient.PostAsJsonAsync(endpoint, passengerRequest);

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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

                // Log the request payload
                string requestJson = JsonSerializer.Serialize(bookingRequest, _jsonOptions);
                Debug.WriteLine($"[ApiService] Booking request payload: {requestJson}");

                var response = await _httpClient.PostAsJsonAsync(endpoint, bookingRequest);

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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
                // First, find or create the passenger
                Debug.WriteLine($"[ApiService] Finding or creating passenger with passport {passportNumber}");
                var passenger = await FindOrCreatePassengerAsync(passportNumber, firstName, lastName, email, phone);
                if (passenger == null)
                {
                    Debug.WriteLine("[ApiService] Failed to find or create passenger");
                    return null;
                }
                Debug.WriteLine($"[ApiService] Using passenger with ID {passenger.PassengerId}");

                // Then, find the flight
                Debug.WriteLine($"[ApiService] Finding flight with number {flightNumber}");
                var flight = await GetFlightByNumberAsync(flightNumber);
                if (flight == null)
                {
                    Debug.WriteLine($"[ApiService] Flight {flightNumber} not found");
                    return null;
                }
                Debug.WriteLine($"[ApiService] Using flight with ID {flight.FlightId}");

                // Check if booking already exists
                Debug.WriteLine($"[ApiService] Checking if booking already exists for passenger {passenger.PassengerId} on flight {flight.FlightId}");
                var existingBooking = await FindBookingAsync(passportNumber, flight.FlightId);
                if (existingBooking != null)
                {
                    Debug.WriteLine($"[ApiService] Booking already exists for passenger {passenger.PassengerId} on flight {flight.FlightId}");
                    return existingBooking;
                }

                // Create booking request
                var bookingRequest = new
                {
                    PassengerId = passenger.PassengerId,
                    FlightId = flight.FlightId,
                    ReservationDate = DateTime.Now
                };

                string endpoint = $"{GetCurrentBaseUrl()}/bookings";
                LogApiCall("POST", endpoint, $"Creating booking for {firstName} {lastName} on flight {flightNumber}");

                // Log the request payload
                string requestJson = JsonSerializer.Serialize(bookingRequest, _jsonOptions);
                Debug.WriteLine($"[ApiService] Booking request payload: {requestJson}");

                // Send request to create booking
                Debug.WriteLine($"[ApiService] Sending POST request to {endpoint}");
                var response = await _httpClient.PostAsJsonAsync(endpoint, bookingRequest);

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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

                // Try the fallback URL if we haven't already
                if (_useHttps)
                {
                    Debug.WriteLine($"[ApiService] Trying fallback URL for CreateBookingAsync");
                    _useHttps = false;

                    // Test connection to fallback URL before attempting retry
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
                            _useHttps = true; // Reset to default
                            return null;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"[ApiService] Fallback URL is not available");
                        _useHttps = true; // Reset to default
                        return null;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Error creating booking: {ex.Message}");
                Debug.WriteLine($"[ApiService] Exception type: {ex.GetType().Name}");
                Debug.WriteLine($"[ApiService] Stack trace: {ex.StackTrace}");

                // Log detailed diagnostics for network-related exceptions
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
        } // Added missing closing brace here

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

        // Helper method to handle HTTP errors
        private void HandleHttpError(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP error: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        #region Flight Management Methods

        /// <summary>
        /// Create a new flight
        /// </summary>
        public async Task<Flight> CreateFlightAsync(Flight flight)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights";
            try
            {
                LogApiCall("POST", endpoint, $"Flight: {flight.FlightNumber}");

                var response = await _httpClient.PostAsJsonAsync(endpoint, flight);

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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

        /// <summary>
        /// Update flight status
        /// </summary>
        public async Task<bool> UpdateFlightStatusAsync(int flightId, FlightStatus status)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights/{flightId}/status";
            try
            {
                LogApiCall("PUT", endpoint, $"Status: {status}");

                var statusRequest = new { Status = status };
                var response = await _httpClient.PutAsJsonAsync(endpoint, statusRequest);

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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
                // Removed misplaced retryCount and retryDelayMs parameters
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

        /// <summary>
        /// Update flight details
        /// </summary>
        public async Task<bool> UpdateFlightAsync(Flight flight)
        {
            string endpoint = $"{GetCurrentBaseUrl()}/flights/{flight.FlightId}";
            try
            {
                // Removed RetryHelper if it's not defined or you don't need it.
                // Assuming direct call for now.
                // If you want to use RetryHelper, define it first.

                LogApiCall("PUT", endpoint, $"Flight: {flight.FlightNumber}");

                var response = await _httpClient.PutAsJsonAsync(endpoint, flight);

                // Log the HTTP response status
                Debug.WriteLine($"[ApiService] Received HTTP response: {(int)response.StatusCode} {response.StatusCode} from {endpoint}");

                response.EnsureSuccessStatusCode();

                // Log the response content for debugging
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

        // The GetSeatsByFlightAsync method was previously duplicated and also had a separate
        // GetFlightSeatsAsync. I've consolidated to one `GetSeatsByFlightAsync` and removed the other.
        // If you intended GetFlightSeatsAsync to be distinct, please re-add it with a different purpose/logic.

        #endregion // End of Flight Management Methods
    }
}