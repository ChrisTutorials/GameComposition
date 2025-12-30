using System;

namespace BarkMoon.GameComposition.Core.Events
{
    /// <summary>
    /// Base class for all service-related events in the BarkMoon ecosystem.
    /// Provides standard metadata like timestamp and source.
    /// </summary>
    public abstract class ServiceEvent : EventArgs
    {
        /// <summary>
        /// Timestamp when the event occurred (UTC).
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Source of the event.
        /// </summary>
        public object Source { get; }

        /// <summary>
        /// Optional associated event data.
        /// </summary>
        public object? Data { get; }

        protected ServiceEvent(object source, object? data = null)
        {
            Timestamp = DateTime.UtcNow;
            Source = source;
            Data = data;
        }
    }

    /// <summary>
    /// Event raised when a service lifecycle status changes.
    /// </summary>
    public class ServiceStatusEvent : ServiceEvent
    {
        public ServiceStatus Status { get; }

        public ServiceStatusEvent(object source, ServiceStatus status, object? data = null) 
            : base(source, data)
        {
            Status = status;
        }
    }

    public enum ServiceStatus
    {
        Starting,
        Started,
        Stopping,
        Stopped,
        Error
    }
}
