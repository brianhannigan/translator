using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Translator.Commands;
using Translator.Interfaces;
using Translator.PDF;
using Translator.Wrappers;
using TranslatorBackend.Interfaces;

namespace Translator.Controls
{
    /// <summary>
    /// Interaction logic for PdfTranslatorControl.xaml
    /// </summary>
    public partial class PdfTranslatorControl : BaseControl
    {
        #region Properties
        /// <summary>
        /// Indicator for if the pdf preview should be shown
        /// </summary>
        private bool m_showPdf;

        /// <summary>
        /// The current pdf page number
        /// </summary>
        private int m_pageNum;

        /// <summary>
        /// The currently active layout mode
        /// </summary>
        private PdfLayoutMode m_currentLayout;

        /// <summary>
        /// The pdf data container
        /// </summary>
        private PdfPageContainer m_pdfContainer;

        /// <summary>
        /// The translation container
        /// </summary>
        private TranslatedPdfPageContainer m_translationContainer;

        /// <summary>
        /// Translation result
        /// </summary>
        public PdfPageContainer PdfDataContainer
        {
            get { return m_pdfContainer; }
            private set { SetProperty(GetPropertyName(), ref m_pdfContainer, value); }
        }

        /// <summary>
        /// Translation result
        /// </summary>
        public TranslatedPdfPageContainer TranslationContainer
        {
            get { return m_translationContainer; }
            private set { SetProperty(GetPropertyName(), ref m_translationContainer, value); }
        }

        /// <summary>
        /// The current page number
        /// </summary>
        public int PageNum
        {
            get { return m_pageNum; }
            set
            {
                if (SetProperty(GetPropertyName(), ref m_pageNum, value))
                {
                    SetSelectedPage();
                }
            }
        }

        /// <summary>
        /// Flag to indicate to the UI if the page number control
        /// should be shown.
        /// </summary>
        public bool ShowPageNumber
        {
            get
            {
                return IsLayout(PdfLayoutMode.PREVIEW) || IsLayout(PdfLayoutMode.TRANSLATE);
            }
        }

        public bool ShowPdfCommands
        {
            get
            {
                if (m_pdfViewer == null)
                    return false;

                return m_pdfViewer.IsDocumentLoaded && m_showPdf;
            }
        }

        public bool ShowPdf
        {
            get { return m_showPdf; }
            set 
            { 
                if(SetProperty(GetPropertyName(), ref m_showPdf, value))
                {
                    RaisePropertyChanged(nameof(ShowPdfCommands));
                }
            }
        }

        /// <summary>
        /// Active layout
        /// </summary>
        public PdfLayoutMode CurrentLayout
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
        #endregion

        public PdfTranslatorControl(ITranslatorBackend backend, INetworkManager networkManager, IServerManager serverManager, ILanguageManager languageManager, IErrorManager errorManager)
            : base(backend, networkManager, serverManager, languageManager, errorManager)
        {
            InitializeComponent();
            BuildCommands();
            ShowPdf = true;
            PdfDataContainer = new PdfPageContainer();
            PdfSharp.Fonts.GlobalFontSettings.UseWindowsFontsUnderWindows = true;
        }

        /// <summary>
        /// Builds the button command handling lookup
        /// </summary>
        protected override void BuildCommands()
        {
            // Pdf Commands
            m_commandLookup[PdfCommands.LoadCommand] = new TranslatorCommand(ProcessCommandLoadPdf);
            m_commandLookup[PdfCommands.ClearPdfCommand] = new TranslatorCommand(ProcessClearPdfCommand, CanProcessClearPdfCommand);
            m_commandLookup[PdfCommands.ZoomInCommand] = new TranslatorCommand(ProcessZoomInCommand, CanProcessZoomCommand);
            m_commandLookup[PdfCommands.ZoomOutCommand] = new TranslatorCommand(ProcessZoomOutCommand, CanProcessZoomCommand);
            //m_commandLookup[PdfCommands.ZoomResetCommand] = new TranslatorCommand(ProcessResetZoomCommand, CanProcessZoomCommand);

            // Translation Commands
            m_commandLookup[TranslationCommands.ClearCommand] = new TranslatorCommand(ProcessCommandClearTranslate, CanProcessClearTranslateCommand);
            m_commandLookup[TranslationCommands.TranslateCommand] = new TranslatorCommand(ProcessCommandTranslate, CanProcessCommandTranslate);
            m_commandLookup[TranslationCommands.ExportCommand] = new TranslatorCommand(ProcessCommandExport, CanProcessCommandExport);
        }

