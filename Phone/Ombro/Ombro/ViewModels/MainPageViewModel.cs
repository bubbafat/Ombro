using Caliburn.Micro;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Ombro.ViewModels
{
    public class MainPageViewModel : PropertyChangedBase
    {
        private WeatherStationSettings _cache;
        private readonly object _cacheLock = new object();

        private readonly INavigationService navigationService;

        public MainPageViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.ShowHelp = true;
        }

        private string _stationSource;
        private string _stationName;
        private string _stationId;

        private bool _canRefresh;
        private bool _showHelp;
        private bool _notShowHelp;

        private string _lastUpdateDisplay;
        private double _stationDepth1;
        private double _stationDepth7;
        private double _depth;
        private int _daysShown;

        public string LastUpdateDisplay
        {
            get { return _lastUpdateDisplay; }
            set
            {
                _lastUpdateDisplay = value;
                NotifyOfPropertyChange(() => LastUpdateDisplay);
            }
        }

        public int DaysShown
        {
            get { return _daysShown; }
            set
            {
                _daysShown = value;
                NotifyOfPropertyChange(() => DaysShown);
            }
        }

        public double Depth
        {
            get { return _depth; }
            set
            {
                _depth = value;
                NotifyOfPropertyChange(() => Depth);
            }
        }

        public double StationDepth1
        {
            get { return _stationDepth1; }
            set
            {
                _stationDepth1 = value;
                NotifyOfPropertyChange(() => StationDepth1);
            }
        }

        public double StationDepth7
        {
            get { return _stationDepth7; }
            set
            {
                _stationDepth7 = value;
                NotifyOfPropertyChange(() => StationDepth7);
            }
        }

        public bool CanRefresh
        {
            get { return _canRefresh; }
            set
            {
                _canRefresh = value;
                NotifyOfPropertyChange(() => CanRefresh);
            }
        }

        public bool ShowHelp
        {
            get { return _showHelp; }
            set
            {
                _showHelp = value;
                NotifyOfPropertyChange(() => ShowHelp);
            }
        }

        public bool NotShowHelp
        {
            get { return _notShowHelp; }
            set
            {
                _notShowHelp = value;
                NotifyOfPropertyChange(() => NotShowHelp);
            }
        }

        public string StationSource
        {
            get { return _stationSource; }
            set
            {
                _stationSource = value;
                NotifyOfPropertyChange(() => StationSource);
            }
        }

        public string StationName
        {
            get { return _stationName; }
            set
            {
                _stationName = value;
                NotifyOfPropertyChange(() => StationName);
            }
        }

        public string StationID
        {
            get { return _stationId; }
            set
            {
                _stationId = value;
                NotifyOfPropertyChange(() => StationID);
            }
        }

        internal void DisplayHelp(bool showHelp)
        {
            ShowHelp = showHelp;
            NotShowHelp = !showHelp;
        }

        public void ChooseStationBarAction()
        {
            navigationService.UriFor<SelectStationViewModel>().Navigate();
        }

        public void RateMyAppAction()
        {
            var marketplace = new MarketplaceReviewTask();
            marketplace.Show();
        }

        public void SettingsBarAction()
        {
            navigationService.UriFor<SettingsViewModel>().Navigate();
        }

        public void AboutAction()
        {
            navigationService.UriFor<AboutViewModel>().Navigate();
        }

        public void BuyMeABeerAction()
        {
            navigationService.UriFor<BuyMeABeerViewModel>().Navigate();
        }

        public async void RefreshCurrentAction()
        {
            await RefreshCurrentSelection();
        }

        private async Task RefreshCurrentSelection()
        {
            MarkRefreshing();
            DateTime start = DateTime.UtcNow;

            await WeatherStationRepository.GetStationData(_cache.CurrentStation.SiteNumber).ContinueWith(t =>
            {
                using (Ombro.WeatherStationRepository.RequestState state = t.Result)
                {
                    if (state.Error != null)
                    {
                        Execute.OnUIThread(() =>
                        {
                            MessageBox.Show(string.Format("Unable to refresh station information.\n\nNetwork data or wifi is required to load data.\n\nError message: {0}", state.Error.Message), "Error", MessageBoxButton.OK);
                        });
                    }
                    else
                    {
                        var elapsed = DateTime.UtcNow - start;
                        if (elapsed.TotalMilliseconds < 500)
                        {
                            System.Threading.Thread.Sleep(500 - (int)elapsed.TotalMilliseconds);
                        }

                        lock (_cacheLock)
                        {
                            _cache.CurrentStation = state.Stations.FirstOrDefault();
                            _cache.CurrentStation.LastRefreshUTC = DateTime.UtcNow;
                        }

                        UpdateRainGauge(_cache.CurrentStation);
                    }

                    DoneRefreshing();
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void MarkRefreshing()
        {
            Execute.OnUIThread(() =>
            {
                SystemTray.ProgressIndicator.IsVisible = true;
                this.CanRefresh = false;
            });
        }

        private void DoneRefreshing()
        {
            Execute.OnUIThread(() =>
            {
                SystemTray.ProgressIndicator.IsVisible = false;
                this.CanRefresh = true;
            });
        }


        private async void UpdateRainGauge(WeatherStation current)
        {
            Execute.OnUIThread(() =>
            {
                this.StationDepth1 = current.Precipitation1;
                this.StationDepth7 = current.Precipitation7;
                this.DaysShown = AppSettings.GetValue<int>(OmbroSettings.DaysOfRainToShow, 3);
                this.Depth = AssignProperDepth(current);
                this.StationSource = current.Source;
                this.StationID = current.SiteNumber;
                this.StationName = current.Name;
                this.LastUpdateDisplay = GetRefreshDisplay(current.LastRefreshUTC);
                this.CanRefresh = true;
                DisplayHelp(false);
            });

            if (_cache != null)
            {
                await _cache.Save(true);
            }
        }

        private double AssignProperDepth(WeatherStation station)
        {
            switch(DaysShown)
            {
                case 1:
                    return station.Precipitation1;
                case 2:
                    return station.Precipitation2;
                case 3:
                    return station.Precipitation3;
                case 4:
                    return station.Precipitation4;
                case 5:
                    return station.Precipitation5;
                case 6:
                    return station.Precipitation6;
                case 7:
                    return station.Precipitation7;
                default:
                    DaysShown = 3;
                    return station.Precipitation3;
            }
        }

        private string GetRefreshDisplay(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
            {
                return "never";
            }

            var ago = DateTime.UtcNow - dateTime;
            if (ago.TotalDays >= 1)
            {
                return string.Format("{0} days", (int)ago.TotalDays);
            }

            if (ago.TotalHours >= 1)
            {
                return string.Format("{0} hours", (int)ago.TotalHours);
            }

            if (ago.TotalMinutes >= 1)
            {
                return string.Format("{0} minutes", (int)ago.TotalMinutes);
            }

            return "a few moments";
        }

        public void ChooseStationAction()
        {
            navigationService.UriFor<SelectStationViewModel>().Navigate();
        }

        public void RainGaugeLoaded()
        {
            ReloadFromSettings();
        }

        private void ReloadFromSettings()
        {
            SystemTray.ProgressIndicator.IsVisible = true;

            WeatherStationSettings.Load().ContinueWith(async t =>
            {
                bool refreshing = false;

                if (t.IsCompleted && t.Result != null)
                {
                    if (t.Result.CurrentStation != null)
                    {
                        lock (_cacheLock)
                        {
                            _cache = t.Result;

                            UpdateRainGauge(_cache.CurrentStation);

                            if (_cache.CurrentStation.LastRefreshUTC < DateTime.UtcNow.AddMinutes(-15))
                            {
                                refreshing = true;
                            }
                        }

                        if (refreshing)
                        {
                            await RefreshCurrentSelection();
                        }
                    }
                    else
                    {
                        Execute.OnUIThread(() =>
                        {
                            DisplayHelp(true);
                        });
                    }
                }

                if (!refreshing)
                {
                    Execute.OnUIThread(() =>
                    {
                        SystemTray.ProgressIndicator.IsVisible = false;
                    });
                }
            });
        }

    }
}
