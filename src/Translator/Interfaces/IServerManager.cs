namespace Translator.Interfaces
{
    public interface IServerManager
    {
        ServerStatus OcrStatus { get; }
        ServerStatus TranslatorStatus { get; }
    }
}
