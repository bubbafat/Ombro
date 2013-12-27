using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Ombro
{
    internal class WeatherStationRepository
    {
        internal sealed class RequestState : IDisposable
        {
            public ManualResetEvent Done = new ManualResetEvent(false);
            public List<WeatherStation> Stations = new List<WeatherStation>();
            public bool Cancelled = false;
            public Exception Error = null;

            public void Dispose()
            {
                using (Done) { }
            }
        }

        public static RequestState GetStationsNear(GeoPosition<GeoCoordinate> geoPosition, double withinKilometers)
        {
            var boundingBox = GpsBoundingBoxBuilder.GetBoundingBox(geoPosition.Location, withinKilometers);

            string url = "http://waterdata.usgs.gov/nwis/current?nw_longitude_va={MaxLong}&nw_latitude_va={MaxLat}&se_longitude_va={MinLong}&se_latitude_va={MinLat}&coordinate_format=decimal_degrees&index_pmcode_STATION_NM=1&index_pmcode_DATETIME=2&index_pmcode_00045=3&precipitation_interval=precip24h_va&group_key=NONE&format=sitefile_output&sitefile_output_format=rdb_file&column_name=agency_cd&column_name=site_no&column_name=station_nm&column_name=dec_lat_va&column_name=dec_long_va&sort_key_2=site_no&html_table_group_key=NONE&rdb_compression=file&list_of_search_criteria=lat_long_bounding_box%2Crealtime_parameter_selection";
            url = url.Replace("{MaxLong}", boundingBox.MaxPoint.Longitude.ToString());
            url = url.Replace("{MaxLat}", boundingBox.MaxPoint.Latitude.ToString());
            url = url.Replace("{MinLong}", boundingBox.MinPoint.Longitude.ToString());
            url = url.Replace("{MinLat}", boundingBox.MinPoint.Latitude.ToString());

            return RunRequest(url);
        }

        private static RequestState RunRequest(string url)
        {
            WebClient client = new WebClient();
            client.DownloadStringCompleted += client_DownloadStringCompleted;
            RequestState request = new RequestState();
            client.DownloadStringAsync(new Uri(url, UriKind.Absolute), request);

            TimeSpan waitTime = System.Diagnostics.Debugger.IsAttached ? TimeSpan.FromDays(1) : TimeSpan.FromSeconds(30);

            if (request.Done.WaitOne(waitTime))
            {
                return request;
            }

            request.Error = new TimeoutException();
            return request;
        }

        static void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            RequestState state = (RequestState)e.UserState;

            state.Cancelled = e.Cancelled;
            state.Error = e.Error;

            if(false == state.Cancelled && state.Error == null)
            {
                state.Stations = TsvParser.ParseStations(e.Result);
            }

            state.Done.Set();
        }

        public static async Task<RequestState> GetStationData(string stationId)
        {
            return await Task<RequestState>.Run(() =>
            {
                // 
                // http://waterdata.usgs.gov/nwis/current?search_site_no=02084000&search_site_no_match_type=exact&index_pmcode_STATION_NM=1&index_pmcode_DATETIME=2&index_pmcode_00045=3&precipitation_interval=precip24h_va&precipitation_interval=precip02d_va&precipitation_interval=precip03d_va&precipitation_interval=precip04d_va&precipitation_interval=precip05d_va&precipitation_interval=precip06d_va&precipitation_interval=precip07d_va&group_key=NONE&sitefile_output_format=html_table&column_name=agency_cd&column_name=site_no&column_name=station_nm&sort_key_2=site_no&html_table_group_key=NONE&format=rdb&rdb_compression=file&list_of_search_criteria=search_site_no%2Crealtime_parameter_selection&column_name=dec_lat_va&column_name=dec_long_va
                string url = "http://waterdata.usgs.gov/nwis/current?search_site_no={SiteNumber}&search_site_no_match_type=exact&index_pmcode_STATION_NM=1&index_pmcode_DATETIME=2&index_pmcode_00045=3&precipitation_interval=precip24h_va&precipitation_interval=precip02d_va&precipitation_interval=precip03d_va&precipitation_interval=precip04d_va&precipitation_interval=precip05d_va&precipitation_interval=precip06d_va&precipitation_interval=precip07d_va&group_key=NONE&sitefile_output_format=html_table&column_name=agency_cd&column_name=site_no&column_name=station_nm&sort_key_2=site_no&html_table_group_key=NONE&format=rdb&rdb_compression=file&list_of_search_criteria=search_site_no%2Crealtime_parameter_selection&column_name=dec_lat_va&column_name=dec_long_va";

                url = url.Replace("{SiteNumber}", stationId);

                return RunRequest(url);
            });
        }
    }
}
