using OpenSearch.Client;

namespace WebCrawler;

public class ElasticConnector(IOpenSearchClient elasticClient, IContext context) : IElasticConnector, IHasContext
{
    public IOpenSearchClient ElasticClient { get; set; } = elasticClient;
    public IContext Context { get; set; } = context;

    public async Task IndexObjectAsync(ScrapedPage scrapedPage, string indexName)
    {
        // Get the index name based on the current date
        var response = await ElasticClient.IndexAsync
            (scrapedPage, idx => idx.Index(indexName).Id(scrapedPage.Url));
    
        if (!response.IsValid)
        {
            Context.Logger.Error("Failed to index {ScrapedPage}: {Error}",
                scrapedPage.ToString(), response.ServerError?.Error.Reason);
        }
        else
        {
            Context.Logger.Information("Indexed {ScrapedPage} to {Index}", 
                scrapedPage.ToString(), indexName);
        }
    }
}