using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Translator.Config;
using Translator.Interfaces;
using VTEControls;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;


namespace Translator.Managers
{
    public class ServerManager : PropertyHandler, IModule, IServerManager
    {
        private enum ServerNames { OCR, TRANSLATE }

        /// <summary>
        /// The network manager
        /// </summary>
        private readonly INetworkManager m_networkManager;

        /// <summary>
        /// Cancellation token for ping task
        /// </summary>
        private CancellationTokenSource m_cancellationToken;

        /// <summary>
        /// Task for pinging the server
        /// </summary>
        private Task m_serverStatusTask;

        /// <summary>
        /// The status of the ocr server
        /// </summary>
        private ServerStatus m_ocrOnline;

        /// <summary>
        /// The status of the translator server
        /// </summary>
        private ServerStatus m_translateOnline;

        public ServerStatus OcrStatus
        {
            get { return m_ocrOnline; }
            set { SetProperty(GetPropertyName(), ref m_ocrOnline, value); }
        }

        public ServerStatus TranslatorStatus
        {
            get { return m_translateOnline; }
            set { SetProperty(GetPropertyName(), ref m_translateOnline, value); }
        }

        private const string C_FormattedHealthAddress = "http://{0}:{1}/health";

        public ServerManager(INetworkManager networkManager)
        {
            m_networkManager = networkManager;
            OcrStatus = ServerStatus.DISCONNECTED;
            TranslatorStatus = ServerStatus.DISCONNECTED;
        }

        public void OnStart()
        {
            if (m_networkManager == null)
                return;

            if (m_serverStatusTask != null)
                return;

            if (m_cancellationToken != null)
                m_cancellationToken.Cancel();

            m_cancellationToken = new CancellationTokenSource();
            m_serverStatusTask = new Task(() => CheckStatus(2000, m_cancellationToken));

            m_serverStatusTask.ContinueWith((task) =>
            {
                if (task == m_serverStatusTask)
                {
                    m_serverStatusTask = null;
                    m_cancellationToken = null;
                }
            });
            m_serverStatusTask.Start();
        }

        public void OnStop()
        {
            if (m_cancellationToken == null)
                return;
            m_cancellationToken.Cancel();
        }

        private void CheckStatus(int delay, CancellationTokenSource cts)
        {
            UpdateServerStatus();

            while (!cts.Token.IsCancellationRequested)
            {
                WaitHandle.WaitAny(new[] { cts.Token.WaitHandle }, delay);

                if (!cts.Token.IsCancellationRequested)
                {
                    UpdateServerStatus();
                }
            }
        }

        private void UpdateServerStatus()
        {
            if (m_networkManager == null)
                return;

            UpdateServerStatusAsync(string.Format(C_FormattedHealthAddress, m_networkManager.OcrIpAddress, m_networkManager.OcrPort), ServerNames.OCR);
            UpdateServerStatusAsync(string.Format(C_FormattedHealthAddress, m_networkManager.TranslationIpAddress, m_networkManager.TranslationPort), ServerNames.TRANSLATE);
        }

