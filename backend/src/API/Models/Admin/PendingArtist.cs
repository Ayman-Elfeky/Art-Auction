using System.Text.Json.Serialization;

namespace Api.Models;

/// <summary>Artist account waiting for admin approval</summary>
public class PendingArtist
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    public PendingArtist() { }

    public PendingArtist(Guid id, string username, string email, DateTime createdAt)
    {
        Id = id;
        Username = username;
        Email = email;
        CreatedAt = createdAt;
    }
}
