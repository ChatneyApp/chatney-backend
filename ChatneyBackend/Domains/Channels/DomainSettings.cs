using ChatneyBackend.Domains.Permissions;

namespace ChatneyBackend.Domains.Channels;

public class DomainSettings
{
    public const string ChannelTableName = "channels";
    public const string ChannelTypeTableName = "channel_types";
    public const string ChannelGroupTableName = "channel_groups";
    public const string ChannelMemberTableName = "channel_members";

    public const string DmChannelTypeKey = "dm";
    public const string DmChannelTypeName = "Direct message";
    public const string DmChannelName = "DM";

    public static readonly Permission[] DirectMessageParticipantPermissions =
    [
        Permission.ChannelReadChannel,
        Permission.ChannelReadMessage,
        Permission.ChannelCreateMessage,
        Permission.ChannelEditOwnMessage,
        Permission.ChannelDeleteOwnMessage,
    ];
}