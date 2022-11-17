using System.Threading;

using Kalmia_Game.Model.Reversi;

namespace Kalmia_Game.Model.Game
{
    internal class HumanPlayer : IPlayer
    {
        public string Name { get; }

        BoardCoordinate humanInput;
        bool waitingForInput = false;
        bool quitFlag = false;
        object lockObj = new();

        public HumanPlayer(string name) => this.Name = name;

        public void Quit() => this.quitFlag = true;

        public BoardCoordinate GenerateMove(Position pos)
        {
            if (pos.CanPass)
                return BoardCoordinate.Pass;

            this.waitingForInput = true;
            var move = BoardCoordinate.Null;
            while (!this.quitFlag)
            {
                while (!this.quitFlag && this.humanInput == BoardCoordinate.Null)
                    Thread.Yield();

                lock (this.lockObj)
                {
                    if (pos.IsLegalMove(this.humanInput))
                    {
                        move = this.humanInput;
                        break;
                    }
                    this.humanInput = BoardCoordinate.Null;
                }
            }
            this.waitingForInput = false;
            this.humanInput = BoardCoordinate.Null;
            return move;
        }

        public void SetInput(BoardCoordinate move)
        {
            if (this.waitingForInput)
                lock (this.lockObj)
                    this.humanInput = move;
        }
    }
}
