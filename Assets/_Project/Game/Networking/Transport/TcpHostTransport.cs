using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Networking.Transport
{
    /// <summary>
    /// Host side of the TCP transport: listens for clients and broadcasts replicated events to joined ones.
    /// Message handling lives in <see cref="HostNetworkService"/>; this class only depends on launch options so the
    /// event bus can depend on it without a DI cycle.
    /// </summary>
    public sealed class TcpHostTransport : IEventTransport, IDisposable
    {
        public const int MaxClientFrame = 64 * 1024;

        public sealed class Peer
        {
            public readonly TcpConnection Connection;
            public PersistentId Player; // Empty until Hello is accepted
            public bool Joined;
            public bool PendingHello;
            public float ConnectedAt;

            public Peer(TcpConnection connection) => Connection = connection;
        }

        private readonly TcpListener _listener;
        private readonly object _acceptedLock = new();
        private readonly List<TcpClient> _accepted = new();
        private readonly List<Peer> _peers = new();
        private volatile bool _disposed;

        public IReadOnlyList<Peer> Peers => _peers;

        public TcpHostTransport(NetworkLaunchOptions options)
        {
            _listener = new TcpListener(IPAddress.Any, options.Port);
            _listener.Start();
            new Thread(AcceptLoop) { IsBackground = true, Name = "Wordania.Net.Accept" }.Start();
            Debug.Log($"[TcpHostTransport] Listening on port {options.Port}.");
        }

        public void Broadcast<T>(T gameEvent) where T : struct, IReplicatedEvent
        {
            if (_peers.Count == 0) return;

            byte[] frame = NetCodec.BuildEvent(gameEvent); // encode once for all peers
            if (frame == null) return;

            foreach (Peer peer in _peers)
                if (peer.Joined) peer.Connection.Send(frame);
        }

        /// <summary>Main thread: moves sockets accepted on the background thread into the peer list.</summary>
        public void CollectNewPeers(float now)
        {
            lock (_acceptedLock)
            {
                foreach (TcpClient client in _accepted)
                {
                    var peer = new Peer(new TcpConnection(client, MaxClientFrame)) { ConnectedAt = now };
                    _peers.Add(peer);
                    Debug.Log($"[TcpHostTransport] Connection from {peer.Connection.RemoteEndPoint}.");
                }
                _accepted.Clear();
            }
        }

        public void RemovePeer(Peer peer)
        {
            peer.Connection.Close();
            _peers.Remove(peer);
        }

        public void Dispose()
        {
            _disposed = true;
            _listener.Stop();
            foreach (Peer peer in _peers) peer.Connection.Close();
            _peers.Clear();
            lock (_acceptedLock)
            {
                foreach (TcpClient client in _accepted) client.Close();
                _accepted.Clear();
            }
        }

        private void AcceptLoop()
        {
            while (!_disposed)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    lock (_acceptedLock) _accepted.Add(client);
                }
                catch (Exception e) when (e is SocketException or ObjectDisposedException or InvalidOperationException)
                {
                    if (!_disposed) Debug.LogWarning($"[TcpHostTransport] Accept failed: {e.Message}");
                    return;
                }
            }
        }
    }
}
