using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Kalmia_Game.View.Controls;

namespace Kalmia_Game.View.Scenes
{
    partial class ResultScene
    {
        const float BLINK_SPEED = 0.1f;

        IContainer components = null;

        TransparentLabel titleLabel;
        TransparentLabel messageLabel;
        TransparentLabel backToTitleLabel;
        PositionViewer gameRecordViewer;

        Animator showScene;
        Animator hideScene;
        Animator blinkBackToTitleLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        void InitializeComponent()
        {
            components = new Container();
            this.AutoScaleMode = AutoScaleMode.Font;

            // Scene view
            this.BackColor = Color.Black;
            this.Width = GlobalConfig.Instance.ScreenWidth;
            this.Height = GlobalConfig.Instance.ScreenHeight;

            // Controls

            // Title
            this.titleLabel = new TransparentLabel
            {
                Text = "Result",
                Size = new Size(this.Width, this.Height / 10),
                Location = new Point(0, this.Height / 10),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Height / 11, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.titleLabel);

            // Message label
            this.messageLabel = new TransparentLabel
            {
                Text = message,
                Size = new Size(this.Width, this.Height / 12),
                Location = new Point(0, this.titleLabel.Location.Y + this.titleLabel.Height + this.Height / 10),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Height / 20, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.messageLabel);

            // Game record viewer
            var gameRecordViewerSize = this.Width / 4;
            this.gameRecordViewer = new PositionViewer(this.EndGamePosition)
            {
                Size = new Size(gameRecordViewerSize, gameRecordViewerSize),
                Location = new Point((this.Width - gameRecordViewerSize) / 2, this.messageLabel.Location.Y + this.messageLabel.Height + this.Height / 25),
                CoordFontFamily = GlobalConfig.Instance.DefaultFontFamily,
                MoveNumFontFamily = GlobalConfig.Instance.DefaultFontFamily,
                ShowMoveHistory = true,
                BackgroundImage = Bitmap.FromFile($"{FilePath.GraphDirPath}/board_viewer_bg.bmp"),
                BoardBackgroundImage = Bitmap.FromFile($"{FilePath.GraphDirPath}/board_bg.bmp")
            };
            this.Controls.Add(this.gameRecordViewer);

            // Next scene label
            var nextSceneLabelHeight = this.Height / 10;
            this.backToTitleLabel = new TransparentLabel
            {
                Text = "画面をクリックしてタイトルに戻る",
                Location = new Point(0, this.gameRecordViewer.Location.Y + this.gameRecordViewer.Height),
                Size = new Size(this.Width, nextSceneLabelHeight),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, nextSceneLabelHeight * 0.5f, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.backToTitleLabel);

            // Animation
            this.showScene = new Animator((frameCount, frameNum) =>
            {
                var rate = (float)frameCount / (frameNum - 1);
                var y = this.Height * 0.5f * (1 - rate);
                Invoke(() => this.Region = new Region(new RectangleF(0, y, this.Width, this.Height * rate)));
                return true;
            });

            this.hideScene = new Animator((frameCount, frameNum) =>
            {
                var rate = (float)frameCount / (frameNum - 1);
                var y = this.Height * 0.5f * rate;
                Invoke(() => this.Region = new Region(new RectangleF(0, y, this.Width, this.Height * (1.0f - rate))));
                return true;
            });

            this.blinkBackToTitleLabel = new Animator((frameCount, frameNum) =>
            {
                var rate = (MathF.Sin(BLINK_SPEED * frameCount) + 1.0f) * 0.5f;
                if (!this.Disposing && !this.IsDisposed)
                {
                    Invoke(() => this.backToTitleLabel.ForeColor = Color.FromArgb((int)(255.0f * rate), this.backToTitleLabel.ForeColor));
                    return true;
                }
                return false;
            });

            // Events
            this.Load += ResultScene_Load;
            this.Disposed += ResultScene_Disposed;
            this.Click += ResultScene_Click;
            this.titleLabel.Click += ResultScene_Click;
            this.messageLabel.Click += ResultScene_Click;
            this.backToTitleLabel.Click += ResultScene_Click;
            this.gameRecordViewer.Click += ResultScene_Click;
        }
    }
}
