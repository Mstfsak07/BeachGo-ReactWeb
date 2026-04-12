using BeachRehberi.API.Data;
using BeachRehberi.API.Features.Auth.Commands.Register;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Tests;

public class RegisterHandlerTests
{
    [Fact]
    public async Task Handle_creates_business_account_when_business_fields_are_present()
    {
        await using var db = CreateDbContext();
        var handler = new RegisterHandler(db, new FakeOtpService(), new FakeEmailService());

        var result = await handler.Handle(
            new RegisterCommand(new RegisterRequest
            {
                BusinessName = "Beach Club",
                ContactName = "Ayse Kaya",
                Email = "Business@Example.com",
                Password = "Password123!",
                BeachId = 12
            }),
            CancellationToken.None);

        var createdUser = await db.BusinessUsers.SingleAsync();

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal(UserRoles.Business, result.User!.Role);
        Assert.Equal(UserRoles.Business, result.User.AccountType);
        Assert.Equal(UserRoles.Business, createdUser.Role);
        Assert.Equal("business@example.com", createdUser.Email);
        Assert.Equal(12, createdUser.BeachId);
        Assert.Equal("Beach Club", createdUser.BusinessName);
        Assert.Equal("Ayse", createdUser.FirstName);
    }

    [Fact]
    public async Task Handle_creates_business_account_for_business_register_command()
    {
        await using var db = CreateDbContext();
        var handler = new RegisterHandler(db, new FakeOtpService(), new FakeEmailService());

        var result = await handler.Handle(
            new BusinessRegisterCommand(new BusinessRegisterRequest
            {
                BusinessName = "Blue Bay",
                ContactName = "Mehmet Demir",
                Email = " BlueBay@Example.com ",
                Password = "Password123!"
            }),
            CancellationToken.None);

        var createdUser = await db.BusinessUsers.SingleAsync();

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal(UserRoles.Business, result.User!.Role);
        Assert.Equal(UserRoles.Business, result.User.AccountType);
        Assert.Equal(UserRoles.Business, createdUser.Role);
        Assert.Equal("bluebay@example.com", createdUser.Email);
        Assert.Equal("Blue Bay", createdUser.BusinessName);
        Assert.Equal("Mehmet", createdUser.FirstName);
    }

    private static BeachDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BeachDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BeachDbContext(options);
    }

    private sealed class FakeOtpService : IOtpService
    {
        public Task<string> GenerateOtpAsync(string email, OtpPurpose purpose) => Task.FromResult("123456");
        public Task<bool> ValidateOtpAsync(string email, string otpCode, OtpPurpose purpose) => Task.FromResult(true);
        public Task<string> GenerateTokenAsync(string email, string purpose) => Task.FromResult("verification-token");
        public Task<bool> ValidateTokenAsync(string email, string purpose, string token) => Task.FromResult(true);
        public Task InvalidateTokenAsync(string email, string purpose) => Task.CompletedTask;
        public Task<string> SendOtpAsync(string email) => Task.FromResult("verification-id");
        public Task<bool> VerifyOtpAsync(string verificationId, string code) => Task.FromResult(true);
        public Task<bool> IsEmailVerifiedAsync(string email) => Task.FromResult(true);
    }

    private sealed class FakeEmailService : IEmailService
    {
        public Task SendEmailVerificationAsync(string toEmail, string fullName, string token)
            => Task.CompletedTask;

        public Task SendPasswordResetAsync(string toEmail, string fullName, string token)
            => Task.CompletedTask;
    }
}
