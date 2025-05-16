using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System.Windows.Markup;
using System.Globalization;
using System.Windows.Threading;

namespace PassagePlanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // This will make date format etc. follow Regional Settings in Windows.
            //FrameworkElement.LanguageProperty.OverrideMetadata(
            //    typeof(FrameworkElement),
            //    new FrameworkPropertyMetadata(
            //        XmlLanguage.GetLanguage(
            //        CultureInfo.CurrentCulture.IetfLanguageTag)));
            base.OnStartup(e);
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            ErrorHandler.Show(e.Exception);

            // Prevent default unhandled exception processing
            e.Handled = true;
        }

    }
}
