using System;
namespace ILICheck.Web
{
    /// <summary>
    /// Provides data for the SignalR disconnected event.
    /// </summary>
    public class SignalRDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the SignalR connection id.
        /// </summary>
        public string ConnectionId { get; set; }
    }
}
