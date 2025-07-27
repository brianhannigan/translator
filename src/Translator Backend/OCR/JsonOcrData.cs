using Newtonsoft.Json;
using System.Collections.Generic;
using TranslatorBackend.Interfaces;

namespace TranslatorBackend.Ocr
{
    /// <summary>
    /// The ocr json object
    /// </summary>
    internal class JsonOcrData : BaseJsonData, IOcrData
    {
        /// <summary>
        /// The ocr captured text
        /// </summary>
        [JsonProperty(PropertyName = OcrPropertyNames.TextPropertyName)]
        public string Text { get; set; }

        /// <summary>
        /// Information for all words
        /// </summary>
        [JsonProperty(PropertyName = OcrPropertyNames.WordInfoPropertyName)]
        public List<WordInfo> WordInfo { get; set; }

        /// <summary>
        /// Explict interface return for word info
        /// </summary>
        IEnumerable<IWordInfo> IOcrData.WordInfo { get { return WordInfo; } }
    }

    /// <summary>
    /// The ocr word info
    /// </summary>
    /// 
    internal class WordInfo : IWordInfo
    {
        /// <summary>
        /// The word
        /// </summary>
        [JsonProperty(PropertyName = OcrPropertyNames.WordPropertyName)]
        public string Word { get; set; }

        /// <summary>
        /// The confidence of the word
        /// </summary>
        [JsonProperty(PropertyName = OcrPropertyNames.ConfidencePropertyName)]
        public int Confidence { get; set; }

        /// <summary>
        /// The coordinates of the word
        /// </summary>
        [JsonProperty(PropertyName = OcrPropertyNames.BoundingBoxPropertyName)]
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Explict interface return for bounding box
        /// </summary>
        IBoundingBox IWordInfo.BoundingBox { get { return BoundingBox; } }
    }

    internal class BoundingBox : IBoundingBox
    {
        /// <summary>
        /// The x1 location
        /// </summary>
        [JsonProperty(PropertyName = OcrPropertyNames.BoundingBoxX1PropertyName)]
        public int X1 { get; set; }

        /// <summary>
        /// The y1 location
        /// </summary>
        [JsonProperty(PropertyName = OcrPropertyNames.BoundingBoxY1PropertyName)]
        public int Y1 { get; set; }

        /// <summary>
        /// The x2 location
        /// </summary>
        [JsonProperty(PropertyName = OcrPropertyNames.BoundingBoxX2PropertyName)]
        public int X2 { get; set; }

        /// <summary>
        /// The y2 location
        /// </summary>
        [JsonProperty(PropertyName = OcrPropertyNames.BoundingBoxY2PropertyName)]
        public int Y2 { get; set; }

        /// <summary>
        /// The widht of the box
        /// </summary>
        public int Width
        {
            get
            {
                return X2 - X1;
            }
        }

        /// <summary>
        /// The height of the box
        /// </summary>
        public int Height
        {
            get
            {
                return Y2 - Y1;
            }
        }
    }
}
