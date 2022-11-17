using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;

using Kalmia_Game.Model.Engine;
using Kalmia_Game.Model.Reversi;

namespace Kalmia_Game.Model.Game
{
    internal class KalmiaDifficulty
    {
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string WinMessage { get; set; } = string.Empty;
        public string DrawMessage { get; set; } = string.Empty;
        public string LossMessage { get; set; } = string.Empty;
        public KalmiaOptions KalmiaOptions { get; set; } = new();

        public override string ToString() => this.Label;
    }

    internal class KalmiaOptions
    {
        public int PlayoutNum { get; set; } = 3200;
        public int StochasticMoveNum { get; set; } = 0;
        public double SoftmaxTemperature { get; set; } = 1.0;
        public int ThreadNum { get; set; } = Environment.ProcessorCount;
        public bool EnableExtraSearch { get; set; } = false;
        public bool ReuseSubtree { get; set; } = true;
        public long NodeNumLimit { get; set; } = (long)2.0e+8;
        public int EndgameMoveNum { get; set; } = -1;
        public int EndgameTranspositionTableSizeMiB = 256;
        public int SendInformationIntervalCs = 10;

        public KalmiaOptions() { }

        public KalmiaOptions Clone() 
        {
            var ret = JsonSerializer.Deserialize<KalmiaOptions>(JsonSerializer.Serialize(this));
            Debug.Assert(ret is not null);  // thisがnullではないので, retがnullになることはないと思うが, 警告を抑えるためにnullチェック.
            return ret;
        }
    }

    internal class KalmiaPlayer : IPlayer
    {
        public string Name { get; }
        public DiscColor CurrentMoveColor { get; private set; }
        public int LastMoveNum { get; private set; }

        bool quitFlag = false;

        public event USIBestMoveEventHandler BestMoveWasRecieved = delegate { };
        public event USIInfoEventHandler InformationWasRecieved = delegate { };
        public event EventHandler UnexpectedShutdownOccured = delegate { };

        readonly KalmiaEngine ENGINE;
        readonly KalmiaOptions OPTIONS;

        public KalmiaPlayer(string enginePath, string engineArgs, string engineWorkDir, KalmiaOptions options)
        {
            this.ENGINE = new KalmiaEngine(enginePath, engineArgs, engineWorkDir);
            this.ENGINE.OnInformationRecieved += ENGINE_OnInformationRecieved;
            this.ENGINE.OnBestMoveRecieved += ENGINE_OnBestMoveRecieved;
            this.ENGINE.OnTerminated += Engine_OnTerminated;
            this.ENGINE.Run();
            this.OPTIONS = options;
            this.Name = this.ENGINE.Name ?? "Kalmia";
            SetOptions();
        }

        public KalmiaOptions GetOptions() => this.OPTIONS.Clone();

        public BoardCoordinate GenerateMove(Position pos)
        {
            this.ENGINE.SetPosition(pos);
            this.CurrentMoveColor = pos.SideToMove;
            this.LastMoveNum = (Position.SQUARE_NUM - Position.CrossCoordinates.Length) - pos.CountEmptySquares();

            if (pos.Equals(new Position()))
                return BoardCoordinate.F5;

            var bestMove = BoardCoordinate.Null;
            var waiting = true;
            USIBestMoveEventHandler handler = (sender, move) => { bestMove = move; waiting = false; };
            this.ENGINE.OnBestMoveRecieved += handler;
            this.ENGINE.Go();
            while (!this.quitFlag && waiting) Thread.Yield();
            this.ENGINE.OnBestMoveRecieved -= handler;
            return bestMove;
        }

        public void StartPonder(Position pos)
        {
            this.ENGINE.SetPosition(pos);
            this.CurrentMoveColor = pos.SideToMove;
            this.LastMoveNum = (Position.SQUARE_NUM - Position.CrossCoordinates.Length) - pos.CountEmptySquares();
            this.ENGINE.GoPonder();
        }

        public void StopPonder() => this.ENGINE.StopGo();

        public void Quit()
        {
            this.quitFlag = true;
            this.ENGINE.Quit();
            if (!this.ENGINE.HasExited)
                this.ENGINE.Kill();
        }

        void SetOptions()
        {
            this.ENGINE.SetOption("playout", this.OPTIONS.PlayoutNum);
            this.ENGINE.SetOption("stochastic_move_num", this.OPTIONS.StochasticMoveNum);
            this.ENGINE.SetOption("softmax_temperature", this.OPTIONS.SoftmaxTemperature);
            this.ENGINE.SetOption("thread_num", this.OPTIONS.ThreadNum);
            this.ENGINE.SetOption("enable_extra_search", this.OPTIONS.EnableExtraSearch);
            this.ENGINE.SetOption("reuse_subtree", this.OPTIONS.ReuseSubtree);
            this.ENGINE.SetOption("node_num_limit", this.OPTIONS.NodeNumLimit);
            this.ENGINE.SetOption("endgame_move_num", this.OPTIONS.EndgameMoveNum);
            this.ENGINE.SetOption("endgame_tt_size_mib", this.OPTIONS.EndgameTranspositionTableSizeMiB);
            this.ENGINE.SetOption("show_search_info_interval_cs", this.OPTIONS.SendInformationIntervalCs);
        }

        void ENGINE_OnBestMoveRecieved(KalmiaEngine sender, BoardCoordinate move)
            => this.BestMoveWasRecieved.Invoke(sender, move);

        void ENGINE_OnInformationRecieved(KalmiaEngine engine, KalmiaInfo info) 
            => this.InformationWasRecieved.Invoke(engine, info);

        void Engine_OnTerminated(object? sender, EventArgs e)
        {
            if (!this.quitFlag && !this.ENGINE.HasQuitSuccessfully && !this.ENGINE.WasKilled)
                this.UnexpectedShutdownOccured.Invoke(this, EventArgs.Empty);
        }
    }
}
