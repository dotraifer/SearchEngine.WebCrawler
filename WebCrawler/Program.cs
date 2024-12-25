using HtmlAgilityPack;
using WebCrawler;

class Program
{
    private static void Main(string[] args)
    {
        var context = new Context(args[0]);
        new Facade(context).Run();
    }
}