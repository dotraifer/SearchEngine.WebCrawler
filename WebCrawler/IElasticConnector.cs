using OpenSearch.Client;
using WebCrawler.ScrapedPages;

namespace WebCrawler;

public interface IElasticConnector
{
    IOpenSearchClient ElasticClient { get; set; }

    void IndexObjectAsync(IList<ScrapedPage> scrapedPages, string indexName);
}