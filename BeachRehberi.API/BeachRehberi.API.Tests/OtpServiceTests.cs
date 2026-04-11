using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BeachRehberi.API.Tests;

public class OtpServiceTests
{
    [Fact]
    public async Task VerifyOtpAsync_requires_matching_verification_id()
    {
        await using var db = CreateDbContext();
        var firstVerification = new VerificationCode
        {
            Email = "guest@example.com",
            CodeHash = ComputeSha256Hash("111111"),
            Purpose = OtpPurpose.EmailVerification,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        var secondVerification = new VerificationCode
        {
            Email = "guest@example.com",
            CodeHash = ComputeSha256Hash("222222"),
            Purpose = OtpPurpose.EmailVerification,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        db.VerificationCodes.AddRange(firstVerification, secondVerification);
        await db.SaveChangesAsync();

        var service = new OtpService(db, NullLogger<OtpService>.Instance);

        Assert.False(await service.VerifyOtpAsync(firstVerification.Id.ToString(), "222222"));
        Assert.True(await service.VerifyOtpAsync(secondVerification.Id.ToString(), "222222"));
        Assert.False(await service.IsEmailVerifiedAsync(firstVerification.Id.ToString()));
        Assert.True(await service.IsEmailVerifiedAsync(secondVerification.Id.ToString()));
    }

    [Fact]
    public async Task IsEmailVerifiedAsync_returns_true_only_after_successful_verification()
    {
        await using var db = CreateDbContext();
        var verification = new VerificationCode
        {
            Email = "guest@example.com",
            CodeHash = ComputeSha256Hash("654321"),
            Purpose = OtpPurpose.EmailVerification,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        db.VerificationCodes.Add(verification);
        await db.SaveChangesAsync();

        var service = new OtpService(db, NullLogger<OtpService>.Instance);

        var verified = await service.VerifyOtpAsync(verification.Id.ToString(), "654321");

        Assert.True(verified);
        Assert.True(await service.IsEmailVerifiedAsync(verification.Id.ToString()));
    }

    private static BeachDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BeachDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BeachDbContext(options);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256Hash = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
