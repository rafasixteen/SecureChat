using Client.Transport;
using Shared;
using System.ComponentModel;

namespace Client.State
{
    public static class AppState
    {
        public static ClientConnection Connection { get; } = new();

        public static ObservableValue<string?> Username { get; } = new(null);

        public static BindingList<Friend> FriendUsernames { get; } = new();

        public static Action? LoggedIn;

        public static Action? LoggedOut;

        public static bool IsLoggedIn => Username.Value != null;
    }
}