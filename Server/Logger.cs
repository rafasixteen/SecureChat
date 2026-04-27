namespace Server
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.log");

        private static readonly object Lock = new();

        public static void Log(string message)
        {
            lock (Lock)
            {
                File.AppendAllText(LogFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
        }

        public static void LogError(string message, Exception? ex = default)
        {
            lock (Lock)
            {
                File.AppendAllText(LogFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message} {ex?.ToString() ?? ""}{Environment.NewLine}");
            }
        }
    }
}
