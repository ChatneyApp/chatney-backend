namespace ChatneyBackend.Utils;

public class AppConfig
{
    public string UserPasswordSalt { get; set; }

    public string JwtSecret { get; set;}

    public string S3Bucket { get; set; }

    /// <summary>Loaded from the "system.adminUserId" config. Null until the system is installed.</summary>
    public Guid? AdminUserId { get; set; }

    /// <summary>Loaded from the "system.defaultUserRoleId" config. Null until the system is installed.</summary>
    public int? DefaultUserRoleId { get; set; }
}
