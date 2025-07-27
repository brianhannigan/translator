using System.Threading.Tasks;

namespace TranslatorBackend.Interfaces
{
    public interface ITranslatorBackend : ILanguageCodeManager
    {
        /// <summary>
        /// Translates text from one language to another
        /// </summary>
        /// <param name="uri">The uri to the server for processing</param>
        /// <param name="sourceLanguageCode">The source language code</param>
        /// <param name="text">the text to translate</param>
        /// <param name="targetLanguageCode">the target language code</param>
        /// <param name="alternatives">number of alternatives to allow</param>
        /// <returns>A text translation result</returns>
        ITextTranslationResult TranslateText(string uri,
                                             string sourceLanguageCode,
                                             string text,
                                             string targetLanguageCode,
                                             int alternatives);

        /// <summary>
        /// Extracts text from an image using OCR processing
        /// </summary>
        /// <param name="uri">The uri to the server for processing</param>
        /// <param name="sourceLanguageCode">the language code for the image</param>
        /// <param name="imgBytes">the image data</param>
        /// <returns>An image ocr result</returns>
        IImageOcrResult OcrImage(string uri,
                                 string sourceLanguageCode,
                                 byte[] imgBytes);

        /// <summary>
        /// Translates text from one language to another asyncronously
        /// </summary>
        /// <param name="uri">The uri to the server for processing</param>
        /// <param name="sourceLanguageCode">The source language code</param>
        /// <param name="text">the text to translate</param>
        /// <param name="targetLanguageCode">the target language code</param>
        /// <param name="alternatives">number of alternatives to allow</param>
        /// <returns>A text translation result</returns>
        Task<ITextTranslationResult> TranslateTextAsync(string uri,
                                                        string sourceLanguageCode,
                                                        string text,
                                                        string targetLanguageCode,
                                                        int alternatives);

        /// <summary>
        /// Extracts text from an image using OCR processing asyncronously
        /// </summary>
        /// <param name="uri">The uri to the server for processing</param>
        /// <param name="sourceLanguageCode">the language code for the image</param>
        /// <param name="imgBytes">the image data</param>
        /// <returns>An image ocr result</returns>
        Task<IImageOcrResult> OcrImageAsync(string uri,
                                            string sourceLanguageCode,
                                            byte[] imgBytes);
    }

    public interface ILanguageCodeManager
    {
        ILanguageCodeContainer LanguageCodeContainer { get; }
        bool LoadLanguageCodeFile(string filePath);
    }
}
