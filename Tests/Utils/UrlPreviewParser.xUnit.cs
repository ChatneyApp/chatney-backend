using ChatneyBackend.Utils;
using Xunit.Abstractions;

namespace ChatneyBackend.Tests.Utils;

public class UrlPreviewParserTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UrlPreviewParserTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ExtractUrls()
    {
        var text = @"Look at these articles: 
                https://github.com/dotnet/runtime 
                and https://docs.microsoft.com/dotnet
                and here is a bit more: dorian.live
                youtube.com/watch?v=dQw4w9WgXcQ
";

        var urls = UrlPreviewExtractor.ExtractUrls(text);
        var expected = new List<string>
        {
            "https://github.com/dotnet/runtime",
            "https://docs.microsoft.com/dotnet",
            "https://dorian.live/",
            "https://youtube.com/watch?v=dQw4w9WgXcQ",
        };
        Assert.Equal(expected, urls);
    }

    [Fact]
    public async void ExtractPreviewFromUrls()
    {
        var urls = new List<string>
        {
            // "https://github.com/dotnet/runtime",
            // "https://docs.microsoft.com/dotnet",
            // "https://dorian.live/",
            "https://youtube.com/watch?v=dQw4w9WgXcQ",
        };

        foreach (var url in urls)
        {
            _testOutputHelper.WriteLine($"\nWorking on: {url}");
            try {
                var preview = await UrlPreviewExtractor.GetPreviewAsync(url);
                _testOutputHelper.WriteLine($"  Title: {preview.Title}");
                _testOutputHelper.WriteLine(
                    $"  Описание: {preview.Description.Substring(0, Math.Min(100, preview.Description.Length))}...");
                _testOutputHelper.WriteLine($"  Image: {preview.ImageUrl}");
                if (preview.ImageWidth != null && preview.ImageHeight != null)
                {
                    _testOutputHelper.WriteLine($"  Media size: {preview.ImageWidth}x{preview.ImageHeight}");
                }

                _testOutputHelper.WriteLine($"  Domain: {preview.Domain}");
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine($"  Error retrieving the url preview {ex.Message}");
            }
        }
    }
}
