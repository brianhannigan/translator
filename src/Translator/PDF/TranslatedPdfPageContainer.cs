using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using Translator.Interfaces;
using VTEControls;

namespace Translator.PDF
{
    public class TranslatedPdfPageContainer : PropertyHandler, IDisposable
    {
        /// <summary>
        /// A value indicating whether this instance is disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The index of the current selected page
        /// </summary>
        private int m_currentIndex;

        /// <summary>
        /// The currently selected page
        /// </summary>
        private TranslatedPdfPage m_currentPage;

        /// <summary>
        /// The language used for translation
        /// </summary>
        private readonly string m_language;

        /// <summary>
        /// The translated pages
        /// </summary>
        private List<TranslatedPdfPage> m_pages = new List<TranslatedPdfPage>();

        /// <summary>
        /// The list of pages
        /// </summary>
        public IEnumerable<TranslatedPdfPage> Pages
        {
            get { return m_pages; }
        }

        /// <summary>
        /// The language used for the translation
        /// </summary>
        public string Language
        {
            get { return m_language; }
        }

        /// <summary>
        /// Gets or sets the current page
        /// </summary>
        public TranslatedPdfPage CurrentPage
        {
            get { return m_currentPage; }
            private set { SetProperty(GetPropertyName(), ref m_currentPage, value); }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="languageCode"></param>
        public TranslatedPdfPageContainer(string languageCode, IEnumerable<PdfDataPage> pages)
        {
            m_language = languageCode;
            ParsePages(pages);
        }

        private void ParsePages(IEnumerable<PdfDataPage> pages)
        {
            if (pages == null) { return; }
            foreach (var page in pages)
            {
                m_pages.Add(new TranslatedPdfPage(page));
            }
        }

        /// <summary>
        /// Gets a translated page based on index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TranslatedPdfPage GetPageAt(int index)
        {
            return m_pages[index];
        }

        /// <summary>
        /// Goes to a page
        /// </summary>
        /// <param name="pageNum"></param>
        public void GoToPage(int pageNum)
        {
            if (pageNum >= 0 && pageNum < m_pages.Count)
            {
                m_currentIndex = pageNum;
            }
            UpdateCurrentPage();
        }

        /// <summary>
        /// Determines if can go to next page
        /// </summary>
        /// <returns></returns>
        public bool CanGoNext()
        {
            return m_currentIndex < m_pages.Count - 1;
        }

        /// <summary>
        /// Go to next page
        /// </summary>
        public void GoNext()
        {
            if (CanGoNext())
                m_currentIndex++;

            UpdateCurrentPage();
        }

        /// <summary>
        /// Determines if can go back
        /// </summary>
        /// <returns></returns>
        public bool CanGoBack()
        {
            return m_currentIndex > 0;
        }

        /// <summary>
        /// Go back a page
        /// </summary>
        public void GoBack()
        {
            if (CanGoBack())
                m_currentIndex--;

            UpdateCurrentPage();
        }

        /// <summary>
        /// Navigate to another page
        /// </summary>
        private void UpdateCurrentPage()
        {
            CurrentPage = m_pages[m_currentIndex];
        }

        /// <summary>
        /// Exports all pages to a pdf file.
        /// </summary>
        /// <param name="filePath"></param>
        public IEnumerable<int> ExportPdf(string filePath)
        {
            Document exportedDocument = new Document();
            exportedDocument.Info.Title = Path.GetFileName(filePath);

            for (int i = 0; i < m_pages.Count; i++)
            {
                TranslatedPdfPage page = m_pages[i];

                // Add a section to the document
                Section section = exportedDocument.AddSection();

                // It is recommended to clone the DefaultPageSetup before modifying it
                // if you want to base your settings on the default but then adjust
                // for your specific needs, rather than modifying DefaultPageSetup directly.
                // This is because DefaultPageSetup is generally unchangeable, and you should work with clones.
                section.PageSetup = exportedDocument.DefaultPageSetup.Clone();

                // Set narrow margins using the Unit class
                section.PageSetup.TopMargin = Unit.FromCentimeter(1.5); // Example: 1.5 cm top margin
                section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5); // Example: 1.5 cm bottom margin
                section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5); // Example: 1.5 cm left margin
                section.PageSetup.RightMargin = Unit.FromCentimeter(1.5); // Example: 1.5 cm right margin

                Paragraph paragraph = section.AddParagraph();
                foreach (var content in page.PageContents)
                {
                    switch (content)
                    {
                        case IStringPageContent stringContent:
                            if (page.TryGetTranslatedData(stringContent, out ITranslatedData result))
                            {
                                paragraph.AddText(result.TranslatedText);
                            }
                            break;
                        case IImagePageContent imageContent:
                            try
                            {
                                if (imageContent.ImageBytes != null)
                                {
                                    MigraDoc.DocumentObjectModel.Shapes.Image image = paragraph.AddImage($"base64:{Convert.ToBase64String(imageContent.ImageBytes)}");
                                    image.LockAspectRatio = true;

                                    // Calculate max allowed dimensions on the page (considering margins)
                                    double maxWidth = section.PageSetup.PageWidth.Point - section.PageSetup.LeftMargin.Point - section.PageSetup.RightMargin.Point; //
                                    double maxHeight = section.PageSetup.PageHeight.Point - section.PageSetup.TopMargin.Point - section.PageSetup.BottomMargin.Point; //

                                    double imageWidth = imageContent.Content.WidthInSamples;
                                    double imageHeight = imageContent.Content.HeightInSamples;

                                    // Determine if scaling is required
                                    bool scalingRequired = imageWidth >= maxWidth || imageHeight >= maxHeight;
                                    if (scalingRequired)
                                    {
                                        // Calculate scaling factors for both width and height
                                        double widthScale = maxWidth / imageWidth;
                                        double heightScale = maxHeight / imageHeight;

                                        // Use the smaller scale factor to ensure the image fits both dimensions
                                        double scaleFactor = Math.Min(widthScale, heightScale);

                                        // set the image width and 
                                        image.Width = Unit.FromPoint(imageWidth * scaleFactor);
                                    }

                                    paragraph.AddLineBreak();
                                }
                            }
                            catch
                            {

                            }
                            break;
                    }
                }
                yield return i;
            }

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(); // true for Unicode support
            pdfRenderer.Document = exportedDocument;
            pdfRenderer.RenderDocument();

            pdfRenderer.PdfDocument.Save(filePath);
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
                    foreach (var page in m_pages)
                    {
                        page.Dispose();
                    }
                    m_pages.Clear();
                    m_pages = null;
                }

                // Cleanup unmanaged resources
            }

            this._disposed = true;
        }
    }
}
