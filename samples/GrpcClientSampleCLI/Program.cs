using Grpc.Core;
using Grpc.Net.Client;
using GrpcClientSampleCLI;

using var channel = GrpcChannel.ForAddress("https://localhost:7143");
var client = new Sample.SampleClient(channel);
var streams = client.Bind();

var status = 0;
var lat = 43.0735;
var lon = 141.336;
var heading = 0.0;
var velocity = 0.0;

var cts = new CancellationTokenSource();

var task = Task.Run(async () =>
{
  while (!cts.IsCancellationRequested)
  {
    await Task.Delay(100);
    lat += (new Random().NextDouble() - 0.5) * 0.00001;
    lon += (new Random().NextDouble() - 0.5) * 0.00001;
    var req = new BindRequest()
    {
      Latitude = lat,
      Longitude = lon,
      Heading = heading,
      Velocity = velocity,
      GnssQuality = status is 0 ? 0 : 4,
      Status = status
    };
    await streams.RequestStream.WriteAsync(req);
  }
  await channel.ShutdownAsync();
});

await foreach (var rep in streams.ResponseStream.ReadAllAsync(cts.Token))
{
  switch (rep.OperationCase)
  {
    case BindReply.OperationOneofCase.Run:
      status = rep.Run ? 1 : 0;
      if (status is 0) cts.Cancel();
      break;
    case BindReply.OperationOneofCase.Auto:
      status = rep.Auto ? 2 : 1;
      break;
    default:
      break;
  }
}

await task;

Console.WriteLine("Press any key to exit...");
Console.ReadKey();