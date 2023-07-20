namespace Harmonia.Core.Serializer;

public sealed class JsonSerializer : ISerializer
{
  public string Serialize(IDictionary<string, string> value)
  {
    return System.Text.Json.JsonSerializer.Serialize(value);
  }

  public IDictionary<string, string> Deserialize(string value)
  {
    return System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, string>>(value) ?? new Dictionary<string, string>();
  }

}
