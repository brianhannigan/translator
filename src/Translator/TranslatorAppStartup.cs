using System;
using System.IO;
using System.Windows;

namespace Translator
{
    class TranslatorAppStartup
    {
        [STAThread]
        static int Main(string[] args)
        {
            var identifier = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            using (SingleInstanceApp sia = new SingleInstanceApp(identifier))
            {                // Allow application to run only if single another instance of
                // the same application is not running or a debugger is attached.
                if (sia.IsOnlyInstance || System.Diagnostics.Debugger.IsAttached)
                {
                    int success = 0;
                    TranslatorApp mainApp = null;
                    try
                    {
                        mainApp = new TranslatorApp();
                        // Parse command line arguments into settings
                        success = mainApp.ParseCommandLineArgs(args);
                        if (success != 0)
                            return success;

                        // Prebuild main window and splash screen
                        success = mainApp.PreBuild();
                        if (success != 0)
                            return success;

                        // Build all the emulators
                        success = mainApp.Build();
                        if (success != 0)
                            return success;

                        // Finalizing
                        success = mainApp.PostBuild();
                        if (success != 0)
                            return success;

                        // Run the app
                        mainApp.Run();
                    }
                    finally
                    {
                        // Shutdown anything that is still running and
                        // prevent any loss of data or memory leaks
                        mainApp?.Stop(success);
                    }
                }
                else
                {
                    MessageBox.Show("An instance of the application is already running.", "Translator", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
            return Environment.ExitCode;
        }
    }
}
