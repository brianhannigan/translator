using TranslatorBackend.Interfaces;

namespace Translator.Interfaces
{
    public interface ILanguageManager
    {
        ILanguageCode SourceLanguage { get; }
        ILanguageCode TargetLanguage { get; }
        void Load(ILanguageStorageConfig storage);
    }
}
