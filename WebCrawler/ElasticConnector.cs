using OpenSearch.Client;
using WebCrawler.Context;
using WebCrawler.ScrapedPages;

namespace WebCrawler;

/// <summary>
/// Provides functionality to connect to and interact with an OpenSearch instance.
/// </summary>
public class ElasticConnector : IElasticConnector, IHasContext
{
    private static List<ScrapedPage> Batch { get; set; } = new List<ScrapedPage>();
    public IOpenSearchClient ElasticClient { get; set; }
    public IContext Context { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticConnector"/> class.
    /// </summary>
    /// <param name="elasticClient">The OpenSearch client to be used.</param>
    /// <param name="context">The context to be used.</param>
    public ElasticConnector(IOpenSearchClient elasticClient, IContext context)
    {
        ElasticClient = elasticClient;
        Context = context;
    }

    /// <summary>
    /// Asynchronously indexes a list of scraped pages to the specified index.
    /// </summary>
    /// <param name="scrapedPages">The list of scraped pages to be indexed.</param>
    /// <param name="indexName">The name of the index.</param>
    public void IndexObjectAsync(IList<ScrapedPage> scrapedPages, string indexName)
    {
        Batch.AddRange(scrapedPages);
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