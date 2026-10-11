using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;
using Wordania.Combat;
using Wordania.Combat.Data;
using Wordania.Combat.Events;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Player.Events;
using Wordania.SaveSystem.Data;
using Wordania.WeaponStore;
using Wordania.World;
using Wordania.World.Editing;

namespace Wordania.Networking.Transport
{
    public enum NetMessageType : byte
    {
        Hello = 1,      // client -> host: protocol version, PersistentId
        Welcome = 2,    // host -> client: JoinPayload
        Reject = 3,     // host -> client: reason
        Input = 4,      // client -> host: PlayerInputFrame
        Command = 5,    // client -> host: PlayerCommand
        Event = 6,      // host -> client: replicated event
    }

    /// <summary>
    /// Binary encoding of everything that crosses the network. Every replicated event type must be registered in the
    /// static constructor (with a stable id); unregistered ones are logged once and not sent.
    /// Save-format data (world, inventory, skills, journal) travels as gzipped JSON, reusing the save serializer.
    /// </summary>
    public static class NetCodec
    {
        public const int ProtocolVersion = 3;

        private static readonly Dictionary<Type, (ushort Id, Delegate Write)> Writers = new();
        private static readonly Dictionary<ushort, Action<BinaryReader, IEventBus>> Readers = new();
        private static readonly HashSet<Type> ReportedUnknown = new();

        static NetCodec()
        {
            Register<HealthChangedEvent>(1, WriteHealth, ReadHealth);
            Register<DamageTakenEvent>(2, WriteDamageTaken, ReadDamageTaken);
            Register<ProjectileFiredEvent>(3, WriteProjectileFired, ReadProjectileFired);
            Register<HitRegisteredEvent>(4, WriteHitRegistered, ReadHitRegistered);
            Register<PlayerDeathEvent>(5, (w, e) => Write(w, e.Id), r => new PlayerDeathEvent(ReadInstanceId(r)));
            Register<WeaponBoughtEvent>(6, (w, e) => { Write(w, e.Buyer); Write(w, e.Id); }, r => new WeaponBoughtEvent(ReadPersistentId(r), ReadAssetId(r)));
            Register<EntitySpawnedEvent>(7, WriteSpawn, ReadSpawn);
            Register<EntityDespawnedEvent>(8, (w, e) => Write(w, e.Id), r => new EntityDespawnedEvent(ReadInstanceId(r)));
            Register<EntitySnapshotBatchEvent>(9, WriteSnapshots, ReadSnapshots);
            Register<TileChangesEvent>(10, WriteTileChanges, ReadTileChanges);
            Register<PlayerProgressEvent>(11, WriteProgress, ReadProgress);
        }

        private static void Register<T>(ushort id, Action<BinaryWriter, T> write, Func<BinaryReader, T> read)
            where T : struct, IReplicatedEvent
        {
            Writers[typeof(T)] = (id, write);
            Readers[id] = (reader, bus) => bus.Receive(read(reader));
        }

        // ---------- messages ----------

        public static byte[] Build(NetMessageType type, Action<BinaryWriter> body)
        {
            using var stream = new MemoryStream(64);
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write((byte)type);
                body?.Invoke(writer);
            }
            return stream.ToArray();
        }

        public static BinaryReader Open(byte[] frame, out NetMessageType type)
        {
            var reader = new BinaryReader(new MemoryStream(frame, writable: false), Encoding.UTF8);
            type = (NetMessageType)reader.ReadByte();
            return reader;
        }

        /// <summary>Returns null for event types that have no codec.</summary>
        public static byte[] BuildEvent<T>(T gameEvent) where T : struct, IReplicatedEvent
        {
            if (!Writers.TryGetValue(typeof(T), out var entry))
            {
                if (ReportedUnknown.Add(typeof(T)))
                    Debug.LogWarning($"[NetCodec] No codec for {typeof(T).Name}; it is not sent to clients.");
                return null;
            }

            return Build(NetMessageType.Event, w =>
            {
                w.Write(entry.Id);
                ((Action<BinaryWriter, T>)entry.Write)(w, gameEvent);
            });
        }

