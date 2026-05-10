namespace Api.Errors;

/// <summary>Standard API error response.</summary>
public record ApiErrorResponse(string Error, string Code, int Status);
