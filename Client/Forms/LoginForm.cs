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

        private void LoginForm_Load(object sender, EventArgs e)
        {
            _connection.On("login-success", OnLoginSuccess);
            _connection.On("login-failed", OnLoginFailed);
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _connection.RemoveHandler("login-success");
            _connection.RemoveHandler("login-failed");
        }

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

        private void CreateAccountLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LoginForm_FormClosing(sender, new FormClosingEventArgs(CloseReason.UserClosing, false));
            Program.SwitchDialog<RegisterForm>(_provider, this);
        }

        #endregion

        #region Packet Handlers

        private void OnLoginSuccess(byte[] data)
        {
            Invoke(() =>
            {
                LoginResponse response = Serializer.Deserialize<LoginResponse>(data);
                _state.Login(response.Username);
                Close();
            });
        }

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