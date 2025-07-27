using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Translator.Extensions;
using Translator.Interfaces;
using UglyToad.PdfPig.Content;
using VTEControls;

namespace Translator.PDF
{
    public class TranslatedPdfPageImage : PropertyHandler
    {
        #region Properties
        /// <summary>
        /// A thumbnail image
        /// </summary>
        private readonly Image m_image;

        /// <summary>
        /// A value indicating whether the attachment is active
        /// </summary>
        private bool m_isActive;

        /// <summary>
        /// The window for display purposes
        /// </summary>
        private Window _window;

        /// <summary>
        /// The index of the image
        /// </summary>
        private int m_index;

        /// <summary>
        /// A value indicating whether this instance is disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Gets the image thumbnail
        /// </summary>
        public Image Thumbnail
        {
            get { return this.m_image; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the attachment is active
        /// </summary>
        /// <value>
        /// a value indicating if the attachment is active
        /// </value>
        public bool IsActive
        {
            get { return m_isActive; }
            protected set { SetProperty(GetPropertyName(), ref this.m_isActive, value); }
        }

        /// <summary>
        /// Gets or sets the index of the image
        /// </summary>
        public int Index
        {
            get { return m_index; }
            private set { SetProperty(GetPropertyName(), ref m_index, value); }
        }
        #endregion

        #region Commands
        private ICommand _openCommand;
        private ICommand _closeCommand;

        public ICommand OpenCommand
        {
            get
            {
                return this._openCommand
                   ?? (this._openCommand = new CommandHandler(
                       () =>
                       {
                           if (this._window != null)
                           {
                               CloseWindow();
                           }
                           else
                           {
                               OpenWindow();
                           }
                       }));
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return this._closeCommand
                   ?? (this._closeCommand = new CommandHandler(
                       () =>
                       {
                           CloseWindow();
                       }));
            }
        }
        #endregion

        public TranslatedPdfPageImage(IImagePageContent content)
        {
            if(content != null)
            {
                try
                {
                    m_image = new Image { Source = content.ImageBytes.ToBitmapImage(), Stretch = Stretch.Uniform };
                }
                catch
                {

                }
                finally
                {
                    Index = content.Index;
                }
            }
        }

        private void OpenWindow()
        {
            this._window = new Window();
            this._window.Title = $"Image #{Index}";
            this._window.Closed += this.Window_Closed;
            this._window.Width = 400;
            this._window.Height = 400;
            this._window.Background = Brushes.DimGray;
            this._window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this._window.Content = new Image { Source = this.Thumbnail.Source, Stretch = Stretch.Uniform };

            this._window.Show();
            this.IsActive = true;
            this._window.Activate();
        }

        private void CloseWindow()
        {
            this.IsActive = false;
            if (this._window != null)
            {
                this._window.Closed -= this.Window_Closed;
                this._window?.Close();
                this._window = null;
            }
        }

        /// <summary>
        /// Raised when window is closed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void Window_Closed(object sender, EventArgs e)
        {
            CloseWindow();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed == false)
            {
                if (disposing)
                {
                    this.IsActive = false;
                    CloseWindow();
                }

                // Cleanup unmanaged resources
            }

            this._disposed = true;
        }
    }
}
