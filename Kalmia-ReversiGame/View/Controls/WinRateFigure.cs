using System.Collections.Generic;
using System.Windows.Forms;

namespace Kalmia_Game.View.Controls
{
    public partial class WinRateFigure : UserControl
    {
        public List<float> BlackWinRates { set { this.blackWinRates = new(value); this.figureDisplay.Invalidate(); } }

        List<float> blackWinRates = new();

        public WinRateFigure() => InitializeComponent();
    }
}
