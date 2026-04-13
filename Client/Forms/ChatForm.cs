using Client.Extensions;

namespace Client.Forms
{
    public partial class ChatForm : Form
    {
        private AuthForm? _activeAuthForm;

        public ChatForm()
        {
            InitializeComponent();
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
            catch (Exception ex)
            {
                MessageBox.Show("Connection error: " + ex.Message);
            }
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppSession.Connection.Ensure().Dispose();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            string message = _textBoxMessage.Text.Trim();

            string recipientUsername = _usersListView.SelectedItems.Count > 0 ? _usersListView.SelectedItems[0].Text : null;



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
