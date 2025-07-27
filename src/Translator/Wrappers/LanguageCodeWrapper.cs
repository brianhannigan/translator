using TranslatorBackend.Interfaces;

namespace Translator.Wrappers
{
    internal class LanguageCodeWrapper : ILanguageCode
    {
        public string DisplayName { get; private set; }

        public string TranslationCode { get; private set; }

        public string OcrCode { get; private set; }

        public LanguageCodeWrapper(ILanguageCode languageCode)
        {
            DisplayName = languageCode.DisplayName;
            TranslationCode = languageCode.TranslationCode;
            OcrCode = languageCode.OcrCode;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
