namespace TranslatorBackend.Interfaces
{
    public interface IResult<T>
    {
        /// <summary>
        /// Indicates if the translation was completely successful
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// Gets an error message
        /// </summary>
        /// <returns></returns>
        string GetErrorMessage();

        /// <summary>
        /// Gets teh result
        /// </summary>
        T Result { get; }
    }

    /// <summary>
    /// The result of the text translation
    /// </summary>
    public interface ITextTranslationResult : IResult<ITranslationData>
    {
    }

    /// <summary>
    /// The result of the image ocr
    /// </summary>
    public interface IImageOcrResult : IResult<IOcrData>
    {
    }
}
