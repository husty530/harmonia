using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Harmonia.Core;
using Harmonia.RCL;

var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHarmoniaCore("harmonia.json");
builder.Services.AddSingleton(config.GetSection("MapFiles").Get<string[]>()!);
builder.Services.AddSingleton(Observable.Interval(TimeSpan.FromMilliseconds(333), new EventLoopScheduler()));
builder.Services.AddScoped<ArcGisJsInterop>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error");
  app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();