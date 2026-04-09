namespace Client.Forms
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
            Thread listenThread = new(() => ListenForServerMessages());
        }

        private void ButtonRegister_Click(object sender, EventArgs e)
        {
            ClientSession? session = AppSession.Current;

            if (session == null)
            {
                MessageBox.Show("No active session. Please try again.");
                return;
            }

            string username = _textBoxUsername.Text;
            string password = txtbx_password.Text;

            session.SendMessage($"{username}:{password}", EI.SI.ProtocolSICmdType.USER_OPTION_1);
        }

        private void ListenForServerMessages()
        {
            // Implement listening for server messages if needed
        }
    }
}
