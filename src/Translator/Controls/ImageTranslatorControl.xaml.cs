using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Translator.Commands;
using Translator.Extensions;
using Translator.Interfaces;
using Translator.Wrappers;
using TranslatorBackend.Interfaces;
using Point = System.Windows.Point;

namespace Translator.Controls
{
    /// <summary>
    /// Interaction logic for ImageTranslatorControl.xaml
    /// </summary>
    public partial class ImageTranslatorControl : BaseControl
    {
        #region Internal classes for data
        /// <summary>
        /// Stores the rectangle crop area as a percentage of the 
        /// image
        /// </summary>
        private class CropArea
        {
            public bool IsValid
            {
                get { return Bounds.Width > 0 && Bounds.Height > 0; }
            }
            /// <summary>
            /// Rectangle bounds
            /// </summary>
            public Rect Bounds { get; private set; }
            public CropArea(double x, double y, double width, double height)
            {
                Bounds = new Rect(x, y, width, height);
            }
        }

        /// <summary>
        /// Used for moving around the image in the scrollviewer
        /// </summary>
        private class PanInfo
        {
            private readonly Point m_startPoint;
            private readonly double m_hOffset;
            private readonly double m_vOffset;

            /// <summary>
            /// The start point on mouse down
            /// </summary>
            public Point StartPoint
            {
                get { return m_startPoint; }
            }

            /// <summary>
            /// The horizontal offset of the scrollviewer
            /// </summary>
            public double HOffset
            {
                get { return m_hOffset; }
            }

            /// <summary>
            /// The vertical offset of the scrollviewer
            /// </summary>
            public double VOffset
            {
                get { return m_vOffset; }
            }

            public PanInfo(Point startPoint, double hOffset, double vOffset)
            {
                m_startPoint = startPoint;
                m_hOffset = hOffset;
                m_vOffset = vOffset;
            }
        }
        #endregion

        #region Private Properties
        /// <summary>
        /// The current frame being displayed
        /// </summary>
        private Bitmap m_currentFrame;

        /// <summary>
        /// Flag enum for active image manipulation modes
        /// </summary>
        private ImageManiupualtionMode m_activeModes = ImageManiupualtionMode.NONE;

        /// <summary>
        /// The last layout mode active
        /// </summary>
        private LayoutMode m_previousLayout;

        /// <summary>
        /// The currently active layout mode
        /// </summary>
        private LayoutMode m_currentLayout;

        /// <summary>
        /// List of video devices
        /// </summary>
        private ObservableCollection<CameraSourceWrapper> m_videoDevices =
            new ObservableCollection<CameraSourceWrapper>();

        /// <summary>
        /// The active video source
        /// </summary>
        private VideoCaptureDevice m_videoSource;

        /// <summary>
        /// The selected video device to use
        /// </summary>
        private CameraSourceWrapper m_selectedDevice;

        /// <summary>
        /// Ocr result
        /// </summary>
        private IOcrData m_ocrData;

        /// <summary>
        /// Translation result
        /// </summary>
        private LibreTranslateDataWrapper m_translationResult;

        /// <summary>
        /// If translated text should be show.
        /// </summary>
        private bool m_showTranslatedText;

        /// <summary>
        /// The image pan data
        /// </summary>
        private PanInfo m_panInfo;

        /// <summary>
        /// Starting point for crop
        /// </summary>
        private System.Windows.Point m_startCropPoint;

        /// <summary>
        /// The stored crop area
        /// </summary>
        private CropArea m_cropArea;

        /// <summary>
        /// Current x zoom value
        /// </summary>
        private double m_currentZoomX = 1;

        /// <summary>
        /// Current y zoom value
        /// </summary>
        private double m_currentZoomY = 1;

        /// <summary>
        /// List of registered touch devices
        /// </summary>
        private HashSet<int> m_touchDevices =
            new HashSet<int>();
        #endregion

        #region Public Properties
        /// <summary>
        /// The list of video devices
        /// </summary>
        public ObservableCollection<CameraSourceWrapper> VideoDevices
        {
            get { return m_videoDevices; }
            private set { SetProperty(GetPropertyName(), ref m_videoDevices, value); }
        }

        /// <summary>
        /// The selected video device
        /// </summary>
        public CameraSourceWrapper SelectedVideoDevice
        {
            get { return m_selectedDevice; }
            set
            {
                if (SetProperty(GetPropertyName(), ref m_selectedDevice, value))
                {
                    HandleCaptureDeviceChanged();
                }
            }
        }

        /// <summary>
        /// Active layout
        /// </summary>
        public LayoutMode CurrentLayout
        {
            get { return m_currentLayout; }
            private set
            {
                if (SetProperty(GetPropertyName(), ref m_currentLayout, value))
                {
                    HandleCurrentLayoutChanged();
                }
            }
        }

