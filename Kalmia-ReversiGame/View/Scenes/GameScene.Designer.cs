using System;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

using Kalmia_Game.View.Controls;

namespace Kalmia_Game.View.Scenes
{
    partial class GameScene
    {
        const float BLINK_SPEED = 0.1f;

        IContainer components = null;

        PositionViewer posViewer;
        TransparentLabel discCountLabel;
        TransparentLabel sideToMoveLabel;
        TransparentLabel searchInfoLabel;
        TransparentLabel messageLabel;
        TransparentLabel situationLabel;
        TransparentLabel difficultyLabel;
        TransparentLabel blackPlayerNameLabel;
        TransparentLabel whitePlayerNameLabel;
        TransparentLabel nextSceneLabel;
        TransparentLabel cutInLabel;
        WinRateBar winRateBar;
        WinRateFigure winRateFigure;

        Animator showGameScene;
        Animator hideGameScene;
        Animator blinkNextSceneLabel;
        Animator cutIn;
        Animator cutOut;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;

            this.BackColor = Color.Black;
            this.Width = GlobalConfig.Instance.ScreenWidth;
            this.Height = GlobalConfig.Instance.ScreenHeight;

            // Controls

            // Position viewer
            var posViewerSize = (int)(this.Width * (1.0f / 2.5f));
            this.posViewer = new PositionViewer(new Kalmia_Game.Model.Reversi.Position())
            {
                Size = new Size(posViewerSize, posViewerSize),
                Location = new Point((this.Width - posViewerSize) / 2, (this.Height - posViewerSize) / 2),
                CoordFontFamily = GlobalConfig.Instance.DefaultFontFamily,
                BoardBackColor = Color.Green,
                ShowLegalMovePointers = false,
                BackgroundImage = Image.FromFile($"{FilePath.GraphDirPath}board_viewer_bg.bmp"),
                BoardBackgroundImage = Image.FromFile($"{FilePath.GraphDirPath}board_bg.bmp")
            };
            this.Controls.Add(this.posViewer);

            // Disc count label
            this.discCountLabel = new TransparentLabel
            {
                Location = new Point(this.Width / 16, this.Height / 3),
                Size = new Size(3 * this.Width / 16, this.Height / 10),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Height / 10.0f, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.discCountLabel);

            // Side to move label
            this.sideToMoveLabel = new TransparentLabel
            {
                Location = new Point(this.Width / 16, this.Height / 3 + this.Height / 9),
                Size = new Size(3 * this.Width / 16, this.Height / 10),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Height / 25.0f, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.sideToMoveLabel);

            // Search info label

            // Message label
            var messageLabelHeight = this.Height / 12;
            this.messageLabel = new TransparentLabel
            {
                Location = new Point
                {
                    X = this.posViewer.Location.X + this.posViewer.Width + this.Width / 35,
                    Y = this.posViewer.Location.Y
                },
                Size = new Size((int)(this.Width * 0.3), messageLabelHeight),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, messageLabelHeight * 0.5f, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.messageLabel);

            // Situation label
            this.situationLabel = new TransparentLabel
            {
                Location = new Point
                {
                    X = this.messageLabel.Location.X,
                    Y = this.messageLabel.Location.Y + this.messageLabel.Height
                },
                Size = new Size((int)(this.Width * 0.3), messageLabelHeight),
                ForeColor = Color.White,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, messageLabelHeight * 0.5f, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.situationLabel);

            // Difficulty label
            this.difficultyLabel = new TransparentLabel
            {
                Location = new Point
                {
                    X = this.situationLabel.Location.X,
                    Y = this.situationLabel.Location.Y + this.situationLabel.Height + this.Height / 10
                },
                Size = new Size((int)(this.Width * 0.3), messageLabelHeight),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, messageLabelHeight * 0.35f, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.difficultyLabel);

            // Player name label + Win rate bar + Search info label
            const float WIN_RATE_BAR_RATIO = 0.7f;

            var winRateBarMargin = (int)(this.Width * 0.01f);
            var playerNameLabelWidth = (int)(this.Width * (1.0f - WIN_RATE_BAR_RATIO) * 0.5f);
            var playerNameLabelHeight = this.Height / 15;
            var playerNameFontSize = playerNameLabelHeight * 0.5f;

            this.blackPlayerNameLabel = new TransparentLabel
            {
                Location = new Point(0, winRateBarMargin),
                Size = new Size(playerNameLabelWidth, playerNameLabelHeight),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, playerNameFontSize, GraphicsUnit.Pixel)
            };

