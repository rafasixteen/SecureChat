using Client.Extensions;
using Shared.DTOs;
using System.Text;

namespace Client.Forms
{
    public partial class LoginForm : AuthForm
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        // Handler de sucesso no login
        private void OnLoginSuccess(byte[] data)
        {
            Invoke(() =>
            {
                LoginResponse response = Serializer.Deserialize<LoginResponse>(data);
                AppSession.Username.Value = response.Username;
                AppSession.LoggedIn?.Invoke();
                Close();
            });
        }

        // Handler de falha no login
        private void OnLoginFailed(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        // Clique do botão de login, envia packet de login
        private async void LoginButton_Click(object sender, EventArgs e)
        {
            string username = _textBoxUsername.Text;
            string password = _textBoxPassword.Text;

            await AppSession.Connection.SendLoginPacketAsync(username, password);
        }

        // On Load, quando o form inicia
        private void LoginForm_Load(object sender, EventArgs e)
        {
            // Configura os handlers para os eventos corretos
            AppSession.Connection.On("login-success", OnLoginSuccess);
            AppSession.Connection.On("login-failed", OnLoginFailed);
        }

        // Link label para ir para o form de registo
        private void CreateAccountLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SwitchToOther();
        }
    }
}