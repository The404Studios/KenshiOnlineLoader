using System;
using System.IO;
using System.Threading;

namespace KenshiMultiplayerLoader.UI
{
    public static class Logger
    {
        private static string logFilePath;
        private static readonly object logLock = new object();
        private static bool isInitialized = false;
        private static int logLevel = 3; // 0=None, 1=Error, 2=Warning, 3=Info, 4=Debug
        
        // Log file rotation
        private static long maxLogSize = 5 * 1024 * 1024; // 5MB
        private static int maxLogFiles = 3;
        
        public static void Initialize(string filePath, int level = 3)
        {
            logFilePath = filePath;
            logLevel = level;
            
            try
            {
                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Check if log rotation is needed
                RotateLogFileIfNeeded();
                
                // Initialize the log file
                File.WriteAllText(logFilePath, $"=== Kenshi Multiplayer Log Started {DateTime.Now} ===\n");
                isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize log file: {ex.Message}");
            }
        }
        
        public static void SetLogLevel(int level)
        {
            if (level >= 0 && level <= 4)
            {
                logLevel = level;
                Log($"Log level set to {logLevel}", 3);
            }
        }
        
        public static void Log(string message, int level = 3)
        {
            // Skip if message level is higher than current log level
            if (level > logLevel)
                return;
                
            string logType;
            switch (level)
            {
                case 1: logType = "ERROR"; break;
                case 2: logType = "WARN"; break;
                case 3: logType = "INFO"; break;
                case 4: logType = "DEBUG"; break;
                default: logType = "INFO"; break;
            }
            
            string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logType}] {message}";
            
            // Print to console for debugging
            Console.WriteLine(formattedMessage);
            
            if (!isInitialized || string.IsNullOrEmpty(logFilePath))
                return;
                
            try
            {
                lock (logLock)
                {
                    File.AppendAllText(logFilePath, formattedMessage + Environment.NewLine);
                    
                    // Check if log rotation is needed after writing
                    RotateLogFileIfNeeded();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
        
        public static void Error(string message)
        {
            Log(message, 1);
        }
        
        public static void Warning(string message)
        {
            Log(message, 2);
        }
        
        public static void Info(string message)
        {
            Log(message, 3);
        }
        
        public static void Debug(string message)
        {
            Log(message, 4);
        }
        
        public static void LogException(Exception ex, string context = "")
        {
            string message = string.IsNullOrEmpty(context) 
                ? $"Exception: {ex.Message}" 
                : $"Exception in {context}: {ex.Message}";
                
            Error(message);
            Debug($"Stack trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Debug($"Inner exception: {ex.InnerException.Message}");
                Debug($"Inner stack trace: {ex.InnerException.StackTrace}");
            }
        }
        
        private static void RotateLogFileIfNeeded()
        {
            if (!File.Exists(logFilePath))
                return;
                
            var fileInfo = new FileInfo(logFilePath);
            if (fileInfo.Length >= maxLogSize)
            {
                // Rotate log files
                for (int i = maxLogFiles - 1; i >= 1; i--)
                {
                    string oldLogFile = $"{logFilePath}.{i}";
                    string newLogFile = $"{logFilePath}.{i + 1}";
                    
                    if (File.Exists(oldLogFile))
                    {
                        if (i == maxLogFiles - 1)
                        {
                            File.Delete(oldLogFile);
                        }
                        else
                        {
                            File.Move(oldLogFile, newLogFile);
                        }
                    }
                }
                
                // Move current log to .1
                if (File.Exists(logFilePath))
                {
                    File.Move(logFilePath, $"{logFilePath}.1");
                }
                
                // Create a new log file
                File.WriteAllText(logFilePath, $"=== Kenshi Multiplayer Log Rotated {DateTime.Now} ===\n");
            }
        }
    }
}