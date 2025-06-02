using System;
using System.Diagnostics;
using System.IO;

namespace ClientLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Client Launcher Debug Info ---");
            Console.WriteLine($"AppDomain.CurrentDomain.BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");

            // --- Step 1: Find the actual solution directory ---
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string solutionDir = null;

            // Walk up the directory tree to find the .sln file
            // This is a robust way to find the main solution folder
            while (currentDir != null && !string.Equals(Path.GetPathRoot(currentDir), currentDir, StringComparison.OrdinalIgnoreCase))
            {
                if (Directory.GetFiles(currentDir, "*.sln").Length > 0)
                {
                    solutionDir = currentDir;
                    break;
                }
                currentDir = Path.GetDirectoryName(currentDir);
            }

            if (solutionDir == null)
            {
                Console.WriteLine("Error: Could not find the solution directory (.sln file) by walking up from the launcher's executable path.");
                Console.WriteLine("Please ensure the ClientLauncher.exe is run from within its project's bin folder in a standard solution structure.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Successfully identified Solution Directory: {solutionDir}");

            // --- Step 2: Construct the path to the Forms App executable ---
            string formsAppProjectName = "FlightCheckInSystem.FormsApp";
            // IMPORTANT: Make sure 'Debug' here matches your active Visual Studio build configuration
            string buildConfiguration = "Debug";
            // If you switch to 'Release' in Visual Studio, change this line to:
            // string buildConfiguration = "Release";

            // ***** THIS LINE HAS BEEN CORRECTED *****
            // We are adding "net8.0" to the path because modern .NET apps output to a subfolder
            // If your Forms App targets a different .NET version (e.g., .NET 6.0), change "net8.0" to "net6.0"
            string formsAppExePath = Path.Combine(solutionDir, formsAppProjectName, "bin", buildConfiguration, "net8.0", $"{formsAppProjectName}.exe");
            // ***************************************

            Console.WriteLine($"Calculated Forms App Path: {formsAppExePath}");

            // --- Step 3: Verify the Forms App executable exists ---
            if (!File.Exists(formsAppExePath))
            {
                Console.WriteLine($"Error: Forms application executable not found at '{formsAppExePath}'");
                Console.WriteLine("Possible reasons:");
                Console.WriteLine("1. The 'FlightCheckInSystem.FormsApp' project has not been built successfully.");
                Console.WriteLine("2. The build configuration in ClientLauncher (Debug/Release) does not match the FormsApp build configuration.");
                Console.WriteLine("3. The project name 'FlightCheckInSystem.FormsApp' is misspelled or the folder structure is different.");
                Console.WriteLine("4. The target framework ('net8.0' in the code) does not match the actual target framework of FlightCheckInSystem.FormsApp.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(); // Keep console open to read error messages
                return;
            }

            // --- Step 4: Launch multiple instances of the Forms App ---
            int numberOfClientsToLaunch = 3; // You can change this number
            Console.WriteLine($"Launching {numberOfClientsToLaunch} client instances...");

            for (int i = 0; i < numberOfClientsToLaunch; i++)
            {
                try
                {
                    // Optionally pass unique arguments to each client if your forms app needs them
                    // For example: Process.Start(formsAppExePath, $"--clientId {i + 1}");
                    Process.Start(formsAppExePath); // Launch without arguments for simplicity
                    Console.WriteLine($"Client {i + 1} launched successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to launch client {i + 1}. Error: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                }
            }

            Console.WriteLine("\n--- All launch attempts completed ---");
            Console.WriteLine("Press any key to close the Client Launcher console...");
            Console.ReadKey();
        }
    }
}