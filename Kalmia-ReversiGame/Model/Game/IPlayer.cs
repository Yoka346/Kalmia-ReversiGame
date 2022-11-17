using Kalmia_Game.Model.Reversi;

namespace Kalmia_Game.Model.Game
{
    internal interface IPlayer
    {
        public string Name { get; }
        public void Quit();
        public BoardCoordinate GenerateMove(Position pos);
    }
}
