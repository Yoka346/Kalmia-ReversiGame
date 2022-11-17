using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Kalmia_Game.View.Controls
{
    /// <summary>
    /// 透明な文字の描画が可能なラベル.
    /// </summary>
    internal class TransparentLabel : Label
    {
        public TransparentLabel() { }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graph = e.Graphics;
            graph.TextRenderingHint = TextRenderingHint.AntiAlias;
            graph.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor), new RectangleF(new PointF(0.0f, 0.0f), this.Size), ContentAlignmentToStringFormat());
        }

        StringFormat ContentAlignmentToStringFormat()
        {
            return this.TextAlign switch
            {
                ContentAlignment.TopLeft => new StringFormat { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near },
                ContentAlignment.TopCenter => new StringFormat { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center },
                ContentAlignment.TopRight => new StringFormat { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Far },
                ContentAlignment.MiddleLeft => new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near },
                ContentAlignment.MiddleCenter => new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center },
                ContentAlignment.MiddleRight => new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far },
                ContentAlignment.BottomLeft => new StringFormat { LineAlignment = StringAlignment.Far, Alignment = StringAlignment.Near },
                ContentAlignment.BottomCenter => new StringFormat { LineAlignment = StringAlignment.Far, Alignment = StringAlignment.Center },
                ContentAlignment.BottomRight => new StringFormat { LineAlignment = StringAlignment.Far, Alignment = StringAlignment.Far },
                _ => new()
            };
        }
    }
}
