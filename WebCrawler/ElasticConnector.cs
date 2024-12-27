using OpenSearch.Client;
using WebCrawler.Context;

namespace WebCrawler;

public class ElasticConnector(IOpenSearchClient elasticClient, IContext context)
    : IElasticConnector, IHasContext
{
    public IOpenSearchClient ElasticClient { get; set; } = elasticClient;
    public IContext Context { get; set; } = context;


    public async Task IndexObjectAsync(IList<ScrapedPage> scrapedPages, string indexName)
    {
        if (scrapedPages.Count == 0) return;

        // Prepare the bulk request
        var bulkDescriptor = new BulkDescriptor();
        foreach (var page in scrapedPages)
        {
            bulkDescriptor.Index<ScrapedPage>(op =>
                op.Document(page).Index(indexName).Id(page.Url));
        }

        // Perform bulk indexing
        var response = await ElasticClient.BulkAsync(bulkDescriptor);

        if (!response.IsValid)
        {
            Context.Logger.Error("Failed to bulk index: {Error}", response.ServerError?.Error.Reason);
        }
        else
        {
            Context.Logger.Information("Successfully indexed {Count} documents to {Index}",
                scrapedPages.Count, indexName);
        }
    }
}