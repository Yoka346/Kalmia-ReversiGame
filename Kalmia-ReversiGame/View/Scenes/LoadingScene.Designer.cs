using System;
using System.Drawing;
using System.ComponentModel;

using Kalmia_Game.View.Controls;

namespace Kalmia_Game.View.Scenes
{
    partial class LoadingScene
    {
        IContainer components = null;

        TransparentLabel loadingLabel;

        Animator blinkLoadingLabelText;

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

            // Scene view
            this.BackColor = Color.Black;
            this.Width = GlobalConfig.Instance.ScreenWidth;
            this.Height = GlobalConfig.Instance.ScreenHeight;

            // loading label
            this.loadingLabel = new TransparentLabel
            {
                Text = "Now Loading",
                Location = new Point(0, 0),
                Size = this.Size,
                ForeColor = Color.White,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Width / 30.0f, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.loadingLabel);

            // Animation
            this.blinkLoadingLabelText = new Animator((frameCount, frameNum) =>
            {
                const double SPEED = 0.1;
                var rate = (Math.Sin(SPEED * frameCount) + 1.0) * 0.5;
                Invoke(() => this.loadingLabel.ForeColor = Color.FromArgb((int)(255.0 * rate), Color.White));
                return this.nextScene is null;
            });

            // Events
            this.Load += LoadingScene_Load;
        }
    }
}
