using UnityEngine;
using VContainer;
using Wordania.Events;
using Wordania.Identifiers;
using Wordania.Bosses.Data;
using Wordania.Bosses.Events;

namespace Wordania.Bosses.Core
{
    public interface IBossController
    {
        void Initialize(BossTemplate template, InstanceId instanceId);
    }
    public abstract class BossController : MonoBehaviour, IBossController
    {
        public InstanceId InstanceId { get; protected set; }
        public abstract void Initialize(BossTemplate template, InstanceId instanceId);
    }
    public abstract class BossController<TTemplate> : BossController
        where TTemplate : BossTemplate
    {
        private IEventBus _eventBus;
        protected TTemplate _template;

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        public override void Initialize(BossTemplate template, InstanceId instanceId)
        {
            InstanceId = instanceId;
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
            _eventBus.PublishSimulation(new BossDeathEvent(_template.Id));
        }
    }
}