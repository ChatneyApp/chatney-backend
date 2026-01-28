using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Messages;

public class UrlPreviewMediaSize
{
    [BsonElement("width")]
    public int Width { get; set; }
    [BsonElement("height")]
    public int Height { get; set; }
}

public class UrlPreview : DatabaseItem
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public required string Id { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("url")]
    public string Url { get; set; }    // Нормализованный URL (без query params если нужно)

    [BsonElement("title")]
    public string? Title { get; set; }            // Заголовок страницы

    [BsonElement("description")]
    public string? Description { get; set; }      // Описание страницы

    [BsonElement("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }         // URL главного изображения

    [BsonElement("videoThumbnailUrl")]
    public string? VideoThumbnailUrl { get; set; }         // URL главного изображения

    [BsonElement("siteName")]
    public string? SiteName { get; set; }         // Название сайта

    [BsonElement("favIconUrl")]
    public string? FavIconUrl { get; set; }       // URL фавикона

    [BsonElement("type")]
    public string? Type { get; set; }             // Тип контента (article, website, video и т.д.)

    [BsonElement("author")]
    public string? Author { get; set; }           // Автор контента

    [BsonElement("thumbnailWidth")]
    public int? ThumbnailWidth { get; set; }

    [BsonElement("thumbnailHeight")]
    public int? ThumbnailHeight { get; set; }
}
