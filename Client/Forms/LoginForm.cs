using Client.Extensions;
using EI.SI;
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
                MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            });
        }

        private void OnLoginFailed(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show($"Login failed: {message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            ClientConnection connection = AppSession.Connection.Ensure();

            string username = _textBoxUsername.Text;
            string password = _textBoxPassword.Text;

            await connection.SendLoginPacketAsync(username, password);
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            ClientConnection connection = AppSession.Connection.Ensure();

            connection.On(ProtocolSICmdType.ACK, OnLoginSuccess);
            connection.On(ProtocolSICmdType.NACK, OnLoginFailed);

            connection.StartListening();
        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClientConnection connection = AppSession.Connection.Ensure();

            connection.StopListening();
            connection.ClearHandlers();
        }

        private void CreateAccountLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SwitchToOther();
        }
    }
}
