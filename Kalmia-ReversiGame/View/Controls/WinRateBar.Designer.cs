using System.Drawing;
using System.Windows.Forms;

namespace Kalmia_Game.View.Controls
{
    partial class WinRateBar
    {
        private System.ComponentModel.IContainer components = null;

        public Color BlackWinRateTextColor
        { 
            get => this.blackWinRateLabel.ForeColor; 
            set => this.blackWinRateLabel.ForeColor = value; 
        }

        public Color WhiteWinRateTextColor
        {
            get => this.whiteWinRateLabel.ForeColor;
            set => this.whiteWinRateLabel.ForeColor = value;
        }

        public Color BlackBarColor
        {
            get => this.winRateBar.ForeColor;
            set => this.winRateBar.ForeColor = value;
        }

        public Color WhiteBarColor
        {
            get => this.winRateBar.BackColor;
            set => this.winRateBar.BackColor = value;
        }

        public Font WinRateFont { get => this.blackWinRateLabel.Font; set { this.blackWinRateLabel.Font = this.whiteWinRateLabel.Font = value; } }

        public int BlackWinRate 
        { 
            get => this.winRateBar.Value;

            set 
            { 
                this.winRateBar.Value = value;
                this.blackWinRateLabel.Text = $"{value}%";
                this.whiteWinRateLabel.Text = $"{(100 - value)}%";
            } 
        }

        public int WhiteWinRate 
        { 
            get => 100 - this.winRateBar.Value;

            set 
            { 
                this.winRateBar.Value = 100 - value;
                this.blackWinRateLabel.Text = $"{(100 - value)}%";
                this.whiteWinRateLabel.Text = $"{value}%";
            }
        }

        const float BAR_RATIO = 0.8f;

        TransparentLabel blackWinRateLabel;
        TransparentLabel whiteWinRateLabel;
        ProgressBar winRateBar;

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
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

            // Win rate label
            this.blackWinRateLabel = new TransparentLabel
            {
                Text = "50%",
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(this.blackWinRateLabel);

            this.whiteWinRateLabel = new TransparentLabel
            {
                Text = "50%",
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(this.whiteWinRateLabel);

            // Win rate bar
            this.winRateBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                Style = ProgressBarStyle.Continuous
            };
            this.Controls.Add(this.winRateBar);

            AdjustWinRateLabel();
            AdjustWinRateBar();

            // Events
            this.SizeChanged += (s, e) => { AdjustWinRateLabel(); AdjustWinRateBar(); };
        }

        void AdjustWinRateLabel()
        {
            var width = this.Width * (1.0f - BAR_RATIO) * 0.5f;
            this.blackWinRateLabel.Size = this.whiteWinRateLabel.Size = new Size((int)width, this.Height);
            this.blackWinRateLabel.Location = new Point(0, 0);
            this.whiteWinRateLabel.Location = new Point(this.Width - (int)width, 0);
        }

        void AdjustWinRateBar()
        {
            var width = this.Width * BAR_RATIO;
            this.winRateBar.Size = new Size((int)width, this.Height);
            this.winRateBar.Location = new Point(this.blackWinRateLabel.Width, 0);
        }
    }
}
