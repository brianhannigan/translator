using System;
using System.Collections.Generic;
using Translator.Palette;

namespace Translator.Interfaces
{
    public interface IPaletteManager
    {
        event EventHandler<PaletteChangedEventArgs> PaletteBrushChanged;
        IEnumerable<IPaletteBrush> PaletteBrushes { get; }
        void SwapPaletteTo(IPaletteContainer paletteContainer);
        IPaletteBrush GetDefaultBrush(string brushName);
    }
}
