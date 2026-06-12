using Client.Extensions;
using Client.State;
using Client.Transport;
using Shared.DTOs;
using System.Text;

namespace Client.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AppState _state;

        private readonly ClientConnection _connection;

        private readonly IServiceProvider _provider;

        public LoginForm(AppState state, ClientConnection connection, IServiceProvider provider)
        {
            InitializeComponent();

            _state = state;
            _connection = connection;
            _provider = provider;
        }

        #region Control Event Handlers

        /// <summary>
        /// Handles the Click event of the login button. Validates user input and initiates the login process.
        /// </summary>
        private void LoginForm_Load(object sender, EventArgs e)
        {
            _connection.On("login-success", OnLoginSuccess);
            _connection.On("login-failed", OnLoginFailed);
        }

        /// <summary>
        /// Handles the Click event of the login button. Validates user input and initiates the login process.
        /// </summary>
        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _connection.RemoveHandler("login-success");
            _connection.RemoveHandler("login-failed");
        }

        /// <summary>
        /// Handles the Click event of the login button. Validates user input and initiates the login process.
        /// </summary>
        private async void LoginButton_Click(object sender, EventArgs e)
        {
            string username = _textBoxUsername.Text;
            string password = _textBoxPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Username and password cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _loginButton.Enabled = false;
            await _connection.SendLoginPacketAsync(username, password);
        }

        /// <summary>
        /// Handles the Click event of the register button. Opens the registration form.
        /// </summary>
        private void CreateAccountLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LoginForm_FormClosing(sender, new FormClosingEventArgs(CloseReason.UserClosing, false));
            Program.SwitchDialog<RegisterForm>(_provider, this);
        }

        #endregion

        #region Packet Handlers

        /// <summary>
        /// Handles the "login-success" packet from the server. Updates the application state and transitions to the main form.
        /// </summary>
        /// <param name="data"> The data received from the server, expected to be a LoginSuccessDTO.</param>
        private void OnLoginSuccess(byte[] data)
        {
            Invoke(() =>
            {
                LoginResponse response = Serializer.Deserialize<LoginResponse>(data);
                _state.Login(response.Username);
                Close();
            });
        }

        /// <summary>
        /// Handles the "login-failed" packet from the server. Displays an error message to the user.
        /// </summary>
        /// <param name="data"> The data received from the server, expected to be a LoginFailedDTO.</param>
        private void OnLoginFailed(byte[] data)
        {
            Invoke(() =>
            {
                _loginButton.Enabled = true;

                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        #endregion
    }
}