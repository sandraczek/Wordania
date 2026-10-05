namespace Wordania.World.Editing
{
    /// <summary>
    /// The only entry point for gameplay code that wants to change the world. Requests are intents;
    /// the authority validates them and the outcome arrives as a batch of <see cref="TileChange"/>.
    /// </summary>
    public interface IWorldEditAuthority
    {
        void RequestMine(in MineRequest request);
        void RequestPlace(in PlaceRequest request);
    }
}
