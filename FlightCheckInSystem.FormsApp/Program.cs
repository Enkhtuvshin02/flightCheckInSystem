using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlightCheckInSystem.FormsApp.Services;

namespace FlightCheckInSystem.FormsApp
{
    internal static class Program
    {
        [STAThread]
        static async Task Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Create and configure the API service
                var apiService = new ApiService(); // Adjust URL as needed

                // Create and configure the SignalR service
                var signalRService = new SeatStatusSignalRService("http://localhost:5001/flighthub"); // Adjust URL as needed

                // Start SignalR connection
                await signalRService.StartAsync();

                // Create and show the main form with both services
                var mainForm = new MainForm(apiService, signalRService);
                Application.Run(mainForm);

                // Clean up SignalR when application exits
                await signalRService.StopAsync();
                signalRService.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize application: {ex.Message}",
                    "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}