using UnityEngine;
using VContainer;
using Wordania.Core.Events;
using Wordania.Core.Gameplay;
using Wordania.Core.Identifiers;
using Wordania.Features.Bosses.Data;
using Wordania.Features.Bosses.Events;

namespace Wordania.Features.Bosses.Core
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
        private IEventBusGameplay _eventBus;
        protected TTemplate _template;

        [Inject]
        public void Construct(IEventBusGameplay eventBus)
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
            _eventBus.Publish(new BossDeathEvent(_template.Id));
        }
    }
}