using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Session;

namespace Wordania.Events
{
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, object> _subscribers = new();
        private readonly bool _isHost;
        private readonly IEventTransport _transport;

        public EventBus(SessionConfig session, IEventTransport transport)
        {
            _isHost = session.IsHost;
            _transport = transport;
        }

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

        public void PublishSimulation<T>(T gameEvent) where T : struct, ISimulationEvent
        {
            if (!_isHost)
            {
                Debug.LogError($"[EventBus] Simulation event {typeof(T).Name} published on a client - dropped.");
                return;
            }
            Dispatch(gameEvent);
        }

        public void PublishReplicated<T>(T gameEvent) where T : struct, IReplicatedEvent
        {
            if (!_isHost)
            {
                Debug.LogError($"[EventBus] Replicated event {typeof(T).Name} published on a client - dropped.");
                return;
            }
            Dispatch(gameEvent);
            _transport.Broadcast(gameEvent);
        }

        public void PublishLocal<T>(T gameEvent) where T : struct, ILocalEvent
        {
            Dispatch(gameEvent);
        }

        public void Receive<T>(T gameEvent) where T : struct, IReplicatedEvent
        {
            if (_isHost)
            {
                Debug.LogError($"[EventBus] Host received replicated event {typeof(T).Name} from the network - dropped.");
                return;
            }
            Dispatch(gameEvent);
        }

        private void Dispatch<T>(T gameEvent) where T : struct, IGameEvent
        {
            if (_subscribers.TryGetValue(typeof(T), out var existingHandlers))
            {
                ((Action<T>)existingHandlers)?.Invoke(gameEvent);
            }
        }
    }
}