namespace HrSaas.SharedKernel.Exceptions;

public sealed class DomainException(string message) : Exception(message);

public sealed class TenantNotFoundException(string message) : Exception(message);

public sealed class TenantAccessDeniedException(string message) : Exception(message);
