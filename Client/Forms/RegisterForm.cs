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

        #region Control Event Handlers

        private void RegisterForm_Load(object sender, EventArgs e)
        {
            AppState.Connection.On("register-success", OnRegisterSuccess);
            AppState.Connection.On("register-failed", OnRegistrationFailed);
        }

        private void RegisterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppState.Connection.RemoveHandler("register-success");
            AppState.Connection.RemoveHandler("register-failed");
        }

        private async void ButtonRegister_Click(object sender, EventArgs e)
        {
            string username = _usernameTextBox.Text.Trim();
            string password = _passwordTextBox.Text.Trim();
            string confirmedPassword = _confirmPasswordTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Username and password cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (password != confirmedPassword)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _registerButton.Enabled = false;
            await AppState.Connection.SendRegistrationPacketAsync(username, password);
        }

        private void HaveAccountLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SwitchToOther();
        }

        #endregion

        #region Packet Handlers

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
                _registerButton.Enabled = true;

                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        #endregion
    }
}