using System;
using System.IO;
using System.Windows.Forms;
using FlightCheckInSystem.Business.Interfaces;
using FlightCheckInSystem.Business.Services;
using FlightCheckInSystem.Data.Interfaces;
using FlightCheckInSystem.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FlightCheckInSystem.FormsApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Setup dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // Create main form with DI
            var mainForm = new MainForm();
            Application.Run(mainForm);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // Get the database path
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FlightCheckIn.db");
            string connectionString = $"Data Source={dbPath};Version=3;";

            // Register repositories
            services.AddSingleton<IFlightRepository>(provider => new FlightRepository(connectionString));
            services.AddSingleton<IPassengerRepository>(provider => new PassengerRepository(connectionString));
            services.AddSingleton<ISeatRepository>(provider => new SeatRepository(connectionString));
            services.AddSingleton<IBookingRepository>(provider => new BookingRepository(connectionString));

            // Register services
            services.AddSingleton<IFlightManagementService, FlightManagementService>();
            services.AddSingleton<ICheckInService, CheckInService>();
            services.AddSingleton<IBookingService, BookingService>();

            // Initialize database if it doesn't exist
            if (!File.Exists(dbPath))
            {
                var dbInitializer = new FlightCheckInSystem.Data.DatabaseInitializer(dbPath);
                dbInitializer.InitializeAsync().Wait();
            }
        }
    }
}