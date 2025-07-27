using System.Collections.Generic;
using Translator.Interfaces;
using TranslatorBackend;
using TranslatorBackend.Interfaces;
using VTEControls;

namespace Translator.Wrappers
{
    public class LibreTranslateDataWrapper : PropertyHandler, ITranslatedData
    {
        private int m_currentIndex;
        private List<string> m_translations = new List<string>();

        public int CurrentIndex
        {
            get { return m_currentIndex; }
        }

        public int Count
        {
            get { return m_translations.Count; }
        }

        public string TranslatedText
        {
            get { return m_translations[m_currentIndex]; }
        }

        public LibreTranslateDataWrapper(ITranslationData result)
        {
            m_currentIndex = 0;
            if (result != null)
            {
                m_translations.Add(result.TranslatedText);
                if (result.AlternativeTranslations != null)
                {
                    m_translations.AddRange(result.AlternativeTranslations);
                }
            }
        }

        public bool CanGoNext()
        {
            return m_currentIndex < m_translations.Count - 1;
        }

        public void GoNext()
        {
            if (CanGoNext())
                m_currentIndex++;

            RaisePropertyChanged(nameof(CurrentIndex));
            RaisePropertyChanged(nameof(TranslatedText));
        }

        public bool CanGoBack()
        {
            return m_currentIndex > 0;
        }

        public void GoBack()
        {
            if (CanGoBack())
                m_currentIndex--;

            RaisePropertyChanged(nameof(CurrentIndex));
            RaisePropertyChanged(nameof(TranslatedText));
        }
    }
}
