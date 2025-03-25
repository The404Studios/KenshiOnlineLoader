namespace KenshiMultiplayerLoader.MODELS
{
    public static class MessageType
    {
        // Basic types
        public const string Position = "position";
        public const string Inventory = "inventory";
        public const string Combat = "combat";
        public const string Health = "health";
        public const string Chat = "chat";
        public const string Reconnect = "reconnect";
        public const string AdminKick = "adminkick";

        // Authentication & session management
        public const string Login = "login";
        public const string Logout = "logout";
        public const string Register = "register";
        public const string Authentication = "auth";

        // Data synchronization
        public const string WorldState = "worldstate";
        public const string PlayerState = "playerstate";
        public const string EntityUpdate = "entity";
        public const string Batch = "batch";
        public const string Delta = "delta";
        public const string Acknowledgment = "ack";

        // System messages
        public const string Error = "error";
        public const string Warning = "warning";
        public const string Notification = "notification";
        public const string SystemMessage = "system";

        // Ping/heartbeat for connection validation
        public const string Ping = "ping";
        public const string Pong = "pong";
    }
}