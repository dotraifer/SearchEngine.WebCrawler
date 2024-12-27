using System.Text.Json;

namespace WebCrawler.ScrapedPages;

public class ScrapedPage
{
    public required string Url { get; set; } // The URL of the scraped page
    public string? Title { get; set; } // The title of the page
    public string? HtmlContent { get; set; } // The raw HTML content
    public List<string>? Headings { get; set; } // List of headings (H1, H2, etc.)
    public List<string>? Paragraphs { get; set; } // List of paragraphs
    public List<Link>? Links { get; set; } // List of links on the page
    public List<Image>? Images { get; set; } // List of images
    public int Clicks { get; set; } = 0;
    public DateTime ScrapedAt { get; set; } // The time the page was scraped

    public override string ToString()
    {
        var jsonData = new
        {
            URL = Url,
            Title,
            ScrapedAt
        };

        return JsonSerializer.Serialize(jsonData, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}