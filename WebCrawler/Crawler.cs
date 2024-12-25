using System.Collections.Concurrent;
using ConcurrentCollections;
using HtmlAgilityPack;

namespace WebCrawler;

public class Crawler : IHasContext, ICrawler
{
    public IContext Context { get; set; }
    private readonly ConcurrentQueue<KeyValuePair<string, int>> _urlsToScrape = new();
    private readonly HttpClient _httpClient = new();
    private readonly ConcurrentHashSet<string> _visitedUrls = new(StringComparer.OrdinalIgnoreCase);
    private readonly IElasticConnector _elasticConnector;

    public Crawler(IContext context,  IElasticConnector elasticConnector)
    {
        Context = context;
        Context.Configuration.UrlList?.ForEach(url => _urlsToScrape?.Enqueue
            (new KeyValuePair<string, int>(url, 0)));
        _elasticConnector = elasticConnector;
    }

    public async Task CrawlAsync()
    {
        while (!_urlsToScrape.IsEmpty)
        {
            var batches = DequeueBatch
                (Math.Min(_urlsToScrape.Count, Context.Configuration.NumberOfConcurrentTasks));

            Parallel.ForEach(batches, batch =>
            {
                var scrapedPage = ProcessUrlAsync(batch.Key, batch.Value).Result;
                Task.Delay(1000);
                if (scrapedPage != null)
                {
                    _elasticConnector.IndexObjectAsync(scrapedPage, 
                        $"{GetType().Name.ToLower()}-{scrapedPage.ScrapedAt:dd-MM-yy}");
                }
            });
        };
    }
    
    private IEnumerable<KeyValuePair<string, int>> DequeueBatch(int batchSize)
    {
        var batch = new List<KeyValuePair<string, int>>();
        while (batch.Count < batchSize && _urlsToScrape.TryDequeue(out var pair))
        {
            batch.Add(pair);
        }
        return batch;
    }
    
    
    private async Task<ScrapedPage?> ProcessUrlAsync(string url, int currentDepth)
    {
        if (!_visitedUrls.Add(url))
        {
            return null; // Skip if already visited
        }
        try
        {
            // Fetch the content of the URL
            var content = await _httpClient.GetStringAsync(url);
            // Extract links from the content
            var links = ExtractLinks(url, content);

            // Enqueue new links for crawling
            // Enqueue new links for crawling if within the depth limit
            if (currentDepth + 1 <= Context.Configuration.MaximumSearchDepth)
            {
                foreach (var link in links.Where(link => !_visitedUrls.Contains(link.Url)))
                {
                    _urlsToScrape.Enqueue(new KeyValuePair<string, int>(link.Url, currentDepth+1));
                }
            }
            else
            {
                Context.Logger.Information("Max depth {Depth} reached for {Url}", currentDepth, url);
            }

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
    
    private static List<Link> ExtractLinks(string baseUrl, string htmlContent)
    {
        var links = new List<Link>();
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        var nodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var href = node.GetAttributeValue("href", string.Empty);
                if (!string.IsNullOrEmpty(href))
                {
                    // Convert relative URLs to absolute URLs
                    Uri baseUri = new Uri(baseUrl);
                    Uri fullUri;
                    if (Uri.TryCreate(baseUri, href, out fullUri))
                    {
                        links.Add(new Link
                        {
                            Text = node.Name,
                            Url = fullUri.ToString()
                        });
                    }
                }
            }
        }
        
        return links;
    }
}