namespace Wordania.Core.Identifiers
{
    public interface IInstanceIdProvider
    {
        /// <summary>
        /// Generates a unique, deterministic-friendly InstanceId for dynamic entities.
        /// Starts from value 1001 to preserve reserved system identifiers (0-1000).
        /// </summary>
        InstanceId Next();
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
    }
}