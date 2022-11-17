using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

using Kalmia_Game.Model.Game;
using Kalmia_Game.Model.Reversi;
using Kalmia_Game.SDLWrapper;
using Kalmia_Game.View.Controls;

namespace Kalmia_Game.View.Scenes
{
    internal partial class GameScene : UserControl
    {
        GameSceneModel model;
        Func<Position, UserControl> nextSceneCreator;
        Dictionary<string, Action> propertyChangedEventTable = new();

        MixerChunk putDiscSE;
        MixerChunk gameStartSE;
        MixerChunk gameOverSE;
        MixerChunk cutInSE;

        public GameScene(GameSceneModel model, Func<Position, UserControl> nextSceneCreator)
        {
            this.model = model;
            this.nextSceneCreator = nextSceneCreator;
            
            this.gameStartSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}game_start.ogg");
            this.gameOverSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}game_over.ogg");
            this.cutInSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}cut_in.ogg");
            this.putDiscSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}put_disc.ogg");

            InitializeComponent();
            BindEvents();
            this.Controls.HideAll();
        }

        void GameScene_Load(object sender, EventArgs e)
        {
            if (this.model.CurrentPlayer is HumanPlayer)
                this.posViewer.ShowLegalMovePointers = true;

            this.messageLabel.Text = "対局開始!!";
            this.blackPlayerNameLabel.Text = this.model.BlackPlayer.Name;
            this.whitePlayerNameLabel.Text = this.model.WhitePlayer.Name;
            this.difficultyLabel.Text = this.model.Title;

            this.showGameScene.AnimationEnded += ShowGameScene_OnEndAnimation;
            this.showGameScene.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 500);
            Thread.Sleep(1);
            this.Controls.ShowAll();
        }

        void GameScene_Disposed(object sender, EventArgs e)
        {
            this.model.Dispose();
            this.gameStartSE.Dispose();
            this.gameOverSE.Dispose();
            this.cutInSE.Dispose();
        }

        void BindEvents()
        {
            this.model.PropertyChanged += Model_PropertyChanged;
            this.model.BlackWinRateHistoryWasUpdated += Model_BlackWinRateHistoryWasUpdated;
            this.model.GameEnded += Model_GameEnded;
            this.model.EvaluatorShutdownUnexpectedly += Model_KalmiaPlayerShutdownUnexpectedly;
            this.model.KalmiaPlayerShutdownUnexpectedly += Model_KalmiaPlayerShutdownUnexpectedly;

            this.propertyChangedEventTable["Position"] = OnPositionChanged;
            this.propertyChangedEventTable["DiscCount"] = OnDiscCountChanged;
            this.propertyChangedEventTable["EvaluationInfo"] = OnEvaluationInfoChanged;
            this.propertyChangedEventTable["CurrentPlayer"] = OnCurrentPlayerChanged;
            this.propertyChangedEventTable["LastMoveInfo"] = OnLastMoveInfoChanged;
        }

        void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e) => Invoke(() =>
        {
            if (e.PropertyName is not null && this.propertyChangedEventTable.TryGetValue(e.PropertyName, out Action? handler))
                if (handler is not null)
                    Invoke(handler.Invoke);
        });

        void OnPositionChanged()
        {
            
        }

        void OnDiscCountChanged()
        {
            (var b, var w) = this.model.DiscCount;
            this.discCountLabel.Text = $"{b}-{w}";
        }

        void OnEvaluationInfoChanged()
        {
            (var nodeCount, var depth) = this.model.EvaluationInfo;
            this.searchInfoLabel.Text = $"読み: {NodeCountToText(nodeCount)}局面 {depth}手先";
        }

        void OnCurrentPlayerChanged() 
        { 
            this.sideToMoveLabel.Text = $"Turn: {this.model.CurrentPlayer?.Name}";
            this.posViewer.ShowLegalMovePointers = this.model.CurrentPlayer is HumanPlayer;
        }

        void OnLastMoveInfoChanged()
        {
            if (this.model.LastMoveInfo is null)
                return;

            (var player, var move) = this.model.LastMoveInfo.Value;
            if (move == BoardCoordinate.Pass)
            {
                this.messageLabel.Text = $"{player.Name} : パス";
                this.cutInLabel.Text = "パス";
                AudioMixer.PlayChannel(-1, this.cutInSE);
                DoCutInOutAnimation(1000);
            }
            else
            {
                this.messageLabel.Text = $"{player.Name} : {move}";
                AudioMixer.PlayChannel(-1, this.putDiscSE);
            }

            this.posViewer.Update(move);
        }

        void Model_BlackWinRateHistoryWasUpdated(object? sender, EventArgs e) => Invoke(() =>
        {
            var winRate = this.model.BlackWinRateHistory[^1];
            this.situationLabel.Text = WinRateToSituationText(DiscColor.Black, winRate);
            this.winRateBar.BlackWinRate = (int)winRate;
            this.winRateFigure.BlackWinRates = this.model.BlackWinRateHistory.ToList();
        });

        void Model_KalmiaPlayerShutdownUnexpectedly(object? sender, EventArgs e)
        {
            MessageBox.Show($"申し訳ございません。Kalmiaが予期せず終了しました。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            MessageBox.Show($"アプリケーションを終了します。", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (this.Parent is MainForm mainForm)
                this.Invoke(() => mainForm.ChangeScene(null));
        }

        void Model_GameEnded(GameSceneModel sender, GameEventArgs e) => Invoke(() =>
        {
            if (e.Result is not null && e.Result.Draw)
                this.messageLabel.Text = "Draw";
            else
            {
                var winner = e.Winner;
                Debug.Assert(winner is not null);
                if (this.model.BlackPlayer is HumanPlayer || this.model.WhitePlayer is HumanPlayer)
                {
                    if (winner is KalmiaPlayer)
                        this.messageLabel.Text = "You Lose...";
                    else
                        this.messageLabel.Text = "You Win!!";
                }
                else
                    this.messageLabel.Text = $"{winner.Name} Wins!!";
            }

            AudioMixer.PlayChannel(-1, this.gameOverSE);

            this.cutInLabel.Text = this.messageLabel.Text;
            DoCutInOutAnimation();
            this.nextSceneLabel.MouseEnter += NextSceneLabel_MouseEnter;
            this.nextSceneLabel.MouseLeave += NextSceneLabel_MouseLeave;
            this.nextSceneLabel.MouseClick += NextSceneLabel_MouseClick;
            this.blinkNextSceneLabel.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, int.MaxValue);
            this.nextSceneLabel.Show();
        });

        void ShowGameScene_OnEndAnimation(object? sender, EventArgs e)
        {
            Invoke(() => this.cutInLabel.Text = "対局開始");
            this.cutOut.AnimationEnded += StartGame;
            AudioMixer.PlayChannel(-1, this.gameStartSE);
            DoCutInOutAnimation();
        }

        void HideGameScene_OnEndAnimation(object? sender, EventArgs e) => Invoke(() =>
        {
            Debug.Assert(this.Parent is MainForm);
            ((MainForm)this.Parent).ChangeScene(this.nextSceneCreator.Invoke(this.model.Position ?? new Position()));
        });

        void BlinkNextSceneLabel_OnEndAnimation(object sender, EventArgs e)
            => this.nextSceneLabel.ForeColor = Color.FromArgb(255, this.nextSceneLabel.ForeColor);

        void StartGame(object? sender, EventArgs e)
        {
            this.model.GameStart();
            this.cutOut.AnimationEnded -= StartGame;
        }

        void PosViewer_OnMouseClicked(PositionViewer sender, BoardCoordinate coord) => this.model.SetHumanInput(coord);

        void NextSceneLabel_MouseEnter(object? sender, EventArgs e)
        {
            this.blinkNextSceneLabel.Stop();
        }

        void NextSceneLabel_MouseLeave(object? sender, EventArgs e)
            => this.blinkNextSceneLabel.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, int.MaxValue);

        void NextSceneLabel_MouseClick(object? sender, MouseEventArgs e)
        {
            this.blinkNextSceneLabel.Stop();
            this.nextSceneLabel.MouseLeave -= NextSceneLabel_MouseLeave;
            this.nextSceneLabel.MouseEnter -= NextSceneLabel_MouseEnter;
            this.hideGameScene.AnimationEnded += HideGameScene_OnEndAnimation;
            this.hideGameScene.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 500);
        }

        string NodeCountToText(long nodeCount)
        {
            if(nodeCount < (int)1.0e+7)
                return (nodeCount >= 10000) ? $"{(int)(nodeCount * 1.0e-4)}万" : nodeCount.ToString();

            var sb = new StringBuilder();
            if (nodeCount >= (int)1.0e+8)
            {
                sb.Append((int)(nodeCount * 1.0e-8)).Append('億');
                nodeCount %= (int)1.0e+8;
            }

            if(nodeCount >= (int)1.0e+7)
            {
                sb.Append((int)(nodeCount * 1.0e-7)).Append('千');
                nodeCount %= (int)1.0e+7;

                if (nodeCount >= 10000)
                    sb.Append((int)(nodeCount * 1.0e-4));

                sb.Append('万');
            }

            return sb.ToString();
        }

        string WinRateToSituationText(DiscColor color, float winRate)
        {
            DiscColor player;
            if (winRate < 50.0f)
            {
                player = (DiscColor)(-(int)color);
                winRate = 100.0f - winRate;
            }
            else
                player = color;

            if (50.0f <= winRate && winRate <= 55.0f)
                return "互角";

            var sb = new StringBuilder((player == DiscColor.Black) ? "先手(黒)" : "後手(白)").Append('が');
            if (winRate > 90.0f)
                sb.Append("勝勢");
            else if (winRate > 75.0f)
                sb.Append("優勢");
            else if (winRate > 65.0f)
                sb.Append("有利");
            else
                sb.Append("やや有利");

            return sb.ToString();
        }
    }
}
