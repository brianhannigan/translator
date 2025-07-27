using UglyToad.PdfPig.Content;

namespace Translator.Interfaces
{
    public interface IPageContent
    {
    }

    public interface IContentHolder<T> : IPageContent
    {
        T Content { get; }
        bool HasValidContent();
    }

    public interface IPageContent<T> : IContentHolder<T>
    {
    }


    public interface IStringPageContent : IPageContent<string>
    {
    }

    public interface IImagePageContent : ILocationPageContent<IPdfImage>
    {
        int Index { get; }
        byte[] ImageBytes { get; }
        bool TryGetPng(out byte[] pngBytes);
    }

    public interface ILetterPageContent : ILocationPageContent<Letter>
    {
    }

    public interface ILocationPageContent : IPageContent
    {
        double Top { get; }
        double Left { get; }
        double Right { get; }
        double Width { get; }
        double Height { get; }
    }

    public interface ILocationPageContent<T> : IContentHolder<T>, ILocationPageContent
    {
    }
}
