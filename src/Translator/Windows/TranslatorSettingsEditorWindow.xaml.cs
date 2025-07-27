using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Translator.Config;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace Translator.Windows
{
    public partial class TranslatorSettingsEditorWindow : Window
    {
        private CombinedSettings _config;
        private readonly string _configPath;

        public TranslatorSettingsEditorWindow(string configPath)
        {
            InitializeComponent();
            _configPath = configPath;
            LoadConfig();

            DataContext = _config;

            if (_config.Translation != null)
            {
                _config.Translation.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(TranslationSettings.Engine))
                        Dispatcher.Invoke(UpdateTranslationFieldVisibility);
                };

            }

            UpdateTranslationFieldVisibility(); // optional initial check
        }


        private void LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    _config = JsonConvert.DeserializeObject<CombinedSettings>(json);
                    _config?.EnsureDefaults();
                }
                else
                {
                    _config = new CombinedSettings();
                    _config.EnsureDefaults();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load config:\n{ex.Message}");
                _config = new CombinedSettings();
                _config.EnsureDefaults();
            }
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
                MessageBox.Show("Configuration saved successfully.");
                DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save config:\n{ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //private void TranslationEngineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    UpdateTranslationFieldVisibility();
        //}


        private void UpdateTranslationFieldVisibility()
        {
            var engine = _config?.Translation?.Engine?.ToLowerInvariant() ?? "argostranslate";

            if (engine == "mariancli")
            {
                MarianSettingsPanel.Visibility = Visibility.Visible;
                ArgosSettingsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MarianSettingsPanel.Visibility = Visibility.Collapsed;
                ArgosSettingsPanel.Visibility = Visibility.Visible;
            }
        }


        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            var tag = ((System.Windows.Controls.Button)sender).Tag?.ToString();
            var ofd = new OpenFileDialog { Filter = "All Files|*.*" };

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                switch (tag)
                {
                    case "marian_exe": _config.Translation.Marian.MarianExecutablePath = ofd.FileName; break;
                    case "marian_model": _config.Translation.Marian.ModelPath = ofd.FileName; break;
                    case "marian_vocab": _config.Translation.Marian.VocabPath = ofd.FileName; break;
                    case "ocr_py": _config.Ocr.PythonExecutablePath = ofd.FileName; break;
                    case "ocr_script": _config.Ocr.OcrScriptPath = ofd.FileName; break;
                    case "translator_batch": _config.Ocr.StartBatchPath = ofd.FileName; break;
                }

                // Refresh binding
                DataContext = null;
                DataContext = _config;
            }
        }

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _config.Ocr.BaseDir = dialog.SelectedPath;
                    DataContext = null;
                    DataContext = _config;
                }
            }
        }

        private void BrowseArgosPythonExe_Click(object sender, RoutedEventArgs e)
        {
            HandleFileBrowse("argos_py");
        }

        private void BrowseArgosScript_Click(object sender, RoutedEventArgs e)
        {
            HandleFileBrowse("argos_script");
        }

        private void BrowseMarianExe_Click(object sender, RoutedEventArgs e)
        {
            HandleFileBrowse("marian_exe");
        }

        private void BrowseMarianModel_Click(object sender, RoutedEventArgs e)
        {
            HandleFileBrowse("marian_model");
        }

        private void BrowseMarianVocab_Click(object sender, RoutedEventArgs e)
        {
            HandleFileBrowse("marian_vocab");
        }

        private void BrowseOcrPythonExe_Click(object sender, RoutedEventArgs e)
        {
            HandleFileBrowse("ocr_py");
        }

        private void BrowseOcrScript_Click(object sender, RoutedEventArgs e)
        {
            HandleFileBrowse("ocr_script");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save_Click(sender, e); // Forward to existing method
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel_Click(sender, e); // Forward to existing method
        }
        private void BrowseMarianConfig_Click(object sender, RoutedEventArgs e)
        {
            HandleFileBrowse("marian_config");
        }


        private void HandleFileBrowse(string tag)
        {
            var ofd = new OpenFileDialog { Filter = "All Files|*.*" };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                switch (tag)
                {
                    case "argos_py": _config.Translation.Argos.PythonExecutablePath = ofd.FileName; break;
                    case "argos_script": _config.Translation.Argos.TranslatorScriptPath = ofd.FileName; break;
                    case "marian_config": _config.Translation.Marian.ConfigPath = ofd.FileName; break;
                    case "marian_exe": _config.Translation.Marian.MarianExecutablePath = ofd.FileName; break;
                    case "marian_model": _config.Translation.Marian.ModelPath = ofd.FileName; break;
                    case "marian_vocab": _config.Translation.Marian.VocabPath = ofd.FileName; break;
                    case "ocr_py": _config.Ocr.PythonExecutablePath = ofd.FileName; break;
                    case "ocr_script": _config.Ocr.OcrScriptPath = ofd.FileName; break;
                    case "translator_batch": _config.Ocr.StartBatchPath = ofd.FileName; break;
                }

                DataContext = null;
                DataContext = _config;
                _config.Translation.PropertyChanged += (_, __) => UpdateTranslationFieldVisibility();
            }
        }

    }
}
