using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Device.Location;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Ombro
{
    public sealed partial class SelectStation : PhoneApplicationPage, IDisposable
    {
        private GeoCoordinateWatcher _loc;
        private readonly ObservableCollection<WeatherStation> _stations = new ObservableCollection<WeatherStation>();
        private readonly object _listLock = new object();

        public SelectStation()
        {
            InitializeComponent();
            WeatherStationList.DataContext = _stations;
            this.Loaded += SelectStation_Loaded;
            this.WeatherStationList.SelectionChanged += WeatherStationList_SelectionChanged;
        }

        void WeatherStationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.btnSelect.IsEnabled = this.WeatherStationList.SelectedItem is WeatherStation;
        }

        void SelectStation_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnSelect.IsEnabled = false;

            var loadTask = WeatherStationSettings.Load();
            loadTask.ContinueWith(s =>
            {
                WeatherStationSettings settings = s.Result;

                if (settings.NearbyStations != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        this.btnSelect.IsEnabled = false;
                        lock (_listLock)
                        {
                            _stations.Clear();
                            foreach (var station in settings.NearbyStations)
                            {
                                this._stations.Add(station);
                            }
                        }
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

        private void btnFindNearMe_Click(object sender, RoutedEventArgs e)
        {
            this.btnFindNearMe.IsEnabled = false;
            this.btnSelect.IsEnabled = false;

            _stations.Clear();
            this.LoadingIndicator.IsVisible = true;

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
            Dispatcher.BeginInvoke(() =>
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
                    this.LoadingIndicator.IsVisible = false;
                    this.btnFindNearMe.IsEnabled = true;
                    this.btnSelect.IsEnabled = false;
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
            Dispatcher.BeginInvoke(() =>
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

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            lock (_listLock)
            {
                if (this.WeatherStationList.SelectedItem is WeatherStation)
                {
                    WeatherStationSettings settings = new WeatherStationSettings(
                        this.WeatherStationList.SelectedItem as WeatherStation, this._stations);
                    
                    var saveTask = settings.Save();
                    saveTask.ContinueWith(a =>
                        {
                            Dispatcher.BeginInvoke(() =>
                                {
                                    if (NavigationService.CanGoBack)
                                    {
                                        NavigationService.GoBack();
                                    }
                                    else
                                    {
                                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                                    }
                                });
                        },
                        TaskContinuationOptions.OnlyOnRanToCompletion);
                }
            }
        }
    }
}