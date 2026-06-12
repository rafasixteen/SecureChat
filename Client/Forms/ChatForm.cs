using Client.Extensions;
using Client.State;
using Client.Transport;
using Shared.DTOs;
using System.ComponentModel;
using System.Text;

namespace Client.Forms
{
    public partial class ChatForm : Form
    {
        private const int MaxMessageLength = 256;

        private const string ServerHost = "127.0.0.1";

        private const int ServerPort = 8080;

        private readonly AppState _appState;

        private readonly ClientConnection _connection;

        private readonly IServiceProvider _provider;

        public ChatForm(AppState appState, ClientConnection connection, IServiceProvider provider)
        {
            InitializeComponent();

            _appState = appState;
            _connection = connection;
            _provider = provider;
        }

        #region Control Event Handlers

        /// <summary>
        /// Handles the Load event of the form. Initializes the connection and starts listening for messages.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            // Assign event handlers
            _appState.Username.ValueChanged += OnUsernameChanged;
            _appState.LoggedIn += OnLoggedIn;
            _appState.LoggedOut += OnLoggedOut;
            _appState.FriendUsernames.ListChanged += OnFriendsListChanged;

            if (await ConnectAsync())
            {
                // Small delay to give the ChatForm time to show before the LoginForm appears.
                await Task.Delay(500);
                Program.NavigateTo<LoginForm>(_provider, true);
            }
        }

