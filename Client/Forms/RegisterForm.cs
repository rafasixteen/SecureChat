using Client.Extensions;
using Shared.DTOs;
using Shared.DTOs.Shared.DTOs;
using System.Text;

namespace Client.Forms
{
    public partial class RegisterForm : AuthForm
    {
        public RegisterForm()
        {
            InitializeComponent();
        }

        // Handler de sucesso no registo
        private void OnRegisterSuccess(byte[] data)
        {
            Invoke(() =>
            {
                RegisterResponse response = Serializer.Deserialize<RegisterResponse>(data);
                AppSession.Username.Value = response.Username;
                AppSession.LoggedIn?.Invoke();
                Close();
            });
        }

        // Handler de falhar no registo
        private void OnRegistrationFailed(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        // Clique do botão de registo, envia packet de registo
        private async void ButtonRegister_Click(object sender, EventArgs e)
        {
            string username = _usernameTextBox.Text;
            string password = _passwordTextBox.Text;

            await AppSession.Connection.SendRegistrationPacketAsync(username, password);
        }

        // On Load, quando o form inicia, configurar os handlers para os eventos corretos
        private void RegisterForm_Load(object sender, EventArgs e)
        {
            AppSession.Connection.On("register-success", OnRegisterSuccess);
            AppSession.Connection.On("register-failed", OnRegistrationFailed);
        }

        // Link label para ir para o form de login
        private void HaveAccountLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SwitchToOther();
        }
    }
}