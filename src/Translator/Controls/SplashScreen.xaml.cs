using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using VTEControls.WPF;

namespace Translator
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : WindowExtension
    {
        /// <summary>
        /// The current status
        /// </summary>
        private string m_status;

        /// <summary>
        /// The current status
        /// </summary>
        public string Status
        {
            get { return m_status; }
            private set { SetProperty(GetPropertyName(), ref m_status, value); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SplashScreen()
        {
            InitializeComponent();
            StatusBar.IsIndeterminate = true;
        }

        /// <summary>
        /// Sets the status text of the splash screen
        /// </summary>
        /// <param name="status"></param>
        public void SetStatus(string status)
        {
            Status = status;
        }

        /// <summary>
        /// Override mouse move to allow moving window
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Lets the splash screen know the main window has been shown and it can be closed.
        /// </summary>
        public void Cleanup()
        {
            this.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                new Action(() =>
                {
                    StatusBar.IsIndeterminate = false;
                    this.Close();
                }));
        }
    }
}
