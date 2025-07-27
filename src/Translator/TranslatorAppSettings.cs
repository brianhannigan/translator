using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Translator
{
    /// <summary>
    /// Displayed when help is prompted
    /// </summary>
    public class HelpAttribute : Attribute
    {
        public string CMD { get; protected set; }
        public string Message { get; protected set; }
        public HelpAttribute(string cmd, string message)
        {
            CMD = cmd;
            Message = message;
        }
    }

    public class TranslatorAppSettings
    {
        public const string HelpCmd = "-help";
        public const string HelpHCmd = "-h";
        public const string HelpQuestionCmd = "?";

        public const string LanguagesCmd  = "-languages";

        [Help(LanguagesCmd, "The path to the languages config file.")]
        public string LanguagesPath { get; set; }

        public TranslatorAppSettings()
        {
        }

        public void Displayhelp()
        {
            StringBuilder cmdDisplay = new StringBuilder();
            cmdDisplay.AppendLine("Bradley Emu Command Line Help:\n");

            var helpProperties = this.GetType().GetProperties();
            foreach (var property in helpProperties)
            {
                var helpAttribute = property.GetCustomAttribute<HelpAttribute>();
                if (helpAttribute != null)
                {
                    cmdDisplay.AppendLine(
                        String.Format("{0} : {1}\nCurrent Value: {2}\n",
                        helpAttribute.CMD,
                        helpAttribute.Message,
                        property.GetValue(this)));
                }
            }
            MessageBox.Show(cmdDisplay.ToString(), "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
