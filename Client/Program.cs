using Client.Forms;

namespace Client
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Iniciar a aplicação com o formulário de chat
            Application.Run(new ChatForm());
        }
    }
}