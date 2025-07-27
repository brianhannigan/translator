using System.Collections.ObjectModel;
using System.Linq;
using Translator.Interfaces;
using Translator.Wrappers;
using TranslatorBackend.Interfaces;
using VTEControls;

namespace Translator.Managers
{
    public class LanguageManager : PropertyHandler, ILanguageManager
    {
        private ILanguageCode m_sourceLanguage;
        private ILanguageCode m_targetLanguage;

        /// <summary>
        /// List of supported languages
        /// </summary>
        private ObservableCollection<ILanguageCode> m_langugeCodes =
            new ObservableCollection<ILanguageCode>();

        public ObservableCollection<ILanguageCode> LanguageCodes
        {
            get { return m_langugeCodes; }
            set { SetProperty(GetPropertyName(), ref m_langugeCodes, value); }
        }

        public ILanguageCode SourceLanguage
        {
            get { return m_sourceLanguage; }
            set { SetProperty(GetPropertyName(), ref m_sourceLanguage, value); }
        }

        public ILanguageCode TargetLanguage
        {
            get { return m_targetLanguage; }
            set { SetProperty(GetPropertyName(), ref m_targetLanguage, value); }
        }

        public LanguageManager(ITranslatorBackend backend, string path)
        {
            if (backend == null) { return; }

            if (backend.LoadLanguageCodeFile(path))
            {
            }

            if (backend.LanguageCodeContainer != null)
            {
                foreach (ILanguageCode lc in backend.LanguageCodeContainer.LanguageCodes)
                {
                    LanguageCodeWrapper lcw = new LanguageCodeWrapper(lc);
                    LanguageCodes.Add(lcw);
                }

                if (m_langugeCodes.Count > 0)
                {
                    SourceLanguage = TargetLanguage = m_langugeCodes[0];
                }
            }
        }

        public void Load(ILanguageStorageConfig languageStorage)
        {
            if (languageStorage == null) { return; }

            if (!string.IsNullOrEmpty(languageStorage.SourceLanguage))
            {
                ILanguageCode foundSourceLanguage = m_langugeCodes.FirstOrDefault(x => x.DisplayName == languageStorage.SourceLanguage);
                if (foundSourceLanguage != null)
                {
                    SourceLanguage = foundSourceLanguage;
                }
            }

            if (!string.IsNullOrEmpty(languageStorage.TargetLanguage))
            {
                ILanguageCode foundTargetLanguage = m_langugeCodes.FirstOrDefault(x => x.DisplayName == languageStorage.TargetLanguage);
                if (foundTargetLanguage != null)
                {
                    TargetLanguage = foundTargetLanguage;
                }
            }
        }
    }
}
