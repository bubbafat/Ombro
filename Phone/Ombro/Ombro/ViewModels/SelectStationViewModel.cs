using Caliburn.Micro;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ombro.ViewModels
{
    public class SelectStationViewModel : Screen, IDisposable
    {
        private GeoCoordinateWatcher _loc;
        private ObservableCollection<WeatherStation> _stations = new ObservableCollection<WeatherStation>();
        private WeatherStation _selectedStation;

        private readonly object _listLock = new object();
        private readonly INavigationService _navigationService;

        public SelectStationViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            CanFind = true;
        }

        public ObservableCollection<WeatherStation> Stations
        {
            get { return _stations; }
            set
            {
                _stations = value;
                NotifyOfPropertyChange(() => Stations);
            }
        }

        public WeatherStation SelectedStation
        {
            get { return _selectedStation; }
            set
            {
                _selectedStation = value;
                NotifyOfPropertyChange(() => SelectedStation);
                CanSelect = value != null;
            }
        }

        private bool _canSelect;
        public bool CanSelect
        {
            get { return _canSelect; }
            set 
            {
                _canSelect = value;
                NotifyOfPropertyChange(() => CanSelect);
            }
        }


        private bool _canFind;
        public bool CanFind
        {
            get { return _canFind; }
            set 
            {
                _canFind = value;
                NotifyOfPropertyChange(() => CanFind);
            }
        }

        protected override void OnActivate()
        {
            LoadStationsFromCache();
        }

        protected override void OnDeactivate(bool close)
        {
            if(close)
            {
                using (_loc) { }
                _loc = null;
            }

            base.OnDeactivate(close);
        }

        public void LoadStationsFromCache()
        {
            CanSelect = false;

            var loadTask = WeatherStationSettings.Load();
            loadTask.ContinueWith(s =>
            {
                WeatherStationSettings settings = s.Result;

                if (settings.NearbyStations != null)
                {
                    Execute.OnUIThread(() =>
                    {
                        lock (_listLock)
                        {
                            _stations.Clear();
                            foreach (var station in settings.NearbyStations)
                            {
                                this._stations.Add(station);
                            }
                        }
                        CanSelect = false;
                    });
                }
            },
            TaskContinuationOptions.OnlyOnRanToCompletion);
        }
        public void Dispose()
        {
            using (_loc) { }
            _loc = null;
            GC.SuppressFinalize(this);
        }

        public void FindNearMeAction()
        {
            CanFind = false;
            CanSelect = false;

            _stations.Clear();
            SystemTray.ProgressIndicator.IsVisible = true;

            if(_loc == null)
            {
                _loc = new GeoCoordinateWatcher();
                _loc.MovementThreshold = 1000;
                _loc.StatusChanged += _loc_StatusChanged;
                _loc.PositionChanged += _loc_PositionChanged;
            }

            _loc.Start();
        }

        private void DoneLoading(IEnumerable<WeatherStation> stationList = null)
        {
            Execute.OnUIThread(() =>
                {
                    if (stationList != null)
                    {
                        this._stations.Clear();
                        foreach(WeatherStation s in stationList)
                        {
                            this._stations.Add(s);
                        }
                    }

                    _loc.Stop();
                    SystemTray.ProgressIndicator.IsVisible = false;
                    CanFind = true;
                    CanSelect = false;
                });
        }

        void _loc_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var getListTask = new Task<Ombro.WeatherStationRepository.RequestState>(() => WeatherStationRepository.GetStationsNear(e.Position, 50));

            getListTask.ContinueWith(request =>
                    {
                        if (request.IsFaulted)
                        {
                            request.Exception.Handle(ex =>
                                {
                                    ShowError(ex);
                                    DoneLoading(null);
                                    return true;
                                });
                        }
                        else
                        {
                            using (request.Result)
                            {
                                if (request.Result.Error != null)
                                {
                                    ShowError(request.Result.Error);
                                    DoneLoading(null);
                                }
                                else
                                {
                                    List<WeatherStation> newList = new List<WeatherStation>();
                                    foreach (var s in request.Result.Stations)
                                    {
                                        s.Distance = (long)e.Position.Location.GetDistanceTo(s.Location);
                                        newList.Add(s);
                                    }
                                    DoneLoading(newList.OrderBy(s => s.Distance));
                                }
                            }
                        }
                    });

            getListTask.Start();
        }

        private void ShowError(Exception ex)
        {
            Execute.OnUIThread(() =>
                {
                    MessageBox.Show(string.Format("Unable to load stations.\n\nNetwork data or wifi is required to load data.\n\nError message: {0}", ex.Message), "Error", MessageBoxButton.OK);
                });
        }

        void _loc_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch(e.Status)
            {
                case GeoPositionStatus.Disabled:
                    if(_loc.Permission == GeoPositionPermission.Denied)
                    {
                        MessageBox.Show("Location services are disabled for this application.");
                    }
                    else
                    {
                        MessageBox.Show("Location services are not functioning.");
                    }
                    DoneLoading();
                    break;
                case GeoPositionStatus.Initializing:
                    break;
                case GeoPositionStatus.NoData:
                    MessageBox.Show("Location data is not available");
                    DoneLoading();
                    break;
                case GeoPositionStatus.Ready:
                    break;
                default:
                    DoneLoading();
                    break;
            }
        }

        public void SelectAction()
        {
            lock (_listLock)
            {
                if (SelectedStation is WeatherStation)
                {
                    WeatherStationSettings settings = new WeatherStationSettings(
                        SelectedStation, _stations);
                    
                    var saveTask = settings.Save();
                    saveTask.ContinueWith(a =>
                        {
                            Execute.OnUIThread(() =>
                                {
                                    if (_navigationService.CanGoBack)
                                    {
                                        _navigationService.GoBack();
                                    }
                                    else
                                    {
                                        _navigationService.UriFor<MainPageViewModel>().Navigate();
                                    }
                                });
                        },
                        TaskContinuationOptions.OnlyOnRanToCompletion);
                }
            }
        }
    }
}
