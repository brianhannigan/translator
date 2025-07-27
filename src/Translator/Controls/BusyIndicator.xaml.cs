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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VTEControls;
using VTEControls.WPF;

namespace Translator.Controls
{
    /// <summary>
    /// Interaction logic for BusyIndicator.xaml
    /// </summary>
    public partial class BusyIndicator : UserControlExtension
    {
        #region Dependency Properties
        private const string IsBusyTag = "IsBusy";
        private const string BusyTextTag = "BusyText";
        private const string CurrentProgressTag = "CurrentProgress";
        private const string MaxValueTag = "MaxValue";
        private const string IsMarqueeTag = "IsMarquee";
        private const string ShowAbortButtonTag = "ShowAbortButton";

        public static readonly DependencyProperty
            BusyTextProperty = DependencyProperty.Register(
                BusyTextTag, typeof(string), typeof(BusyIndicator), new PropertyMetadata("Please Wait...", OnPropertyChanged));

        public static readonly DependencyProperty
            IsBusyProperty = DependencyProperty.Register(
                IsBusyTag, typeof(bool), typeof(BusyIndicator), new PropertyMetadata(false, OnPropertyChanged));

        public static readonly DependencyProperty
            ShowAbortButtonProperty = DependencyProperty.Register(
                ShowAbortButtonTag, typeof(bool), typeof(BusyIndicator), new PropertyMetadata(false, OnPropertyChanged));

        public static readonly DependencyProperty
            CurrentProgressProperty = DependencyProperty.Register(
                CurrentProgressTag, typeof(double), typeof(BusyIndicator), new PropertyMetadata(0.0, OnPropertyChanged));

        public static readonly DependencyProperty
            MaxValueProperty = DependencyProperty.Register(
                MaxValueTag, typeof(double), typeof(BusyIndicator), new PropertyMetadata(100.0, OnPropertyChanged));

        public static readonly DependencyProperty
            IsMarqueeProperty = DependencyProperty.Register(
                IsMarqueeTag, typeof(bool), typeof(BusyIndicator), new PropertyMetadata(true, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            BusyIndicator dpadControl = sender as BusyIndicator;
            if (dpadControl != null)
            {
                dpadControl.HandlePropertyChanged(e.Property.Name);
            }
        }
        #endregion

        #region Properties
        private bool m_userAborted = false;

        public string BusyText
        {
            get { return (string)GetValue(BusyTextProperty); }
            set { SetValue(BusyTextProperty, value); }
        }

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public bool IsMarquee
        {
            get { return (bool)GetValue(IsMarqueeProperty); }
            set { SetValue(IsMarqueeProperty, value); }
        }

        public bool ShowAbortButton
        {
            get { return (bool)GetValue(ShowAbortButtonProperty); }
            set { SetValue(ShowAbortButtonProperty, value); }
        }

        public double CurrentProgress
        {
            get { return (double)GetValue(CurrentProgressProperty); }
            set { SetValue(CurrentProgressProperty, value); }
        }

        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public bool UserAborted
        {
            get { return m_userAborted; }
        }
        #endregion

        #region Commands
        private ICommand m_abortCommand;

        public ICommand AbortCommand
        {
            get
            {
                return m_abortCommand ??
                    (m_abortCommand = new CommandHandler(() =>
                {
                    m_userAborted = true;
                },
                () =>
                {
                    return !m_userAborted && IsBusy;
                }));
            }
        }

        #endregion

        public BusyIndicator()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public void HandlePropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case IsBusyTag:
                    m_userAborted = false;
                    break;
                case CurrentProgressTag:
                    if (CurrentProgress > MaxValue)
                        CurrentProgress = MaxValue;
                    break;
            }
        }
    }
}
