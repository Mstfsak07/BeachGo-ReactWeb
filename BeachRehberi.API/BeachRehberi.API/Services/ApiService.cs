using System.Text;
using System.Text.Json;

namespace BeachRehberi.API.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private string _token = "";
    private const string BaseUrl = "http://192.168.1.6:5143";

    public ApiService()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public void SetToken(string token)
    {
        _token = token;
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<BeachDto>> GetBeachesAsync()
    {
        try
        {
            var response = await _http.GetAsync("/api/beaches");
            if (!response.IsSuccessStatusCode) return new();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<BeachDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
        catch { return new(); }
    }

    public async Task<LoginResponseDto?> LoginAsync(string email, string password)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { email, password }),
                Encoding.UTF8,
                "application/json");
            var response = await _http.PostAsync("/api/auth/login", content);
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LoginResponseDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }

    public async Task<bool> UpdateOccupancyAsync(int percent)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { percent }),
                Encoding.UTF8,
                "application/json");
            var response = await _http.PutAsync("/api/business/occupancy", content);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> UpdateSpecialAsync(string message)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { message }),
                Encoding.UTF8,
                "application/json");
            var response = await _http.PutAsync("/api/business/special", content);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<DashboardDto?> GetDashboardAsync()
    {
        try
        {
            var response = await _http.GetAsync("/api/business/dashboard");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DashboardDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }
}

// DTO'lar sınıf dışında, namespace içinde
public class DashboardDto
{
    public BeachDto Beach { get; set; } = new();
    public DashboardStats Stats { get; set; } = new();
}

public class DashboardStats
{
    public int TodayReservationCount { get; set; }
    public int TodayPersonCount { get; set; }
    public int OccupancyPercent { get; set; }
    public string OccupancyLevel { get; set; } = "";
}

public class BeachDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string CoverImageUrl { get; set; } = "";
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public string OpenTime { get; set; } = "";
    public string Amenities { get; set; } = "";
    public bool HasBar { get; set; }
    public bool HasWaterSports { get; set; }
    public bool HasDJ { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = "";
    public string Message { get; set; } = "";
}