namespace Api.Errors;

/// <summary>404 Not Found — throw from service implementations for quick status responses.</summary>
public class NotFoundException : ApiException
{
    public NotFoundException(string? message = null, string? code = null)
        : base(code ?? "NOT_FOUND", 404, message ?? "Not Found") { }
}
