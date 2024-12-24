using Autofac;
using Nest;

namespace WebCrawler;

public class Facade(Context context) : IHasContext
{
    public IContext Context { get; set; } = context;

    public void Run()
    {
        var builder = new ContainerBuilder();
        var settings = new ConnectionSettings(Context.Configuration.Elastic.Uri)
            .DefaultIndex("scraped_pages");
        
        builder.RegisterInstance(new ElasticClient(settings)).As<IElasticClient>();
        builder.RegisterType<ElasticConnector>().As<IElasticConnector>();
        builder.RegisterType<Crawler>().AsSelf();
        builder.RegisterInstance(Context).As<IContext>();

        var container = builder.Build();
        using var scope = container.BeginLifetimeScope();
        container.Resolve<Crawler>().CrawlAsync().RunSynchronously();
    }
}