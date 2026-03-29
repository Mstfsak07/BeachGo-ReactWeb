namespace BeachRehberi.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    private Money() { Currency = "TRY"; }

    public Money(decimal amount, string currency = "TRY")
    {
        if (amount < 0)
            throw new ArgumentException("Para miktarı negatif olamaz.", nameof(amount));

        Amount = amount;
        Currency = currency ?? "TRY";
    }

    public static Money Zero => new(0);
    public static Money FromTry(decimal amount) => new(amount, "TRY");

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Farklı para birimleri toplanamaz.");
        return new Money(Amount + other.Amount, Currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
