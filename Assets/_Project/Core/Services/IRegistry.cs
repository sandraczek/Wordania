using Wordania.Core.Identifiers;

namespace Wordania.Core.Services
{
    public interface IRegistry<T> where T:IEntity
    {
        void Register(T entity);
        void Unregister(int entityId);
    }
}