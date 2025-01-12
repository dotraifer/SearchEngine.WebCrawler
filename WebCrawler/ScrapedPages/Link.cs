namespace WebCrawler.ScrapedPages;

public record Link
{
    public string Url { get; set; }
    public string Text { get; set; } // Anchor text
}