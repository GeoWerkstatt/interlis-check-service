using DotSpatial.Projections;
using Geowerkstatt.Ilicop.Web.XtfLog;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace Geowerkstatt.Ilicop.Web
{
    /// <summary>
    /// Provides helper methods for the GeoJSON (RFC 7946) format.
    /// </summary>
    public static class GeoJsonHelper
    {
        private static readonly ProjectionInfo lv95 = ProjectionInfo.FromEpsgCode(2056);

        /// <summary>
        /// Converts XTF log entries to a GeoJSON feature collection.
        /// </summary>
        /// <param name="logResult">The XTF log entries.</param>
        /// <returns>A feature collection containing the log entries or <c>null</c> if the log entries contain either no coordinates or coordinates outside of the LV95 bounds.</returns>
        public static FeatureCollection CreateFeatureCollection(IEnumerable<LogError> logResult)
        {
            if (!AllCoordinatesAreLv95(logResult))
            {
                return null;
            }

            var features = logResult
                .Where(log => log.Geometry?.Coord != null)
                .Select(log => new Feature(ProjectLv95ToWgs84(log.Geometry.Coord), new AttributesTable(new KeyValuePair<string, object>[]
                {
                    new("type", log.Type),
                    new("message", log.Message),
                    new("objTag", log.ObjTag),
                    new("dataSource", log.DataSource),
                    new("line", log.Line),
                    new("techDetails", log.TechDetails),
                })));

            var featureCollection = new FeatureCollection();
            foreach (var feature in features)
            {
                featureCollection.Add(feature);
            }

            return featureCollection;
        }

        /// <summary>
        /// Checks that the log entries contain coordinates and all are in the LV95 bounds.
        /// </summary>
        /// <param name="logResult">The XTF log entries.</param>
        /// <returns><c>true</c> if the log entries contain coordinates and all are in the LV95 bounds; otherwise, <c>false</c>.</returns>
        private static bool AllCoordinatesAreLv95(IEnumerable<LogError> logResult)
        {
            var hasLv95Coordinates = false;
            foreach (var logEntry in logResult)
            {
                if (logEntry.Geometry?.Coord != null)
                {
                    hasLv95Coordinates = true;
                    if (!IsLv95Coordinate(logEntry.Geometry.Coord))
                    {
                        return false;
                    }
                }
            }

            return hasLv95Coordinates;
        }

        /// <summary>
        /// Checks if the coordinate is within the LV95 bounds.
        /// </summary>
        /// <param name="coord">The coordinate to check.</param>
        /// <returns><c>true</c> if the coordinate is in the LV95 bounds; otherwise, <c>false</c>.</returns>
        private static bool IsLv95Coordinate(Coord coord)
        {
            // Values are based on https://models.geo.admin.ch/CH/CHBase_Part1_GEOMETRY_V2.ili
            return coord.C1 >= 2_460_000 && coord.C1 <= 2_870_000
                && coord.C2 >= 1_045_000 && coord.C2 <= 1_310_000;
        }

        /// <summary>
        /// Projects the LV95 coordinate to WGS84.
        /// </summary>
        /// <param name="coord">The coordinate using LV95 CRS.</param>
        /// <returns>The coordinate projected to WGS84.</returns>
        private static Point ProjectLv95ToWgs84(Coord coord)
        {
            var source = lv95;
            var target = KnownCoordinateSystems.Geographic.World.WGS1984;
            double[] xy = { (double)coord.C1, (double)coord.C2 };
            double[] z = { 0 };
            Reproject.ReprojectPoints(xy, z, source, target, 0, 1);
            return new Point(xy[0], xy[1]);
        }
    }
}
