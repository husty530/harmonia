namespace Harmonia.Core.Serializer;

public sealed class SeparatorSerializer : ISerializer
{

  private readonly string _elementSeparator;
  private readonly string _keyValueSeparator;

  public SeparatorSerializer(string elementSeparator, string keyValueSeparator)
  {
    _elementSeparator = elementSeparator;
    _keyValueSeparator = keyValueSeparator;
  }

  public string Serialize(IDictionary<string, string> value)
  {
    return string.Join(_elementSeparator, value.Select(x => $"{x.Key}{_keyValueSeparator}{x.Value}")) ?? "";
  }

  public IDictionary<string, string> Deserialize(string value)
  {
    return value
      .Split(_elementSeparator)
      .Select(x => x.Split(_keyValueSeparator))
      .ToDictionary(x => x[0].Trim(), x => x.Length > 1 ? x[1].Trim() : "");
  }
}
