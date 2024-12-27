using OpenSearch.Client;

namespace WebCrawler;

public interface IElasticConnector
{
    IOpenSearchClient ElasticClient { get; set; }

    Task IndexObjectAsync(IList<ScrapedPage> scrapedPages, string indexName);
}