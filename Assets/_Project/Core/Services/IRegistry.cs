using System.Collections.Generic;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Services
{
    public interface IRegistry<T> where T : IEntity
    {
        int Count { get; }
        void Register(T entity);
        void Unregister(InstanceId entityId);
        IReadOnlyList<T> GetAll();
        public bool TryGet(InstanceId entityId, out T entity);
    }
}