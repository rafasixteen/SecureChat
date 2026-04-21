using Client.Extensions;
using Client.State;
using Client.Transport;
using Shared.DTOs;
using System.Text;

namespace Client.Forms
{
    public partial class ChatForm : Form
    {
        private const int MaxMessageLength = 256;

        public ChatForm()
        {
            InitializeComponent();
        }

        #region Control Event Handlers

        private async void ChatForm_Load(object sender, EventArgs e)
        {
            _friendsList.DrawMode = DrawMode.OwnerDrawFixed;
            _friendsList.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;

                if (_friendsList.Items[e.Index] is Friend friend)
                {
                    e.DrawBackground();

                    Brush brush = friend.NotificationCount > 0 ? Brushes.Green : Brushes.Black;
                    e.Graphics.DrawString(friend.ToString(), e.Font!, brush, e.Bounds);

                    e.DrawFocusRectangle();
                }
            };

            _friendsList.DataSource = AppState.FriendUsernames;

            AppState.Username.ValueChanged += Username_ValueChanged;
            AppState.LoggedIn += AppSession_LoggedIn;

            try
            {
                AppState.Connection = new ClientConnection("127.0.0.1", 8080);
                await AppState.Connection.PerformHandshakeAsync();

                AppState.Connection.On("server-failed", OnServerFailed);
                AppState.Connection.StartListening();

                ShowAuthForm<LoginForm>();
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to connect to the server.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            await AppState.Connection.SendEotPacketAsync();
            AppState.Connection.Dispose();
        }

        private async void FriendsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_friendsList.SelectedItem is Friend friend)
            {
                await AppState.Connection.RequestConversation(friend.Username);

                ClearNotificationCount(friend.Username);
                ClearMessages();
            }
        }

        private async void SendButton_Click(object sender, EventArgs e)
        {
            string message = _messageTextBox.Text.Trim();

            if (string.IsNullOrEmpty(message))
                return;

            if (_friendsList.SelectedItem is not Friend friend)
            {
                MessageBox.Show("Please select a friend to send the message to.", "No Friend Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (message.Length > MaxMessageLength)
            {
                MessageBox.Show($"Message exceeds the maximum length of {MaxMessageLength} characters.", "Message Too Long", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (AppState.Username.Value == null)
            {
                MessageBox.Show("You must be logged in to send messages.", "Not Logged In", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Local update of the chat panel to show the message immediately.
            AddMessage(message, DateTime.UtcNow, AppState.Username.Value);
            _messageTextBox.Clear();

            await AppState.Connection.SendMessage(friend.Username, message);
        }

        private void MessageTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && _messageTextBox.Focused)
            {
                SendButton_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }

        #endregion

        #region Packet Handlers

        private void OnFriendsListReceived(byte[] data)
        {
            Invoke(() =>
            {
                FriendsListResponse response = Serializer.Deserialize<FriendsListResponse>(data);

                AppState.FriendUsernames.Clear();

                foreach (string username in response.FriendUsernames)
                {
                    AppState.FriendUsernames.Add(new Friend(username));
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

        private void OnGetConversationChunk(byte[] data)
        {
            Invoke(() =>
            {
                GetConversationResponse response = Serializer.Deserialize<GetConversationResponse>(data);

                Console.WriteLine($"[Client] Received conversation with {response.Messages.Count} messages.");

                foreach (MessageResponse message in response.Messages)
                {
                    AddMessage(message.Content, message.SentAt, message.SenderUsername);
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
            Invoke(() =>
            {
                MessageResponse response = Serializer.Deserialize<MessageResponse>(data);

                Console.WriteLine($"[Client] Received message from {response.SenderUsername}: {response.Content}");

                if (_friendsList.SelectedItem is Friend friend && response.SenderUsername == friend.Username)
                {
                    AddMessage(response.Content, response.SentAt, response.SenderUsername);
                }
                else
                {
                    IncrementNotificationCount(response.SenderUsername);
                }
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

        #endregion

        private async void AppSession_LoggedIn()
        {
            AppState.Connection.On("friends-list-success", OnFriendsListReceived);
            AppState.Connection.On("friends-list-failed", OnFriendsListRejected);

            AppState.Connection.On("get-conversation-chunk", OnGetConversationChunk);
            AppState.Connection.On("get-conversation-failed", OnGetConversationFailed);

            AppState.Connection.On("send-message-success", OnSendMessageSuccess);
            AppState.Connection.On("send-message-failed", OnSendMessageFailed);

            AppState.Connection.On("message-received", OnMessageReceived);

            await AppState.Connection.RequestFriendsList();
        }

        private void Username_ValueChanged(string? username)
        {
            _usernameLabel.Text = username ?? "Not logged in";
        }

        private void AddMessage(string text, DateTime sentAt, string senderUsername)
        {
            bool isReceived = senderUsername != AppState.Username.Value;

            // Use FlowLayoutPanel for the bubble so labels stack vertically
            FlowLayoutPanel bubble = new()
            {
                AutoSize = true,
                MaximumSize = new Size(300, 0),
                Padding = new Padding(10),
                BackColor = isReceived ? Color.LightGray : Color.LightBlue,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
            };

            Label messageLabel = new()
            {
                Text = text,
                AutoSize = true,
                MaximumSize = new Size(280, 0),
                Margin = new Padding(0, 0, 0, 2),
            };

            Label timeLabel = new()
            {
                Text = sentAt.ToLocalTime().ToString("HH:mm"),
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 7),
                ForeColor = Color.Gray,
                Margin = new Padding(0),
            };

            bubble.Controls.Add(messageLabel);
            bubble.Controls.Add(timeLabel);

            // Container panel — full width so we can anchor the bubble left or right
            Panel container = new()
            {
                AutoSize = true,
                MinimumSize = new Size(_chatPanel.ClientSize.Width - 20, 0),
                Margin = new Padding(0, 2, 0, 2),
            };

            // Add bubble first so it gets AutoSized, then reposition
            container.Controls.Add(bubble);
            bubble.Location = new Point(0, 0); // temporary; repositioned below

            // Reposition after the bubble has been sized by the layout engine
            container.Layout += (_, _) =>
            {
                if (!isReceived)
                    bubble.Left = container.ClientSize.Width - bubble.Width - 5;
                else
                    bubble.Left = 5;
            };

            _chatPanel.Controls.Add(container);
            _chatPanel.ScrollControlIntoView(container);
        }

        private void ClearMessages()
        {
            _chatPanel.Controls.Clear();
        }

        private void IncrementNotificationCount(string friendUsername)
        {
            Friend? friend = AppState.FriendUsernames.FirstOrDefault(f => f.Username == friendUsername);

            if (friend != null)
            {
                friend.NotificationCount++;
                _friendsList.Refresh();
            }
        }

        private void ClearNotificationCount(string friendUsername)
        {
            Friend? friend = AppState.FriendUsernames.FirstOrDefault(f => f.Username == friendUsername);

            if (friend != null)
            {
                friend.NotificationCount = 0;
                _friendsList.Refresh();
            }
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