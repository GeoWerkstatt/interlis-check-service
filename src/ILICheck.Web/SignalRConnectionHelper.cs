using System;

namespace ILICheck.Web
{
    /// <summary>
    /// Helper class to let anyone know if a specific SignalR connection has been disconnected.
    /// </summary>
    public static class SignalRConnectionHelper
    {
        /// <summary>
        /// Occurs when a SignalR connection with the specified connection id has been disconnected.
        /// </summary>
        public static event EventHandler<SignalRDisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Called if a SignalR connection with the specified <paramref name="connectionId"/> has been disconnected.
        /// </summary>
        public static void OnDisconnected(string connectionId)
            => Disconnected?.Invoke(null, new SignalRDisconnectedEventArgs { ConnectionId = connectionId });
    }
}
