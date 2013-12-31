using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombro.ViewModels
{
    public class CreditsViewModel : PropertyChangedBase
    {
        private ObservableCollection<CreditItem> _credits = new ObservableCollection<CreditItem>();

        public CreditsViewModel()
        {
            _credits.Add(
                new CreditItem
                {
                    Title="Buy me a soda image",
                    Body = "User vjeran2001  on stock.xchng",
                    Uri = "http://www.sxc.hu/photo/1387826",
                });

            _credits.Add(
               new CreditItem
               {
                   Title = "Buy me a beer image",
                   Body = "User engindeniz on stock.xchng",
                   Uri = "http://www.sxc.hu/photo/1209277",
               });

            _credits.Add(
               new CreditItem
               {
                   Title = "Buy me a good beer image",
                   Body = "User mzacha on stock.xchng",
                   Uri = "http://www.sxc.hu/photo/795106",
               });
        }

        public ObservableCollection<CreditItem> CreditItems
        {
            get { return _credits; }
            set
            {
                _credits = value;
                NotifyOfPropertyChange(() => CreditItems);
            }
        }
    }

    public class CreditItem
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Uri { get; set; }
    }
}
