using System.Threading;
using Cysharp.Threading.Tasks;

namespace Wordania.HUD.Journal
{
    public interface IJournalView
    {
        UniTask InitializeAsync(CancellationToken cancellation);
    }
}