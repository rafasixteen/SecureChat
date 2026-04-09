using Client.Extensions;
using EI.SI;
using System.Text;

namespace Client.Forms
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }

        private void OnRegistrationSuccess(byte[] data)
        {
            Invoke(() =>
           {
               MessageBox.Show("Registration successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
               Close();
           });
        }

        private void OnRegistrationFailed(byte[] data)
        {   
            Invoke(() =>
            {
                string message = Encoding.UTF8.GetString(data);
                MessageBox.Show($"Registration failed: {message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private void ButtonRegister_Click(object sender, EventArgs e)
        {
            ClientSession session = AppSession.Current.Ensure();

            string username = _textBoxUsername.Text;
            string password = txtbx_password.Text;

            session.SendRegistrationPacket(username, password);
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {
            ClientSession session = AppSession.Current.Ensure();

            session.On(ProtocolSICmdType.ACK, OnRegistrationSuccess);
            session.On(ProtocolSICmdType.NACK, OnRegistrationFailed);

            session.StartListening();
        }

        private void RegisterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClientSession session = AppSession.Current.Ensure();

            session.StopListening();
            session.ClearHandlers();
        }
    }
}