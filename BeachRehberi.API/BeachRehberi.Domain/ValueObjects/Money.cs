using BeachRehberi.Domain.Exceptions;

namespace BeachRehberi.Domain.ValueObjects;

/// <summary>
/// Money value object for handling monetary values
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "TRY")
    {
        if (amount < 0)
            throw new DomainValidationException("Amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainValidationException("Currency cannot be empty.");

        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = "TRY") => new Money(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainValidationException("Cannot add money with different currencies.");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainValidationException("Cannot subtract money with different currencies.");

        var result = Amount - other.Amount;
        if (result < 0)
            throw new DomainValidationException("Result cannot be negative.");

        return new Money(result, Currency);
    }

    public bool Equals(Money? other)
    {
        return other != null && Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Money);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    public override string ToString()
    {
        return $"{Amount:N2} {Currency}";
    }
}