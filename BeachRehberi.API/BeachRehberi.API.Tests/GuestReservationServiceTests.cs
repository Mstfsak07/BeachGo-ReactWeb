using BeachRehberi.API.Data;
using BeachRehberi.API.DTOs.Reservation;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BeachRehberi.API.Tests;

public class GuestReservationServiceTests
{
    [Fact]
    public async Task CreateAsync_rejects_unverified_email()
    {
        await using var db = CreateDbContext();
        db.Beaches.Add(new Beach("Lara", "desc", "Antalya", 36.0, 30.0, 5)
        {
            HasEntryFee = true,
            EntryFee = 50
        });
        await db.SaveChangesAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Features:UseRealPayment"] = "true"
            })
            .Build();

        var service = new GuestReservationService(db, new RejectingOtpService(), new FakePaymentService(), config);

        var result = await service.CreateAsync(new CreateGuestReservationDto
        {
            BeachId = db.Beaches.Select(x => x.Id).Single(),
            ReservationDate = DateTime.UtcNow.Date.AddDays(1),
            ReservationTime = "10:00",
            ReservationType = "Standart",
            PersonCount = 2,
            FirstName = "Ada",
            LastName = "Yilmaz",
            Phone = "5550000000",
            Email = "guest@example.com",
            VerificationId = "invalid"
        });

        Assert.False(result.Success);
        Assert.Contains("doğrulama", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static BeachDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BeachDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BeachDbContext(options);
    }

    private sealed class RejectingOtpService : IOtpService
    {
        public Task<string> GenerateOtpAsync(string email, OtpPurpose purpose) => Task.FromResult(string.Empty);
        public Task<bool> ValidateOtpAsync(string email, string otpCode, OtpPurpose purpose) => Task.FromResult(false);
        public Task<string> GenerateTokenAsync(string email, string purpose) => Task.FromResult(string.Empty);
        public Task<bool> ValidateTokenAsync(string email, string purpose, string token) => Task.FromResult(false);
        public Task InvalidateTokenAsync(string email, string purpose) => Task.CompletedTask;
        public Task<string> SendOtpAsync(string email) => Task.FromResult(string.Empty);
        public Task<bool> VerifyOtpAsync(string verificationId, string code) => Task.FromResult(false);
        public Task<bool> IsEmailVerifiedAsync(string email) => Task.FromResult(false);
    }

    private sealed class FakePaymentService : IPaymentService
    {
        public Task<PaymentProcessResult> ProcessPaymentAsync(int reservationId, decimal amount, string paymentMethod = "Stripe", string? confirmationCode = null)
            => Task.FromResult(new PaymentProcessResult { Success = true });
    }
}
