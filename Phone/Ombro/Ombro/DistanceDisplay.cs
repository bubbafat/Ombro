using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombro
{
    public sealed class DistanceDisplay
    {
        public DistanceDisplay(double miles)
        {
            UpdateMiles(miles);
        }

        public void UpdateMiles(double miles)
        {
            DistanceMiles = miles;
            DistanceKM = DistanceConverter.MilesToKM(miles);
        }

        public double DistanceMiles { get; private set; }
        public double DistanceKM { get; private set; }
    }

    internal static class DistanceConverter
    {
        public static double MilesToKM(double miles)
        {
            return miles * 1.60934;
        }

        public static double KMtoMiles(double km)
        {
            return km * 0.621371;
        }
    }
}