        #region Command Handling
        /// <summary>
        /// Indicates if can do the clear pdf command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        private bool CanProcessClearPdfCommand()
        {
            if (m_pdfViewer != null)
            {
                return m_pdfViewer.IsDocumentLoaded;
            }
            return false;
        }

        /// <summary>
        /// process the clear pdf command
        /// </summary>
        private void ProcessClearPdfCommand()
        {
            ClearLoadedPdf();
            SetCurrentLayout(PdfLayoutMode.NONE);
        }

        /// <summary>
        /// Indicates if can do a zoom command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        private bool CanProcessZoomCommand()
        {
            if (m_pdfViewer != null)
            {
                return m_pdfViewer.IsDocumentLoaded;
            }

            return false;
        }

        /// <summary>
        /// Process zoom in command
        /// </summary>
        private void ProcessZoomInCommand()
        {
            if (m_pdfViewer != null)
                m_pdfViewer.ZoomIn();
        }

        /// <summary>
        /// Process zoom out command
        /// </summary>
        private void ProcessZoomOutCommand()
        {
            if (m_pdfViewer != null)
                m_pdfViewer.ZoomOut();
        }

        /// <summary>
        /// process the load pdf command
        /// </summary>
        private void ProcessCommandLoadPdf()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".pdf";
            dlg.Filter = "PDF (*.pdf)|*.pdf";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                LoadPdfFile(dlg.FileName);
            }
        }

        /// <summary>
        /// Indicates if can do the translate command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanProcessCommandTranslate()
        {
            return m_translationContainer == null && 
                   m_pdfContainer != null && 
                   m_pdfContainer.IsPdfLoaded && 
                   m_serverManager.TranslatorStatus == ServerStatus.CONNECTED;
        }

        /// <summary>
        /// process the translate command
        /// </summary>
        async void ProcessCommandTranslate()
        {
            SetBusyIndicatorWithProgress("Translating Pages...", 1, m_pdfContainer.Pages.Count());

            bool overallSuccess = true;
            try
            {
                TranslationContainer = new TranslatedPdfPageContainer(m_languageManager.TargetLanguage.DisplayName, m_pdfContainer.Pages);
                await Task.Run(async () =>
                {
                    foreach (var translatedPage in m_translationContainer.Pages)
                    {
                        if (!overallSuccess) { break; }

                        // Check for user abort
                        if (BusyIndicator.UserAborted)
                        {
                            overallSuccess = false;
                            break;
                        }

                        bool pageSuccess = true;
                        // Collect all translation tasks for the current page
                        var translationTasks = new List<Task<Tuple<IStringPageContent, ITextTranslationResult>>>();
                        var validContentsInPage = translatedPage
                            .GetPageContentsAs<IStringPageContent>()
                            .Where(x => x.HasValidContent())
                            .ToList();

                        foreach (IStringPageContent stringContent in validContentsInPage)
                        {
                            // Start each translation without awaiting it immediately
                            translationTasks.Add(Task.Run(async () =>
                            {
                                ITextTranslationResult translationResult = await m_backend.TranslateTextAsync(TranslationUri, m_languageManager.SourceLanguage.TranslationCode, stringContent.Content, m_languageManager.TargetLanguage.TranslationCode, 1)
                                                                                          .ConfigureAwait(false); // Improves performance by avoiding context switching if not needed,
                                return Tuple.Create(stringContent, translationResult);
                            })); // Pass cancellation token to the task
                        }

                        // Wait for all translations on the page to complete
                        var results = await Task.WhenAll(translationTasks);

                        // Process the results
                        foreach (var result in results)
                        {
                            if (pageSuccess &= result.Item2.Success)
                            {
                                translatedPage.AddTranslatedResult(result.Item1, new LibreTranslateDataWrapper(result.Item2.Result));
                            }
                            else
                            {
                                m_errorManager.AddError("Failed Translation: " + result.Item2.GetErrorMessage());
                                break;
                            }
                        }

                        if (pageSuccess)
                        {
                            // Increment the current progress
                            Dispatcher.Invoke(() =>
                            {
                                BusyIndicator.CurrentProgress++;
                            });
                        }

                        // Check for user abort
                        if (BusyIndicator.UserAborted)
                        {
                            pageSuccess = false;
                            overallSuccess = false;
                            break;
                        }

                        overallSuccess &= pageSuccess;
                    }

                    if (overallSuccess)
                    {
                        if (m_translationContainer != null)
                        {
                            m_translationContainer.GoToPage(m_pageNum - 1);
                            SetCurrentLayout(PdfLayoutMode.TRANSLATE);
                        }
                    }
                    else
                    {
                        ClearTranslatedContainer();
                    }
                });
            }
            catch (Exception ex)
            {
                ClearTranslatedContainer();
                m_errorManager.AddError("Failed Translation: " + ex.Message);
            }
            finally
            {
                ClearBusyIndicatorWithProgress();
            }
        }


        /// <summary>
        /// Indicates if can do the clear translation command
        /// </summary>
        /// <returns>true if can process command; otherwise false</returns>
        bool CanProcessClearTranslateCommand()
        {
            return m_translationContainer != null && IsLayout(PdfLayoutMode.TRANSLATE);
        }

        /// <summary>
        /// process the clear translation command
        /// </summary>
        void ProcessCommandClearTranslate()
        {
            ShowPdf = true;
            ClearTranslatedContainer();
            SetCurrentLayout(PdfLayoutMode.PREVIEW);
        }

        /// <summary>
        /// Indicates if can do the export command
        /// </summary>
        /// <returns></returns>
        private bool CanProcessCommandExport()
        {
            return m_translationContainer != null;
        }

        /// <summary>
        /// Exports the translatted text
        /// </summary>
        private async void ProcessCommandExport()
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = "Pdf Files (*.pdf)|*.pdf|All files (*.*)|*.*",
                FileName = string.Format("{0}_{1}_{2}_{3}", Path.GetFileNameWithoutExtension(m_pdfContainer.PdfPath), "TranslatedTo", m_translationContainer.Language, DateTime.Now.ToString("yyyyMMdd"))
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    SetBusyIndicatorWithProgress("Exporting Pages...", 1, m_translationContainer.Pages.Count());

                    await Task.Run(() =>
                    {
                        string filePath = dlg.FileName;
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }

                        foreach (int pageIndex in m_translationContainer.ExportPdf(filePath))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                BusyIndicator.CurrentProgress++;
                            });

                            if (BusyIndicator.UserAborted)
                            {
                                break;
                            }
                        }
                    });

                }
                catch (Exception ex)
                {
                    m_errorManager.AddError("Failure to export pdf document: " + ex.Message);
                }
                finally
                {
                    ClearBusyIndicatorWithProgress();
                }
            }
        }
        #endregion

        /// <summary>
        /// Sets the current layout
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="savePrevious"></param>
        void SetCurrentLayout(PdfLayoutMode layout)
        {
            CurrentLayout = layout;
        }

        /// <summary>
        /// Indicates if the layout in a current mode
        /// </summary>
        /// <param name="layout"></param>
        /// <returns></returns>
        bool IsLayout(PdfLayoutMode layout)
        {
            return m_currentLayout == layout;
        }

        /// <summary>
        /// Handles the current layout changed
        /// </summary>
        void HandleCurrentLayoutChanged()
        {
            switch (m_currentLayout)
            {
                case PdfLayoutMode.NONE:
                    ClearTranslatedContainer();
                    m_pdfContainer.Clear();
                    break;
            }

            RaisePropertyChanged(nameof(ShowPdfCommands));
            RaisePropertyChanged(nameof(ShowPageNumber));
        }

        /// <summary>
        /// Loads the pdf file
        /// </summary>
        /// <param name="filePath"></param>
        private async void LoadPdfFile(string filePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    ShowPdf = true;
                    IsBusy = true;
                    ClearLoadedPdf();
                    SetCurrentLayout(PdfLayoutMode.NONE);

                    if (IsValidPDF(filePath))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            m_pdfViewer.OpenPdf(filePath);
                            pageNumControl.Maximum = m_pdfViewer.PageCount;
                            PageNum = 1;
                        });
                        m_pdfContainer.LoadPdf(filePath);
                        SetCurrentLayout(PdfLayoutMode.PREVIEW);
                    }
                }
                catch (Exception ex)
                {
                    ClearLoadedPdf();
                    m_errorManager.AddError(string.Format("Failed to load PDF {0} due to {1}: ", filePath, ex.Message));
                }
                finally
                {
                    IsBusy = false;
                }
            });
        }

        private void ClearLoadedPdf()
        {
            Dispatcher.Invoke(() =>
            {
                ClearTranslatedContainer();
                m_pdfContainer.Clear();
                m_pdfViewer.UnLoad();
            });
        }

        private void ClearTranslatedContainer()
        {
            if (TranslationContainer != null)
            {
                TranslationContainer.Dispose();
                TranslationContainer = null;
            }
        }

        /// <summary>
        /// Go to the selected page
        /// </summary>
        private void SetSelectedPage()
        {
            int pageNum = m_pageNum - 1; // 1 indexed
            m_pdfViewer.GotoPage(pageNum);
            if (m_translationContainer != null)
            {
                m_translationContainer.GoToPage(pageNum);
            }
        }

        /// <summary>
        /// Validates a pdf file path is a pdf file that exists
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool IsValidPDF(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) &&
                    File.Exists(filePath) &&
                    string.Equals(Path.GetExtension(filePath), ".pdf", StringComparison.OrdinalIgnoreCase);
        }

        #region BusyIndicator Progress
        void SetBusyIndicatorWithProgress(string command, double currentValue, double maxValue)
        {
            DispatcherBeginInvoke(() =>
            {
                this.BusyIndicator.BusyText = command;
                this.BusyIndicator.ShowAbortButton = true;
                this.BusyIndicator.IsMarquee = false;
                this.BusyIndicator.CurrentProgress = currentValue;
                this.BusyIndicator.MaxValue = maxValue;
                this.IsBusy = true;
            });
        }

        void ClearBusyIndicatorWithProgress()
        {
            DispatcherBeginInvoke(() =>
            {
                this.BusyIndicator.BusyText = null;
                this.BusyIndicator.ShowAbortButton = false;
                this.BusyIndicator.IsMarquee = true;
                this.BusyIndicator.CurrentProgress = 1;
                this.BusyIndicator.MaxValue = 1;
                this.IsBusy = false;
            });
        }
        #endregion

        #region PDF Viewer Events
        private void pdfViewer_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (m_pdfViewer != null)
            {
                int currentPage = m_pdfViewer.PageNo + 1;
                if (!int.Equals(m_pageNum, currentPage))
                    PageNum = currentPage;
            }
        }

        private void pdfViewer_PreviewDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0];
                    if (IsValidPDF(filePath))
                    {
                        LoadPdfFile(filePath);
                    }
                }
            }
        }

        private void pdfViewer_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0];
                    if (IsValidPDF(filePath))
                    {
                        e.Effects = DragDropEffects.Copy; // Allow the drop
                        e.Handled = true; // Mark the event as handled
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None; // Disallow the drop
                        e.Handled = true;
                    }
                }
                else
                {
                    e.Effects = DragDropEffects.None; // Disallow the drop
                    e.Handled = true;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None; // Disallow the drop
                e.Handled = true;
            }
        }
        #endregion

        #region IModule Implementation
        public override void OnStart()
        {
            base.OnStart();
        }

        public override void OnStop()
        {
            base.OnStop();
            try
            {
                m_pdfViewer.UnLoad();
                m_pdfViewer.Dispose();
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}
