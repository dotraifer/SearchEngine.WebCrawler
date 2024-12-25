using Autofac;
using OpenSearch.Client;
using OpenSearch.Net;

namespace WebCrawler;

public class Facade(Context context) : IHasContext
{
    public IContext Context { get; set; } = context;

    public void Run()
    {
        var builder = new ContainerBuilder();
        var settings = new ConnectionSettings(Context.Configuration.Elastic.Uri)
            .BasicAuthentication(Context.Configuration.Elastic.User, Context.Configuration.Elastic.Password)
            .DefaultIndex("scraped-pages");
            
        
        builder.RegisterInstance(new OpenSearchClient(settings)).As<IOpenSearchClient>();
        builder.RegisterType<ElasticConnector>().As<IElasticConnector>();
        builder.RegisterType<Crawler>().AsSelf();
        builder.RegisterInstance(Context).As<IContext>();

        var container = builder.Build();
        using var scope = container.BeginLifetimeScope();
        container.Resolve<Crawler>().CrawlAsync().RunSynchronously();
    }
}