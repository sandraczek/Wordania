using System;

namespace Wordania.Core.Identifiers
{
    [Flags]
    public enum EntityFaction
    {
        Player = 1 << 0,
        Enemy = 1 << 1,
        Enviroment = 1 << 2
    }
}