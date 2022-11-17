using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kalmia_Game.View.Scenes
{
    /// <summary>
    /// ロード画面. 与えられたユーザーコントロールのコンストラクタの呼び出しを終えたら、そのコントロールに遷移する.
    /// </summary>
    public partial class LoadingScene : UserControl
    {
        readonly Func<UserControl> NEXT_SCENE_CONSTRUCTOR;
        UserControl? nextScene;

        public LoadingScene(Func<UserControl> nextSceneConstructor)
        {
            this.NEXT_SCENE_CONSTRUCTOR = nextSceneConstructor;
            InitializeComponent();
        }

        async void LoadingScene_Load(object sender, EventArgs e)
        {
            this.blinkLoadingLabelText.AnimationEnded += BlinkLoadingLabelText_OnEndAnimation;
            this.blinkLoadingLabelText.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, int.MaxValue);
            await Task.Run(() => this.nextScene = this.NEXT_SCENE_CONSTRUCTOR.Invoke());
        }

        void BlinkLoadingLabelText_OnEndAnimation(object? sender, EventArgs e) => Invoke(() =>
        {
            if (this.Parent is MainForm form)
                form.ChangeScene(this.nextScene);
        });
    }
}
