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

        public async Task StartValidation(string connectionId, string uploadedFileName)
        {
            sessionLogger = GetLogger(uploadedFileName);
            applicationLogger.LogInformation($"Validation started for file: {uploadedFileName}");
            sessionLogger.Information($"Validation started for file: {uploadedFileName}");
            await Clients.Client(connectionId).SendAsync("validationStarted", $"Validation started for file {uploadedFileName}");

            // Simulate validation process
            await Task.Delay(TimeSpan.FromSeconds(2));
            await Clients.Client(connectionId).SendAsync("firstValidationPass", "First validation passed");
            sessionLogger.Information($"First validation passed");
            await Task.Delay(TimeSpan.FromSeconds(5));
            await Clients.Client(connectionId).SendAsync("secondValidationPass", "Second validation passed");
            sessionLogger.Information($"Second validation passed");
            await Task.Delay(TimeSpan.FromSeconds(3));
            await Clients.Client(connectionId).SendAsync("validationDone", "Validation done");
            sessionLogger.Information($"Validation done");
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
