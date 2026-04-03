using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Identity.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        Guard.NotNullOrWhiteSpace(value, nameof(value));
        var normalized = value.Trim().ToLowerInvariant();
        if (!normalized.Contains('@') || normalized.Length > 254)
        {
            throw new ArgumentException("Invalid email address.", nameof(value));
        }

        return new Email(normalized);
    }

    public override string ToString() => Value;
    public static implicit operator string(Email email) => email.Value;
}

public sealed record HashedPassword
{
    public string Value { get; }

    private HashedPassword(string value) => Value = value;

    public static HashedPassword FromHash(string hash)
    {
        Guard.NotNullOrWhiteSpace(hash, nameof(hash));
        return new HashedPassword(hash);
    }

    public override string ToString() => Value;
}
