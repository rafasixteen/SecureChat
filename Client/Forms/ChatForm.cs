using Client.Extensions;
using Client.Transport;
using Shared.DTOs;
using System.Text;

namespace Client.Forms
{
    public partial class ChatForm : Form
    {
        public ChatForm()
        {
            InitializeComponent();
        }

        private async void ChatForm_Load(object sender, EventArgs e)
        {
            _friendsList.DataSource = AppSession.FriendUsernames;

            AppSession.Username.ValueChanged += Username_ValueChanged;
            AppSession.LoggedIn += AppSession_LoggedIn;

            try
            {
                AppSession.Connection = new ClientConnection("127.0.0.1", 8080);
                await AppSession.Connection.PerformHandshakeAsync();

                AppSession.Connection.On("server-failed", OnServerFailed);
                AppSession.Connection.StartListening();

                ShowAuthForm<LoginForm>();
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to connect to the server.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // TODO: Make the client send a EOT packet to the server before closing the connection.
            AppSession.Connection.Dispose();
        }

        private void OnFriendsListReceived(byte[] data)
        {
            Invoke(() =>
            {
                FriendsListResponse response = Serializer.Deserialize<FriendsListResponse>(data);

                AppSession.FriendUsernames.Clear();

                foreach (string friend in response.FriendUsernames)
                {
                    AppSession.FriendUsernames.Add(friend);
                }

                _friendsList.SelectedItem = null;
            });
        }

        private void OnFriendsListRejected(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private void OnGetConversationSuccess(byte[] data)
        {
            Invoke(() =>
            {
                GetConversationResponse response = Serializer.Deserialize<GetConversationResponse>(data);

                Console.WriteLine($"[Client] Received conversation with {response.Messages.Count} messages.");

                ClearMessages();

                foreach (MessageResponse message in response.Messages)
                {
                    AddMessage(message.Content, message.SentAt, message.Received);
                }
            });
        }

        private void OnGetConversationFailed(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private void OnSendMessageSuccess(byte[] data)
        {
            Invoke(() =>
            {
                // DO nothing.
            });
        }

        private void OnSendMessageFailed(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // TODO: Remove the last sent message from the chat panel if it failed to send,
                // or mark it as failed to send.
            });
        }

        private void OnMessageReceived(byte[] data)
        {
            // TODO: Fix Me - If the message was sent from 2 different clients, the message will be displayed in the chat panel
            // even if the user is not currently viewing the conversation with that friend. Consider adding a number of unread
            // messages next to the friend's name in the friends list, and only display the message in the chat panel if the
            // user is currently viewing that conversation.

            Invoke(() =>
            {
                MessageResponse response = Serializer.Deserialize<MessageResponse>(data);
                AddMessage(response.Content, response.SentAt, response.Received);
            });
        }

        private void OnServerFailed(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private async void AppSession_LoggedIn()
        {
            AppSession.Connection.On("friends-list-success", OnFriendsListReceived);
            AppSession.Connection.On("friends-list-failed", OnFriendsListRejected);

            AppSession.Connection.On("get-conversation-success", OnGetConversationSuccess);
            AppSession.Connection.On("get-conversation-failed", OnGetConversationFailed);

            AppSession.Connection.On("send-message-success", OnSendMessageSuccess);
            AppSession.Connection.On("send-message-failed", OnSendMessageFailed);

            AppSession.Connection.On("message-received", OnMessageReceived);

            await AppSession.Connection.RequestFriendsList();
        }

        private void Username_ValueChanged(string? username)
        {
            _usernameLabel.Text = username ?? "Not logged in";
        }

        private void AddMessage(string text, DateTime sentAt, bool received)
        {
            // TODO: Implement a more sophisticated message display with timestamps and sender information.

            Label msg = new()
            {
                Text = text,
                AutoSize = true,
                MaximumSize = new Size(300, 0),
                Padding = new Padding(10),
                BackColor = received ? Color.LightGray : Color.LightBlue,
                Anchor = received ? AnchorStyles.Right : AnchorStyles.Left,
            };

            _chatPanel.Controls.Add(msg);
            _chatPanel.ScrollControlIntoView(msg);
        }

        private void ClearMessages()
        {
            _chatPanel.Controls.Clear();
        }

        private async void FriendsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_friendsList.SelectedItem is string selectedFriend)
                await AppSession.Connection.RequestConversation(selectedFriend);
        }

        private async void SendButton_Click(object sender, EventArgs e)
        {
            string message = _messageTextBox.Text.Trim();

            if (string.IsNullOrEmpty(message))
                return;

            if (_friendsList.SelectedItem is not string selectedFriend)
            {
                MessageBox.Show("Please select a friend to send the message to.", "No Friend Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Local update of the chat panel to show the message immediately.
            AddMessage(message, DateTime.UtcNow, received: false);
            await AppSession.Connection.SendMessage(selectedFriend, message);
        }

        private void ShowAuthForm<T>() where T : AuthForm, new()
        {
            using AuthForm form = new T();
            DialogResult result = form.ShowDialog();

            if (result == DialogResult.Retry)
            {
                if (form is LoginForm)
                    ShowAuthForm<RegisterForm>();
                else
                    ShowAuthForm<LoginForm>();
            }
        }
    }
}