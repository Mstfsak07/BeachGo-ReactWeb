namespace BeachRehberi.API.DTOs;

public class GoogleReviewsResponseDto
{
    public bool IsConfigured { get; set; }
    public bool HasPlaceMatch { get; set; }
    public string PlaceId { get; set; } = string.Empty;
    public string PlaceName { get; set; } = string.Empty;
    public string GoogleMapsUri { get; set; } = string.Empty;
    public double? Rating { get; set; }
    public int? UserRatingCount { get; set; }
    public List<GoogleReviewDto> Reviews { get; set; } = new();
}

public class GoogleReviewDto
{
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorUri { get; set; } = string.Empty;
    public string AuthorPhotoUri { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string RelativePublishTimeDescription { get; set; } = string.Empty;
    public string PublishTime { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
}
