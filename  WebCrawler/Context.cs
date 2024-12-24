using Serilog;
using WebCrawler.ConfigurationObjects;

namespace WebCrawler;

public class Context : IContext
{
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

    public Configuration Configuration { set; get; }
    
    public ILogger Logger { get; set; }


    private Configuration GetConfiguration(string yamlFilePath)
    {
        var yaml = File.ReadAllText(yamlFilePath);
        return YamlParser.Parse<Configuration>(yaml);
    }
    
    
}