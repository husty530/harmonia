using System.Xml.Linq;

namespace Harmonia.Core.Serializer;

public sealed class XmlSerializer : ISerializer
{
  public string Serialize(IDictionary<string, string> value)
  {
    return string.Join("", value.Select(x => $"<{x.Key}>{x.Value}</{x.Key}>"));
  }

  public IDictionary<string, string> Deserialize(string value)
  {
    return XDocument.Parse(value).Elements().ToDictionary(x => x.Name.ToString(), x => x.Value);
  }
}