        /// <summary>
        /// Ocr result
        /// </summary>
        public IOcrData OcrData
        {
            get { return m_ocrData; }
            private set { SetProperty(GetPropertyName(), ref m_ocrData, value); }
        }

        /// <summary>
        /// Translation result
        /// </summary>
        public LibreTranslateDataWrapper TranslationResult
        {
            get { return m_translationResult; }
            private set { SetProperty(GetPropertyName(), ref m_translationResult, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating if the translated text should be shown.
        /// </summary>
        public bool ShowTranslatedText
        {
            get { return m_showTranslatedText; }
            set { SetProperty(GetPropertyName(), ref m_showTranslatedText, value); }
        }

        /// <summary>
        /// Gets a value indicating if camera settings should be shown.
        /// </summary>
        public bool ShowCameraSettings
        {
            get
            {
                return IsLayout(LayoutMode.NONE,
                                LayoutMode.PREVIEW);
            }
        }

        /// <summary>
        /// Gets a value indicating if the image can be zoomed in or panned
        /// </summary>
        public bool CanPanAndZoom
        {
            get
            {
                return IsLayout(LayoutMode.IMAGE,
                                LayoutMode.EDIT,
                                LayoutMode.TRANSLATE,
                                LayoutMode.CROP);
            }
        }

        /// <summary>
        /// Gets or sets the zoom x value
        /// </summary>
        public double CurrentZoomX
        {
            get { return m_currentZoomX; }
            private set
            {
                if (SetProperty(GetPropertyName(), ref m_currentZoomX, value))
                {
                    UpdateCanvasWidth();
                }
            }
        }

        /// <summary>
        /// Gets or sets the zoom y value
        /// </summary>
        public double CurrentZoomY
        {
            get { return m_currentZoomY; }
            private set
            {
                if (SetProperty(GetPropertyName(), ref m_currentZoomY, value))
                {
                    UpdateCanvasHeight();
                }
            }
        }
        #endregion

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="backend"></param>
        /// <param name="networkManager"></param>
        /// <param name="languageManager"></param>
        /// <param name="errorManager"></param>
        public ImageTranslatorControl(ITranslatorBackend backend, INetworkManager networkManager, IServerManager serverManager, ILanguageManager languageManager, IErrorManager errorManager)
            : base(backend, networkManager, serverManager, languageManager, errorManager)
        {
            InitializeComponent();
            BuildCommands();
            CurrentLayout = LayoutMode.NONE;
            ScanForCameras();
        }

        /// <summary>
        /// Scans and builds the list of available camera devices
        /// </summary>
        private void ScanForCameras()
        {
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in videoDevices)
            {
                VideoDevices.Add(new CameraSourceWrapper(device.Name, device.MonikerString));
            }

            if (VideoDevices.Count > 0)
            {
                SelectedVideoDevice = VideoDevices[0];
            }
        }

        /// <summary>
        /// Builds the button command handling lookup
        /// </summary>
        protected override void BuildCommands()
        {
            // Preview layout
            m_commandLookup[ImageCommands.PreviewCommand] = new TranslatorCommand(ProcessPreviewCommand, CanProcessPreviewCommand);
            m_commandLookup[ImageCommands.CaptureCommand] = new TranslatorCommand(ProcessCaptureCommand, IsPreviewActive);
            m_commandLookup[ImageCommands.EndPreviewCommand] = new TranslatorCommand(ProcessEndPreviewCommand, IsPreviewActive);

            // Load layout
            m_commandLookup[ImageCommands.LoadCommand] = new TranslatorCommand(LoadImageFromFile, CanLoadImageFromFile);

            // image controls
            m_commandLookup[ImageCommands.ClearPhotoCommand] = new TranslatorCommand(ProcessClearPhotoCommand, CanProcessClearPhotoCommand);

            // edit commands
            m_commandLookup[ImageCommands.EditCommand] = new TranslatorCommand(ProcessEditCommand, CanProcessEditCommand);
            m_commandLookup[ImageCommands.CancelEditCommand] = new TranslatorCommand(ProcessCancelEditCommand, CanProcessCancelEditCommand);

            // zoom commands
            m_commandLookup[ImageCommands.ZoomInCommand] = new TranslatorCommand(ZoomIn, CanZoomIn);
            m_commandLookup[ImageCommands.ZoomOutCommand] = new TranslatorCommand(ZoomOut, CanZoomOut);
            m_commandLookup[ImageCommands.ZoomResetCommand] = new TranslatorCommand(ResetZoom);

            // crop commands
            m_commandLookup[ImageCommands.CropCommand] = new TranslatorCommand(ProcessCropCommand, CanProcessCropCommand);
            m_commandLookup[ImageCommands.ApplyCropCommand] = new TranslatorCommand(ApplyCrop, CanApplyCrop);
            m_commandLookup[ImageCommands.CancelCropCommand] = new TranslatorCommand(CancelCrop, CanCancelCrop);

            // translation commands
            m_commandLookup[TranslationCommands.TranslateCommand] = new TranslatorCommand(ProcessCommandTranslate, CanProcessCommandTranslate);
            m_commandLookup[TranslationCommands.NextCommand] = new TranslatorCommand(ProcessNextCommand, CanProcessNextCommand);
            m_commandLookup[TranslationCommands.PreviousCommand] = new TranslatorCommand(ProcessPreviousCommand, CanProcessPreviousCommand);
            m_commandLookup[TranslationCommands.ClearCommand] = new TranslatorCommand(ProcessCommandClearTranslate, CanProcessClearTranslateCommand);
        }

        #region Preview
        /// <summary>
        /// Indicates if can do the prievew command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanProcessPreviewCommand()
        {
            return m_videoSource == null &&
                   IsLayout(LayoutMode.NONE);
        }

        /// <summary>
        /// process the preivew command
        /// </summary>
        void ProcessPreviewCommand()
        {
            SetCurrentLayout(LayoutMode.PREVIEW);
        }

        /// <summary>
        /// Indicates if preview is currently running
        /// </summary>
        /// <returns></returns>
        bool IsPreviewActive()
        {
            return m_videoSource != null &&
                   m_videoSource.IsRunning &&
                   m_currentFrame != null;
        }

        /// <summary>
        /// process the end preview command
        /// </summary>
        void ProcessEndPreviewCommand()
        {
            SetCurrentLayout(LayoutMode.NONE);
        }

        /// <summary>
        /// Process the capture command
        /// </summary>
        void ProcessCaptureCommand()
        {
            SetCurrentLayout(LayoutMode.IMAGE);
        }

        /// <summary>
        /// Process the video source selection changed
        /// </summary>
        private void HandleCaptureDeviceChanged()
        {
            if (m_videoSource == null) { return; }

            bool isPlaying = m_videoSource.IsRunning;
            StopVideoSource();
            if (isPlaying)
                StartVideoSource();
        }

        /// <summary>
        /// Start the video source
        /// </summary>
        private void StartVideoSource()
        {
            m_videoSource = new VideoCaptureDevice(m_selectedDevice.MonikerString);
            m_videoSource.NewFrame += VideoSource_NewFrame;
            m_videoSource.Start();
        }

        /// <summary>
        /// Stop the video source
        /// </summary>
        private void StopVideoSource()
        {
            if (m_videoSource != null && m_videoSource.IsRunning)
            {
                m_videoSource.NewFrame -= VideoSource_NewFrame;
                m_videoSource.SignalToStop();
                m_videoSource.WaitForStop();
                m_videoSource = null;
            }
        }

        /// <summary>
        /// Handles video source new frame update event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs args)
        {
            if (!m_videoSource.IsRunning)
            {
                return;
            }

            try
            {
                Bitmap bitmap = (Bitmap)args.Frame.Clone();
                DisplayOrientation orientation = WindowOrientationHelper.GetCurrentDisplayOrientation();

                switch (orientation)
                {
                    case DisplayOrientation.Portrait:
                        switch (m_selectedDevice.CameraDirection)
                        {
                            case CameraDirection.FRONT:
                                bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                            case CameraDirection.REAR:
                                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                        }
                        break;
                    case DisplayOrientation.PortraitFlipped:
                        switch (m_selectedDevice.CameraDirection)
                        {
                            case CameraDirection.FRONT:
                                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                            case CameraDirection.REAR:
                                bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                        }
                        break;
                }

                UpdateCurrentFrame(bitmap);
            }
            catch
            {

            }
        }
        #endregion

        #region Translate
        /// <summary>
        /// Indicates if can do the translate command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanProcessCommandTranslate()
        {
            return m_serverManager.OcrStatus == ServerStatus.CONNECTED &&
                   m_serverManager.TranslatorStatus == ServerStatus.CONNECTED &&
                   m_videoSource == null &&
                   m_currentFrame != null &&
                   TranslationResult == null &&
                   OcrData == null;
        }

        /// <summary>
        /// process the translate command
        /// </summary>
        async void ProcessCommandTranslate()
        {
            IsBusy = true;
            TranslationResult = null;
            OcrData = null;

            try
            {
                byte[] imgBytes = m_currentFrame.ToByteArray(ImageFormat.Png);
                if (imgBytes != null)
                {
                    IImageOcrResult ocrResult = await m_backend.OcrImageAsync(OcrUri, m_languageManager.SourceLanguage.OcrCode, imgBytes);
                    if (ocrResult.Success)
                    {
                        OcrData = ocrResult.Result;

                        ITextTranslationResult translationResult = await m_backend.TranslateTextAsync(TranslationUri, m_languageManager.SourceLanguage.TranslationCode, m_ocrData.Text, m_languageManager.TargetLanguage.TranslationCode, 3);
                        if (translationResult.Success)
                        {
                            TranslationResult = new LibreTranslateDataWrapper(translationResult.Result);
                            DrawRectangleAndTextOnImage();
                            SetCurrentLayout(LayoutMode.TRANSLATE);
                        }
                        else
                            m_errorManager.AddError("Failed Translation: " + translationResult.GetErrorMessage());
                    }
                    else
                    {
                        m_errorManager.AddError("Failed Image Processing: " + ocrResult.GetErrorMessage());
                    }
                }
            }
            catch
            {
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Update the image based on the ocr/translate results
        /// </summary>
        void DrawRectangleAndTextOnImage()
        {
            DispatcherBeginInvoke(() =>
            {
                if (m_ocrData == null)
                    return;

                try
                {
                    BitmapImage img = displayImage.Source as BitmapImage;
                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(img.PixelWidth, img.PixelHeight, 96, 96, PixelFormats.Pbgra32);

                    DrawingVisual visual = new DrawingVisual();
                    using (DrawingContext context = visual.RenderOpen())
                    {
                        // Draw the image iteself
                        context.DrawImage(img, new Rect(0, 0, img.PixelWidth, img.PixelHeight));

                        // Draw boxes around each word using the bounding box defined.
                        System.Windows.Media.Pen rectanglePen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Green, 1);
                        foreach (IWordInfo wordInfo in m_ocrData.WordInfo)
                        {
                            // Do we want to do anything with confidence level here?
                            IBoundingBox boundingBox = wordInfo.BoundingBox;
                            if (boundingBox != null)
                            {
                                // Change this to iterate through the word binding boxes
                                Rect rect = new Rect(boundingBox.X1 - 1, boundingBox.Y1 - 1, boundingBox.Width + 1, boundingBox.Height + 1);

                                // TODO: Change this to detect background color and add translated text
                                context.DrawRectangle(null, rectanglePen, rect);

                                //System.Windows.Point textLocation = new System.Windows.Point(boundingBox.X1 + 2, (boundingBox.Y1 + (height / 2) - (6)));
                                //Typeface typeFace = new Typeface("Times New Roman");
                                //FormattedText formattedText = new FormattedText("test", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeFace, 12, System.Windows.Media.Brushes.Red);
                                //context.DrawText(formattedText, textLocation);
                            }
                        }
                    }
                    renderTargetBitmap.Render(visual);
                    UpdateCurrentFrame(renderTargetBitmap.ToBitmap());
                }
                catch (Exception ex)
                {
                    m_errorManager.AddError("Exception processing image: " + ex.Message);
                }
            });
        }

        /// <summary>
        /// Indicates if can do the next translate command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanProcessNextCommand()
        {
            return m_translationResult != null && m_translationResult.CanGoNext();
        }

        /// <summary>
        /// Process the next translation command
        /// </summary>
        void ProcessNextCommand()
        {
            if (m_translationResult == null) { return; }
            m_translationResult.GoNext();
        }

        /// <summary>
        /// Indicates if can do the previous translation command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanProcessPreviousCommand()
        {
            return m_translationResult != null && m_translationResult.CanGoBack();
        }

        /// <summary>
        /// Process the previous translation command
        /// </summary>
        void ProcessPreviousCommand()
        {
            if (m_translationResult == null) { return; }
            m_translationResult.GoBack();
        }

        /// <summary>
        /// Indicates if can do the clear translation command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanProcessClearTranslateCommand()
        {
            return m_translationResult != null &&
                   m_currentLayout == LayoutMode.TRANSLATE;
        }

        /// <summary>
        /// process the clear translation command
        /// </summary>
        void ProcessCommandClearTranslate()
        {
            TranslationResult = null;
            OcrData = null;
            SetCurrentLayout(LayoutMode.NONE);
        }
        #endregion

        #region Load Image From File
        /// <summary>
        /// Indicates if can do the load image from file command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanLoadImageFromFile()
        {
            return m_videoSource == null;
        }

        /// <summary>
        /// Load an image from file
        /// </summary>
        void LoadImageFromFile()
        {
            ClearCurrentFrameAndImage();
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;|Bitmap (*.bmp)|*.bmp|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                SetCurrentLayout(LayoutMode.IMAGE);
                UpdateCurrentFrame((Bitmap)System.Drawing.Image.FromFile(dlg.FileName));
            }
        }
        #endregion

        #region Capture
        /// <summary>
        /// Indicates if can do the clear photo command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanProcessClearPhotoCommand()
        {
            return m_videoSource == null &&
                   m_currentFrame != null &&
                   IsLayout(LayoutMode.IMAGE);
        }

        /// <summary>
        /// Process the clear photo command
        /// </summary>
        void ProcessClearPhotoCommand()
        {
            // Return to previous layout 
            ResetZoom();
            SetCurrentLayout(m_previousLayout);
        }
        #endregion

        #region Edit
        /// <summary>
        /// Indicates if can do the edit command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanProcessEditCommand()
        {
            return m_currentFrame != null && IsLayout(LayoutMode.IMAGE);
        }

        /// <summary>
        /// Process the edit command
        /// </summary>
        void ProcessEditCommand()
        {
            SetCurrentLayout(LayoutMode.EDIT, false);
        }

        /// <summary>
        /// Indicates if can do the cancel edit command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanProcessCancelEditCommand()
        {
            return m_currentFrame != null && IsLayout(LayoutMode.EDIT);
        }

        /// <summary>
        /// Process the cancel edit command
        /// </summary>
        void ProcessCancelEditCommand()
        {
            SetCurrentLayout(LayoutMode.IMAGE, false);
        }
        #endregion

        #region Pan
        /// <summary>
        /// Start pan on the image
        /// </summary>
        /// <param name="e"></param>
        void StartPan(Point startPoint)
        {
            if (m_activeModes.HasFlag(ImageManiupualtionMode.PAN)) { return; }
            if (!CanPanAndZoom) { return; }
            if (m_currentZoomY <= 1 && m_currentZoomX <= 1) { return; }
            if (m_panInfo != null) return;

            this.Cursor = Cursors.ScrollAll;
            m_panInfo = new PanInfo(startPoint, imageScrollView.HorizontalOffset, imageScrollView.VerticalOffset);
            m_activeModes |= ImageManiupualtionMode.PAN;
            DisplayImageCapture();
        }

        /// <summary>
        /// Update the pan of an image
        /// </summary>
        /// <param name="e"></param>
        void UpdatePan(Point currentPoint)
        {
            if (!m_activeModes.HasFlag(ImageManiupualtionMode.PAN)) { return; }
            if (m_panInfo == null) return;
            var delta = m_panInfo.StartPoint - currentPoint;

            imageScrollView.ScrollToHorizontalOffset(m_panInfo.HOffset + delta.X);
            imageScrollView.ScrollToVerticalOffset(m_panInfo.VOffset + delta.Y);
        }

        /// <summary>
        /// End panning of the image
        /// </summary>
        void EndPan()
        {
            if (!m_activeModes.HasFlag(ImageManiupualtionMode.PAN)) { return; }

            this.Cursor = Cursors.Arrow;
            m_panInfo = null;
            m_activeModes &= ~ImageManiupualtionMode.PAN;
            ReleaseDisplayImageCapture();
        }
        #endregion

        #region Crop
        /// <summary>
        /// Indicates if can do the crop command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanProcessCropCommand()
        {
            return m_currentFrame != null && IsLayout(LayoutMode.EDIT);
        }

        /// <summary>
        /// process the crop command
        /// </summary>
        void ProcessCropCommand()
        {
            SetCurrentLayout(LayoutMode.CROP, false);
        }

        /// <summary>
        /// Indicates if can do apply crop
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanApplyCrop()
        {
            return m_cropArea != null && m_cropArea.IsValid;
        }

        /// <summary>
        /// Apply the crop
        /// </summary>
        void ApplyCrop()
        {
            try
            {
                BitmapImage bitmapImage = displayImage.Source as BitmapImage;
                var imageBounds = GetImageBounds();
                // get the scale of the image
                double scaleX = displayImage.ActualWidth / bitmapImage.PixelWidth;
                double scaleY = displayImage.ActualHeight / bitmapImage.PixelHeight;

                // Adjust based on zoom of image
                scaleX *= m_currentZoomX;
                scaleY *= m_currentZoomY;

                // get the crop x/y/width/height
                double cropX = (Canvas.GetLeft(cropRectangle) - imageBounds.X) / scaleX;
                double cropY = (Canvas.GetTop(cropRectangle) - imageBounds.Y) / scaleY;
                double cropWidth = cropRectangle.Width / scaleX;
                double cropHeight = cropRectangle.Height / scaleY;

                // Clamp the x and y to the bounds of the image 
                cropX = Math.Max(0, Math.Min(cropX, bitmapImage.PixelWidth - cropWidth));
                cropY = Math.Max(0, Math.Min(cropY, bitmapImage.PixelHeight - cropHeight));

                var croptRect = new Int32Rect((int)cropX,
                                              (int)cropY,
                                              (int)cropWidth,
                                              (int)cropHeight);

                var bitmap = new CroppedBitmap(bitmapImage, croptRect);
                UpdateCurrentFrame(bitmap.ToBitmap());
                ResetZoom();
            }
            catch (Exception ex)
            {
                m_errorManager.AddError("Failure to crop image: " + ex.Message);
            }

            SetCurrentLayout(LayoutMode.IMAGE, false);
            ClearCrop();
        }

        /// <summary>
        /// Indicates if can do the cancel crop command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanCancelCrop()
        {
            return IsLayout(LayoutMode.CROP);
        }

        /// <summary>
        /// Cancel the crop
        /// </summary>
        void CancelCrop()
        {
            SetCurrentLayout(LayoutMode.EDIT, false);
            ClearCrop();
        }

        /// <summary>
        /// Clear the stored data related to cropping
        /// </summary>
        void ClearCrop()
        {
            cropRectangle.Width = 0;
            cropRectangle.Height = 0;
            cropRectangle.Visibility = Visibility.Collapsed;
            m_activeModes &= ~ImageManiupualtionMode.CROP;
            m_cropArea = null;
        }

        /// <summary>
        /// Adjusts the crop rectangle size and location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void SetCropRectangle(double x, double y, double width, double height)
        {
            Canvas.SetLeft(cropRectangle, x);
            Canvas.SetTop(cropRectangle, y);
            cropRectangle.Width = width;
            cropRectangle.Height = height;
        }

        /// <summary>
        /// Start the cropping of an image
        /// </summary>
        /// <param name="e"></param>
        void StartCrop(Point startPoint)
        {
            if (m_activeModes.HasFlag(ImageManiupualtionMode.CROP)) { return; }
            if (!IsLayout(LayoutMode.CROP)) { return; }

            m_startCropPoint = startPoint;
            cropRectangle.Visibility = Visibility.Visible;
            Canvas.SetLeft(cropRectangle, m_startCropPoint.X);
            Canvas.SetTop(cropRectangle, m_startCropPoint.Y);
            cropRectangle.Width = 0;
            cropRectangle.Height = 0;
            DisplayImageCapture();
            m_activeModes |= ImageManiupualtionMode.CROP;
        }

        /// <summary>
        /// Update the cropping of an image
        /// </summary>
        /// <param name="e"></param>
        void UpdateCrop(Point currentPoint)
        {
            if (!m_activeModes.HasFlag(ImageManiupualtionMode.CROP)) { return; }

            Rect imageBounds = GetImageBounds();
            currentPoint.X = Math.Max(imageBounds.X, Math.Min(currentPoint.X, imageBounds.X + imageBounds.Width));
            currentPoint.Y = Math.Max(imageBounds.Y, Math.Min(currentPoint.Y, imageBounds.Y + imageBounds.Height));

            double x = Math.Min(currentPoint.X, m_startCropPoint.X);
            double y = Math.Min(currentPoint.Y, m_startCropPoint.Y);
            double width = Math.Abs(currentPoint.X - m_startCropPoint.X);
            double height = Math.Abs(currentPoint.Y - m_startCropPoint.Y);

            SetCropRectangle(x, y, width, height);
        }

        /// <summary>
        /// End the cropping of an image
        /// </summary>
        void EndCrop()
        {
            if (!m_activeModes.HasFlag(ImageManiupualtionMode.CROP)) { return; }

            Rect imageBounds = GetImageBounds();
            // Save the area of the crop rectangle to handle resizing the crop rectangle when the image is resized
            m_cropArea = new CropArea((Canvas.GetLeft(cropRectangle) - imageBounds.X) / imageBounds.Width,
                                      (Canvas.GetTop(cropRectangle) - imageBounds.Y) / imageBounds.Height,
                                      cropRectangle.Width / imageBounds.Width,
                                      cropRectangle.Height / imageBounds.Height);
            m_activeModes &= ~ImageManiupualtionMode.CROP;
            ReleaseDisplayImageCapture();
        }

        /// <summary>
        /// Get the bounds of the image
        /// </summary>
        /// <returns></returns>
        Rect GetImageBounds()
        {
            Vector imageOffset = VisualTreeHelper.GetOffset(displayImage);

            // returns the bounds of the image 
            return new Rect(imageOffset.X,
                            imageOffset.Y,
                            displayImage.ActualWidth * m_currentZoomX,
                            displayImage.ActualHeight * m_currentZoomY);
        }

        /// <summary>
        /// Resize the crop area 
        /// </summary>
        void ResizeCropArea()
        {
            if (m_cropArea != null && IsLayout(LayoutMode.CROP))
            {
                Rect imageBounds = GetImageBounds();
                double x = imageBounds.X + m_cropArea.Bounds.X * imageBounds.Width;
                double y = imageBounds.Y + m_cropArea.Bounds.Y * imageBounds.Height;
                double width = m_cropArea.Bounds.Width * imageBounds.Width;
                double height = m_cropArea.Bounds.Height * imageBounds.Height;

                SetCropRectangle(x, y, width, height);
            }
        }
        #endregion

        #region Image Zoom
        void StartZoom()
        {
            if (m_activeModes.HasFlag(ImageManiupualtionMode.ZOOM)) { return; }
            if (!CanPanAndZoom) { return; }
            m_activeModes |= ImageManiupualtionMode.ZOOM;
        }

        void UpdateZoom(double deltaX, double deltaY)
        {
            if (!m_activeModes.HasFlag(ImageManiupualtionMode.ZOOM)) { return; }

            ProcessZoom(deltaX, deltaY);
        }

        void EndZoom()
        {
            if (!m_activeModes.HasFlag(ImageManiupualtionMode.ZOOM)) { return; }
            m_activeModes &= ~ImageManiupualtionMode.ZOOM;
        }

        /// <summary>
        /// Resets the zoom values to 1
        /// </summary>
        void ResetZoom()
        {
            CurrentZoomX = 1;
            CurrentZoomY = 1;
            ResizeCropArea();
        }

        /// <summary>
        /// Indicates if can do the zoom in command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanZoomIn()
        {
            return CanPanAndZoom;
        }

        /// <summary>
        /// Zoom in on the image
        /// </summary>
        void ZoomIn()
        {
            ProcessZoom(1.1, 1.1);
        }

        /// <summary>
        /// Indicates if can do the zoom out command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanZoomOut()
        {
            return CanPanAndZoom;
        }

        /// <summary>
        /// Zoom out on the image
        /// </summary>
        void ZoomOut()
        {
            ProcessZoom(-1.1, -1.1);
        }

        /// <summary>
        /// Updates the canvas width
        /// </summary>
        void UpdateCanvasWidth()
        {
            if (m_currentZoomX == 1)
            {
                // clear out the width if we are a zoom of 1
                imageCanvas.Width = double.NaN;
            }
            else
            {
                // update the canvas width based on the zoom
                imageCanvas.Width = displayImage.Width * m_currentZoomX;
            }
        }

        /// <summary>
        /// Updates the canvas height
        /// </summary>
        void UpdateCanvasHeight()
        {
            if (m_currentZoomY == 1)
            {
                // clear out the height if we are a zoom of 1
                imageCanvas.Height = double.NaN;
            }
            else
            {
                // update the canvas height based on the zoom
                imageCanvas.Height = displayImage.Height * m_currentZoomY;
            }
        }

        /// <summary>
        /// Update the canvas width and height
        /// </summary>
        void UpdateCanvasWidthAndHeight()
        {
            UpdateCanvasWidth();
            UpdateCanvasHeight();
        }

        /// <summary>
        /// Set the zoom x and y 
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        private void ProcessZoom(double deltaX, double deltaY)
        {
            double zoomX = m_currentZoomX;
            double zoomY = m_currentZoomY;

            if (deltaX > 0)
                zoomX *= deltaX;
            else
                zoomX /= Math.Abs(deltaX);

            if (deltaY > 0)
                zoomY *= deltaY;
            else
                zoomY /= Math.Abs(deltaX);

            CurrentZoomX = Math.Max(.5, Math.Min(5, zoomX));
            CurrentZoomY = Math.Max(.5, Math.Min(5, zoomY));
            ResizeCropArea();
        }
        #endregion

        #region Display Image Event Handling
        /// <summary>
        /// Handles the display image on [preview mouse down] event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState != MouseButtonState.Pressed)
                return;

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    StartCrop(e.GetPosition(imageCanvas));
                    e.Handled = true;
                    break;
                case MouseButton.Middle:
                    StartPan(e.GetPosition(imageScrollView));
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Handles the display image on [preview mouse up] event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayImage_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState != MouseButtonState.Released)
                return;

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    EndCrop();
                    e.Handled = true;
                    break;
                case MouseButton.Middle:
                    EndPan();
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Handles the display image on [preview mouse move] event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayImage_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateCrop(e.GetPosition(imageCanvas));
                e.Handled = true;
            }

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                UpdatePan(e.GetPosition(imageScrollView));
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the display image on [size changed] event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void displayImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCanvasWidthAndHeight();
        }
        #endregion

