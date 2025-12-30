using System;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Interface for event dispatching and subscription.
    /// Provides engine-agnostic event management for the framework.
    /// </summary>
    public interface IEventDispatcher
    {
        void Publish<TEvent>(TEvent eventData);
        void Subscribe<TEvent>(Action<TEvent> handler);
        void Unsubscribe<TEvent>(Action<TEvent> handler);
        void ClearAllSubscriptions();
    }

    /// <summary>
    /// Central event bus for cross-plugin communication.
    /// Integrated into the BarkMoon.GameComposition foundation.
    /// </summary>
    public interface IEventBus : IEventDispatcher
    {
    }
}
