namespace Wordania.Core.Identifiers
{
    using System;

    /// <summary>
    /// Represents a dynamic runtime identifier for a specific spawned entity, world position, or abstract system.
    /// Values 0-1000 are strictly reserved for global systems and immutable sources.
    /// </summary>
    public readonly struct InstanceId : IEquatable<InstanceId>
    {
        public readonly ulong Value;

        // --- RESERVED SYSTEM IDENTIFIERS ---

        public static readonly InstanceId Empty = new(0);
        public static readonly InstanceId Debug = new(1);

        public static readonly InstanceId Innate = new(2);

        public static readonly InstanceId Journal = new(3);

        public static readonly InstanceId Environment = new(4);
        public static readonly InstanceId SkillTree = new(5);


        // --- CONSTRUCTORS & FACTORIES ---

        public InstanceId(ulong value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a deterministic InstanceId based on grid coordinates (e.g., for environmental blocks like Asphalt or Spikes).
        /// Bitwise shifts X to the upper 32 bits and leaves Y in the lower 32 bits of the 64-bit ulong.
        /// </summary>
        public static InstanceId FromGridCoordinates(int x, int y)
        {
            ulong packed = ((ulong)(uint)x << 32) | (uint)y;
            return new InstanceId(packed);
        }

        // --- EQUALITY OPERATORS ---

        public bool Equals(InstanceId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is InstanceId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(InstanceId left, InstanceId right) => left.Equals(right);
        public static bool operator !=(InstanceId left, InstanceId right) => !left.Equals(right);
    }
}