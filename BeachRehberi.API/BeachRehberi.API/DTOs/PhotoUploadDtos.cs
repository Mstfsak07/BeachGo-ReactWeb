using Microsoft.AspNetCore.Http;

namespace BeachRehberi.API.DTOs;

public class UploadBeachPhotoRequest
{
    public IFormFile? File { get; set; }
    public string? Caption { get; set; }
    public bool IsCover { get; set; }
}

public class UploadBeachPhotoResponse
{
    public int PhotoId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public bool IsCover { get; set; }
}
