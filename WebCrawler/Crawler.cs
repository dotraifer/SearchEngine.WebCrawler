using System.Collections.Concurrent;
using ConcurrentCollections;
using HtmlAgilityPack;
using WebCrawler.Context;
using WebCrawler.ScrapedPages;

namespace WebCrawler;

/// <summary>
/// Represents a web crawler that processes URLs and scrapes web pages.
/// </summary>
public class Crawler : IHasContext, ICrawler
{
    /// <summary>
    /// Gets or sets the context for the crawler.
    /// </summary>
    public IContext Context { get; set; }

    private readonly IElasticConnector _elasticConnector;
    private readonly ConcurrentQueue<KeyValuePair<string, int>> _urlsToScrape = new();
    private readonly ConcurrentHashSet<string> _visitedUrls = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="Crawler"/> class.
    /// </summary>
    /// <param name="context">The context for the crawler.</param>
    /// <param name="elasticConnector">The elastic connector for indexing scraped pages.</param>
    /// <param name="httpClient">The HTTP client for fetching web pages.</param>
    public Crawler(IContext context, IElasticConnector elasticConnector, HttpClient httpClient)
    {
        HttpClient = httpClient;
        Context = context;
        Context.Configuration.UrlList?.ForEach(url => _urlsToScrape?.Enqueue(new KeyValuePair<string, int>(url, 0)));
        _elasticConnector = elasticConnector;
    }

    private HttpClient HttpClient { get; }

    /// <summary>
    /// Crawls the URLs in the queue in parallel.
    /// </summary>
    public void CrawlUrls()
    {
        while (!_urlsToScrape.IsEmpty)
        {
            var pages = new ConcurrentBag<ScrapedPage>();
            var batches = DequeueBatch(Math.Min(_urlsToScrape.Count, Context.Configuration.NumberOfConcurrentTasks));
            Parallel.ForEach(batches, batch =>
            {
                var scrapedPage = ProcessUrlAsync(batch.Key, batch.Value).Result;
                if (scrapedPage != null) pages.Add(scrapedPage);
            });
            _elasticConnector.IndexObjectAsync(pages.ToList(), $"{GetType().Name.ToLower()}");
        }
    }

    /// <summary>
    /// Dequeues a batch of URLs from the queue.
    /// </summary>
    /// <param name="batchSize">The size of the batch to dequeue.</param>
    /// <returns>A collection of URL and depth pairs.</returns>
    private IEnumerable<KeyValuePair<string, int>> DequeueBatch(int batchSize)
    {
        var batch = new List<KeyValuePair<string, int>>();
        while (batch.Count < batchSize && _urlsToScrape.TryDequeue(out var pair)) batch.Add(pair);
        return batch;
    }

    /// <summary>
    /// Processes a URL asynchronously.
    /// </summary>
    /// <param name="url">The URL to process.</param>
    /// <param name="currentDepth">The current depth of the URL in the crawl.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the scraped page or null if the URL was already visited.</returns>
    private async Task<ScrapedPage?> ProcessUrlAsync(string url, int currentDepth)
    {
        if (!_visitedUrls.Add(url)) return null; // Skip if already visited
        try
        {
            // Fetch the content of the URL
            var content = await HttpClient.GetStringAsync(url);
            Context.Logger.Information("Processing {Url}", url);
            // Extract links from the content
            var links = ExtractLinks(url, content);

            // Enqueue new links for crawling if within the depth limit
            if (currentDepth + 1 <= Context.Configuration.MaximumSearchDepth)
                foreach (var link in links.Where(link => !_visitedUrls.Contains(link.Url)))
                    _urlsToScrape.Enqueue(new KeyValuePair<string, int>(link.Url, currentDepth + 1));
            else
                Context.Logger.Information("Max depth {Depth} reached for {Url}", currentDepth, url);

            // Parse and create a ScrapedPage object
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            var title = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim();
            var headings = htmlDoc.DocumentNode
                .SelectNodes("//h1 | //h2 | //h3")
                ?.Select(h => h.InnerText.Trim())
                .ToList() ?? new List<string>();
            var paragraphs = htmlDoc.DocumentNode
                .SelectNodes("//p")
                ?.Select(p => p.InnerText.Trim())
                .ToList() ?? new List<string>();

            var images = htmlDoc.DocumentNode
                .SelectNodes("//img[@src]")
                ?.Select(node => new Image
                {
                    Url = node.GetAttributeValue("src", string.Empty),
                    AltText = node.GetAttributeValue("alt", string.Empty)
                })
                .ToList() ?? new List<Image>();

            return new ScrapedPage
            {
                Url = url,
                Title = title,
                Images = images,
                HtmlContent = content,
                Headings = headings,
                Paragraphs = paragraphs,
                Links = links,
                ScrapedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _urlsToScrape.Enqueue(new KeyValuePair<string, int>(url, currentDepth));
            Context.Logger.Error("Error processing {Url}: {Message}", url, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Extracts links from the HTML content of a page.
    /// </summary>
    /// <param name="baseUrl">The base URL of the page.</param>
    /// <param name="htmlContent">The HTML content of the page.</param>
    /// <returns>A list of links extracted from the page.</returns>
    private static List<Link> ExtractLinks(string baseUrl, string htmlContent)
    {
        var links = new List<Link>();
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        var nodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
        if (nodes != null)
            foreach (var node in nodes)
            {
                var href = node.GetAttributeValue("href", string.Empty);
                if (!string.IsNullOrEmpty(href))
                {
                    // Convert relative URLs to absolute URLs
                    var baseUri = new Uri(baseUrl);
                    Uri fullUri;
                    if (Uri.TryCreate(baseUri, href, out fullUri))
                        links.Add(new Link
                        {
                            Text = node.Name,
                            Url = fullUri.ToString()
                        });
                }
            }

        return links;
    }
}