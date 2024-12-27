using YamlDotNet.Serialization;

namespace WebCrawler;

public static class YamlParser
{
    public static T Parse<T>(string yaml)
    {
        // Create the deserializer
        var deserializer = new DeserializerBuilder().Build();

        // Deserialize the YAML into the Person object
        return deserializer.Deserialize<T>(yaml);
    }
}