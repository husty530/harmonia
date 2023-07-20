using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Harmonia.Core.Agent;

namespace Harmonia.Core;

public static class Injection
{
  public static void AddHarmoniaCore(this IServiceCollection services, string filePath)
  {
    var agents = new ConcurrentDictionary<string, IAgent>();
    var config = new ConfigurationBuilder().AddJsonFile(filePath).Build();
    if (config.GetSection("Agents").Get<AgentConfig[]>() is AgentConfig[] configs)
    {
      foreach (var cfg in configs)
        agents.AddOrUpdate(cfg.Id, AgentFactory.Create(cfg), (k, v) => v);
    }
    services.AddSingleton<IDictionary<string, IAgent>>(agents);
  }
}