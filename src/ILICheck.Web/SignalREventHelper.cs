using System;
namespace ILICheck.Web
{
    public static class SignalREventHelper
    {
        public static event EventHandler<SignalrEventArgs> DisconnectedEvent;

        public static void InvokeDisconnectedEvent(string connectionId)
        {
            var args = new SignalrEventArgs
            {
                ConnectionId = connectionId,
            };
            DisconnectedEvent?.Invoke(null, args);
        }
    }

    public class SignalrEventArgs : EventArgs
    {
        public string ConnectionId { get; set; }
    }
}
