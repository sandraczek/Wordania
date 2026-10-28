using System.Collections.Generic;
using UnityEngine;
using Wordania.Combat;
using Wordania.Enemies.AI;
using Wordania.Enemies.Data;
using Wordania.Gameplay;
using Wordania.Identifiers;

namespace Wordania.Enemies.FSM.States
{
    /// <summary>
    /// Telegraphed melee swing: Windup (tinted, standing) -> one hit check in front -> Recovery.
    /// Damage goes through ICombatAuthority like every other hit in the game.
    /// </summary>
    public sealed class EnemyMeleeAttackState : EnemyBaseState
    {
        private static readonly Color WindupTint = new(1f, 0.85f, 0.3f);
        private bool _hasStruck;

        public EnemyMeleeAttackState(EnemyBrain brain) : base(brain) { }

        private EnemyMeleeAttackData Melee => _controller.Data.Combat.Melee;

        public override bool IsFinished => _brain.TimeInState >= Melee.WindupDuration + Melee.RecoveryDuration;

        public override void Enter()
        {
            _hasStruck = false;
            if (_bb.HasTarget)
                _controller.CheckForFlip(_bb.TargetPosition.x - _controller.Position.x);
            _controller.SetTint(WindupTint);
        }

        public override void FixedUpdate()
        {
            _controller.Stop();

            if (!_hasStruck && _brain.TimeInState >= Melee.WindupDuration)
            {
                _hasStruck = true;
                _controller.SetTint(Color.white);
                Strike();
            }
        }

        public override void Exit()
        {
            _controller.SetTint(Color.white);
        }

        private void Strike()
        {
            EnemyMeleeAttackData melee = Melee;
            float facing = _controller.IsFacingRight ? 1f : -1f;
            Vector2 center = _controller.Position + new Vector2(melee.HitboxOffset.x * facing, melee.HitboxOffset.y);
            Bounds hitbox = new(center, melee.HitboxSize);

            DrawHitbox(hitbox);

            IReadOnlyList<Entity> players = _brain.Entities.Players;
            for (int i = 0; i < players.Count; i++)
            {
                Entity player = players[i];
                if (!EnemyBlackboard.IsAlive(player)) continue;
                if (!player.TryGetFeature(out Collider2D col) || !col.bounds.Intersects(hitbox)) continue;
                if (!player.TryGetFeature(out ITrackable trackable)) continue;

                Vector2 knockback = new(facing * melee.Knockback.x, melee.Knockback.y);
                var payload = new DamagePayload(melee.Damage, melee.DamageType, melee.DamageSource,
                    _controller.InstanceId, col.bounds.ClosestPoint(center), knockback);
                _brain.Combat.RequestDamage(trackable.InstanceId, payload);
            }
        }

        private static void DrawHitbox(Bounds b)
        {
            Vector2 min = b.min, max = b.max;
            Debug.DrawLine(new(min.x, min.y), new(max.x, min.y), Color.red, 0.3f);
            Debug.DrawLine(new(max.x, min.y), new(max.x, max.y), Color.red, 0.3f);
            Debug.DrawLine(new(max.x, max.y), new(min.x, max.y), Color.red, 0.3f);
            Debug.DrawLine(new(min.x, max.y), new(min.x, min.y), Color.red, 0.3f);
        }
    }
}
