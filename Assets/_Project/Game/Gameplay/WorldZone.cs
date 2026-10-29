using UnityEngine;

namespace Wordania.Gameplay
{
    /// <summary>
    /// Named world-space rectangle (e.g. the safe camp of a structure). Plain data: it lives in world/save data,
    /// not in the scene, so generated content can define zones without any GameObject.
    /// </summary>
    public sealed class WorldZone
    {
        public string Id { get; }
        public Rect Area { get; }

        public WorldZone(string id, Rect area)
        {
            Id = id;
            Area = area;
        }

        public bool Contains(Vector2 point) => Area.Contains(point);

        public void DebugDraw(Color color, float duration)
        {
            Vector2 min = Area.min;
            Vector2 max = Area.max;
            Debug.DrawLine(new Vector2(min.x, min.y), new Vector2(max.x, min.y), color, duration);
            Debug.DrawLine(new Vector2(max.x, min.y), new Vector2(max.x, max.y), color, duration);
            Debug.DrawLine(new Vector2(max.x, max.y), new Vector2(min.x, max.y), color, duration);
            Debug.DrawLine(new Vector2(min.x, max.y), new Vector2(min.x, min.y), color, duration);
        }
    }
}
