using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Ombro.ViewModels
{
    public class AboutViewModel : PropertyChangedBase
    {
        private ObservableCollection<AboutItem> _abouts = new ObservableCollection<AboutItem>();
        private readonly INavigationService navigationService;

        public AboutViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;

            _abouts.Add(
                new AboutItem
                {
                    Title = "Publisher",
                    Body = GetManifestInfo("Publisher")
                });
            _abouts.Add(
                new AboutItem
                {
                    Title = "Author",
                    Body = GetManifestInfo("Author")
                });
            _abouts.Add(
                new AboutItem
                {
                    Title = "Version",
                    Body = GetManifestInfo("Version"),
                });
            _abouts.Add(
                new AboutItem
                {
                    Title = "Privacy Policy",
                    Body = "We respect your privacy.  We don't collect information about you or your device, we don't track you or your app usage, we don't share your data.  We only connect to the USGS servers to download weather station data."
                });
        }

        private static string GetManifestInfo(string part)
        {
            return (from manifest in System.Xml.Linq.XElement.Load("WMAppManifest.xml").Descendants("App") select manifest).SingleOrDefault().Attribute(part).Value.ToString();
        }

        public ObservableCollection<AboutItem> AboutItems
        {
            get { return _abouts; }
            set
            {
                _abouts = value;
                NotifyOfPropertyChange(() => AboutItems);
            }
        }

        public void CreditsAction()
        {
            navigationService.UriFor<CreditsViewModel>().Navigate();
        }
    }

    public class AboutItem
    {
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
