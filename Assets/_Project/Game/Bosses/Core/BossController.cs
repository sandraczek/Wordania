using UnityEngine;
using VContainer;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Bosses.Data;
using Wordania.Bosses.Events;
using Wordania.Session;
using Wordania.Networking;
using Wordania.Combat;

namespace Wordania.Bosses.Core
{
    public interface IBossController
    {
        void Initialize(BossTemplate template, InstanceId instanceId);
    }
    [RequireComponent(typeof(ThreatAttacker))]
    public abstract class BossController : MonoBehaviour, IBossController
    {
        public InstanceId InstanceId { get; protected set; }
        public AssetId TemplateId { get; protected set; }

        /// <summary>One target for the whole boss, shared by all parts. Set before OnInitialize runs.</summary>
        public IBossTarget Target { get; protected set; }

        /// <summary>
        /// Number of registered parts. The spawner reserves 1 + PartCount consecutive ids: the boss takes the first,
        /// part N (1-based) takes InstanceId + N. Host and clients derive the same part ids this way.
        /// </summary>
        public abstract int PartCount { get; }

        public abstract void Initialize(BossTemplate template, InstanceId instanceId);

        protected InstanceId PartId(int index) => new(InstanceId.Value + (ulong)index);
    }
    public abstract class BossController<TTemplate> : BossController
        where TTemplate : BossTemplate
    {
        private IEventBus _eventBus;
        private SessionConfig _session;
        protected TTemplate _template;

        /// <summary>False on clients: the boss is a proxy driven by host snapshots, its AI must not run.</summary>
        protected bool IsSimulated => _session.IsHost;

        [Inject]
        public void Construct(IEventBus eventBus, SessionConfig session)
        {
            _eventBus = eventBus;
            _session = session;
        }
        public override void Initialize(BossTemplate template, InstanceId instanceId)
        {
            InstanceId = instanceId;
            TemplateId = template.Id;
            Target = new BossTarget(GetComponent<ThreatAttacker>(), transform.position);
            if (!TryGetComponent<ThreatAttacker>(out _))
                Debug.LogError($"[BossSystem] {gameObject.name} has no ThreatAttacker on its root; add it to the prefab and save it. Parts will not pick targets.", this);
            if (template is TTemplate typedTemplate)
            {
                OnInitialize(typedTemplate);
                _template = typedTemplate;
            }
            else
            {
                Debug.LogError($"[BossSystem] Template mismatch on {gameObject.name}. " +
                                      $"Expected {typeof(TTemplate).Name}, got {template.GetType().Name}");
            }
        }

        protected abstract void OnInitialize(TTemplate template);
        public virtual void OnDeathSequenceComplete()
        {
            if (!_session.IsHost) return;

            _eventBus.PublishSimulation(new BossDeathEvent(_template.Id));
            _eventBus.PublishReplicated(new EntityDespawnedEvent(InstanceId));
        }
    }
}