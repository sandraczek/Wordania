using System;
using UnityEngine;
using Wordania.Identifiers;

namespace Wordania.Session
{
    /// <summary>
    /// Stable identity of the local player on this device. Generated once and reused across sessions,
    /// so the host can match the player to their data in a world save (and later on reconnect).
    /// </summary>
    public static class LocalPlayerIdentity
    {
        private const string PrefsKey = "Wordania.LocalPersistentId";

        public static PersistentId GetOrCreate()
        {
            if (Guid.TryParse(PlayerPrefs.GetString(PrefsKey, string.Empty), out var guid) && guid != Guid.Empty)
            {
                return new PersistentId(guid);
            }

            var id = PersistentId.New();
            PlayerPrefs.SetString(PrefsKey, id.ToString());
            PlayerPrefs.Save();
            return id;
        }
    }
}
