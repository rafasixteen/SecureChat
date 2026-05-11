namespace Server
{
    public class Logger
    {
        private readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.log");

        private readonly object Lock = new();

        public void Log(string message, bool writeToConsole = false)
        {
            lock (Lock)
            {
                string text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";

                File.AppendAllText(LogFilePath, text);

                if (writeToConsole)
                    Console.WriteLine(message);
            }
        }

        public void LogError(string message, Exception? ex = default, bool writeToConsole = false)
        {
            lock (Lock)
            {
                string text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message} {ex?.ToString() ?? ""}{Environment.NewLine}";

                File.AppendAllText(LogFilePath, text);

                if (writeToConsole)
                    Console.WriteLine(text);
            }
        }
    }
}
