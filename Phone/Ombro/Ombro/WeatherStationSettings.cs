using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombro
{
    public class WeatherStationSettings
    {
        private static WeatherStationSettings _cached = null;

        public WeatherStationSettings()
        {
        }

        internal WeatherStationSettings(WeatherStation current, IEnumerable<WeatherStation> stations)
        {
            CurrentStation = current;
            NearbyStations = new List<WeatherStation>(stations);
        }

        public List<WeatherStation> NearbyStations { get; set; }

        public WeatherStation CurrentStation { get; set; }

        public async Task Save(bool flush = false)
        {
            _cached = this;
            if(flush)
            {
                await Flush();
            }
        }

        public static async Task Flush()
        {
            WeatherStationSettings c = _cached;
            if (c != null)
            {
                await IsolatedStorageOperations.Save<WeatherStationSettings>(c, "OmbroSettings.xml");
            }
        }

        public static async Task<WeatherStationSettings> Load()
        {
            WeatherStationSettings settings = _cached;

            if (settings == null)
            {
                return await IsolatedStorageOperations.Load<WeatherStationSettings>("OmbroSettings.xml");
            }
            else
            {
                return await Task<WeatherStationSettings>.Run(() =>
                    {
                        return settings;
                    });
            }
        }
    }
}
