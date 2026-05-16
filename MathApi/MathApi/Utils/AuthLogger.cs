using System;
using System.IO;

namespace MathApi.Utils
{
    public sealed class AuthLogger
    {
        // The single instance of the class (Singleton Pattern)
        private static readonly AuthLogger instance = new AuthLogger();
        private readonly StreamWriter fileWriter;

        static AuthLogger() { }

        private AuthLogger()
        {
            // Inside a Docker container, this will save to the app folder
            fileWriter = new StreamWriter("auth_errors.log", true);
            fileWriter.AutoFlush = true;
        }

        public static AuthLogger Instance
        {
            get
            {
                return instance;
            }
        }

        public void LogError(string errorMessage)
        {
            string logMessage = $"{DateTime.Now} - ERROR: {errorMessage}";
            fileWriter.WriteLine(logMessage);
        }
    }
}