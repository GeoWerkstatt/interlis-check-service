using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;

namespace SignalR.Hubs
{
    public class SignalRHub : Hub
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<SignalRHub> applicationLogger;
        private Serilog.ILogger sessionLogger;

        public SignalRHub(ILogger<SignalRHub> applicationLogger, IConfiguration configuration)
        {
            this.applicationLogger = applicationLogger;
            this.configuration = configuration;
        }

        public async Task SendConnectionId(string connectionId)
        {
            applicationLogger.LogInformation($"SignalR Hub received Connection Id: {connectionId}");
            await Clients.Client(connectionId).SendAsync("confirmConnection", "A connection with ID '" + connectionId + "' has been established.");
        }

        public async Task StartUpload(string connectionId, string uploadedFileName)
        {
            sessionLogger = GetLogger(uploadedFileName);
            applicationLogger.LogInformation($"Start uploading: {uploadedFileName}");
            sessionLogger.Information($"Start uploading: {uploadedFileName}");
            await Clients.Client(connectionId).SendAsync("uploadStarted", $"Upload started for file {uploadedFileName}");
        }

        private Serilog.ILogger GetLogger(string uploadedFileName)
        {
            var sessionPathFormat = configuration.GetSection("Logging")["PathFormatSession"];
            var timestamp = DateTime.Now.ToString("yyyy_MM_d_HHmmss");
            var sessionId = $"{timestamp}_{uploadedFileName}";
            var logFileName = sessionPathFormat.Replace("{SessionId}", sessionId);

            return new LoggerConfiguration()
                .WriteTo.File(logFileName)
                .CreateLogger();
        }
    }
}
