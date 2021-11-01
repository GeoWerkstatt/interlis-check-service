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

        public SignalRHub(ILogger<SignalRHub> applicationLogger)
        {
            this.applicationLogger = applicationLogger;
        }

        public async Task SendConnectionId(string connectionId)
        {
            applicationLogger.LogInformation($"SignalR Hub received Connection Id: {connectionId}");
            await Clients.Client(connectionId).SendAsync("confirmConnection", "A connection with ID '" + connectionId + "' has been established.");
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            SignalREventHelper.InvokeDisconnectedEvent(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
