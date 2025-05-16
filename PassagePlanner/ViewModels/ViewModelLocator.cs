/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:PassagePlanner"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace PassagePlanner
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //if (ViewModelBase.IsInDesignModeStatic)
            //{
            //    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            //}
            //else
            //{
            //    SimpleIoc.Default.Register<IDataService, DataService>();
            //}

            //SimpleIoc.Default.Register<IRouteService, RouteService>();

            SimpleIoc.Default.Register<AppSettingsViewModel>();
            SimpleIoc.Default.Register<RouteViewModel>();
            SimpleIoc.Default.Register<VezzelViewModel>();
            SimpleIoc.Default.Register<SquatViewModel>();
        }

        /// <summary>
        /// Gets the RouteViewModel property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public RouteViewModel RouteVM
        {
            get
            {
                return ServiceLocator.Current .GetInstance<RouteViewModel>();
            }
        }

        /// <summary>
        /// Gets the VesselViewModel property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public VezzelViewModel VesselVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<VezzelViewModel>();
            }
        }

        /// <summary>
        /// Gets the SquatViewModel property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public SquatViewModel SquatVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SquatViewModel>();
            }
        }

        /// <summary>
        /// Gets the AppSettingsViewModel property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public AppSettingsViewModel AppSettingsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AppSettingsViewModel>();
            }
        }

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            SimpleIoc.Default.Unregister<AppSettingsViewModel>();
            SimpleIoc.Default.Unregister<RouteViewModel>();
            SimpleIoc.Default.Unregister<VezzelViewModel>();
            SimpleIoc.Default.Unregister<SquatViewModel>();
        }
    }
}