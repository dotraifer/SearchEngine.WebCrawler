namespace WebCrawler.Context;

public interface IHasContext
{
    IContext Context { get; set; }
}