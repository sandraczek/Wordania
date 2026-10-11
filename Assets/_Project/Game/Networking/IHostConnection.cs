using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.SaveSystem.Data;

namespace Wordania.Networking
{
    /// <summary>Client: joins the host's session. The transport binds this connection to the local PersistentId.</summary>
    public interface IHostConnection
    {
        UniTask<JoinPayload> JoinAsync(CancellationToken cancellation);

        /// <summary>Start applying host events. Call after the join payload is applied, so nothing arrives earlier.</summary>
        void StartReceiving();
    }

    /// <summary>Placeholder until a transport exists: there is no host to join.</summary>
    public sealed class NullHostConnection : IHostConnection
    {
        public UniTask<JoinPayload> JoinAsync(CancellationToken cancellation)
        {
            Debug.LogError("[NullHostConnection] No network transport; joining an empty world.");
            return UniTask.FromResult(new JoinPayload { World = new GameSaveData() });
        }

        public void StartReceiving() { }
    }
}
