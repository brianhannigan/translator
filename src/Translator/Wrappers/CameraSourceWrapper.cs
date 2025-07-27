using System;
using VTEControls;

namespace Translator.Wrappers
{
    public class CameraSourceWrapper : PropertyHandler
    {
        private readonly string m_name;
        private readonly string m_monikerString;
        private CameraDirection m_cameraDirection;

        public string Name
        {
            get { return m_name; }
        }

        public string MonikerString
        {
            get { return m_monikerString; }
        }

        public CameraDirection CameraDirection
        {
            get { return m_cameraDirection; }
            set { SetProperty(GetPropertyName(), ref m_cameraDirection, value); }
        }

        public CameraSourceWrapper(string name, string monikerString)
        {
            m_name = name;
            m_monikerString = monikerString;
            ProcessHeuristicName();
        }

        void ProcessHeuristicName()
        {
            if (string.IsNullOrWhiteSpace(m_name)) { return; }

            if (m_name.IndexOf("front", StringComparison.OrdinalIgnoreCase) > 0)
                CameraDirection = CameraDirection.FRONT;
            else if (m_name.IndexOf("rear", StringComparison.OrdinalIgnoreCase) > 0)
                CameraDirection = CameraDirection.REAR;
        }
    }
}
