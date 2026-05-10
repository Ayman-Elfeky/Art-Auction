namespace Api.Errors;

/// <summary>403 Forbidden — throw from service implementations for quick status responses.</summary>
public class ForbiddenException : ApiException
{
    public ForbiddenException(string? message = null, string? code = null)
        : base(code ?? "FORBIDDEN", 403, message ?? "Forbidden") { }
}
