public class MessagePermissions
{
    private const string Domain = "message";

    public const string DeleteMessage = Domain + ".deleteOwnMessage";
    public const string EditMessage   = Domain + ".editOwnMessage";
    public const string CreateMessage = Domain + ".createMessage";
    public const string ReadMessage   = Domain + ".readMessage";
}
