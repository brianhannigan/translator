using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator.Commands
{
    public static class TranslationCommands
    {
        public const string TranslateCommand = "Translate";
        public const string NextCommand = "Next";
        public const string PreviousCommand = "Previous";
        public const string ClearCommand = "Clear Translation";
        public const string ExportCommand = "Export";
    }

    public static class ImageCommands
    {
        public const string CaptureCommand = "Capture";
        public const string EditCommand = "Edit";
        public const string CropCommand = "Crop";
        public const string LoadCommand = "Load Image";
        public const string PreviewCommand = "Preview";
        public const string EndPreviewCommand = "End Preview";
        public const string ClearPhotoCommand = "Clear Photo";
        public const string CancelEditCommand = "Cancel Edit";
        public const string CancelCropCommand = "Cancel Crop";
        public const string ApplyCropCommand = "Apply Crop";
        public const string ZoomInCommand = "Zoom In";
        public const string ZoomOutCommand = "Zoom Out";
        public const string ZoomResetCommand = "Reset Zoom";
    }

    public static class PdfCommands
    {
        public const string ZoomInCommand = "Zoom In";
        public const string ZoomOutCommand = "Zoom Out";
        public const string ZoomResetCommand = "Reset Zoom";

        public const string ClearPdfCommand = "Clear PDF";
        public const string LoadCommand = "Load PDF";
    }
}
