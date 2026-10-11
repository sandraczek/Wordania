using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;
using Wordania.Events;
using Wordania.Session;

namespace Wordania.Networking.Transport
{
    /// <summary>
    /// Client side of the TCP transport: joins the host, sends input/commands, and feeds host events into the bus.
    /// Events that arrive while the client is still loading stay queued until <see cref="StartReceiving"/>.
    /// </summary>
    public sealed class TcpClientConnection : IHostConnection, IClientUplink, ITickable, IDisposable
    {
        private const int MaxHostFrame = 64 * 1024 * 1024;
        private const int ConnectTimeoutMs = 10000;
        private const float WelcomeTimeout = 30f;

        private readonly NetworkLaunchOptions _options;
        private readonly SessionConfig _session;
        private readonly IEventBus _bus;

        private TcpConnection _connection;
        private bool _receiving;
        private bool _reportedDisconnect;

        public TcpClientConnection(NetworkLaunchOptions options, SessionConfig session, IEventBus bus)
        {
            _options = options;
            _session = session;
            _bus = bus;
        }

        public async UniTask<JoinPayload> JoinAsync(CancellationToken cancellation)
        {
            var client = new TcpClient();
            try
            {
                Debug.Log($"[TcpClientConnection] Connecting to {_options.Address}:{_options.Port}...");
                var connect = client.ConnectAsync(_options.Address, _options.Port).AsUniTask();
                int winner = await UniTask.WhenAny(connect, UniTask.Delay(ConnectTimeoutMs, cancellationToken: cancellation));
                if (winner != 0) throw new TimeoutException($"Could not reach {_options.Address}:{_options.Port}.");
            }
            catch
            {
                client.Close();
                throw;
            }

            _connection = new TcpConnection(client, MaxHostFrame);
            _connection.Send(NetCodec.Build(NetMessageType.Hello, w => NetCodec.WriteHello(w, _session.LocalPersistentId)));

            float deadline = Time.realtimeSinceStartup + WelcomeTimeout;
            while (true)
            {
                if (_connection.TryReceive(out byte[] frame))
                {
                    using BinaryReader reader = NetCodec.Open(frame, out NetMessageType type);
                    switch (type)
                    {
                        case NetMessageType.Welcome:
                            Debug.Log("[TcpClientConnection] Joined.");
                            return NetCodec.ReadJoinPayload(reader);
                        case NetMessageType.Reject:
                            throw new InvalidOperationException($"Host rejected the join: {reader.ReadString()}");
                        default:
                            throw new InvalidDataException($"Unexpected {type} before Welcome.");
                    }
                }

                if (_connection.IsClosed) throw new IOException("Host closed the connection.");
                if (Time.realtimeSinceStartup > deadline) throw new TimeoutException("Host did not answer.");
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
            }
        }

        public void StartReceiving() => _receiving = true;

        public void SendInput(in PlayerInputFrame frame)
        {
            if (_connection == null) return;
            PlayerInputFrame copy = frame;
            _connection.Send(NetCodec.Build(NetMessageType.Input, w => NetCodec.WriteInput(w, copy)));
        }

        public void SendCommand(in PlayerCommand command)
        {
            if (_connection == null) return;
            PlayerCommand copy = command;
            _connection.Send(NetCodec.Build(NetMessageType.Command, w => NetCodec.WriteCommand(w, copy)));
        }

        public void Tick()
        {
            if (_connection == null) return;

            if (_receiving)
            {
                while (_connection.TryReceive(out byte[] frame))
                {
                    try
                    {
                        using BinaryReader reader = NetCodec.Open(frame, out NetMessageType type);
                        if (type == NetMessageType.Event) NetCodec.DispatchEvent(reader, _bus);
                        else Debug.LogWarning($"[TcpClientConnection] Unexpected {type} from host.");
                    }
                    catch (Exception e) when (e is IOException or InvalidDataException or ArgumentException)
                    {
                        Debug.LogError($"[TcpClientConnection] Bad event from host, disconnecting: {e.Message}");
                        _connection.Close();
                        break;
                    }
                }
            }

            if (_connection.IsClosed && !_reportedDisconnect)
            {
                _reportedDisconnect = true;
                Debug.LogError("[TcpClientConnection] Disconnected from host.");
            }
        }

        public void Dispose() => _connection?.Close();
    }
}
