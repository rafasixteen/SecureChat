namespace Client.Forms
{
    public class AuthForm : Form
    {
        protected void SwitchToOther()
        {
            DialogResult = DialogResult.Retry;
            Close();
        }
    }
}
