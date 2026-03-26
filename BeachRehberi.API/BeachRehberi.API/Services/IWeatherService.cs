namespace BeachRehberi.API.Services;

public interface IWeatherService
{
    Task<WeatherData> GetWeatherAsync(double lat, double lng);
    Task<SeaData> GetSeaDataAsync(double lat, double lng);
}

public class WeatherData
{
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double WindSpeed { get; set; }
    public int Humidity { get; set; }
    public int UvIndex { get; set; }
    public string BeachCondition { get; set; } = string.Empty;
}

public class SeaData
{
    public double WaterTemperature { get; set; }
    public double WaveHeight { get; set; }
    public double WindSpeed { get; set; }
    public string Condition { get; set; } = string.Empty;
}