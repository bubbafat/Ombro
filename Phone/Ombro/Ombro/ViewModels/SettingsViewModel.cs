using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ombro.ViewModels
{
    public class SettingsViewModel : PropertyChangedBase
    {
        private ObservableCollection<DistanceDisplay> _distance = new ObservableCollection<DistanceDisplay>();
        private DistanceDisplay _selectedDistance;

        private ObservableCollection<DaysDisplay> _days = new ObservableCollection<DaysDisplay>();
        private DaysDisplay _selectedDay;


        public SettingsViewModel()
        {
            _distance.Add(new DistanceDisplay(1));
            _distance.Add(new DistanceDisplay(5));
            _distance.Add(new DistanceDisplay(10));
            _distance.Add(new DistanceDisplay(25));
            _distance.Add(new DistanceDisplay(50));
            _distance.Add(new DistanceDisplay(100));

            var distanceSetting =  AppSettings.GetValue<double>(OmbroSettings.SearchDistanceMiles, 50);

            SelectedDistance =
                _distance.Where(d => d.DistanceMiles == distanceSetting).FirstOrDefault();

            if(SelectedDistance == null)
            {
                SelectedDistance = _distance.Where(d => d.DistanceMiles == 25).First();
            }

            _days.Add(new DaysDisplay(1));
            _days.Add(new DaysDisplay(2));
            _days.Add(new DaysDisplay(3));
            _days.Add(new DaysDisplay(4));
            _days.Add(new DaysDisplay(5));
            _days.Add(new DaysDisplay(6));
            _days.Add(new DaysDisplay(7));

            var daySetting = AppSettings.GetValue<int>(OmbroSettings.DaysOfRainToShow, 3);

            SelectedDay =
                _days.Where(d => d.Days == daySetting).FirstOrDefault();

            if (SelectedDay == null)
            {
                SelectedDay = _days.Where(d => d.Days == 3).First();
            }

        }

        public ObservableCollection<DaysDisplay> Days
        {
            get { return _days; }
            set
            {
                _days = value;
                NotifyOfPropertyChange(() => Days);
            }
        }

        public DaysDisplay SelectedDay
        {
            get { return _selectedDay; }
            set
            {
                _selectedDay = value;

                if (_selectedDay != null)
                {
                    AppSettings.AddOrUpdate(OmbroSettings.DaysOfRainToShow, _selectedDay.Days);
                }

                NotifyOfPropertyChange(() => SelectedDay);
            }
        }


        public ObservableCollection<DistanceDisplay> Distances
        {
            get { return _distance; }
            set
            {
                _distance = value;
                NotifyOfPropertyChange(() => Distances);
            }
        }

        public DistanceDisplay SelectedDistance
        {
            get { return _selectedDistance; }
            set
            {
                _selectedDistance = value;

                if (_selectedDistance != null)
                {
                    AppSettings.AddOrUpdate(OmbroSettings.SearchDistanceMiles, _selectedDistance.DistanceMiles);
                }

                NotifyOfPropertyChange(() => SelectedDistance);
            }
        }
    }
}
