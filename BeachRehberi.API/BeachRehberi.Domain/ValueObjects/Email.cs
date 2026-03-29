using System.Text.RegularExpressions;
using BeachRehberi.Domain.Exceptions;

namespace BeachRehberi.Domain.ValueObjects;

/// <summary>
/// Email value object
/// </summary>
public sealed class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainValidationException("Email cannot be empty.");

        if (!EmailRegex.IsMatch(email))
            throw new DomainValidationException("Invalid email format.");

        if (email.Length > 254)
            throw new DomainValidationException("Email is too long.");

        return new Email(email.ToLowerInvariant());
    }

    public bool Equals(Email? other)
    {
        return other != null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Email);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Email email) => email.Value;
}