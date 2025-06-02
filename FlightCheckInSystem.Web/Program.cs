using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlightCheckInSystem.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // For local development, hardcode the server URL explicitly with the correct port
            // This matches the port from your server logs (7106)
            var serverUrl = "https://localhost:7106/";
            Console.WriteLine($"Using server URL: {serverUrl}");

            // Configure the HttpClient with explicit settings
            builder.Services.AddScoped(sp => {
                var httpClient = new HttpClient { 
                    BaseAddress = new Uri(serverUrl) 
                };
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                // Longer timeout for debugging
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                return httpClient;
            });
            
            // Configure SignalR with detailed settings
            builder.Services.AddTransient<HubConnection>(sp => {
                var hubUrl = $"{serverUrl.TrimEnd('/')}/flighthub";
                Console.WriteLine($"Configuring SignalR hub connection to: {hubUrl}");
                
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options => {
                        // Try different transports
                        options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                                            Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents |
                                            Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                        
                        // Skip negotiation for WebSockets
                        options.SkipNegotiation = false;
                        
                        // Log everything for troubleshooting
                    })
                    .WithAutomaticReconnect(new[] { 
                        TimeSpan.Zero, 
                        TimeSpan.FromSeconds(2), 
                        TimeSpan.FromSeconds(5), 
                        TimeSpan.FromSeconds(10), 
                        TimeSpan.FromSeconds(15)
                    })
                    .Build();
                
                // Add logging for connection lifecycle events
                hubConnection.Closed += async (error) => {
                    Console.WriteLine($"SignalR connection closed: {error?.Message ?? "No error"}");
                    await Task.CompletedTask;
                };
                
                hubConnection.Reconnecting += (error) => {
                    Console.WriteLine($"SignalR reconnecting: {error?.Message ?? "No error"}");
                    return Task.CompletedTask;
                };
                
                hubConnection.Reconnected += (connectionId) => {
                    Console.WriteLine($"SignalR reconnected with ID: {connectionId}");
                    return Task.CompletedTask;
                };
                
                return hubConnection;
            });

            var host = builder.Build();
            
            await host.RunAsync();
        }
    }
}