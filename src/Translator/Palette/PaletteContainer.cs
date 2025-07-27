using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Translator.Interfaces;

namespace Translator.Palette
{
    public class PaletteContainer : IPaletteContainer
    {
        private Dictionary<string, IPaletteBrush> m_brushLookup = 
            new Dictionary<string, IPaletteBrush>();

        public IPaletteBrush this[string brushName]
        {
            get
            {
                if (m_brushLookup.ContainsKey(brushName))
                    return m_brushLookup[brushName];
                return null;
            }
        }

        public IEnumerable<IPaletteBrush> Brushes
        {
            get 
            { 
                foreach(IPaletteBrush brush in m_brushLookup.Values)
                {
                    yield return brush;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteContainer"/> class.
        /// </summary>
        public PaletteContainer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteContainer"/> class.
        /// </summary>
        /// <param name="container"></param>
        public PaletteContainer(IPaletteContainer container)
        {
            if (container != null)
            {
                foreach (IPaletteBrush brush in container.Brushes)
                {
                    AddBrush(new PaletteBrush(brush));
                }
            }
        }

        /// <summary>
        /// Loads current resources
        /// </summary>
        public PaletteContainer(ResourceDictionary resourceDictionary)
        {
            if (resourceDictionary != null)
            {
                foreach (string key in resourceDictionary.Keys)
                {
                    if (resourceDictionary.TryGetValue(key, out SolidColorBrush brush))
                    {
                        AddBrush(new PaletteBrush(key, brush.ToString()));
                    }
                }
            }
        }

        /// <summary>
        /// Adds the specified row.
        /// </summary>
        /// <param name="brush">The row.</param>
        public void AddBrush(PaletteBrush brush)
        {
            if (brush == null) 
                return;
            m_brushLookup[brush.BrushName] = brush;
        }
    }
}
