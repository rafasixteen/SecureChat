using Client.Forms;
using Client.State;
using Client.Transport;
using Microsoft.Extensions.DependencyInjection;
using System.Formats.Tar;

namespace Client
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            ServiceCollection services = new();

            services.AddSingleton<ApplicationContext>();
            services.AddSingleton<AppState>();
            services.AddSingleton<ClientConnection>();

            services.AddTransient<LoginForm>();
            services.AddTransient<RegisterForm>();
            services.AddTransient<ChatForm>();

            ServiceProvider provider = services.BuildServiceProvider();

            ApplicationConfiguration.Initialize();

            ApplicationContext context = provider.GetRequiredService<ApplicationContext>();
            context.MainForm = provider.GetRequiredService<ChatForm>();

            Application.Run(context);
        }

        public static void NavigateTo<TForm>(IServiceProvider provider, bool dialog = false) where TForm : Form
        {
            ApplicationContext context = provider.GetRequiredService<ApplicationContext>();
            TForm next = provider.GetRequiredService<TForm>();

            if (dialog)
            {
                Form? owner = context.MainForm;

                if (owner is not null)
                {
                    next.ShowDialog(owner);
                }
                else
                {
                    next.ShowDialog();
                }

                return;
            }

            Form? previous = context.MainForm;
            context.MainForm = next;

            next.Show();

            // Prevent Navigation to close the Chat Form
            // Note: ChatForm holds the connection to the server, if it closes, the connection is severed
            if (previous is not ChatForm)
                previous?.Close();
        }

        public static void SwitchDialog<TNext>(IServiceProvider provider, Form current) where TNext : Form
        {
            TNext next = provider.GetRequiredService<TNext>();

            // Hides the current Form
            current.Hide();

            // Shows the next one
            next.ShowDialog();

            // Closes the current one
            current.Close();
        }
    }
}