        public static void DispatchEvent(BinaryReader reader, IEventBus bus)
        {
            ushort id = reader.ReadUInt16();
            if (!Readers.TryGetValue(id, out var read))
                throw new InvalidDataException($"Unknown event id {id}");
            read(reader, bus);
        }

        public static void WriteHello(BinaryWriter w, PersistentId player)
        {
            w.Write(ProtocolVersion);
            Write(w, player);
        }

        public static void WriteInput(BinaryWriter w, in PlayerInputFrame f)
        {
            w.Write(f.Sequence);
            Write(w, f.Movement);
            w.Write(f.JumpHeld);
            Write(w, f.AimWorldPosition);
            w.Write(f.PrimaryHeld);
            w.Write(f.SecondaryHeld);
            w.Write(f.JumpPressCount);
            w.Write(f.HotbarPressCount);
            w.Write(f.LastHotbarSlot);
            w.Write(f.CyclePressCount);
        }

        public static PlayerInputFrame ReadInput(BinaryReader r) => new(
            r.ReadUInt32(),
            ReadVector2(r),
            r.ReadBoolean(),
            ReadVector2(r),
            r.ReadBoolean(),
            r.ReadBoolean(),
            r.ReadUInt16(),
            r.ReadUInt16(),
            r.ReadInt32(),
            r.ReadUInt16());

        public static void WriteCommand(BinaryWriter w, in PlayerCommand c)
        {
            w.Write((byte)c.Type);
            Write(w, c.Asset);
        }

        public static PlayerCommand ReadCommand(BinaryReader r)
        {
            var type = (PlayerCommandType)r.ReadByte();
            if (!Enum.IsDefined(typeof(PlayerCommandType), type))
                throw new InvalidDataException($"Unknown command {type}");
            return new PlayerCommand(type, ReadAssetId(r));
        }

        public static void WriteJoinPayload(BinaryWriter w, JoinPayload p)
        {
            WriteJson(w, p.World);
            w.Write(p.Entities.Count);
            foreach (var spawn in p.Entities) WriteSpawn(w, spawn);
            w.Write(p.Health.Count);
            foreach (var health in p.Health) WriteHealth(w, health);
        }

        public static JoinPayload ReadJoinPayload(BinaryReader r)
        {
            var payload = new JoinPayload { World = ReadJson<GameSaveData>(r) ?? new GameSaveData() };
            int spawns = ReadCount(r);
            for (int i = 0; i < spawns; i++) payload.Entities.Add(ReadSpawn(r));
            int health = ReadCount(r);
            for (int i = 0; i < health; i++) payload.Health.Add(ReadHealth(r));
            return payload;
        }

        // ---------- events ----------

        private static void WriteHealth(BinaryWriter w, HealthChangedEvent e)
        {
            Write(w, e.Target);
            w.Write(e.Current);
            w.Write(e.Max);
        }

        private static HealthChangedEvent ReadHealth(BinaryReader r) => new(ReadInstanceId(r), r.ReadSingle(), r.ReadSingle());

        private static void WriteDamageTaken(BinaryWriter w, DamageTakenEvent e)
        {
            Write(w, e.Target);
            DamagePayload p = e.Result.Payload;
            w.Write(p.Amount);
            w.Write((int)p.Type);
            w.Write((int)p.Source);
            Write(w, p.InstigatorId);
            Write(w, p.HitPoint);
            Write(w, p.Knockback);
            w.Write(e.Result.FinalDamage);
            w.Write(e.Result.WasEvaded);
        }

