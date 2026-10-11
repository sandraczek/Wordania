using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Wordania.Networking.Transport
{
    /// <summary>
    /// One TCP connection with length-prefixed frames. Reading and writing happen on background threads;
    /// the main thread only enqueues outgoing frames and drains <see cref="TryReceive"/>.
    /// Frame on the wire: [int32 little-endian length][length bytes].
    /// </summary>
    public sealed class TcpConnection : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly int _maxIncomingFrame;
        private readonly ConcurrentQueue<byte[]> _incoming = new();
        private readonly BlockingCollection<byte[]> _outgoing = new(new ConcurrentQueue<byte[]>());
        private int _closed;

        public bool IsClosed => Volatile.Read(ref _closed) != 0;
        public string RemoteEndPoint { get; }

        public TcpConnection(TcpClient client, int maxIncomingFrame)
        {
            _client = client;
            _client.NoDelay = true;
            _client.SendTimeout = 5000; // a stalled peer can't hold the writer (and the socket) forever
            _stream = client.GetStream();
            _maxIncomingFrame = maxIncomingFrame;
            RemoteEndPoint = client.Client.RemoteEndPoint?.ToString() ?? "?";

            new Thread(ReadLoop) { IsBackground = true, Name = "Wordania.Net.Read" }.Start();
            new Thread(WriteLoop) { IsBackground = true, Name = "Wordania.Net.Write" }.Start();
        }

        public void Send(byte[] frame)
        {
            if (IsClosed) return;
            try { _outgoing.Add(frame); }
            catch (InvalidOperationException) { } // completed by Close on another thread
        }

        public bool TryReceive(out byte[] frame) => _incoming.TryDequeue(out frame);

        /// <summary>Stops accepting frames; already queued ones (e.g. a Reject) are still flushed, then the socket closes.</summary>
        public void Close()
        {
            if (Interlocked.Exchange(ref _closed, 1) != 0) return;
            _outgoing.CompleteAdding();
        }

        public void Dispose() => Close();

        private void ReadLoop()
        {
            var header = new byte[4];
            try
            {
                while (!IsClosed)
                {
                    if (!ReadExactly(header, 4)) break;
                    int length = BitConverter.ToInt32(header, 0);
                    if (!BitConverter.IsLittleEndian) length = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(length);

                    // Never trust the peer's length: a bogus value would make us allocate gigabytes.
                    if (length <= 0 || length > _maxIncomingFrame) break;

                    var frame = new byte[length];
                    if (!ReadExactly(frame, length)) break;
                    _incoming.Enqueue(frame);
                }
            }
            catch (Exception e) when (e is IOException or ObjectDisposedException or SocketException) { }
            finally
            {
                Close();
            }
        }

        private void WriteLoop()
        {
            try
            {
                foreach (byte[] frame in _outgoing.GetConsumingEnumerable())
                {
                    int length = frame.Length;
                    if (!BitConverter.IsLittleEndian) length = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(length);
                    _stream.Write(BitConverter.GetBytes(length), 0, 4);
                    _stream.Write(frame, 0, frame.Length);
                }
            }
            catch (Exception e) when (e is IOException or ObjectDisposedException or SocketException or InvalidOperationException) { }
            finally
            {
                Close();
                _client.Close(); // also unblocks the read thread
            }
        }

        private bool ReadExactly(byte[] buffer, int count)
        {
            int offset = 0;
            while (offset < count)
            {
                int read = _stream.Read(buffer, offset, count - offset);
                if (read <= 0) return false; // remote closed
                offset += read;
            }
            return true;
        }
    }
}
