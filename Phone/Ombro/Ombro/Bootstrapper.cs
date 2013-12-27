using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Ombro.ViewModels;
using Caliburn.Micro.BindableAppBar;
using System.Windows.Controls;

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

            AddCustomConventions();
        }

        static void AddCustomConventions()
        {
            // App Bar Conventions
            ConventionManager.AddElementConvention<BindableAppBarButton>(
                Control.IsEnabledProperty, "DataContext", "Click");
            ConventionManager.AddElementConvention<BindableAppBarMenuItem>(
                Control.IsEnabledProperty, "DataContext", "Click");
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
