using System.Collections.Generic;
using Translator.Extensions;
using Translator.Interfaces;
using UglyToad.PdfPig.Content;
using VTEControls;

namespace Translator.PDF
{
    public class PdfDataPage : PropertyHandler
    {
        private readonly Page m_pdfPage;
        private List<IPageContent> m_pageContents;

        public IEnumerable<IPageContent> OrderedContents
        {
            get
            {
                foreach (IPageContent content in m_pageContents)
                {
                    yield return content;
                }
            }
        }

        public Page PdfPage
        {
            get { return m_pdfPage; }
        }

        public PdfDataPage(Page pdfPage)
        {
            m_pdfPage = pdfPage;
            m_pageContents = new List<IPageContent>(m_pdfPage.GetPageContents());
        }
    }
}
