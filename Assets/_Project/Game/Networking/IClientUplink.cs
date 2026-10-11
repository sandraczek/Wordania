namespace Wordania.Networking
{
    /// <summary>
    /// Client -> host channel (netcode seam). Input may go unreliable/unordered (frames are idempotent);
    /// commands must be reliable.
    /// </summary>
    public interface IClientUplink
    {
        void SendInput(in PlayerInputFrame frame);
        void SendCommand(in PlayerCommand command);
    }

    public sealed class NullClientUplink : IClientUplink
    {
        public void SendInput(in PlayerInputFrame frame) { }
        public void SendCommand(in PlayerCommand command) { }
    }
}
