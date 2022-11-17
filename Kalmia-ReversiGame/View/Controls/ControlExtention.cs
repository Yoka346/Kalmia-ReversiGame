using System.Windows.Forms;

namespace Kalmia_Game.View.Controls
{
    internal static class ControlExtention
    {
        public static void ShowAll(this Control.ControlCollection controls)
        {
            foreach (var control in controls)
                if (control is not null)
                    ((Control)control).Show();
        }

        public static void HideAll(this Control.ControlCollection controls)
        {
            foreach (var control in controls)
                if (control is not null)
                    ((Control)control).Hide();
        }
    }
}
