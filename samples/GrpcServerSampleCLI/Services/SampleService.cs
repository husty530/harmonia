using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Grpc.Core;

namespace GrpcServerSampleCLI.Services;

public class SampleService : Sample.SampleBase
{

  private readonly ILogger<SampleService> _logger;

  public SampleService(ILogger<SampleService> logger)
  {
    _logger = logger;
  }

  public override async Task Bind(IAsyncStreamReader<BindRequest> requestStream, IServerStreamWriter<BindReply> responseStream, ServerCallContext context)
  {
    Observable.Repeat(0, new EventLoopScheduler())
      .Select(_ => Console.ReadLine()?.Split(":"))
      .Where(x => x is not null && x.Length is 2)
      .Select(x => x[0].ToLower() switch
      {
        "run" => new BindReply() { Run = bool.Parse(x[1]) },
        "auto" => new BindReply() { Auto = bool.Parse(x[1]) },
        "speed" => new BindReply() { Speed = double.Parse(x[1]) },
        _ => new BindReply()
      })
      .Subscribe(x => responseStream.WriteAsync(x));
    await foreach (var req in requestStream.ReadAllAsync(context.CancellationToken))
    {
      Console.WriteLine($"latitude:{req.Latitude},longitude:{req.Longitude},heading:{req.Heading},velocity:{req.Velocity},quality:{req.GnssQuality},status:{req.Status}");
    }
  }

}