namespace Harmonia.Core;

public record class WgsPoint(string Id, double Latitude, double Longitude);

public record class WgsPath(string Id, List<WgsPoint> Points);

public record class WgsMap(string Name, List<WgsPath> Paths);

public record class UtmPoint(string Id, double X, double Y);

public record class UtmPath(string Id, List<UtmPoint> Points);

public record class UtmMap(string Name, List<UtmPath> Paths);