            this.whitePlayerNameLabel = new TransparentLabel
            {
                Location = new Point(this.Width - playerNameLabelWidth, winRateBarMargin),
                Size = new Size(playerNameLabelWidth, playerNameLabelHeight),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, playerNameFontSize, GraphicsUnit.Pixel)
            };

            var winRateFontSize = playerNameFontSize * 0.8f;
            this.winRateBar = new WinRateBar
            {
                Location = new Point(playerNameLabelWidth, winRateBarMargin),
                Size = new Size((int)(this.Width * WIN_RATE_BAR_RATIO), playerNameLabelHeight),
                BlackWinRateTextColor = Color.White,
                WhiteWinRateTextColor = Color.White,
                BlackBarColor = Color.Red,
                WhiteBarColor = Color.Blue,
                BackColor = Color.Transparent,
                WinRateFont = new Font(GlobalConfig.Instance.DefaultFontFamily, winRateFontSize, GraphicsUnit.Pixel)
            };

            var searchInfoLabelHeight = playerNameLabelHeight;
            this.searchInfoLabel = new TransparentLabel
            {
                Location = new Point(0, this.winRateBar.Location.Y + this.winRateBar.Height),
                Size = new Size(this.Width, searchInfoLabelHeight),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, searchInfoLabelHeight * 0.5f, GraphicsUnit.Pixel)
            };

            this.Controls.Add(this.blackPlayerNameLabel);
            this.Controls.Add(this.whitePlayerNameLabel);
            this.Controls.Add(this.winRateBar);
            this.Controls.Add(this.searchInfoLabel);

            // Win rate figure
            var winRateFigureWidth = this.Width / 4;
            var winRateFigureHeight = this.Height / 5;
            var winRateFigureMargin = this.Width / 50;
            this.winRateFigure = new WinRateFigure
            {
                Size = new Size(winRateFigureWidth, winRateFigureHeight),
                Location = new Point
                {
                    X = this.Width - winRateFigureWidth - winRateFigureMargin,
                    Y = this.posViewer.Location.Y + this.posViewer.Height - winRateFigureHeight
                },
                BackColor = Color.DarkBlue,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, winRateFigureWidth / 10, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.winRateFigure);

            // Next scene label
            var nextSceneLabelHeight = this.Height / 10;
            this.nextSceneLabel = new TransparentLabel
            {
                Text = "ここをクリックして次の画面へ",
                Location = new Point(this.posViewer.Location.X, this.posViewer.Location.Y + this.posViewer.Height),
                Size = new Size(this.posViewer.Width, nextSceneLabelHeight),
                ForeColor = Color.FromArgb(0, Color.White),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, nextSceneLabelHeight * 0.5f, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.nextSceneLabel);
            this.nextSceneLabel.Hide();

            // Cut in label
            var cutInLabelHeight = this.Height / 10;
            this.cutInLabel = new TransparentLabel
            {
                Location = new Point(-this.Width,  (this.Height - cutInLabelHeight) / 2),
                Size = new Size(this.Width, nextSceneLabelHeight),
                ForeColor = Color.White,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, cutInLabelHeight * 0.5f, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.cutInLabel);
            this.cutInLabel.BringToFront();

            // Animation
            this.showGameScene = new Animator((frameCount, frameNum) =>
            {
                var rate = (float)frameCount / (frameNum - 1);
                var y = this.Height * 0.5f * (1 - rate);
                Invoke(() => this.Region = new Region(new RectangleF(0, y, this.Width, this.Height * rate)));
                return true;
            });

            this.hideGameScene = new Animator((frameCount, frameNum) =>
            {
                var rate = (float)frameCount / (frameNum - 1);
                var y = this.Height * 0.5f * rate;
                Invoke(() => this.Region = new Region(new RectangleF(0, y, this.Width, this.Height * (1.0f - rate))));
                return true;
            });

            this.blinkNextSceneLabel = new Animator((frameCount, frameNum) =>
            {
                var rate = (MathF.Sin(BLINK_SPEED * frameCount) + 1.0f) * 0.5f;
                if (!this.Disposing && !this.IsDisposed)
                {
                    Invoke(() => this.nextSceneLabel.ForeColor = Color.FromArgb((int)(255.0f * rate), this.nextSceneLabel.ForeColor));
                    return true;
                }
                return false;
            });
            this.blinkNextSceneLabel.AnimationEnded += BlinkNextSceneLabel_OnEndAnimation;

            this.cutIn = new Animator((frameCount, frameNum) =>
            {
                var rate = (float)frameCount / (frameNum - 1);
                float startX = -this.cutInLabel.Width;
                var endX = (this.Width - this.cutInLabel.Width) * 0.5f;
                var x = startX + (endX - startX) * rate;
                Invoke(() => this.cutInLabel.Location = new Point((int)x, this.cutInLabel.Location.Y));
                return true;
            });

            this.cutOut = new Animator((frameCount, frameNum) =>
            {
                var rate = (float)frameCount / (frameNum - 1);
                float startX = (this.Width - this.cutInLabel.Width) * 0.5f;
                var endX = this.Width + this.cutInLabel.Width;
                var x = startX + (endX - startX) * rate;
                Invoke(() => this.cutInLabel.Location = new Point((int)x, this.cutInLabel.Location.Y));
                return true;
            });

            // Events
            this.Load += GameScene_Load;
            this.Disposed += GameScene_Disposed;
            this.posViewer.OnMouseClicked += PosViewer_OnMouseClicked;
        }

        void DoCutInOutAnimation(int show_ms = 2000)
        {
            this.cutIn.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 500);
            this.cutIn.WaitForEndOfAnimation();

            Thread.Sleep(show_ms);

            this.cutOut.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 500);
            this.cutOut.WaitForEndOfAnimation();
        }
    }
}
