using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Harmonia.Core.Serializer;

namespace Harmonia.Core.Agent;

public sealed class ConsoleAgent : AgentBase
{

  private readonly ConcurrentDictionary<string, string> _profile;
  private readonly Process _process;
  private readonly ISerializer _serializer;
  private IDisposable? _connector;

  public override IDictionary<string, string> Profile => _profile;

  public ConsoleAgent(ISerializer serializer, string[] args)
  {
    _profile = new();
    _serializer = serializer;
    _process = new()
    {
      StartInfo = new()
      {
        FileName = Path.GetFileNameWithoutExtension(args[0]),
        Arguments = string.Join(' ', args[1..]),
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
      }
    };
  }

  protected override void DoDispose()
  {
    _process.Dispose();
  }

  protected override void DoOpen()
  {
    foreach (var p in Process.GetProcessesByName(_process.StartInfo.FileName))
    {
      p.Kill();
    }
    _process.Start();
    _connector = Observable.Repeat(0, new EventLoopScheduler())
      .Finally(_process.Close)
      .Select(_ => _process.StandardOutput.ReadLine())
      .Subscribe(x =>
      {
        if (x is null) return;
        foreach (var y in _serializer.Deserialize(x))
          _profile.AddOrUpdate(y.Key, y.Value, (k, v) => y.Value);
      });
  }

  protected override void DoClose()
  {
    _connector?.Dispose();
  }

  protected override void DoSet(IDictionary<string, string> value)
  {
    var message = _serializer.Serialize(value);
    _process.StandardInput.WriteLine(message);
  }

  protected override void DoSetRawString(string value)
  {
    _process.StandardInput.WriteLine(value);
  }

}
