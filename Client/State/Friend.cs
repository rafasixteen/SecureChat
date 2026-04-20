namespace Client.State
{
    public class Friend(string username)
    {
        public string Username { get; set; } = username;

        public int NotificationCount { get; set; } = 0;

        public override string ToString()
        {
            return NotificationCount > 0 ? $"{Username} ({NotificationCount})" : Username;
        }
    }
}