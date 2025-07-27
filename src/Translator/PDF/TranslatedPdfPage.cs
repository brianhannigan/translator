using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Translator.Interfaces;
using VTEControls;

namespace Translator.PDF
{
    public class TranslatedPdfPage : PropertyHandler, IDisposable
    {
        /// <summary>
        /// A value indicating whether this instance is disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The images in the page
        /// </summary>
        private ObservableCollection<TranslatedPdfPageImage> m_images =
            new ObservableCollection<TranslatedPdfPageImage>();

        /// <summary>
        /// Translated data results lookup
        /// </summary>
        private Dictionary<IStringPageContent, ITranslatedData> m_translatedData =
            new Dictionary<IStringPageContent, ITranslatedData>();

        /// <summary>
        /// The contents of the page
        /// </summary>
        private List<IPageContent> m_pageContents = new List<IPageContent>();

        /// <summary>
        /// Gets the contents of the page
        /// </summary>
        public IEnumerable<IPageContent> PageContents
        {
            get { return m_pageContents; }
        }

        public ObservableCollection<TranslatedPdfPageImage> Images
        {
            get { return m_images; }
            private set { SetProperty(GetPropertyName(), ref m_images, value); }
        }

        /// <summary>
        /// Gets if images should be shown
        /// </summary>
        public bool ShowImages
        {
            get { return m_images.Count > 0; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page"></param>
        public TranslatedPdfPage(PdfDataPage page)
        {
            Images = new ObservableCollection<TranslatedPdfPageImage>();

            if (page != null)
            {
                m_pageContents = new List<IPageContent>(page.OrderedContents);
                foreach(IImagePageContent imageContent in GetPageContentsAs<IImagePageContent>())
                {
                    Images.Add(new TranslatedPdfPageImage(imageContent));
                }
            }
        }

        /// <summary>
        /// Adds a translation result to the lookup
        /// </summary>
        /// <param name="content"></param>
        /// <param name="result"></param>
        public void AddTranslatedResult(IStringPageContent content, ITranslatedData result)
        {
            m_translatedData.Add(content, result);
        }

        /// <summary>
        /// Gets all page contents of a specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetPageContentsAs<T>()
            where T : class, IPageContent
        {
            foreach (IPageContent content in m_pageContents)
            {
                if (content is T)
                {
                    yield return content as T;
                }
            }
        }

        /// <summary>
        /// Gets translated data result 
        /// </summary>
        /// <param name="stringContent"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGetTranslatedData(IStringPageContent stringContent, out ITranslatedData result)
        {
            return m_translatedData.TryGetValue(stringContent, out result);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var content in m_pageContents)
            {
                switch (content)
                {
                    case IStringPageContent stringContent:
                        if (m_translatedData.TryGetValue(stringContent, out ITranslatedData translated))
                            sb.AppendLine(translated.TranslatedText);
                        break;
                    case IImagePageContent imageContent:
                        sb.AppendLine($"<IMAGE #{imageContent.Index}>");
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed == false)
            {
                if (disposing)
                {
                    foreach(var image in m_images)
                    {
                        image.Dispose();
                    }
                    m_images.Clear();
                    m_images = null;
                }

                // Cleanup unmanaged resources
            }

            this._disposed = true;
        }
    }
}
