using Translator.Interfaces;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VTEControls;
using VTEControls.WPF;

namespace Translator.Palette
{
    /// <summary>
    /// Interaction logic for DynamicPaletteRow.xaml
    /// </summary>
    public partial class DynamicPaletteBrush : UserControlExtension, IPaletteBrush
    {
        #region Commands
        /// <summary>
        /// Resets the brush
        /// </summary>
        private ICommand m_resetBrush;

        /// <summary>
        /// Resets the brush to default
        /// </summary>
        private ICommand m_defaultResetBrush;

        /// <summary>
        /// Resets the current brush
        /// </summary>
        public ICommand ResetBrush
        {
            get
            {
                return m_resetBrush ?? 
                    (m_resetBrush = new CommandHandler(() =>
                {
                    Brush = m_brushOnLoad;
                },
                () =>
                {
                    return m_brushOnLoad != null;
                }));
            }
        }

        public ICommand DefaultResetBrush
        {
            get
            {
                return m_defaultResetBrush ??
                    (m_defaultResetBrush = new CommandHandler(() =>
                    {
                        Brush = m_defaultBrush.Brush;
                    },
                () =>
                {
                    return m_defaultBrush != null;
                }));
            }
        }
        #endregion

        /// <summary>
        /// Occurs when [swatch updated].
        /// </summary>
        public event EventHandler<EventArgs> BrushValueChanged;

        /// <summary>
        /// The name of the brush.
        /// </summary>
        private string m_brushName;

        /// <summary>
        /// The current brush.
        /// </summary>
        private SolidColorBrush m_currentBrush;

        /// <summary>
        /// The default brush.
        /// </summary>
        private SolidColorBrush m_brushOnLoad;

        /// <summary>
        /// The default brush.
        /// </summary>
        private IPaletteBrush m_defaultBrush;

        /// <summary>
        /// Gets the name of the brush.
        /// </summary>
        /// <value>
        /// The name of the brush.
        /// </value>
        public string BrushName
        {
            get { return m_brushName; }
            private set { SetProperty(GetPropertyName(), ref m_brushName, value); } 
        }

        /// <summary>
        /// Gets the brush.
        /// </summary>
        /// <value>
        /// The brush.
        /// </value>
        public SolidColorBrush Brush
        {
            get { return m_currentBrush; }
            private set
            {
                if (SetProperty(GetPropertyName(), ref m_currentBrush, value))
                {
                    HandleCurrentBrushChanged();
                }
            }
        }

        /// <summary>
        /// The text value of the brush
        /// </summary>
        public string Value
        {
            get { return m_currentBrush?.Color.ToString(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPaletteBrush"/> class.
        /// </summary>
        /// <param name="brushName">Name of the brush.</param>
        /// <param name="brush">The brush.</param>
        public DynamicPaletteBrush(string brushName, SolidColorBrush brush, IPaletteBrush defaultBrush)
        {
            // save default value
            m_defaultBrush = defaultBrush;

            // save current value
            m_brushOnLoad = brush;

            // Initialize the component
            InitializeComponent();

            // set the brush name
            BrushName = brushName;
            
            // set the brush
            Brush = brush;
        }

        /// <summary>
        /// Handles the Click event of the BrushSwatchButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BrushSwatchButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker.HexadecimalString = BrushValueText.Text;
            BrushSwatchPopup.IsOpen = true;
        }

        /// <summary>
        /// Handles the LostFocus event of the BrushValueText control, which results in a swatch update.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BrushValueText_LostFocus(object sender, RoutedEventArgs e)
        {
            var possibleNewBrushValue = BrushValueText.Text;
            try
            {
                Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(possibleNewBrushValue));
            }
            catch (Exception error)
            {
                // Reset the displayed text to the current brush color value.
                BrushValueText.Text = m_currentBrush.Color.ToString();
                MessageBox.Show(possibleNewBrushValue + " is not a valid color string in the format of #XXXXXXXX." + "\n" + error.Message, "Brush Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the SelectedColorChanged event of the ColorPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The instance containing the event data.</param>
        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Color newColor = (Color)ColorConverter.ConvertFromString(ColorPicker.HexadecimalString);
            if (newColor.DiffersFrom(m_currentBrush.Color))
                Brush = new SolidColorBrush(newColor);
        }

        /// <summary>
        /// Handle brush property change
        /// </summary>
        private void HandleCurrentBrushChanged()
        {
            BrushValueText.Text = m_currentBrush.Color.ToString();

            if (m_isLoaded)
                BrushValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
