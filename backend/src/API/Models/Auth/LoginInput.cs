using System.Text.Json.Serialization;

namespace Api.Models;

/// <summary>Credentials for user login</summary>
public class LoginInput
{
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    public LoginInput() { }

    public LoginInput(string email, string password)
    {
        Email = email;
        Password = password;
    }
}
