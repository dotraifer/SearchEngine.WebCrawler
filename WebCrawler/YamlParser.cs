using YamlDotNet.Serialization;

namespace WebCrawler;

/// <summary>
/// A class that parses YAML strings into objects.
/// </summary>
public static class YamlParser
{
    /// <summary>
    /// Parses a YAML string into an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="yaml">The YAML string to parse.</param>
    /// <returns>An object of type T deserialized from the YAML string.</returns>
    public static T Parse<T>(string yaml)
    {
        // Create the deserializer
        var deserializer = new DeserializerBuilder().Build();

        // Deserialize the YAML into the Person object
        return deserializer.Deserialize<T>(yaml);
    }
}