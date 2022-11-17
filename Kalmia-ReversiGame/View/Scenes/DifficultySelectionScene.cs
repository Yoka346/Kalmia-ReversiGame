using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

using Kalmia_Game.Model.Game;
using Kalmia_Game.SDLWrapper;
using Kalmia_Game.View.Controls;

namespace Kalmia_Game.View.Scenes
{
    internal partial class DifficultySelectionScene : UserControl
    {
        bool transitionToNextScene = false;

        public DifficultySelectionScene()
        {
            InitializeComponent();
            this.Controls.HideAll();
        }

        static IEnumerable<KalmiaDifficulty> LoadDifficulties()
        {
            var count = 0;
            foreach (var files in Directory.GetFiles(FilePath.DifficultyDirPath))
                if (Path.GetFileName(files) == $"{count}.json")
                {
                    var difficulty = JsonSerializer.Deserialize<KalmiaDifficulty>(File.ReadAllText(files));
                    if (difficulty is not null)
                        yield return difficulty;
                    count++;
                }
        }

        void DifficultySelectionScene_Load(object sender, EventArgs e)
        {
            this.selectMenu.AddItemRange(LoadDifficulties());
            this.fadeIn.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
            Thread.Sleep(1);
            this.Controls.ShowAll();
        }

        void SelectMenu_OnSelectedIdxChanged(SelectMenu<KalmiaDifficulty> sender, int selectedIdx)
        {
            if (selectedIdx != -1)
                AudioMixer.PlayChannel(-1, GlobalSE.CursorSE);
            this.descriptionLabel.Text = sender.SelectedItem?.Description;
        }

        void SelectMenu_OnLeftClickItem(SelectMenu<KalmiaDifficulty> sender, int selectedIdx)
        {
            if (this.transitionToNextScene)
                return;

            AudioMixer.PlayChannel(-1, GlobalSE.ButtonPressSE);

            this.fadeOut.AnimationEnded += (s, e) =>
            {
                if (this.Parent is not MainForm)
                    return;
                var mainForm = (MainForm)this.Parent;
                mainForm.Invoke(() => mainForm.ChangeScene(new DiscSelectionScene(sender.Items[selectedIdx])));
            };

            this.transitionToNextScene = true;
            this.fadeOut.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
        }
    }
}
