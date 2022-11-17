using System;
using System.Threading;
using System.Threading.Tasks;

using Kalmia_Game.Model.Reversi;

namespace Kalmia_Game.Model.Game
{
    internal class GameEventArgs : EventArgs
    {
        public BoardCoordinate Coord { get; }
        public GameResult? Result { get; }
        public IPlayer? Winner { get; }

        public GameEventArgs(BoardCoordinate coord, GameResult? result, IPlayer? winner)
        {
            this.Coord = coord;
            this.Result = result;
            this.Winner = winner;
        }
    }

    internal delegate void GameEventHandler(GameManager sender, GameEventArgs e);

    /// <summary>
    /// 対局を管理するクラス.
    /// </summary>
    internal class GameManager : IDisposable
    {
        public IPlayer BlackPlayer { get; }
        public IPlayer WhitePlayer { get; }
        public IPlayer? CurrentPlayer { get; private set; }
        public IPlayer? OpponentPlayer { get; private set; }
        public DiscColor SideToMove { get { return this.pos.SideToMove; } }
        public int BlackDiscCount { get { return this.pos.CountDiscs(DiscColor.Black); } }
        public int WhiteDiscCount { get { return this.pos.CountDiscs(DiscColor.White); } }
        public int MoveCount { get; private set; }
        public bool NowPlaying { get; private set; }
        public bool Paused { get; private set; }

        public event GameEventHandler SideToMoveChanged = delegate { };
        public event GameEventHandler BlackPlayed = delegate { };
        public event GameEventHandler WhitePlayed = delegate { };
        public event GameEventHandler GameEnded = delegate { };

        Position pos;
        bool suspendFlag = false;

        public GameManager(IPlayer blackPlayer, IPlayer whitePlayer)
        {
            this.BlackPlayer = blackPlayer;
            this.WhitePlayer = whitePlayer;
            this.pos = new Position();
        }

        ~GameManager() => Dispose();

        public void Dispose()
        {
            this.BlackPlayer.Quit();
            this.WhitePlayer.Quit();
            GC.SuppressFinalize(this);
        }

        public Position GetPosition() => new(this.pos);
        public void Start() => Task.Run(Mainloop);
        public void Pause() => this.Paused = true;
        public void Resume() => this.Paused = false;

        public void Suspend()
        {
            this.suspendFlag = true;
            this.BlackPlayer.Quit();
            this.WhitePlayer.Quit();
            var startTime = Environment.TickCount;
            while (this.NowPlaying && Environment.TickCount - startTime < 10000)
                Thread.Yield();
        }

        void Mainloop()
        {
            this.NowPlaying = true;
            this.CurrentPlayer = this.BlackPlayer;
            this.OpponentPlayer = this.WhitePlayer;
            while (!pos.GetGameResult().GameOver)
            {
                if (this.suspendFlag)
                {
                    this.NowPlaying = false;
                    return;
                }
                WaitForResume();

                var move = this.CurrentPlayer.GenerateMove(this.pos);

                if (this.suspendFlag)
                {
                    this.NowPlaying = false;
                    return;
                }
                WaitForResume();

                if (!this.pos.IsLegalMove(move))
                    throw new IllegalMoveException(this.CurrentPlayer, move);

                this.pos.Update(move);

                if (this.CurrentPlayer == this.BlackPlayer)
                {
                    this.BlackPlayed.Invoke(this, new GameEventArgs(move, null, null));
                    this.CurrentPlayer = this.WhitePlayer;
                    this.OpponentPlayer = this.BlackPlayer;
                }
                else
                {
                    this.WhitePlayed.Invoke(this, new GameEventArgs(move, null, null));
                    this.CurrentPlayer = this.BlackPlayer;
                    this.OpponentPlayer = this.WhitePlayer;
                }
                this.SideToMoveChanged.Invoke(this, new GameEventArgs(BoardCoordinate.Null, null, null));

                this.MoveCount++;
            }

            this.NowPlaying = false;

            var result = this.pos.GetGameResult();
            IPlayer? winner;
            if (result.Draw)
                winner = null;
            else
                winner = (result.Winner == DiscColor.Black) ? this.BlackPlayer : this.WhitePlayer;

            this.GameEnded.Invoke(this, new GameEventArgs(BoardCoordinate.Null, result, winner));
            this.BlackPlayer.Quit();
            this.WhitePlayer.Quit();
        }

        void WaitForResume()
        {
            while (this.Paused)
                Thread.Yield();
        }
    }
}
