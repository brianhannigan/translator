using TranslatorBackend.Interfaces;
using TranslatorBackend.LanguageCodes;

namespace TranslatorBackend.Factory
{
    public static class LanguageCodeFactory
    {
        public static ILanguageCodeContainer CreateLanguageCodeManager()
        {
            return new LanguageCodeContainer();
        }

        public static ILanguageCode CreateNewLanguageCode(string displayname, string translationCode, string ocrCode)
        {
            return new LanguageCode(displayname, translationCode, ocrCode);
        }
    }
}
