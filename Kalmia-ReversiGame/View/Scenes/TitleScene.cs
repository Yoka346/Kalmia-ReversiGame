using System;
using System.Drawing;
using System.Windows.Forms;

using Kalmia_Game.SDLWrapper;
using Kalmia_Game.View.Controls;

namespace Kalmia_Game.View.Scenes
{
    internal partial class TitleScene : UserControl
    {
        bool transitionToNextScene = false;

        MixerMusic bgm;

        public TitleScene()
        {
            this.bgm = MixerMusic.LoadMusic($"{FilePath.BGMDirPath}title.ogg");
            InitializeComponent();
        }

        void TitleScene_Load(object sender, EventArgs e)
        {
            this.fadeIn.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1500);
            AudioMixer.PlayMusic(this.bgm);
        }

        void TitleScene_Disposed(object sender, EventArgs e) => this.bgm.Dispose();

        void FadeIn_OnEndAnimation(object? sender, EventArgs e)
            => this.fadeInForthAnniversaryLabel.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);

        void SelectMenu_OnSelectedIdxChanged(SelectMenu<string> sender, int selectedIdx)
        {
            if (selectedIdx != -1)
                AudioMixer.PlayChannel(-1, GlobalSE.CursorSE, 0);
        }

        void SelectMenu_OnLeftClickItem(SelectMenu<string> sender, int selectedIdx)
        {
            if (this.transitionToNextScene)
                return;

            AudioMixer.PlayChannel(-1, GlobalSE.ButtonPressSE, 0);

            var selectedItem = sender.SelectedItem;
            if (selectedItem == "Start")
            {
                this.fadeOut.AnimationEnded += (sender, e) =>
                {
                    if (this.Parent is not MainForm)
                        return;
                    var mainForm = (MainForm)this.Parent;
                    mainForm.Invoke(() => mainForm.ChangeScene(new DifficultySelectionScene()));
                };

                this.transitionToNextScene = true;
                SuspendFadeIn();
                this.fadeOut.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 2000);
            }
            else
            {
                this.fadeOut.AnimationEnded += (sender, e) =>
                {
                    if (this.Parent is not MainForm)
                        return;
                    var mainForm = (MainForm)this.Parent;
                    mainForm.Invoke(() => mainForm.ChangeScene(null));
                };

                this.transitionToNextScene = true;
                SuspendFadeIn();
                this.fadeOut.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 2000);
            }
            AudioMixer.FadeOutMusic(2000);
        }

        void SuspendFadeIn()
        {
            this.fadeIn.AnimationEnded -= FadeIn_OnEndAnimation;
            this.fadeIn.Stop();
            this.fadeInForthAnniversaryLabel.Stop();
        }
    }
}
