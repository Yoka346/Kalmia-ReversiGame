using System;
using System.Windows.Forms;

using Kalmia_Game.Model.Game;
using Kalmia_Game.Model.Reversi;
using Kalmia_Game.SDLWrapper;
using Kalmia_Game.View.Controls;

namespace Kalmia_Game.View.Scenes
{
    internal partial class DiscSelectionScene : UserControl
    {
        bool transitionToNextScene = false;

        KalmiaDifficulty difficulty;

        public DiscSelectionScene(KalmiaDifficulty difficulty)
        {
            this.difficulty = difficulty;
            InitializeComponent();
            this.Controls.HideAll();
        }

        void DiscSelectionScene_Load(object sender, EventArgs e)
        {
            this.fadeIn.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
            this.Controls.ShowAll();
        }

        void SelectMenu_OnSelectedIdxChanged(SelectMenu<DiscColor> sender, int selectedIdx)
        {
            if (selectedIdx == -1)
            {
                this.descriptionLabel.Text = string.Empty;
                return;
            }

            AudioMixer.PlayChannel(-1, GlobalSE.CursorSE);

            var disc = sender.SelectedItem;
            if (disc == DiscColor.Black)
                this.descriptionLabel.Text = "先手";
            else if (disc == DiscColor.White)
                this.descriptionLabel.Text = "後手";
        }

        void SelectMenu_OnLeftClickItem(SelectMenu<DiscColor> sender, int selectedIdx)
        {
            if (this.transitionToNextScene)
                return;

            AudioMixer.PlayChannel(-1, GlobalSE.ButtonPressSE);

            this.fadeOut.AnimationEnded += (s, e) =>
            {
                if (this.Parent is MainForm mainForm)
                {
                    Invoke(() => mainForm.ChangeScene(
                        new LoadingScene(() => CreateGameScene(sender.Items[selectedIdx]))));
                }
            };

            this.transitionToNextScene = true;
            this.fadeOut.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
        }

        GameScene CreateGameScene(DiscColor humanDiscColor)
        {
            var model = GameSceneModel.CreateGameSceneModel(humanDiscColor, this.difficulty);
            return new GameScene(model, nextSceneCreator);

            UserControl nextSceneCreator(Position pos)
            {
                string message, bgmPath;
                var res = pos.GetGameResult();
                if (res.Draw)
                {
                    message = difficulty.DrawMessage;
                    bgmPath = $"{FilePath.BGMDirPath}draw.ogg";
                }
                else if (res.Winner == humanDiscColor)
                {
                    message = difficulty.WinMessage;
                    bgmPath = $"{FilePath.BGMDirPath}win.ogg";
                }
                else
                {
                    message = difficulty.LossMessage;
                    bgmPath = $"{FilePath.BGMDirPath}loss.ogg";
                }
                return new ResultScene(pos, message, bgmPath);
            }
        }
    }
}
