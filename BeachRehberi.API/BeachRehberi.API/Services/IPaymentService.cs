using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Models.Enums;

namespace BeachRehberi.API.Services;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(int reservationId, decimal amount, string paymentMethod = "Mock");
}

public class MockPaymentService : IPaymentService
{
    private readonly ILogger<MockPaymentService> _logger;
    private readonly BeachDbContext _db;

    public MockPaymentService(ILogger<MockPaymentService> logger, BeachDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<bool> ProcessPaymentAsync(int reservationId, decimal amount, string paymentMethod = "Mock")
    {
        _logger.LogInformation("[MOCK PAYMENT] ReservationId: {ResId} | Amount: {Amount}", reservationId, amount);

        var payment = new ReservationPayment
        {
            ReservationId = reservationId,
            Amount = amount,
            Status = PaymentStatus.Paid,
            TransactionId = Guid.NewGuid().ToString("N"),
            PaymentMethod = paymentMethod,
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow
        };

        _db.ReservationPayments.Add(payment);
        await _db.SaveChangesAsync();

        return true;
    }
}