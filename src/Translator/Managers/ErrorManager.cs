using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Translator.Interfaces;
using VTEControls;

namespace Translator.Managers
{
    public class ErrorManager : PropertyHandler, IErrorManager
    {
        #region Commands
        private ICommand m_clearCommand;

        public ICommand ClearCommand
        {
            get
            {
                return m_clearCommand ??
                  (m_clearCommand = new CommandHandler(
                      () =>
                      {
                          Errors.Clear();
                          RaiseErrorsChanged();
                      }));
            }
        }
        #endregion

        private ObservableCollection<string> m_errors = new ObservableCollection<string>();

        public event EventHandler<string> OnErrorReceived;
        public event EventHandler OnErrorsCleared;

        public int ErrorCount 
        { 
            get { return m_errors.Count; } 
        }

        public ObservableCollection<string> Errors
        {
            get { return m_errors; }
            private set { SetProperty(GetPropertyName(), ref m_errors, value); }
        }

        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                Errors.Add(error);
                OnErrorReceived?.Invoke(this, error);
            }
        }

        void RaiseErrorsChanged()
        {
            OnErrorsCleared?.Invoke(this, EventArgs.Empty);
        }
    }
}
