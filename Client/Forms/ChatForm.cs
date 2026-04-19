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

        // Ao iniciar o formulário, configurar a lista de amigos, registrar os eventos de mudança de username e login, e tentar estabelecer uma conexão com o servidor.
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

        // Handler de receber lista de amigos
        private void OnFriendsListReceived(byte[] data)
        {
            Invoke(() =>
            {
                FriendsListResponse response = Serializer.Deserialize<FriendsListResponse>(data);

                AppSession.FriendUsernames.Clear();

                // Adiciona os amigos recebidos à lista de amigos na AppSession
                foreach (string friend in response.FriendUsernames)
                {
                    AppSession.FriendUsernames.Add(friend);
                }

                _friendsList.SelectedItem = null;
            });
        }

        // Handler de falha ao receber a lista de amigos, que pode ocorrer se o cliente não estiver autenticado ou se houver um erro no servidor.
        private void OnFriendsListRejected(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        // Handler de receber a conversa com um amigo, que inclui uma lista de mensagens trocadas entre o utilizador autenticado e o amigo selecionado.
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

        // Handler de falha ao receber a conversa, que pode ocorrer se o cliente não estiver autenticado, se o amigo selecionado não existir ou se houver um erro no servidor.
        private void OnGetConversationFailed(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        // Handler de sucesso (Não faz nada)
        private void OnSendMessageSuccess(byte[] data)
        {
            Invoke(() =>
            {
                // DO nothing.
            });
        }

        // Handler de falha ao enviar uma mensagem, que pode ocorrer se o cliente não estiver autenticado, se o amigo selecionado não existir ou se houver um erro no servidor.
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

        // Handler de receber mensagens em tempo real
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

        // Handler de falha do servidor
        private void OnServerFailed(byte[] data)
        {
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show(message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        // Handler de login, que é chamado quando o utilizador efetua login com sucesso. Regista os handlers para os eventos relacionados com a sessão do utilizador e solicita a lista de amigos ao servidor.
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

        // Função para adicionar mensagens à caixa de mensagens
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

        // Função de limpar as mensagens da caixa de mensagens
        private void ClearMessages()
        {
            _chatPanel.Controls.Clear();
        }

        // Handler de mudar a conversa com utilizador
        private async void FriendsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_friendsList.SelectedItem is string selectedFriend)
                await AppSession.Connection.RequestConversation(selectedFriend);
        }

        // Função de enviar mensagens, chamada quando o utilizador clica no botão de enviar
        private async void SendButton_Click(object sender, EventArgs e)
        {
            string message = _messageTextBox.Text.Trim();

            // Verifica se está vazia
            if (string.IsNullOrEmpty(message))
                return;

            // Verifica se já escolheu um amigo para enviar mensagem
            if (_friendsList.SelectedItem is not string selectedFriend)
            {
                MessageBox.Show("Please select a friend to send the message to.", "No Friend Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Atualização local da mensagem a aparecer
            AddMessage(message, DateTime.UtcNow, received: false);

            // Enviar mensagem para o outro cliente
            await AppSession.Connection.SendMessage(selectedFriend, message);
        }

        // Função para fazer mostrar o form de autenticação (login)
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