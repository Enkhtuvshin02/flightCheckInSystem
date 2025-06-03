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
                                [STAThread]
        static void Main()
        {
                                    ApplicationConfiguration.Initialize();

                        var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

                        var mainForm = new MainForm();
            Application.Run(mainForm);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
                        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FlightCheckIn.db");
            string connectionString = $"Data Source={dbPath};Version=3;";

                        services.AddSingleton<IFlightRepository>(provider => new FlightRepository(connectionString));
            services.AddSingleton<IPassengerRepository>(provider => new PassengerRepository(connectionString));
            services.AddSingleton<ISeatRepository>(provider => new SeatRepository(connectionString));
            services.AddSingleton<IBookingRepository>(provider => new BookingRepository(connectionString));

                        services.AddSingleton<IFlightManagementService, FlightManagementService>();
            services.AddSingleton<ICheckInService, CheckInService>();
            services.AddSingleton<IBookingService, BookingService>();

                        if (!File.Exists(dbPath))
            {
                var dbInitializer = new FlightCheckInSystem.Data.DatabaseInitializer(dbPath);
                dbInitializer.InitializeAsync().Wait();
            }
        }
    }
}