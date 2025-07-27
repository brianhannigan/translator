using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Media;
using Translator.Interfaces;
using TranslatorBackend.Interfaces;

namespace Translator
{
    internal class StorageConfigurationSection : ConfigurationSection
    {
        public const string StorageKey = "storage_section";

        public const string NetworkConfigurationKey = "network_configuration";
        public const string TranslationIpKey = "translation_ip";
        public const string TranslationPortKey = "translation_port";
        public const string OcrIpKey = "ocr_ip";
        public const string OcrPortKey = "ocr_port";

        public const string PaletteKey = "palette";
        public const string FilePathTag = "file_path";
        public const string NameTag = "brush_name";
        public const string ValueTag = "brush_value";

        public const string LanguageKey = "language";
        public const string SourceLanguageTag = "source_lanaguage";
        public const string TargetLanguageTag = "target_language";

        [ConfigurationProperty(PaletteKey, IsRequired = true, IsDefaultCollection = true)]
        public PaletteConfigurationCollection Palette
        {
            get { return (PaletteConfigurationCollection)this[PaletteKey]; }
            set { this[PaletteKey] = value; }
        }

        [ConfigurationProperty(NetworkConfigurationKey, IsRequired = true)]
        public NetworkConfiguration NetworkConfiguration
        {
            get { return (NetworkConfiguration)this[NetworkConfigurationKey]; }
            set { this[NetworkConfigurationKey] = value; }
        }

        [ConfigurationProperty(LanguageKey, IsRequired = true)]
        public LanguageConfiguration LanguageConfiguration
        {
            get { return (LanguageConfiguration)this[LanguageKey]; }
            set { this[LanguageKey] = value; }
        }
    }

    #region Network
    public class NetworkConfiguration : ConfigurationElement, INetworkManager
    {
        [ConfigurationProperty(StorageConfigurationSection.TranslationIpKey, IsRequired = true)]
        public string TranslationIpAddress
        {
            get { return (string)this[StorageConfigurationSection.TranslationIpKey]; }
            set { this[StorageConfigurationSection.TranslationIpKey] = value; }
        }

        [ConfigurationProperty(StorageConfigurationSection.TranslationPortKey, IsRequired = true)]
        public int TranslationPort
        {
            get { return (int)this[StorageConfigurationSection.TranslationPortKey]; }
            set { this[StorageConfigurationSection.TranslationPortKey] = value; }
        }

        [ConfigurationProperty(StorageConfigurationSection.OcrIpKey, IsRequired = true)]
        public string OcrIpAddress
        {
            get { return (string)this[StorageConfigurationSection.OcrIpKey]; }
            set { this[StorageConfigurationSection.OcrIpKey] = value; }
        }

        [ConfigurationProperty(StorageConfigurationSection.OcrPortKey, IsRequired = true)]
        public int OcrPort
        {
            get { return (int)this[StorageConfigurationSection.OcrPortKey]; }
            set { this[StorageConfigurationSection.OcrPortKey] = value; }
        }

        public void Load(INetworkManager configurationManager)
        {
            if (configurationManager != null)
            {
                TranslationIpAddress = configurationManager.TranslationIpAddress;
                TranslationPort = configurationManager.TranslationPort;
                OcrIpAddress = configurationManager.OcrIpAddress;
                OcrPort = configurationManager.OcrPort;
            }
        }
    }
    #endregion

    #region Palette
    public class PaletteConfigurationCollection : ConfigurationElementCollection, IPaletteContainer
    {
        public IEnumerable<IPaletteBrush> Brushes
        {
            get { return this.Cast<IPaletteBrush>().ToList(); }
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PaletteBrushElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PaletteBrushElement)element).BrushName;
        }

        public void AddBrush(string name, string value)
        {
            BaseAdd(new PaletteBrushElement(name, value), true);
        }

        public void ClearBrushes()
        {
            BaseClear();
        }
    }

    public class PaletteBrushElement : ConfigurationElement, IPaletteBrush
    {
        [ConfigurationProperty(StorageConfigurationSection.NameTag, IsRequired = true)]
        public string BrushName
        {
            get { return (string)this[StorageConfigurationSection.NameTag]; }
            set { this[StorageConfigurationSection.NameTag] = value; }
        }

        [ConfigurationProperty(StorageConfigurationSection.ValueTag, IsRequired = true)]
        public string Value
        {
            get { return (string)this[StorageConfigurationSection.ValueTag]; }
            set { this[StorageConfigurationSection.ValueTag] = value; }
        }

        public SolidColorBrush Brush
        {
            get
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(this.Value));
            }
        }

        public PaletteBrushElement()
        {
        }

        public PaletteBrushElement(string name, string value)
        {
            BrushName = name;
            Value = value;
        }
    }
    #endregion

    #region Language
    public class LanguageConfiguration : ConfigurationElement, ILanguageStorageConfig
    {
        [ConfigurationProperty(StorageConfigurationSection.SourceLanguageTag, IsRequired = true)]
        public string SourceLanguage
        {
            get { return (string)this[StorageConfigurationSection.SourceLanguageTag]; }
            set { this[StorageConfigurationSection.SourceLanguageTag] = value; }
        }

        [ConfigurationProperty(StorageConfigurationSection.TargetLanguageTag, IsRequired = true)]
        public string TargetLanguage
        {
            get { return (string)this[StorageConfigurationSection.TargetLanguageTag]; }
            set { this[StorageConfigurationSection.TargetLanguageTag] = value; }
        }

        public void Load(ILanguageManager languageManager)
        {
            if (languageManager != null)
            {
                if (languageManager.SourceLanguage != null)
                    SourceLanguage = languageManager.SourceLanguage.DisplayName;
                if (languageManager.TargetLanguage != null)
                    TargetLanguage = languageManager.TargetLanguage.DisplayName;
            }
        }
    }
    #endregion
}
