using Translator.Interfaces;
using UglyToad.PdfPig.Content;

namespace Translator.PDF
{
    public abstract class PageContent<T> : IPageContent<T>
    {
        public T Content { get; }

        public PageContent(T content)
        {
            Content = content;
        }

        public virtual double Top { get; }
        public virtual double Left { get; }
        public virtual double Right { get; }
        public virtual double Width { get; }
        public virtual double Height { get; }

        public virtual bool HasValidContent()
        {
            return Content != null;
        }
    }

    public class LetterPageContent : PageContent<Letter>, ILetterPageContent
    {
        public LetterPageContent(Letter content)
            : base(content)
        {
        }

        public override double Top => Content.StartBaseLine.Y;
        public override double Left => Content.GlyphRectangle.Left;
        public override double Right => Content.GlyphRectangle.Right;
        public override double Width => Content.GlyphRectangle.Width;
        public override double Height => Content.GlyphRectangle.Height;
    }

    public class StringPageContent : PageContent<string>, IStringPageContent
    {
        public StringPageContent(string content)
            : base(content)
        {
        }

        public override bool HasValidContent()
        {
            return Content != null && !string.IsNullOrWhiteSpace(Content);
        }
    }

    public class ImagePageContent : PageContent<IPdfImage>, IImagePageContent
    {
        /// <summary>
        /// The index of the image
        /// </summary>
        private readonly int m_index;

        /// <summary>
        /// The image bytes
        /// </summary>
        private readonly byte[] m_imageBytes;

        /// <summary>
        /// Gets the image bytes
        /// </summary>
        public byte[] ImageBytes
        {
            get { return m_imageBytes; }
        }

        public int Index { get { return m_index; } }

        public ImagePageContent(IPdfImage content, int index)
            : base(content)
        {
            m_index = index;

            if (!content.TryGetPng(out m_imageBytes))
            {
                m_imageBytes = content.RawBytes.ToArray();
            }
        }

        public override double Top => Content.Bounds.Top;
        public override double Left => Content.Bounds.Left;
        public override double Right => Content.Bounds.Right;
        public override double Width => Content.Bounds.Width;
        public override double Height => Content.Bounds.Height;

        public bool TryGetPng(out byte[] pngBytes)
        {
            pngBytes = null;
            if (Content == null)
                return false;

            return Content.TryGetPng(out pngBytes);
        }
    }
}
