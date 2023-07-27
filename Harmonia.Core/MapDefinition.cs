using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;

namespace Harmonia.Core;

public record class WgsPoint(string Id, double Latitude, double Longitude)
{
  public UtmPoint ToUtm()
  {
    var zo = (int)((Longitude + 180) / 6);
    var factory = new CoordinateTransformationFactory();
    var wgs84geo = GeographicCoordinateSystem.WGS84;
    var isNorthZone = Latitude > 0;
    var utm = ProjectedCoordinateSystem.WGS84_UTM(zo, isNorthZone);
    var transformation = factory.CreateFromCoordinateSystems(wgs84geo, utm);
    var utmAxes = transformation.MathTransform.Transform(new[] { Longitude, Latitude });
    var easting = utmAxes[0];
    var northing = utmAxes[1];
    return new(Id, easting, northing);
  }
}

public record class WgsPath(string Id, List<WgsPoint> Points)
{
  public UtmPath ToUtm()
  {
    var zo = (int)((Points[0].Longitude + 180) / 6);
    var factory = new CoordinateTransformationFactory();
    var wgs84geo = GeographicCoordinateSystem.WGS84;
    var isNorthZone = Points[0].Latitude > 0;
    var utm = ProjectedCoordinateSystem.WGS84_UTM(zo, isNorthZone);
    var transformation = factory.CreateFromCoordinateSystems(wgs84geo, utm);
    var utmAxes = transformation.MathTransform.TransformList(Points.Select(p => new[] { p.Longitude, p.Latitude }).ToList());
    var output = new List<UtmPoint>();
    for (int i = 0; i < utmAxes.Count; i++)
      output.Add(new(Points[i].Id, utmAxes[i][0], utmAxes[i][1]));
    return new(Id, output);
  }
  public WgsPath Prune(double intervalMeter = 1)
  {
    var zo = (int)((Points[0].Longitude + 180) / 6);
    return ToUtm().Prune(intervalMeter).ToWgs(zo);
  }
}

public record class WgsMap(string Name, List<WgsPath> Paths)
{
  public UtmMap ToUtm()
  {
    var zo = (int)((Paths[0].Points[0].Longitude + 180) / 6);
    var factory = new CoordinateTransformationFactory();
    var wgs84geo = GeographicCoordinateSystem.WGS84;
    var isNorthZone = Paths[0].Points[0].Latitude > 0;
    var utm = ProjectedCoordinateSystem.WGS84_UTM(zo, isNorthZone);
    var transformation = factory.CreateFromCoordinateSystems(wgs84geo, utm);
    var paths = new List<UtmPath>();
    foreach (var path in Paths)
    {
      var utmAxes = transformation.MathTransform.TransformList(path.Points.Select(p => new[] { p.Longitude, p.Latitude }).ToList());
      var output = new List<UtmPoint>();
      for (int i = 0; i < utmAxes.Count; i++)
        output.Add(new(path.Points[i].Id, utmAxes[i][0], utmAxes[i][1]));
      paths.Add(new(path.Id, output));
    }
    return new(Name, paths);
  }

  public WgsMap Prune(double intervalMeter = 1)
  {
    var zo = (int)((Paths[0].Points[0].Longitude + 180) / 6);
    return ToUtm().Prune(intervalMeter).ToWgs(zo);
  }
}

public record class UtmPoint(string Id, double X, double Y)
{
  public WgsPoint ToWgs(int zone)
  {
    var factory = new CoordinateTransformationFactory();
    var wgs84geo = GeographicCoordinateSystem.WGS84;
    var utm = ProjectedCoordinateSystem.WGS84_UTM(zone, true);
    var transformation = factory.CreateFromCoordinateSystems(wgs84geo, utm);
    var inversedTransform = transformation.MathTransform.Inverse();
    var points = inversedTransform.Transform(new[] { X, Y });
    return new(Id, points[1], points[0]);
  }
}

public record class UtmPath(string Id, List<UtmPoint> Points)
{
  public WgsPath ToWgs(int zone)
  {
    var factory = new CoordinateTransformationFactory();
    var wgs84geo = GeographicCoordinateSystem.WGS84;
    var utm = ProjectedCoordinateSystem.WGS84_UTM(zone, true);
    var transformation = factory.CreateFromCoordinateSystems(wgs84geo, utm);
    var inversedTransform = transformation.MathTransform.Inverse();
    var points = inversedTransform.TransformList(Points.Select(p => new[] { p.X, p.Y }).ToList());
    var output = new List<WgsPoint>();
    for (int i = 0; i < points.Count; i++)
      output.Add(new(Points[i].Id, points[i][1], points[i][0]));
    return new(Id, output);
  }

  public UtmPath Prune(double intervalMeter = 1)
  {
    var pts = new List<UtmPoint> { Points.First() };
    foreach (var p in Points)
    {
      if (Math.Sqrt(Math.Pow(pts.Last().X - p.X, 2) + Math.Pow(pts.Last().Y - p.Y, 2)) > intervalMeter)
        pts.Add(p);
    }
    return new(Id, pts);
  }
}

public record class UtmMap(string Name, List<UtmPath> Paths)
{
  public WgsMap ToWgs(int zone)
  {
    var factory = new CoordinateTransformationFactory();
    var wgs84geo = GeographicCoordinateSystem.WGS84;
    var utm = ProjectedCoordinateSystem.WGS84_UTM(zone, true);
    var transformation = factory.CreateFromCoordinateSystems(wgs84geo, utm);
    var inversedTransform = transformation.MathTransform.Inverse();
    var paths = new List<WgsPath>();
    foreach (var path in Paths)
    {
      var points = inversedTransform.TransformList(path.Points.Select(p => new[] { p.X, p.Y }).ToList());
      var output = new List<WgsPoint>();
      for (int i = 0; i < points.Count; i++)
        output.Add(new(path.Points[i].Id, points[i][1], points[i][0]));
      paths.Add(new(path.Id, output));
    }
    return new(Name, paths);
  }

  public UtmMap Prune(double intervalMeter = 1)
  {
    return new(Name, Paths.Select(path => path.Prune(intervalMeter)).ToList());
  }
}
