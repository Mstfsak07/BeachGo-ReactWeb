namespace BeachRehberi.API.Services;

public interface IGeoCalculator
{
    double GetDistance(double lat1, double lon1, double lat2, double lon2);
}

public class GeoCalculator : IGeoCalculator
{
    public double GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var d1 = lat1 * Math.PI / 180.0;
        var d2 = lat2 * Math.PI / 180.0;
        var d3 = (lat2 - lat1) * Math.PI / 180.0;
        var d4 = (lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(d3 / 2) * Math.Sin(d3 / 2) +
                Math.Cos(d1) * Math.Cos(d2) * Math.Sin(d4 / 2) * Math.Sin(d4 / 2);
        return 6371 * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
