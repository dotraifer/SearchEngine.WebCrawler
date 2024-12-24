using Nest;

namespace WebCrawler;

public class ElasticConnector(IElasticClient elasticClient, IContext context) : IElasticConnector, IHasContext
{
    public IElasticClient ElasticClient { get; set; } = elasticClient;
    public IContext Context { get; set; } = context;

    public async Task IndexObjectAsync(object obj, string indexName)
    { 
        // Get the index name based on the current date
        var response = await ElasticClient.IndexAsync(obj, idx => idx.Index(indexName));
    
        if (!response.IsValid)
        {
            Context.Logger.Error("Failed to index object {Object}: {Error}",
                obj.GetType().Name, response.ServerError?.Error.Reason);
        }
        else
        {
            Context.Logger.Information("Indexed object {Object} to {Index}", 
                obj.GetType().Name, indexName);
        }
    }
}