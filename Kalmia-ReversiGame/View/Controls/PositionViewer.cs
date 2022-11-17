using System.Windows.Forms;

using Kalmia_Game.Model.Reversi;

namespace Kalmia_Game.View.Controls
{
    delegate void PositionViewerEventHandler(PositionViewer sender, BoardCoordinate coord);

    internal partial class PositionViewer : UserControl
    {
        Position pos;

        public event PositionViewerEventHandler OnMouseClicked = delegate { };

        public PositionViewer(Position pos)
        {
            this.pos = pos;
            InitializeComponent();
        }

        public bool Update(BoardCoordinate move)
        {
            if (this.pos.Update(move) is not null)
            {
                this.posDisplay.Invalidate();
                return true;
            }
            return false;
        }

        void PosDisplay_MouseClick(object sender, MouseEventArgs e)
        {
            var gridSize = (float)this.posDisplay.Width / Position.BOARD_SIZE;
            var x = (int)(e.X / gridSize);
            var y = (int)(e.Y / gridSize);
            this.OnMouseClicked.Invoke(this, (BoardCoordinate)(x + y * Position.BOARD_SIZE));
        }
    }
}
