using System;

using Kalmia_Game.Model.Engine;
using Kalmia_Game.Model.Reversi;

namespace Kalmia_Game.Model.Game
{
    internal class EvaluatorOptions
    {
        public int PlayoutNum { get; set; }
        public int NodeNumLimit { get; set; }
        public int ThreadNum { get; set; }
        public int SendInformationIntervalCs { get; set; }
    }

    internal class Evaluator : IDisposable
    {
        readonly KalmiaEngine ENGINE;
        readonly int PLAYOUT_NUM;
        readonly int NODE_NUM_LIMIT;
        readonly int THREAD_NUM;
        readonly int SEND_INFORMATION_INTERVAL_CS;

        public DiscColor LastEvaluatedDiscColor { get; private set; }
        public int LastEvaluatedMoveNum { get; private set; }
        public bool HasQuitUnexpectedly => this.ENGINE.HasQuitUnexpectedly;

        public event EventHandler OnEvaluationStopped = delegate { };
        public event USIInfoEventHandler OnInformationRecieved = delegate { };
        public event EventHandler OnTerminated = delegate { };

        public Evaluator(string enginePath, string engineArgs, string engineWorkDir, EvaluatorOptions options)
        {
            this.ENGINE = new KalmiaEngine(enginePath, engineArgs, engineWorkDir);
            this.ENGINE.OnBestMoveRecieved += (s, e) => this.OnEvaluationStopped.Invoke(this, EventArgs.Empty);
            this.ENGINE.OnInformationRecieved += (s, e) => this.OnInformationRecieved.Invoke(s, e);
            this.ENGINE.OnTerminated += (s, e) => { this.OnTerminated.Invoke(s, e); };

            this.PLAYOUT_NUM = options.PlayoutNum;
            this.NODE_NUM_LIMIT = options.NodeNumLimit;
            this.THREAD_NUM = options.ThreadNum;
            this.SEND_INFORMATION_INTERVAL_CS = options.SendInformationIntervalCs;
        }

        ~Evaluator() => this.ENGINE.Quit();

        public void Dispose()
        {
            this.ENGINE.Quit();
            GC.SuppressFinalize(this);
        }

        public void Run()
        {
            this.ENGINE.Run();
            this.ENGINE.SetOption("playout", this.PLAYOUT_NUM);
            this.ENGINE.SetOption("node_num_limit", this.NODE_NUM_LIMIT);
            this.ENGINE.SetOption("thread_num", this.THREAD_NUM);
            this.ENGINE.SetOption("show_search_info_interval_cs", this.SEND_INFORMATION_INTERVAL_CS);
        }

        public void StartEvaluation(Position pos)
        {
            this.LastEvaluatedDiscColor = pos.SideToMove;
            this.LastEvaluatedMoveNum = pos.MoveHistroy.Count;
            this.ENGINE.SetPosition(pos);
            this.ENGINE.GoPonder();
        }

        public void StopEvaluation() => this.ENGINE.StopGo();

        public void Quit() => this.ENGINE.Quit();
    }
}
