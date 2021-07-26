using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SignalR.Hubs;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ILICheck.Web.Controllers
{
    public class UploadController : Controller
    {
        private readonly IHubContext<SignalRHub> hubContext;
        public UploadController(IHubContext<SignalRHub> hubcontext)
        {
            this.hubContext = hubcontext;
        }

        /// <summary>
        /// Action to upload files to a directory.
        /// </summary>
        /// <returns>A <see cref="Task"/> of type <see cref="IActionResult"/>.</returns>
        [HttpPost]
        [Route("api/[controller]")]
        public async Task<IActionResult> UploadAsync()
        {
            var request = HttpContext.Request;
            var connectionId = request.Query["connectionId"][0];
            CancellationTokenSource cts = new ();
            Task<IActionResult> uploadTask = UploadToDirectory(request);
            await Task.WhenAny(uploadTask, SendPeriodicUploadFeedback(connectionId,  cts.Token));
            cts.Cancel();
            cts.Dispose();
            return await uploadTask;
        }

        private async Task<IActionResult> UploadToDirectory(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            if (!request.HasFormContentType ||
                !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) ||
                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }

            var reader = new MultipartReader(mediaTypeHeader.Boundary.Value, request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var contentDisposition);

                if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    var path = Directory.GetCurrentDirectory();

                    // TODO: read upload path from config
                    var uploadPath = $"{path}\\Upload\\";
                    var saveToPath = Path.Combine(uploadPath, contentDisposition.FileName.Value);

                    using (var targetStream = System.IO.File.Create(saveToPath))
                    {
                        await section.Body.CopyToAsync(targetStream);
                    }

                    return Ok();
                }

                section = await reader.ReadNextSectionAsync();
            }

            return BadRequest("No files data in the request.");
        }

        private async Task SendPeriodicUploadFeedback(string connectionId, CancellationToken cancellationToken)
        {
            while (true)
            {
                await hubContext.Clients.Client(connectionId).SendAsync("fileUploading", "File is uploading...", cancellationToken: cancellationToken);
                await Task.Delay(20000, cancellationToken);
            }
        }
    }
}
