using Client.Extensions;
using EI.SI;
using Shared.DTOs;
using System.Text;

namespace Client.Forms
{
    public partial class ChatForm : Form
    {
        private AuthForm? _activeAuthForm;

        public ChatForm()
        {
            InitializeComponent();

            _friendsList.DataSource = AppSession.FriendUsernames;
            AppSession.Username.ValueChanged += Username_ValueChanged;
            AppSession.LoggedIn += AppSession_LoggedIn;
        }

        private void OnFriendsListReceived(byte[] data)
        {
            Invoke(() =>
            {
                FriendsListResponse response = Serializer.Deserialize<FriendsListResponse>(data);

                AppSession.FriendUsernames.Clear();

                MessageBox.Show($"You have {response.FriendUsernames.Count} friends.", "Friends List", MessageBoxButtons.OK, MessageBoxIcon.Information);

                foreach (string friend in response.FriendUsernames)
                {
                    AppSession.FriendUsernames.Add(friend);
                }
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

        private async void AppSession_LoggedIn()
        {
            ClientConnection connection = AppSession.Connection.Ensure();

            connection.On(ProtocolSICmdType.ACK, OnFriendsListReceived);
            connection.On(ProtocolSICmdType.NACK, OnFriendsListRejected);

            connection.StartListening();

            await connection.RequestFriendsList();
        }

        private void Username_ValueChanged(string? username)
        {
            _usernameLabel.Text = username ?? "Not logged in";
        }

        private async void ChatForm_Load(object sender, EventArgs e)
        {
            try
            {
                ClientConnection connection = new("127.0.0.1", 8080);
                await connection.PerformHandshakeAsync();
                AppSession.Connection = connection;

                ShowAuthForm<LoginForm>();
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to connect to the server.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppSession.Connection.Ensure().Dispose();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (_activeAuthForm != null)
            {
                return;
            }

            ShowAuthForm<LoginForm>();
        }

        private void ShowAuthForm<T>() where T : AuthForm, new()
        {
            using AuthForm form = new T();
            _activeAuthForm = form;
            DialogResult result = form.ShowDialog();
            _activeAuthForm = null;

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
