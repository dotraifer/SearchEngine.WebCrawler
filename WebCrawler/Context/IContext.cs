using Serilog;
using WebCrawler.ConfigurationObjects;

namespace WebCrawler.Context;

public interface IContext
{
    Configuration Configuration { get; set; }

    ILogger Logger { get; set; }
}