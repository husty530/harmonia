using Harmonia.Core.Agent;
using Microsoft.AspNetCore.Components;

namespace Harmonia.RCL;

public static class Extensions
{
  public static IDictionary<string, IComponent> GetOrderedProfile(
    this IAgent agent,
    IDictionary<string, Func<string, IComponent>> args
  )
  {
    var dict = new Dictionary<string, IComponent>();
    foreach (var arg in args)
      if (agent.Profile.TryGetValue(arg.Key, out var value))
        dict.Add(arg.Key, arg.Value(value));
    return dict;
  }
}
