using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranslatorBackend.Interfaces;

namespace TranslatorBackend.LanguageCodes
{
    internal class LanguageCode : ILanguageCode
    {
        public string DisplayName { get; set; }
        public string TranslationCode { get; set; }
        public string OcrCode { get; set; }

        public LanguageCode() 
        {
        }

        public LanguageCode(string displayName, string translationCode, string ocrCode)
        {
            DisplayName = displayName;
            TranslationCode = translationCode;
            OcrCode = ocrCode;
        }   
    }
}
