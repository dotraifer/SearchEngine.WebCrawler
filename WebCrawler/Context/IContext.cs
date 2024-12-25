using Serilog;

namespace WebCrawler.Context;

public interface IContext
{
    ConfigurationObjects.Configuration Configuration { get; set; }
    
    ILogger Logger { get; set; }
    
    
}