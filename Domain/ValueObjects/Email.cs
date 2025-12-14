namespace GoldPriceTracker.Domain.ValueObjects;

/// <summary>
/// Email value object with validation.
/// </summary>
public class Email
{
    public string Value { get; private set; }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));

        if (!value.Contains('@'))
            throw new ArgumentException("Invalid email format", nameof(value));

        Value = value.ToLowerInvariant();
    }

    public static Email Create(string email)
    {
        return new Email(email);
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is Email email && Value == email.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
}

