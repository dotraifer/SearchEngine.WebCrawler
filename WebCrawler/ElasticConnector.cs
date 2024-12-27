using OpenSearch.Client;
using WebCrawler.Context;
using WebCrawler.ScrapedPages;

namespace WebCrawler;

public class ElasticConnector(IOpenSearchClient elasticClient, IContext context)
    : IElasticConnector, IHasContext
{
    private static List<ScrapedPage> Batch { get; set; } = [];
    public IOpenSearchClient ElasticClient { get; set; } = elasticClient;
    
    public IContext Context { get; set; } = context;


    public void IndexObjectAsync(IList<ScrapedPage> scrapedPages, string indexName)
    {
        Batch.AddRange(scrapedPages);
        Console.WriteLine(Batch.Count);
        if (scrapedPages.Count == 0) return;

        if (Batch.Count < Context.Configuration.Elastic.BulkSize)
            return;


        // Prepare the bulk request
        var bulkDescriptor = new BulkDescriptor();
        foreach (var page in Batch)
            bulkDescriptor.Index<ScrapedPage>(op =>
                op.Document(page).Index(indexName).Id(page.Url));

        // Perform bulk indexing
        var response = ElasticClient.Bulk(bulkDescriptor);
        if (!response.IsValid)
        {
            Context.Logger.Error("Failed to bulk index: {Error}", response.ServerError.Error.RootCause);
        }
        else
        {
            Context.Logger.Information("Successfully indexed {Count} documents to {Index}",
                Batch.Count, indexName);
            Batch.Clear();
        }
    }
}