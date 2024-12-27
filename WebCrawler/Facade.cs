using Autofac;
using OpenSearch.Client;
using WebCrawler.Context;

namespace WebCrawler;

public class Facade(Context.Context context) : IHasContext
{
    public IContext Context { get; set; } = context;

    public void Run()
    {
        var builder = new ContainerBuilder();
        var settings = new ConnectionSettings(Context.Configuration.Elastic.Uri)
            .BasicAuthentication(Context.Configuration.Elastic.User, Context.Configuration.Elastic.Password)
            .DefaultIndex("scraped-pages")
            .RequestTimeout(TimeSpan.FromSeconds(300));

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
        httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.102 Safari/537.36");
        httpClient.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
        httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

        builder.RegisterInstance(httpClient).As<HttpClient>().SingleInstance();
        builder.RegisterInstance(new OpenSearchClient(settings)).As<IOpenSearchClient>().SingleInstance();
        builder.RegisterType<ElasticConnector>().As<IElasticConnector>().SingleInstance();
        builder.RegisterType<Crawler>().AsSelf();
        builder.RegisterInstance(Context).As<IContext>().SingleInstance();

        var container = builder.Build();
        using var scope = container.BeginLifetimeScope();
        _ = container.Resolve<Crawler>().CrawlAsync();
    }
}