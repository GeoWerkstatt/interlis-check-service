using ILICheck.Web.XtfLog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ILICheck.Web.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DownloadController : Controller
    {
        private readonly ILogger<DownloadController> logger;
        private readonly IFileProvider fileProvider;

        public DownloadController(ILogger<DownloadController> logger, IFileProvider fileProvider)
        {
            this.logger = logger;
            this.fileProvider = fileProvider;
        }

        /// <summary>
        /// Gets the ilivalidator log file for the specified <paramref name="jobId"/> and <paramref name="logType"/>.
        /// </summary>
        /// <param name="jobId" example="2e71ae96-e6ad-4b67-b817-f09412d09a2c">The job identifier.</param>
        /// <param name="logType">The log type to download.</param>
        /// <returns>The ilivalidator log file.</returns>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status201Created, "Returns the ilivalidator log file.", ContentTypes = new[] { "text/xml; charset=utf-8" })]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The server cannot process the request due to invalid or malformed request.", typeof(ProblemDetails), new[] { "application/json" })]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The log file for the requested jobId cannot be found.", ContentTypes = new[] { "application/json" })]
        public IActionResult Download(Guid jobId, LogType logType)
        {
            fileProvider.Initialize(jobId);

            try
            {
                logger.LogInformation("Log file (<{LogType}>) for job identifier <{JobId}> requested.", HttpUtility.HtmlEncode(logType), jobId);
                return File(fileProvider.OpenText(fileProvider.GetLogFile(logType)).BaseStream, "text/xml; charset=utf-8");
            }
            catch (Exception)
            {
                Response.StatusCode = 404;
                return View("PageNotFound", "Die gesuchte Log-Datei wurde nicht gefunden. Möglicherweise wurde sie bereits gelöscht.");
            }
        }

        /// <summary>
        /// Gets the log data of the specified <paramref name="jobId"/> in JSON format.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>The log data for the specified <paramref name="jobId"/>.</returns>
        [HttpGet("json")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the ilivalidator log data in JSON format.", typeof(IEnumerable<LogError>), new[] { "application/json" })]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The server cannot process the request due to invalid or malformed request.", typeof(ValidationProblemDetails), new[] { "application/json" })]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The log file for the requested jobId cannot be found.", ContentTypes = new[] { "application/json" })]
        public IActionResult GetJsonLog(Guid jobId)
        {
            logger.LogTrace("JSON log for job <{JobId}> requested.", jobId);

            fileProvider.Initialize(jobId);

            try
            {
                var xtfLogFile = fileProvider.GetLogFile(LogType.Xtf);
                using var reader = fileProvider.OpenText(xtfLogFile);

                var result = XtfLogParser.Parse(reader);
                return Ok(result);
            }
            catch (FileNotFoundException)
            {
                return Problem($"No xtf log available for job id <{jobId}>", statusCode: StatusCodes.Status404NotFound);
            }
        }

        /// <summary>
        /// Gets the geographic log data of the specified <paramref name="jobId"/> in GeoJSON (RFC 7946) format.
        /// </summary>
        /// <remarks>
        /// The Coordinate Reference System of the uploaded data is used and log entries without coordinates are ignored.
        /// </remarks>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>A FeatureCollection for the log data of the specified <paramref name="jobId"/>.</returns>
        [HttpGet("geojson")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the geographic ilivalidator log data in GeoJSON format.", ContentTypes = new[] { "application/geo+json" })]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The server cannot process the request due to invalid or malformed request.", typeof(ValidationProblemDetails), new[] { "application/json" })]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The log file for the requested jobId cannot be found.", ContentTypes = new[] { "application/json" })]
        public IActionResult GetGeoJson(Guid jobId)
        {
            logger.LogTrace("GeoJSON log for job <{JobId}> requested.", jobId);

            fileProvider.Initialize(jobId);

            try
            {
                var xtfLogFile = fileProvider.GetLogFile(LogType.Xtf);
                using var reader = fileProvider.OpenText(xtfLogFile);
                var logResult = XtfLogParser.Parse(reader);

                var featureCollection = CreateFeatureCollection(logResult);

                Response.Headers.ContentType = "application/geo+json";
                return Ok(featureCollection);
            }
            catch (FileNotFoundException)
            {
                return Problem($"No xtf log available for job id <{jobId}>", statusCode: StatusCodes.Status404NotFound);
            }
        }

        internal static FeatureCollection CreateFeatureCollection(IEnumerable<LogError> logResult)
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
