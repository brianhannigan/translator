using Newtonsoft.Json;
using System.Collections.Generic;
using TranslatorBackend.Interfaces;

namespace TranslatorBackend.Translation
{
    internal class JsonTranslationData : BaseJsonData, ITranslationData
    {
        /// <summary>
        /// The translated text
        /// </summary>
        [JsonProperty(PropertyName = TranslationPropertyNames.TextPropertyName)]
        public string TranslatedText { get; set; }

        /// <summary>
        /// The list of alternatives
        /// </summary>
        [JsonProperty(PropertyName = TranslationPropertyNames.AlternativesPropertyName)]
        public List<string> Alternatives { get; set; }

        /// <summary>
        /// The alternative translated text
        /// </summary>
        IEnumerable<string> ITranslationData.AlternativeTranslations { get { return Alternatives; } }
    }
}
