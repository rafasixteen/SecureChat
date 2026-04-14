using Client.Transport;
using Shared;
using System.ComponentModel;

namespace Client
{
    public static class AppSession
    {
        public static ClientConnection? Connection { get; set; }

        public static ObservableValue<string?> Username { get; } = new(null);

        public static BindingList<string> FriendUsernames { get; } = new();

        public static Action? LoggedIn;
    }
}