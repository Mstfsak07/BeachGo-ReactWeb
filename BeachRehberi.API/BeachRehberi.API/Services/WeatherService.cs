namespace BeachRehberi.API.Services;


public class WeatherService : IWeatherService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public WeatherService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<WeatherData> GetWeatherAsync(double lat, double lng)
    {
        var apiKey = _config["ApiKeys:OpenWeather"];
        var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lng}&appid={apiKey}&units=metric&lang=tr";

        try
        {
            var response = await _http.GetFromJsonAsync<OpenWeatherResponse>(url);
            if (response == null) return GetDefaultWeather();

            var temp = response.Main.Temp;
            return new WeatherData
            {
                Temperature = Math.Round(temp, 1),
                FeelsLike = Math.Round(response.Main.FeelsLike, 1),
                Description = response.Weather.FirstOrDefault()?.Description ?? "",
                Icon = response.Weather.FirstOrDefault()?.Icon ?? "",
                WindSpeed = Math.Round(response.Wind.Speed * 3.6, 1),
                Humidity = response.Main.Humidity,
                UvIndex = 0,
                BeachCondition = GetBeachCondition(temp, response.Wind.Speed)
            };
        }
        catch
        {
            return GetDefaultWeather();
        }
    }

    public async Task<SeaData> GetSeaDataAsync(double lat, double lng)
    {
        var apiKey = _config["ApiKeys:Stormglass"];
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var url = $"https://api.stormglass.io/v2/weather/point?lat={lat}&lng={lng}&params=waterTemperature,waveHeight,windSpeed&start={now}&end={now}";

        try
        {
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", apiKey);
            var response = await _http.GetFromJsonAsync<StormglassResponse>(url);

            var hour = response?.Hours?.FirstOrDefault();
            if (hour == null) return GetDefaultSea();

            var waterTemp = hour.WaterTemperature?.Sg ?? 24;
            var waveHeight = hour.WaveHeight?.Sg ?? 0.3;

            return new SeaData
            {
                WaterTemperature = Math.Round(waterTemp, 1),
                WaveHeight = Math.Round(waveHeight, 2),
                WindSpeed = Math.Round(hour.WindSpeed?.Sg ?? 10, 1),
                Condition = waveHeight < 0.5 ? "Sakin" : waveHeight < 1.5 ? "Hafif dalgalı" : "Dalgalı"
            };
        }
        catch
        {
            return GetDefaultSea();
        }
    }

    private static string GetBeachCondition(double temp, double wind)
    {
        if (temp >= 28 && wind < 5) return "Mükemmel plaj günü! ☀️";
        if (temp >= 24 && wind < 8) return "Güzel bir gün 🌤";
        if (temp >= 20) return "Serin ama yüzülebilir 🌥";
        return "Plaj için serin 🌧";
    }

    private static WeatherData GetDefaultWeather() => new()
    {
        Temperature = 32,
        FeelsLike = 34,
        Description = "Açık",
        WindSpeed = 12,
        Humidity = 45,
        UvIndex = 8,
        BeachCondition = "Mükemmel plaj günü! ☀️"
    };

    private static SeaData GetDefaultSea() => new()
    {
        WaterTemperature = 26,
        WaveHeight = 0.3,
        WindSpeed = 10,
        Condition = "Sakin"
    };
}

// OpenWeather response modelleri
public class OpenWeatherResponse
{
    public MainData Main { get; set; } = new();
    public List<WeatherInfo> Weather { get; set; } = new();
    public WindData Wind { get; set; } = new();
}
public class MainData
{
    public double Temp { get; set; }
    public double FeelsLike { get; set; }
    public int Humidity { get; set; }
}
public class WeatherInfo
{
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
public class WindData { public double Speed { get; set; } }

// Stormglass response modelleri
public class StormglassResponse { public List<StormglassHour> Hours { get; set; } = new(); }
public class StormglassHour
{
    public StormglassValue? WaterTemperature { get; set; }
    public StormglassValue? WaveHeight { get; set; }
    public StormglassValue? WindSpeed { get; set; }
}
public class StormglassValue { public double Sg { get; set; } }