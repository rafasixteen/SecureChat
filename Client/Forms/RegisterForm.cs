using Client.Extensions;
using EI.SI;
using System.Text;

namespace Client.Forms
{
    public partial class RegisterForm : AuthForm
    {
        public RegisterForm()
        {
            InitializeComponent();
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
            ClientConnection session = AppSession.Connection.Ensure();

            string username = _usernameTextBox.Text;
            string password = _passwordTextBox.Text;

            await session.SendRegistrationPacketAsync(username, password);
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {
            ClientConnection connection = AppSession.Connection.Ensure();

            connection.On(ProtocolSICmdType.ACK, (_) => Invoke(Close));
            connection.On(ProtocolSICmdType.NACK, OnRegistrationFailed);

            connection.StartListening();
        }

        private void RegisterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClientConnection connection = AppSession.Connection.Ensure();

            connection.StopListening();
            connection.ClearHandlers();
        }

        private void HaveAccountLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SwitchToOther();
        }
    }
}