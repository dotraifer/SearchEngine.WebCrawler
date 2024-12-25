using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Serilog.Events;

namespace WebCrawler.ConfigurationObjects;

public record Configuration
{
    [Required, Description("Elastic configuration")]
    public required ElasticConfiguration Elastic { get; init; }
    public LogEventLevel LogLevel { get; init; } = LogEventLevel.Debug;
    
    [Required, Description("List of sites urls to scrape")]
    public required List<string?> UrlList { get; init; }

    [Description("Number of conccurent scrape tasks")]
    public int NumberOfConcurrentTasks { get; init; } = 5;

    public int MaximumSearchDepth { get; init; } = 5;
}