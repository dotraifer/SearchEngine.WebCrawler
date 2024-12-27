using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using ConcurrentCollections;
using Moq;
using NUnit.Framework.Legacy;
using Serilog;
using WebCrawler.ConfigurationObjects;
using WebCrawler.Context;
using WebCrawler.HttpAbstractions;
using WebCrawler.ScrapedPages;

namespace WebCrawler.Tests;

public class CrawlerTests
{
    private readonly Mock<IContext> _mockContext = new();
    private readonly Mock<IElasticConnector> _mockElasticConnector  = new();
    private readonly Mock<IHttpClient> _mockHttpClient = new();

    private readonly FieldInfo? _urlsToScrapeField = typeof(Crawler).
        GetField("_urlsToScrape", BindingFlags.NonPublic | BindingFlags.Instance);

    private readonly FieldInfo? _visitedUrlsFieldInfo = typeof(Crawler).
        GetField("_visitedUrls", BindingFlags.NonPublic | BindingFlags.Instance);

    private readonly MethodInfo? _processUrlAsyncMethod = typeof(Crawler).
        GetMethod("ProcessUrlAsync", BindingFlags.NonPublic | BindingFlags.Instance);



    [SetUp]
    public void Setup()
    {
        _mockContext.SetupGet(c => c.Logger).Returns(new LoggerConfiguration().CreateLogger());
        _mockContext.SetupGet(c => c.Configuration).Returns(new Configuration
        {
            NumberOfConcurrentTasks = 2,
            MaximumSearchDepth = 2,
            UrlList = ["http://example.com", "http://example2.com"],
            Elastic = new ElasticConfiguration
            {
                Uri = new Uri("http://localhost:9200"),
                IndexName = "webcrawler",
                User = "elastic",
                Password = "Password1"
            }
        });
    }

    [Test]
        public void CrawlUrls_ProcessesAllUrlsInQueue()
        {
            var crawler = new Crawler(_mockContext.Object, _mockElasticConnector.Object, _mockHttpClient.Object);
            crawler.CrawlUrls();

            _mockElasticConnector.Verify(e => e.IndexObjectAsync(It.IsAny<List<ScrapedPage>>(),
                It.IsAny<string>()), Times.AtLeastOnce);

            var urlsQueue =  (ConcurrentQueue<KeyValuePair<string, int>>)_urlsToScrapeField?.GetValue(crawler)!;

            Assert.That(urlsQueue, Is.Empty);
        }

        [Test]
        public async Task ProcessUrlAsync_ReturnsNullForVisitedUrl()
        {

            var crawler = new Crawler(_mockContext.Object, _mockElasticConnector.Object, _mockHttpClient.Object);
            _visitedUrlsFieldInfo?.SetValue(crawler, new ConcurrentHashSet<string> { "http://example.com" });

            var task = (Task<ScrapedPage?>?)_processUrlAsyncMethod?.Invoke(crawler, ["http://example.com", 1]);
            if (task != null)
            {
                await task.ConfigureAwait(false);

                // Retrieve the result using reflection
                var resultProperty = task.GetType().GetProperty("Result");
                var result = (ScrapedPage?)resultProperty?.GetValue(task);

                Assert.That(result, Is.Null);
            }
        }

        [Test]
        public void ProcessUrlAsync_EnqueuesNewLinks()
        {
            _mockHttpClient.Setup(c => c.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync("<a href='http://example.com/page1'>Link</a>");
        
            var crawler = new Crawler(_mockContext.Object, _mockElasticConnector.Object, _mockHttpClient.Object);
        
            _ = (Task<ScrapedPage?>?)_processUrlAsyncMethod?.Invoke(crawler, ["http://example.com", 1]);
        
            Assert.That((ConcurrentQueue<KeyValuePair<string, int>>)_urlsToScrapeField?.GetValue(crawler)!,
                Has.Some.Matches<KeyValuePair<string, int>>(kvp => kvp.Key == "http://example.com/page1"));
        }
}