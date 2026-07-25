using System;
using UnityEngine;

namespace Wordania.Core.Identifiers
{
    [Serializable]
    public struct AssetId : IEquatable<AssetId>
    {
        [SerializeField, Tooltip("Designer-friendly string identifier. Hashed automatically.")]
        private string _editorId;

        [SerializeField, HideInInspector]
        private int _hash;

        public static readonly AssetId Empty = new(string.Empty);

        public readonly int Hash => _hash;

        public readonly bool IsEmpty => _hash == 0;

        public AssetId(string id)
        {
            _editorId = id;
            _hash = GenerateHash(id);
        }
        public AssetId(int savedHash)
        {
            _editorId = string.Empty;
            _hash = savedHash;
        }

        public void EditorInitialize()
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(_editorId))
            {
                _hash = GenerateHash(_editorId);
            }
            else
            {
                _hash = 0;
            }
#endif
        }

        // FNV-1a Hash algorithm - highly resistant to collisions for short strings
        private static int GenerateHash(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;

            unchecked
            {
                int hash = (int)2166136261;
                foreach (char c in input)
                {
                    hash = (hash ^ c) * 16777619;
                }
                return hash;
            }
        }

        public readonly bool Equals(AssetId other) => _hash == other._hash;
        public override readonly bool Equals(object obj) => obj is AssetId other && Equals(other);
        public override readonly int GetHashCode() => _hash;

        public static bool operator ==(AssetId left, AssetId right) => left.Equals(right);
        public static bool operator !=(AssetId left, AssetId right) => !left.Equals(right);

        public override string ToString()
        {
#if UNITY_EDITOR
            return string.IsNullOrEmpty(_editorId) ? "INVALID_ID" : _editorId;
#else
            return _hash.ToString();
#endif
        }
    }
}