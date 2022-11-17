using System;
using System.Drawing;
using System.Windows.Forms;

using Kalmia_Game.SDLWrapper;

namespace Kalmia_Game.View
{
    public partial class MainForm : Form
    {
        UserControl currentScene;
        Point sceneLocation;
        Size sceneSize;

        public MainForm(Func<UserControl> sceneInitializer)
        {
            if (GlobalConfig.Instance.FullScreen)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.Location = Screen.PrimaryScreen.Bounds.Location;
                this.ClientSize = Screen.PrimaryScreen.Bounds.Size;
                GlobalConfig.Instance.ScreenHeight = this.ClientSize.Height;
                AdjustAspectRatio();
            }
            else
            {
                this.ClientSize = new Size(GlobalConfig.Instance.ScreenWidth, GlobalConfig.Instance.ScreenHeight);
                this.MinimumSize = this.MaximumSize = this.ClientSize;
                this.MinimizeBox = this.MaximizeBox = false;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
            }

            this.BackColor = Color.Black;
            this.currentScene = sceneInitializer.Invoke();
            this.currentScene.AutoSize = false;
            this.currentScene.Size = this.sceneSize;
            this.currentScene.Location = this.sceneLocation;
            this.Controls.Add(this.currentScene);
            this.Load += MainForm_Load;
            this.FormClosed += MainForm_FormClosed;
        }

        void MainForm_Load(object? sender, EventArgs e) => AudioMixer.Init();

        void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            GlobalConfig.Save();
            GlobalSE.DisposeAll();
        }

        void AdjustAspectRatio()
        {
            const float EPSILON = 1.0e-4f;
            var aspectRatio = GlobalConfig.Instance.AspectRatio;
            if (MathF.Abs((float)this.Width / this.Height - aspectRatio) <= EPSILON)
            {
                this.sceneLocation = new Point(0, 0);
                this.sceneSize = new Size(this.Width, this.Height);
                return;
            }

            var width = this.Height * aspectRatio;
            var height = (float)this.Height;
            var diff = width - this.Width;
            if (diff > 0.0f)
            {
                width -= diff;
                height -= diff / aspectRatio;
            }

            this.sceneSize = new Size((int)width, (int)height);
            this.sceneLocation = new Point((int)((this.Width - width) * 0.5f), (int)((this.Height - height) * 0.5f));
            GlobalConfig.Instance.ScreenHeight = (int)height;
        }

        public void ChangeScene(UserControl? scene)
        {
            var idx = this.Controls.Count - 1;
            this.Controls[idx].Hide();
            this.Controls[idx].Dispose();

            if (scene is null)
            {
                Close();
                return;
            }

            scene.AutoSize = false;
            scene.Size = this.sceneSize;
            scene.Location = this.sceneLocation;

            this.Controls.Add(scene);
            this.currentScene = scene;
            scene.Show();
        }
    }
}