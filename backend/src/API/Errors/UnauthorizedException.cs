namespace Api.Errors;

/// <summary>401 Unauthorized — throw from service implementations for quick status responses.</summary>
public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string? message = null, string? code = null)
        : base(code ?? "UNAUTHORIZED", 401, message ?? "Unauthorized") { }
}
