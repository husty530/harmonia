using static System.Math;
using System.Reactive.Subjects;
using Microsoft.JSInterop;
using Harmonia.Core;

namespace Harmonia.RCL;

public sealed class ArcGisJsInterop : IAsyncDisposable
{

  private readonly IJSRuntime _js;
  private IJSObjectReference? _module;

  public static Subject<string> SelectNotifier { set; get; } = new();

  public ArcGisJsInterop(IJSRuntime js)
  {
    _js = js;
  }

  public async ValueTask DisposeAsync()
  {
    if (_module is null) return;
    await _module.DisposeAsync().ConfigureAwait(false);
  }

  public async ValueTask InitializeAsync()
  {
    _module = await _js.InvokeAsync<IJSObjectReference>("import", "./_content/Harmonia.RCL/arcgis.js").ConfigureAwait(false);
  }

  public async ValueTask PutMarkerAsync(string id, WgsPoint point, double heading, List<int> color)
  {
    heading = heading * PI / 180;
    var r = 30;
    var ptx = r * Cos(PI / 2 - heading) * 0.8;
    var pty = r * Sin(PI / 2 - heading) * 0.8;
    var pbx = r * Cos(-PI / 2 - heading) / 2;
    var pby = r * Sin(-PI / 2 - heading) / 2;
    var plx = r * Cos(-PI * 2 / 3 - heading);
    var ply = r * Sin(-PI * 2 / 3 - heading);
    var prx = r * Cos(-PI / 3 - heading);
    var pry = r * Sin(-PI / 3 - heading);
    var ring = new double[] { ptx, pty, plx, ply, pbx, pby, prx, pry };
    try
    {
      if (_module is null) return;
      await _module.InvokeVoidAsync("drawMarker", id, point.Latitude, point.Longitude, ring, color).ConfigureAwait(false);
    }
    catch { }
  }

  public async ValueTask AddPointAsync(string id, WgsPoint point, List<int> color)
  {
    if (_module is null) return;
    await _module.InvokeVoidAsync("drawPoint", id, point.Latitude, point.Longitude, color).ConfigureAwait(false);
  }

  public async ValueTask RemovePointAsync(string id)
  {
    if (_module is null) return;
    await _module.InvokeVoidAsync("removePoint", id).ConfigureAwait(false);
  }

  public async ValueTask RegisterMapAsync(string id, WgsMap map, List<int> color)
  {
    if (_module is null) return;
    await _module.InvokeVoidAsync("registerMap", id, map, color).ConfigureAwait(false);
  }

  public async Task SetPathColorAsync(string id, int index, List<int> color)
  {
    if (_module is null) return;
    await _module.InvokeVoidAsync("setPathColor", id, index, color).ConfigureAwait(false);
  }

  public async Task ShowMapAsync(string id)
  {
    if (_module is null) return;
    await _module.InvokeVoidAsync("showMap", id).ConfigureAwait(false);
  }

  public async Task HideMapAsync(string id)
  {
    if (_module is null) return;
    await _module.InvokeVoidAsync("hideMap", id).ConfigureAwait(false);
  }

  public async Task AddTrajectoryAsync(string id, WgsPoint point)
  {
    if (_module is null) return;
    await _module.InvokeVoidAsync("addTrajectory", id, point.Latitude, point.Longitude).ConfigureAwait(false);
  }

  public async Task RemoveTrajectoryAsync(string id)
  {
    if (_module is null) return;
    await _module.InvokeVoidAsync("removeTrajectory", id).ConfigureAwait(false);
  }

  public async Task RemoveAllAsync(string id)
  {
    if (_module is null) return;
    await _module.InvokeVoidAsync("removeAll", id).ConfigureAwait(false);
  }

  public async Task FocusSelectedObjectAsync(string id)
  {
    if (_module is null) return;
    await _module.InvokeVoidAsync("selectMarker", id).ConfigureAwait(false);
  }

  [JSInvokable]
  public static void SelectClickedObject(string id)
  {
    SelectNotifier.OnNext(id);
  }

}

public struct Color
{
  public static List<int> Red { get; } = new() { 200, 0, 0, 255 };
  public static List<int> Green { get; } = new() { 0, 200, 0, 255 };
  public static List<int> Blue { get; } = new() { 0, 0, 200, 255 };
  public static List<int> Black { get; } = new() { 0, 0, 0, 255 };
  public static List<int> White { get; } = new() { 255, 255, 255, 255 };
}
