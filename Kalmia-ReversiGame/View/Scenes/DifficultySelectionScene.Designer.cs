using System.Drawing;
using System.ComponentModel;

using Kalmia_Game.Model.Game;
using Kalmia_Game.View.Controls;

namespace Kalmia_Game.View.Scenes
{
    partial class DifficultySelectionScene
    {
        IContainer components = null;

        TransparentLabel requestLabel;
        SelectMenu<KalmiaDifficulty> selectMenu;
        TransparentLabel descriptionLabel;

        const int TEXT_ALPHA = 255;
        const int SELECT_MENU_NOT_SELECTED_ALPHA = 128;

        Animator fadeIn;
        Animator fadeOut;

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

            // Controls

            // Request label
            this.requestLabel = new TransparentLabel
            {
                ForeColor = Color.White,
                Text = "難易度を選択してください",
                Location = new Point(0, this.Height / 20),
                Size = new Size(this.Width, this.Height / 20),
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Height / 30, GraphicsUnit.Pixel),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            this.Controls.Add(this.requestLabel);

            // Select menu
            this.selectMenu = new SelectMenu<KalmiaDifficulty>(this.Width / 3, this.Height / 5, this.Width / 3, (int)(this.Height - this.Height * 0.6));
            this.Controls.Add(selectMenu);

            // Description label
            this.descriptionLabel = new TransparentLabel
            {
                ForeColor = Color.White,
                Location = new Point(0, (int)(this.Height - this.Height * 0.2)),
                Size = new Size(this.Width, (int)(this.Height * 0.05)),
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, (int)(this.Height * 0.045), GraphicsUnit.Pixel),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(this.descriptionLabel);

            // Animation
            this.fadeIn = new Animator((frameCount, frameNum) =>
            {
                setAlphaRate((double)frameCount / (frameNum - 1));
                return true;
            });

            this.fadeOut = new Animator((frameCount, frameNum) =>
            {
                setAlphaRate(1.0 - (double)frameCount / (frameNum - 1));
                return true;
            });

            void setAlphaRate(double rate)
            {
                this.requestLabel.ForeColor = Color.FromArgb((int)(TEXT_ALPHA * rate), this.requestLabel.ForeColor);

                var color = Color.FromArgb((int)(SELECT_MENU_NOT_SELECTED_ALPHA * rate), this.selectMenu.NotSelectedTextColor);
                this.selectMenu.NotSelectedTextColor = color;

                color = Color.FromArgb((int)(TEXT_ALPHA * rate), this.selectMenu.SelectedTextColor);
                this.selectMenu.SelectedTextColor = color;

                color = Color.FromArgb((int)(TEXT_ALPHA * rate), this.descriptionLabel.ForeColor);
                this.descriptionLabel.ForeColor = color;
            }

            // Events
            this.Load += DifficultySelectionScene_Load;
            this.selectMenu.OnSelectedIdxChanged += SelectMenu_OnSelectedIdxChanged;
            this.selectMenu.OnLeftClickItem += SelectMenu_OnLeftClickItem;
        }
    }
}
