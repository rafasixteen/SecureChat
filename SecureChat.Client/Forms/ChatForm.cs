namespace SecureChat.Client
{
    /// <summary>
    /// Chat form — sends and receives messages
    /// </summary>
    public partial class ChatForm : Form
    {
        private readonly ChatClient _client;

        private readonly string _username;

        public ChatForm(ChatClient client, string username)
        {
            InitializeComponent();

            _client = client;
            _username = username;

            Text = $"SecureChat — {username}";

            // Hook incoming message callback to UI update
            _client.MessageReceived += (message) =>
            {
                // Must invoke on UI thread
                Invoke(() => AppendMessage(message));
            };
        }

        /// <summary>
        /// Sends the typed message to the server and clears the input box.
        /// </summary>
        private void OnSendButtonClick(object sender, EventArgs e)
        {
            string text = _textBoxInput.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            string message = $"{_username}: {text}";
            _client.SendMessage(message);

            AppendMessage(message);
            _textBoxInput.Clear();
        }

        /// <summary>
        /// Allow sending with Enter key.
        /// </summary>
        private void OnTextBoxInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OnSendButtonClick(sender, e);
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Appends a message to the chat display.
        /// </summary>
        private void AppendMessage(string message)
        {
            _textBoxChat.AppendText($"[{DateTime.Now:HH:mm}] {message}{Environment.NewLine}");
            _textBoxChat.ScrollToCaret();
        }

        /// <summary>
        /// Disconnect cleanly when closing the form.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _client.Disconnect();
            base.OnFormClosing(e);
        }
    }
}