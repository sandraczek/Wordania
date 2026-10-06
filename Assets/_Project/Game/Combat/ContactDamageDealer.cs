using UnityEngine;
using VContainer;
using Wordania.Combat.Authority;
using Wordania.Gameplay;
using Wordania.Identifiers;

namespace Wordania.Combat
{
    public class ContactDamageDealer : MonoBehaviour
    {
        private ICombatAuthority _combat;
        private float _damageAmount;
        private Vector2 _knockback;
        private DamageType _damageType;
        private HealthChangeSource _source;
        private InstanceId _ownerId = InstanceId.Empty;

        [Inject]
        public void Construct(ICombatAuthority combat)
        {
            _combat = combat;
        }
        public void Initialize(float damageAmount, Vector2 knockbackForce, DamageType damageType, HealthChangeSource damageSource)
        {
            _damageAmount = damageAmount;
            _knockback = knockbackForce;
            _damageType = damageType;
            _source = damageSource;
        }
        public void InitializeSpawn(InstanceId ownerId)
        {
            _ownerId = ownerId;
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            TryDealDamage(collision.gameObject, collision.GetContact(0).point);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDealDamage(other.gameObject, other.ClosestPoint(transform.position));
        }

        private void TryDealDamage(GameObject target, Vector2 contactPoint)
        {
            if (target.TryGetComponent<ITrackable>(out var targetEntity))
            {
                float direction = Mathf.Sign(target.transform.position.x - contactPoint.x);
                Vector2 knockback = new(direction * _knockback.x, _knockback.y);
                var damageData = new DamagePayload(_damageAmount, _damageType, _source, _ownerId, contactPoint, knockback);
                _combat.RequestDamage(targetEntity.InstanceId, damageData);
            }
        }
    }
}