using System;
using System.Windows.Input;
using Translator.Commands;
using Translator.Interfaces;
using Translator.Wrappers;
using TranslatorBackend.Interfaces;
using VTEControls;

namespace Translator.Controls
{
    /// <summary>
    /// Interaction logic for TextTranslatorControl.xaml
    /// </summary>
    public partial class TextTranslatorControl : BaseControl
    {
        private string m_textToTranslate;
        private LibreTranslateDataWrapper m_translationResult;

        public string TextToTranslate
        {
            get { return m_textToTranslate; }
            set { SetProperty(GetPropertyName(), ref m_textToTranslate, value); }
        }

        public LibreTranslateDataWrapper TranslationResult
        {
            get { return m_translationResult; }
            private set { SetProperty(GetPropertyName(), ref m_translationResult, value); }
        }

        public TextTranslatorControl(ITranslatorBackend backend, INetworkManager networkManager, IServerManager serverManager, ILanguageManager languageManager, IErrorManager errorManager)
            : base(backend, networkManager, serverManager, languageManager, errorManager)
        {
            InitializeComponent();
        }

        protected override void BuildCommands()
        {
            m_commandLookup[TranslationCommands.TranslateCommand] = new TranslatorCommand(ProcessCommandTranslate, CanProcessTranslateCommand);
            m_commandLookup[TranslationCommands.ClearCommand] = new TranslatorCommand(ProcessCommandClearTranslate, CanProcessCommandClearTranslate);
            m_commandLookup[TranslationCommands.NextCommand] = new TranslatorCommand(ProcessNextCommand, CanProcessGoNextCommand);
            m_commandLookup[TranslationCommands.PreviousCommand] = new TranslatorCommand(ProcessPreviousCommand, CanProcessPreviousCommand);
        }

        bool CanProcessTranslateCommand()
        {
            return m_backend != null && 
                   !string.IsNullOrWhiteSpace(m_textToTranslate) && 
                   m_serverManager.TranslatorStatus == ServerStatus.CONNECTED;
        }

        async void ProcessCommandTranslate()
        {
            IsBusy = true;
            TranslationResult = null;
            try
            {
                ITextTranslationResult translationResult = await m_backend.TranslateTextAsync(TranslationUri, m_languageManager.SourceLanguage.TranslationCode, m_textToTranslate, m_languageManager.TargetLanguage.TranslationCode, 3);
                if (translationResult.Success)
                    TranslationResult = new LibreTranslateDataWrapper(translationResult.Result);
                else
                     m_errorManager.AddError(translationResult.GetErrorMessage());
            }
            catch
            {
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanProcessCommandClearTranslate()
        {
            return m_translationResult != null;
        }

        void ProcessCommandClearTranslate()
        {
            TranslationResult = null;
        }

        bool CanProcessGoNextCommand()
        {
            return m_translationResult != null && m_translationResult.CanGoNext();
        }

        void ProcessNextCommand()
        {
            if (m_translationResult == null) { return; }
            m_translationResult.GoNext();
        }

        bool CanProcessPreviousCommand()
        {
            return m_translationResult != null && m_translationResult.CanGoBack();
        }

        void ProcessPreviousCommand()
        {
            if (m_translationResult == null) { return; }
            m_translationResult.GoBack();
        }
    }
}
