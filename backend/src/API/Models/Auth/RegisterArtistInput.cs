using System.Text.Json.Serialization;

namespace Api.Models;

/// <summary>Data for artist account registration</summary>
public class RegisterArtistInput
{
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    public RegisterArtistInput() { }

    public RegisterArtistInput(string email, string name, string password)
    {
        Email = email;
        Name = name;
        Password = password;
    }
}
