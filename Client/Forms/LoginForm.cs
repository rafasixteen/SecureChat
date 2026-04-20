using Client.Extensions;
using Client.State;
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

        private void OnLoginSuccess(byte[] data)
        {
            Invoke(() =>
            {
                LoginResponse response = Serializer.Deserialize<LoginResponse>(data);
                AppState.Username.Value = response.Username;
                AppState.LoggedIn?.Invoke();
                Close();
            });
        }

        private void OnLoginFailed(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            string username = _textBoxUsername.Text;
            string password = _textBoxPassword.Text;

            await AppState.Connection.SendLoginPacketAsync(username, password);
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            AppState.Connection.On("login-success", OnLoginSuccess);
            AppState.Connection.On("login-failed", OnLoginFailed);
        }

        private void CreateAccountLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SwitchToOther();
        }
    }
}