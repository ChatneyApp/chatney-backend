using System.Linq.Expressions;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Roles;

public class SecureObject : IPgKey<SecureObject, int>
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    public static Expression<Func<SecureObject, bool>> MatchByKey(int key) => obj => obj.Id == key;
}
