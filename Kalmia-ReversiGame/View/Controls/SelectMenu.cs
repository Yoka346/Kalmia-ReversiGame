using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kalmia_Game.View.Controls
{
    public delegate void SelectMenuEventHandler<T>(SelectMenu<T> sender, int selectedIdx);

    /// <summary>
    /// 選択メニュー. 選択項目の表示及びユーザーからの入力をイベントで通知する役割を受け持つ.
    /// </summary>
    /// <typeparam name="T">選択項目の型</typeparam>
    public partial class SelectMenu<T> : UserControl
    {
        public int SelectedIdx { get; private set; } = -1;
        public T? SelectedItem { get { return (this.SelectedIdx != -1) ? this.items[this.SelectedIdx] : default(T); } }

        public ReadOnlyCollection<T> Items => new(this.items);

        public Color SelectedTextColor
        {
            get => this.selectedTextColor;

            set
            {
                this.selectedTextColor = value;
                InitLabelColors();
            }
        }

        public Color NotSelectedTextColor
        {
            get => this.notSelectedTextColor;

            set
            {
                this.notSelectedTextColor = value;
                InitLabelColors();
            }
        }

        public event SelectMenuEventHandler<T> OnLeftClickItem = delegate { };
        public event SelectMenuEventHandler<T> OnSelectedIdxChanged = delegate { };

        List<T> items;

        Color selectedTextColor = Color.FromArgb(255, Color.White);
        Color notSelectedTextColor = Color.FromArgb(128, Color.White);
        Font? font;
        string fontFamily;
        int selectBoxHeight;
        int stringHeight;
        List<TransparentLabel> itemLabels;

        public SelectMenu(int width, int height, int x, int y)
            : this(width, height, x, y, GlobalConfig.Instance.DefaultFontFamily) { }

        public SelectMenu(int width, int height, int x, int y, string fontFamily)
        {
            InitializeComponent();
            this.BackColor = Color.Transparent;
            this.Location = new Point(x, y);
            this.Size = new Size(width, height);
            this.fontFamily = fontFamily;
            this.itemLabels = new List<TransparentLabel>();
            this.items = new List<T>();
        }

        public void AddItem(T item)
        {
            this.items.Add(item);
            InitLabels();
        }

        public void AddItemRange(IEnumerable<T> items)
        {
            this.items.AddRange(items);
            InitLabels();
        }

        public void RemoveItem(T item)
        {
            this.items.Remove(item);
            InitLabels();
        }

        void InitLabels()
        {
            for (var i = 0; i < this.itemLabels.Count; i++)
            {
                this.Controls.Remove(this.itemLabels[i]);
                this.itemLabels.RemoveAt(i);
            }

            this.itemLabels.Clear();
            this.selectBoxHeight = this.Height / this.items.Count;
            this.stringHeight = (int)(this.selectBoxHeight * 0.8f);
            this.font = new Font(this.fontFamily, this.stringHeight, GraphicsUnit.Pixel);
            for (var i = 0; i < this.items.Count; i++)
            {
                var y = this.selectBoxHeight * i;
                var label = new TransparentLabel
                {
                    Width = this.Width,
                    Height = this.selectBoxHeight,
                    Location = new Point(0, y),
                    Font = this.font,
                    ForeColor = this.notSelectedTextColor,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Text = this.items[i]?.ToString()
                };
                label.MouseEnter += Label_MouseEnter;
                label.MouseLeave += Label_MouseLeave;
                label.MouseClick += Label_MouseClick;
                this.Controls.Add(label);
                this.itemLabels.Add(label);
            }
            Invalidate();
        }

        void InitLabelColors()
        {
            for (var i = 0; i < this.itemLabels.Count; i++)
                this.itemLabels[i].ForeColor = (i == this.SelectedIdx) ? this.selectedTextColor : this.notSelectedTextColor;
        }

        void Label_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is null)
                return;

            var label = (TransparentLabel)sender;
            this.SelectedIdx = this.itemLabels.IndexOf(label);
            this.OnSelectedIdxChanged.Invoke(this, this.SelectedIdx);
            InitLabelColors();
            Refresh();
        }

        void Label_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is null)
                return;

            var label = (Label)sender;
            this.SelectedIdx = -1;
            this.OnSelectedIdxChanged.Invoke(this, this.SelectedIdx);
            InitLabelColors();
            Refresh();
        }

        void Label_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.OnLeftClickItem.Invoke(this, this.SelectedIdx);
        }
    }
}
