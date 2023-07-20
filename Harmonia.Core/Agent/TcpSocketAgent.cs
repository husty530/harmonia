using System.Collections.Concurrent;
using Harmonia.Core.Serializer;

namespace Harmonia.Core.Agent;

public sealed class TcpSocketAgent : AgentBase
{

  private readonly ConcurrentDictionary<string, string> _profile;
  private readonly ISerializer _serializer;

  public override IDictionary<string, string> Profile => _profile;

  public TcpSocketAgent(ISerializer serializer, string[] args)
  {
    _profile = new();
    _serializer = serializer;
  }

  protected override void DoDispose()
  {

  }

  protected override void DoOpen()
  {

  }

  protected override void DoClose()
  {

  }

  protected override void DoSet(IDictionary<string, string> value)
  {

  }

}
