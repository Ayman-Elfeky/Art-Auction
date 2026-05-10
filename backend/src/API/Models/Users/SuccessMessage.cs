using System.Text.Json.Serialization;

namespace Api.Models;

/// <summary>Generic success response</summary>
public class SuccessMessage
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    public SuccessMessage() { }

    public SuccessMessage(bool success, string? message)
    {
        Success = success;
        Message = message;
    }
}
