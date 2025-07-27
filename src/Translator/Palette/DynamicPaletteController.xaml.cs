using Translator.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VTEControls;
using VTEControls.WPF;

namespace Translator.Palette
{
    /// <summary>
    /// Interaction logic for DynamicPaletteController.xaml
    /// </summary>
    public partial class DynamicPaletteController : WindowExtension, IPaletteContainer
    {
        #region Commands
        private ICommand m_importCommand;
        private ICommand m_exportCommand;
        private ICommand m_resetCommand;
        private ICommand m_cancelCommand;
        private ICommand m_applyCommand;

        /// <summary>
        /// Imports a palette
        /// </summary>
        public ICommand ImportCommand
        {
            get
            {
                return m_importCommand ??
                    (m_importCommand = new CommandHandler(
                    () =>
                    {
                        OpenFileDialog ofd = new OpenFileDialog()
                        {
                            Filter = "Plt Files (*.plt) | *.plt"
                        };

                        if (ofd.ShowDialog() == true)
                        {
                            IPaletteContainer palette = PaletteFileGeneration.ParsePaletteFile(ofd.FileName);
                            if (palette != null)
                            {
                                LoadBrushesFromPaletteCollection(palette);
                                m_paletteManager?.SwapPaletteTo(this);
                            }
                        }
                    },
                    () =>
                    {
                        return m_paletteManager != null;
                    }));
            }
        }

        /// <summary>
        /// Exports a palette
        /// </summary>
        public ICommand ExportCommand
        {
            get
            {
                return m_exportCommand ??
                    (m_exportCommand = new CommandHandler(
                    () =>
                    {
                        SaveFileDialog dlg = new SaveFileDialog
                        {
                            Filter = "Plt Files (*.plt)|*.plt|All files (*.*)|*.*"
                        };

                        if (dlg.ShowDialog() == true)
                        {
                            if (File.Exists(dlg.FileName))
                            {
                                File.Delete(dlg.FileName);
                            }
                            PaletteFileGeneration.GeneratePaletteFile(dlg.FileName, new PaletteContainer(this));
                        }
                    },
                    () =>
                    {
                        return m_paletteManager != null;
                    }));
            }
        }
        /// <summary>
        /// Resets the current palette 
        /// </summary>
        public ICommand ResetCommand
        {
            get
            {
                return m_resetCommand ??
                    (m_resetCommand = new CommandHandler(
                    () =>
                    {
                        LoadBrushesFromPaletteCollection(m_originalPaletteContainer);
                        m_paletteManager?.SwapPaletteTo(m_originalPaletteContainer);
                    },
                    () =>
                    {
                        return m_paletteManager != null;
                    }));
            }
        }

        /// <summary>
        /// Resets the current palette 
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                return m_cancelCommand ??
                    (m_cancelCommand = new CommandHandler(
                    () =>
                    {
                        m_paletteManager?.SwapPaletteTo(m_originalPaletteContainer);
                        Close();
                    },
                    () =>
                    {
                        return m_paletteManager != null;
                    }));
            }
        }

        /// <summary>
        /// Applies the current palette
        /// </summary>
        public ICommand ApplyCommand
        {
            get
            {
                return m_applyCommand ??
                    (m_applyCommand = new CommandHandler(
                    () =>
                    {
                        m_paletteManager?.SwapPaletteTo(this);
                        m_applied = true;
                        Close();
                    },
                    () =>
                    {
                        return m_paletteManager != null;
                    }));
            }
        }
        #endregion

        public event EventHandler<PaletteChangedEventArgs> PaletteBrushChanged;

        /// <summary>
        /// The original palette settings stored as a palette brush collection.
        /// </summary>
        private PaletteContainer m_originalPaletteContainer;

        /// <summary>
        /// The palette brushes
        /// </summary>
        private ObservableCollection<DynamicPaletteBrush> m_paletteBrushes;

        /// <summary>
        /// The palette manager
        /// </summary>
        private IPaletteManager m_paletteManager;

        /// <summary>
        /// If the palette has been applied
        /// </summary>
        private bool m_applied = false;

        /// <summary>
        /// The list of palette brushes
        /// </summary>
        public ObservableCollection<DynamicPaletteBrush> Brushes
        {
            get { return m_paletteBrushes; }
            set { SetProperty(GetPropertyName(), ref m_paletteBrushes, value); }
        }

        #region IPaletteContainer
        IEnumerable<IPaletteBrush> IPaletteContainer.Brushes
        {
            get { return m_paletteBrushes; }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPaletteController" /> class.
        /// </summary>
        /// <param name="paletteManager">The palette manager.</param>
        /// <param name="resourceDictionary">The resource dictionary.</param>
        public DynamicPaletteController(IPaletteManager paletteManager, ResourceDictionary resourceDictionary)
        {
            // store the palette manager
            m_paletteManager = paletteManager;

            // Initialize the component
            InitializeComponent();

            // Update the brushes based on the resource dictionary.
            UpdateBrushes(resourceDictionary);
        }

        /// <summary>
        /// Handles closing event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!m_applied)
                m_paletteManager?.SwapPaletteTo(m_originalPaletteContainer);
        }

        /// <summary>
        /// Loads the brushes from a palette brush collection.
        /// </summary>
        /// <param name="collection">The palette brush collection.</param>
        private void LoadBrushesFromPaletteCollection(IPaletteContainer collection)
        {
            Brushes = new ObservableCollection<DynamicPaletteBrush>();
            foreach (IPaletteBrush paletteBrush in collection.Brushes)
            {
                DynamicPaletteBrush newDynBrush = new DynamicPaletteBrush(paletteBrush.BrushName, paletteBrush.Brush, m_paletteManager?.GetDefaultBrush(paletteBrush.BrushName));
                newDynBrush.BrushValueChanged += HandleBrushChanged;
                Brushes.Add(newDynBrush);
            }
        }

        /// <summary>
        /// Updates the palette brushes to match what is currently stored in the application resource dictionary. Will save this as the default configuration if save is set to true.
        /// </summary>
        /// <param name="save">if set to <c>true</c> [save].</param>
        private void UpdateBrushes(ResourceDictionary resources)
        {
            Brushes = new ObservableCollection<DynamicPaletteBrush>();
            if (resources != null)
            {
                foreach (string key in resources.Keys)
                {
                    if (resources.TryGetValue(key, out SolidColorBrush brush))
                    {
                        DynamicPaletteBrush newDynBrush = new DynamicPaletteBrush(key, brush, m_paletteManager?.GetDefaultBrush(key));
                        newDynBrush.BrushValueChanged += HandleBrushChanged;
                        Brushes.Add(newDynBrush);
                    }
                }
            }
            m_originalPaletteContainer = new PaletteContainer(this);
        }

        /// <summary>
        /// Handles the swatch updated event for all attached palette rows.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleBrushChanged(object sender, EventArgs e)
        {
            DynamicPaletteBrush dynBrush = sender as DynamicPaletteBrush;
            if (dynBrush != null)
            {
                PaletteBrushChanged?.Invoke(this, new PaletteChangedEventArgs(dynBrush.BrushName, dynBrush.Brush));
            }
        }
    }
}