        #region Image ScrollViewer Event Handling
        /// <summary>
        /// Handles the image scrollviewer on [mouse wheel] event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imageScrollView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (CanPanAndZoom)
                {
                    var position = e.GetPosition(displayImage);

                    if (e.Delta > 0)
                        ZoomIn();
                    else
                        ZoomOut();

                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles the image scrollviewer on [size changed] event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imageScrollView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeCropArea();
            e.Handled = true;
        }
        #endregion

        #region Touch Screen Processing
        private void displayImage_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = imageScrollView;
            e.Mode = ManipulationModes.Translate | ManipulationModes.Scale;
            e.Handled = true;
        }

        private void displayImage_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            EndCrop();
            EndPan();
            EndZoom();
            e.Handled = true;
        }

        private void displayImage_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (m_touchDevices.Count == 0)
                return;

            if (IsLayout(LayoutMode.CROP))
            {
                var element = e.ManipulationContainer as UIElement;
                var imageCanvasRelativePoint = element.TranslatePoint(e.ManipulationOrigin, imageCanvas);

                StartCrop(imageCanvasRelativePoint);
                UpdateCrop(imageCanvasRelativePoint);
            }
            else
            {
                if (CanPanAndZoom)
                {
                    bool isPanning = e.DeltaManipulation.Translation.Length > 0.1;
                    bool isZooming = Math.Abs(e.DeltaManipulation.Scale.X - 1.0) > 0.01 ||
                                     Math.Abs(e.DeltaManipulation.Scale.Y - 1.0) > 0.01;
                    if (isPanning)
                    {
                        StartPan(e.ManipulationOrigin);
                        UpdatePan(e.ManipulationOrigin);
                    }

                    if (isZooming && m_touchDevices.Count >= 2)
                    {
                        StartZoom();
                        UpdateZoom(e.DeltaManipulation.Scale.X, e.DeltaManipulation.Scale.Y);
                    }
                }
            }
            e.Handled = true;
        }

        private void displayImage_TouchDown(object sender, TouchEventArgs e)
        {
            m_touchDevices.Add(e.TouchDevice.Id);
        }

        private void displayImage_TouchUp(object sender, TouchEventArgs e)
        {
            m_touchDevices.Remove(e.TouchDevice.Id);
        }

        private void displayImage_TouchLeave(object sender, TouchEventArgs e)
        {
            m_touchDevices.Remove(e.TouchDevice.Id);
        }
        #endregion

        #region Image Processing
        /// <summary>
        /// Updates the current frame and displayed image source
        /// </summary>
        /// <param name="source"></param>
        void UpdateCurrentFrame(Bitmap source)
        {
            try
            {
                if (m_currentFrame != null)
                    m_currentFrame.Dispose();

                m_currentFrame = source;

                DispatcherBeginInvoke(() =>
                {
                    displayImage.Source = m_currentFrame.ToBitmapImage();
                });
            }
            catch
            {
                // exception
            }
        }

        /// <summary>
        /// Clears the current frame and displayed image source
        /// </summary>
        void ClearCurrentFrameAndImage()
        {
            if (m_currentFrame != null)
            {
                m_currentFrame.Dispose();
                m_currentFrame = null;
            }

            DispatcherBeginInvoke(() =>
            {
                displayImage.Source = null;
            });
        }

        /// <summary>
        /// Capture the displayed image 
        /// </summary>
        void DisplayImageCapture()
        {
            if (m_activeModes.HasFlag(ImageManiupualtionMode.CROP) ||
                m_activeModes.HasFlag(ImageManiupualtionMode.PAN))
            {
                if (!displayImage.IsMouseCaptured)
                    displayImage.CaptureMouse();
            }
        }

        /// <summary>
        /// Release the displayed image
        /// </summary>
        void ReleaseDisplayImageCapture()
        {
            if (!m_activeModes.HasFlag(ImageManiupualtionMode.CROP) &&
                !m_activeModes.HasFlag(ImageManiupualtionMode.PAN))
            {
                if (displayImage.IsMouseCaptured)
                    displayImage.ReleaseMouseCapture();
            }
        }
        #endregion

        #region Layout
        /// <summary>
        /// Checks if the current layout mode matches a specified mode
        /// </summary>
        /// <param name="layouts"></param>
        /// <returns></returns>
        bool IsLayout(params LayoutMode[] layouts)
        {
            if (layouts == null) { return false; }
            foreach (var layout in layouts)
            {
                if (m_currentLayout == layout)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the current layout
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="savePrevious"></param>
        void SetCurrentLayout(LayoutMode layout, bool savePrevious = true)
        {
            if (savePrevious)
                m_previousLayout = m_currentLayout;

            CurrentLayout = layout;
        }

        /// <summary>
        /// Handles the current layout changed
        /// </summary>
        void HandleCurrentLayoutChanged()
        {
            switch (m_currentLayout)
            {
                case LayoutMode.NONE:
                    OcrData = null;
                    TranslationResult = null;
                    ShowTranslatedText = false;
                    StopVideoSource();
                    ClearCurrentFrameAndImage();
                    break;
                case LayoutMode.PREVIEW:
                    ClearCurrentFrameAndImage();
                    StartVideoSource();
                    break;
                case LayoutMode.IMAGE:
                    StopVideoSource();
                    break;
            }

            if (!CanPanAndZoom && !IsLayout(LayoutMode.CROP))
                ResetZoom();

            RaisePropertyChanged("CanPanAndZoom");
            RaisePropertyChanged("ShowCameraSettings");
        }
        #endregion

        #region IModule
        /// <summary>
        /// IModule start implementation
        /// </summary>
        public override void OnStart()
        {
            base.OnStart();
        }

        /// <summary>
        /// IModule stop implementation
        /// </summary>
        public override void OnStop()
        {
            base.OnStop();
            StopVideoSource();
            ClearCurrentFrameAndImage();
        }
        #endregion
    }
}
