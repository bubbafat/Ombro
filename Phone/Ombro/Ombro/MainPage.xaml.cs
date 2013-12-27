using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Ombro.Resources;
using System.Threading.Tasks;

namespace Ombro
{
    public partial class MainPage : PhoneApplicationPage
    {
        private WeatherStationSettings _cache;
        private readonly object _cacheLock = new object();
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            PhoneApplicationService.Current.Closing += Current_Closing;
            PhoneApplicationService.Current.Deactivated += Current_Deactivated;

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();

            this.CanRefresh = false;
            this.DataContext = this;

            this.RainGauge.Loaded += RainGauge_Loaded;
        }

        void RainGauge_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadFromSettings();
        }

        public static readonly DependencyProperty StationSourceProperty =
            DependencyProperty.Register("StationSource", typeof(String), typeof(MainPage), new PropertyMetadata(null));
        
        public static readonly DependencyProperty StationIDProperty =
            DependencyProperty.Register("StationID", typeof(String), typeof(MainPage), new PropertyMetadata(null));

        public static readonly DependencyProperty StationNameProperty =
            DependencyProperty.Register("StationName", typeof(String), typeof(MainPage), new PropertyMetadata(null));

        public string StationSource
        {
            get { return (String)GetValue(StationSourceProperty); }
            set { SetValue(StationSourceProperty, value); }
        }

        public string StationID
        {
            get { return (String)GetValue(StationIDProperty); }
            set { SetValue(StationIDProperty, value); }
        }

        public string StationName
        {
            get { return (String)GetValue(StationNameProperty); }
            set { SetValue(StationNameProperty, value); }
        }



        public bool CanRefresh
        {
            get { return (bool)GetValue(CanRefreshProperty); }
            set { SetValue(CanRefreshProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanRefresh.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanRefreshProperty =
            DependencyProperty.Register("CanRefresh", typeof(bool), typeof(MainPage), new PropertyMetadata(null));



        public bool ShowHelp
        {
            get { return (bool)GetValue(ShowHelpProperty); }
            set { SetValue(ShowHelpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowHelp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowHelpProperty =
            DependencyProperty.Register("ShowHelp", typeof(bool), typeof(MainPage), new PropertyMetadata(null));

        public bool NotShowHelp
        {
            get { return (bool)GetValue(NotShowHelpProperty); }
            set { SetValue(NotShowHelpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NotShowHelp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotShowHelpProperty =
            DependencyProperty.Register("NotShowHelp", typeof(bool), typeof(MainPage), new PropertyMetadata(null));

        

        public string LastUpdateDisplay
        {
            get { return (string)GetValue(LastUpdateDisplayProperty); }
            set { SetValue(LastUpdateDisplayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LastUpdateDisplay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastUpdateDisplayProperty =
            DependencyProperty.Register("LastUpdateDisplay", typeof(string), typeof(MainPage), new PropertyMetadata(null));

        

        public Double StationDepth1
        {
            get { return (Double)GetValue(StationDepth1Property); }
            set { SetValue(StationDepth1Property, value); }
        }

        public static readonly DependencyProperty StationDepth1Property =
            DependencyProperty.Register("StationDepth1", typeof(Double), typeof(MainPage), new PropertyMetadata(null));

        public Double StationDepth7
        {
            get { return (Double)GetValue(StationDepth7Property); }
            set { SetValue(StationDepth7Property, value); }
        }

        public static readonly DependencyProperty StationDepth7Property =
            DependencyProperty.Register("StationDepth7", typeof(Double), typeof(MainPage), new PropertyMetadata(null));

        void Current_Deactivated(object sender, DeactivatedEventArgs e)
        {
            lock (_cacheLock)
            {
                if (_cache != null)
                {
                    _cache.Save(true).Wait(TimeSpan.FromSeconds(5));
                }
            }
        }

        void DisplayHelp(bool showHelp)
        {
            ShowHelp = showHelp;
            NotShowHelp = !showHelp;
        }

        void Current_Closing(object sender, ClosingEventArgs e)
        {
            lock (_cacheLock)
            {
                if (_cache != null)
                {
                    _cache.Save(true).Wait(TimeSpan.FromSeconds(5));
                }
            }
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
                        Dispatcher.BeginInvoke(() =>
                            {
                                DisplayHelp(true);
                            });
                    }
                }

                if (!refreshing)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        SystemTray.ProgressIndicator.IsVisible = false;
                    });
                }
            });
        }

        private void ApplicationBarMenuItem_ChooseStation(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SelectStation.xaml", UriKind.Relative));
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
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
                        Dispatcher.BeginInvoke(() =>
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
            Dispatcher.BeginInvoke(() =>
            {
                SystemTray.ProgressIndicator.IsVisible = true;
                this.CanRefresh = false;
            });
        }

        private void DoneRefreshing()
        {
            Dispatcher.BeginInvoke(() =>
            {
                SystemTray.ProgressIndicator.IsVisible = false;
                this.CanRefresh = true;
            });
        }


        private async void UpdateRainGauge(WeatherStation current)
        {
            Dispatcher.BeginInvoke(() =>
            {
                this.RainGauge.Depth = current.Precipitation7;
                this.StationDepth1 = current.Precipitation1;
                this.StationDepth7 = current.Precipitation7;
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

        private string GetRefreshDisplay(DateTime dateTime)
        {
            if(dateTime == DateTime.MinValue)
            {
                return "never";
            }

            var ago = DateTime.UtcNow - dateTime;
            if(ago.TotalDays >= 1)
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

        private void ChooseStation_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SelectStation.xaml", UriKind.Relative));
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}