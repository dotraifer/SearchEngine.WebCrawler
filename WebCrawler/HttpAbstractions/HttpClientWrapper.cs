namespace WebCrawler.HttpAbstractions;

/// <summary>
/// A wrapper class for HttpClient that implements the IHttpClient interface.
/// </summary>
public class HttpClientWrapper : IHttpClient
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientWrapper"/> class.
    /// </summary>
    /// <param name="httpClient">The HttpClient instance to be wrapped.</param>
    public HttpClientWrapper(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <summary>
    /// Asynchronously gets the string content from the specified URL.
    /// </summary>
    /// <param name="url">The URL to get the string content from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the string content from the URL.</returns>
    public Task<string> GetStringAsync(string url)
    {
        return httpClient.GetStringAsync(url);
    }
}