        private async void UpdateServerStatusAsync(string url, ServerNames server)
        {
            ServerStatus status;
            try
            {
                var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.GetAsync(url);
                status = response.IsSuccessStatusCode ? ServerStatus.CONNECTED : ServerStatus.DISCONNECTED;
            }
            catch
            {
                status = ServerStatus.DISCONNECTED;
            }

            switch (server)
            {
                case ServerNames.OCR:
                    OcrStatus = status;
                    break;
                case ServerNames.TRANSLATE:
                    TranslatorStatus = status;
                    break;
            }
        }
        #region UseConfig
        public static void StartFromCombinedSettings(string configPath)
        {
            Console.WriteLine("Loading config from: " + configPath);

            if (!File.Exists(configPath))
            {
                Console.WriteLine("Settings file not found.");
                return;
            }

            var json = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<CombinedSettings>(json);

            if (config == null)
            {
                Console.WriteLine("Failed to parse config.");
                return;
            }

            Console.WriteLine($"Using engine: {config.Translation.Engine}");

            string appBase = AppDomain.CurrentDomain.BaseDirectory;
            string marianDir = Path.Combine(appBase, "marian");

            config.Translation.Marian.BaseDir = marianDir;
            config.Translation.Marian.MarianExecutablePath = Path.Combine(marianDir, "marian-server.exe");
            config.Translation.Marian.ConfigPath = Path.Combine(marianDir, "config.yml");

            if (config.Translation.Engine == "mariancli")
            {
                var modelPath = config.Translation.Marian.ModelPath;
                var vocabPath = config.Translation.Marian.VocabPath;
                var marianExe = config.Translation.Marian.MarianExecutablePath;

                var extraArgs = string.Join(" ", config.Translation.Marian.ExtraArgs ?? new List<string>());
                string fullArgs = $"--models {modelPath} --vocabs {vocabPath} {extraArgs}";

                StartProcess("mariancli", marianExe, fullArgs);
            }
            else if (config.Translation.Engine == "marianrest")
            {
                var exe = config.Translation.Marian.MarianExecutablePath;
                var restConfigPath = config.Translation.Marian.ConfigPath ?? "config.yml";
                string fullArgs = $"--config \"{restConfigPath}\" --port 5002";

                StartExecutable("marianrest", exe, fullArgs);
            }

            else if (config.Translation.Engine == "argostranslate")
            {
                StartProcess("Translator", config.Translation.Argos.PythonExecutablePath, config.Translation.Argos.TranslatorScriptPath);
            }

            StartProcess("OCR", config.Ocr.PythonExecutablePath, config.Ocr.OcrScriptPath);
        }


        private static void StartProcess(string name, string executablePath, string scriptPath)
        {
            if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
            {
                Console.WriteLine($"{name} server not started. Executable not found: {executablePath}");
                return;
            }

            if (string.IsNullOrWhiteSpace(scriptPath) || !File.Exists(scriptPath))
            {
                Console.WriteLine($"{name} server not started. Script not found: {scriptPath}");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath, // python.exe
                Arguments = $"\"{scriptPath}\"", // "ocr_server.py"
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            try
            {
                var process = Process.Start(startInfo);
                if (process != null)
                {
                    string pidFile = Path.Combine(Path.GetDirectoryName(scriptPath), $"{name.ToLower()}server.pid");
                    File.WriteAllText(pidFile, process.Id.ToString());

                    Console.WriteLine($"{name} server started. PID = {process.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start {name} server: {ex.Message}");
            }
        }

        private static void StartExecutable(string name, string exePath, string args)
        {
            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                Console.WriteLine($"{name} not started. Executable not found: {exePath}");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            try
            {
                var process = Process.Start(startInfo);
                if (process != null)
                {
                    string pidFile = Path.Combine(Path.GetDirectoryName(exePath), $"{name.ToLower()}server.pid");
                    File.WriteAllText(pidFile, process.Id.ToString());
                    Console.WriteLine($"{name} server started. PID = {process.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start {name} server: {ex.Message}");
            }
        }

        private static void KillProcessFromPidFile(string pidFile)
        {
            try
            {
                if (File.Exists(pidFile))
                {
                    int pid = int.Parse(File.ReadAllText(pidFile));
                    Process.GetProcessById(pid).Kill();
                    File.Delete(pidFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to stop process from {pidFile}: {ex.Message}");
            }
        }


        public static void StopAll()
        {
            Console.WriteLine("Stopping all servers...");

            Console.WriteLine("Stopping all servers...");

            // This points to where the PID files are actually created
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Portable_Translator", "Portable_Translator");
            baseDir = Path.GetFullPath(baseDir);

            string ocrPidPath = Path.Combine(baseDir, "ocrserver.pid");
            string argosPidPath = Path.Combine(baseDir, "translatorserver.pid");
            string marianPidPath = Path.Combine(baseDir, "translatorserver.pid"); // Assume Marian uses same naming convention

            KillProcessFromPidFile(marianPidPath);
            KillProcessFromPidFile(argosPidPath);
            KillProcessFromPidFile(ocrPidPath);
        }
    #endregion
}
}
