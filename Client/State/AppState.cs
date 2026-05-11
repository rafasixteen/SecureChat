using Shared;
using System.ComponentModel;

namespace Client.State
{
    public class AppState
    {
        public ObservableValue<string?> Username { get; } = new(null);

        public BindingList<Friend> FriendUsernames { get; } = new();

        public event Action? LoggedIn;

        public event Action? LoggedOut;

        public bool IsLoggedIn => Username.Value != null;

        public void Login(string username)
        {
            Username.Value = username;
            LoggedIn?.Invoke();
        }

        public void Logout()
        {
            Username.Value = null;
            FriendUsernames.Clear();
            LoggedOut?.Invoke();
        }
    }
}