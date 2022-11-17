using System;
using System.Windows.Forms;

using Kalmia_Game.Model.Reversi;
using Kalmia_Game.SDLWrapper;

namespace Kalmia_Game.View.Scenes
{
    internal partial class ResultScene : UserControl
    {
        public Position EndGamePosition { private get; set; }

        string message;

        MixerMusic bgm;

        public ResultScene(Position endPos, string message, string bgmPath)
        {
            this.message = message;
            this.EndGamePosition = endPos;
            this.bgm = MixerMusic.LoadMusic(bgmPath);
            InitializeComponent();
        }

        void ResultScene_Load(object sender, EventArgs e)
        {
            this.showScene.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 500);
            this.blinkBackToTitleLabel.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, int.MaxValue);
            AudioMixer.PlayMusic(this.bgm);
        }

        void ResultScene_Disposed(object sender, EventArgs e) => this.bgm.Dispose();

        void ResultScene_Click(object sender, EventArgs e)
        {
            this.blinkBackToTitleLabel.Stop();
            this.backToTitleLabel.Hide();
            this.hideScene.AnimationEnded += HideScene_OnEndAnimation;
            this.hideScene.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
            AudioMixer.FadeOutMusic(1000);
        }

        void HideScene_OnEndAnimation(object? sender, EventArgs e) => Invoke(() =>
        {
            if (this.Parent is MainForm mainForm)
                mainForm.ChangeScene(new TitleScene());
        });
    }
}
