using System;
using System.Collections.Generic;

namespace Wordania.Core.Events
{
    public abstract class EventBus : IEventBus
    {
        private readonly Dictionary<Type, object> _subscribers = new();

        public void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            Type eventType = typeof(T);
            if (_subscribers.TryGetValue(eventType, out var existingHandlers))
            {
                _subscribers[eventType] = (Action<T>)existingHandlers + handler;
            }
            else
            {
                _subscribers[eventType] = handler;
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            Type eventType = typeof(T);
            if (_subscribers.TryGetValue(eventType, out var existingHandlers))
            {
                var currentHandlers = (Action<T>)existingHandlers;
                currentHandlers -= handler;

                if (currentHandlers == null)
                {
                    _subscribers.Remove(eventType);
                }
                else
                {
                    _subscribers[eventType] = currentHandlers;
                }
            }
        }

        public void Publish<T>(T gameEvent) where T : struct, IGameEvent
        {
            Type eventType = typeof(T);
            if (_subscribers.TryGetValue(eventType, out var existingHandlers))
            {
                ((Action<T>)existingHandlers)?.Invoke(gameEvent);
            }
        }
    }
    public sealed class ProjectEventBus : EventBus, IEventBusProject { }
}