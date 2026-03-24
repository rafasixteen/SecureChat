using ProtoIP;

namespace SecureChat.Client
{
    /// <summary>
    /// Login form — collects credentials and connects to the server.
    /// On success, opens the ChatForm.
    /// </summary>
    public partial class LoginForm : Form
    {
        private ChatClient? _client;

        public LoginForm()
        {
            InitializeComponent();

            _textBoxServer.Text = "127.0.0.1";
        }

        /// <summary>
        /// Attempts to connect to the server and authenticate the user.
        /// </summary>
        private void ButtonLoginOnClick(object sender, EventArgs e)
        {
            string username = _textBoxUsername.Text.Trim();
            string password = _textBoxPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter username and password.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _client = new ChatClient();

                _client.AuthenticationSucceeded += OnAuthenticationSucceeded;

                _client.Connect(_textBoxServer.Text.Trim(), 8080);

                // Send FIRST, then start listening for the response.
                Packet authPacket = new(Packet.Type.HANDSHAKE_REQ);
                authPacket.SetPayload($"{username}:{password}");
                _client.Send(Packet.Serialize(authPacket));

                _client.Receive();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to server:\n{ex.Message}", "Connection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnAuthenticationSucceeded()
        {
            if (_client == null) return;

            // Must invoke on UI thread since this is called from background listen thread.
            Invoke(() =>
            {
                string username = _textBoxUsername.Text.Trim();
                ChatForm chat = new(_client, username);
                chat.Show();
                Hide();
            });
        }
    }
}