        private static DamageTakenEvent ReadDamageTaken(BinaryReader r)
        {
            InstanceId target = ReadInstanceId(r);
            var payload = new DamagePayload(
                r.ReadSingle(),
                (DamageType)r.ReadInt32(),
                (HealthChangeSource)r.ReadInt32(),
                ReadInstanceId(r),
                ReadVector2(r),
                ReadVector2(r));
            return new DamageTakenEvent(target, new DamageResult(payload, r.ReadSingle(), r.ReadBoolean()));
        }

        private static void WriteProjectileFired(BinaryWriter w, ProjectileFiredEvent e)
        {
            ProjectileSpawnData d = e.SpawnData;
            w.Write(d.Position.x); w.Write(d.Position.y); w.Write(d.Position.z);
            Write(w, d.Direction);
            Write(w, d.ProjectileId);
            w.Write(d.DamageMultiplier);
            w.Write(d.SpeedMultiplier);
            w.Write(d.LifetimeMultiplier);
            Write(w, d.InstigatorId);
            w.Write((int)d.TargetFactionMask);
        }

        private static ProjectileFiredEvent ReadProjectileFired(BinaryReader r) => new(new ProjectileSpawnData
        {
            Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
            Direction = ReadVector2(r),
            ProjectileId = ReadAssetId(r),
            DamageMultiplier = r.ReadSingle(),
            SpeedMultiplier = r.ReadSingle(),
            LifetimeMultiplier = r.ReadSingle(),
            InstigatorId = ReadInstanceId(r),
            TargetFactionMask = (EntityFaction)r.ReadInt32(),
        });

        private static void WriteHitRegistered(BinaryWriter w, HitRegisteredEvent e)
        {
            ProjectileHitData h = e.HitData;
            w.Write(h.Direction.x); w.Write(h.Direction.y);
            w.Write(h.ProjectileDataId);
            Write(w, h.HitEntityId);
            w.Write(h.HitPosition.x); w.Write(h.HitPosition.y);
            w.Write(h.DamageMultiplier);
            Write(w, h.InstigatorId);
        }

        private static HitRegisteredEvent ReadHitRegistered(BinaryReader r) => new(new ProjectileHitData
        {
            Direction = new float2(r.ReadSingle(), r.ReadSingle()),
            ProjectileDataId = r.ReadInt32(),
            HitEntityId = ReadInstanceId(r),
            HitPosition = new float2(r.ReadSingle(), r.ReadSingle()),
            DamageMultiplier = r.ReadSingle(),
            InstigatorId = ReadInstanceId(r),
        });

        private static void WriteSpawn(BinaryWriter w, EntitySpawnedEvent e)
        {
            Write(w, e.Id);
            w.Write((byte)e.Kind);
            Write(w, e.Asset);
            Write(w, e.Owner);
            Write(w, e.Position);
            w.Write(e.Health);
            w.Write(e.MaxHealth);
        }

        private static EntitySpawnedEvent ReadSpawn(BinaryReader r) => new(
            ReadInstanceId(r),
            (EntityKind)r.ReadByte(),
            ReadAssetId(r),
            ReadPersistentId(r),
            ReadVector2(r),
            r.ReadSingle(),
            r.ReadSingle());

        private static void WriteSnapshots(BinaryWriter w, EntitySnapshotBatchEvent e)
        {
            w.Write(e.Tick);
            w.Write(e.HostTime);
            w.Write(e.Snapshots.Length);
            foreach (EntitySnapshot s in e.Snapshots)
            {
                Write(w, s.Id);
                Write(w, s.Position);
                w.Write(s.Rotation.x); w.Write(s.Rotation.y); w.Write(s.Rotation.z); w.Write(s.Rotation.w);
                w.Write(s.InputAck);
            }
        }

        private static EntitySnapshotBatchEvent ReadSnapshots(BinaryReader r)
        {
            uint tick = r.ReadUInt32();
            float hostTime = r.ReadSingle();
            var snapshots = new EntitySnapshot[ReadCount(r)];
            for (int i = 0; i < snapshots.Length; i++)
            {
                snapshots[i] = new EntitySnapshot(
                    ReadInstanceId(r),
                    ReadVector2(r),
                    new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
                    r.ReadUInt32());
            }
            return new EntitySnapshotBatchEvent(tick, hostTime, snapshots);
        }

