using System;

namespace Wordania.Core.Events
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent;
        void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent;
        void Publish<T>(T gameEvent) where T : struct, IGameEvent;
    }
    public interface IEventBusProject : IEventBus { }
}