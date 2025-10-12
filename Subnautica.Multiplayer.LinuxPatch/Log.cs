using System;
using System.IO;

namespace Subnautica.Multiplayer.LinuxPatch
{
    public static class Logger
    {
        private static readonly string logFilePath;

        static Logger()
        {
            // Get the Documents folder path for the current user
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            // Set the log file path
            logFilePath = Path.Combine(documentsFolder, "linux.log");

            try
            {
                // Ensure the file exists
                if (!File.Exists(logFilePath))
                {
                    using (File.Create(logFilePath)) { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize logger: {ex.Message}");
            }
        }

        /// <summary>
        /// Writes a line to the log file with a timestamp.
        /// </summary>
        public static void Log(string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log: {ex.Message}");
            }
        }

        /// <summary>
        /// Writes a line to the log file with a timestamp and exception details.
        /// </summary>
        public static void LogException(Exception ex, string message = "")
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message} Exception: {ex}";
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                Console.WriteLine("Failed to write exception to log.");
            }
        }
    }
}