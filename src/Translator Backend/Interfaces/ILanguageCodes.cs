using System.Collections.Generic;

namespace TranslatorBackend.Interfaces
{
    /// <summary>
    /// The language codes
    /// </summary>
    public interface ILanguageCode
    {
        /// <summary>
        /// Display name of the language code
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Translation abbreviated language code
        /// </summary>
        string TranslationCode { get; }

        /// <summary>
        /// Ocr abbreviated language code
        /// </summary>
        string OcrCode { get; }
    }

    /// <summary>
    /// Container for the language codes
    /// </summary>
    public interface ILanguageCodeContainer
    {
        /// <summary>
        /// Gets the language codes
        /// </summary>
        IEnumerable<ILanguageCode> LanguageCodes { get; }
    }
}
