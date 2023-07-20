namespace Harmonia.Core.Serializer;

public interface ISerializer
{
  public string Serialize(IDictionary<string, string> value);
  public IDictionary<string, string> Deserialize(string value);
}

public record class SerializerConfig
{
  public string Type { get; set; }
  public string[] Args { get; set; }
}

public static class SerializerFactory
{
  public static ISerializer Create(SerializerConfig config)
  {
    return config.Type.ToLower() switch
    {
      "json" => new JsonSerializer(),
      "xml" => new XmlSerializer(),
      "separator" => new SeparatorSerializer(config.Args[0], config.Args[1]),
      _ => throw new NotSupportedException()
    };
  }
}
