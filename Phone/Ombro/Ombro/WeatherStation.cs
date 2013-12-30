using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombro
{
    public class WeatherStation
    {
        public WeatherStation()
        {
            LastRefreshUTC = DateTime.MinValue;
            ResultTime = DateTime.MinValue;
        }

        public string Source { get; set; }
        public string SiteNumber { get; set; }
        public string Name { get; set; }
        public GeoCoordinate Location { get; set; }
        public double Precipitation1 { get; set; }
        public double Precipitation2 { get; set; }
        public double Precipitation3 { get; set; }
        public double Precipitation4 { get; set; }
        public double Precipitation5 { get; set; }
        public double Precipitation6 { get; set; }
        public double Precipitation7 { get; set; }
        public long Distance { get; set; }
        public double DistanceMiles { get; set; }
        public DateTime ResultTime { get; set; }
        public DateTime LastRefreshUTC { get; set; }
    }
}
