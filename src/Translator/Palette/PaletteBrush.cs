using System.Windows.Media;
using Translator.Interfaces;
using VTEControls;

namespace Translator.Palette
{
    /// <summary>
    /// Describes a palette brush, composed of a name and SolidColorBrush.
    /// </summary>
    public class PaletteBrush : PropertyHandler, IPaletteBrush
    {
        /// <summary>
        /// The brush name
        /// </summary>
        private string m_name;
        /// <summary>
        /// The brush value
        /// </summary>
        private string m_value;
        /// <summary>
        /// The brush based on the value
        /// </summary>
        private SolidColorBrush m_brush;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string BrushName
        {
            get { return m_name; }
            set { SetProperty(GetPropertyName(), ref m_name, value); }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            get { return m_value; }
            set
            {
                if (SetProperty(GetPropertyName(), ref m_value, value))
                {
                    HandleBrushValueChanged();
                }
            }
        }

        /// <summary>
        /// Gets the brush.
        /// </summary>
        /// <value>
        /// The brush.
        /// </value>
        public SolidColorBrush Brush
        {
            get { return m_brush; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteBrush"/> class.
        /// </summary>
        public PaletteBrush()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteBrush"/> class.
        /// </summary>
        /// <param name="brush"></param>
        public PaletteBrush(IPaletteBrush brush) 
            : this(brush.BrushName, brush.Value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteBrush"/> class.
        /// </summary>
        /// <param name="brush"></param>
        public PaletteBrush(string name, string value)
        {
            BrushName = name;
            Value = value;
        }

        private void HandleBrushValueChanged()
        {
            m_brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(m_value));
        }
    }
}
