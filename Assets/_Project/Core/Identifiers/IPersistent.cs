using Wordania.Core.Identifiers;

namespace Wordania.Features.Identifiers
{
    public interface IPersistent
    {
        PersistentId PersistentId { get; }
    }
}