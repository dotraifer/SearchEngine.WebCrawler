namespace WebCrawler;

/// <summary>
/// Defines an interface for a web crawler.
/// </summary>
public interface ICrawler
{
    /// <summary>
    /// Initiates the crawling process for URLs.
    /// </summary>
    void CrawlUrls();
}