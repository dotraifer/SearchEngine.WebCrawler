﻿using WebCrawler;
using WebCrawler.Context;

internal class Program
{
    private static void Main(string[] args)
    {
        var context = new Context(args[0]);
        var facade = new Facade(context);
        while (true) facade.Run();
    }
}