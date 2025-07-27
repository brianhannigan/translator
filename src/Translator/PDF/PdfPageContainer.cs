using System;
using System.Collections.Generic;
using UglyToad.PdfPig;
using VTEControls;

namespace Translator.PDF
{
    public class PdfPageContainer : PropertyHandler
    {
        /// <summary>
        /// The pdf file path
        /// </summary>
        private string m_pdfPath;

        /// <summary>
        /// The currently loaded pdf file
        /// </summary>
        private PdfDocument m_pdfDoc;

        /// <summary>
        /// The list of pages
        /// </summary>
        private List<PdfDataPage> m_pages = new List<PdfDataPage>();

        /// <summary>
        /// The list of pages
        /// </summary>
        public IEnumerable<PdfDataPage> Pages
        {
            get { return m_pages; }
        }

        /// <summary>
        /// Indicates if a pdf file is loaded
        /// </summary>
        public bool IsPdfLoaded
        {
            get { return m_pdfDoc != null; }
        }

        /// <summary>
        /// The path to the loaded pdf file
        /// </summary>
        public string PdfPath
        {
            get { return m_pdfPath; }
            private set { SetProperty(GetPropertyName(), ref m_pdfPath, value); }
        }

        public bool LoadPdf(string filePath)
        {
            bool success;
            PdfPath = filePath;
            try
            {
                m_pdfDoc = PdfDocument.Open(m_pdfPath);
                for (int pageNumber = 0; pageNumber < m_pdfDoc.NumberOfPages; pageNumber++)
                {
                    var page = m_pdfDoc.GetPage(pageNumber + 1); // PdfPig is 1-indexed
                    m_pages.Add(new PdfDataPage(page));
                }
                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        public void Clear()
        {
            PdfPath = null;
            m_pages.Clear();
            if (m_pdfDoc != null)
            {
                m_pdfDoc.Dispose();
                m_pdfDoc = null;
            }
        }
    }
}
