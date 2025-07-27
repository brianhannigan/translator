namespace Translator.Interfaces
{
    public interface INetworkManager
    {
        string TranslationIpAddress { get; }
        int TranslationPort { get; }
        string OcrIpAddress { get; }
        int OcrPort { get; } 
        void Load(INetworkManager manager);
    }
}
