using System.Linq;
using UnityEngine;
using Wordania.Combat;
using Wordania.Gameplay;
using Wordania.Services;
using Wordania.SFM;
using Wordania.Bosses.Data.SharedAttacks;

namespace Wordania.Bosses.Yeinn.Parts
{
    public sealed class YeinnHeadChaseState : IState
    {
        private readonly ChasePlayerAttack _data;
        private readonly YeinnHeadController _head;
        private readonly IEntityRegistry _entities;
        public YeinnHeadChaseState(ChasePlayerAttack chase, YeinnHeadController head, IEntityRegistry entities)
        {
            _head = head;
            _entities = entities;
            _data = chase;
        }

        public void CheckSwitchStates()
        {

        }
        public void Enter()
        {
            _head.CommandTrack(_entities.Players.FirstOrDefault().Transform, _data.Speed);
        }

        public void Update()
        {

        }
        public void FixedUpdate()
        {
            for (int i = 0; i < _entities.Players.Count; i++)
            {
                if (_entities.Players[i].TryGetFeature(out IReadOnlyHealth health) && !health.IsDead)
                {
                    return;
                }
            }
            _head.CommandHoverAttack();
        }
        public void Exit()
        {
            _head.StopMovement();
        }
    }
}