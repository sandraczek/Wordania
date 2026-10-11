using System;
using System.IO;
using UnityEngine;
using VContainer.Unity;
using Wordania.Identifiers;
using Wordania.Session;

namespace Wordania.Networking.Transport
{
    /// <summary>
    /// Host: handles client messages on the main thread. A connection is bound to one PersistentId at Hello time,
    /// and everything it sends afterwards is attributed to that id only.
    /// </summary>
    public sealed class HostNetworkService : ITickable, IDisposable
    {
        private const int MaxClients = 3;
        private const float HelloTimeout = 10f;
        private const int MaxFramesPerPeerPerTick = 256;

        private readonly TcpHostTransport _transport;
        private readonly IHostSession _session;
        private readonly IHostInbox _inbox;
        private readonly SessionConfig _config;

        public HostNetworkService(TcpHostTransport transport, IHostSession session, IHostInbox inbox, SessionConfig config)
        {
            _transport = transport;
            _session = session;
            _inbox = inbox;
            _config = config;
        }

        public void Tick()
        {
            float now = Time.realtimeSinceStartup;
            _transport.CollectNewPeers(now);

            var peers = _transport.Peers;
            for (int i = peers.Count - 1; i >= 0; i--)
            {
                TcpHostTransport.Peer peer = peers[i];
                if (!Pump(peer, now) || peer.Connection.IsClosed)
                    Disconnect(peer, "connection closed");
            }
        }

        public void Dispose()
        {
            var peers = _transport.Peers;
            for (int i = peers.Count - 1; i >= 0; i--)
                Disconnect(peers[i], "host shutting down");
        }

        /// <returns>false if the peer must be dropped.</returns>
        private bool Pump(TcpHostTransport.Peer peer, float now)
        {
            if (!peer.Joined && !peer.PendingHello && now - peer.ConnectedAt > HelloTimeout)
                return false;

            // A Hello that arrived before the host was ready waits here, other frames stay queued behind it.
            if (peer.PendingHello)
            {
                if (!_session.IsOpen) return true;
                peer.PendingHello = false;
                if (!Accept(peer)) return false;
            }

            for (int n = 0; n < MaxFramesPerPeerPerTick && peer.Connection.TryReceive(out byte[] frame); n++)
            {
                try
                {
                    using BinaryReader reader = NetCodec.Open(frame, out NetMessageType type);
                    switch (type)
                    {
                        case NetMessageType.Hello when !peer.Joined && peer.Player.IsEmpty:
                            if (!ReadHello(peer, reader)) return false;
                            if (!_session.IsOpen)
                            {
                                peer.PendingHello = true;
                                return true;
                            }
                            if (!Accept(peer)) return false;
                            break;

                        case NetMessageType.Input when peer.Joined:
                            _inbox.ReceiveInput(peer.Player, NetCodec.ReadInput(reader));
                            break;

                        case NetMessageType.Command when peer.Joined:
                            _inbox.ReceiveCommand(peer.Player, NetCodec.ReadCommand(reader));
                            break;

                        default:
                            Debug.LogWarning($"[HostNetworkService] Unexpected {type} from {peer.Connection.RemoteEndPoint}.");
                            return false;
                    }
                }
                catch (Exception e) when (e is IOException or InvalidDataException or ArgumentException)
                {
                    Debug.LogWarning($"[HostNetworkService] Malformed message from {peer.Connection.RemoteEndPoint}: {e.Message}");
                    return false;
                }
            }
            return true;
        }

        private bool ReadHello(TcpHostTransport.Peer peer, BinaryReader reader)
        {
            int version = reader.ReadInt32();
            PersistentId id = NetCodec.ReadPersistentId(reader);

            string reason = null;
            if (version != NetCodec.ProtocolVersion) reason = $"Protocol mismatch (host {NetCodec.ProtocolVersion}, client {version}).";
            else if (id.IsEmpty || id == _config.LocalPersistentId) reason = "Invalid player id (same as host? start the client with -newid).";
            else if (JoinedCount() >= MaxClients) reason = "Session is full.";
            else if (IsTaken(id)) reason = "This player is already connected.";

            if (reason != null)
            {
                peer.Connection.Send(NetCodec.Build(NetMessageType.Reject, w => w.Write(reason)));
                Debug.Log($"[HostNetworkService] Rejected {peer.Connection.RemoteEndPoint}: {reason}");
                return false;
            }

            peer.Player = id;
            return true;
        }

        private bool Accept(TcpHostTransport.Peer peer)
        {
            JoinPayload payload = _session.AcceptClient(peer.Player);
            peer.Connection.Send(NetCodec.Build(NetMessageType.Welcome, w => NetCodec.WriteJoinPayload(w, payload)));
            // Joined only now: everything broadcast from here on comes after the Welcome on the same ordered stream.
            peer.Joined = true;
            Debug.Log($"[HostNetworkService] Player {peer.Player} joined from {peer.Connection.RemoteEndPoint}.");
            return true;
        }

        private void Disconnect(TcpHostTransport.Peer peer, string reason)
        {
            bool wasJoined = peer.Joined;
            peer.Joined = false;
            _transport.RemovePeer(peer);
            if (!wasJoined) return;

            Debug.Log($"[HostNetworkService] Player {peer.Player} left ({reason}).");
            _session.RemoveClient(peer.Player);
        }

        private int JoinedCount()
        {
            int count = 0;
            foreach (var p in _transport.Peers) if (p.Joined || p.PendingHello) count++;
            return count;
        }

        private bool IsTaken(PersistentId id)
        {
            foreach (var p in _transport.Peers)
                if ((p.Joined || p.PendingHello) && p.Player == id) return true;
            return false;
        }
    }
}
