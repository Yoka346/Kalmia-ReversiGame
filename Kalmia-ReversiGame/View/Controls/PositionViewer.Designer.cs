using System.Linq;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;

using Kalmia_Game.Model.Reversi;

namespace Kalmia_Game.View.Controls
{
    partial class PositionViewer
    {
        const float MARGIN_SIZE_RATIO = 0.05f;
        const float DISC_SIZE_RATIO_TO_GRID_SIZE = 0.8f;
        const float MOVE_NUM_RATIO_TO_DISC_SIZE = 0.6f;
        const float COORD_SIZE_RATIO_TO_GRID_SIZE = 0.3333f;

        public Color GridColor { get => this.gridColor; set { this.gridColor = value; Invalidate(); } }
        public Color BlackDiscColor { get => this.blackDiscColor; set { this.blackDiscColor = value; Invalidate(); } }
        public Color WhiteDiscColor { get => this.whiteDiscColor; set { this.whiteDiscColor = value; Invalidate(); } }
        public Color LegalMovePointerColor { get => this.legalMovePointerColor; set { this.legalMovePointerColor = value; Invalidate(); } }
        public Color CoordLabelTextColor { get => this.coordLabelTextColor; set { this.coordLabelTextColor = value; Invalidate(); } }
        public string CoordFontFamily { get => this.coordFontFamily; set { this.coordFontFamily = value; Invalidate(); } }
        public Color BlackMoveNumTextColor { get => this.blackMoveNumTextColor; set { this.blackMoveNumTextColor = value; Invalidate(); } }
        public Color WhiteMoveNumTextColor { get => this.whiteMoveNumTextColor; set { this.whiteMoveNumTextColor = value; Invalidate(); } }
        public string MoveNumFontFamily { get => this.moveNumFontFamily; set { this.moveNumFontFamily = value; Invalidate(); } }
        public Color BoardBackColor { get => this.posDisplay.BackColor; set => this.posDisplay.BackColor = value; }
        public Image BoardBackgroundImage { get => this.boardBackgroundImage; set { this.boardBackgroundImage = value; this.posDisplay.Invalidate(); } }
        public bool ShowLegalMovePointers { get => showLegalMovePointers; set { this.showLegalMovePointers = value; this.posDisplay.Invalidate(); } }
        public bool ShowMoveHistory { get => showMoveHistory; set { this.showMoveHistory = value; this.posDisplay.Invalidate(); } }

        public override Image BackgroundImage { get => this.backgroundImage; set {this.backgroundImage = value; Invalidate(); } }

        System.ComponentModel.IContainer components = null;

        Image backgroundImage;
        Image boardBackgroundImage;
        Color gridColor = Color.Black;
        Color blackDiscColor = Color.Black;
        Color whiteDiscColor = Color.White;
        Color legalMovePointerColor = Color.Red;
        Color coordLabelTextColor = Color.White;
        string coordFontFamily = Control.DefaultFont.FontFamily.Name;
        Color blackMoveNumTextColor = Color.White;
        Color whiteMoveNumTextColor = Color.Black;
        string moveNumFontFamily = Control.DefaultFont.FontFamily.Name;
        bool showLegalMovePointers = true;
        bool showMoveHistory = false;
        int legalMovePointerThickness = 3;

        PictureBox posDisplay;
        TransparentLabel[] XCoordLabels;
        TransparentLabel[] YCoordLabels;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.DoubleBuffered = true;
            this.BackColor = Color.Brown;

            // Controls

            // Position display
            var margin = this.Width * MARGIN_SIZE_RATIO;
            this.posDisplay = new PictureBox
            {
                Size = new Size((int)(this.Width - margin * 2.0f), (int)(this.Width - margin * 2.0f)),
                Location = new Point((int)margin, (int)margin),
                BackColor = Color.Green,
                SizeMode = PictureBoxSizeMode.Normal,
                Anchor = AnchorStyles.None,
            };
            AdjustPosDisplay();
            this.Controls.Add(this.posDisplay);

