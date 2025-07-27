using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Translator.Controls;
using Translator.Interfaces;
using Translator.Managers;
using Translator.Palette;
using Translator.Wrappers;
using TranslatorBackend.Factory;
using TranslatorBackend.Interfaces;
using VTEControls;
using VTEControls.WPF;

namespace Translator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowExtension
    {
        #region Commands
        private ICommand m_errorDisplayCommand;

        public ICommand ErrorDisplayCommand
        {
            get
            {
                return m_errorDisplayCommand ??
                  (m_errorDisplayCommand = new CommandHandler(
                      () =>
                      {
                          if (errorGrid.IsVisible)
                          {
                              HideErrorGrid();
                          }
                          else
                          {
                              ShowErrorGrid();
                          }
                      }));
            }
        }
        #endregion

        DispatcherTimer commandTimer;

        protected readonly int m_MaxLogLength = 100000000;

        /// <summary>
        /// Settings for build
        /// </summary>
        private TranslatorAppSettings m_settings;

        /// <summary>
        /// Manages the palette
        /// </summary>
        private PaletteManager m_paletteManager;

        /// <summary>
        /// The language manager
        /// </summary>
        private LanguageManager m_languageManager;

        /// <summary>
        /// The network manager
        /// </summary>
        private NetworkManager m_networkManager;

        /// <summary>
        /// Manages stored files loading/saving
        /// </summary>
        private StorageFileManager m_storageFileManager;

        /// <summary>
        /// The error manager
        /// </summary>
        private ErrorManager m_errorManager;

        /// <summary>
        /// The server manager
        /// </summary>
        private ServerManager m_serverManager;

        /// <summary>
        /// the tab control for text translation
        /// </summary>
        private TextTranslatorControl m_textTranslatorTab;

        /// <summary>
        /// The tab control for the image translation
        /// </summary>
        private ImageTranslatorControl m_imageTranslatorTab;

        /// <summary>
        /// The tab control for the pdf translation
        /// </summary>
        private PdfTranslatorControl m_pdfTranslatorTab;

        /// <summary>
        /// The backend for translation
        /// </summary>
        private ITranslatorBackend m_backend;

        /// <summary>
        /// Modules
        /// </summary>
        private List<IModule> m_modules = new List<IModule>();

        /// <summary>
        /// Palette Manager 
        /// </summary>
        public PaletteManager PaletteManager
        {
            get { return m_paletteManager; }
            private set { SetProperty(GetPropertyName(), ref m_paletteManager, value); }
        }

        /// <summary>
        /// The network manager
        /// </summary>
        public NetworkManager NetworkManager
        {
            get { return m_networkManager; }
            private set { SetProperty(GetPropertyName(), ref m_networkManager, value); }
        }

        /// <summary>
        /// The language manager
        /// </summary>
        public LanguageManager LanguageManager
        {
            get { return m_languageManager; }
            private set { SetProperty(GetPropertyName(), ref m_languageManager, value); }
        }

        /// <summary>
        /// The error manager
        /// </summary>
        public ErrorManager ErrorManager
        {
            get { return m_errorManager; }
            private set { SetProperty(GetPropertyName(), ref m_errorManager, value); }
        }

        /// <summary>
        /// Storage File manager 
        /// </summary>
        public StorageFileManager StorageFileManager
        {
            get { return m_storageFileManager; }
            private set { SetProperty(GetPropertyName(), ref m_storageFileManager, value); }
        }

        /// <summary>
        /// Storage File manager 
        /// </summary>
        public ServerManager ServerManager
        {
            get { return m_serverManager; }
            private set { SetProperty(GetPropertyName(), ref m_serverManager, value); }
        }

        public bool ShowError
        {
            get
            {
                if (m_errorManager == null)
                    return false;

                // check if theres errors or it is already open
                return m_errorManager.ErrorCount > 0 || (m_errorManager.ErrorCount == 0 && errorGrid.IsVisible);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
         }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Intializes tabs and managers
        /// </summary>
        /// <param name="logger"></param>
        public int PreBuild(TranslatorAppSettings settings)
        {
            // store the settings
            m_settings = settings;

            // the backend
            m_backend = TranslatorBackendFactory.CreateNewBackend();


            // creates the language manager
            LanguageManager = new LanguageManager(m_backend, m_settings.LanguagesPath);

            // Creates the network manager
            NetworkManager = new NetworkManager();

            // Creates the palette manager
            PaletteManager = new PaletteManager();

            // creates the error manager
            ErrorManager = new ErrorManager();
            ErrorManager.OnErrorsCleared += ErrorManager_OnErrorEventChanged;
            ErrorManager.OnErrorReceived += ErrorManager_OnErrorReceived;

            // Creates the storage file manager to load/save manager data
            StorageFileManager = new StorageFileManager(m_networkManager, m_paletteManager, m_languageManager);

            // Creates the server manager
            ServerManager = new ServerManager(m_networkManager);

            return (int)ExitCodes.SUCCESS;
        }

        private void ErrorManager_OnErrorReceived(object sender, string error)
        {
            DispatcherBeginInvoke(() =>
            {
                errorLog.AppendText("\u2022 '" + error + "'\n");
                //This will use up a lot of memory over a period of time so we should put in a check to see the length and make sure it gets condensed if it is over.
                if (errorLog.Text.Length > m_MaxLogLength)
                {
                    errorLog.Text = errorLog.Text.Substring(m_MaxLogLength / 2);
                }
                errorLog.ScrollToEnd();
                RaisePropertyChanged("ShowError");
            });
        }

        private void ErrorManager_OnErrorEventChanged(object sender, System.EventArgs e)
        {
            DispatcherBeginInvoke(() =>
            {
                errorLog.Clear();
                RaisePropertyChanged("ShowError");
            });
        }

        /// <summary>
        /// Creates all backend and frontend classes
        /// </summary>
        /// <returns></returns>
        public int Build()
        {
            #region TextTab
            m_textTranslatorTab = new TextTranslatorControl(m_backend, m_networkManager, m_serverManager, m_languageManager, m_errorManager);
            this.tabControl.AddTabItem("Text", m_textTranslatorTab);
            #endregion

            #region ImageTab
            m_imageTranslatorTab = new ImageTranslatorControl(m_backend, m_networkManager, m_serverManager, m_languageManager, m_errorManager);
            this.tabControl.AddTabItem("Image", m_imageTranslatorTab);
            #endregion

            #region PdfTab
            m_pdfTranslatorTab = new PdfTranslatorControl(m_backend, m_networkManager, m_serverManager, m_languageManager, m_errorManager);
            this.tabControl.AddTabItem("PDF", m_pdfTranslatorTab);
            #endregion

            return (int)ExitCodes.SUCCESS;
        }

        /// <summary>
        /// Application post build 
        /// </summary>
        /// <returns></returns>
        public int PostBuild()
        {
            // call post build on palette manager
            m_paletteManager?.PostBuild();

            m_modules.Add(m_serverManager);
            m_modules.Add(m_storageFileManager);
            m_modules.Add(m_imageTranslatorTab);
            m_modules.Add(m_textTranslatorTab);
            m_modules.Add(m_pdfTranslatorTab);

            return (int)ExitCodes.SUCCESS;
        }

        /// <summary>
        /// Application startup
        /// </summary>
        public void OnStartup()
        {
            foreach (IModule module in m_modules)
                module.OnStart();

            commandTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
            commandTimer.Interval = System.TimeSpan.FromSeconds(1);
            commandTimer.Tick += Timer_Tick;
            commandTimer.Start();

            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "translator_settings.json");
            Translator.Managers.ServerManager.StartFromCombinedSettings(configPath);
            _serversRunning = true;
            RaisePropertyChanged(nameof(ServerToggleText));

        }

        /// <summary>
        /// Application Stop
        /// </summary>
        public void OnStop()
        {
            foreach (IModule module in m_modules)
                module.OnStop();

            if (commandTimer != null)
            {
                commandTimer.Tick -= Timer_Tick;
                commandTimer.Stop();
                commandTimer = null;
            }

            if (!IsClosed)
                this.Close();
        }

        #region Window Events
        protected override void OnWindowClosing(CancelEventArgs e)
        {
            m_paletteManager?.OnClosing();
            Translator.Managers.ServerManager.StopAll();
        }
        #endregion

        private void errorGrid_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!errorGrid.IsKeyboardFocusWithin)
            {
                HideErrorGrid();
            }
        }

        private void HideErrorGrid()
        {
            errorGrid.Visibility = System.Windows.Visibility.Collapsed;
            RaisePropertyChanged("ShowError");
        }

        private void ShowErrorGrid()
        {
            errorGrid.Visibility = System.Windows.Visibility.Visible;
            errorLog.Focus();
            RaisePropertyChanged("ShowError");
        }
        #region Translation Settings
        private void OpenTranslatorSettingsEditor_Click(object sender, RoutedEventArgs e)
        {
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "translator_settings.json");

            var settingsEditor = new Translator.Windows.TranslatorSettingsEditorWindow(configPath)
            {
                Owner = this
            };

            bool? result = settingsEditor.ShowDialog();

            if (result == true)
            {
                // Optional: refresh or restart the servers using new config
                Translator.Managers.ServerManager.StartFromCombinedSettings(configPath);
            }
        }

        private bool _serversRunning = false;

        public string ServerToggleText => _serversRunning ? "Stop Servers" : "Start Servers";

        private void ToggleServerButton_Click(object sender, RoutedEventArgs e)
        {
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "translator_settings.json");

            if (_serversRunning)
            {
                Translator.Managers.ServerManager.StopAll();  // Add this method if you want a clean shutdown
            }
            else
            {
                Translator.Managers.ServerManager.StartFromCombinedSettings(configPath);
            }

            _serversRunning = !_serversRunning;
            RaisePropertyChanged(nameof(ServerToggleText));
        }


        #endregion
    }
}
