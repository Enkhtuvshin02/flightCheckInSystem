using System;
using System.Windows.Forms;
using FlightCheckInSystem.FormsApp.Services;

namespace FlightCheckInSystem.FormsApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Create and configure the API service
            var apiService = new ApiService();

            // Create and show the main form with the API service
            var mainForm = new MainForm(apiService);
            Application.Run(mainForm);
        }
    }
}