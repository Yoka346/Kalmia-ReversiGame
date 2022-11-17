using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Kalmia_Game.View.Controls
{
    partial class WinRateFigure
    {
        private System.ComponentModel.IContainer components = null;

        public int FigureLineWidth { get; set; } = 3;
        public Color FigureColor { get; set; } = Color.Yellow;
        public Color EvenLineColor { get; set; } = Color.White;
        public Color EndBarColor { get; set; } = Color.Red;

        public override Font Font { get => base.Font; set { this.blackLabel.Font = this.whiteLabel.Font = base.Font = value; } }

        TransparentLabel blackLabel;
        TransparentLabel whiteLabel;
        PictureBox figureDisplay;

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

            // Controls
            (var labelWidth, var labelHeight) = (this.Width / 10, this.Height / 2);
            this.blackLabel = new TransparentLabel
            {
                Text = "黒",
                Size = new Size(labelWidth, labelHeight),
                Location = new Point(0, 0),
                Font = this.Font,
                ForeColor = Color.White,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(this.blackLabel);

            this.whiteLabel = new TransparentLabel
            {
                Text = "白",
                Size = new Size(labelWidth, labelHeight),
                Location = new Point(0, this.blackLabel.Height),
                Font = this.Font,
                ForeColor = Color.Black,
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(this.whiteLabel);

            this.figureDisplay = new PictureBox
            {
                Size = new Size(this.Width - labelWidth, this.Height),
                Location = new Point(labelWidth, 0),
            };
            this.Controls.Add(this.figureDisplay);

            // Events
            this.SizeChanged += WinRateGraph_SizeChanged;
            this.figureDisplay.Paint += FigureDisplay_Paint;
        }

        void WinRateGraph_SizeChanged(object sender, EventArgs e)
        {
            (var labelWidth, var labelHeight) = (this.Width / 10, this.Height / 2);
            this.blackLabel.Size = this.whiteLabel.Size = new Size(labelWidth, labelHeight);
            this.blackLabel.Location = new Point(0, 0);
            this.whiteLabel.Location = new Point(0, this.blackLabel.Height);
            this.figureDisplay.Size = new Size(this.Width - labelWidth, this.Height);
            this.figureDisplay.Location = new Point(labelWidth, 0);
        }

        void FigureDisplay_Paint(object sender, PaintEventArgs e)
        {
            (var width, var height) = (this.figureDisplay.Width, this.figureDisplay.Height);
            var graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.HighQuality;
            graph.Clear(this.BackColor);
            graph.DrawLine(new Pen(this.EvenLineColor), 0.0f, height * 0.5f, width, height * 0.5f);

            if (this.blackWinRates.Count < 2)
                return;

            var plotWidth = width * 0.0125f;
            var points = new List<PointF>();
            for (var i = 0; i < this.blackWinRates.Count; i++)
                points.Add(new PointF(plotWidth * i, height * (100.0f - this.blackWinRates[i]) * 0.01f));

            graph.DrawLines(new Pen(this.FigureColor, this.FigureLineWidth), points.ToArray());
            graph.DrawLine(new Pen(this.EndBarColor), points[^1].X, 0.0f, points[^1].X, height);
        }
    }
}
