using System.ComponentModel;
using Harmonia.Core.Serializer;

namespace Harmonia.Core.Agent;

public interface IAgent : IDisposable
{
  public bool IsActive { get; }
  public IDictionary<string, string> Profile { get; }
  public void Open();
  public void Close();
  public void Set(IDictionary<string, string> value);
  public void Set(KeyValuePair<string, string> value);
  public void Set(string key, string value);
  public void Set(string key);
  public bool TryGet<T>(string key, out T value);
}

public abstract class AgentBase : IAgent
{
  public bool IsActive { get; private set; }
  public abstract IDictionary<string, string> Profile { get; }
  public void Dispose()
  {
    Close();
    DoDispose();
  }
  public void Open() 
  {
    if (IsActive) return;
    IsActive = true;
    DoOpen(); 
  }
  public void Close()
  {
    if (!IsActive) return;
    IsActive = false; 
    DoClose(); 
  }
  public void Set(IDictionary<string, string> value) 
  { 
    if (IsActive) 
      DoSet(value); 
  }
  public void Set(KeyValuePair<string, string> value) => Set(new Dictionary<string, string>(new[] { value }));
  public void Set(string key, string value) => Set(new KeyValuePair<string, string>(key, value));
  public void Set(string key) => Set(key, "");
  public bool TryGet<T>(string key, out T output)
  {
    output = default!;
    if (Profile.TryGetValue(key, out var value))
    {
      try
      {
        var converter = TypeDescriptor.GetConverter(typeof(T));
        if (converter != null)
          output = (T)converter.ConvertFromString(value);
        return output is T;
      }
      catch
      {
        return false;
      }
    }
    return false;
  }
  protected abstract void DoDispose();
  protected abstract void DoOpen();
  protected abstract void DoClose();
  protected abstract void DoSet(IDictionary<string, string> value);
}

public class AgentConfig
{
  public string Type { get; set; }
  public string Id { get; set; }
  public string[] Args { get; set; }
  public SerializerConfig Serializer { get; set; }
}

public static class AgentFactory
{
  public static IAgent Create(AgentConfig config)
  {
    var serializer = SerializerFactory.Create(config.Serializer);
    return config.Type.ToLower() switch
    {
      "console" => new ConsoleAgent(serializer, config.Args),     // 0:path, 1:args
      "tcpserver" => new TcpServerAgent(serializer, config.Args), // 0:port, (1:encoding)
      "tcpclient" => new TcpClientAgent(serializer, config.Args), // 0:ip, 1:port, (2:encoding)
      "udp" => new UdpAgent(serializer, config.Args),             // 0:local port, 1:remote ip, 2:remote port, (3:encoding)
      _ => throw new NotSupportedException()
    };
  }
}
