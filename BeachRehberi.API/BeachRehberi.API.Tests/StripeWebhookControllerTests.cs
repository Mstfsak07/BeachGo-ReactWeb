using System.Reflection;
using BeachRehberi.API.Controllers;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Stripe.Checkout;

namespace BeachRehberi.API.Tests;

public class StripeWebhookControllerTests
{
    [Fact]
    public async Task Completed_session_marks_pending_reservation_paid_and_approved()
    {
        await using var db = CreateDbContext();
        var reservation = new Reservation
        {
            UserId = 7,
            BeachId = 11,
            ReservationDate = DateTime.UtcNow.Date.AddDays(1),
            Status = ReservationStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            TotalPrice = 300m
        };
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        await InvokeCompletedAsync(controller, new Session
        {
            Id = "cs_test_123",
            Metadata = new Dictionary<string, string> { ["reservationId"] = reservation.Id.ToString() }
        });

        var storedReservation = await db.Reservations.SingleAsync();
        var storedPayment = await db.ReservationPayments.SingleAsync();

        Assert.Equal(PaymentStatus.Paid, storedReservation.PaymentStatus);
        Assert.Equal(ReservationStatus.Approved, storedReservation.Status);
        Assert.Equal(PaymentStatus.Paid, storedPayment.Status);
        Assert.Equal("cs_test_123", storedPayment.TransactionId);
    }

    [Fact]
    public async Task Completed_session_is_idempotent_for_already_paid_reservation()
    {
        await using var db = CreateDbContext();
        var reservation = new Reservation
        {
            UserId = 7,
            BeachId = 11,
            ReservationDate = DateTime.UtcNow.Date.AddDays(1),
            Status = ReservationStatus.Approved,
            PaymentStatus = PaymentStatus.Paid,
            TotalPrice = 300m
        };
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();

        db.ReservationPayments.Add(new ReservationPayment
        {
            ReservationId = reservation.Id,
            Amount = 300m,
            Status = PaymentStatus.Paid,
            TransactionId = "cs_original",
            PaymentMethod = "Stripe",
            PaidAt = DateTime.UtcNow.AddMinutes(-5)
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        await InvokeCompletedAsync(controller, new Session
        {
            Id = "cs_duplicate",
            Metadata = new Dictionary<string, string> { ["reservationId"] = reservation.Id.ToString() }
        });

        var payments = await db.ReservationPayments.ToListAsync();

        Assert.Single(payments);
        Assert.Equal("cs_original", payments[0].TransactionId);
        Assert.Equal(PaymentStatus.Paid, payments[0].Status);
    }

    private static StripeWebhookController CreateController(BeachDbContext db)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        return new StripeWebhookController(config, NullLogger<StripeWebhookController>.Instance, db);
    }

    private static async Task InvokeCompletedAsync(StripeWebhookController controller, Session session)
    {
        var method = typeof(StripeWebhookController).GetMethod(
            "HandleCheckoutSessionCompletedAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(method);

        var task = method!.Invoke(controller, new object?[] { session, CancellationToken.None }) as Task;
        Assert.NotNull(task);
        await task!;
    }

    private static BeachDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BeachDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BeachDbContext(options);
    }
}
