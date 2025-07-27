using System;
using System.Diagnostics;
using System.IO;

namespace Translator.Helpers
{
    public static class PythonProcessHelper
    {
        public static Process GetPythonProcessFromPidFile(string scriptDirectory, string label)
        {
            string pidFile = Path.Combine(scriptDirectory, label + "server.pid");

            if (!File.Exists(pidFile))
            {
                Console.WriteLine($"{label} PID file not found at: {pidFile}");
                return null;
            }

            try
            {
                string pidText = File.ReadAllText(pidFile).Trim();
                if (int.TryParse(pidText, out int pid))
                {
                    var proc = Process.GetProcessById(pid);
                    if (proc.ProcessName.ToLower().Contains("python"))
                        return proc;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {label} PID file: {ex.Message}");
            }

            return null;
        }

        public static bool DeletePidFile(string scriptDirectory, string label)
        {
            try
            {
                string pidFile = Path.Combine(scriptDirectory, label + "server.pid");
                if (File.Exists(pidFile))
                {
                    File.Delete(pidFile);
                    Console.WriteLine($"Deleted PID file: {pidFile}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting {label} PID file: {ex.Message}");
            }
            return false;
        }

        public static bool IsProcessRunningFromPid(string scriptDirectory, string label)
        {
            var proc = GetPythonProcessFromPidFile(scriptDirectory, label);
            return proc != null && !proc.HasExited;
        }

        public static void KillProcessFromPid(string scriptDirectory, string label)
        {
            var proc = GetPythonProcessFromPidFile(scriptDirectory, label);
            if (proc != null && !proc.HasExited)
            {
                try
                {
                    proc.Kill();
                    proc.WaitForExit();
                    Console.WriteLine($"Killed {label} process with PID {proc.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to kill {label} process: {ex.Message}");
                }
            }
        }
    }
}
