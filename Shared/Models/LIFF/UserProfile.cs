using System.Text.Json.Serialization;

namespace LifeHelper.Shared.Models.LIFF;

public class UserProfile
{
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = null!;

    [JsonPropertyName("sub")]
    public string UserLineId { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}
