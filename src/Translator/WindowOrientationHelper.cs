using System.Runtime.InteropServices;

namespace Translator
{
    internal enum DisplayOrientation
    {
        Landscape = 0,
        Portrait = 1,
        LandscapeFlipped = 2,
        PortraitFlipped = 3
    }

    internal static class WindowOrientationHelper
    {
        const int ENUM_CURRENT_SETTINGS = 0x00000000;

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        public static DisplayOrientation GetCurrentDisplayOrientation()
        {
            DEVMODE devMode = new DEVMODE();
            if(EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode))
            {
                switch (devMode.dmDisplayOrientation)
                {
                    case 0:
                        return DisplayOrientation.Landscape;
                    case 1:
                        return DisplayOrientation.Portrait;
                    case 2:
                        return DisplayOrientation.LandscapeFlipped;
                    case 3:
                        return DisplayOrientation.PortraitFlipped;
                    default:
                        break;
                }
            }
            return DisplayOrientation.Landscape;
        }
    }
}