            // Coordinate labels
            this.XCoordLabels = (from i in Enumerable.Range(0, Position.BOARD_SIZE)
                                 select new TransparentLabel
                                 {
                                     Text = ((char)('A' + i)).ToString(),
                                     TextAlign = ContentAlignment.MiddleCenter,
                                     BackColor = Color.Transparent
                                 }).ToArray();

            this.YCoordLabels = (from i in Enumerable.Range(0, Position.BOARD_SIZE)
                                 select new TransparentLabel
                                 {
                                     Text = (i + 1).ToString(),
                                     TextAlign = ContentAlignment.MiddleCenter,
                                     BackColor = Color.Transparent
                                 }).ToArray();
            AdjustCoordLabels();
            this.Controls.AddRange(this.XCoordLabels);
            this.Controls.AddRange(this.YCoordLabels);

            // Events
            this.SizeChanged += (s, e) => { this.Height = this.Width;  Invalidate(); };
            this.Paint += PositionViewer_Paint;
            this.posDisplay.MouseClick += PosDisplay_MouseClick;

            this.posDisplay.Paint += PosDisplay_Paint;
        }

        void PositionViewer_Paint(object sender, PaintEventArgs e)
        {
            if(this.backgroundImage is not null)
                e.Graphics.DrawImage(this.backgroundImage, 0, 0); 
            AdjustPosDisplay(); 
            AdjustCoordLabels(); 
            this.posDisplay.Invalidate();
        }

        void PosDisplay_Paint(object sender, PaintEventArgs e)
        {
            if(this.boardBackgroundImage is not null)
                e.Graphics.DrawImage(this.boardBackgroundImage, 0, 0);
            DrawGrid(e);

            if (!this.showMoveHistory)
            {
                DrawDiscs(e);
                if (this.showLegalMovePointers)
                    DrawLegalMovePointers(e);
            }
            else
                DrawMoveHistory(e);
        }

        void AdjustPosDisplay()
        {
            var margin = this.Width * MARGIN_SIZE_RATIO;
            this.posDisplay.Size = new Size((int)(this.Width - margin * 2.0f), (int)(this.Width - margin * 2.0f));
            this.posDisplay.Location = new Point((int)margin, (int)margin);
        }

        void AdjustCoordLabels()
        {
            var margin = this.Width * MARGIN_SIZE_RATIO;
            var gridSize = (float)this.posDisplay.Width / Position.BOARD_SIZE;
            var startCoord = (gridSize + margin) * 0.5f; 
            var labelSize = gridSize * 0.333f;
            var fontSize = gridSize * COORD_SIZE_RATIO_TO_GRID_SIZE;

            for (var i = 0; i < Position.BOARD_SIZE; i++)
            {
                var coord = gridSize * i + startCoord;

                var label = this.XCoordLabels[i];
                label.ForeColor = this.coordLabelTextColor;
                label.Size = new Size((int)margin, (int)margin);
                label.Location = new Point((int)coord, 0);
                label.Font = new Font(this.coordFontFamily, fontSize, GraphicsUnit.Pixel);

                label = this.YCoordLabels[i];
                label.ForeColor = this.coordLabelTextColor;
                label.Size = new Size((int)margin, (int)margin);
                label.Location = new Point(0, (int)coord);
                label.Font = new Font(this.coordFontFamily, fontSize, GraphicsUnit.Pixel);
            }
        }

        void DrawGrid(PaintEventArgs e)
        {
            var graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.HighQuality;

            var pen = new Pen(this.gridColor);
            var lineLen = (float)this.posDisplay.Width;
            var gridSize = lineLen / Position.BOARD_SIZE;
            for (var i = 1; i < Position.BOARD_SIZE; i++)
            {
                var coord = gridSize * i;
                graph.DrawLine(pen, coord, 0.0f, coord, lineLen);
                graph.DrawLine(pen, 0.0f, coord, lineLen, coord);
            }
        }

