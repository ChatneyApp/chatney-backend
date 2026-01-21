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

    [BsonElement("imageUrl")]
    public string? ImageUrl { get; set; }         // URL главного изображения

    [BsonElement("videoUrl")]
    public string? VideoUrl { get; set; }         // URL главного изображения

    [BsonElement("siteName")]
    public string? SiteName { get; set; }         // Название сайта

    [BsonElement("favIconUrl")]
    public string? FavIconUrl { get; set; }       // URL фавикона

    [BsonElement("type")]
    public string? Type { get; set; }             // Тип контента (article, website, video и т.д.)

    [BsonElement("author")]
    public string? Author { get; set; }           // Автор контента

    [BsonElement("imageWidth")]
    public int? ImageWidth { get; set; }

    [BsonElement("imageHeight")]
    public int? ImageHeight { get; set; }

    [BsonElement("domain")]
    public string? Domain { get; set; }           // Домен сайта
}
