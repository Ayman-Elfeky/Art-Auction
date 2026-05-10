using System.Text.Json.Serialization;

namespace Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    [System.Runtime.Serialization.EnumMember(Value = "admin")]
    Admin,
    [System.Runtime.Serialization.EnumMember(Value = "user")]
    User,
    [System.Runtime.Serialization.EnumMember(Value = "artist")]
    Artist,
    [System.Runtime.Serialization.EnumMember(Value = "guest")]
    Guest,
}
