using System.Drawing;
using System.ComponentModel;

using Kalmia_Game.View.Controls;

namespace Kalmia_Game.View.Scenes
{
    partial class TitleScene
    {
        System.ComponentModel.IContainer components = null;

        TransparentLabel titleLabel;
        SelectMenu<string> selectMenu;
        TransparentLabel forthAnniversaryLabel;

        const int TEXT_ALPHA = 255;
        const int SELECT_MENU_NOT_SELECTED_ALPHA = 128;

        Animator fadeIn;
        Animator fadeOut;
        Animator fadeInForthAnniversaryLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

            this.BackColor = Color.Black;
            this.Width = GlobalConfig.Instance.ScreenWidth;
            this.Height = GlobalConfig.Instance.ScreenHeight;

            // Controls

            // Title
            this.titleLabel = new TransparentLabel
            {
                ForeColor = Color.White,
                Text = "リバーシAI Kalmia",
                Location = new Point(0, this.Height / 10),
                Size = new Size(this.Width, this.Height / 5),
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Height / 10, GraphicsUnit.Pixel),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(this.titleLabel);

            // Forth anniversary
            this.forthAnniversaryLabel = new TransparentLabel
            {
                Text = "~4th Anniversary Edition~",
                ForeColor = Color.FromArgb(0, Color.White),
                Size = new Size(this.Width, this.Height / 8),
                Location = new Point(0, this.titleLabel.Location.Y + this.titleLabel.Height),
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Height / 15, FontStyle.Italic, GraphicsUnit.Pixel),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(this.forthAnniversaryLabel);

            // Select Menu
            this.selectMenu = new SelectMenu<string>(this.Width / 3, this.Height / 5, this.Width / 3, 7 * this.Height / 10);
            this.selectMenu.AddItemRange(new string[] { "Start", "Quit" });
            this.Controls.Add(selectMenu);

            this.fadeIn = new Animator((frameCount, frameNum) =>
            {
                var rate = (double)frameCount / (frameNum - 1);
                this.titleLabel.ForeColor = Color.FromArgb((int)(TEXT_ALPHA * rate), this.titleLabel.ForeColor);

                var color = Color.FromArgb((int)(SELECT_MENU_NOT_SELECTED_ALPHA * rate), this.selectMenu.NotSelectedTextColor);
                this.selectMenu.NotSelectedTextColor = color;

                color = Color.FromArgb((int)(TEXT_ALPHA * rate), this.selectMenu.SelectedTextColor);
                this.selectMenu.SelectedTextColor = color;
                return true;
            });

            this.fadeOut = new Animator((frameCount, frameNum) =>
            {
                var rate = 1.0 - (double)frameCount / (frameNum - 1);
                this.titleLabel.ForeColor = Color.FromArgb((int)(TEXT_ALPHA * rate), this.titleLabel.ForeColor);

                var color = Color.FromArgb((int)(SELECT_MENU_NOT_SELECTED_ALPHA * rate), this.selectMenu.NotSelectedTextColor);
                this.selectMenu.NotSelectedTextColor = color;

                color = Color.FromArgb((int)(TEXT_ALPHA * rate), this.selectMenu.SelectedTextColor);
                this.selectMenu.SelectedTextColor = color;

                color = Color.FromArgb((int)(TEXT_ALPHA * rate), this.forthAnniversaryLabel.ForeColor);
                this.forthAnniversaryLabel.ForeColor = color;
                return true;
            });

            this.fadeInForthAnniversaryLabel = new Animator((frameCount, frameNum) =>
            {
                var rate = (double)frameCount / (frameNum - 1);
                this.forthAnniversaryLabel.ForeColor
                = Color.FromArgb((int)(TEXT_ALPHA * rate), this.forthAnniversaryLabel.ForeColor);
                return true;
            });

            // Events
            this.Load += TitleScene_Load;
            this.Disposed += TitleScene_Disposed;
            this.selectMenu.OnLeftClickItem += SelectMenu_OnLeftClickItem;
            this.selectMenu.OnSelectedIdxChanged += SelectMenu_OnSelectedIdxChanged;
            this.fadeIn.AnimationEnded += FadeIn_OnEndAnimation;
        }
    }
}
