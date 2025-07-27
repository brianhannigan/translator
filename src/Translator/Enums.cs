using System;

namespace Translator
{
    public enum LayoutMode { NONE, PREVIEW, IMAGE, EDIT, CROP, TRANSLATE }
    public enum CameraDirection { FRONT, REAR }

    [Flags]
    public enum ImageManiupualtionMode
    {
        NONE = 0,
        CROP = 1,
        ZOOM = 2,
        PAN = 4
    }

    public enum PdfLayoutMode { NONE, PREVIEW, TRANSLATE }

    public enum ServerStatus { DISCONNECTED, CONNECTED }
}
