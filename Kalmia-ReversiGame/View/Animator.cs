using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kalmia_Game.View
{
    internal class Animator
    {
        public event EventHandler AnimationEnded = delegate { };

        Func<int, int, bool> callback;
        bool stopFlag;
        Task? animationTask;

        public Animator(Func<int, int, bool> callback)
        {
            this.callback = callback;
        }

        public void AnimateForFrameNum(double interval, int frameNum)
        {
            this.stopFlag = false;

            this.animationTask = Task.Run(() =>
            {
                var nextTiming = (double)Environment.TickCount;
                for (var frameCount = 0; !this.stopFlag && frameCount < frameNum; frameCount++)
                {
                    while (Environment.TickCount < nextTiming) ;
                    if (!this.callback(frameCount, frameNum))
                        break;
                    nextTiming += interval;
                }
                this.AnimationEnded.Invoke(this, EventArgs.Empty);
            });
        }

        public void AnimateForDuration(double interval, int duration)
        {
            if (duration < interval)
                duration = (int)interval;
            AnimateForFrameNum(interval, (int)(duration / interval));
        }

        public void WaitForEndOfAnimation()
        {
            if (this.animationTask is null)
                return;

            while (!this.animationTask.IsCompleted)
                Application.DoEvents();
        }

        public void Stop() => this.stopFlag = true;
    }
}
