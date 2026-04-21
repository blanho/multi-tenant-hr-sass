namespace HrSaas.SharedKernel.CQRS;

public sealed record Result<T>
{
    public bool IsSuccess { get; private init; }

    public bool IsFailure => !IsSuccess;

    public T? Value { get; private init; }

    public string? Error { get; private init; }

    public string? ErrorCode { get; private init; }

    private Result() { }

    public static Result<T> Success(T value) =>
        new() { IsSuccess = true, Value = value };

    public static Result<T> Failure(string error, string? errorCode = null) =>
        new() { IsSuccess = false, Error = error, ErrorCode = errorCode };

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);

    public Result<TNew> Map<TNew>(Func<T, TNew> mapper) =>
        IsSuccess
            ? Result<TNew>.Success(mapper(Value!))
            : Result<TNew>.Failure(Error!, ErrorCode);

    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder) =>
        IsSuccess
            ? binder(Value!)
            : Result<TNew>.Failure(Error!, ErrorCode);

    public async Task<Result<TNew>> BindAsync<TNew>(Func<T, Task<Result<TNew>>> binder) =>
        IsSuccess
            ? await binder(Value!).ConfigureAwait(false)
            : Result<TNew>.Failure(Error!, ErrorCode);

    public override string ToString() =>
        IsSuccess ? $"Success({Value})" : $"Failure({ErrorCode}: {Error})";
}

public sealed record Result
{
    public bool IsSuccess { get; private init; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; private init; }
    public string? ErrorCode { get; private init; }

    private Result() { }

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error, string? errorCode = null) =>
        new() { IsSuccess = false, Error = error, ErrorCode = errorCode };
}
