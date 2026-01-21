using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using ChatneyBackend.Domains.Messages;

namespace ChatneyBackend.Utils;

public static class UrlPreviewExtractor
{
    /// <summary>
    /// Extracts the urls in canonical form
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static List<string> ExtractUrls(string text)
    {
        var urlPattern = @"\b(?:https?:\/\/)?((?:[a-z\d]+)(?:\.[a-z\d][a-z\d-]*[a-z\d])+(?:/)?\S+)\b";
        var matches = Regex.Matches(text, urlPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        return matches
            .Select(m =>
            {
                var url = m.Value;
                if (!url.StartsWith("https://") && !url.StartsWith("http://"))
                {
                    url = $"https://{url}";
                }

                try
                {
                    return new Uri(url).ToString();
                }
                catch (UriFormatException)
                {
                    return "";
                }
            })
            .Where(url => !string.IsNullOrEmpty(url))
            .Distinct()
            .ToList();
    }

    public static async Task<UrlPreview?> GetPreviewAsync(string url)
    {
        var preview = new UrlPreview
        {
            Id = Guid.NewGuid().ToString(),
            Url = url,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        try
        {
            var config = Configuration.Default
                .WithDefaultLoader();
            var context = BrowsingContext.New(config);
            IDocument doc = await context.OpenAsync(url);
            var uri = new Uri(url);
            preview.Url = uri.AbsoluteUri;
            preview.Domain = uri.Host;

            ParseDocument(doc, preview, uri);
            return preview;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching {url}: {ex.Message}");
            return null;
        }

    }

    private static void ParseDocument(IDocument doc, UrlPreview preview, Uri baseUri)
    {
        var title = doc.QuerySelectorAll("title")
            .Select(t => t.Text().Trim())
            .ToList()
            .LastOrDefault();

        var metaNames = doc.QuerySelectorAll("meta")
            .Select(m => (name: m.GetAttribute("name"), content: m.GetAttribute("content")))
            .Aggregate(
                new Dictionary<string, string>(),
                (acc, pair) =>
                {
                    if (pair.name != null && pair.content != null)
                    {
                        if (acc.ContainsKey(pair.name))
                        {
                            acc.Remove(pair.name);
                        }
                        acc.Add(pair.name, pair.content);
                    }

                    return acc;
                });
        var metaProperties = doc.QuerySelectorAll("meta")
            .Where(m => !string.IsNullOrEmpty(m.GetAttribute("content")))
            .Select(m => (name: m.GetAttribute("property"), content: m.GetAttribute("content")))
            .Aggregate(
                new Dictionary<string, string>(),
                (acc, pair) =>
                {
                    if (pair.name != null && pair.content != null)
                    {
                        if (acc.ContainsKey(pair.name))
                        {
                            acc.Remove(pair.name);
                        }
                        acc.Add(pair.name, pair.content);
                    }

                    return acc;
                });
        var favIconUrl = doc.QuerySelectorAll("link")
            .Where(l =>
            {
                var attr = l.GetAttribute("rel")?.ToLower();
                return attr == "shortcut icon" || attr == "icon";
            })
            .Select(l => l.GetAttribute("href"))
            .LastOrDefault();

        // Open Graph tags
        preview.Title = metaProperties.GetValueOrDefault( "og:title")
                        ?? metaNames.GetValueOrDefault("twitter:title")
                        ?? title;

        preview.Description = metaProperties.GetValueOrDefault("og:description")
                              ?? metaNames.GetValueOrDefault("twitter:description")
                              ?? metaNames.GetValueOrDefault("description");

        preview.Type = metaProperties.GetValueOrDefault("og:type");
        var mediaWidth = 0;
        var mediaHeight = 0;

        int.TryParse(metaProperties.GetValueOrDefault("og:image:width"), out mediaWidth);
        int.TryParse(metaProperties.GetValueOrDefault("og:image:height"), out mediaHeight);

        if (mediaWidth == 0 || mediaHeight == 0)
        {
            int.TryParse(metaNames.GetValueOrDefault("twitter:player:width"), out mediaWidth);
            int.TryParse(metaNames.GetValueOrDefault("twitter:player:height"), out mediaHeight);
        }

        if (mediaWidth > 0 && mediaHeight > 0)
        {
            preview.ImageWidth = mediaWidth;
            preview.ImageHeight = mediaHeight;
        }

        preview.ImageUrl = metaProperties.GetValueOrDefault("og:image")
                           ?? metaProperties.GetValueOrDefault("og:image:url")
                           ?? metaNames.GetValueOrDefault("twitter:image");

        preview.VideoUrl = metaProperties.GetValueOrDefault("og:video")
                           ?? metaProperties.GetValueOrDefault("og:video:url");

        if (!string.IsNullOrEmpty(preview.ImageUrl))
        {
            preview.ImageUrl = MakeAbsoluteUrl(preview.ImageUrl, baseUri);
        }

        preview.SiteName = metaProperties.GetValueOrDefault("og:site_name");
        preview.Type = metaProperties.GetValueOrDefault( "og:type");
        preview.Author = metaProperties.GetValueOrDefault("article:author")
                         ?? metaNames.GetValueOrDefault("author");

        // Favicon
        if (!string.IsNullOrEmpty(favIconUrl))
        {
            preview.FavIconUrl = MakeAbsoluteUrl(favIconUrl, baseUri);
        }
    }

    private static string? MakeAbsoluteUrl(string url, Uri baseUri)
    {
        if (string.IsNullOrEmpty(url)) return null;

        if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            return url;
        }

        try
        {
            var absoluteUri = new Uri(baseUri, url);
            return absoluteUri.AbsoluteUri;
        }
        catch
        {
            return url;
        }
    }
}
