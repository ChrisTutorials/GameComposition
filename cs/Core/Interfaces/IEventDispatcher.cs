using System;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Central event dispatcher for cross-domain and cross-plugin communication.
    /// Provides engine-agnostic event management for the framework.
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Publishes an event to all subscribers.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="eventData">The event data to publish.</param>
        void Publish<TEvent>(TEvent eventData);

        /// <summary>
        /// Subscribes a handler to events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="handler">The handler to invoke when the event is published.</param>
        void Subscribe<TEvent>(Action<TEvent> handler);

        /// <summary>
        /// Unsubscribes a handler from events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="handler">The handler to remove.</param>
        void Unsubscribe<TEvent>(Action<TEvent> handler);

        /// <summary>
        /// Removes all subscribers for all event types.
        /// </summary>
        void ClearAllSubscriptions();
    }
}
