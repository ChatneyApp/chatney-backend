namespace ChatneyBackend.Domains.Channels;

public static class ChannelPermissions
{
    public const string Create = "create";
    public const string List = "List";
    public const string Update = "Update";
    public const string Delete = "Delete";

    public static readonly HashSet<string> All = new()
    {
        Create, List, Update, Delete
    };
}
