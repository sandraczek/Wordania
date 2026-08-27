namespace Wordania.Core.Identifiers
{
    using System;

    public readonly struct PersistentId : IEquatable<PersistentId>
    {
        public readonly Guid Value;

        public static readonly PersistentId Empty = new(Guid.Empty);

        public readonly bool IsEmpty => Value == Guid.Empty;

        public PersistentId(Guid value)
        {
            Value = value;
        }

        /// <summary>
        /// Generates a brand-new, globally unique PersistentId. Call this only once per identity
        /// (e.g. when a player is created for the very first time) and then persist the result.
        /// </summary>
        public static PersistentId New() => new(Guid.NewGuid());

        public readonly bool Equals(PersistentId other) => Value == other.Value;
        public override readonly bool Equals(object obj) => obj is PersistentId other && Equals(other);
        public override readonly int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(PersistentId left, PersistentId right) => left.Equals(right);
        public static bool operator !=(PersistentId left, PersistentId right) => !left.Equals(right);

        public override readonly string ToString() => Value.ToString();
    }
}
