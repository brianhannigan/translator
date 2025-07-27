using TranslatorBackend.Interfaces;

namespace TranslatorBackend.Results
{
    internal abstract class ResultBase
    {
        protected bool m_success;
        protected string m_errorMessage;

        /// <summary>
        /// Indicates if the translation was successful
        /// </summary>
        public bool Success
        {
            get { return m_success; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ResultBase()
        {
            m_success = false;
            m_errorMessage = null;
        }

        public abstract void TryMarkAsSuccess(object result);

        /// <summary>
        /// Sets the result as error with a message
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public void MarkAsError(string error)
        {
            m_success = false;
            m_errorMessage = error;
        }

        /// <summary>
        /// Returns the error message
        /// </summary>
        /// <returns></returns>
        public string GetErrorMessage()
        {
            return m_errorMessage;
        }
    }

    /// <summary>
    /// Translation Result Implementation
    /// </summary>
    internal abstract class ResultBase<T> : ResultBase, IResult<T>
        where T : class, IData
    {
        private T m_result;

        /// <summary>
        /// Gets the result
        /// </summary>
        public T Result
        {
            get { return m_result; }
        }

        /// <summary>
        /// Sets the result as successful
        /// </summary>
        /// <returns></returns>
        public override void TryMarkAsSuccess(object result)
        {
            T converted = result as T;
            if (converted == null)
                return;

            if (ValidateResult(converted))
            {
                m_errorMessage = null;
                m_success = true;
                m_result = converted;
            }
        }

        /// <summary>
        /// Validate the result
        /// </summary>
        /// <returns></returns>
        protected virtual bool ValidateResult(T result)
        {
            return result != null;
        }
    }

    /// <summary>
    /// Text translation Implementation
    /// </summary>
    internal class TextTranslationResult : ResultBase<ITranslationData>, ITextTranslationResult
    {
        protected override bool ValidateResult(ITranslationData result)
        {
            bool valid = base.ValidateResult(result);
            if (result != null)
            {
                valid &= !string.IsNullOrWhiteSpace(result.TranslatedText);
                valid &= result.AlternativeTranslations != null;
            }
            return valid;
        }
    }

    /// <summary>
    /// Image translation Implementation
    /// </summary>
    internal class ImageOcrResult : ResultBase<IOcrData>, IImageOcrResult
    {
        protected override bool ValidateResult(IOcrData result)
        {
            bool valid = base.ValidateResult(result);
            if (result != null)
            {
                valid &= !string.IsNullOrWhiteSpace(result.Text);
                valid &= result.WordInfo != null;
            }
            return valid;
        }
    }
}
