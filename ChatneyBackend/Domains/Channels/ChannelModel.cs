using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using ChatneyBackend.Infra;
using ChatneyBackend.Utils;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Channels;

public class Channel : IPgKey<Channel, int>, IPgTimestamped
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    [Map("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Map("channel_type_id")]
    public int ChannelTypeId { get; set; }

    [Map("workspace_id")]
    public int? WorkspaceId { get; set; }

    [Map("is_dm")]
    public bool IsDm { get; set; }

    [Map("sec_obj_id")]
    public int SecObjId { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public static Channel FromDto(ChannelDto channel)
    {
        return new Channel
        {
            Name = channel.Name,
            WorkspaceId = channel.WorkspaceId,
            ChannelTypeId = channel.ChannelTypeId,
            IsDm = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public static Expression<Func<Channel, bool>> MatchByKey(int key) => channel => channel.Id == key;

    public static int GetKey(Channel record) => record.Id;
}

public class ChannelDto : IDto<Channel>
{
    [MaxLength(255)]
    public required string Name { get; set; }

    public int ChannelTypeId { get; set; }

    public int WorkspaceId { get; set; }

    public Channel ToModel() => Channel.FromDto(this);
}

public class WebsocketChannelPayload
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int ChannelTypeId { get; set; }
    public int? WorkspaceId { get; set; }
    public bool IsDm { get; set; }
    public Guid[] MemberUserIds { get; set; } = [];

    public static WebsocketChannelPayload FromChannel(Channel channel, IEnumerable<Guid>? memberUserIds = null) => new()
    {
        Id = channel.Id,
        Name = channel.Name,
        ChannelTypeId = channel.ChannelTypeId,
        WorkspaceId = channel.WorkspaceId,
        IsDm = channel.IsDm,
        MemberUserIds = memberUserIds?.ToArray() ?? [],
    };
}

public class WebsocketChannelDeletedPayload
{
    public int Id { get; set; }
    public int? WorkspaceId { get; set; }
    public bool IsDm { get; set; }
}

public class DirectMessageUser
{
    public Guid Id { get; set; }
    public required string Nickname { get; set; }
    public string? AvatarUrl { get; set; }

    public static DirectMessageUser FromUser(ChatneyBackend.Domains.Users.User user) => new()
    {
        Id = user.Id,
        Nickname = user.Nickname,
        AvatarUrl = user.AvatarUrl,
    };
}

public class DirectMessage
{
    public required Channel Channel { get; set; }
    public required List<DirectMessageUser> OtherUsers { get; set; }

    public static DirectMessage From(
        Channel channel,
        IEnumerable<ChatneyBackend.Domains.Users.User> otherUsers) => new()
    {
        Channel = channel,
        OtherUsers = otherUsers.Select(DirectMessageUser.FromUser).ToList(),
    };
}
