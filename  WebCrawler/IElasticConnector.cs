using Nest;

namespace WebCrawler;

public interface IElasticConnector
{
    IElasticClient ElasticClient { get; set; }

    Task IndexObjectAsync(object obj, string indexName);
}