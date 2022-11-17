using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;

using Kalmia_Game.Model.Reversi;
using Kalmia_Game.Model.Engine;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace Kalmia_Game.Model.Game
{
    internal class GameSceneModel : INotifyPropertyChanged, IDisposable
    {
        public delegate void GameEventHandler(GameSceneModel sender, GameEventArgs e);
        public string Title { get; }

        public Position? Position 
        {
            get => this.pos;
            private set { this.pos = value; NotifyPropertyChanged(); }
        }

        public (int black, int white) DiscCount 
        { 
            get => this.discCount; 
            private set { this.discCount = value; NotifyPropertyChanged(); } 
        }

        public (long nodeCount, int depth) EvaluationInfo
        {
            get => this.evaluationInfo;
            private set { this.evaluationInfo = value; NotifyPropertyChanged(); }
        }

        public IPlayer? CurrentPlayer
        {
            get => this.currentPlayer;
            private set { this.currentPlayer = value; NotifyPropertyChanged(); }
        }

        public (IPlayer player, BoardCoordinate move)? LastMoveInfo
        {
            get => this.lastMoveInfo;
            private set { this.lastMoveInfo = value; NotifyPropertyChanged(); }
        }

        public float BlackWinRate
        {
            get => (this.blackWinRateHistory.Count >= 1) ? this.blackWinRateHistory[^1] : 50.0f;
            private set { this.blackWinRateHistory[^1] = value; BlackWinRateHistoryWasUpdated?.Invoke(this, EventArgs.Empty); }
        }

        public float WhiteWinRate
        {
            get => 100.0f - this.BlackWinRate;
            private set => this.BlackWinRate = 100.0f - value;
        }

        public IPlayer BlackPlayer => this.game.BlackPlayer;
        public IPlayer WhitePlayer => this.game.WhitePlayer;

        public ReadOnlyCollection<float> BlackWinRateHistory => new(this.blackWinRateHistory);

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? BlackWinRateHistoryWasUpdated;
        public event EventHandler? EvaluatorShutdownUnexpectedly;
        public event EventHandler? KalmiaPlayerShutdownUnexpectedly;
        public event GameEventHandler? GameEnded;

        GameManager game;
        KalmiaPlayer evaluator;

        Position? pos;
        (int black, int white) discCount;
        (long nodeCount, int depth) evaluationInfo;
        IPlayer? currentPlayer;
        (IPlayer player, BoardCoordinate move)? lastMoveInfo;
        List<float> blackWinRateHistory = new();

        private GameSceneModel(string title, IPlayer blackPlayer, IPlayer whitePlayer, KalmiaPlayer evaluator)
        {
            this.Title = title;
            this.game = new GameManager(blackPlayer, whitePlayer);
            this.evaluator = evaluator;
            BindEvents();
        }

        public static GameSceneModel CreateGameSceneModel(DiscColor humanDiscColor, KalmiaDifficulty difficulty)
        {
            var human = new HumanPlayer("You");
            var kalmia = new KalmiaPlayer($"{FilePath.EngineDirPath}Kalmia.exe", string.Empty, FilePath.EngineDirPath, difficulty.KalmiaOptions);
            var title = $"Difficulty: {difficulty.Label}";
            return (humanDiscColor == DiscColor.Black) ? new GameSceneModel(title, human, kalmia, kalmia)
                                                       : new GameSceneModel(title, kalmia, human, kalmia);
        }

        public void Dispose()
        {
            if (this.game.NowPlaying)
                this.game.Dispose();
        }

        public void GameStart()
        {
            AddBlackWinRateToHistory(50.0f);
            var pos = this.game.GetPosition();
            this.CurrentPlayer = (pos.SideToMove == DiscColor.Black) ? this.game.BlackPlayer : this.game.WhitePlayer;
            if (this.CurrentPlayer != this.evaluator)
                this.evaluator.StartPonder(pos);
            this.game.Start();
        }

        public void SetHumanInput(BoardCoordinate coord)
        {
            if(this.CurrentPlayer is HumanPlayer human)
                human.SetInput(coord);
        }

        void BindEvents()
        {
            this.game.SideToMoveChanged += Game_OnSideToMoveChanged;
            this.game.BlackPlayed += Game_BlackPlayed;
            this.game.WhitePlayed += Game_WhitePlayed;
            this.game.GameEnded += Game_GameEnded;

            this.evaluator.BestMoveWasRecieved += Evaluator_BestMoveWasRecieved;
            this.evaluator.InformationWasRecieved += Evaluator_InformationWasRecieved;
            this.evaluator.UnexpectedShutdownOccured += Evaluator_UnexpectedShutdownOccured;

            {
                if (this.game.BlackPlayer is KalmiaPlayer kalmia)
                    kalmia.UnexpectedShutdownOccured += Kalmia_UnexpectedShutdownOccured;
            }

            {
                if (this.game.WhitePlayer is KalmiaPlayer kalmia)
                    kalmia.UnexpectedShutdownOccured += Kalmia_UnexpectedShutdownOccured;
            }
        }

        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        void AddBlackWinRateToHistory(float blackWinRate)
        {
            this.blackWinRateHistory.Add(blackWinRate);
            this.BlackWinRateHistoryWasUpdated?.Invoke(this, EventArgs.Empty);
        }

        void AddWhiteWinRateToHistory(float whiteWinRate) => AddBlackWinRateToHistory(100.0f - whiteWinRate);

        void Game_OnSideToMoveChanged(GameManager sender, GameEventArgs e)
        {
            this.Position = this.game.GetPosition();
            this.DiscCount = (this.game.BlackDiscCount, this.game.WhiteDiscCount);
            this.CurrentPlayer= this.game.CurrentPlayer;

            if (this.CurrentPlayer == this.evaluator)
            {
                var waitFlag = true;
                USIBestMoveEventHandler handler = (_, _) => waitFlag = false;
                this.evaluator.BestMoveWasRecieved += handler;
                this.evaluator.StopPonder();
                while (waitFlag)
                {
                    Application.DoEvents();
                    Thread.Yield();
                }
                this.evaluator.BestMoveWasRecieved -= handler;
                AddBlackWinRateToHistory(this.blackWinRateHistory[^1]);
            }
            else
            {
                AddBlackWinRateToHistory(this.blackWinRateHistory[^1]);
                this.evaluator.StartPonder(this.Position); 
            }
        }

        void Game_BlackPlayed(GameManager sender, GameEventArgs e) => this.LastMoveInfo = (sender.BlackPlayer, e.Coord);

        void Game_WhitePlayed(GameManager sender, GameEventArgs e) => this.LastMoveInfo = (sender.WhitePlayer, e.Coord);

        void Game_GameEnded(GameManager sender, GameEventArgs e) => this.GameEnded?.Invoke(this, e);

        void Evaluator_BestMoveWasRecieved(KalmiaEngine sender, BoardCoordinate move)
        {
            
        }

        void Evaluator_InformationWasRecieved(KalmiaEngine engine, KalmiaInfo info)
        {
            if (this.evaluator is null) // このイベントが発生する時点で, evaluatorはnullにはなり得ないが, 警告を抑えるためにnullチェック.
                return;

            if (info.MultiPV is not null)
                return;

            if (info.NodeCount is not null && info.Depth is not null)
                this.EvaluationInfo = (info.NodeCount.Value, info.Depth.Value);

            if(info.EvalScore is not null)
            {
                var value = (float)info.EvalScore.Value;
                if(engine.ScoreType == EvalScoreType.DiscDiff)
                    value = DiscDiffToWinRate(value);

                if (this.evaluator.CurrentMoveColor == DiscColor.Black)
                    this.BlackWinRate = value;
                else
                    this.WhiteWinRate = value;
            }
        }

        void Evaluator_UnexpectedShutdownOccured(object? sender, EventArgs e) => this.EvaluatorShutdownUnexpectedly?.Invoke(this, EventArgs.Empty);

        void Kalmia_UnexpectedShutdownOccured(object? sender, EventArgs e) => this.KalmiaPlayerShutdownUnexpectedly?.Invoke(this, EventArgs.Empty);

        static float DiscDiffToWinRate(float discDiff)
        {
            const double EPSILON = 1.0e-7;
            if (Math.Abs(discDiff) <= EPSILON)
                return 50.0f;
            return (discDiff > 0.0f) ? 100.0f : 0.0f;
        }
    }
}
