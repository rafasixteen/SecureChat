using Client.Transport;
using Shared;
using System.ComponentModel;

namespace Client
{
    public static class AppSession
    {
        private static ClientConnection? _connection;

        public static ClientConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    throw new InvalidOperationException("No connection is established.");
                }

                return _connection;
            }
            set
            {
                if (_connection == value)
                {
                    throw new InvalidOperationException("The connection is already set.");
                }

                _connection = value;
            }
        }

        public static ObservableValue<string?> Username { get; } = new(null);

        public static BindingList<string> FriendUsernames { get; } = new();

        public static Action? LoggedIn;
    }
}