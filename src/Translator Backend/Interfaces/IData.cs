using System.Collections.Generic;

namespace TranslatorBackend.Interfaces
{
    /// <summary>
    /// Base data interface
    /// </summary>
    public interface IData
    {
    }

    #region Ocr Data
    /// <summary>
    /// The result of the ocr
    /// </summary>
    public interface IOcrData : IData
    {
        /// <summary>
        /// The ocr captured text
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Information for all words
        /// </summary>
        IEnumerable<IWordInfo> WordInfo { get; }
    }

    /// <summary>
    /// The ocr word info
    /// </summary>
    public interface IWordInfo
    {
        /// <summary>
        /// The word
        /// </summary>
        string Word { get; }
        /// <summary>
        /// The confidence of the words found
        /// </summary>
        int Confidence { get; }
        /// <summary>
        /// The coordinates of the word
        /// </summary>
        IBoundingBox BoundingBox { get; }
    }

    /// <summary>
    /// the ocr bounding box for all words
    /// </summary>
    public interface IBoundingBox
    {
        /// <summary>
        /// The x1 location
        /// </summary>
        int X1 { get; }

        /// <summary>
        /// The x2 location
        /// </summary>
        int X2 { get; }

        /// <summary>
        /// The y1 location
        /// </summary>
        int Y1 { get; }

        /// <summary>
        /// The y2 location
        /// </summary>
        int Y2 { get; }

        /// <summary>
        /// The width of the box
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the box
        /// </summary>
        int Height { get; }
    }
    #endregion

    #region Translation Data
    public interface ITranslationData : IData
    {
        string TranslatedText { get; }
        IEnumerable<string> AlternativeTranslations { get; }
    }
    #endregion
}