        void DrawDiscs(PaintEventArgs e)
        {
            var graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.HighQuality;

            var blackDiscBrush = new SolidBrush(this.blackDiscColor);
            var whiteDiscBrush = new SolidBrush(this.whiteDiscColor);
            var gridSize = (float)this.posDisplay.Width / Position.BOARD_SIZE;
            var discSize = gridSize * DISC_SIZE_RATIO_TO_GRID_SIZE;
            var margin = (gridSize - discSize) * 0.5f;
            for (var i = 0; i < Position.SQUARE_NUM; i++)
            {
                var disc = this.pos[(BoardCoordinate)i];
                if (disc == DiscColor.Empty)
                    continue;

                var x = (i % Position.BOARD_SIZE) * gridSize + margin;
                var y = (i / Position.BOARD_SIZE) * gridSize + margin;
                if (disc == DiscColor.Black)
                    graph.FillEllipse(blackDiscBrush, x, y, discSize, discSize);
                else
                    graph.FillEllipse(whiteDiscBrush, x, y, discSize, discSize);
            }
        }

        void DrawLegalMovePointers(PaintEventArgs e)
        {
            var graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.HighQuality;

            var pen = new Pen(this.legalMovePointerColor, this.legalMovePointerThickness);
            var gridSize = (float)this.posDisplay.Width / Position.BOARD_SIZE;
            var discSize = gridSize * DISC_SIZE_RATIO_TO_GRID_SIZE;
            var margin = (gridSize - discSize) * 0.5f;
            foreach (var coord in this.pos.GetNextMoves())
            {
                var x = ((int)coord % Position.BOARD_SIZE) * gridSize + margin;
                var y = ((int)coord / Position.BOARD_SIZE) * gridSize + margin;
                graph.DrawEllipse(pen, x, y, discSize, discSize);
            }
        }

        void DrawMoveHistory(PaintEventArgs e)
        {
            var graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.HighQuality;
            graph.TextRenderingHint = TextRenderingHint.AntiAlias;

            var blackDiscBrush = new SolidBrush(this.blackDiscColor);
            var whiteDiscBrush = new SolidBrush(this.whiteDiscColor);
            var gridSize = (float)this.posDisplay.Width / Position.BOARD_SIZE;
            var discSize = gridSize * DISC_SIZE_RATIO_TO_GRID_SIZE;
            var margin = (gridSize - discSize) * 0.5f;
            var font = new Font(this.moveNumFontFamily, discSize * MOVE_NUM_RATIO_TO_DISC_SIZE, GraphicsUnit.Pixel);
            var blackMoveNumBrush = new SolidBrush(this.blackMoveNumTextColor);
            var whiteMoveNumBrush = new SolidBrush(this.whiteMoveNumTextColor);
            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };

            var moveNum = 0;
            foreach(var move in this.pos.MoveHistroy)
            {
                if (move.Coord == BoardCoordinate.Pass)
                    continue;

                (var brush, var moveNumBrush) = (move.Player == DiscColor.Black)
                    ? (blackDiscBrush, blackMoveNumBrush)
                    : (whiteDiscBrush, whiteMoveNumBrush);
                var gridX = ((int)move.Coord % Position.BOARD_SIZE) * gridSize;
                var gridY = ((int)move.Coord / Position.BOARD_SIZE) * gridSize;
                var x = gridX + margin;
                var y = gridY + margin;
                graph.FillEllipse(brush, x, y, discSize, discSize);

                var rect = new RectangleF(x, y, discSize, discSize);
                graph.DrawString((++moveNum).ToString(), font, moveNumBrush, rect, format);
            }

            foreach(var loc in Position.CrossCoordinates)
            {
                var brush = (loc.color == DiscColor.Black) ? blackDiscBrush : whiteDiscBrush;
                var x = ((int)loc.coord % Position.BOARD_SIZE) * gridSize + margin;
                var y = ((int)loc.coord / Position.BOARD_SIZE) * gridSize + margin;
                graph.FillEllipse(brush, x, y, discSize, discSize);
            }
        }
    }
}
