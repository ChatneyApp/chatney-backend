namespace ChatneyBackend.Utils;

public class AppConfig
{
    public string UserPasswordSalt { get; set; }

    public string JwtSecret { get; set;}

    public string S3Bucket { get; set; }
}