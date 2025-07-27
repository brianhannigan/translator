using System.Collections.Generic;
using TranslatorBackend.Interfaces;

namespace TranslatorBackend.LanguageCodes
{
    internal class LanguageCodeContainer : ILanguageCodeContainer
    {
        private List<ILanguageCode> m_languageCodes = 
            new List<ILanguageCode>();

        public IEnumerable<ILanguageCode> LanguageCodes
        {
            get { return m_languageCodes; }
        }

        public LanguageCodeContainer()
        {
        }

        public void AddLanguageCode(ILanguageCode languageCode)
        {
            m_languageCodes.Add(languageCode);
        }

        public void Clear()
        {
            m_languageCodes.Clear();
        }
    }
}
