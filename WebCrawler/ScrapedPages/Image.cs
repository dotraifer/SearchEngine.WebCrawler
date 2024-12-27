namespace WebCrawler.ScrapedPages;

public record Image
{
    public string Url { get; set; }
    public string AltText { get; set; }
}