        private static void WriteTileChanges(BinaryWriter w, TileChangesEvent e)
        {
            w.Write(e.Changes.Length);
            foreach (TileChange c in e.Changes)
            {
                w.Write(c.X);
                w.Write(c.Y);
                Write(w, c.Main);
                w.Write(c.Damage);
                w.Write((int)c.Layers);
            }
        }

        private static TileChangesEvent ReadTileChanges(BinaryReader r)
        {
            var changes = new TileChange[ReadCount(r)];
            for (int i = 0; i < changes.Length; i++)
                changes[i] = new TileChange(r.ReadInt32(), r.ReadInt32(), ReadAssetId(r), r.ReadSingle(), (WorldLayer)r.ReadInt32());
            return new TileChangesEvent(changes);
        }

        private static void WriteProgress(BinaryWriter w, PlayerProgressEvent e)
        {
            Write(w, e.Player);
            WriteJson(w, e.Inventory);
            WriteJson(w, e.Skills);
            WriteJson(w, e.Journal);
        }

        private static PlayerProgressEvent ReadProgress(BinaryReader r) => new(
            ReadPersistentId(r),
            ReadJson<InventorySaveData>(r),
            ReadJson<SkillSaveData>(r),
            ReadJson<JournalSaveData>(r));

        // ---------- primitives ----------

        private static void Write(BinaryWriter w, InstanceId id) => w.Write(id.Value);
        private static InstanceId ReadInstanceId(BinaryReader r) => new(r.ReadUInt64());

        private static void Write(BinaryWriter w, AssetId id) => w.Write(id.Hash);
        private static AssetId ReadAssetId(BinaryReader r) => new(r.ReadInt32());

        private static void Write(BinaryWriter w, PersistentId id) => w.Write(id.Value.ToByteArray());
        public static PersistentId ReadPersistentId(BinaryReader r) => new(new Guid(r.ReadBytes(16)));

        private static void Write(BinaryWriter w, Vector2 v) { w.Write(v.x); w.Write(v.y); }

        /// <summary>Rejects NaN/infinity so a malformed peer can't poison physics or interpolation.</summary>
        private static Vector2 ReadVector2(BinaryReader r)
        {
            var v = new Vector2(r.ReadSingle(), r.ReadSingle());
            if (!float.IsFinite(v.x) || !float.IsFinite(v.y))
                throw new InvalidDataException("Non-finite vector");
            return v;
        }

        private static int ReadCount(BinaryReader r)
        {
            int count = r.ReadInt32();
            long remaining = r.BaseStream.Length - r.BaseStream.Position;
            if (count < 0 || count > remaining) // every element takes at least one byte
                throw new InvalidDataException($"Bad element count {count}");
            return count;
        }

        private static void WriteJson<T>(BinaryWriter w, T value) where T : class
        {
            if (value == null)
            {
                w.Write(0);
                return;
            }

            byte[] json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, Formatting.None));
            using var compressed = new MemoryStream();
            using (var gzip = new GZipStream(compressed, System.IO.Compression.CompressionLevel.Fastest, leaveOpen: true))
                gzip.Write(json, 0, json.Length);

            w.Write((int)compressed.Length);
            w.Write(compressed.GetBuffer(), 0, (int)compressed.Length);
        }

        private static T ReadJson<T>(BinaryReader r) where T : class
        {
            int length = ReadCount(r);
            if (length == 0) return null;

            using var compressed = new MemoryStream(r.ReadBytes(length), writable: false);
            using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
            using var text = new StreamReader(gzip, Encoding.UTF8);
            return JsonConvert.DeserializeObject<T>(text.ReadToEnd());
        }
    }
}
