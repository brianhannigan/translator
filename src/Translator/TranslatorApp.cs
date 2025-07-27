using System;
using System.Threading;
using System.Windows;

namespace Translator
{
    internal class TranslatorApp : Application
    {
        public static string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Translator";

        /// <summary>
        /// Storage class for all command line arguments that are read in or parsed from the xml file
        /// </summary>
        private TranslatorAppSettings m_settings = null;

        /// <summary>
        /// The main window for the application
        /// </summary>
        private MainWindow m_mainWindow = null;

        /// <summary>
        /// The splash screen
        /// </summary>
        private SplashScreen m_splashScreen = null;

        /// <summary>
        /// The splash thread
        /// </summary>
        private Thread m_splashThread = null;

        /// <summary>
        /// Reset event marker.
        /// </summary>
        private static ManualResetEventSlim m_splashSignal;

        /// <summary>
        /// Constructor
        /// </summary>
        public TranslatorApp()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initalize components
        /// </summary>
        private void InitializeComponent()
        {
            // Load application resources
            Uri resourceLocater = new Uri("/Translator;component/app.xaml", System.UriKind.Relative);
            LoadComponent(this, resourceLocater);
        }

        /// <summary>
        /// Parse command line arguments into settings
        /// </summary>
        /// <returns></returns>
        public int ParseCommandLineArgs(string[] args)
        {
            int success = (int)ExitCodes.SUCCESS;
            m_settings = new TranslatorAppSettings();
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToLower())
                    {
                        case TranslatorAppSettings.HelpCmd:
                        case TranslatorAppSettings.HelpHCmd:
                        case TranslatorAppSettings.HelpQuestionCmd:
                            m_settings.Displayhelp();
                            return (int)ExitCodes.HELP_REQUESTED;
                        case TranslatorAppSettings.LanguagesCmd:
                            if (args.Length > i + 1)
                            {
                                m_settings.LanguagesPath = args[++i];
                            }
                            break;
                    }
                }
            }
            catch
            {
                return (int)ExitCodes.SETTINGS_NOT_LOADED;
            }
            return success;
        }

        /// <summary>
        /// Prebuild main window and splash screen
        /// </summary>
        /// <returns>(int) error code</returns>
        public int PreBuild()
        {
            // Create the main window
            m_mainWindow = new MainWindow();

            // Call prebuild main window
            int success = m_mainWindow.PreBuild(m_settings);
            if (success != 0)
                return success;

            // Create and show the splash screen
            m_splashSignal = new ManualResetEventSlim(false);

            // Create and show splash screen in new thread
            m_splashThread = new Thread(() =>
            {
                m_splashScreen = new SplashScreen();
                m_splashScreen.Show();
                m_splashSignal.Set();
                System.Windows.Threading.Dispatcher.Run();

                m_splashScreen.SetStatus("Loading...");
            });

            m_splashThread.SetApartmentState(ApartmentState.STA);
            m_splashThread.IsBackground = true;
            m_splashThread.Start();

            // Wait for splash screen to show
            m_splashSignal.Wait();

            return success;
        }

        /// <summary>
        /// Build main window
        /// </summary>
        /// <returns>(int) error code</returns>
        public int Build()
        {
            m_splashScreen?.SetStatus("Loading...");
            int success = m_mainWindow.Build();
            if (success != 0)
            {
                // Notify user there was an issue during build?
                // Maybe an external log file used to store app run data?
            }

            return success;
        }

        /// <summary>
        /// Post build main window
        /// </summary>
        /// <returns>(int) error code</returns>
        public int PostBuild()
        {
            m_splashScreen?.SetStatus("Finalizing...");
            int success = m_mainWindow.PostBuild();
            if (success != 0)
            {
                // Notify user there was an issue during build?
                // Maybe an external log file used to store app run data?
            }
            return success;
        }

        /// <summary>
        /// Handles application startup
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Called after app.run
            base.OnStartup(e);

            // Set the status of the splash screen to complete
            m_splashScreen?.SetStatus("Load Complete.");

            // Show the main window
            m_mainWindow?.Show();

            // Activate the main window
            m_mainWindow?.Activate();

            // Cleanup splash screen
            m_splashScreen?.Cleanup();
            m_splashScreen = null;

            // Call main window on startup
            m_mainWindow?.OnStartup();
        }

        /// <summary>
        /// Do any cleanup that is needed to prevent memory leaks
        /// </summary>
        public void Stop(int exitCode)
        {
            // Cleanup splash window, if it hasn't been done yet
            m_splashScreen?.Cleanup();
            // Cleanup main window, if it hasn't been done yet
            m_mainWindow?.OnStop();

            int exitingValue = exitCode != 0 ? exitCode : Environment.ExitCode;
        }
    }
}
