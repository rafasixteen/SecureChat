namespace SecureChat.Server
{
    public class Program
    {
        public const int PORT = 8080;

        public static void Main(string[] args)
        {
            Console.WriteLine($"[Server] Starting on port {PORT}...");

            ChatServer server = new();
            Thread serverThread = new(() => server.Start(PORT));
            serverThread.Start();

            Console.WriteLine("[Server] Running. Press ENTER to stop.");
            Console.ReadLine();

            server.Stop();

            Console.WriteLine("[Server] Stopped.");
        }
    }
}