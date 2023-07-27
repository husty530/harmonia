namespace Harmonia.Core.Serializer;

public class ValueOnlySerializer : ISerializer
{
  public string Serialize(IDictionary<string, string> value)
  {
    return value.Last().Value;
  }

  public IDictionary<string, string> Deserialize(string value)
  {
    return new Dictionary<string, string>(new KeyValuePair<string, string>[] { new("", value) });
  }
}
