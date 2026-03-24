namespace SecureChat.Client
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Thread t1 = new Thread(() => Application.Run(new LoginForm()));
            t1.SetApartmentState(ApartmentState.STA);
            t1.Start();

            Thread t2 = new Thread(() => Application.Run(new LoginForm()));
            t2.SetApartmentState(ApartmentState.STA);
            t2.Start();

            t1.Join();
            t2.Join();
        }
    }
}