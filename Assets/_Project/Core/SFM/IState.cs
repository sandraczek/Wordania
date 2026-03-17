namespace Wordania.Core.SFM
{
    public interface IState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
        void CheckSwitchStates();
    }
}