using Client.Extensions;
using Client.State;
using Client.Transport;
using Shared.DTOs;
using Shared.DTOs.Shared.DTOs;
using System.Text;

namespace Client.Forms
{
    public partial class RegisterForm : Form
    {
        private readonly AppState _state;

        private readonly ClientConnection _connection;

        private readonly IServiceProvider _provider;

        public RegisterForm(AppState state, ClientConnection connection, IServiceProvider provider)
        {
            InitializeComponent();

            _state = state;
            _connection = connection;
            _provider = provider;
        }

        #region Control Event Handlers

        private void RegisterForm_Load(object sender, EventArgs e)
        {
            _connection.On("register-success", OnRegisterSuccess);
            _connection.On("register-failed", OnRegistrationFailed);
        }

        private void RegisterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _connection.RemoveHandler("register-success");
            _connection.RemoveHandler("register-failed");
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
            await _connection.SendRegistrationPacketAsync(username, password);
        }

        private void HaveAccountLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RegisterForm_FormClosing(sender, new FormClosingEventArgs(CloseReason.UserClosing, false));
            Program.SwitchDialog<LoginForm>(_provider, this);
        }

        #endregion

        #region Packet Handlers

        private void OnRegisterSuccess(byte[] data)
        {
            Invoke(() =>
            {
                RegisterResponse response = Serializer.Deserialize<RegisterResponse>(data);
                _state.Login(response.Username);
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