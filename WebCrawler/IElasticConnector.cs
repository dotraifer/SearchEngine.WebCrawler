using OpenSearch.Client;
using WebCrawler.ScrapedPages;

namespace WebCrawler;

/// <summary>
/// Defines an interface for an Elastic connector.
/// </summary>
public interface IElasticConnector
{
    /// <summary>
    /// Gets or sets the OpenSearch client.
    /// </summary>
    IOpenSearchClient ElasticClient { get; set; }

    /// <summary>
    /// Asynchronously indexes a list of scraped pages to the specified index.
    /// </summary>
    /// <param name="scrapedPages">The list of scraped pages to be indexed.</param>
    /// <param name="indexName">The name of the index.</param>
    void IndexObjectAsync(IList<ScrapedPage> scrapedPages, string indexName);
}