using Serilog;
using WebCrawler.ConfigurationObjects;

namespace WebCrawler.Context;

/// <summary>
/// Represents the context for the web crawler, including configuration and logging.
/// </summary>
public class Context : IContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    /// <param name="yamlFilePath">The file path to the YAML configuration file.</param>
    public Context(string yamlFilePath)
    {
        Configuration = GetConfiguration(yamlFilePath);
        Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/webcrawler-.logs",
                rollingInterval: RollingInterval.Day)
            .MinimumLevel.Is(Configuration.LogLevel)
            .CreateLogger();
    }

    /// <summary>
    /// Gets or sets the configuration for the web crawler.
    /// </summary>
    public Configuration Configuration { set; get; }

    /// <summary>
    /// Gets or sets the logger for the web crawler.
    /// </summary>
    public ILogger Logger { get; set; }

    /// <summary>
    /// Reads the YAML configuration file and parses it into a <see cref="Configuration"/> object.
    /// </summary>
    /// <param name="yamlFilePath">The file path to the YAML configuration file.</param>
    /// <returns>The parsed <see cref="Configuration"/> object.</returns>
    private static Configuration GetConfiguration(string yamlFilePath)
    {
        var yaml = File.ReadAllText(yamlFilePath);
        return YamlParser.Parse<Configuration>(yaml);
    }
}