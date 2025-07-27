using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Translator.Interfaces;
using VTEControls;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace Translator.Managers
{
    public class StorageFileManager : PropertyHandler, IModule
    {

        /// <summary>
        /// Used to store data in Translator.exe.config
        /// </summary>
        private Configuration m_configStorage;

        /// <summary>
        /// The palette manager
        /// </summary>
        private readonly IPaletteManager m_paletteManager;

        /// <summary>
        /// the network manager
        /// </summary>
        private readonly INetworkManager m_networkManager;

        /// <summary>
        /// The language manager
        /// </summary>
        private readonly ILanguageManager m_languageManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dirtyTracker"></param>
        /// <param name="collectionManager"></param>
        /// <param name="configurationMnaager"></param>
        public StorageFileManager(INetworkManager networkManager, IPaletteManager paletteManager, ILanguageManager languageManager)
        {
            m_networkManager = networkManager;
            m_paletteManager = paletteManager;
            m_languageManager = languageManager;

            LoadConfigurations();
        }

        /// <summary>
        /// Load previously loaded configurations from stored app data file
        /// </summary>
        private void LoadConfigurations()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string fileName = string.Format("{0}.exe.config", executingAssembly.GetName().Name);

            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = Path.Combine(TranslatorApp.AppDataFolder, fileName);

            // Open exe configuration file
            m_configStorage = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            StorageConfigurationSection configStorage = null;

            try
            {
                configStorage = m_configStorage.Sections[StorageConfigurationSection.StorageKey] as StorageConfigurationSection;
            }
            catch
            {
                // Error parsing the file so we will remove it and create a new one.
                m_configStorage.Sections.Remove(StorageConfigurationSection.StorageKey);
            }

            // Add storage section if it does not already exist in .exe.config file.
            if (configStorage == null)
            {
                configStorage = new StorageConfigurationSection();
                configStorage.SectionInformation.ForceSave = true;
                m_configStorage.Sections.Add(StorageConfigurationSection.StorageKey, configStorage);
            }

            // load the network configuration
            m_networkManager.Load(configStorage.NetworkConfiguration);

            // load the previous languages
            m_languageManager.Load(configStorage.LanguageConfiguration);

            // Set the pallete
            m_paletteManager.SwapPaletteTo(configStorage.Palette);
        }

        public void OnStart()
        {

        }

        /// <summary>
        /// Application Stop
        /// </summary>
        public void OnStop()
        {
            if (m_configStorage == null)
                return;

            StorageConfigurationSection configStorage = m_configStorage.Sections[StorageConfigurationSection.StorageKey] as StorageConfigurationSection;
            if (configStorage == null)
                return;

            // Save the network manager data
            configStorage.NetworkConfiguration.Load(m_networkManager);

            // Clear the current store brushes
            configStorage.Palette.ClearBrushes();

            // Add current color scheme to the configuration storage
            foreach (IPaletteBrush brush in m_paletteManager.PaletteBrushes)
            {
                if (brush == null || brush.Brush == null)
                    continue;

                configStorage.Palette.AddBrush(brush.BrushName, brush.Value);
            }

            // Save the language manager data
            configStorage.LanguageConfiguration.Load(m_languageManager);

            // Save config file
            m_configStorage?.Save(ConfigurationSaveMode.Modified);
        }
    }
}
