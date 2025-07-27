using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TranslatorBackend.Interfaces;
using TranslatorBackend.LanguageCodes;
using TranslatorBackend.Ocr;
using TranslatorBackend.Results;
using TranslatorBackend.Translation;

namespace TranslatorBackend
{
    internal class TranslatorBackend : ITranslatorBackend
    {
        private readonly LibreTranslateHttpHandler m_textTranslation;
        private readonly TesseractHttpHandler m_ocrImage;
        private LanguageCodeContainer m_languageCodeManager;

        public ILanguageCodeContainer LanguageCodeContainer
        {
            get { return m_languageCodeManager; }
        }

        /// <summary>
        /// The constructor
        /// </summary>
        public TranslatorBackend()
        {
            m_textTranslation = new LibreTranslateHttpHandler();
            m_ocrImage = new TesseractHttpHandler();
            LoadDefaultLanguageCodes();
        }

        #region Langauge Code Manager
        /// <summary>
        /// Loads default language code config file
        /// </summary>
        void LoadDefaultLanguageCodes()
        {
            try
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                string defaultFilePath = string.Format("{0}.LanguageCodes.{1}", Assembly.GetExecutingAssembly().GetName().Name, "DefaultLanguages.config");
                string resourceFullPath = executingAssembly.GetManifestResourceNames().FirstOrDefault(x => string.Equals(x, defaultFilePath, StringComparison.OrdinalIgnoreCase));
                using (Stream stream = executingAssembly.GetManifestResourceStream(resourceFullPath))
                {
                    LanguageCodeContainer lcm = new LanguageCodeContainer();
                    if (lcm.ParseLanguagesFile(stream))
                        m_languageCodeManager = lcm;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception loading default langauges: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads a config file from a user
        /// </summary>
        /// <param name="filePath">File path to the config file</param>
        /// <returns>true if successful:otherwise false</returns>
        public bool LoadLanguageCodeFile(string filePath)
        {
            bool success;
            LanguageCodeContainer lcm = new LanguageCodeContainer();
            if (success = lcm.ParseLanguagesFile(filePath))
                m_languageCodeManager = lcm;

            return success;
        }
        #endregion

        #region ITranslationBackend
        /// <summary>
        /// Translates text from one language to another
        /// </summary>
        /// <param name="uri">The uri to the server for processing</param>
        /// <param name="sourceLanguageCode">The source language code</param>
        /// <param name="text">the text to translate</param>
        /// <param name="targetLanguageCode">the target language code</param>
        /// <param name="alternatives">number of alternatives to allow</param>
        /// <returns>A text translation result</returns>
        public ITextTranslationResult TranslateText(string uri, string sourceLanguageCode, string text, string targetLanguageCode, int alternatives)
        {
            TextTranslationResult result = new TextTranslationResult();
            if (string.IsNullOrWhiteSpace(uri) ||
                string.IsNullOrWhiteSpace(sourceLanguageCode) ||
                string.IsNullOrWhiteSpace(targetLanguageCode))
            {
                result.MarkAsError("Invalid parameters, please verify the uri and language codes have been set.");
            }
            else
            {
                m_textTranslation.TranslateText(result, uri, text, sourceLanguageCode, targetLanguageCode, alternatives);
            }

            return result;
        }

        /// <summary>
        /// Extracts text from an image using OCR processing
        /// </summary>
        /// <param name="uri">The uri to the server for processing</param>
        /// <param name="sourceLanguageCode">the language code for the image</param>
        /// <param name="imgBytes">the image data</param>
        /// <returns>An image ocr result</returns>
        public IImageOcrResult OcrImage(string uri, string sourceLanguageCode, byte[] imgBytes)
        {
            ImageOcrResult result = new ImageOcrResult();
            if (string.IsNullOrWhiteSpace(uri) ||
                string.IsNullOrWhiteSpace(sourceLanguageCode) ||
                imgBytes == null)
            {
                result.MarkAsError("Invalid parameters, please verify the uri, language code and image has been set.");
            }

            else
            {
                m_ocrImage.OCRAnImage(result, uri, imgBytes, sourceLanguageCode);
            }

            return result;
        }

        #region Asyncronous Methods
        /// <summary>
        /// Translates text from one language to another asyncronously
        /// </summary>
        /// <param name="uri">The uri to the server for processing</param>
        /// <param name="sourceLanguageCode">The source language code</param>
        /// <param name="text">the text to translate</param>
        /// <param name="targetLanguageCode">the target language code</param>
        /// <param name="alternatives">number of alternatives to allow</param>
        /// <returns>A text translation result</returns>
        public async Task<ITextTranslationResult> TranslateTextAsync(string uri, string sourceLanguageCode, string text, string targetLanguageCode, int alternatives)
        {
            return await Task.Run(() => TranslateText(uri, sourceLanguageCode, text, targetLanguageCode, alternatives));
        }

        /// <summary>
        /// Extracts text from an image using OCR processing asyncronously
        /// </summary>
        /// <param name="uri">The uri to the server for processing</param>
        /// <param name="sourceLanguageCode">the language code for the image</param>
        /// <param name="imgBytes">the image data</param>
        /// <returns>An image ocr result</returns>
        public async Task<IImageOcrResult> OcrImageAsync(string uri, string sourceLanguageCode, byte[] imgBytes)
        {
            return await Task.Run(() => OcrImage(uri, sourceLanguageCode, imgBytes));
        }
        #endregion
        #endregion
    }
}
