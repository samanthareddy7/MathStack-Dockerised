using System;
using System.IO;

namespace MathApi_Client.Utils
{
    public sealed class AuthLogger
    {
        // The single instance of the class
        private static readonly AuthLogger instance = new AuthLogger();
        private readonly StreamWriter fileWriter;

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static AuthLogger() { }

        // Private constructor so no one else can create an instance
        private AuthLogger()
        {
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