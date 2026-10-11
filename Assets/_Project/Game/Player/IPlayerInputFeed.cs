namespace Wordania.Player
{
    /// <summary>Supplies the input of a player that is not driven by local devices; called once per physics step.</summary>
    public interface IPlayerInputFeed
    {
        void Advance();
    }
}
