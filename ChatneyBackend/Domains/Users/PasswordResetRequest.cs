using System.Linq.Expressions;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Users;

public class PasswordResetRequest : Infra.IPgKey<PasswordResetRequest, Guid>
{
    [Primary]
    [Map("code")]
    public Guid Code { get; set; }

    [Map("user_id")]
    public Guid UserId { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("valid_until")]
    public DateTime ValidUntil { get; set; }

    public static Expression<Func<PasswordResetRequest, bool>> MatchByKey(Guid key) =>
        request => request.Code == key;

    public static Guid GetKey(PasswordResetRequest record) => record.Code;
}
