using Serilog;

namespace WebCrawler;

public interface IContext
{
    ConfigurationObjects.Configuration Configuration { get; set; }
    
    ILogger Logger { get; set; }
    
    
}