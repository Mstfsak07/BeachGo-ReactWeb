using System.Net.Http.Json;
using System.Text.Json;
using BeachRehberi.API.Data;
using BeachRehberi.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public class GooglePlaceReviewsService : IGooglePlaceReviewsService
{
    private const string PlacesApiBaseUrl = "https://places.googleapis.com/v1";

    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly BeachDbContext _db;

    public GooglePlaceReviewsService(HttpClient http, IConfiguration config, BeachDbContext db)
    {
        _http = http;
        _config = config;
        _db = db;
    }

    public async Task<GoogleReviewsResponseDto?> GetBeachReviewsAsync(int beachId, CancellationToken cancellationToken = default)
    {
        var beach = await _db.Beaches.FirstOrDefaultAsync(b => b.Id == beachId, cancellationToken);
        if (beach == null)
        {
            return null;
        }

        var apiKey = _config["ApiKeys:GooglePlaces"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("YOUR_GOOGLE_PLACES_API_KEY", StringComparison.Ordinal))
        {
            return new GoogleReviewsResponseDto
            {
                IsConfigured = false,
                HasPlaceMatch = false
            };
        }

        var placeId = string.IsNullOrWhiteSpace(beach.GooglePlaceId)
            ? await ResolvePlaceIdAsync(beach, apiKey, cancellationToken)
            : beach.GooglePlaceId;

        if (string.IsNullOrWhiteSpace(placeId))
        {
            return new GoogleReviewsResponseDto
            {
                IsConfigured = true,
                HasPlaceMatch = false
            };
        }

        if (!string.Equals(beach.GooglePlaceId, placeId, StringComparison.Ordinal))
        {
            beach.GooglePlaceId = placeId;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return await GetPlaceDetailsAsync(placeId, apiKey, cancellationToken);
    }

    private async Task<string> ResolvePlaceIdAsync(Models.Beach beach, string apiKey, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{PlacesApiBaseUrl}/places:searchText");
        request.Headers.Add("X-Goog-Api-Key", apiKey);
        request.Headers.Add("X-Goog-FieldMask", "places.id");
        request.Content = JsonContent.Create(new
        {
            textQuery = $"{beach.Name} {beach.Address}".Trim(),
            languageCode = "tr",
            regionCode = "TR",
            maxResultCount = 1
        });

        using var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return string.Empty;
        }

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("places", out var places) || places.GetArrayLength() == 0)
        {
            return string.Empty;
        }

        var firstPlace = places[0];
        return firstPlace.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? string.Empty : string.Empty;
    }

    private async Task<GoogleReviewsResponseDto> GetPlaceDetailsAsync(string placeId, string apiKey, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{PlacesApiBaseUrl}/places/{placeId}?languageCode=tr&regionCode=TR");
        request.Headers.Add("X-Goog-Api-Key", apiKey);
        request.Headers.Add("X-Goog-FieldMask", "id,displayName,googleMapsUri,rating,userRatingCount,reviews");

        using var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new GoogleReviewsResponseDto
            {
                IsConfigured = true,
                HasPlaceMatch = false,
                PlaceId = placeId
            };
        }

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var root = document.RootElement;

        return new GoogleReviewsResponseDto
        {
            IsConfigured = true,
            HasPlaceMatch = true,
            PlaceId = placeId,
            PlaceName = ReadLocalizedText(root, "displayName"),
            GoogleMapsUri = ReadString(root, "googleMapsUri"),
            Rating = ReadNullableDouble(root, "rating"),
            UserRatingCount = ReadNullableInt(root, "userRatingCount"),
            Reviews = ReadReviews(root)
        };
    }

    private static List<GoogleReviewDto> ReadReviews(JsonElement root)
    {
        var result = new List<GoogleReviewDto>();
        if (!root.TryGetProperty("reviews", out var reviewsElement) || reviewsElement.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var review in reviewsElement.EnumerateArray())
        {
            var attribution = review.TryGetProperty("authorAttribution", out var authorElement)
                ? authorElement
                : default;

            result.Add(new GoogleReviewDto
            {
                AuthorName = ReadString(attribution, "displayName"),
                AuthorUri = ReadString(attribution, "uri"),
                AuthorPhotoUri = ReadString(attribution, "photoUri"),
                Rating = ReadNullableDouble(review, "rating") ?? 0,
                RelativePublishTimeDescription = ReadString(review, "relativePublishTimeDescription"),
                PublishTime = ReadString(review, "publishTime"),
                Text = ReadLocalizedText(review, "text"),
                OriginalText = ReadLocalizedText(review, "originalText")
            });
        }

        return result;
    }

    private static string ReadLocalizedText(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var localizedText) || localizedText.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        return localizedText.TryGetProperty("text", out var text) ? text.GetString() ?? string.Empty : string.Empty;
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var value))
        {
            return string.Empty;
        }

        return value.GetString() ?? string.Empty;
    }

    private static double? ReadNullableDouble(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.Number ? value.GetDouble() : null;
    }

    private static int? ReadNullableInt(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.Number ? value.GetInt32() : null;
    }
}
