using System.Collections.Generic;
using System.Windows.Media;

namespace Translator.Interfaces
{
    public interface IPaletteContainer
    {
        /// <summary>
        /// List of brushes
        /// </summary>
        IEnumerable<IPaletteBrush> Brushes { get; }
    }

    public interface IPaletteBrush
    {
        /// <summary>
        /// The brush name
        /// </summary>
        string BrushName { get; }

        /// <summary>
        /// the brush value
        /// </summary>
        string Value { get; }

        /// <summary>
        /// The solid color brush
        /// </summary>
        SolidColorBrush Brush { get; }
    }
}
