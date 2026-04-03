namespace HrSaas.SharedKernel.Guards;

public static class Guard
{
    public static T NotNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        return value;
    }

    public static string NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} must not be null or whitespace.", paramName);
        }

        return value;
    }

    public static Guid NotEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{paramName} must not be an empty Guid.", paramName);
        }

        return value;
    }

    public static T NotDefault<T>(T value, string paramName) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
        {
            throw new ArgumentException($"{paramName} must not be the default value.", paramName);
        }

        return value;
    }

    public static int Positive(int value, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be positive.");
        }

        return value;
    }

    public static int NonNegative(int value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be non-negative.");
        }

        return value;
    }

    public static string MaxLength(string value, int maxLength, string paramName)
    {
        if (value.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} must not exceed {maxLength} characters.", paramName);
        }

        return value;
    }

    public static string MinLength(string value, int minLength, string paramName)
    {
        if (value.Length < minLength)
        {
            throw new ArgumentException($"{paramName} must be at least {minLength} characters.", paramName);
        }

        return value;
    }

    public static DateTime NotInFuture(DateTime value, string paramName)
    {
        if (value > DateTime.UtcNow)
        {
            throw new ArgumentException($"{paramName} must not be in the future.", paramName);
        }

        return value;
    }

    public static DateTime NotInPast(DateTime value, string paramName)
    {
        if (value < DateTime.UtcNow)
        {
            throw new ArgumentException($"{paramName} must not be in the past.", paramName);
        }

        return value;
    }
}
