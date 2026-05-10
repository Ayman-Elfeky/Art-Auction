using System.Text.Json.Serialization;

namespace Api.Models;

/// <summary>Message returned after approve/reject actions</summary>
public class AdminActionMessage
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    public AdminActionMessage() { }

    public AdminActionMessage(string message)
    {
        Message = message;
    }
}
