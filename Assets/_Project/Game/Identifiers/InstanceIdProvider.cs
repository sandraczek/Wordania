namespace Wordania.Identifiers
{
    public interface IInstanceIdProvider
    {
        /// <summary>
        /// Generates a unique, deterministic-friendly InstanceId for dynamic entities.
        /// Starts from value 1001 to preserve reserved system identifiers (0-1000).
        /// </summary>
        InstanceId Next();

        /// <summary>
        /// Reserves <paramref name="count"/> consecutive ids and returns the first one. Used by multi-entity spawns
        /// (boss + parts) so clients can derive the part ids from the root id alone.
        /// </summary>
        InstanceId NextBlock(int count);
    }

    public class InstanceIdProvider : IInstanceIdProvider
    {
        // Reserved system IDs occupy 0-1000. Dynamic entities start at 1001.
        private ulong _currentCounter = 1000;

        public InstanceId Next()
        {
            _currentCounter++;
            return new InstanceId(_currentCounter);
        }

        public InstanceId NextBlock(int count)
        {
            if (count < 1) count = 1;

            InstanceId first = new(_currentCounter + 1);
            _currentCounter += (ulong)count;
            return first;
        }
    }
}