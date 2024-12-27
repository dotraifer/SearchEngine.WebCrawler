namespace WebCrawler.HttpAbstractions;

/// <summary>
/// Defines an interface for an HTTP client.
/// </summary>
public interface IHttpClient
{
    /// <summary>
    /// Asynchronously gets the string content from the specified URL.
    /// </summary>
    /// <param name="url">The URL to get the string content from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the string content from the URL.</returns>
    Task<string> GetStringAsync(string url);
}