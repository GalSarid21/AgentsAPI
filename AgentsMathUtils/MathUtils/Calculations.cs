using AgentsUtils.Consts;
using GoogleApi.Entities.Common;
using System;

namespace AgentsUtils.MathUtils
{
    public static class Calculations
    {
        public static double GetDistanceFromLatLongInKM(Coordinate dstCoordination, Coordinate originCoordination)
        {
            double latDiffInRad = ConvertDegreesToRadians(dstCoordination.Latitude - originCoordination.Latitude);
            double lonDiffInRad = ConvertDegreesToRadians(dstCoordination.Longitude - originCoordination.Longitude);
            double haversine = Math.Sin(latDiffInRad / 2) * Math.Sin(latDiffInRad / 2) +
                               Math.Cos(ConvertDegreesToRadians(originCoordination.Latitude)) *
                               Math.Cos(ConvertDegreesToRadians(dstCoordination.Latitude)) *
                               Math.Sin(lonDiffInRad / 2) * Math.Sin(lonDiffInRad / 2);

            return 2 * CalculationConsts.EARTH_RADIUS_IN_KM * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
        }
        public static double ConvertDegreesToRadians(double input)
        {
            return input * (Math.PI / 180);
        }
    }
}
