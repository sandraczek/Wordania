using System.Threading;
using Cysharp.Threading.Tasks;

namespace Wordania.Features.HUD.Journal
{
    public interface IJournalView
    {
        UniTask InitializeAsync(CancellationToken cancellation);
    }
}