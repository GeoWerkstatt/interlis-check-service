using ILICheck.Web.XtfLog;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace ILICheck.Web
{
    /// <summary>
    /// Provides helper methods for the GeoJSON (RFC 7946) format.
    /// </summary>
    public static class GeoJsonHelper
    {
        /// <summary>
        /// Converts XTF log entries to a GeoJSON feature collection.
        /// </summary>
        /// <param name="logResult">The XTF log entries.</param>
        /// <returns>A feature collection containing the log entries.</returns>
        public static FeatureCollection CreateFeatureCollection(IEnumerable<LogError> logResult)
        {
            var features = logResult
                    .Where(log => log.Geometry?.Coord != null)
                    .Select(log => new Feature(new Point((double)log.Geometry.Coord.C1, (double)log.Geometry.Coord.C2), new AttributesTable(new KeyValuePair<string, object>[]
                    {
                        new ("type", log.Type),
                        new ("message", log.Message),
                        new ("objTag", log.ObjTag),
                        new ("dataSource", log.DataSource),
                        new ("line", log.Line),
                        new ("techDetails", log.TechDetails),
                    })));

            var featureCollection = new FeatureCollection();
            foreach (var feature in features)
            {
                featureCollection.Add(feature);
            }

            return featureCollection;
        }
    }
}
