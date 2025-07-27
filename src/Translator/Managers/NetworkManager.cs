using System.Net;
using Translator.Interfaces;
using VTEControls;

namespace Translator.Managers
{
    public class NetworkManager : PropertyHandler, INetworkManager
    {
        private string m_translationIpAddress;
        private string m_ocrIpAddress;
        private int m_translationPort;
        private int m_ocrPort;

        public string TranslationIpAddress
        {
            get { return m_translationIpAddress; }
            set { SetProperty(GetPropertyName(), ref m_translationIpAddress, value); }
        }

        public int TranslationPort
        {
            get { return m_translationPort; }
            set { SetProperty(GetPropertyName(), ref m_translationPort, value); }
        }

        public string OcrIpAddress
        {
            get { return m_ocrIpAddress; }
            set { SetProperty(GetPropertyName(), ref m_ocrIpAddress, value); }
        }

        public int OcrPort
        {
            get { return m_ocrPort; }
            set { SetProperty(GetPropertyName(), ref m_ocrPort, value); }
        }

        public NetworkManager()
        {
            TranslationIpAddress = IPAddress.Loopback.ToString();
            TranslationPort = 5000;
            OcrIpAddress = IPAddress.Loopback.ToString();
            OcrPort = 5100;
        }

        public void Load(INetworkManager manager)
        {
            if (manager != null)
            {
                if (!string.IsNullOrWhiteSpace(manager.TranslationIpAddress))
                    TranslationIpAddress = manager.TranslationIpAddress;

                if(manager.TranslationPort > 0)
                    TranslationPort = manager.TranslationPort;

                if (!string.IsNullOrWhiteSpace(manager.OcrIpAddress))
                    OcrIpAddress = manager.OcrIpAddress;

                if (manager.OcrPort > 0)
                    OcrPort = manager.OcrPort;
            }
        }
    }
}
