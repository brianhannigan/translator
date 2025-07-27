using System;
using System.Windows.Media;

namespace Translator.Palette
{
    public class PaletteChangedEventArgs : EventArgs
    {
        public string BrushName { get; private set; }
        public SolidColorBrush Brush { get; private set; }
        public PaletteChangedEventArgs(string brushName, SolidColorBrush brush)
        {
            BrushName = brushName;
            Brush = brush;
        }
    }
}
