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

            var serverUrl = "http://localhost:5001";
            Console.WriteLine($"Using server URL: {serverUrl}");

            builder.Services.AddScoped(sp => {
                var httpClient = new HttpClient { 
                    BaseAddress = new Uri(serverUrl) 
                };
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                return httpClient;
            });
            
            var hubUrl = $"{serverUrl}/flighthub";
            builder.Services.AddTransient<HubConnection>(sp => {
                
                Console.WriteLine($"Configuring SignalR hub connection to: {hubUrl}");
                
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options => {
                                                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                                            Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents |
                                            Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                        
                                                options.SkipNegotiation = false;
                        
                                            })
                    .WithAutomaticReconnect(new[] { 
                        TimeSpan.Zero, 
                        TimeSpan.FromSeconds(2), 
                        TimeSpan.FromSeconds(5), 
                        TimeSpan.FromSeconds(10), 
                        TimeSpan.FromSeconds(15)
                    })
                    .Build();
                
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