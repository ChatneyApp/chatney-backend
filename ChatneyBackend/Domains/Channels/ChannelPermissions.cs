namespace ChatneyBackend.Domains.Channels;

public class ChannelPermissions
{
    private const string Domain = "channel";

    public const string DeleteMessage = Domain + ".deleteMessage";
    public const string EditMessage   = Domain + ".editMessage";
    public const string CreateMessage = Domain + ".createMessage";
    public const string ReadMessage   = Domain + ".readMessage";

    public const string DeleteChannel = Domain + ".deleteChannel";
    public const string EditChannel   = Domain + ".editChannel";
    public const string CreateChannel = Domain + ".createChannel";
    public const string ReadChannel   = Domain + ".readChannel";
}
