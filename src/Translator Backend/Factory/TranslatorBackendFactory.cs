using TranslatorBackend.Interfaces;

namespace TranslatorBackend.Factory
{
    public static class TranslatorBackendFactory
    {
        public static ITranslatorBackend CreateNewBackend()
        {
            return new TranslatorBackend();
        }
    }
}