        /// <summary>
        /// Handles the FormClosing event of the form. Disconnects from the server and cleans up resources.
        /// </summary>
        private async void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _appState.Logout();
            await DisconnectAsync();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the friends list. Updates the current chat partner.
        /// </summary>
        private async void FriendsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_friendsList.SelectedItem is Friend friend)
            {
                await _connection.RequestConversation(friend.Username);
                ClearNotificationCount(friend.Username);
                ClearMessages();
            }
        }

        /// <summary>
        /// Handles the Click event of the send button. Sends the message to the current chat partner.
        /// </summary>
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

            if (_appState.Username.Value == null)
            {
                MessageBox.Show("You must be logged in to send messages.", "Not Logged In", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Optimistic local update — show message immediately before server confirms.
            AddMessage(message, DateTime.UtcNow, _appState.Username.Value);
            _messageTextBox.Clear();

            await _connection.SendMessage(friend.Username, message);
        }

        /// <summary>
        /// Handles the Press event of a key, when a enter key is pressed in the message text box. Triggers the send button click event.
        /// </summary>
        private void MessageTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && _messageTextBox.Focused)
            {
                SendButton_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the logout button. Logs the user out and navigates back to the login form.
        /// </summary>
        private async void AuthButton_Click(object sender, EventArgs e)
        {
            if (_appState.IsLoggedIn)
            {
                await DisconnectAsync();
                _appState.Logout();

                // Reconnect for the next login session
                if (await ConnectAsync())
                    Program.NavigateTo<LoginForm>(_provider, true);
            }
            else
            {
                if (!_connection.IsConnected && !await ConnectAsync())
                    return;

                Program.NavigateTo<LoginForm>(_provider, true);
            }
        }

        #endregion

        #region Connection Lifecycle

        /// <summary> 
        /// Attempts to connect to the server and perform the handshake.
        /// </summary>
        /// <returns>True if connection and handshake succeeded; false otherwise.</returns>
        private async Task<bool> ConnectAsync()
        {
            try
            {
                await _connection.ConnectAsync(ServerHost, ServerPort);
                await _connection.PerformHandshakeAsync();
                _connection.StartListening();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to the server: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Sends EOT, then disposes the connection. Safe to call when not connected.
        /// </summary>
        private async Task DisconnectAsync()
        {
            if (_connection == null)
                return;

            try
            {
                await _connection.SendEotPacketAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] EOT send error: {ex.Message}");
            }
            finally
            {
                await _connection.DisconnectAsync();
            }
        }

        #endregion

        #region Packet Handlers

        /// <summary>
        /// Handles the FriendListPacket received from the server. Updates the friends list in the UI.
        /// </summary>
        /// <param name="data"> The packet data containing the list of friends.</param>
        private void OnFriendsListReceived(byte[] data)
        {
            Invoke(() =>
            {
                FriendsListResponse response = Serializer.Deserialize<FriendsListResponse>(data);

                _appState.FriendUsernames.Clear();

                foreach (string username in response.FriendUsernames)
                    _appState.FriendUsernames.Add(new Friend(username));

                _friendsList.SelectedItem = null;
            });
        }

        /// <summary>
        /// Handles the FriendsListRejectionPacket received from the server. Displays an error message to the user.
        /// </summary>
        /// <param name="data"> The packet data containing the rejection reason.</param>
        private void OnFriendsListRejected(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        /// <summary>
        /// Handles the ConversationPacket received from the server. Updates the current chat partner and displays the conversation history.
        /// </summary>
        /// <param name="data"> The packet data containing the conversation details.</param>
        private void OnGetConversationChunk(byte[] data)
        {
            Invoke(() =>
            {
                GetConversationResponse response = Serializer.Deserialize<GetConversationResponse>(data);
                Console.WriteLine($"[Client] Received conversation with {response.Messages.Count} messages.");

                foreach (MessageResponse message in response.Messages)
                    AddMessage(message.Content, message.SentAt, message.SenderUsername);
            });
        }

        /// <summary>
        /// Handles the ConversationFailed packet
        /// </summary>
        /// <param name="data"></param>
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
            /* no-op */
        }

        /// <summary>
        /// Handles the failure to send a message
        /// </summary>
        private void OnSendMessageFailed(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // TODO: Remove or mark failed message in the chat panel.
            });
        }
        
        /// <summary>
        /// Handles messages received
        /// </summary>
        private void OnMessageReceived(byte[] data)
        {
            Invoke(() =>
            {
                MessageResponse response = Serializer.Deserialize<MessageResponse>(data);
                Console.WriteLine($"[Client] Received message from {response.SenderUsername}: {response.Content}");

                if (_friendsList.SelectedItem is Friend friend && response.SenderUsername == friend.Username)
                    AddMessage(response.Content, response.SentAt, response.SenderUsername);
                else
                    IncrementNotificationCount(response.SenderUsername);
            });
        }

        /// <summary>
        /// Handles server failure
        /// </summary>
        private void OnServerFailed(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        #endregion

        #region App State Events
        
        /// <summary>
        /// Configures events when the user logs in
        /// </summary>
        private async void OnLoggedIn()
        {
            _authButton.Text = "Logout";

            _connection.On("server-failed", OnServerFailed);

            _connection.On("friends-list-success", OnFriendsListReceived);
            _connection.On("friends-list-failed", OnFriendsListRejected);

            _connection.On("get-conversation-chunk", OnGetConversationChunk);
            _connection.On("get-conversation-failed", OnGetConversationFailed);

            _connection.On("send-message-success", OnSendMessageSuccess);
            _connection.On("send-message-failed", OnSendMessageFailed);

            _connection.On("message-received", OnMessageReceived);

            await _connection.RequestFriendsList();
        }

        /// <summary>
        /// Clears events when the user logs out
        /// </summary>
        public async void OnLoggedOut()
        {
            _connection.ClearHandlers();
            _authButton.Text = "Login";

            ClearMessages();
        }

        /// <summary>
        /// Handles the user's username change
        /// </summary>
        private void OnUsernameChanged(string? username)
        {
            _usernameLabel.Text = username != null ? $"Logged in as: {username}" : "Not logged in";
        }

        /// <summary>
        /// Handles the event of the friends list change
        /// </summary>
        private void OnFriendsListChanged(object? sender, ListChangedEventArgs e)
        {
            _friendsList.DataSource = null;
            _friendsList.DataSource = _appState.FriendUsernames;
            _friendsList.Refresh();
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Adds messages to the layout panel
        /// </summary>
        /// <param name="text">Text of the message</param>
        /// <param name="sentAt">When the message was sent</param>
        /// <param name="senderUsername">Who sent the message</param>
        private void AddMessage(string text, DateTime sentAt, string senderUsername)
        {
            bool isReceived = senderUsername != _appState.Username.Value;

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

            Panel container = new()
            {
                AutoSize = true,
                Margin = new Padding(0, 2, 0, 2),
            };

            container.Controls.Add(bubble);

            /// <summary>
            /// Position the text bubble accordingly
            /// </summary>
            void PositionBubble()
            {
                container.Width = _chatPanel.ClientSize.Width - 20;
                bubble.Left = isReceived ? 5 : container.Width - bubble.Width - 5;
            }

            container.Layout += (_, _) => PositionBubble();

            _chatPanel.Controls.Add(container);
            _chatPanel.ScrollControlIntoView(container);
        }

        /// <summary>
        /// Clears all messages
        /// </summary>
        private void ClearMessages()
        {
            _chatPanel.Controls.Clear();
        }

        /// <summary>
        /// Increments the notification count for users who sent the message to the current user
        /// </summary>
        /// <param name="friendUsername">Which friend sent the message</param>
        private void IncrementNotificationCount(string friendUsername)
        {
            Friend? friend = _appState.FriendUsernames.FirstOrDefault(f => f.Username == friendUsername);

            if (friend != null)
            {
                friend.NotificationCount++;
                _friendsList.Refresh();
            }
        }

        /// <summary>
        /// Clear notification count for a specified friend
        /// </summary>
        /// <param name="friendUsername">What friend to clear the notification counter</param>
        private void ClearNotificationCount(string friendUsername)
        {
            Friend? friend = _appState.FriendUsernames.FirstOrDefault(f => f.Username == friendUsername);

            if (friend != null)
            {
                friend.NotificationCount = 0;
                _friendsList.Refresh();
            }
        }

        #endregion
    }
}