using Translator.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VTEControls;

namespace Translator.Palette
{
    public class PaletteManager : IPaletteManager
    {
        #region BrushNames
        public const string BrushPropertyGridBackground = "Brush.PropertyGridBackground";
        public const string BrushPropertyGridForeground = "Brush.PropertyGridForeground";
        public const string BrushPropertyGridAccent= "Brush.PropertyGridAccent";
        #endregion

        #region Commands
        private ICommand m_editCommand;
        private ICommand m_resetCommand;
        private ICommand m_importCommand;
        private ICommand m_exportCommand;

        /// <summary>
        /// Edit the palette
        /// </summary>
        public ICommand EditCommand
        {
            get
            {
                return m_editCommand ??
                    (m_editCommand = new CommandHandler(
                        () =>
                        {
                            m_activeController = new DynamicPaletteController(this, m_resourceDictionary);
                            m_activeController.Owner = Application.Current.MainWindow;
                            m_activeController.PaletteBrushChanged += (s, e) =>
                            {
                                if (m_resourceDictionary.TryGetValue(e.BrushName, out SolidColorBrush currentBrush))
                                {
                                    SetResource(e.BrushName, currentBrush, e.Brush);
                                }
                            };

                            m_activeController.Closed += (s, e) =>
                            {
                                m_activeController = null;
                            };
                            m_activeController.Show();
                        },
                        () =>
                        {
                            return m_activeController == null;
                        }));
            }
        }

        /// <summary>
        /// Resets a palette to default
        /// </summary>
        public ICommand ResetCommand
        {
            get
            {
                return m_resetCommand ??
                    (m_resetCommand = new CommandHandler(
                    () =>
                    {
                        if (MessageBox.Show("Are you sure you want to reset all colors to default?", "Reset Palette", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            SwapPaletteTo(m_defaultPaletteContainer);
                        }
                    },
                    () =>
                    {
                        return m_activeController == null;
                    }));
            }
        }

        /// <summary>
        /// Imports and applies a palette
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
                            SwapPaletteTo(PaletteFileGeneration.ParsePaletteFile(ofd.FileName));
                        }
                    },
                    () =>
                    {
                        return m_activeController == null;
                    }));
            }
        }

        /// <summary>
        /// Exports the currently applied palette
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
                            PaletteFileGeneration.GeneratePaletteFile(dlg.FileName, new PaletteContainer(m_resourceDictionary));
                        }
                    },
                    () =>
                    {
                        return m_activeController == null;
                    }));
            }
        }
        #endregion

        /// <summary>
        /// Palette changed event
        /// </summary>
        public event EventHandler<PaletteChangedEventArgs> PaletteBrushChanged;

        /// <summary>
        /// Palette brush collection
        /// </summary>
        private PaletteContainer m_defaultPaletteContainer;

        /// <summary>
        /// The currently active controller
        /// </summary>
        private DynamicPaletteController m_activeController;

        /// <summary>
        /// The resource Dictionary;
        /// </summary>
        private ResourceDictionary m_resourceDictionary;

        /// <summary>
        /// Current pallete brushes
        /// </summary>
        private HashSet<IPaletteBrush> m_activePalette = new HashSet<IPaletteBrush>();

        /// <summary>
        /// Gets the current brushes of the palette
        /// </summary>
        public IEnumerable<IPaletteBrush> PaletteBrushes
        {
            get { return m_activePalette; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PaletteManager()
        {
            m_resourceDictionary = Application.Current.Resources;
            m_defaultPaletteContainer = new PaletteContainer(m_resourceDictionary);
        }

        /// <summary>
        /// Close the active controller on closing
        /// </summary>
        public void OnClosing()
        {
            m_activeController?.Close();
        }

        /// <summary>
        /// Set palette changed for all current values post build
        /// </summary>
        public void PostBuild()
        {
            foreach (string key in m_resourceDictionary.Keys)
            {
                SolidColorBrush currentBrush = m_resourceDictionary[key] as SolidColorBrush;
                if (currentBrush != null)
                {
                    PaletteBrushChanged?.Invoke(this, new PaletteChangedEventArgs(key, currentBrush));
                }
            }
        }

        /// <summary>
        /// Gets a brush value from default container
        /// </summary>
        /// <param name="brushName"></param>
        /// <returns></returns>
        public IPaletteBrush GetDefaultBrush(string brushName)
        {
            return m_defaultPaletteContainer[brushName];
        }

        /// <summary>
        /// Swaps the current palette to a new one specified by a palette brush collection.
        /// </summary>
        /// <param name="paletteContainer">The brush collection.</param>
        /// <param name="resourceDictionary">A resource dictionary to update.</param>
        public void SwapPaletteTo(IPaletteContainer paletteContainer)
        {
            if (paletteContainer == null) return;

            m_activePalette.Clear();
            foreach (IPaletteBrush paletteBrush in paletteContainer.Brushes)
            {
                if (m_resourceDictionary.TryGetValue(paletteBrush.BrushName, out SolidColorBrush currentBrush))
                {
                    SetResource(paletteBrush.BrushName, currentBrush, paletteBrush.Brush);
                    m_activePalette.Add(paletteBrush);
                }
            }
        }

        /// <summary>
        /// Sets the resource to the new brush
        /// </summary>
        /// <param name="brushName"></param>
        /// <param name="currentBrush"></param>
        /// <param name="targetBrush"></param>
        private void SetResource(string brushName, SolidColorBrush currentBrush, SolidColorBrush targetBrush)
        {
            if (currentBrush == null || targetBrush == null) 
                return;

            if (currentBrush.Color.DiffersFrom(targetBrush.Color))
            {
                m_resourceDictionary[brushName] = targetBrush;
                PaletteBrushChanged?.Invoke(this, new PaletteChangedEventArgs(brushName, targetBrush));
            }
        }
    }
}
