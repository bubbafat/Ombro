using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Ombro.ViewModels;
using Caliburn.Micro.BindableAppBar;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace Ombro
{
    public class Bootstrapper : PhoneBootstrapper
    {
        PhoneContainer container;

        protected override void Configure()
        {
            container = new PhoneContainer();
            container.RegisterPhoneServices(RootFrame);
            container.PerRequest<MainPageViewModel>();
            container.PerRequest<SelectStationViewModel>();

            container.PerRequest<AboutViewModel>();
//            var settings = container.RegisterSettingsService();

            //settings.RegisterCommand<AboutViewModel>("About");

#if DEBUG
            LogManager.GetLog = type => new DebugLogger(type);
#else
            if (System.Diagnostics.Debugger.IsAttached)
            {
                LogManager.GetLog = type => new DebugLogger(type);
            }
#endif

            AddCustomConventions();
        }

        static void AddCustomConventions()
        {
            // App Bar Conventions
            ConventionManager.AddElementConvention<BindableAppBarButton>(
                Control.IsEnabledProperty, "DataContext", "Click");
            ConventionManager.AddElementConvention<BindableAppBarMenuItem>(
                Control.IsEnabledProperty, "DataContext", "Click");

            ConventionManager.AddElementConvention<ListPicker>(ListPicker.ItemsSourceProperty, "SelectedItem", "SelectionChanged")
                .ApplyBinding = (viewModelType, path, property, element, convention) =>
                {
                    if (ConventionManager.GetElementConvention(typeof(ItemsControl)).ApplyBinding(viewModelType, path, property, element, convention))
                    {
                        ConventionManager.ConfigureSelectedItem(element, ListPicker.SelectedItemProperty, viewModelType, path);
                        return true;
                    }
                    return false;
                };
        }

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }
        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }
    }
}
