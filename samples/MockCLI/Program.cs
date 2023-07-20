var status = 0;
var lat = 43.0735;
var lon = 141.336;
var heading = 0.0;
var velocity = 0.0;

var cts = new CancellationTokenSource();

var task = Task.Run(() =>
{
  while (!cts.IsCancellationRequested)
  {
    Thread.Sleep(100);
    lat += (new Random().NextDouble() - 0.5) * 0.00001;
    lon += (new Random().NextDouble() - 0.5) * 0.00001;
    if (status is 0)
      Console.WriteLine($"quality:0,status:{status}");
    else
      Console.WriteLine($"latitude:{lat:f6},longitude:{lon:f6},heading:{heading:f2},velocity:{velocity:f2},quality:4,status:{status}");
  }
});

while (!cts.IsCancellationRequested)
{
  var command = Console.ReadLine().Split(":");
  var key = command[0];
  var value = command[1];
  switch (key)
  {
    case "run":
      status = bool.TryParse(value, out var r) && r ? 1 : 0;
      if (status is 0) cts.Cancel();
      break;
    case "auto":
      status = bool.TryParse(value, out var a) && a ? 2 : 1;
      break;
    default:
      break;
  }
}

await task;
Console.WriteLine($"status:{status}");
