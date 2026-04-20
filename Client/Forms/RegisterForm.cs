using Client.Extensions;
using Client.State;
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

        private void OnRegisterSuccess(byte[] data)
        {
            Invoke(() =>
            {
                RegisterResponse response = Serializer.Deserialize<RegisterResponse>(data);
                AppState.Username.Value = response.Username;
                AppState.LoggedIn?.Invoke();
                Close();
            });
        }

        private void OnRegistrationFailed(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private async void ButtonRegister_Click(object sender, EventArgs e)
        {
            string username = _usernameTextBox.Text;
            string password = _passwordTextBox.Text;

            await AppState.Connection.SendRegistrationPacketAsync(username, password);
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {
            AppState.Connection.On("register-success", OnRegisterSuccess);
            AppState.Connection.On("register-failed", OnRegistrationFailed);
        }

        private void HaveAccountLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SwitchToOther();
        }
    }
}