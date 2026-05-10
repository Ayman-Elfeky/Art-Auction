using System.Text.Json.Serialization;

namespace Api.Models;

/// <summary>Error response payload</summary>
public class ErrorMessage
{
    [JsonPropertyName("error")]
    public string Error { get; set; }

    public ErrorMessage() { }

    public ErrorMessage(string error)
    {
        Error = error;
    }
}
