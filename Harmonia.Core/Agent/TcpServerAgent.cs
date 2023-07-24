using System.Collections.Concurrent;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Harmonia.Core.Serializer;

namespace Harmonia.Core.Agent;

public class TcpServerAgent : AgentBase
{

  private readonly ConcurrentDictionary<string, string> _profile;
  private readonly ISerializer _serializer;
  private readonly TcpListener _listener;
  private readonly Encoding _encoding;
  private TcpClient? _client;
  private NetworkStream? _stream;
  private StreamWriter? _writer;
  private StreamReader? _reader;
  private CancellationTokenSource? _cts;

  public override IDictionary<string, string> Profile => _profile;

  public TcpServerAgent(ISerializer serializer, string[] args)
  {
    _profile = new();
    _serializer = serializer;
    _listener = new(IPAddress.Any, int.Parse(args[0]));
    _encoding = Encoding.Default;
    if (args.Length > 1)
    {
      _encoding = args[1].ToLower() switch
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
    _cts = new();
    _listener.Start();
    _client = _listener.AcceptTcpClient();
    _stream = _client.GetStream();
    _writer = new(_stream, _encoding);
    _reader = new(_stream, _encoding);
    Observable.Repeat(0, new EventLoopScheduler())
      .Finally(() =>
      {
        _reader?.Close();
        _reader = null;
        _writer?.Close();
        _writer = null;
        _stream?.Close();
        _stream = null;
        _client?.Close();
        _listener.Stop();
      })
      .TakeUntil(_ => _cts.IsCancellationRequested)
      .Select(_ => _reader.ReadLine())
      .Subscribe(x =>
      {
        if (x is null) return;
        foreach (var y in _serializer.Deserialize(x))
          _profile.AddOrUpdate(y.Key, y.Value, (k, v) => y.Value);
      });
  }

  protected override void DoClose()
  {
    _cts?.Cancel();
  }

  protected override void DoSet(IDictionary<string, string> value)
  {
    if (_writer is not null)
      _writer.WriteLine(_serializer.Serialize(value));
  }
}
