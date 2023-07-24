using System.Collections.Concurrent;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Harmonia.Core.Serializer;

namespace Harmonia.Core.Agent;

public class UdpAgent : AgentBase
{

  private readonly ConcurrentDictionary<string, string> _profile;
  private readonly ISerializer _serializer;
  private readonly UdpClient _client;
  private readonly string _ip;
  private readonly int _port;
  private readonly Encoding _encoding;
  private CancellationTokenSource? _cts;

  public override IDictionary<string, string> Profile => _profile;

  public UdpAgent(ISerializer serializer, string[] args)
  {
    _profile = new();
    _serializer = serializer;
    _client = new(int.Parse(args[0]));
    _ip = args[1];
    _port = int.Parse(args[2]);
    _encoding = Encoding.Default;
    if (args.Length > 3)
    {
      _encoding = args[3].ToLower() switch
      {
        "utf-32" => Encoding.UTF32,
        "utf-8" => Encoding.UTF8,
        "ascii" => Encoding.ASCII,
        "unicode" => Encoding.Unicode,
        _ => Encoding.Default
      };
    }
  }

  protected override void DoDispose()
  {

  }

  protected override void DoOpen()
  {
    IPEndPoint? ep = null;
    _cts = new();
    _client.Connect(_ip, _port);
    Observable.Repeat(0, new EventLoopScheduler())
      .Finally(_client.Close)
      .TakeUntil(_ => _cts.IsCancellationRequested)
      .Where(_ => _client.Available > 0)
      .Select(_ => _client.Receive(ref ep))
      .Subscribe(x =>
      {
        if (x is null || !x.Any()) return;
        var data = _encoding.GetString(x);
        foreach (var y in _serializer.Deserialize(data))
          _profile.AddOrUpdate(y.Key, y.Value, (k, v) => y.Value);
      });
  }

  protected override void DoClose()
  {
    _cts?.Cancel();
  }

  protected override void DoSet(IDictionary<string, string> value)
  {
    var data = _encoding.GetBytes(_serializer.Serialize(value));
    _client.Send(data, _ip, _port);
  }

}
