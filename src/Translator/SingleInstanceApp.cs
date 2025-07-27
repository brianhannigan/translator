using System;
using System.Reflection;
using System.Threading;

namespace Translator
{
    internal class SingleInstanceApp : IDisposable
    {
        /// <summary>
        /// A value indicating whether this instance is disposed.
        /// </summary>
        private bool m_disposed = false;

        /// <summary>
        /// The mutex that enforces the single program instance.
        /// </summary>
        private Mutex m_mutex = null;

        /// <summary>
        /// The new instance created flag.
        /// </summary>
        private bool m_newInstanceCreated = false;

        /// <summary>
        /// The single instance flag.
        /// </summary>
        public bool IsOnlyInstance
        {
            get { return m_newInstanceCreated; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleProgramInstance"/> class.
        /// </summary>
        public SingleInstanceApp(string identifier)
        {
            m_mutex = new Mutex(true, Assembly.GetExecutingAssembly().GetName().Name + identifier, out m_newInstanceCreated);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SingleProgramInstance"/> class.
        /// </summary>
        ~SingleInstanceApp()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!m_disposed && disposing)
            {
                // Cleanup managed resources
                if (m_newInstanceCreated)
                {
                    m_mutex.ReleaseMutex();
                    m_mutex.Close();
                    m_newInstanceCreated = false;
                }
            }
            m_disposed = true;
        }
    }
}
