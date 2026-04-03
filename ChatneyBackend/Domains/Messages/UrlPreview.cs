using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Messages;

public class UrlPreview : IPgKey<UrlPreview, int>, IPgTimestamped
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Map("url")]
    [MaxLength(4096)]
    public required string Url { get; set; }

    [Map("title")]
    [MaxLength(4096)]
    public string? Title { get; set; }

    [Map("description")]
    public string? Description { get; set; }

    [Map("thumbnail_url")]
    [MaxLength(4096)]
    public string? ThumbnailUrl { get; set; }

    [Map("video_thumbnail_url")]
    [MaxLength(4096)]
    public string? VideoThumbnailUrl { get; set; }

    [Map("site_name")]
    [MaxLength(4096)]
    public string? SiteName { get; set; }

    [Map("fav_icon_url")]
    [MaxLength(4096)]
    public string? FavIconUrl { get; set; }

    [Map("type")]
    [MaxLength(255)]
    public string? Type { get; set; }

    [Map("author")]
    [MaxLength(4096)]
    public string? Author { get; set; }

    [Map("thumbnail_width")]
    public int? ThumbnailWidth { get; set; }

    [Map("thumbnail_height")]
    public int? ThumbnailHeight { get; set; }

    public static Expression<Func<UrlPreview, bool>> MatchByKey(int key) => preview => preview.Id == key;
}
