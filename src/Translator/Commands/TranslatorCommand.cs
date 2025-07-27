using System;

namespace Translator.Commands
{
    public class TranslatorCommand
    {
        private Action m_action;
        private Func<bool> m_canExecute;

        public TranslatorCommand(Action action)
            : this(action, null)
        {
        }

        public TranslatorCommand(Action action, Func<bool> canExecute)
        {
            m_action = action;
            m_canExecute = canExecute;
        }

        public void Execute()
        {
            m_action?.Invoke();
        }

        public bool CanExecute()
        {
            return m_canExecute?.Invoke() ?? true;
        }
    }
}
