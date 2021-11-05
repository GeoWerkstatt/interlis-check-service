using ILICheck.Web;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SignalR.Hubs
{
    public class SignalRHub : Hub
    {
        private readonly ILogger<SignalRHub> applicationLogger;
        private readonly SignalRConnectionHelper signalRConnectionHelper;

        public SignalRHub(ILogger<SignalRHub> applicationLogger, SignalRConnectionHelper signalRConnectionHelper)
        {
            this.applicationLogger = applicationLogger;
            this.signalRConnectionHelper = signalRConnectionHelper;
        }

        public async Task SendConnectionId(string connectionId)
        {
            applicationLogger.LogInformation($"SignalR Hub received Connection Id: {connectionId}");
            await Clients.Client(connectionId).SendAsync("confirmConnection", "A connection with ID '" + connectionId + "' has been established.");
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            signalRConnectionHelper.OnDisconnected(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
