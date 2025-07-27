using System.Collections.Generic;
using System.ComponentModel;

namespace Translator.Config
{
    public class CombinedSettings
    {
        public TranslationSettings Translation { get; set; } = new TranslationSettings();
        public OcrSettings Ocr { get; set; } = new OcrSettings();

        public void EnsureDefaults()
        {
            if (Translation == null)
                Translation = new TranslationSettings();
            else
                Translation.EnsureDefaults();

            if (Ocr == null)
                Ocr = new OcrSettings();
        }
    }

    public class TranslationSettings : INotifyPropertyChanged
    {
        private string _engine = "argostranslate";
        public string Engine
        {
            get => _engine;
            set
            {
                if (_engine != value)
                {
                    _engine = value;
                    OnPropertyChanged(nameof(Engine));
                }
            }
        }

        public ArgosSettings Argos { get; set; } = new ArgosSettings();
        public MarianSettings Marian { get; set; } = new MarianSettings();

        public void EnsureDefaults()
        {
            if (Argos == null)
                Argos = new ArgosSettings();
            if (Marian == null)
                Marian = new MarianSettings();
            if (string.IsNullOrEmpty(Engine))
                Engine = "argostranslate";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ArgosSettings
    {
        public string IP { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 5000;
        public string PythonExecutablePath { get; set; }
        public string TranslatorScriptPath { get; set; }

    }

    public class MarianSettings
    {
        public string BaseDir { get; set; }
        public string VocabPath { get; set; }
        public string ModelPath { get; set; }
        public string MarianExecutablePath { get; set; }
        public string ConfigPath { get; set; }
        public List<string> ExtraArgs { get; set; } = new List<string>();
    }

    public class OcrSettings
    {
        public string BaseDir { get; set; }
        public string PythonExecutablePath { get; set; }
        public string OcrScriptPath { get; set; }
        public string StartBatchPath { get; set; }
    }
}
